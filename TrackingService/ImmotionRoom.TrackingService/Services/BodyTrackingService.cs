namespace ImmotionAR.ImmotionRoom.TrackingService.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Helpers;
    using Interfaces;
    using Logger;
    using Model;
    using Networking.Interfaces;
    using TrackingEngine.Calibration;
    using TrackingEngine.Interfaces;
    using TrackingEngine.Model;
    using TrackingEngine.Tracking;
    using TrackingEngine.Walking;

    public class BodyTrackingService : IBodyTrackingService
    {
        #region Events

        public event EventHandler<DataSourceStatusChangedEventArgs> DataSourceStatusChanged;

        #endregion

        #region Constants

        private const string MergedBodyStreamerId = "MERGED";
        private const byte MergedBodyStreamerUniqueId = 255;

        #endregion

        #region Private fields

        private readonly object StreamerLock = new object();

        private readonly ILogger m_Logger;
        private readonly IConfigurationService m_ConfigurationService;
        private readonly IDataSourceService m_DataSourceService;
        private readonly ITcpServerFactory m_TcpServerFactory;
        private readonly INetworkClientFactory m_NetworkClientFactory;

        private readonly Dictionary<string, SceneDataStreamer> m_SceneDataRawStreamers;
        private readonly Dictionary<string, SceneDataStreamerInfo> m_SceneDataStreamerInfo;
        private readonly Stopwatch m_RunningTimeMeasurement;

        private SceneDataStreamer m_BodyDataMergedStreamer;

        private CancellationTokenSource m_CancellationTokenSource;

        private DateTime m_LastActiveClientDisconnected;

        private int m_AutomaticTrackingStopTimeout;
        private int m_ActiveClientsMonitorInterval;

        private int m_UpdateLoopRefreshInterval;

        private long m_LastTick;
        private int m_LastFrameRate;
        private int m_FrameRate;

        // TODO: at the moment, we use a direct reference to the implementation. It would be
        //       better to use an interface or the abstract class
        //private DataSourcesCalibrator m_DataSourcesCalibrator;
        private DataSourcesCalibratorSelective m_DataSourcesCalibrator;
        private DataSourcesPeopleTracker m_DataSourcesPeopleTracker;

        #endregion

        #region Properties

        public int ActiveClients { get; private set; }

        public BodyTrackingServiceState CurrentState { get; private set; }

        public IReadOnlyDictionary<string, SceneDataStreamerInfo> SceneDataStreamers
        {
            get { return new ReadOnlyDictionary<string, SceneDataStreamerInfo>(m_SceneDataStreamerInfo); }
        }

        #endregion

        #region Constructor

        public BodyTrackingService(IConfigurationService configurationService, IDataSourceService dataSourceService, ITcpServerFactory tcpServerFactory, INetworkClientFactory networkClientFactory)
        {
            Requires.NotNull(configurationService, "configurationService");
            Requires.NotNull(dataSourceService, "dataSourceService");
            Requires.NotNull(tcpServerFactory, "tcpServerFactory");
            Requires.NotNull(networkClientFactory, "networkClientFactory");

            m_Logger = LoggerService.GetLogger(typeof(BodyTrackingService));
            m_ConfigurationService = configurationService;
            m_DataSourceService = dataSourceService;
            m_TcpServerFactory = tcpServerFactory;
            m_NetworkClientFactory = networkClientFactory;

            m_SceneDataStreamerInfo = new Dictionary<string, SceneDataStreamerInfo>(StringComparer.OrdinalIgnoreCase);
            m_SceneDataRawStreamers = new Dictionary<string, SceneDataStreamer>(StringComparer.OrdinalIgnoreCase);

            m_RunningTimeMeasurement = new Stopwatch();
            m_RunningTimeMeasurement.Start();
        }

        #endregion

        #region Methods

        public void StartDataSourceMonitor()
        {
            m_DataSourceService.DataSourceStatusChanged += OnDataSourceServiceStatusChanged;
            m_DataSourceService.StartMonitor();
        }

        public void StopDataSourceMonitor()
        {
            m_DataSourceService.DataSourceStatusChanged -= OnDataSourceServiceStatusChanged;
            m_DataSourceService.StopMonitor();
        }

        public async Task StartTrackingAsync(TrackingSessionConfiguration sessionConfiguration = null)
        {
            if (!m_ConfigurationService.CalibrationData.CalibrationDone)
            {
                return;
            }

            if (CurrentState == BodyTrackingServiceState.Diagnostic)
            {
                StopDiagnosticMode();
            }

            if (CurrentState == BodyTrackingServiceState.Calibration)
            {
                StopTrackingInternal();
            }

            CurrentState = BodyTrackingServiceState.Tracking;

            // Used in ActiveClientsMonitor and UpdateLoop tasks
            m_CancellationTokenSource = new CancellationTokenSource();

            if (sessionConfiguration == null)
            {
                sessionConfiguration = TrackingSessionConfiguration.Default;
            }
            else
            {
                if (sessionConfiguration.Calibration == null)
                {
                    sessionConfiguration.Calibration = TrackingSessionConfiguration.Default.Calibration;
                }

                if (sessionConfiguration.WalkingDetection == null)
                {
                    sessionConfiguration.WalkingDetection = TrackingSessionConfiguration.Default.WalkingDetection;
                }

                if (sessionConfiguration.DataSourceTrackingSettings == null)
                {
                    sessionConfiguration.DataSourceTrackingSettings = TrackingSessionConfiguration.Default.DataSourceTrackingSettings;
                }
            }

            // Overrides the default EstimatedFrameRate for Walking Detection with the currently configured FPS
            sessionConfiguration.WalkingDetection.Parameters[KnaivePlayerWalkingDetectorSettings.Knee_EstimatedFrameRate_Key] = m_ConfigurationService.CurrentConfiguration.UpdateLoopFrameRate.ToString();

            await StartTrackingInternalAsync(sessionConfiguration).ConfigureAwait(false);

            // Configure and start the ActiveClients Monitor task
            m_AutomaticTrackingStopTimeout = m_ConfigurationService.CurrentConfiguration.AutomaticTrackingStopTimeoutInSeconds;
            m_ActiveClientsMonitorInterval = m_ConfigurationService.CurrentConfiguration.ActiveClientsMonitorIntervalInSeconds * 1000;
            m_LastActiveClientDisconnected = DateTime.UtcNow;
#pragma warning disable 4014
            Task.Factory.StartNew(ActiveClientsMonitor, m_CancellationTokenSource.Token, TaskCreationOptions.LongRunning);
#pragma warning restore 4014
        }

        public void StopTracking()
        {
            if (CurrentState == BodyTrackingServiceState.Calibration || CurrentState == BodyTrackingServiceState.Diagnostic)
            {
                StopRawStreamers();
            }

            StopTrackingInternal();
        }

        public async Task StartDiagnosticModeAsync(TrackingSessionConfiguration sessionConfiguration = null)
        {
            if (CurrentState == BodyTrackingServiceState.Tracking || CurrentState == BodyTrackingServiceState.Calibration)
            {
                StopTracking();
            }

            CurrentState = BodyTrackingServiceState.Diagnostic;

            // Used in ActiveClientsMonitor and UpdateLoop tasks
            m_CancellationTokenSource = new CancellationTokenSource();

            if (sessionConfiguration == null)
            {
                sessionConfiguration = TrackingSessionConfiguration.Default;
            }

            await StartTrackingInternalAsync(sessionConfiguration).ConfigureAwait(false);
            StartRawStreamers();
        }

        public void StopDiagnosticMode()
        {
            StopTrackingInternal();
            StopRawStreamers();
        }

        public async Task StartCalibrationProcedureAsync(TrackingSessionConfiguration sessionConfiguration = null)
        {
            if (CurrentState == BodyTrackingServiceState.Tracking || CurrentState == BodyTrackingServiceState.Diagnostic)
            {
                StopTracking();
            }

            CurrentState = BodyTrackingServiceState.Calibration;

            // Used in ActiveClientsMonitor and UpdateLoop tasks
            m_CancellationTokenSource = new CancellationTokenSource();

            if (sessionConfiguration == null)
            {
                sessionConfiguration = TrackingSessionConfiguration.Default;
            }

            m_DataSourcesCalibrator = new DataSourcesCalibratorSelective(m_ConfigurationService.CurrentMasterDataSource, (IBodyDataProvider)m_DataSourceService);
            m_DataSourcesCalibrator.AdditionalMasterYRotationAngle = sessionConfiguration.Calibration.AdditionalMasterYRotation;
            m_DataSourcesCalibrator.CalibratingUserHeight = sessionConfiguration.Calibration.CalibratingUserHeight;
            m_DataSourcesCalibrator.CalibrateSlavesUsingCentroids = sessionConfiguration.Calibration.CalibrateSlavesUsingCentroids;
            m_DataSourcesCalibrator.LastButNthValidMatrix = sessionConfiguration.Calibration.LastButNthValidMatrix;

            await StartTrackingInternalAsync(sessionConfiguration).ConfigureAwait(false);
            StartRawStreamers();
        }

        public void ExecuteCalibrationStep(CalibrationParameters parameters)
        {
            if (CurrentState != BodyTrackingServiceState.Calibration)
            {
                if (m_Logger.IsErrorEnabled)
                {
                    m_Logger.Error("StartCalibration must be called prior to use ExecuteCalibrationStep({0})", parameters.Step);
                }
                return;
            }

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("ExecuteCalibrationStep({0})", parameters.Step);
            }

            if (parameters.Step == TrackingServiceCalibrationSteps.StartCalibrateDataSourceWithMaster)
            {
                if (!m_DataSourcesCalibrator.SetCalibratorStatus(parameters.DataSource1, true))
                {
                    if (m_Logger.IsErrorEnabled)
                    {
                        m_Logger.Error("ExecuteCalibrationStep({0}) - SetCalibratorStatus({1}, true) FAILED", parameters.Step, parameters.DataSource1);
                    }
                }
            }
            else if (parameters.Step == TrackingServiceCalibrationSteps.StopCalibrateDataSourceWithMaster)
            {
                if (!m_DataSourcesCalibrator.SetCalibratorStatus(parameters.DataSource1, false))
                {
                    if (m_Logger.IsErrorEnabled)
                    {
                        m_Logger.Error("ExecuteCalibrationStep({0}) - SetCalibratorStatus({1}, false) FAILED", parameters.Step, parameters.DataSource1);
                    }
                }
            }
            else if (parameters.Step == TrackingServiceCalibrationSteps.StartCalibrateMaster)
            {
                if (!m_DataSourcesCalibrator.SetCalibratorStatus(m_ConfigurationService.CurrentMasterDataSource, true))
                {
                    if (m_Logger.IsErrorEnabled)
                    {
                        m_Logger.Error("ExecuteCalibrationStep({0}) - SetCalibratorStatus(MASTER, true) FAILED", parameters.Step);
                    }
                }
            }
            else if (parameters.Step == TrackingServiceCalibrationSteps.StopCalibrateMaster)
            {
                if (!m_DataSourcesCalibrator.SetCalibratorStatus(m_ConfigurationService.CurrentMasterDataSource, false))
                {
                    if (m_Logger.IsErrorEnabled)
                    {
                        m_Logger.Error("ExecuteCalibrationStep({0}) - SetCalibratorStatus(MASTER, false) FAILED", parameters.Step);
                    }
                }
            }
        }

        public void CompleteCalibration()
        {
            StopRawStreamers();
            StopTrackingInternal();

            m_ConfigurationService.UpdateCalibrationData(m_DataSourcesCalibrator.SaveCalibrationData());
        }

        #endregion

        #region Private methods

        private async Task StartTrackingInternalAsync(TrackingSessionConfiguration trackingSessionConfiguration)
        {
            m_UpdateLoopRefreshInterval = (int)((float)1 / m_ConfigurationService.CurrentConfiguration.UpdateLoopFrameRate * 1000);

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("FPS: {0} --> RefreshInterval: {1}ms", m_ConfigurationService.CurrentConfiguration.UpdateLoopFrameRate, m_UpdateLoopRefreshInterval);
            }

            ActiveClients = 0;

            m_DataSourcesPeopleTracker = new DataSourcesPeopleTracker((IBodyDataProvider)m_DataSourceService, m_ConfigurationService.CalibrationData, trackingSessionConfiguration.WalkingDetection);

#if DEBUG
            //// DEBUG ONLY
            //if (m_Logger != null && m_Logger.IsDebugEnabled)
            //{
            //    m_Logger.Debug("DataSourcesPeopleTracker - MasterToWorldMatrix:\n{0}", m_ConfigurationService.CalibrationData.MasterToWorldCalibrationMatrix);
            //}
#endif

            m_BodyDataMergedStreamer = new SceneDataStreamer(MergedBodyStreamerId /*, MergedBodyStreamerUniqueId,*/, m_ConfigurationService.CurrentConfiguration.DataStreamerEndpoint, m_ConfigurationService.CurrentConfiguration.DataStreamerPort, false, m_TcpServerFactory, m_NetworkClientFactory, m_ConfigurationService);
            m_BodyDataMergedStreamer.ClientConnected += MergedBodyDataStreamer_OnClientConnected;
            m_BodyDataMergedStreamer.ClientDisconnected += MergedBodyDataStreamer_OnClientDisconnected;
            m_BodyDataMergedStreamer.TransformationMatrices[TrackingServiceSceneDataStreamModes.WorldTransform] = m_ConfigurationService.CalibrationData.MasterToWorldCalibrationMatrix;

            m_BodyDataMergedStreamer.Start();

            var mergedBodyStreamerInfo = new SceneDataStreamerInfo { Id = m_BodyDataMergedStreamer.DataSourceId, /*UniqueId = MergedBodyStreamerUniqueId,*/ StreamEndpoint = m_BodyDataMergedStreamer.StreamingEndpoint, StreamPort = m_BodyDataMergedStreamer.StreamingPort, IsMaster = false };
            mergedBodyStreamerInfo.SupportedStreamModes.Add(TrackingServiceSceneDataStreamModes.Raw);
            mergedBodyStreamerInfo.SupportedStreamModes.Add(TrackingServiceSceneDataStreamModes.WorldTransform);

            lock (StreamerLock)
            {
                m_SceneDataStreamerInfo.Add(m_BodyDataMergedStreamer.DataSourceId, mergedBodyStreamerInfo);
            }

            await m_DataSourceService.StartAsync(trackingSessionConfiguration.DataSourceTrackingSettings).ConfigureAwait(false);

            // Starts Update Loop (Unity 3D like)
#pragma warning disable 4014
            Task.Factory.StartNew(UpdateLoop, m_CancellationTokenSource.Token, TaskCreationOptions.LongRunning);
#pragma warning restore 4014
        }

        private void StopTrackingInternal()
        {
            // Stops the ActiveClientsMonitor and UpdateLoop tasks (if any)
            if (m_CancellationTokenSource != null)
            {
                m_CancellationTokenSource.Cancel();
            }

            if (m_BodyDataMergedStreamer != null)
            {
                m_BodyDataMergedStreamer.ClientConnected -= MergedBodyDataStreamer_OnClientConnected;
                m_BodyDataMergedStreamer.ClientDisconnected -= MergedBodyDataStreamer_OnClientDisconnected;
                m_BodyDataMergedStreamer.Stop();

                lock (StreamerLock)
                {
                    m_SceneDataStreamerInfo.Remove(m_BodyDataMergedStreamer.DataSourceId);
                }
            }

            m_DataSourceService.Stop();

            ActiveClients = 0;

            m_ConfigurationService.CurrentConfiguration.Scene.FloorClipPlane = new Vector4();

            CurrentState = BodyTrackingServiceState.Idle;
        }

        private void MergedBodyDataStreamer_OnClientConnected(object sender, EventArgs args)
        {
            ActiveClients++;

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("MergedBodyDataStreamer_OnClientConnected (ActiveClients: {0})", ActiveClients);
            }
        }

        private void MergedBodyDataStreamer_OnClientDisconnected(object sender, EventArgs args)
        {
            ActiveClients--;

            if (ActiveClients <= 0)
            {
                m_LastActiveClientDisconnected = DateTime.UtcNow;
                ActiveClients = 0;
            }

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("MergedBodyDataStreamer_OnClientDisconnected (ActiveClients: {0})", ActiveClients);
            }
        }

        private void ActiveClientsMonitor(object args)
        {
            if (CurrentState != BodyTrackingServiceState.Tracking)
            {
                if (m_Logger.IsWarnEnabled)
                {
                    m_Logger.Warn("ActiveClientsMonitor: requested to start, but BodyTrackingService is not in Tracking state. Abort.");
                }
                return;
            }

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("ActiveClientsMonitor: started");
            }

            while (true)
            {
                // Automatic stop Tracking if no client is using the system
                if (CurrentState == BodyTrackingServiceState.Tracking)
                {
                    if (ActiveClients == 0 && (DateTime.UtcNow - m_LastActiveClientDisconnected).TotalSeconds >= m_AutomaticTrackingStopTimeout)
                    {
                        if (m_Logger.IsDebugEnabled)
                        {
                            m_Logger.Warn("ActiveClientsMonitor: No ActiveClients - TIMEOUT!!");
                        }

                        StopTrackingInternal();
                    }
                    else if (ActiveClients > 0)
                    {
                        // Reset timeout timer
                        m_LastActiveClientDisconnected = DateTime.UtcNow;
                    }
                }

                var cancelled = m_CancellationTokenSource.Token.WaitHandle.WaitOne(m_ActiveClientsMonitorInterval);
                if (cancelled)
                {
                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("ActiveClientsMonitor: requested to stop");
                    }
                    break;
                }
            }

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("ActiveClientsMonitor: stopped");
            }
        }

        private void UpdateLoop(object args)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("UpdateLoop: started");
            }

            var loopMeasurement = new Stopwatch();
            var globalTimeMeasurement = new Stopwatch();

            globalTimeMeasurement.Start();

#if DEBUG
            // DEBUG ONLY
            var fpsCount = 0;
            long meanFps = 0;
            var updateLoopFpsInterval = 10.0;
#endif
            long lastUpdate = 0;
            double deltaTime = 0;
            var incrementalTime = TimeSpan.Zero;

            while (true)
            {
                loopMeasurement.Restart();

                try
                {
                    // Retrieve updated BodyData from all configured DataSources
                    m_DataSourceService.Update(deltaTime);

                    // Update the Floor in the Scene Descriptor (Tracking Service Status)
                    // using the one detected from the Master DataSource.
                    if (m_DataSourceService.DataSources.ContainsKey(m_ConfigurationService.CurrentMasterDataSource))
                    {
                        m_ConfigurationService.CurrentConfiguration.Scene.FloorClipPlane = m_DataSourceService.DataSources[m_ConfigurationService.CurrentMasterDataSource].FloorClipPlane;
                    }
                    else
                    {
                        m_ConfigurationService.CurrentConfiguration.Scene.FloorClipPlane = new Vector4(0, 0, 0, 0);
                    }

                    // If in Calibration Mode
                    if (CurrentState == BodyTrackingServiceState.Calibration)
                    {
                        // Calculate calibration data according to the current Calibration Step
                        m_DataSourcesCalibrator.Update(deltaTime);

                        // Update transformation matrices for the Merged BodyStreamer
                        m_BodyDataMergedStreamer.TransformationMatrices[TrackingServiceSceneDataStreamModes.WorldTransform] = m_DataSourcesCalibrator.MasterCalibrator.CalibrationMatrix;

                        // Update transformation matrices for each enabled raw BodyDataStreamer
                        var rawStreamers = new List<SceneDataStreamer>(m_SceneDataRawStreamers.Values);
                        foreach (var bodyDataRawStreamer in rawStreamers)
                        {
                            if (m_DataSourcesCalibrator.MasterSlaveCalibrators.ContainsKey(bodyDataRawStreamer.DataSourceId))
                            {
                                bodyDataRawStreamer.TransformationMatrices[TrackingServiceSceneDataStreamModes.MasterTransform] = m_DataSourcesCalibrator.MasterSlaveCalibrators[bodyDataRawStreamer.DataSourceId].LastCalibrationMatrix;
                                if (bodyDataRawStreamer.IsMaster)
                                {
                                    bodyDataRawStreamer.TransformationMatrices[TrackingServiceSceneDataStreamModes.WorldTransform] = m_DataSourcesCalibrator.MasterCalibrator.CalibrationMatrix;
                                }
                            }
                        }
                    }

                    // Calculate merged body data for the Merged BodyStreamer
                    m_DataSourcesPeopleTracker.Update(deltaTime, incrementalTime);

                    var mergedStreamFrame = new SceneFrame();

                    foreach (var body in m_DataSourcesPeopleTracker.People)
                    {
                        mergedStreamFrame.Bodies.Add(body);
                    }
                    mergedStreamFrame.Timestamp = DateTime.UtcNow;


                    // Data Streaming:
                    // In TrackingMode, only the merged stream is available.
                    // In CalibrationMode, the merged stream and as many other streams as configured DataSources are enabled.

                    // Send Merged Body data to connected clients
                    m_BodyDataMergedStreamer.SendDataToClients(mergedStreamFrame);

                    // Send raw Body data to connected clients (in Calibration or Diagnostic Mode only)
                    if (CurrentState == BodyTrackingServiceState.Calibration || CurrentState == BodyTrackingServiceState.Diagnostic)
                    {
                        var streamers = new List<SceneDataStreamer>(m_SceneDataRawStreamers.Values);
                        foreach (var bodyDataRawStreamer in streamers)
                        {
                            if (m_DataSourceService.DataSources.ContainsKey(bodyDataRawStreamer.DataSourceId))
                            {
                                bodyDataRawStreamer.SendDataToClients(m_DataSourceService.DataSources[bodyDataRawStreamer.DataSourceId]);
                            }
                        }
                    }

#if DEBUG
                    //// DEBUG ONLY
                    //foreach (string dataSourceId in m_DataSourceService.BodyData.Keys)
                    //{
                    //    BodyDataFrame frame;
                    //    if (m_DataSourceService.BodyData.TryGetValue(dataSourceId, out frame))
                    //    {
                    //        if (m_Logger.IsDebugEnabled)
                    //        {
                    //            m_Logger.Debug("UpdateLoop: BodyData[{0}] -> Bodies: {1}", dataSourceId, frame.Bodies.Count);
                    //        }
                    //    }
                    //}
#endif
                }
                catch (Exception ex)
                {
                    if (m_Logger.IsErrorEnabled)
                    {
                        m_Logger.Error(ex, "UpdateLoop: m_DataSourceService.Update exception {0}", ex.Message);
                    }
                }

                var loopWait = m_UpdateLoopRefreshInterval - (int)loopMeasurement.ElapsedMilliseconds;

                if (loopWait < 0)
                {
                    loopWait = 0;
                }

                var cancelled = m_CancellationTokenSource.Token.WaitHandle.WaitOne(loopWait);
                if (cancelled)
                {
                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("UpdateLoop: requested to stop");
                    }
                    break;
                }

                deltaTime = (double)(globalTimeMeasurement.ElapsedTicks - lastUpdate) / Stopwatch.Frequency;
                // IT IS NOT POSSIBLE TO USE Timespan here, because it consider 1s = 10000000 ticks
                // See: https://msdn.microsoft.com/en-us/library/system.datetime.ticks(v=vs.110).aspx

                // TODO: DIRTY FIX FOR Global Game Jam 2016 -- TO BE REMOVED
                incrementalTime += new TimeSpan(0, 0, 0, 0, (int)(1.1f * deltaTime * 1000));
                //incrementalTime += new TimeSpan(0,0,0,0,(int)(deltaTime*1000));


#if DEBUG
                // DEBUG ONLY
                meanFps += CalculateFrameRate();
                fpsCount++;
                if (updateLoopFpsInterval <= 0)
                {
                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("UpdateLoop - FPS: {1:0}", deltaTime, meanFps / fpsCount);
                    }

                    meanFps = 0;
                    fpsCount = 0;
                    updateLoopFpsInterval = 10.0;
                }
                updateLoopFpsInterval -= deltaTime;
#endif
                loopMeasurement.Stop();

                lastUpdate = globalTimeMeasurement.ElapsedTicks;
            }

            globalTimeMeasurement.Stop();

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("UpdateLoop: stopped");
            }
        }

        private int CalculateFrameRate()
        {
            // Beware that 10000 ticks in 1ms (https://msdn.microsoft.com/it-it/library/system.datetime.ticks(v=vs.110).aspx)
            // So, here instead we need to use Stopwatch Frequency for precise calculations
            if (Stopwatch.GetTimestamp() - m_LastTick >= Stopwatch.Frequency)
            {
                m_LastFrameRate = m_FrameRate;
                m_FrameRate = 0;
                m_LastTick = Stopwatch.GetTimestamp();
            }
            m_FrameRate++;
            return m_LastFrameRate;
        }

        private void StartRawStreamers()
        {
            lock (StreamerLock)
            {
                var knownDataSources = m_ConfigurationService.KnownDataSources;

                var baseStreamingPort = m_ConfigurationService.CurrentConfiguration.DataStreamerPort + 1;

                foreach (var dataSource in knownDataSources.Values)
                {
                    if (!m_SceneDataRawStreamers.ContainsKey(dataSource.Id))
                    {
                        var bodyDataStreamer = new SceneDataStreamer(dataSource.Id, /* m_ConfigurationService.KnownDataSources[dataSource.Id].UniqueId */ m_ConfigurationService.CurrentConfiguration.DataStreamerEndpoint, baseStreamingPort++, m_ConfigurationService.CurrentMasterDataSource == dataSource.Id, m_TcpServerFactory, m_NetworkClientFactory, m_ConfigurationService);
                        m_SceneDataRawStreamers.Add(bodyDataStreamer.DataSourceId, bodyDataStreamer);
                        m_SceneDataRawStreamers[bodyDataStreamer.DataSourceId].Start();

                        var bodyStreamerInfo = new SceneDataStreamerInfo { Id = bodyDataStreamer.DataSourceId, /*UniqueId = m_ConfigurationService.KnownDataSources[dataSource.Id].UniqueId,*/ StreamEndpoint = bodyDataStreamer.StreamingEndpoint, StreamPort = bodyDataStreamer.StreamingPort, IsMaster = bodyDataStreamer.IsMaster };
                        bodyStreamerInfo.SupportedStreamModes.Add(TrackingServiceSceneDataStreamModes.Raw);
                        bodyStreamerInfo.SupportedStreamModes.Add(TrackingServiceSceneDataStreamModes.MasterTransform);
                        if (bodyStreamerInfo.IsMaster)
                        {
                            bodyStreamerInfo.SupportedStreamModes.Add(TrackingServiceSceneDataStreamModes.WorldTransform);
                        }

                        m_SceneDataStreamerInfo.Add(bodyDataStreamer.DataSourceId, bodyStreamerInfo);
                    }
                }
            }
        }

        private void StopRawStreamers()
        {
            lock (StreamerLock)
            {
                foreach (var dataSourceId in m_SceneDataRawStreamers.Keys)
                {
                    m_SceneDataRawStreamers[dataSourceId].Stop();
                    m_SceneDataStreamerInfo.Remove(dataSourceId);
                }

                m_SceneDataRawStreamers.Clear();
            }
        }

        private void OnDataSourceServiceStatusChanged(object sender, DataSourceStatusChangedEventArgs e)
        {
            var localHandler = DataSourceStatusChanged;
            if (localHandler != null)
            {
                localHandler(this, e);
            }
        }

        #endregion
    }
}

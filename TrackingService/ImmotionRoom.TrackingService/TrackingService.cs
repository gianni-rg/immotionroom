namespace ImmotionAR.ImmotionRoom.TrackingService
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Threading.Tasks;
    using Helpers;
    using Helpers.Messaging;
    using Infrastructure.Network;
    using Interfaces;
    using Logger;
    using Model;
    using Networking.Interfaces;
    using Services;
    using TrackingEngine.Model;

    public class TrackingService : ITrackingService
    {
        #region Events
        public event EventHandler<TrackingServiceStatusChangedEventArgs> StatusChanged;
        public event EventHandler<DataSourceStatusChangedEventArgs> DataSourceStatusChanged; 
        #endregion

        #region Private fields

        private readonly ILogger m_Logger;
        private readonly IConfigurationService m_ConfigurationService;
        private readonly INetworkClientFactory m_NetworkClientFactory;
        private readonly ITcpServerFactory m_TcpServerFactory;
        private readonly ITcpClientFactory m_TcpClientFactory;
        private readonly IUdpClientFactory m_UdpClientFactory;

        private readonly INetworkDiscoveryService m_NetworkDiscoveryService;
        private readonly IMessenger m_Messenger;
        private readonly ICommandProcessor m_CommandProcessor;
        private readonly IBodyTrackingService m_BodyTrackingService;

        private TrackingServiceState m_CurrentState;
        private readonly AutoResetEvent m_TrackingStateUpdateEvent;

        #endregion

        #region Properties

        public IReadOnlyDictionary<string, DataSourceInfo> KnownDataSources { get { return m_ConfigurationService.KnownDataSources; } }

        public TrackingServiceState Status
        {
            get
            {
                return m_CurrentState;
            }
        }

        public string InstanceID
        {
            get
            {
                return m_ConfigurationService.CurrentConfiguration.InstanceId;
            }
        }

        #endregion

        #region Constructor

        public TrackingService(TrackingServiceConfiguration configuration, DataSourceCollection knownDataSources, ITcpServerFactory tcpServerFactory, ITcpClientFactory tcpClientFactory, IUdpClientFactory udpClientFactory, INetworkClientFactory networkClientFactory)
        {
            m_Logger = LoggerService.GetLogger<TrackingService>();
            m_Messenger = MessengerService.Messenger;

            m_ConfigurationService = new ConfigurationService();
            m_ConfigurationService.LoadExternalConfiguration(configuration);
            m_ConfigurationService.LoadInternalConfiguration(knownDataSources);

            m_TcpServerFactory = tcpServerFactory;
            m_TcpClientFactory = tcpClientFactory;
            m_UdpClientFactory = udpClientFactory;
            m_NetworkClientFactory = networkClientFactory;

            m_CommandProcessor = new CommandProcessor(m_ConfigurationService);

            m_NetworkDiscoveryService = new NetworkDiscoveryService(m_ConfigurationService, m_UdpClientFactory);
            m_BodyTrackingService = new BodyTrackingService(m_ConfigurationService, new DataSourceService(m_ConfigurationService, new DataSourceControl(), m_TcpClientFactory), m_TcpServerFactory, m_NetworkClientFactory);

            m_TrackingStateUpdateEvent = new AutoResetEvent(true);

        }

        #endregion

        #region Methods

        public async Task Start()
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("TrackingService.Start()");
            }

            await m_ConfigurationService.InitializeAsync().ConfigureAwait(false);

            m_CommandProcessor.Start();

            m_Messenger.Register<EnableAutoDiscoveryEventArgs>(this, OnEnableAutoDiscovery);
            m_Messenger.Register<GetServiceStatusEventArgs>(this, OnGetServiceStatus);
            m_Messenger.Register<StartTrackingSystemEventArgs>(this, OnStartTrackingSystem);
            m_Messenger.Register<ExecuteCalibrationStepEventArgs>(this, OnExecuteCalibrationStep);
            m_Messenger.Register<StopTrackingSystemEventArgs>(this, OnStopTrackingSystem);
            m_Messenger.Register<SetMasterDataSourceEventArgs>(this, OnSetMasterDataSource);
            m_Messenger.Register<StartDiagnosticModeEventArgs>(this, OnStartDiagnosticMode);
            m_Messenger.Register<StopDiagnosticModeEventArgs>(this, OnStopDiagnosticMode);
            m_Messenger.Register<SystemRebootEventArgs>(this, OnSystemReboot);
            m_Messenger.Register<SetSceneDescriptorEventArgs>(this, OnSetSceneDescriptor);

            await m_NetworkDiscoveryService.StartAsync().ConfigureAwait(false);

            await TrackingServiceUpdateState(TrackingServiceState.Starting).ConfigureAwait(false);

            m_BodyTrackingService.DataSourceStatusChanged += OnDataSourceServiceStatusChanged;
            m_BodyTrackingService.StartDataSourceMonitor();
        }


        public async Task Stop()
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("TrackingService.Stop()");
            }

            await TrackingServiceUpdateState(TrackingServiceState.Stopping).ConfigureAwait(false);

            m_CommandProcessor.Stop();

            m_Messenger.Unregister<EnableAutoDiscoveryEventArgs>(this);
            m_Messenger.Unregister<GetServiceStatusEventArgs>(this);
            m_Messenger.Unregister<StartTrackingSystemEventArgs>(this);
            m_Messenger.Unregister<ExecuteCalibrationStepEventArgs>(this);
            m_Messenger.Unregister<StopTrackingSystemEventArgs>(this);
            m_Messenger.Unregister<SetMasterDataSourceEventArgs>(this);
            m_Messenger.Unregister<StartDiagnosticModeEventArgs>(this);
            m_Messenger.Unregister<StopDiagnosticModeEventArgs>(this);
            m_Messenger.Unregister<SystemRebootEventArgs>(this);
            m_Messenger.Unregister<SetSceneDescriptorEventArgs>(this);

            m_NetworkDiscoveryService.DataSourceFound -= NetworkDiscoveryService_DataSourceFound;
            m_NetworkDiscoveryService.DiscoveryCompleted -= NetworkDiscoveryService_DiscoveryCompleted;

            m_NetworkDiscoveryService.Stop();

            m_NetworkDiscoveryService.StopDiscovery();

            m_BodyTrackingService.StopTracking();

            m_BodyTrackingService.DataSourceStatusChanged -= OnDataSourceServiceStatusChanged;
            m_BodyTrackingService.StopDataSourceMonitor();

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("TrackingService Stopped");
            }
        }

        #endregion

        #region Private methods

        private async void NetworkDiscoveryService_DiscoveryCompleted(object sender, EventArgs e)
        {
            if (m_Logger.IsInfoEnabled)
            {
                m_Logger.Info("Found {0} Data Sources", m_ConfigurationService.KnownDataSources.Count);
            }

            await m_ConfigurationService.SaveCurrentConfigurationAsync().ConfigureAwait(false);

            m_NetworkDiscoveryService.DataSourceFound -= NetworkDiscoveryService_DataSourceFound;
            m_NetworkDiscoveryService.DiscoveryCompleted -= NetworkDiscoveryService_DiscoveryCompleted;
            m_NetworkDiscoveryService.StopDiscovery();

            if (m_ConfigurationService.KnownDataSources.Count == 0)
            {
                await Task.Delay(m_ConfigurationService.CurrentConfiguration.AutoDiscovery.RepeatIntervalInSeconds*1000).ConfigureAwait(false); // Wait and then retry with AutoDiscovery
                await TrackingServiceUpdateState(TrackingServiceState.Starting).ConfigureAwait(false);
            }
            else
            {
                await Task.Delay(m_ConfigurationService.CurrentConfiguration.AutoDiscovery.CompletionDelayInSeconds*1000).ConfigureAwait(false);
                await TrackingServiceUpdateState(TrackingServiceState.Starting).ConfigureAwait(false);
            }
        }

        private void NetworkDiscoveryService_DataSourceFound(object sender, DataSourceFoundEventArgs e)
        {
            if (m_Logger.IsInfoEnabled)
            {
                m_Logger.Info("DataSourceFound: '{0}'", e.DataSource.Id);
            }
            
            if (!m_ConfigurationService.AddDataSource(e.DataSource))
            {
                if (m_Logger.IsErrorEnabled)
                {
                    m_Logger.Error("DataSourceFound: unable to add '{0}' to KnownDataSources", e.DataSource.Id);
                }
            }
        }

        private async Task<UpdateStateResult> TrackingServiceUpdateState(TrackingServiceState newState, object args = null, bool waitCompletion = true)
        {
            if (waitCompletion)
            {
                m_TrackingStateUpdateEvent.WaitOne();
            }

            if (newState != TrackingServiceState.Calibration && newState != TrackingServiceState.Running && newState != TrackingServiceState.DiagnosticMode && m_CurrentState == newState)
            {
                // Calibration is a multi-step state, so it is handled differently.
                // Running is particular, because the BodyTrackingService can disable the tracking, while the TrackingService
                // is still in Running -- so subsequent StartTracking command would be ignored.
                // For other states, do nothing

                m_TrackingStateUpdateEvent.Set();
                return new UpdateStateResult {IsError = false};
            }

            switch (newState)
            {
                case TrackingServiceState.Idle:

                    m_CurrentState = TrackingServiceState.Idle;

                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("TrackingServiceUpdateState - State: {0}", m_CurrentState);
                    }
                    
                    if (m_ConfigurationService.CalibrationData.CalibrationDone)
                    {
                        if (string.IsNullOrEmpty(m_ConfigurationService.CurrentMasterDataSource))
                        {
                            if (m_Logger.IsInfoEnabled)
                            {
                                m_Logger.Warn("Tracking System is not configured. Please set a Master DataSource.");
                            }

                            OnServiceStatusChanged(TrackingServiceState.Warning, TrackingServiceStateErrors.NotConfigured);
                        }
                        else
                        {
                            if (m_Logger.IsInfoEnabled)
                            {
                                m_Logger.Info("Tracking System is calibrated and ready");
                            }

                            OnServiceStatusChanged(m_CurrentState);
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(m_ConfigurationService.CurrentMasterDataSource))
                        {
                            if (m_Logger.IsInfoEnabled)
                            {
                                m_Logger.Warn("Tracking System is not configured. Please set a Master DataSource.");
                            }

                            OnServiceStatusChanged(TrackingServiceState.Warning, TrackingServiceStateErrors.NotConfigured);
                        }
                        else
                        {
                            if (m_Logger.IsInfoEnabled)
                            {
                                m_Logger.Warn("Tracking System is not calibrated. Waiting for calibration request.");
                            }

                            OnServiceStatusChanged(TrackingServiceState.Warning, TrackingServiceStateErrors.NotCalibrated);
                        }
                    }

                    m_TrackingStateUpdateEvent.Set();
                    return new UpdateStateResult {IsError = false};

                case TrackingServiceState.Running:

                    if (string.IsNullOrEmpty(m_ConfigurationService.CurrentMasterDataSource))
                    {
                        if (m_Logger.IsErrorEnabled)
                        {
                            m_Logger.Error("Tracking System is not configured. Please set a Master DataSource.");
                        }

                        OnServiceStatusChanged(TrackingServiceState.Warning, TrackingServiceStateErrors.NotConfigured);

                        m_TrackingStateUpdateEvent.Set();
                        return new UpdateStateResult {IsError = true, ErrorCode = TrackingServiceStateErrors.NotConfigured, ErrorDescription = "Tracking System is not configured: please set a Master DataSource. Tracking can't be started."};
                    }

                    if (!m_ConfigurationService.CalibrationData.CalibrationDone)
                    {
                        if (m_Logger.IsErrorEnabled)
                        {
                            m_Logger.Error("TrackingSystem is not calibrated. Tracking can't be started.");
                        }

                        OnServiceStatusChanged(TrackingServiceState.Warning, TrackingServiceStateErrors.NotCalibrated);

                        m_TrackingStateUpdateEvent.Set();
                        return new UpdateStateResult {IsError = true, ErrorCode = TrackingServiceStateErrors.NotCalibrated, ErrorDescription = "TrackingSystem is not calibrated. Tracking can't be started."};
                    }

                    if (m_BodyTrackingService.CurrentState == BodyTrackingServiceState.Diagnostic)
                    {
                        m_BodyTrackingService.StopDiagnosticMode();
                    }

                    if (m_BodyTrackingService.CurrentState != BodyTrackingServiceState.Tracking)
                    {
                        var startTrackingArgs = (StartTrackingSystemEventArgs) args;
                        var trackingSessionConfiguration = startTrackingArgs.Configuration;
                        await m_BodyTrackingService.StartTrackingAsync(trackingSessionConfiguration).ConfigureAwait(false);
                    }

                    m_CurrentState = TrackingServiceState.Running;

                    OnServiceStatusChanged(m_CurrentState);

                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("TrackingServiceUpdateState - State: {0}", m_CurrentState);
                    }

                    m_TrackingStateUpdateEvent.Set();
                    return new UpdateStateResult {IsError = false};


                case TrackingServiceState.Calibration:

                    var calibrationArgs = (ExecuteCalibrationStepEventArgs) args;

                    m_CurrentState = TrackingServiceState.Calibration;

                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("TrackingServiceUpdateState - State: {0}/{1}", m_CurrentState, calibrationArgs.Parameters.Step);
                    }

                    OnServiceStatusChanged(m_CurrentState);
                    
                    if (string.IsNullOrEmpty(m_ConfigurationService.CurrentMasterDataSource))
                    {
                        await TrackingServiceUpdateState(TrackingServiceState.Idle, waitCompletion: false).ConfigureAwait(false);
                        
                        m_TrackingStateUpdateEvent.Set();

                        return new UpdateStateResult {IsError = true, ErrorCode = TrackingServiceStateErrors.NotConfigured, ErrorDescription = "TrackingSystem is not configured."};
                    }

                    if (calibrationArgs.Parameters.Step == TrackingServiceCalibrationSteps.Start)
                    {
                        // Check if the system is already in calibration. If so, avoid to send another StartCalibration.
                        if (m_BodyTrackingService.CurrentState != BodyTrackingServiceState.Calibration)
                        {
                            var trackingSessionConfiguration = TrackingSessionConfiguration.Default;
                            trackingSessionConfiguration.Calibration = calibrationArgs.Parameters;
                            trackingSessionConfiguration.DataSourceTrackingSettings.BodyClippingEdgesEnabled = true;
                            await m_BodyTrackingService.StartCalibrationProcedureAsync(trackingSessionConfiguration).ConfigureAwait(false);
                        }
                    }
                    else if (calibrationArgs.Parameters.Step == TrackingServiceCalibrationSteps.End)
                    {
                        // Check if the system is in calibration. If not, ignore the command.
                        if (m_BodyTrackingService.CurrentState == BodyTrackingServiceState.Calibration)
                        {
                            m_BodyTrackingService.CompleteCalibration();
                            await m_ConfigurationService.SaveCurrentConfigurationAsync().ConfigureAwait(false);
                            await TrackingServiceUpdateState(TrackingServiceState.Starting, waitCompletion: false).ConfigureAwait(false);
                        }
                        else
                        {
                            await TrackingServiceUpdateState(TrackingServiceState.Idle, waitCompletion: false).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        m_BodyTrackingService.ExecuteCalibrationStep(calibrationArgs.Parameters);
                    }

                    m_TrackingStateUpdateEvent.Set();
                    return new UpdateStateResult {IsError = false};

                case TrackingServiceState.Starting:

                    m_CurrentState = TrackingServiceState.Starting;

                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("TrackingServiceUpdateState - State: {0}", m_CurrentState);
                    }

                    if (m_ConfigurationService.KnownDataSources.Count == 0)
                    {
                        if (m_Logger.IsInfoEnabled)
                        {
                            m_Logger.Info("No Known Data Sources: enter in AutoDiscovery mode");
                        }

                        m_CurrentState = TrackingServiceState.AutoDiscovery;

                        if (m_Logger.IsDebugEnabled)
                        {
                            m_Logger.Debug("TrackingServiceUpdateState - State: {0}", m_CurrentState);
                        }

                        OnServiceStatusChanged(m_CurrentState);

                        m_NetworkDiscoveryService.DataSourceFound += NetworkDiscoveryService_DataSourceFound;
                        m_NetworkDiscoveryService.DiscoveryCompleted += NetworkDiscoveryService_DiscoveryCompleted;
                        await m_NetworkDiscoveryService.StartDiscoveryAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        await TrackingServiceUpdateState(TrackingServiceState.Idle, waitCompletion: false).ConfigureAwait(false);
                    }

                    m_TrackingStateUpdateEvent.Set();
                    return new UpdateStateResult {IsError = false};

                case TrackingServiceState.Stopping:

                    if (m_CurrentState == TrackingServiceState.Running || m_CurrentState == TrackingServiceState.Calibration || m_CurrentState == TrackingServiceState.DiagnosticMode)
                    {
                        // Stop BodyTracking Service
                        m_BodyTrackingService.StopTracking();
                    }

                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("TrackingServiceUpdateState - State: {0}", m_CurrentState);
                    }

                    if (m_CurrentState == TrackingServiceState.Running || m_CurrentState == TrackingServiceState.DiagnosticMode)
                    {
                        await TrackingServiceUpdateState(TrackingServiceState.Idle, waitCompletion: false).ConfigureAwait(false);
                    }
                    else if (m_CurrentState == TrackingServiceState.Calibration)
                    {
                        await TrackingServiceUpdateState(TrackingServiceState.Starting, waitCompletion: false).ConfigureAwait(false);
                    }

                    m_TrackingStateUpdateEvent.Set();
                    return new UpdateStateResult {IsError = false};

                case TrackingServiceState.Error:

                    m_CurrentState = TrackingServiceState.Error;
                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("TrackingServiceUpdateState - State: {0}", m_CurrentState);
                    }

                    OnServiceStatusChanged(m_CurrentState);

                    m_TrackingStateUpdateEvent.Set();
                    return new UpdateStateResult {IsError = true, ErrorCode = TrackingServiceStateErrors.Unknown, ErrorDescription = "Unknown Tracking Service error"};

                case TrackingServiceState.AutoDiscovery:

                    var autoDiscoveryArgs = (EnableAutoDiscoveryEventArgs) args;

                    if (m_CurrentState == TrackingServiceState.Running || m_CurrentState == TrackingServiceState.Calibration)
                    {
                        // Stop BodyTracking Service
                        m_BodyTrackingService.StopTracking();
                    }

                    // Reset Configuration
                    m_ConfigurationService.ClearKnownDataSources(autoDiscoveryArgs.Parameters.ClearMasterDataSource);
                    if (autoDiscoveryArgs.Parameters.ClearCalibrationData)
                    {
                        m_ConfigurationService.ClearCalibrationData();
                    }

                    await m_ConfigurationService.SaveCurrentConfigurationAsync().ConfigureAwait(false);

                    m_CurrentState = TrackingServiceState.AutoDiscovery;

                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("TrackingServiceUpdateState - State: {0}", m_CurrentState);
                    }

                    OnServiceStatusChanged(m_CurrentState);

                    m_NetworkDiscoveryService.DataSourceFound += NetworkDiscoveryService_DataSourceFound;
                    m_NetworkDiscoveryService.DiscoveryCompleted += NetworkDiscoveryService_DiscoveryCompleted;
                    await m_NetworkDiscoveryService.StartDiscoveryAsync().ConfigureAwait(false);

                    m_TrackingStateUpdateEvent.Set();
                    return new UpdateStateResult {IsError = false};

                case TrackingServiceState.DiagnosticMode:

                    if (string.IsNullOrEmpty(m_ConfigurationService.CurrentMasterDataSource))
                    {
                        if (m_Logger.IsErrorEnabled)
                        {
                            m_Logger.Error("Tracking System is not configured. Please set a Master DataSource.");
                        }

                        OnServiceStatusChanged(TrackingServiceState.Warning, TrackingServiceStateErrors.NotConfigured);

                        m_TrackingStateUpdateEvent.Set();

                        return new UpdateStateResult {IsError = true, ErrorCode = TrackingServiceStateErrors.NotConfigured, ErrorDescription = "Tracking System is not configured: please set a Master DataSource. Tracking can't be started."};
                    }
                    
                    if (m_BodyTrackingService.CurrentState != BodyTrackingServiceState.Diagnostic)
                    {
                        var trackingSessionConfiguration = TrackingSessionConfiguration.Default;
                        trackingSessionConfiguration.DataSourceTrackingSettings.BodyClippingEdgesEnabled = true;
                        await m_BodyTrackingService.StartDiagnosticModeAsync(trackingSessionConfiguration).ConfigureAwait(false);
                    }

                    m_CurrentState = TrackingServiceState.DiagnosticMode;

                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("TrackingServiceUpdateState - State: {0}", m_CurrentState);
                    }
                    
                    if (!m_ConfigurationService.CalibrationData.CalibrationDone)
                    {
                        if (m_Logger.IsWarnEnabled)
                        {
                            m_Logger.Warn("TrackingSystem is not calibrated. Tracking is not reliable.");
                        }

                        OnServiceStatusChanged(TrackingServiceState.Warning, TrackingServiceStateErrors.NotCalibrated);
                    }
                    else
                    {
                        OnServiceStatusChanged(m_CurrentState);
                    }

                    m_TrackingStateUpdateEvent.Set();
                    return new UpdateStateResult {IsError = false};
            }

            m_TrackingStateUpdateEvent.Set();
            return new UpdateStateResult {IsError = true, ErrorCode = TrackingServiceStateErrors.Unknown, ErrorDescription = "Unknown Tracking Service Status requested"};
        }


        private void OnGetServiceStatus(GetServiceStatusEventArgs args)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("GetServiceStatus request");
            }

            var trackingServiceStatus = new TrackingServiceStatus
            {
                Version = AppVersions.RetrieveExecutableVersion(),
                CurrentState = m_CurrentState,
                CalibrationDone = m_ConfigurationService.CalibrationData.CalibrationDone,
                DataStreamers = m_BodyTrackingService.SceneDataStreamers,
                MasterDataStreamer = m_ConfigurationService.CurrentMasterDataSource,
                DataSources = m_ConfigurationService.KnownDataSources,
                MinDataSourcesForPlay = Math.Min(m_ConfigurationService.KnownDataSources.Count, m_ConfigurationService.CurrentConfiguration.MinDataSourcesForPlay), //do not return a minimum data sources required number that is bigger than the total number of known datasources
                DataFrameRate = m_ConfigurationService.CurrentConfiguration.UpdateLoopFrameRate,
                Scene = m_ConfigurationService.CurrentConfiguration.Scene,
            };

            // GIANNI TODO: find a better way to to model mapping...
            var result = new CommandResult<object>
            {
                RequestId = args.RequestId,
                Data = trackingServiceStatus.ConvertToWebModel(),
            };

            m_Messenger.Send(result);
        }

        private async void OnEnableAutoDiscovery(EnableAutoDiscoveryEventArgs args)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("EnableAutoDiscovery request");
            }

            var updateStateResult = await TrackingServiceUpdateState(TrackingServiceState.AutoDiscovery, args).ConfigureAwait(false);

            var result = new CommandResult<object>
            {
                RequestId = args.RequestId,
                Data = updateStateResult
            };

            m_Messenger.Send(result);
        }

        private async void OnStartTrackingSystem(StartTrackingSystemEventArgs args)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("StartTrackingSystem request");
            }

            var updateStateResult = await TrackingServiceUpdateState(TrackingServiceState.Running, args).ConfigureAwait(false);

            var result = new CommandResult<object>
            {
                RequestId = args.RequestId,
                Data = updateStateResult
            };

            m_Messenger.Send(result);
        }

        private async void OnStopTrackingSystem(StopTrackingSystemEventArgs args)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("StopTrackingSystem request");
            }

            var updateStateResult = await TrackingServiceUpdateState(TrackingServiceState.Stopping).ConfigureAwait(false);

            var result = new CommandResult<object>
            {
                RequestId = args.RequestId,
                Data = updateStateResult
            };

            m_Messenger.Send(result);
        }

        private async void OnExecuteCalibrationStep(ExecuteCalibrationStepEventArgs args)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("ExecuteCalibrationStep request");
            }

            var updateStateResult = await TrackingServiceUpdateState(TrackingServiceState.Calibration, args).ConfigureAwait(false);

            var result = new CommandResult<object>
            {
                RequestId = args.RequestId,
                Data = updateStateResult
            };

            m_Messenger.Send(result);
        }

        private async void OnSetMasterDataSource(SetMasterDataSourceEventArgs args)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("SetMasterDataSource({0}) request", args.DataSourceId);
            }

            var result = new CommandResult<object>
            {
                RequestId = args.RequestId
            };

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("SetMasterDataSource({0}): stopping Tracking System", args.DataSourceId);
            }

            await TrackingServiceUpdateState(TrackingServiceState.Stopping).ConfigureAwait(false);

            if (!m_ConfigurationService.SetMasterDataSource(args.DataSourceId))
            {
                if (m_Logger.IsErrorEnabled)
                {
                    m_Logger.Error("SetMasterDataSource({0}): failed. No Master DataSource configured.", args.DataSourceId);
                }

                result.Data = new UpdateStateResult {IsError = true, ErrorCode = TrackingServiceStateErrors.NotConfigured, ErrorDescription = "Unknown DataSource. No Master configured."};
            }
            else
            {
                if (m_Logger.IsInfoEnabled)
                {
                    m_Logger.Info("SetMasterDataSource({0}): Master DataSource changed. Re-calibration is required.", args.DataSourceId);
                }

                result.Data = new UpdateStateResult {IsError = false};
            }

            await m_ConfigurationService.SaveCurrentConfigurationAsync().ConfigureAwait(false);

            await TrackingServiceUpdateState(TrackingServiceState.Starting).ConfigureAwait(false);

            m_Messenger.Send(result);
        }

        private async void OnStartDiagnosticMode(StartDiagnosticModeEventArgs args)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("StartDiagnosticMode request");
            }

            var updateStateResult = await TrackingServiceUpdateState(TrackingServiceState.DiagnosticMode).ConfigureAwait(false);

            var result = new CommandResult<object>
            {
                RequestId = args.RequestId,
                Data = updateStateResult
            };

            m_Messenger.Send(result);
        }

        private async void OnStopDiagnosticMode(StopDiagnosticModeEventArgs args)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("StopDiagnosticMode request");
            }

            var updateStateResult = await TrackingServiceUpdateState(TrackingServiceState.Stopping).ConfigureAwait(false);

            var result = new CommandResult<object>
            {
                RequestId = args.RequestId,
                Data = updateStateResult
            };

            m_Messenger.Send(result);
        }

        private async void OnSystemReboot(SystemRebootEventArgs args)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("SystemReboot request");
                m_Logger.Debug("Stopping services...");
            }

            var updateStateResult = await TrackingServiceUpdateState(TrackingServiceState.Stopping).ConfigureAwait(false);

            var result = new CommandResult<object>
            {
                RequestId = args.RequestId,
                Data = updateStateResult
            };

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("Rebooting system");
            }

            m_Messenger.Send(result);

            await Task.Delay(m_ConfigurationService.CurrentConfiguration.SystemRebootDelayInMilliseconds).ConfigureAwait(false);

            SystemManagement.Reboot();
        }

        private async void OnSetSceneDescriptor(SetSceneDescriptorEventArgs args)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("SetSceneDescriptor() request");
            }

            var result = new CommandResult<object>
            {
                RequestId = args.RequestId
            };

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("SetSceneDescriptor(): stopping Tracking System");
            }

            await TrackingServiceUpdateState(TrackingServiceState.Stopping).ConfigureAwait(false);

            SceneDescriptor sceneDescriptor = null;

            if (args.HasValues) // else reset to built-in default configuration
            {
                sceneDescriptor = new SceneDescriptor
                {
                    // Floor values are retrieved to maintain an uniform mapping behaviour, bit those values
                    // will be ignored when updating the Scene Descriptor.

                    FloorClipPlane = new Vector4(args.FloorClipPlaneX, args.FloorClipPlaneY, args.FloorClipPlaneZ, args.FloorClipPlaneW),

                    // Stage Area, currently, it is not used anywhere
                    StageArea = new Boundaries
                    {
                        Center = new Vector3(args.StageAreaCenterX, args.StageAreaCenterY, args.StageAreaCenterZ),
                        Size = new Vector3(args.StageAreaSizeX, args.StageAreaSizeY, args.StageAreaSizeZ)
                    },

                    GameArea = new Boundaries
                    {
                        Center = new Vector3(args.GameAreaCenterX, args.GameAreaCenterY, args.GameAreaCenterZ),
                        Size = new Vector3(args.GameAreaSizeX, args.GameAreaSizeY, args.GameAreaSizeZ),
                    },

                    GameAreaInnerLimits = new Vector3(args.GameAreaInnerLimitsX, args.GameAreaInnerLimitsY, args.GameAreaInnerLimitsZ)
                };
            }

            m_ConfigurationService.SetSceneDescriptor(sceneDescriptor);

            if (m_Logger.IsInfoEnabled)
            {
                m_Logger.Info("SetSceneDescriptor(): new scene descriptor configured.");
            }

            result.Data = new UpdateStateResult {IsError = false};

            await m_ConfigurationService.SaveCurrentConfigurationAsync().ConfigureAwait(false);

            await TrackingServiceUpdateState(TrackingServiceState.Starting).ConfigureAwait(false);

            m_Messenger.Send(result);
        }

        private void OnServiceStatusChanged(TrackingServiceState status, TrackingServiceStateErrors error = TrackingServiceStateErrors.Unknown)
        {
            var localHandler = StatusChanged;
            if (localHandler != null)
            {
                localHandler(this, new TrackingServiceStatusChangedEventArgs(status, error));
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

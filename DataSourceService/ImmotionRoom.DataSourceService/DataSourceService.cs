namespace ImmotionAR.ImmotionRoom.DataSourceService
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using Helpers;
    using Helpers.Messaging;
    using Interfaces;
    using Logger;
    using Model;
    using Networking.Interfaces;
    using Recording;
    using Recording.Interfaces;
    using Services;

    public class DataSourceService : IDataSourceService
    {
        #region Events
        public event EventHandler<DataSourceServiceStatusChangedEventArgs> StatusChanged;
        public event EventHandler<TrackingServiceStatusChangedEventArgs> TrackingServiceStatusChanged;
        #endregion

        #region Private fields

        private readonly ILogger m_Logger;
        private readonly IConfigurationService m_ConfigurationService;
        private readonly INetworkDiscoveryService m_NetworkDiscoveryService;
        private readonly IDataStreamerService m_DataStreamerService;
        private readonly IMessenger m_Messenger;
        private readonly ICommandProcessor m_CommandProcessor;

        //private readonly ITcpClientFactory m_TcpClientFactory;

        private int m_AutoDiscoveryReachableTimeout;

        private int m_TrackingServiceMonitorInterval;
        private CancellationTokenSource m_CancellationTokenSource;

        private DataSourceState m_CurrentState;

        private readonly AutoResetEvent m_DataSourceStateUpdateEvent;

        #endregion

        #region Properties

        public IReadOnlyDictionary<string, TrackingServiceInfo> KnownTrackingServices
        {
            get
            {
                var tsCollection = new Dictionary<string, TrackingServiceInfo>();
                if (m_ConfigurationService.TrackingService != null)
                {
                    tsCollection = new Dictionary<string, TrackingServiceInfo> { { m_ConfigurationService.TrackingService.Id, m_ConfigurationService.TrackingService } };
                }

                return new ReadOnlyDictionary<string, TrackingServiceInfo>(tsCollection);
            }
        }


        public DataSourceState Status
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

        public DataSourceService(DataSourceConfiguration configuration, TrackingServiceInfo knownTrackingService, IDataSourceSensor dataSourceSensor, ITcpServerFactory tcpServerFactory, IUdpClientFactory udpClientFactory, INetworkClientFactory networkClientFactory, IVideoRecorder colorRecorder, IVideoRecorder depthRecorder)
        {
            m_Logger = LoggerService.GetLogger<DataSourceService>();
            m_Messenger = MessengerService.Messenger;

            m_ConfigurationService = new ConfigurationService();
            m_ConfigurationService.LoadExternalConfiguration(configuration);
            m_ConfigurationService.LoadInternalConfiguration(knownTrackingService);

            var streamingRecorder = new StreamingRecorder();

            m_CommandProcessor = new CommandProcessor(m_ConfigurationService);

            m_NetworkDiscoveryService = new NetworkDiscoveryService(m_ConfigurationService, udpClientFactory);

            m_DataStreamerService = new DataStreamerService(m_ConfigurationService, dataSourceSensor, tcpServerFactory, networkClientFactory, streamingRecorder, colorRecorder, depthRecorder);

            m_DataSourceStateUpdateEvent = new AutoResetEvent(true);
        }

        #endregion

        #region Methods

        public async Task Start()
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("DataSourceService.Start()");
            }

            await m_ConfigurationService.InitializeAsync().ConfigureAwait(false);

            // Configuration
            m_AutoDiscoveryReachableTimeout = m_ConfigurationService.CurrentConfiguration.AutoDiscovery.ReachableTimeoutInSeconds * 1000;
            m_TrackingServiceMonitorInterval = m_ConfigurationService.CurrentConfiguration.TrackingServiceMonitorIntervalInSeconds * 1000;

            m_CommandProcessor.Start();

            m_Messenger.Register<EnableAutoDiscoveryEventArgs>(this, OnEnableAutoDiscovery);
            m_Messenger.Register<GetServiceStatusEventArgs>(this, OnGetServiceStatus);
            m_Messenger.Register<StartTrackingSystemEventArgs>(this, OnStartTrackingSystem);
            m_Messenger.Register<StopTrackingSystemEventArgs>(this, OnStopTrackingSystem);
            m_Messenger.Register<SystemRebootEventArgs>(this, OnSystemReboot);

            await m_NetworkDiscoveryService.StartAsync().ConfigureAwait(false);

            DataSourceUpdateState(DataSourceState.Starting);
        }

        public Task Stop()
        {
            DataSourceUpdateState(DataSourceState.Stopped);

            m_CommandProcessor.Stop();

            m_Messenger.Unregister<EnableAutoDiscoveryEventArgs>(this);
            m_Messenger.Unregister<GetServiceStatusEventArgs>(this);
            m_Messenger.Unregister<StartTrackingSystemEventArgs>(this);
            m_Messenger.Unregister<StopTrackingSystemEventArgs>(this);
            m_Messenger.Unregister<SystemRebootEventArgs>(this);

            m_NetworkDiscoveryService.TrackingServiceFound -= NetworkDiscoveryService_TrackingServiceFound;
            m_NetworkDiscoveryService.DiscoveryCompleted -= NetworkDiscoveryService_DiscoveryCompleted;

            m_NetworkDiscoveryService.Stop();
            m_NetworkDiscoveryService.StopDiscovery();
            m_DataStreamerService.Stop();

            if (m_Logger.IsInfoEnabled)
            {
                m_Logger.Info("DataSource stopped", m_ConfigurationService.CurrentConfiguration.InstanceId);
            }

            return Task.FromResult<object>(null);
        }

        #endregion

        #region Private methods

        private async void NetworkDiscoveryService_DiscoveryCompleted(object sender, EventArgs e)
        {
            m_NetworkDiscoveryService.TrackingServiceFound -= NetworkDiscoveryService_TrackingServiceFound;
            m_NetworkDiscoveryService.DiscoveryCompleted -= NetworkDiscoveryService_DiscoveryCompleted;
            m_NetworkDiscoveryService.StopDiscovery();

            await m_ConfigurationService.SaveCurrentConfigurationAsync().ConfigureAwait(false);

            if (m_ConfigurationService.TrackingService != null)
            {
                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("Found Tracking Service on {0}:{1} {2}:{3}", m_ConfigurationService.TrackingService.DataStreamEndpoint, m_ConfigurationService.TrackingService.DataStreamPort, m_ConfigurationService.TrackingService.ControlApiEndpoint, m_ConfigurationService.TrackingService.ControlApiPort);
                }

                await Task.Delay(m_ConfigurationService.CurrentConfiguration.AutoDiscoveryDelayInMilliseconds).ConfigureAwait(false);
                DataSourceUpdateState(DataSourceState.Starting);
            }
            else
            {
                if (m_Logger.IsWarnEnabled)
                {
                    m_Logger.Warn("No Tracking Service found");
                }

                await Task.Delay(m_ConfigurationService.CurrentConfiguration.AutoDiscovery.RepeatIntervalInSeconds * 1000).ConfigureAwait(false); // Wait and then retry with AutoDiscovery
                DataSourceUpdateState(DataSourceState.Starting);
            }
        }

        private async void NetworkDiscoveryService_TrackingServiceFound(object sender, TrackingServiceFoundEventArgs e)
        {
            if (m_Logger.IsInfoEnabled)
            {
                m_Logger.Info("Discovery: found Tracking Service '{0}'", e.TrackingService.Id);
            }

            m_ConfigurationService.TrackingService = e.TrackingService;

            // Once a Tracking Service is found, Discovery process can be terminated.
            m_NetworkDiscoveryService.TrackingServiceFound -= NetworkDiscoveryService_TrackingServiceFound;
            m_NetworkDiscoveryService.DiscoveryCompleted -= NetworkDiscoveryService_DiscoveryCompleted;
            m_NetworkDiscoveryService.StopDiscovery();

            await m_ConfigurationService.SaveCurrentConfigurationAsync().ConfigureAwait(false);

            if (m_ConfigurationService.TrackingService != null)
            {
                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("Found Tracking Service on {0}:{1} {2}:{3}", m_ConfigurationService.TrackingService.DataStreamEndpoint, m_ConfigurationService.TrackingService.DataStreamPort, m_ConfigurationService.TrackingService.ControlApiEndpoint, m_ConfigurationService.TrackingService.ControlApiPort);
                }

                await Task.Delay(m_ConfigurationService.CurrentConfiguration.AutoDiscoveryDelayInMilliseconds).ConfigureAwait(false);
                DataSourceUpdateState(DataSourceState.Starting);
            }
        }

        private async void DataSourceUpdateState(DataSourceState newState, object args = null, bool waitCompletion = true)
        {
            if (waitCompletion)
            {
                m_DataSourceStateUpdateEvent.WaitOne();
            }

            if (m_CurrentState == newState)
            {
                // Do nothing
                m_DataSourceStateUpdateEvent.Set();
                return;
            }

            switch (newState)
            {
                case DataSourceState.Idle:

                    m_CurrentState = DataSourceState.Idle;

                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("DataSourceUpdateState - State: {0}", m_CurrentState);
                    }

                    if (m_Logger.IsInfoEnabled)
                    {
                        m_Logger.Info("Data Source is ready");
                    }

                    OnServiceStatusChanged(m_CurrentState);

                    m_DataSourceStateUpdateEvent.Set();

                    return;

                case DataSourceState.Running:

                    m_CurrentState = DataSourceState.Running;
                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("DataSourceUpdateState - State: {0}", m_CurrentState);
                    }

                    var startTrackingArgs = (StartTrackingSystemEventArgs)args;
                    var trackingSessionConfiguration = startTrackingArgs.Configuration;
                    if (!await m_DataStreamerService.StartAsync(trackingSessionConfiguration).ConfigureAwait(false))
                    {
                        if (m_Logger.IsDebugEnabled)
                        {
                            m_Logger.Error("DataSourceUpdateState - DataStreamerService not started.", m_CurrentState);
                            DataSourceUpdateState(DataSourceState.Idle);
                        }

                        m_DataSourceStateUpdateEvent.Set();
                        return;
                    }

                    OnServiceStatusChanged(m_CurrentState);

                    m_DataSourceStateUpdateEvent.Set();
                    return;

                case DataSourceState.Starting:

                    m_CurrentState = DataSourceState.Starting;

                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("DataSourceUpdateState - State: {0}", m_CurrentState);
                    }

                    OnServiceStatusChanged(m_CurrentState);

                    var trackingServiceConfigured = m_ConfigurationService.TrackingService != null;
                    if (!trackingServiceConfigured)
                    {
                        if (m_Logger.IsInfoEnabled)
                        {
                            m_Logger.Info("No Tracking Service configured: enter in AutoDiscovery mode");
                        }

                        m_CurrentState = DataSourceState.AutoDiscovery;

                        if (m_Logger.IsDebugEnabled)
                        {
                            m_Logger.Debug("DataSourceUpdateState - State: {0}", m_CurrentState);
                        }

                        OnServiceStatusChanged(m_CurrentState);

                        m_NetworkDiscoveryService.TrackingServiceFound += NetworkDiscoveryService_TrackingServiceFound;
                        m_NetworkDiscoveryService.DiscoveryCompleted += NetworkDiscoveryService_DiscoveryCompleted;

                        await m_NetworkDiscoveryService.StartDiscoveryAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        if (m_Logger.IsInfoEnabled)
                        {
                            m_Logger.Info("Tracking Service - Control Api: {0}:{1} Data Stream: {2}:{3}", m_ConfigurationService.TrackingService.ControlApiEndpoint, m_ConfigurationService.TrackingService.ControlApiPort, m_ConfigurationService.TrackingService.DataStreamEndpoint, m_ConfigurationService.TrackingService.DataStreamPort);
                        }

                        m_CancellationTokenSource = new CancellationTokenSource();
#pragma warning disable 4014
                        Task.Factory.StartNew(TrackingServiceMonitor, m_CancellationTokenSource.Token, TaskCreationOptions.LongRunning);
#pragma warning restore 4014

                        DataSourceUpdateState(DataSourceState.Idle, waitCompletion: false);
                    }

                    m_DataSourceStateUpdateEvent.Set();
                    return;

                case DataSourceState.Stopped:

                    if (m_CurrentState == DataSourceState.Running)
                    {
                        m_DataStreamerService.Stop();
                    }

                    DataSourceUpdateState(DataSourceState.Idle, waitCompletion: false);

                    m_DataSourceStateUpdateEvent.Set();
                    return;

                case DataSourceState.Error:
                    m_CurrentState = DataSourceState.Error;

                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("DataSourceUpdateState - State: {0}", m_CurrentState);
                    }

                    OnServiceStatusChanged(m_CurrentState);

                    m_DataSourceStateUpdateEvent.Set();
                    return;

                case DataSourceState.AutoDiscovery:

                    if (m_CurrentState == DataSourceState.Running)
                    {
                        // Stop TrackingServiceMonitor
                        m_CancellationTokenSource.Cancel();

                        // Stop Streamer Service
                        m_DataStreamerService.Stop();

                        // Reset Configuration
                        m_ConfigurationService.TrackingService = null;

                        await m_ConfigurationService.SaveCurrentConfigurationAsync().ConfigureAwait(false);
                    }

                    m_CurrentState = DataSourceState.AutoDiscovery;

                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("DataSourceUpdateState - State: {0}", m_CurrentState);
                    }

                    m_NetworkDiscoveryService.TrackingServiceFound += NetworkDiscoveryService_TrackingServiceFound;
                    m_NetworkDiscoveryService.DiscoveryCompleted += NetworkDiscoveryService_DiscoveryCompleted;

                    await m_NetworkDiscoveryService.StartDiscoveryAsync().ConfigureAwait(false);

                    OnServiceStatusChanged(m_CurrentState);

                    m_DataSourceStateUpdateEvent.Set();
                    return;
            }
        }

        private async void TrackingServiceMonitor(object arguments)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("TrackingServiceMonitor Started");
            }

            while (true)
            {
                // If configured, try to connect to the Tracking Service on the local network.
                // It should repeat periodically to verify that the Tracking Service is reachable
                if (m_ConfigurationService.TrackingService == null)
                {
                    if (m_Logger.IsWarnEnabled)
                    {
                        m_Logger.Warn("TrackingServiceMonitor: Tracking Service is not configured. Abort.");
                    }
                    return;
                }

                //if (m_Logger.IsDebugEnabled)
                //{
                //    m_Logger.Debug("TrackingServiceMonitor: connecting to Tracking Service '{0}'", m_ConfigurationService.TrackingService.Id);
                //}

                var isReachable = await TrackingServiceIsReachableAsync(m_ConfigurationService.TrackingService.ControlApiEndpoint, m_ConfigurationService.TrackingService.ControlApiPort).ConfigureAwait(false);

                var statusChanged = isReachable != m_ConfigurationService.TrackingService.IsReachable;
                m_ConfigurationService.TrackingService.IsReachable = isReachable;

                if (isReachable)
                {
                    m_ConfigurationService.TrackingService.LastSeen = DateTime.UtcNow;
                }

                // Always notify the status for System Tray UI refresh
                OnTrackingServiceStatusChanged(m_ConfigurationService.TrackingService.Id, isReachable);

                //if (m_Logger.IsDebugEnabled && isReachable)
                //{
                //    m_Logger.Debug("TrackingServiceMonitor: Tracking Service '{0}' is reachable", m_ConfigurationService.TrackingService.Id);
                //}
                if (m_Logger.IsWarnEnabled && !isReachable)
                {
                    m_Logger.Warn("TrackingServiceMonitor: Tracking Service '{0}' is not reachable", m_ConfigurationService.TrackingService.Id);
                }

                bool cancelled = m_CancellationTokenSource.Token.WaitHandle.WaitOne(m_TrackingServiceMonitorInterval);
                if (cancelled)
                {
                    if (m_Logger.IsInfoEnabled)
                    {
                        m_Logger.Info("TrackingServiceMonitor: requested to stop");
                    }
                    break;
                }
            }

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("TrackingServiceMonitor Stopped");
            }
        }

        // Check if the Tracking Service replied by Auto Discovery is Alive.
        private Task<bool> TrackingServiceIsReachableAsync(string address, int port)
        {
            return Task.Run(async () =>
            {
                string endpoint = string.Format("http://{0}:{1}/internal/v1/Service/Status", address, port);

                // SSL Support
                var httpClient = new HttpClient(new HttpClientHandler
                {
                    ClientCertificateOptions = ClientCertificateOption.Automatic,
                    UseProxy = true,
                    Proxy = WebRequest.DefaultWebProxy
                });

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // At the moment Authentication is NOT supported/implemented
                //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", null);
                httpClient.Timeout = TimeSpan.FromSeconds(5);

                using (httpClient)
                {
                    try
                    {
                        HttpResponseMessage response = await httpClient.GetAsync(endpoint).ConfigureAwait(false);
                        var res = response.IsSuccessStatusCode;
                        if (!res)
                        {
                            if (m_Logger.IsErrorEnabled)
                            {
                                m_Logger.Error("TrackingServiceIsReachableAsync - Request returned {0} {1}", (int)response.StatusCode, response.StatusCode);
                            }
                        }

                        return res;
                    }
                    catch (Exception ex)
                    {
                        var exToTrace = ex.InnerException;
                        if (exToTrace == null)
                        {
                            exToTrace = ex;
                        }

                        if (m_Logger.IsErrorEnabled)
                        {
                            m_Logger.Error("TrackingServiceIsReachableAsync - Unable to contact TrackingService: {0}", exToTrace.Message);
                        }

                        return false;
                    }
                }
            });
        }

        private void OnGetServiceStatus(GetServiceStatusEventArgs args)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("GetServiceStatus request");
            }

            var result = new CommandResult<object>
            {
                RequestId = args.RequestId,
                Data = new DataSourceServiceStatus { Version = AppVersions.RetrieveExecutableVersion(), CurrentState = m_CurrentState, DataStreamerEndpoint = m_ConfigurationService.CurrentConfiguration.DataStreamerEndpoint, DataStreamerPort = m_ConfigurationService.CurrentConfiguration.DataStreamerPort }
            };

            m_Messenger.Send(result);
        }

        private void OnEnableAutoDiscovery(EnableAutoDiscoveryEventArgs args)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("EnableAutoDiscovery request");
            }

            DataSourceUpdateState(DataSourceState.AutoDiscovery);

            var result = new CommandResult<object>
            {
                RequestId = args.RequestId,
                Data = null
            };

            m_Messenger.Send(result);
        }

        private void OnStartTrackingSystem(StartTrackingSystemEventArgs args)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("StartTrackingSystem request");
            }

            DataSourceUpdateState(DataSourceState.Running, args);

            var result = new CommandResult<object>
            {
                RequestId = args.RequestId,
                Data = null
            };

            m_Messenger.Send(result);
        }

        private void OnStopTrackingSystem(StopTrackingSystemEventArgs args)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("StopTrackingSystem request");
            }

            DataSourceUpdateState(DataSourceState.Stopped);

            var result = new CommandResult<object>
            {
                RequestId = args.RequestId,
                Data = null
            };

            m_Messenger.Send(result);
        }

        private void OnStartSessionRecording(StartSessionRecordingEventArgs args)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("StartSessionRecording request");
            }

            m_DataStreamerService.StartRecordingAsync();

            var result = new CommandResult<object>
            {
                RequestId = args.RequestId,
                Data = null
            };

            m_Messenger.Send(result);
        }

        private void OnStopSessionRecording(StopSessionRecordingEventArgs args)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("StopSessionRecording request");
            }

            m_DataStreamerService.StopRecording();

            var result = new CommandResult<object>
            {
                RequestId = args.RequestId,
                Data = null
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

            DataSourceUpdateState(DataSourceState.Stopped);

            var result = new CommandResult<object>
            {
                RequestId = args.RequestId,
                Data = null
            };

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("Rebooting system");
            }

            m_Messenger.Send(result);

            await Task.Delay(m_ConfigurationService.CurrentConfiguration.SystemRebootDelayInMilliseconds).ConfigureAwait(false);

            SystemManagement.Reboot();
        }

        private void OnServiceStatusChanged(DataSourceState status, DataSourceStateErrors error = DataSourceStateErrors.Unknown)
        {
            var localHandler = StatusChanged;
            if (localHandler != null)
            {
                localHandler(this, new DataSourceServiceStatusChangedEventArgs(status, error));
            }
        }

        private void OnTrackingServiceStatusChanged(string trackingServiceId, bool isActive)
        {
            var localHandler = TrackingServiceStatusChanged;
            if (localHandler != null)
            {
                localHandler(this, new TrackingServiceStatusChangedEventArgs(trackingServiceId, isActive));
            }
        }
        #endregion
    }
}

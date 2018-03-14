namespace ImmotionAR.ImmotionRoom.TrackingService.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Helpers;
    using Infrastructure.Network;
    using Interfaces;
    using Logger;
    using Model;
    using Networking;
    using Networking.Interfaces;
    using TrackingEngine.Interfaces;
    using TrackingEngine.Model;

    public class DataSourceService : IDataSourceService, IBodyDataProvider
    {
        #region Events
        public event EventHandler<DataSourceStatusChangedEventArgs> DataSourceStatusChanged;
        #endregion

        #region Private fields

        protected readonly ILogger m_Logger;
        private readonly IConfigurationService m_ConfigurationService;
        private readonly IDataSourceControl m_DataSourceControl;
        private readonly ITcpClientFactory m_TcpClientFactory;

        private CancellationTokenSource m_CancellationTokenSource;
        private CancellationTokenSource m_CancellationTokenSourceMonitor;

        private bool m_Started;

        private int m_DataSourceReachableTimeout;

        private IReadOnlyDictionary<string, DataSourceInfo> m_KnownDataSources;
        private IDictionary<string, DataSourceClient> m_DataSourceClients;

        private ConcurrentDictionary<string, SceneFrame> m_BodyDataFromDataSources;
        private ConcurrentDictionary<string, SceneFrame> m_BodyDataForTrackingEngine;
        private ConcurrentDictionary<string, byte> m_DataSourceMapping;

        private TrackingSessionDataSourceConfiguration m_TrackingSessionConfiguration;

        #endregion

        #region Properties

        public IDictionary<string, SceneFrame> DataSources
        {
            get
            {
                if (m_Started)
                {
                    return m_BodyDataForTrackingEngine;
                }

                // Not yet started!   
                return new Dictionary<string, SceneFrame>(StringComparer.OrdinalIgnoreCase);
            }
        }

        public IDictionary<string, byte> DataSourceMapping
        {
            get
            {
                if (m_Started)
                {
                    return m_DataSourceMapping;
                }

                // Not yet started!   
                return new Dictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
            }
        }

        #endregion

        #region Constructor

        public DataSourceService(IConfigurationService configurationService, IDataSourceControl dataSourceControl, ITcpClientFactory tcpClientFactory)
        {
            Requires.NotNull(configurationService, "configurationService");
            Requires.NotNull(dataSourceControl, "dataSourceControl");

            m_Logger = LoggerService.GetLogger<DataSourceService>();
            m_ConfigurationService = configurationService;
            m_DataSourceControl = dataSourceControl;
            m_TcpClientFactory = tcpClientFactory;
        }

        #endregion

        #region Methods

        public void StartMonitor()
        {
            if (m_CancellationTokenSourceMonitor != null)
            {
                m_CancellationTokenSourceMonitor.Cancel();
            }

            m_CancellationTokenSourceMonitor = new CancellationTokenSource();

            // ReSharper disable once CSharpWarnings::CS4014
#pragma warning disable 4014
            Task.Factory.StartNew(DataSourceReachabilityMonitor, m_CancellationTokenSourceMonitor.Token, TaskCreationOptions.LongRunning);
#pragma warning restore 4014
        }

        public void StopMonitor()
        {
            if (m_CancellationTokenSourceMonitor != null)
            {
                m_CancellationTokenSourceMonitor.Cancel();
            }
        }

        public Task StartAsync(TrackingSessionDataSourceConfiguration trackingSessionConfiguration)
        {
            // Configuration
            m_DataSourceReachableTimeout = m_ConfigurationService.CurrentConfiguration.DataSourceReachableTimeoutInSeconds * 1000;

            m_TrackingSessionConfiguration = trackingSessionConfiguration;

            m_KnownDataSources = m_ConfigurationService.KnownDataSources;

            m_BodyDataFromDataSources = new ConcurrentDictionary<string, SceneFrame>();
            m_BodyDataForTrackingEngine = new ConcurrentDictionary<string, SceneFrame>();
            m_DataSourceMapping = new ConcurrentDictionary<string, byte>();
            foreach (var knownDataSource in m_ConfigurationService.KnownDataSources.Values)
            {
                m_DataSourceMapping.TryAdd(knownDataSource.Id, knownDataSource.UniqueId);
            }

            m_CancellationTokenSource = new CancellationTokenSource();

            m_DataSourceClients = new Dictionary<string, DataSourceClient>(StringComparer.OrdinalIgnoreCase);

            var activableDataSources = m_KnownDataSources.Count;

            foreach (var dataSource in m_KnownDataSources.Values)
            {
                if (activableDataSources == 0)
                {
                    if (m_Logger.IsErrorEnabled)
                    {
                        m_Logger.Error("MAX DATASOURCE NUMBER REACHED");
                    }
                    break;
                }

                activableDataSources--;

                // Send StartTrackingSystem to all known DataSources
                // If already started, the command will be ignored.
                var task = m_DataSourceControl.StartTrackingAsyncFor(m_TrackingSessionConfiguration, dataSource.ControlApiEndpoint, dataSource.ControlApiPort);

                var client = new DataSourceClient(m_TcpClientFactory)
                {
                    Id = dataSource.Id,
                    UniqueId = dataSource.UniqueId,
                    IP = dataSource.DataStreamEndpoint,
                    Port = dataSource.DataStreamPort
                };

                client.DataReady += DataSourceClient_OnDataReady;
                m_DataSourceClients.Add(client.Id, client);

                var startTime = DateTime.UtcNow;
                m_BodyDataForTrackingEngine.TryAdd(dataSource.Id, new SceneFrame { Timestamp = startTime });
            }

            // DISABLED WAITING FOR ALL DataSource StartTracking Command, because the timeout for command result in WebAPI is just 5s
            // and sometimes it is not enough for the complete round-trip (when some DataSource is not reachable...).
            //Task.WaitAll(startTrackingTasks.ToArray());

            // ReSharper disable once CSharpWarnings::CS4014
#pragma warning disable 4014
            Task.Factory.StartNew(DataSourceMonitor, m_CancellationTokenSource.Token, TaskCreationOptions.LongRunning);
#pragma warning restore 4014

            m_Started = true;
            return Task.FromResult(true);
        }

        public void Stop()
        {
            m_Started = false;

            if (m_CancellationTokenSource != null)
            {
                m_CancellationTokenSource.Cancel();
            }

            if (m_DataSourceClients != null)
            {
                foreach (var dataSourceClient in m_DataSourceClients.Values)
                {
                    dataSourceClient.DataReady -= DataSourceClient_OnDataReady;
                    dataSourceClient.Disconnect();
                }
            }

            if (m_KnownDataSources != null)
            {
                var stopTrackingTasks = new List<Task>();
                foreach (var dataSource in m_KnownDataSources.Values)
                {
                    // Send StopTrackingSystem to all known DataSources
                    // If already started, the command will be ignored.
                    var t = m_DataSourceControl.StopTrackingAsyncFor(dataSource.ControlApiEndpoint, dataSource.ControlApiPort);
                    stopTrackingTasks.Add(t);
                }

                // TODO: disabled, because blocking Tracking Service shutdown
                // Task.WaitAll(stopTrackingTasks.ToArray());
                stopTrackingTasks.Clear();
            }

            if (m_BodyDataFromDataSources != null)
            {
                m_BodyDataFromDataSources.Clear();
            }

            if (m_BodyDataForTrackingEngine != null)
            {
                m_BodyDataForTrackingEngine.Clear();
            }

            if (m_DataSourceMapping != null)
            {
                m_DataSourceMapping.Clear();
            }
        }

        public void Update(double deltaTime)
        {
            if (!m_Started)
            {
                // Not yet started!
                return;
            }

            // Update data from all know Data Sources
            var keys = new List<string>(m_BodyDataFromDataSources.Keys);
            foreach (var dataSourceId in keys)
            {
                if (!m_DataSourceClients.ContainsKey(dataSourceId) || !m_BodyDataForTrackingEngine.ContainsKey(dataSourceId))
                {
                    continue;
                }

                if (m_DataSourceClients[dataSourceId].IsConnected)
                {
                    SceneFrame sourceFrame;
                    if (m_BodyDataFromDataSources.TryGetValue(dataSourceId, out sourceFrame))
                    {
                        m_BodyDataForTrackingEngine[dataSourceId] = sourceFrame;
                    }
                }
                else
                {
                    if (m_BodyDataForTrackingEngine.ContainsKey(dataSourceId))
                    {
                        m_BodyDataForTrackingEngine[dataSourceId].Bodies.Clear();
                    }
                }
            }
        }

        #endregion

        #region Private methods

        private void DataSourceMonitor(object arguments)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("DataSourceMonitor Started");
            }

            var dataSourceAliveTimeInMsec = m_ConfigurationService.CurrentConfiguration.DataSourceAliveTimeInSeconds * 1000;
            var dataSourceMonitorInterval = m_ConfigurationService.CurrentConfiguration.DataSourceMonitorIntervalInSeconds * 1000;

            while (true)
            {
                // If configured, try to connect to all known Data Sources on the local network

                Parallel.ForEach(m_DataSourceClients.Values,
                    async client =>
                    {
                        if (client.IsConnected)
                        {
                            if ((DateTime.UtcNow - client.LastReceivedData).TotalMilliseconds > dataSourceAliveTimeInMsec)
                            {
                                client.Disconnect();

                                if (m_Logger.IsDebugEnabled)
                                {
                                    m_Logger.Warn("DataSource '{0}' seems connected, but LastDataReceived is more than {1}s ago. Considering Disconnected...", client.Id, m_ConfigurationService.CurrentConfiguration.DataSourceAliveTimeInSeconds);
                                }
                            }
                            else
                            {
                                return;
                            }
                        }

                        if (m_Logger.IsDebugEnabled)
                        {
                            m_Logger.Debug("DataSourceMonitor: connecting to Data Source '{0}'", client.Id);
                        }

                        var retries = m_ConfigurationService.CurrentConfiguration.DataSourceUnreachableMaxRetries;
                        while (retries > 0)
                        {
                            try
                            {
                                var isReachable = await DataSourceIsReachableAsync(client.IP, client.Port).ConfigureAwait(false);
                                if (isReachable)
                                {
                                    client.Connect();
                                    OnDataSourceServiceStatusChanged(client.Id, true);
                                    return;
                                }

                                client.Disconnect();

                                OnDataSourceServiceStatusChanged(client.Id, false);

                                if (m_Logger.IsInfoEnabled)
                                {
                                    m_Logger.Warn("DataSource '{0}' is not reachable. Sending StartTracking", client.Id); //, m_ConfigurationService.CurrentConfiguration.DataSourceUnreachableMaxRetries - retries + 1);
                                }

                                // Try to send a new StartTracking command: After max number of retries, STOP sending StartTracking
                                // If already started, the command will be ignored (and the DataSource is really unreachable for unknown reasons)

                                if (m_KnownDataSources == null)
                                {
                                    // DataSourceService requested to stop.
                                    return;
                                }

                                if (!m_KnownDataSources.ContainsKey(client.Id))
                                {
                                    // DataSource reconfigured ?!
                                    if (m_Logger.IsDebugEnabled)
                                    {
                                        m_Logger.Warn("DataSource '{0}' is not a KnownDataSource. A reconfiguration is needed?", client.Id);
                                    }

                                    OnDataSourceServiceStatusChanged(client.Id, false);
                                    return;
                                }

                                await m_DataSourceControl.StartTrackingAsyncFor(m_TrackingSessionConfiguration, m_KnownDataSources[client.Id].ControlApiEndpoint, m_KnownDataSources[client.Id].ControlApiPort).ConfigureAwait(false);
                                await Task.Delay(m_ConfigurationService.CurrentConfiguration.DataSourceUnreachableRetryIntervalInMilliseconds).ConfigureAwait(false);
                                retries--;
                            }
                            catch (NullReferenceException ex)
                            {
                                if (m_Logger.IsErrorEnabled)
                                {
                                    m_Logger.Error(ex, "DataSource({0}) Connect fail: {1}", client.Id, ex.Message);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (m_Logger.IsErrorEnabled)
                                {
                                    m_Logger.Error(ex, "DataSource({0}) Connect fail: {1}", client.Id, ex.Message);
                                }
                            }

                            OnDataSourceServiceStatusChanged(client.Id, false);
                        }
                    });

                if (m_Logger.IsDebugEnabled)
                {
                    var connected = 0;
                    foreach (var client in m_DataSourceClients.Values)
                    {
                        if (client.IsConnected)
                        {
                            connected++;
                        }
                    }
                    m_Logger.Debug("DataSourceClients: {0} known, {1} connected", m_KnownDataSources.Count, connected);
                }

                var cancelled = m_CancellationTokenSource.Token.WaitHandle.WaitOne(dataSourceMonitorInterval);
                if (cancelled)
                {
                    if (m_Logger.IsInfoEnabled)
                    {
                        m_Logger.Info("DataSourceMonitor: requested to stop");
                    }
                    break;
                }
            }

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("DataSourceMonitor Stopped");
            }
        }

        // Here is to check if the Data Source replied by Auto Discovery is alive.
        private Task<bool> DataSourceIsReachableAsync(string address, int port)
        {
            return Task.Run(() =>
            {
                var client = m_TcpClientFactory.CreateClient();
                var isReachable = IPEndPoint.TestReachability(client, address, port, m_DataSourceReachableTimeout);
                return isReachable;
            });
        }

        private void DataSourceClient_OnDataReady(object sender, DataFrameReadyEventArgs dataReadyEventArgs)
        {
            var dataSource = (DataSourceClient)sender;

            m_BodyDataFromDataSources[dataSource.Id] = dataReadyEventArgs.Frame;
        }

        private void DataSourceReachabilityMonitor(object arguments)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("DataSourceReachabilityMonitor Started");
            }

            var dataSourceMonitorInterval = m_ConfigurationService.CurrentConfiguration.DataSourceApiMonitorIntervalInSeconds * 1000;

            while (true)
            {
                if (!m_Started)
                {
                    // If configured, try to verify if all known Data Sources on the local network are reachable.
                    // If a tracking session is active, DO NOT check. The check is automatically done by DataSourceMonitor.
                    Parallel.ForEach(m_ConfigurationService.KnownDataSources.Values,
                        async ds =>
                        {
                            //if (m_Logger.IsDebugEnabled)
                            //{
                            //    m_Logger.Debug("DataSourceReachabilityMonitor: verifying Data Source '{0}'", ds.Id);
                            //}

                            try
                            {
                                var isReachable = await m_DataSourceControl.GetStatusAsyncFor(ds.ControlApiEndpoint, ds.ControlApiPort).ConfigureAwait(false);
                                if (isReachable)
                                {
                                    OnDataSourceServiceStatusChanged(ds.Id, true);
                                    return;
                                }

                                if (m_Logger.IsInfoEnabled)
                                {
                                    m_Logger.Warn("DataSource '{0}' is not reachable.", ds.Id);
                                    //m_Logger.Warn("To check: is it powered-on? Is it connected to the network? Is the service running? Are its network settings correct?");
                                }
                            }
                            catch (NullReferenceException ex)
                            {
                                if (m_Logger.IsErrorEnabled)
                                {
                                    m_Logger.Error(ex, "DataSource({0}) API request fail: {1}", ds.Id, ex.Message);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (m_Logger.IsErrorEnabled)
                                {
                                    m_Logger.Error(ex, "DataSource({0}) API request exception: {1}", ds.Id, ex.Message);
                                }
                            }

                            OnDataSourceServiceStatusChanged(ds.Id, false);
                        });
                }

                var cancelled = m_CancellationTokenSourceMonitor.Token.WaitHandle.WaitOne(dataSourceMonitorInterval);
                if (cancelled)
                {
                    if (m_Logger.IsInfoEnabled)
                    {
                        m_Logger.Info("DataSourceReachabilityMonitor: requested to stop");
                    }
                    break;
                }

            }

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("DataSourceReachabilityMonitor Stopped");
            }
        }

        private void OnDataSourceServiceStatusChanged(string dataSourceId, bool isActive)
        {
            var localHandler = DataSourceStatusChanged;
            if (localHandler != null)
            {
                localHandler(this, new DataSourceStatusChangedEventArgs(dataSourceId, isActive));
            }
        }
        #endregion
    }
}

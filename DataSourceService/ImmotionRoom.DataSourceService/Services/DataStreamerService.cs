namespace ImmotionAR.ImmotionRoom.DataSourceService.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Helpers;
    using Interfaces;
    using Logger;
    using Model;
    using Networking;
    using Networking.Interfaces;
    using Protocol;
    using Recording.Interfaces;
    using Recording.Model;

    public class DataStreamerService : IDataStreamerService
    {
        #region Private fields

        private static readonly object LockObj = new object();

        private readonly ILogger m_Logger;
        private readonly IDataSourceSensor m_DataSourceSensor;
        private readonly IConfigurationService m_ConfigurationService;
        private CancellationTokenSource m_CancellationTokenSource;

        private readonly IVideoRecorder m_ColorStreamRecorder;
        private readonly IVideoRecorder m_DepthStreamRecorder;
        private readonly IStreamingRecorder m_SkeletonDataRecorder;

        private bool m_ListenerRunning;
        private MemoryStream m_MemoryStream;
        private BinaryWriter m_BinaryWriter;
        private bool m_ListenerStarting;
        private bool m_ServiceStarted;

        private List<INetworkClient> ClientList { get; set; }

        private readonly ITcpServerFactory m_TcpServerFactory;
        private readonly INetworkClientFactory m_NetworkClientFactory;
        private ITcpServer m_TcpServer;

        #endregion

        #region Constructor

        public DataStreamerService(IConfigurationService configurationService, IDataSourceSensor dataSourceSensor, ITcpServerFactory tcpServerFactory, INetworkClientFactory networkClientFactory, IStreamingRecorder streamingRecorder, IVideoRecorder colorRecorder, IVideoRecorder depthRecorder)
        {
            Requires.NotNull(configurationService, "configurationService");
            Requires.NotNull(dataSourceSensor, "dataSourceSensor");
            Requires.NotNull(tcpServerFactory, "tcpServerFactory");
            Requires.NotNull(networkClientFactory, "networkClientFactory");
            Requires.NotNull(streamingRecorder, "streamingRecorder");
            Requires.NotNull(colorRecorder, "colorRecorder");
            Requires.NotNull(depthRecorder, "depthRecorder");

            m_Logger = LoggerService.GetLogger<DataStreamerService>();
            m_ConfigurationService = configurationService;
            m_DataSourceSensor = dataSourceSensor;

            m_TcpServerFactory = tcpServerFactory;
            m_NetworkClientFactory = networkClientFactory;

            m_SkeletonDataRecorder = streamingRecorder;
            m_ColorStreamRecorder = colorRecorder;
            m_DepthStreamRecorder = depthRecorder;

            ClientList = new List<INetworkClient>();
        }

        #endregion

        #region Overridden Methods

        public Task<bool> StartAsync(TrackingSessionConfiguration trackingSessionConfiguration)
        {
            RemoveClients();

            // Try to avoid multiple Starts
            if (!m_ServiceStarted)
            {
                lock (LockObj)
                {
                    if (!m_ServiceStarted)
                    {
                        m_ServiceStarted = true;
                    }
                    else
                    {
                        return Task.FromResult(false);
                    }
                }
            }
            else
            {
                return Task.FromResult(true);
            }

            StartRecordingAsync(); // if enabled

            m_DataSourceSensor.SkeletonStreamEnabled = true;
            m_DataSourceSensor.SkeletonDataAvailable += DataSourceSensor_DataAvailable;
            m_DataSourceSensor.SensorStatusChanged += DataSourceSensor_SensorStatusChanged;

            if (!m_DataSourceSensor.Start(trackingSessionConfiguration))
            {
                if (m_Logger.IsErrorEnabled)
                {
                    m_Logger.Error("DataStreamerService Start Abort.");
                }

                m_DataSourceSensor.SkeletonStreamEnabled = false;
                m_DataSourceSensor.SkeletonDataAvailable -= DataSourceSensor_DataAvailable;
                m_DataSourceSensor.SensorStatusChanged -= DataSourceSensor_SensorStatusChanged;

                lock (LockObj)
                {
                    m_ServiceStarted = false;
                }

                return Task.FromResult(false);
            }

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("DataStreamerService Started");
            }

            return Task.FromResult(true);
        }

        public void Stop()
        {
            m_DataSourceSensor.SkeletonDataAvailable -= DataSourceSensor_DataAvailable;
            m_DataSourceSensor.SensorStatusChanged -= DataSourceSensor_SensorStatusChanged;
            m_DataSourceSensor.Stop();

            StopDataSourceSensor();

            StopListener();

            StopRecording(); // if was enabled

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("DataStreamerService Stopped");
            }

            lock (LockObj)
            {
                m_ServiceStarted = false;
            }
        }

        private void StopListener()
        {
            if (m_TcpServer == null)
            {
                return;
            }

            if (!m_ListenerRunning)
            {
                return;
            }

            try
            {
                m_TcpServer.ClientConnected -= TcpServer_ClientConnected;
                m_TcpServer.Stop();
            }
            catch (NetworkException)
            {
                // Ignore. Termination requested.
            }

            m_ListenerRunning = false;

            RemoveClients();

            if (m_Logger.IsInfoEnabled)
            {
                m_Logger.Info("ClientListener: stopped");
            }
        }

        private void StopDataSourceSensor()
        {
            if (m_CancellationTokenSource != null)
            {
                m_CancellationTokenSource.Cancel();
            }

            if (m_BinaryWriter != null)
            {
                m_BinaryWriter.Dispose();
                m_BinaryWriter = null;
            }

            if (m_MemoryStream != null)
            {
                m_MemoryStream.Dispose();
                m_MemoryStream = null;
            }

            lock (ClientList)
            {
                foreach (var client in ClientList)
                {
                    client.Close();
                }
            }

            RemoveClients();
        }

        #endregion

        #region Public methods

        public async void StartRecordingAsync()
        {
            string sessionId = null;

            if (m_ConfigurationService.CurrentConfiguration.SkeletonDataRecorderEnabled)
            {
                m_SkeletonDataRecorder.DataRecorderSessionPath = m_ConfigurationService.CurrentConfiguration.DataRecorderSessionPath;
                m_DataSourceSensor.SkeletonStreamEnabled = true;
                sessionId = await m_SkeletonDataRecorder.StartRecordingAsync();

                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("Started Skeleton Data recording session {0}", sessionId);
                }
            }

            if (m_ConfigurationService.CurrentConfiguration.ColorStreamRecorderEnabled)
            {
                m_ColorStreamRecorder.CapturedVideoFps = m_ConfigurationService.CurrentConfiguration.ColorStreamRecorderFps;
                m_ColorStreamRecorder.CapturedVideoWidth = m_ConfigurationService.CurrentConfiguration.ColorStreamRecorderWidth;
                m_ColorStreamRecorder.CapturedVideoHeight = m_ConfigurationService.CurrentConfiguration.ColorStreamRecorderHeight;
                m_ColorStreamRecorder.DataRecorderSessionPath = m_ConfigurationService.CurrentConfiguration.DataRecorderSessionPath;
                m_ColorStreamRecorder.StreamType = StreamType.Color;

                m_DataSourceSensor.ColorDataAvailable += DataSourceSensor_ColorDataAvailable;
                m_DataSourceSensor.ColorStreamEnabled = true;
                sessionId = await m_ColorStreamRecorder.StartRecordingAsync(sessionId);
                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("Started Video (Color) Recording session {0}", sessionId);
                }
            }

            if (m_ConfigurationService.CurrentConfiguration.DepthStreamRecorderEnabled)
            {
                m_DepthStreamRecorder.CapturedVideoFps = m_ConfigurationService.CurrentConfiguration.DepthStreamRecorderFps;
                m_DepthStreamRecorder.CapturedVideoWidth = m_ConfigurationService.CurrentConfiguration.DepthStreamRecorderWidth;
                m_DepthStreamRecorder.CapturedVideoHeight = m_ConfigurationService.CurrentConfiguration.DepthStreamRecorderHeight;
                m_DepthStreamRecorder.DataRecorderSessionPath = m_ConfigurationService.CurrentConfiguration.DataRecorderSessionPath;
                m_DepthStreamRecorder.StreamType = StreamType.Depth;

                m_DataSourceSensor.DepthDataAvailable += DataSourceSensor_DepthDataAvailable;
                m_DataSourceSensor.DepthStreamEnabled = true;
                sessionId = await m_DepthStreamRecorder.StartRecordingAsync(sessionId);
                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("Started Video (Depth) Recording session {0}", sessionId);
                }
            }
        }

        public void StopRecording()
        {
            string sessionId;

            if (m_ConfigurationService.CurrentConfiguration.SkeletonDataRecorderEnabled)
            {
                m_DataSourceSensor.SkeletonStreamEnabled = false;
                sessionId = m_SkeletonDataRecorder.StopRecording();
                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("Stopped Skeleton Data recording session {0}", sessionId);
                }
            }

            if (m_ConfigurationService.CurrentConfiguration.ColorStreamRecorderEnabled)
            {
                m_DataSourceSensor.ColorStreamEnabled = false;
                m_DataSourceSensor.ColorDataAvailable -= DataSourceSensor_ColorDataAvailable;
                sessionId = m_ColorStreamRecorder.StopRecording();
                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("Stopped Video (Color) Recording session {0}", sessionId);
                }
            }

            if (m_ConfigurationService.CurrentConfiguration.DepthStreamRecorderEnabled)
            {
                m_DataSourceSensor.DepthStreamEnabled = false;
                m_DataSourceSensor.DepthDataAvailable -= DataSourceSensor_DepthDataAvailable;
                sessionId = m_DepthStreamRecorder.StopRecording();
                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("Stopped Video (Depth) Recording session {0}", sessionId);
                }
            }
        }

        #endregion

        #region Private methods

        private void DataSourceSensor_DataAvailable(object sender, DataSourceDataAvailableEventArgs args)
        {
            if (args == null || args.Data == null)
            {
                return;
            }

            if (m_SkeletonDataRecorder.IsRecording)
            {
                m_SkeletonDataRecorder.NewDataAvailableHandler(this, args.Data);
            }

            try
            {
                if (ClientList.Count == 0)
                {
                    return;
                }

                var dataToSend = SerializeObjectToByteArray(args.Data);

                Parallel.For(0, ClientList.Count, index =>
                {
                    if (index >= ClientList.Count)
                    {
                        return;
                    }

                    var sc = ClientList[index];
                    sc.Send(BitConverter.GetBytes(dataToSend.Length));
                    sc.Send(dataToSend);
                });
            }
            catch (Exception ex)
            {
                if (m_Logger.IsErrorEnabled)
                {
                    m_Logger.Error(ex, "DataSourceSensor_DataAvailable Exception");
                }
            }

            RemoveClients();
        }

        private void DataSourceSensor_SensorStatusChanged(object sender, SensorStatusChangedEventArgs args)
        {
            if (!m_ListenerRunning && args.IsActive)
            {
                // Try to avoid multiple Listener activations
                if (!m_ListenerStarting)
                {
                    lock (LockObj)
                    {
                        if (!m_ListenerStarting)
                        {
                            m_ListenerStarting = true;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                else
                {
                    return;
                }

                m_CancellationTokenSource = new CancellationTokenSource();
                Task.Factory.StartNew(ClientListener, m_CancellationTokenSource.Token, TaskCreationOptions.LongRunning);

                m_MemoryStream = new MemoryStream();
                m_BinaryWriter = new BinaryWriter(m_MemoryStream);
            }
            else if (m_ListenerRunning && !args.IsActive)
            {
                StopDataSourceSensor();
            }
        }

        private void DataSourceSensor_ColorDataAvailable(object sender, DataSourceImageDataAvailableEventArgs args)
        {
            if (args == null || args.Data == null)
            {
                return;
            }

            if (m_ColorStreamRecorder.IsRecording)
            {
                m_ColorStreamRecorder.NewDataAvailableHandler(this, args.Data);
            }
        }

        private void DataSourceSensor_DepthDataAvailable(object sender, DataSourceImageDataAvailableEventArgs args)
        {
            if (args == null || args.Data == null)
            {
                return;
            }

            if (m_DepthStreamRecorder.IsRecording)
            {
                m_DepthStreamRecorder.NewDataAvailableHandler(this, args.Data);
            }
        }

        private void HandleClientConnected(ITcpClient client)
        {
            string[] clientId = { Guid.NewGuid().ToString("N").ToUpper() };

            lock (ClientList)
            {
                while (ClientList.Exists(c => c.Id == clientId[0]))
                {
                    clientId[0] = Guid.NewGuid().ToString("N").ToUpper();
                }

                var sc = m_NetworkClientFactory.CreateClient(client, clientId[0]);

                ClientList.Add(sc);
                m_DataSourceSensor.SourceEnabled = ClientList.Count > 0;
            }

            if (m_Logger.IsInfoEnabled)
            {
                m_Logger.Info("Client connected: {0}", client.RemoteEndPoint != null ? client.RemoteEndPoint.ToString() : "-");
            }
        }

        private void RemoveClients()
        {
            lock (ClientList)
            {
                for (var i = 0; i < ClientList.Count; i++)
                {
                    if (!ClientList[i].IsConnected)
                    {
                        if (m_Logger.IsInfoEnabled)
                        {
                            m_Logger.Info("Client disconnected{0}", ClientList[i].RemoteEndPoint != null ? string.Format(": {0}", ClientList[i].RemoteEndPoint) : string.Empty);
                        }

                        ClientList.Remove(ClientList[i]);
                    }
                }

                m_DataSourceSensor.SourceEnabled = ClientList.Count > 0;
            }
        }


        private async void ClientListener(object arguments)
        {
            StopListener();

            var retries = m_ConfigurationService.CurrentConfiguration.ClientListenerStartMaxRetries;
            while (!m_ListenerRunning && retries > 0)
            {
                try
                {
                    m_TcpServer = m_TcpServerFactory.CreateServer(m_ConfigurationService.CurrentConfiguration.DataStreamerEndpoint, m_ConfigurationService.CurrentConfiguration.DataStreamerPort);
                    m_TcpServer.ClientConnected += TcpServer_ClientConnected;
                    await m_TcpServer.StartAsync().ConfigureAwait(false);

                    m_ListenerRunning = true;

                    lock (LockObj)
                    {
                        m_ListenerStarting = false;
                    }
                }
                catch (NetworkException ex)
                {
                    if (m_Logger.IsWarnEnabled)
                    {
                        m_Logger.Warn(ex, "Unable to start TcpListener on {0}... retrying in {0}s...", m_ConfigurationService.CurrentConfiguration.DataStreamerPort, m_ConfigurationService.CurrentConfiguration.ClientListenerTimeoutInMilliseconds / 1000);
                    }

                    m_TcpServer.ClientConnected -= TcpServer_ClientConnected;
                    m_ListenerRunning = false;
                    retries--;
                }

                if (!m_ListenerRunning) // Wait some time before retrying...
                {
                    await Task.Delay(m_ConfigurationService.CurrentConfiguration.ClientListenerTimeoutInMilliseconds).ConfigureAwait(false);
                }
            }

            if (retries <= 0)
            {
                if (m_Logger.IsErrorEnabled)
                {
                    m_Logger.Error("Unable to start TcpListener. Abort. ");
                }
                return;
            }


            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("Waiting for clients...");
            }

            while (true)
            {
                try
                {
                    // Wait for clients, if not stopped

                    bool cancelled = m_CancellationTokenSource.Token.WaitHandle.WaitOne(m_ConfigurationService.CurrentConfiguration.ClientListenerTimeoutInMilliseconds);
                    if (cancelled)
                    {
                        if (m_Logger.IsInfoEnabled)
                        {
                            m_Logger.Info("ClientListener: requested to stop");
                        }
                        break;
                    }
                }
                catch (NetworkException)
                {
                    // Ignore. Termination requested.
                    break;
                }
                catch (ObjectDisposedException)
                {
                    // Ignore. Termination requested.
                    break;
                }
                catch (InvalidOperationException ex)
                {
                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Error(ex, "ClientListener: InvalidOperationException \"{0}\"", ex.Message);
                    }
                    break;
                }
            }

            StopListener();
        }

        // See: http://stackoverflow.com/questions/4865104/convert-any-object-to-a-byte
        private byte[] SerializeObjectToByteArray(SensorDataFrame obj)
        {
            var serializer = new SensorDataFrameSerializer();
            return serializer.Serialize(obj);
        }

        private void TcpServer_ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            HandleClientConnected(e.Client);
        }
        #endregion
    }
}

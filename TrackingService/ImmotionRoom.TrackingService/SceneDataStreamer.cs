namespace ImmotionAR.ImmotionRoom.TrackingService
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DataClient.Model;
    using Infrastructure.Network;
    using Interfaces;
    using Logger;
    using Model;
    using Networking;
    using Networking.Interfaces;
    using TrackingEngine.Model;
    
    public class SceneDataStreamer
    {
        #region Events

        public event EventHandler<EventArgs> ClientConnected;
        public event EventHandler<EventArgs> ClientDisconnected;

        #endregion

        #region Private fields

        private readonly int m_ClientListenerTimeout;
        //private readonly int m_TcpListenerBacklog;
        private CancellationTokenSource m_CancellationTokenSource;

        private bool m_ListenerRunning;
        private MemoryStream m_MemoryStream;
        private BinaryWriter m_BinaryWriter;

        private List<INetworkClient> ClientList { get; set; }

        private readonly ILogger m_Logger;
        private readonly ITcpServerFactory m_TcpServerFactory;
        private readonly INetworkClientFactory m_NetworkClientFactory;
        private readonly IConfigurationService m_ConfigurationService;
        private ITcpServer m_TcpServer;

        private readonly TrackingServiceSceneFrameSerializer m_DataSerializer;
        private readonly IDictionary<string, TrackingServiceSceneDataStreamModes> m_ClientStreamModes;

        #endregion

        #region Properties
        

        public string DataSourceId { get; private set; }
        //public byte DataSourceUniqueId { get; private set; }

        public string StreamingEndpoint { get; private set; }

        public int StreamingPort { get; private set; }

        public bool IsMaster { get; private set; }

        public Dictionary<TrackingServiceSceneDataStreamModes, Matrix4x4> TransformationMatrices { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        ///     It is the component which streams SceneData from the Tracking Service to an interested application.
        ///     A SceneDataStream is associated to a specific DataSource. DataSource parameters should be retrieved
        ///     automatically through AutoDiscovery.
        /// </summary>
        /// <param name="dataSourceId">DataSource from which receive and re-stream BodyData</param>
        ///// <param name="dataSourceUniqueId">DataSource's UniqueId</param>
        /// <param name="streamingEndpoint">DataSource Streamer IP</param>
        /// <param name="streamingPort">DataSource Streamer Port</param>
        /// <param name="isMaster">True if this the BodyStreamer associated to the Master DataSource</param>
        public SceneDataStreamer(string dataSourceId, /*byte dataSourceUniqueId, */string streamingEndpoint, int streamingPort, bool isMaster, ITcpServerFactory tcpServerFactory, INetworkClientFactory networkClientFactory, IConfigurationService configurationService)
        {
            m_Logger = LoggerService.GetLogger<SceneDataStreamer>();

            DataSourceId = dataSourceId;
            //DataSourceUniqueId = dataSourceUniqueId;
            StreamingEndpoint = streamingEndpoint;
            StreamingPort = streamingPort;
            IsMaster = isMaster;
            m_TcpServerFactory = tcpServerFactory;
            m_NetworkClientFactory = networkClientFactory;
            m_ConfigurationService = configurationService;

            m_ClientListenerTimeout = m_ConfigurationService.CurrentConfiguration.DataStreamerClientTimeoutInMilliseconds;

            ClientList = new List<INetworkClient>();
            
            m_ClientStreamModes = new Dictionary<string, TrackingServiceSceneDataStreamModes>(StringComparer.OrdinalIgnoreCase);
            TransformationMatrices = new Dictionary<TrackingServiceSceneDataStreamModes, Matrix4x4>(TrackingServiceBodyStreamModesComparer.Instance);

            m_DataSerializer = new TrackingServiceSceneFrameSerializer();
                 
            // Initialize all TransformationMatrices to Identity. They will be updated to the calibrated matrices in the UpdateLoop.
            var streamModes = (TrackingServiceSceneDataStreamModes[]) Enum.GetValues(typeof (TrackingServiceSceneDataStreamModes));
            foreach (TrackingServiceSceneDataStreamModes streamMode in streamModes)
            {
                TransformationMatrices.Add(streamMode, Matrix4x4.Identity);
            }
        }

        #endregion

        #region Overridden Methods

        /// <summary>
        ///     Start the SceneDataStreamer for the specified DataSourceId, waiting for clients
        /// </summary>
        public bool Start()
        {
            // Prior to start, disconnect any pending connected client from past sessions
            RemoveClients();

            if (string.IsNullOrEmpty(DataSourceId))
            {
                throw new ArgumentException("DataSourceId not defined");
            }

            if (!m_ListenerRunning)
            {
                m_CancellationTokenSource = new CancellationTokenSource();
                Task.Factory.StartNew(ClientListener, m_CancellationTokenSource.Token, TaskCreationOptions.LongRunning);

                m_MemoryStream = new MemoryStream();
                m_BinaryWriter = new BinaryWriter(m_MemoryStream);
            }

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("SceneDataStreamer[{0}] - Started", DataSourceId);
            }

            return true;
        }

        /// <summary>
        ///     Stop the SceneDataStreamer, disconnecting any connected client
        /// </summary>
        public bool Stop()
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
                foreach (INetworkClient client in ClientList)
                {
                    client.Close();
                }
            }

            RemoveClients();

            if (m_TcpServer != null)
            {
                m_TcpServer.Stop();
            }

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("SceneDataStreamer[{0}] - Stopped", DataSourceId);
            }

            return true;
        }

        /// <summary>
        ///     Send SceneData to each connected client, in parallel, according to the requested transformed stream type
        /// </summary>
        /// <param name="frame"></param>
        public void SendDataToClients(SceneFrame frame)
        {
            if (!m_ListenerRunning)
            {
                return;
            }

            try
            {
                // OPTIMIZATION: 
                // Transform and send transformed data only if there are clients,
                // and only for those which are interested in a specific type of data
                if (ClientList.Count == 0)
                {
                    return;
                }

                // Count how many clients want to receive raw data
                TrackingServiceSceneDataStreamModes[] clientList = m_ClientStreamModes.Values.ToArray();
                int rawDataClients = clientList.Count(c => c == TrackingServiceSceneDataStreamModes.Raw);
                byte[] rawDataToSend = null;
                byte[] rawDataToSendLength = null;
                if (rawDataClients > 0)
                {
                    rawDataToSend = SerializeObjectToByteArray(frame.ConvertToTrackingServiceSceneFrame(TransformationMatrices[TrackingServiceSceneDataStreamModes.Raw]));
                    rawDataToSendLength = BitConverter.GetBytes(rawDataToSend.Length);
                }

                // Count how many clients want to receive MasterTransform-ed data
                int masterTransformedDataClients = clientList.Count(c => c == TrackingServiceSceneDataStreamModes.MasterTransform);
                byte[] masterTransformedDataToSend = null;
                byte[] masterTransformedDataToSendLength = null;
                if (masterTransformedDataClients > 0)
                {
                    masterTransformedDataToSend = SerializeObjectToByteArray(frame.ConvertToTrackingServiceSceneFrame(TransformationMatrices[TrackingServiceSceneDataStreamModes.MasterTransform]));
                    masterTransformedDataToSendLength = BitConverter.GetBytes(masterTransformedDataToSend.Length);
                }

                // Count how many clients want to receive WorldTransform-ed data
                int worldTransformedDataClients = clientList.Count(c => c == TrackingServiceSceneDataStreamModes.WorldTransform);
                byte[] worldTransformedDataToSend = null;
                byte[] worldTransformedDataToSendLength = null;
                if (worldTransformedDataClients > 0)
                {
                    worldTransformedDataToSend = SerializeObjectToByteArray(frame.ConvertToTrackingServiceSceneFrame(TransformationMatrices[TrackingServiceSceneDataStreamModes.WorldTransform]));
                    worldTransformedDataToSendLength = BitConverter.GetBytes(worldTransformedDataToSend.Length);
                }

                if(clientList.Length == 0)
                {
                    return;
                }

                // Send data to each client, in parallel, according to the requested transformed stream type
                Parallel.For(0, ClientList.Count, index =>
                {
                    INetworkClient sc = ClientList[index];
                    byte[] dataToSend;
                    byte[] dataToSendLength;

                    // sc.Id --> use this id to retrieve the correct transformed data
                    switch (m_ClientStreamModes[sc.Id])
                    {
                        case TrackingServiceSceneDataStreamModes.Raw:
                            dataToSend = rawDataToSend;
                            dataToSendLength = rawDataToSendLength;
                            break;

                        case TrackingServiceSceneDataStreamModes.MasterTransform:
                            dataToSend = masterTransformedDataToSend;
                            dataToSendLength = masterTransformedDataToSendLength;
                            break;

                        case TrackingServiceSceneDataStreamModes.WorldTransform:
                            dataToSend = worldTransformedDataToSend;
                            dataToSendLength = worldTransformedDataToSendLength;
                            break;

                        default:
                            return;
                    }

                    if (dataToSendLength == null || dataToSend == null)
                    {
                        return;
                    }

                    sc.Send(dataToSendLength);
                    sc.Send(dataToSend);
                });
            }
            catch (Exception ex)
            {
                if (m_Logger.IsErrorEnabled)
                {
                    m_Logger.Error(ex, "SceneDataStreamer[{0}] - SendDataToClients Exception", DataSourceId);
                }
            }

            RemoveClients();
        }

        #endregion

        #region Private methods

        private void HandleClientConnected(ITcpClient client)
        {
            string clientId = Guid.NewGuid().ToString("N").ToUpper();

            lock (ClientList)
            {
                while (m_ClientStreamModes.ContainsKey(clientId))
                {
                    clientId = Guid.NewGuid().ToString("N").ToUpper();
                }

                var sc = m_NetworkClientFactory.CreateClient(client, clientId);

                ClientList.Add(sc);
                m_ClientStreamModes.Add(clientId, TrackingServiceSceneDataStreamModes.Raw);
            }

            if (m_Logger.IsInfoEnabled)
            {
                m_Logger.Info("SceneDataStreamer[{0}] - Client {1} ({2}) connected", DataSourceId, clientId, client.RemoteEndPoint);
            }

            OnClientConnected(clientId, client.RemoteEndPoint);

            ReceiveClientStreamMode(clientId);
        }

        private void RemoveClients()
        {
            lock (ClientList)
            {
                for (int i = 0; i < ClientList.Count; i++)
                {
                    if (!ClientList[i].IsConnected)
                    {
                        OnClientDisconnected(ClientList[i].Id, ClientList[i].RemoteEndPoint);

                        if (m_Logger.IsInfoEnabled)
                        {
                            m_Logger.Info("SceneDataStreamer[{0}] - Client {1}{2} disconnected", DataSourceId, ClientList[i].Id, ClientList[i].RemoteEndPoint != null ? string.Format(" ({0})", ClientList[i].RemoteEndPoint) : string.Empty);
                        }

                        m_ClientStreamModes.Remove(ClientList[i].Id);
                        ClientList.Remove(ClientList[i]);
                    }
                }
            }
        }

        private async void ClientListener(object arguments)
        {
            int retries = m_ConfigurationService.CurrentConfiguration.DataStreamerListenerMaxRetries;

            while (!m_ListenerRunning && retries > 0)
            {
                try
                {
                    m_TcpServer = m_TcpServerFactory.CreateServer(StreamingEndpoint, StreamingPort);
                    m_TcpServer.ClientConnected += TcpServer_ClientConnected;
                    await m_TcpServer.StartAsync().ConfigureAwait(false);

                    m_ListenerRunning = true;
                }
                catch (NetworkException ex)
                {
                    if (m_Logger.IsWarnEnabled)
                    {
                        m_Logger.Warn(ex, "SceneDataStreamer[{0}] - Unable to start TcpServer on {1}... retrying in {2}s...", DataSourceId, StreamingPort, m_ConfigurationService.CurrentConfiguration.DataStreamerListenerRetryIntervalInMilliseconds/1000);
                    }

                    m_TcpServer.ClientConnected -= TcpServer_ClientConnected;
                    m_ListenerRunning = false;
                    retries--;
                }

                if (!m_ListenerRunning) // Wait some time before retrying...
                {
                    await Task.Delay(m_ConfigurationService.CurrentConfiguration.DataStreamerListenerRetryIntervalInMilliseconds).ConfigureAwait(false);
                }
            }

            if (retries <= 0)
            {
                if (m_Logger.IsErrorEnabled)
                {
                    m_Logger.Error("SceneDataStreamer[{0}] - Unable to start TcpServer. Abort. ", DataSourceId);
                }
                return;
            }

            m_ListenerRunning = true;

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("SceneDataStreamer[{0}] - Waiting for clients...", DataSourceId);
            }

            while (true)
            {
                try
                {
                    // Wait for clients, if not stopped

                    bool cancelled = m_CancellationTokenSource.Token.WaitHandle.WaitOne(m_ClientListenerTimeout);
                    if (cancelled)
                    {
                        if (m_Logger.IsInfoEnabled)
                        {
                            m_Logger.Info("SceneDataStreamer[{0}] - ClientListener: requested to stop", DataSourceId);
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
                m_Logger.Info("SceneDataStreamer[{0}] - ClientListener: stopped", DataSourceId);
            }
        }

        private void TcpServer_ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            HandleClientConnected(e.Client);
        }

        private byte[] SerializeObjectToByteArray(TrackingServiceSceneFrame obj)
        {
            return m_DataSerializer.Serialize(obj);
        }

        private void OnClientConnected(string clientId, IPEndPoint remoteEndpoint)
        {
            EventHandler<EventArgs> localHandler = ClientConnected;
            if (localHandler != null)
            {
                localHandler(this, EventArgs.Empty);
            }
        }

        private void OnClientDisconnected(string clientId, IPEndPoint remoteEndpoint)
        {
            EventHandler<EventArgs> localHandler = ClientDisconnected;
            if (localHandler != null)
            {
                localHandler(this, EventArgs.Empty);
            }
        }

        private void ReceiveClientStreamMode(string clientId)
        {
            INetworkClient sc = ClientList.FirstOrDefault(c => c.Id == clientId);
            if (sc == null)
            {
                return;
            }

            int mode = sc.ReadClientStreamMode();
            m_ClientStreamModes[clientId] = (TrackingServiceSceneDataStreamModes) mode;
        }

        #endregion
    }
}
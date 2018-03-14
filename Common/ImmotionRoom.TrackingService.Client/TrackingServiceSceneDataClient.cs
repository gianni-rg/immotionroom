namespace ImmotionAR.ImmotionRoom.TrackingService.DataClient
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using ControlClient.Model;
    using Logger;
    using Model;
    using Networking;
    using Networking.Interfaces;

    public class TrackingServiceSceneDataClient
    {
        public event EventHandler<TrackingServiceSceneFrameReadyEventArgs> DataReady;

        #region Private fields

        private static readonly object LockObj = new object();

        private readonly ILogger m_Logger;
        private ITcpClient m_Client;
        private readonly ITcpClientFactory m_ClientFactory;
        private readonly TrackingServiceSceneFrameSerializer m_Serializer;

        #endregion

        #region Properties

        public string Id { get; set; }

        public string IP { get; set; }

        public int Port { get; set; }

        public int ReceivedFrames { get; set; }
        public DateTime LastReceivedData { get; set; }
        public DateTime ConnectedOn { get; set; }
        public DateTime DisconnectedOn { get; set; }

        public TrackingServiceSceneFrame Data { get; private set; }
        public TrackingServiceSceneDataStreamModes Mode { get; private set; }

        public bool IsConnected
        {
            get { return m_Client != null && m_Client.Connected; }
        }

        #endregion

        #region Constructor

#if UNITY_5
        public TrackingServiceSceneDataClient()
        {
            m_Logger = LoggerService.GetLogger<TrackingServiceSceneDataClient>();
            m_ClientFactory = new TcpClientFactory();
            m_Serializer = new TrackingServiceSceneFrameSerializer();
        }
#else
        public TrackingServiceSceneDataClient(ITcpClientFactory tcpClientFactory)
        {
            if (tcpClientFactory == null)
            {
                throw new ArgumentNullException("tcpClientFactory");
            }

            m_Logger = LoggerService.GetLogger<TrackingServiceSceneDataClient>();
            m_ClientFactory = tcpClientFactory;
            m_Serializer = new TrackingServiceSceneFrameSerializer();
        }
#endif

        #endregion

        #region Methods

#if UNITY_5
        public void Connect(TrackingServiceSceneDataStreamModes mode)
#else
        public async void Connect(TrackingServiceSceneDataStreamModes mode)
#endif
        {
            Mode = mode;

            if (m_Client == null)
            {
                lock (LockObj)
                {
                    if (m_Client == null)
                    {
                        m_Client = m_ClientFactory.CreateClient();
                    }
                }
            }

            if (m_Client != null && !m_Client.Connected)
            {
                try
                {
#if UNITY_5
    // GIANNI TODO: check to handle Async/Await here in Unity3D
                    m_Client.ConnectAsync(IP, Port).Wait();
#else
                    await m_Client.ConnectAsync(IP, Port);
#endif
                }
                catch (NullReferenceException)
                {
                    // Ignore.
                }
                catch (NetworkException)
                {
                    // Ignore
                }
                catch (ObjectDisposedException)
                {
                    // Ignore
                }
            }
            else
            {
                return;
            }

            if (m_Client != null && m_Client.Connected)
            {
                // Send selected TrackingServiceBodyStreamModes for this client to Tracking Service 
                var buf = BitConverter.GetBytes((int) mode);
                m_Client.GetStream().Write(buf, 0, buf.Length);

// ReSharper disable once CSharpWarnings::CS4014
#pragma warning disable 4014
                Task.Factory.StartNew(DataListener, TaskCreationOptions.LongRunning);
#pragma warning restore 4014
            }
        }

        public void Disconnect()
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("Disconnect[{0}] called", Id);
            }

            if (m_Client != null)
            {
                lock (LockObj)
                {
                    if (m_Client != null)
                    {
                        m_Client.Close();
                        m_Client = null;
                    }
                }
            }
        }

        #endregion

        #region Private methods

        private void DataListener(object arguments)
        {
            if (m_Client == null)
            {
                return;
            }

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("DataListener[{0}] started", Id);
            }

            ReceivedFrames = 0;
            ConnectedOn = DateTime.UtcNow;

            try
            {
                var ns = m_Client.GetStream();
                var reader = new BinaryReader(ns);

                while (m_Client.Connected)
                {
                    var size = reader.ReadInt32();
                    var data = reader.ReadBytes(size);

                    Data = (TrackingServiceSceneFrame) DeserializeByteArrayToObject(data);

                    LastReceivedData = DateTime.UtcNow;
                    ReceivedFrames++;
                    OnDataReady(Data);

                    if (m_Client == null)
                    {
                        break;
                    }
                }
            }
            catch (EndOfStreamException)
            {
                // Ignore. Client disconnected.
            }
            catch (IOException)
            {
                // Ignore. Client disconnected.
            }
            catch (NullReferenceException)
            {
                // Ignore. Client disconneted.
            }
            catch (Exception e)
            {
                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("DataListener[{0}] - Exception: {1}", Id, e.Message);
                }
            }
            finally
            {
                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("DataListener[{0}] disconnecting", Id);
                }

                if (m_Client != null)
                {
                    m_Client.Close();
                    m_Client = null;
                }

                DisconnectedOn = DateTime.UtcNow;
            }
        }

        private void OnDataReady(TrackingServiceSceneFrame bodyDataFrame)
        {
            var localHandler = DataReady;
            if (localHandler != null)
            {
                localHandler(this, new TrackingServiceSceneFrameReadyEventArgs(bodyDataFrame));
            }
        }

        private object DeserializeByteArrayToObject(byte[] arrBytes)
        {
            return m_Serializer.Deserialize(arrBytes);
        }

        #endregion
    }
}

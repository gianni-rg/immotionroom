namespace ImmotionAR.ImmotionRoom.TrackingService.Infrastructure.Network
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using Logger;
    using Model;
    using Networking;
    using Networking.Interfaces;
    using Protocol;

    public class DataSourceClient
    {
        public event EventHandler<DataFrameReadyEventArgs> DataReady;

        #region Private fields

        private static readonly object LockObj = new object();
        private readonly ITcpClientFactory m_TcpClientFactory;
        private ITcpClient m_Client;
        private readonly ILogger m_Logger;

        #endregion

        #region Properties

        public string Id { get; set; }
        public byte UniqueId { get; set; }

        public string IP { get; set; }

        public int Port { get; set; }

        public int ReceivedFrames { get; set; }
        public DateTime LastReceivedData { get; set; }
        public DateTime ConnectedOn { get; set; }
        public DateTime DisconnectedOn { get; set; }

        public SensorDataFrame SensorData { get; private set; }

        public bool IsConnected
        {
            get { return m_Client != null && m_Client.Connected; }
        }

        #endregion

        #region Constructor
        public DataSourceClient(ITcpClientFactory tcpClientFactory)
        {
            m_Logger = LoggerService.GetLogger<DataSourceClient>();
            m_TcpClientFactory = tcpClientFactory;
        } 
        #endregion

        #region Methods

        public async void Connect()
        {
            if (m_Client == null)
            {
                lock (LockObj)
                {
                    if (m_Client == null)
                    {
                        m_Client = m_TcpClientFactory.CreateClient();
                    }
                }
            }

            if (m_Client != null && !m_Client.Connected)
            {
                try
                {
                    await m_Client.ConnectAsync(IP, Port).ConfigureAwait(false);
                }
                catch (NetworkException)
                {
                    // Ignore.
                }
            }
            else
            {
                return;
            }

            if (m_Client != null && m_Client.Connected)
            {
                // ReSharper disable once CSharpWarnings::CS4014
#pragma warning disable 4014
                Task.Factory.StartNew(DataListener, TaskCreationOptions.LongRunning);
#pragma warning restore 4014
            }
        }

        public void Disconnect()
        {
            if (m_Client != null)
            {
                lock (LockObj)
                {
                    if (m_Client != null)
                    {
                        m_Client.Close();
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

                    SensorData = (SensorDataFrame)DeserializeByteArrayToObject(data);

                    LastReceivedData = DateTime.UtcNow;
                    ReceivedFrames++;
                    OnDataReady(SensorData);

                    if (m_Client == null)
                    {
                        break;
                    }
                }
            }
            catch (IOException)
            {
                // Ignore. Client disconnected.
            }
            catch (NetworkException)
            {
                // Ignore. Client disconnected.
            }
            catch (ObjectDisposedException)
            {
                // Ignore. Client disconnected.
            }
            catch (SerializationException ex)
            {
                // Shared Model issue?! Client disconneted.
                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug(ex, "DataSourceClient[{0}] - SerializationException: {1}", Id, ex.Message);
                }
            }
            finally
            {
                if (m_Client != null)
                {
                    m_Client.Close();
                    m_Client = null;
                }

                DisconnectedOn = DateTime.UtcNow;
            }
        }

        private void OnDataReady(SensorDataFrame sensorData)
        {
            var localHandler = DataReady;
            if (localHandler != null)
            {
                localHandler(this, new DataFrameReadyEventArgs(sensorData.ConvertToModel(UniqueId)));
            }
        }

        private object DeserializeByteArrayToObject(byte[] arrBytes)
        {
            var serializer = new SensorDataFrameSerializer();
            return serializer.Deserialize(arrBytes);
        }

        #endregion
    }
}

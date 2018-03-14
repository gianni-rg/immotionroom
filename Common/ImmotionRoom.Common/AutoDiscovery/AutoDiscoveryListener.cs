namespace ImmotionAR.ImmotionRoom.AutoDiscovery
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Logger;
    using Networking;
    using Networking.Interfaces;

    // See: http://www.nullskull.com/a/1551/clientserver-autodiscovery-in-c-and-udp-sockets.aspx
    public class AutoDiscoveryListener
    {
        #region Private fields

        private readonly ILogger m_Logger;
        private readonly IUdpClientFactory m_UdpClientFactory;

        private IUdpClient m_UdpMulticastListener;

        private readonly ListenerTypes m_ListenerType;

        #endregion

        #region Properties

        public string InstanceId { get; set; }
        public string LocalAddress { get; set; }
        public string AutoDiscoveryMulticastAddress { get; set; }
        public string ControlApiEndpoint { get; set; }
        public int ControlApiPort { get; set; }
        public string DataStreamerEndpoint { get; set; }
        public int DataStreamerPort { get; set; }
        public int AutoDiscoveryMulticastPort { get; set; }
        public int AutoDiscoveryLocalPort { get; set; }
        public int AutoDiscoveryUdpLocalClientTimeout { get; set; }
        public bool AutoDiscoveryLoopbackLogEnabled { get; set; }
        public int AutoDiscoveryListenerPollingTime { get; set; }

        #endregion

        #region Constructor

        public AutoDiscoveryListener(ListenerTypes listenerType, IUdpClientFactory udpClientFactory)
        {
            m_ListenerType = listenerType;
            m_Logger = LoggerService.GetLogger<AutoDiscoveryListener>();
            m_UdpClientFactory = udpClientFactory;
        }

        #endregion

        #region Overridden Methods

        public async Task StartAsync()
        {
            m_UdpMulticastListener = await m_UdpClientFactory.CreateMulticastClientAsync(LocalAddress, AutoDiscoveryMulticastAddress, AutoDiscoveryMulticastPort).ConfigureAwait(false);

            if (m_ListenerType == ListenerTypes.TrackingServiceListener)
            {
                m_UdpMulticastListener.MessageReceived += UdpMulticastListenerMessageReceivedTrackingServiceHandler;
            }
            else
            {
                m_UdpMulticastListener.MessageReceived += UdpMulticastListenerMessageReceivedDataSourceHandler;
            }

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("AutoDiscoveryListener Started");
            }
        }

        public void Stop()
        {
            if (m_UdpMulticastListener != null)
            {
                if (m_ListenerType == ListenerTypes.TrackingServiceListener)
                {
                    m_UdpMulticastListener.MessageReceived -= UdpMulticastListenerMessageReceivedTrackingServiceHandler;
                }
                else
                {
                    m_UdpMulticastListener.MessageReceived -= UdpMulticastListenerMessageReceivedDataSourceHandler;
                }

                m_UdpMulticastListener.Close();
                m_UdpMulticastListener = null;
            }

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("AutoDiscoveryListener Stopped");
            }
        }

        #endregion

        #region Private methods

        private async void UdpMulticastListenerMessageReceivedDataSourceHandler(object sender, UdpMessageReceivedEventArgs e)
        {
            var incomingIp = e.RemoteEndpoint;

            // Data Source Discovery Message
            // Tracking Service send a Data Source Discovery Packet. Reply with ACKDS + Local Data Source Network Inf
            if (e.Data[0] == AutoDiscoveryPackets.DataSourceDiscoveryPacketBytes[0] && e.Data[1] == AutoDiscoveryPackets.DataSourceDiscoveryPacketBytes[1] && e.Data[2] == AutoDiscoveryPackets.DataSourceDiscoveryPacketBytes[2])
            {
                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("AutoDiscoveryMulticastListenerForDataSource: Data Source Discovery Request from {0}/UDP", incomingIp);
                }

                string licenseId = "N/A";

                var packetBytesAck = Encoding.UTF8.GetBytes(string.Format("ACKDS {0} {1} {2} {3} {4} {5}", InstanceId, DataStreamerEndpoint, DataStreamerPort, ControlApiEndpoint, ControlApiPort, licenseId)); // Acknowledged

                var remoteDiscoveryListenerPort = BitConverter.ToInt32(e.Data, 3);
                var remoteDiscoveryEndpoint = new IPEndPoint(incomingIp.Address, remoteDiscoveryListenerPort);
                var udpTempClient = await m_UdpClientFactory.CreateLocalClientAsync(LocalAddress, AutoDiscoveryLocalPort, AutoDiscoveryUdpLocalClientTimeout).ConfigureAwait(false);
                await udpTempClient.SendAsync(packetBytesAck, packetBytesAck.Length, remoteDiscoveryEndpoint).ConfigureAwait(false);
                udpTempClient.Close();

                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("AutoDiscoveryMulticastListenerForDataSource: Answering(ACKDS) {0} bytes to {1}", packetBytesAck.Length, remoteDiscoveryEndpoint);
                }
            }

            // Tracking Service Discovery Message (loopback)
            // Data Source send a Tracking Service Discovery Packet. Here is the loopback message --> Ignore
            else if (e.Data[0] == AutoDiscoveryPackets.TrackingServiceDiscoveryPacketBytes[0] && e.Data[1] == AutoDiscoveryPackets.TrackingServiceDiscoveryPacketBytes[1] && e.Data[2] == AutoDiscoveryPackets.TrackingServiceDiscoveryPacketBytes[2])
            {
                if (AutoDiscoveryLoopbackLogEnabled)
                {
                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("AutoDiscoveryMulticastListenerForDataSource: loopback Tracking Service Discovery Packet received. IGNORE.");
                    }
                }
            }
        }

        private async void UdpMulticastListenerMessageReceivedTrackingServiceHandler(object sender, UdpMessageReceivedEventArgs e)
        {
            var incomingIp = e.RemoteEndpoint;

            // Tracking Service Discovery Message
            // A DataSource send a Discovery Packet. Reply with ACK + Local Tracking Service Network Info
            if (e.Data[0] == AutoDiscoveryPackets.TrackingServiceDiscoveryPacketBytes[0] && e.Data[1] == AutoDiscoveryPackets.TrackingServiceDiscoveryPacketBytes[1] && e.Data[2] == AutoDiscoveryPackets.TrackingServiceDiscoveryPacketBytes[2])
            {
                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("AutoDiscoveryMulticastListenerForTrackingService: Tracking Service Discovery Request from {0}/UDP", incomingIp);
                }

                string licenseId = "N/A";

                var packetBytesAck = Encoding.UTF8.GetBytes(string.Format("ACK {0} {1} {2} {3} {4} {5}", InstanceId, DataStreamerEndpoint, DataStreamerPort, ControlApiEndpoint, ControlApiPort, licenseId)); // Acknowledged

                var remoteDiscoveryListenerPort = BitConverter.ToInt32(e.Data, 3);
                var remoteDiscoveryEndpoint = new IPEndPoint(incomingIp.Address, remoteDiscoveryListenerPort);

                var udpTempClient = await m_UdpClientFactory.CreateLocalClientAsync(LocalAddress, AutoDiscoveryLocalPort, AutoDiscoveryUdpLocalClientTimeout).ConfigureAwait(false);
                await udpTempClient.SendAsync(packetBytesAck, packetBytesAck.Length, remoteDiscoveryEndpoint).ConfigureAwait(false);
                udpTempClient.Close();

                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("AutoDiscoveryMulticastListenerForTrackingService: Answering(ACK) {0} bytes to {1}", packetBytesAck.Length, remoteDiscoveryEndpoint);
                }
            }

            // Data Source Discovery Message
            // Tracking Service send a Data Source Discovery Packet. Here is the loopback message --> Ignore
            else if (e.Data[0] == AutoDiscoveryPackets.DataSourceDiscoveryPacketBytes[0] && e.Data[1] == AutoDiscoveryPackets.DataSourceDiscoveryPacketBytes[1] && e.Data[2] == AutoDiscoveryPackets.DataSourceDiscoveryPacketBytes[2])
            {
                if (AutoDiscoveryLoopbackLogEnabled)
                {
                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("AutoDiscoveryMulticastListenerForTrackingService: loopback DataSource Discovery Packet received. IGNORE.");
                    }
                }
            }
        }

        #endregion
    }
}

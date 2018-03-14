namespace ImmotionAR.ImmotionRoom.AutoDiscovery
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Logger;
    using Networking;
    using Networking.Interfaces;

    // See: http://www.nullskull.com/a/1551/clientserver-autodiscovery-in-c-and-udp-sockets.aspx
    public class AutoDiscoveryDiscoverer
    {
        #region Events
        public event EventHandler<DeviceFoundEventArgs> DeviceFound;
        public event EventHandler DiscoveryCompleted;

        #endregion

        #region Private fields
        private readonly DiscovererTypes m_DiscovererType;

        private readonly ILogger m_Logger;
        private readonly IUdpClientFactory m_UdpClientFactory;

        private CancellationTokenSource m_CancellationTokenSource;
        private IPEndPoint m_UdpMulticastGroupAddress;
        private IUdpClient m_UdpLocalListener;
        #endregion

        #region Properties
        public string LocalAddress { get; set; }
        public string AutoDiscoveryMulticastAddress { get; set; }
        public int AutoDiscoveryMulticastPort { get; set; }
        public int AutoDiscoveryPollingInterval { get; set; }
        public int AutoDiscoveryDuration { get; set; }
        public int AutoDiscoveryLocalPort { get; set; }
        public bool AutoDiscoveryLoopbackLogEnabled { get; set; }
        public int AutoDiscoveryListenerPollingTime { get; set; }
        public int AutoDiscoveryUdpLocalClientTimeout { get; set; }
        
        #endregion

        #region Constructor

        public AutoDiscoveryDiscoverer(DiscovererTypes discovererType, IUdpClientFactory udpClientFactory)
        {
            m_DiscovererType = discovererType;
            m_Logger = LoggerService.GetLogger<AutoDiscoveryDiscoverer>();
            m_UdpClientFactory = udpClientFactory;
        }

        #endregion

        #region Overridden Methods

        public async Task StartAsync()
        {
            if (m_CancellationTokenSource != null)
            {
                m_CancellationTokenSource.Cancel();
            }

            m_CancellationTokenSource = new CancellationTokenSource();

            m_UdpMulticastGroupAddress = new IPEndPoint(AutoDiscoveryMulticastAddress, AutoDiscoveryMulticastPort);
            m_UdpLocalListener = await m_UdpClientFactory.CreateLocalClientAsync(LocalAddress, AutoDiscoveryLocalPort, AutoDiscoveryUdpLocalClientTimeout).ConfigureAwait(false);
            
            if (m_DiscovererType == DiscovererTypes.TrackingServiceDiscoverer)
            {
                // We are in a Data Source
                m_UdpLocalListener.MessageReceived += UdpLocalListenerMessageReceivedDataSourceHandler;
            }
            else
            {
                // We are in a Tracking Service
                m_UdpLocalListener.MessageReceived += UdpLocalListenerMessageReceivedTrackingServiceHandler;
            }

#pragma warning disable 4014
            Task.Factory.StartNew(Discoverer, m_CancellationTokenSource.Token, TaskCreationOptions.LongRunning);
#pragma warning restore 4014

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("AutoDiscoveryDiscoverer Started");
            }
        }

        public void Stop()
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("AutoDiscoveryDiscoverer Stop() called");
            }
            
            if (m_CancellationTokenSource != null)
            {
                m_CancellationTokenSource.Cancel();
            }
            
            if (m_UdpLocalListener != null)
            {
                if (m_DiscovererType == DiscovererTypes.TrackingServiceDiscoverer)
                {
                    // We are in a Data Source
                    m_UdpLocalListener.MessageReceived -= UdpLocalListenerMessageReceivedDataSourceHandler;
                }
                else
                {
                    // We are in a Tracking Service
                    m_UdpLocalListener.MessageReceived -= UdpLocalListenerMessageReceivedTrackingServiceHandler;
                }

                m_UdpLocalListener.Close();
                m_UdpLocalListener = null;
            }
            
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("AutoDiscoveryDiscoverer Stopped");
            }
        }

        #endregion

        #region Private methods

        private async void Discoverer(object arguments)
        {
            if (m_Logger.IsInfoEnabled)
            {
                m_Logger.Info("Starting discovery process");
            }

            byte[] localPortBytes = BitConverter.GetBytes(AutoDiscoveryLocalPort);
            byte[] packetBytes;

            if (m_DiscovererType == DiscovererTypes.DataSourceDiscoverer)
            {
                packetBytes = new byte[AutoDiscoveryPackets.DataSourceDiscoveryPacketBytes.Length + localPortBytes.Length];
                Buffer.BlockCopy(AutoDiscoveryPackets.DataSourceDiscoveryPacketBytes, 0, packetBytes, 0, AutoDiscoveryPackets.DataSourceDiscoveryPacketBytes.Length);
                Buffer.BlockCopy(localPortBytes, 0, packetBytes, AutoDiscoveryPackets.DataSourceDiscoveryPacketBytes.Length, localPortBytes.Length);
            }
            else
            {
                packetBytes = new byte[AutoDiscoveryPackets.TrackingServiceDiscoveryPacketBytes.Length + localPortBytes.Length];
                Buffer.BlockCopy(AutoDiscoveryPackets.TrackingServiceDiscoveryPacketBytes, 0, packetBytes, 0, AutoDiscoveryPackets.TrackingServiceDiscoveryPacketBytes.Length);
                Buffer.BlockCopy(localPortBytes, 0, packetBytes, AutoDiscoveryPackets.TrackingServiceDiscoveryPacketBytes.Length, localPortBytes.Length);
            }

            DateTime startTime = DateTime.UtcNow;

            while (true)
            {
                // If not configured, try to look for Data Sources on the local network.
                // It should repeat for a certain amount of time, to discover new DataSources at runtime

                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("{0}: looking for devices on the network", m_DiscovererType);
                }

                // Multicast the query
                try
                {
                    var udpMulticastTempClient = await m_UdpClientFactory.CreateMulticastClientAsync(LocalAddress, AutoDiscoveryMulticastAddress, AutoDiscoveryMulticastPort).ConfigureAwait(false);
                    await udpMulticastTempClient.SendAsync(packetBytes, packetBytes.Length, m_UdpMulticastGroupAddress).ConfigureAwait(false);
                    udpMulticastTempClient.Close();
                }
                catch (NetworkException se)
                {
                    if (m_Logger.IsErrorEnabled)
                    {
                        m_Logger.Error(se, "{0}: exception on Socket. Retrying...", m_DiscovererType);
                    }
                }

                if ((DateTime.UtcNow - startTime).TotalMilliseconds >= AutoDiscoveryDuration)
                {
                    if (m_Logger.IsInfoEnabled)
                    {
                        m_Logger.Info("Discovery completed");
                    }

                    OnDiscoveryCompleted();

                    break;
                }

                bool cancelled = m_CancellationTokenSource.Token.WaitHandle.WaitOne(AutoDiscoveryPollingInterval);
                if (cancelled)
                {
                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("DiscovererTask: requested to stop");
                    }
                    break;
                }
            }

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("DiscovererTask: stopped");
            }
        }

        private void OnDiscoveryCompleted()
        {
            EventHandler localHandler = DiscoveryCompleted;
            if (localHandler != null)
            {
                localHandler(this, EventArgs.Empty);
            }
        }
        
        private void OnDeviceFound(DeviceInfo deviceInfo)
        {
            EventHandler<DeviceFoundEventArgs> localHandler = DeviceFound;
            if (localHandler != null)
            {
                localHandler(this, new DeviceFoundEventArgs(deviceInfo));
            }
        }


        private void UdpLocalListenerMessageReceivedDataSourceHandler(object sender, UdpMessageReceivedEventArgs e)
        {
            string returnData = Encoding.UTF8.GetString(e.Data, 0, e.Data.Length);

            // Data Source ACKDS Message (loopback)
            // Tracking Service send a Data Source Discovery Packet. This is the loopback ACK + Local Data Source Network Info reply --> Ignore
            if (e.Data.Length > 5 && returnData.Substring(0, 5) == "ACKDS")
            {
                if (AutoDiscoveryLoopbackLogEnabled)
                {
                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("DataSourceAutoDiscoveryLocalListener: loopback Data Source Discovery Packet ACKDS received. IGNORE.");
                    }
                }
            }

            // Tracking Service ACK Message
            // Data Source send a Tracking Service Discovery Packet. Here is the ACK response from the Tracking Service.
            else if (e.Data.Length > 3 && returnData.Substring(0, 3) == "ACK")
            {
                string[] splitRcvd = returnData.Split(' ');

                string trackingServiceId = splitRcvd[1];
                string trackingServiceEndpoint = splitRcvd[2];
                int trackingServicePort = Convert.ToInt16(splitRcvd[3]);
                string controlApiEndpoint = splitRcvd[4];
                int controlApiPort = Convert.ToInt16(splitRcvd[5]);
                string licenseId = splitRcvd[6];

                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("DataSourceAutoDiscoveryLocalListener: ACK response received. Tracking Service is '{0}' at {1}:{2} {3}:{4}", trackingServiceId, trackingServiceEndpoint, trackingServicePort, controlApiEndpoint, controlApiPort);
                }

                var deviceInfo = new DeviceInfo
                {
                    Id = trackingServiceId,
                    LicenseId = licenseId,
                    DataStreamerEndpoint = trackingServiceEndpoint,
                    DataStreamerPort = trackingServicePort,
                    ControlApiEndpoint = controlApiEndpoint,
                    ControlApiPort = controlApiPort,
                    FirstTimeSeen = DateTime.UtcNow,
                    LastSeen = null,
                };
                
                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("DataSourceAutoDiscoveryLocalListener: Tracking Service '{0}' found", trackingServiceId);
                }

                OnDeviceFound(deviceInfo);
            }

            // Data Source NAK Message (loopback)
            // Tracking Service send a Data Source Discovery Packet. This is the loopback NAK reply --> Ignore
            else if (!string.IsNullOrEmpty(returnData) && returnData.Substring(0, 3) == "NAK")
            {
                if (AutoDiscoveryLoopbackLogEnabled)
                {
                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("DataSourceAutoDiscoveryLocalListener: loopback Data Source Discovery Packet NAK received. IGNORE.");
                    }
                }
            }
        }

        private void UdpLocalListenerMessageReceivedTrackingServiceHandler(object sender, UdpMessageReceivedEventArgs e)
        {
            string returnData = Encoding.UTF8.GetString(e.Data, 0, e.Data.Length);

            // Data Source ACK Message
            // Tracking Service send a Data Source Discovery Packet. Here is the ACK response from the Data Source.
            if (e.Data.Length > 5 && returnData.Substring(0, 5) == "ACKDS")
            {
                string[] splitRcvd = returnData.Split(' ');

                string dataSourceId = splitRcvd[1];
                string dataSourceEndpoint = splitRcvd[2];
                int dataSourcePort = Convert.ToInt16(splitRcvd[3]);
                string controlApiEndpoint = splitRcvd[4];
                int controlApiPort = Convert.ToInt16(splitRcvd[5]);
                string licenseId = splitRcvd[6];

                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("TrackingServiceAutoDiscoveryLocalListener: ACKDS Response received. DataSource is '{0}' at {1}:{2} {3}:{4}", dataSourceId, dataSourceEndpoint, dataSourcePort, controlApiEndpoint, controlApiPort);
                }

                var newDevice = new DeviceInfo
                {
                    Id = dataSourceId,
                    LicenseId = licenseId,
                    DataStreamerEndpoint = dataSourceEndpoint,
                    DataStreamerPort = dataSourcePort,
                    ControlApiEndpoint = controlApiEndpoint,
                    ControlApiPort = controlApiPort,
                    FirstTimeSeen = DateTime.UtcNow,
                    LastSeen = null,
                };


                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("TrackingServiceAutoDiscoveryLocalListener: new Data Source found ({0})", dataSourceId);
                }

                OnDeviceFound(newDevice);
            }

            // Tracking Service ACK Message
            // A DataSource send a Tracking Service Discovery Packet. This is the loopback ACK + Local Tracking Service Network Info reply --> Ignore
            else if (e.Data.Length > 3 && returnData.Substring(0, 3) == "ACK")
            {
                if (AutoDiscoveryLoopbackLogEnabled)
                {
                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("TrackingServiceAutoDiscoveryLocalListener: loopback Tracking Service Discovery Packet ACK received. IGNORE.");
                    }
                }
            }

            // Tracking Service NAK Message
            // A DataSource send a Tracking Service Discovery Packet. This is the loopback NAK reply --> Ignore
            else if (!string.IsNullOrEmpty(returnData) && returnData.Substring(0, 3) == "NAK")
            {
                if (AutoDiscoveryLoopbackLogEnabled)
                {
                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("TrackingServiceAutoDiscoveryLocalListener: loopback Tracking Service Discovery Packet NAK received. IGNORE.");
                    }
                }
            }
            
        }
        #endregion
    }
}

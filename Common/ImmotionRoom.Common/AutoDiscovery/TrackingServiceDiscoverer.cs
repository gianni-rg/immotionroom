namespace ImmotionAR.ImmotionRoom.AutoDiscovery
{
    using System;
    using System.Threading.Tasks;
    using Helpers;
    using Logger;
    using Model;
    using Networking;
    using Networking.Interfaces;
#if !UNITY_5
    using Helpers.CrossPlatformSupport;
#endif

    public class TrackingServiceDiscoverer
    {
        #region Constants

        private const string AutoDiscoverySettingsPath = "Config";
        private const string AutoDiscoverySettingsFile = "AutoDiscoverySettings.dat";

        #endregion

        #region Events

        public event EventHandler<TrackingServiceDiscoveryCompletedEventArgs> DiscoveryCompleted;

        #endregion

        #region Private fields

        private readonly ILogger m_Logger;
        private readonly IUdpClientFactory m_UdpClientFactory;
        private readonly AutoDiscoverySettings m_Configuration;
        private AutoDiscoveryDiscoverer m_AutoDiscoveryDiscoverer;

        #endregion

        #region Constructor

        public TrackingServiceDiscoverer(AutoDiscoverySettings settings)
        {
            m_Logger = LoggerService.GetLogger<TrackingServiceDiscoverer>();
#if UNITY_5
            m_UdpClientFactory = new UdpClientFactory();
#else
            m_UdpClientFactory = PlatformAdapter.Resolve<IUdpClientFactory>();
#endif
            m_Configuration = settings;
        }

        #endregion

        #region Methods

        public void StartTrackingServiceDiscoveryAsync()
        {
            if (m_Logger != null && m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("StartTrackingServiceDiscoveryAsync() called");
            }

            Task.Factory.StartNew(() =>
            {
                // Impersonates a fake Data Source
                m_AutoDiscoveryDiscoverer = new AutoDiscoveryDiscoverer(DiscovererTypes.TrackingServiceDiscoverer, m_UdpClientFactory);

                m_AutoDiscoveryDiscoverer.LocalAddress = m_Configuration.LocalAddress;
                m_AutoDiscoveryDiscoverer.AutoDiscoveryMulticastAddress = m_Configuration.MulticastAddress;
                m_AutoDiscoveryDiscoverer.AutoDiscoveryMulticastPort = m_Configuration.MulticastPort;
                m_AutoDiscoveryDiscoverer.AutoDiscoveryLocalPort = m_Configuration.LocalPort;
                m_AutoDiscoveryDiscoverer.AutoDiscoveryPollingInterval = m_Configuration.PollingIntervalInSeconds*1000;
                m_AutoDiscoveryDiscoverer.AutoDiscoveryDuration = m_Configuration.DurationInSeconds*1000;
                m_AutoDiscoveryDiscoverer.AutoDiscoveryLoopbackLogEnabled = false;
                m_AutoDiscoveryDiscoverer.AutoDiscoveryListenerPollingTime = m_Configuration.ListenerIntervalInSeconds*1000;
                m_AutoDiscoveryDiscoverer.AutoDiscoveryUdpLocalClientTimeout = m_Configuration.UdpLocalClientTimeoutInSeconds*1000;

                m_AutoDiscoveryDiscoverer.DeviceFound += AutoDiscoveryDiscoverer_OnDeviceFound;
                m_AutoDiscoveryDiscoverer.DiscoveryCompleted += AutoDiscoveryDiscoverer_OnDiscoveryCompleted;

                m_AutoDiscoveryDiscoverer.StartAsync().Wait();

                if (m_Logger != null && m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("StartTrackingServiceDiscoveryAsync - AutoDiscoveryDiscoverer started");
                }
            });
        }

        public void StopTrackingServiceDiscovery()
        {
            if (m_Logger != null && m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("StopTrackingServiceDiscovery() called");
            }

            if (m_AutoDiscoveryDiscoverer != null)
            {
                m_AutoDiscoveryDiscoverer.DeviceFound -= AutoDiscoveryDiscoverer_OnDeviceFound;
                m_AutoDiscoveryDiscoverer.DiscoveryCompleted -= AutoDiscoveryDiscoverer_OnDiscoveryCompleted;

                m_AutoDiscoveryDiscoverer.Stop();
            }
        }

        #endregion

        #region Private methods

        private void AutoDiscoveryDiscoverer_OnDiscoveryCompleted(object sender, EventArgs eventArgs)
        {
            if (m_Logger != null && m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("AutoDiscoveryDiscoverer_OnDiscoveryCompleted event");
            }

            m_AutoDiscoveryDiscoverer.DeviceFound -= AutoDiscoveryDiscoverer_OnDeviceFound;
            m_AutoDiscoveryDiscoverer.DiscoveryCompleted -= AutoDiscoveryDiscoverer_OnDiscoveryCompleted;
            m_AutoDiscoveryDiscoverer.Stop();

            // No Tracking Service Found
            OnDiscoveryCompleted(new TrackingServiceDiscoveryResult());
        }

        private void AutoDiscoveryDiscoverer_OnDeviceFound(object sender, DeviceFoundEventArgs args)
        {
            if (m_Logger != null && m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("AutoDiscoveryDiscoverer_OnDeviceFound event");
            }

            var result = new TrackingServiceDiscoveryResult
            {
                Id = args.Info.Id,
                ControlApiEndpoint = args.Info.ControlApiEndpoint,
                ControlApiPort = args.Info.ControlApiPort,
                DataStreamerEndpoint = args.Info.DataStreamerEndpoint,
                DataStreamerPort = args.Info.DataStreamerPort
            };

            // If a Tracking Service is found, discovery process can be stopped
            m_AutoDiscoveryDiscoverer.Stop();

            OnDiscoveryCompleted(result);
        }

        private void OnDiscoveryCompleted(TrackingServiceDiscoveryResult result)
        {
            var localHandler = DiscoveryCompleted;
            if (localHandler != null)
            {
                localHandler(this, new TrackingServiceDiscoveryCompletedEventArgs(result));
            }
        }

        #endregion
    }
}

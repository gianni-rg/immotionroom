namespace ImmotionAR.ImmotionRoom.TrackingService.Services
{
    using System;
    using System.Threading.Tasks;
    using AutoDiscovery;
    using Interfaces;
    using Logger;
    using Model;
    using Networking.Interfaces;

    // See: http://www.nullskull.com/a/1551/clientserver-autodiscovery-in-c-and-udp-sockets.aspx
    public class NetworkDiscoveryService : BaseService, INetworkDiscoveryService
    {
        #region Events

        public event EventHandler<DataSourceFoundEventArgs> DataSourceFound;
        public event EventHandler DiscoveryCompleted;

        #endregion

        #region Private fields

        private readonly IConfigurationService m_ConfigurationService;
        private readonly AutoDiscoveryListener m_AutoDiscoveryListener;
        private readonly AutoDiscoveryDiscoverer m_AutoDiscoveryDiscoverer;

        #endregion

        #region Constructor

        public NetworkDiscoveryService(IConfigurationService configurationService, IUdpClientFactory udpClientFactory) : base(LoggerService.GetLogger<NetworkDiscoveryService>())
        {
            Helpers.Requires.NotNull(configurationService, "configurationService");
            Helpers.Requires.NotNull(udpClientFactory, "udpClientFactory");

            m_ConfigurationService = configurationService;
            
            m_AutoDiscoveryListener = new AutoDiscoveryListener(ListenerTypes.TrackingServiceListener, udpClientFactory);
            m_AutoDiscoveryDiscoverer = new AutoDiscoveryDiscoverer(DiscovererTypes.DataSourceDiscoverer, udpClientFactory);
        }

        #endregion

        #region Overridden Methods

        public override async Task StartAsync()
        {
            // Configuration
            m_AutoDiscoveryListener.InstanceId = m_ConfigurationService.CurrentConfiguration.InstanceId;
            m_AutoDiscoveryListener.LocalAddress = m_ConfigurationService.CurrentConfiguration.LocalEndpoint;
            m_AutoDiscoveryListener.AutoDiscoveryMulticastAddress = m_ConfigurationService.CurrentConfiguration.AutoDiscovery.MulticastAddress;
            m_AutoDiscoveryListener.AutoDiscoveryMulticastPort = m_ConfigurationService.CurrentConfiguration.AutoDiscovery.MulticastPort;
            m_AutoDiscoveryListener.AutoDiscoveryLocalPort = m_ConfigurationService.CurrentConfiguration.AutoDiscovery.LocalPort;
            m_AutoDiscoveryListener.AutoDiscoveryUdpLocalClientTimeout = m_ConfigurationService.CurrentConfiguration.AutoDiscovery.UdpLocalClientTimeoutInSeconds * 1000;
            m_AutoDiscoveryListener.DataStreamerPort = m_ConfigurationService.CurrentConfiguration.DataStreamerPort;
            m_AutoDiscoveryListener.DataStreamerEndpoint = m_ConfigurationService.CurrentConfiguration.DataStreamerEndpoint;
            m_AutoDiscoveryListener.AutoDiscoveryLoopbackLogEnabled = m_ConfigurationService.CurrentConfiguration.AutoDiscovery.LoopbackLogEnabled;
            m_AutoDiscoveryListener.AutoDiscoveryListenerPollingTime = m_ConfigurationService.CurrentConfiguration.AutoDiscovery.ListenerIntervalInSeconds*1000;
            m_AutoDiscoveryListener.ControlApiEndpoint = m_ConfigurationService.CurrentConfiguration.ControlApiEndpoint;
            m_AutoDiscoveryListener.ControlApiPort = m_ConfigurationService.CurrentConfiguration.ControlApiPort;

            m_AutoDiscoveryDiscoverer.LocalAddress = m_ConfigurationService.CurrentConfiguration.LocalEndpoint;
            m_AutoDiscoveryDiscoverer.AutoDiscoveryMulticastAddress = m_ConfigurationService.CurrentConfiguration.AutoDiscovery.MulticastAddress;
            m_AutoDiscoveryDiscoverer.AutoDiscoveryMulticastPort = m_ConfigurationService.CurrentConfiguration.AutoDiscovery.MulticastPort;
            m_AutoDiscoveryDiscoverer.AutoDiscoveryLocalPort = m_ConfigurationService.CurrentConfiguration.AutoDiscovery.LocalPort;
            m_AutoDiscoveryDiscoverer.AutoDiscoveryPollingInterval = m_ConfigurationService.CurrentConfiguration.AutoDiscovery.PollingIntervalInSeconds*1000;
            m_AutoDiscoveryDiscoverer.AutoDiscoveryDuration = m_ConfigurationService.CurrentConfiguration.AutoDiscovery.DurationInSeconds*1000;
            m_AutoDiscoveryDiscoverer.AutoDiscoveryListenerPollingTime = m_ConfigurationService.CurrentConfiguration.AutoDiscovery.ListenerIntervalInSeconds * 1000;
            m_AutoDiscoveryDiscoverer.AutoDiscoveryLoopbackLogEnabled = m_ConfigurationService.CurrentConfiguration.AutoDiscovery.LoopbackLogEnabled;
            m_AutoDiscoveryDiscoverer.AutoDiscoveryUdpLocalClientTimeout = m_ConfigurationService.CurrentConfiguration.AutoDiscovery.UdpLocalClientTimeoutInSeconds * 1000;

            m_AutoDiscoveryDiscoverer.DiscoveryCompleted += AutoDiscoveryDiscoverer_DiscoveryCompleted;
            m_AutoDiscoveryDiscoverer.DeviceFound += AutoDiscoveryListener_DeviceFound;

            await m_AutoDiscoveryListener.StartAsync().ConfigureAwait(false);
        }


        public override void Stop()
        {
            m_AutoDiscoveryDiscoverer.DeviceFound -= AutoDiscoveryListener_DeviceFound;
            m_AutoDiscoveryDiscoverer.DiscoveryCompleted -= AutoDiscoveryDiscoverer_DiscoveryCompleted;

            m_AutoDiscoveryListener.Stop();
            m_AutoDiscoveryDiscoverer.Stop();
            
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("NetworkDiscoveryService Stopped");
            }
        }


        public Task StartDiscoveryAsync()
        {
            return m_AutoDiscoveryDiscoverer.StartAsync();
        }

        public void StopDiscovery()
        {
            m_AutoDiscoveryDiscoverer.Stop();
        }

        #endregion

        #region Private methods

        private void AutoDiscoveryListener_DeviceFound(object sender, DeviceFoundEventArgs e)
        {
            var dataSource = new DataSourceInfo
            {
                Id = e.Info.Id,
                DataStreamEndpoint = e.Info.DataStreamerEndpoint,
                DataStreamPort = e.Info.DataStreamerPort,
                ControlApiEndpoint = e.Info.ControlApiEndpoint,
                ControlApiPort = e.Info.ControlApiPort,
                FirstTimeSeen = e.Info.FirstTimeSeen,
                LastSeen = e.Info.LastSeen,
                IsReachable = e.Info.IsReachable,
            };

            OnDataSourceFound(dataSource, e.Info.LicenseId);
        }

        private void AutoDiscoveryDiscoverer_DiscoveryCompleted(object sender, EventArgs e)
        {
            OnDiscoveryCompleted();
        }

        private void OnDataSourceFound(DataSourceInfo dataSource, string licenseId)
        {
            EventHandler<DataSourceFoundEventArgs> localHandler = DataSourceFound;
            if (localHandler != null)
            {
                localHandler(this, new DataSourceFoundEventArgs(dataSource, licenseId));
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
        #endregion
    }
}

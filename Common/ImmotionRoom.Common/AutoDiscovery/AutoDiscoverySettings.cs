namespace ImmotionAR.ImmotionRoom.AutoDiscovery
{
    public class AutoDiscoverySettings
    {
        public static readonly AutoDiscoverySettings Default = DefaultAutoDiscoverySettings();
        public string LocalAddress { get; set; }
        public int LocalPort { get; set; }
        public string MulticastAddress { get; set; }
        public int MulticastPort { get; set; }
        public bool LoopbackLogEnabled { get; set; }
        public int ListenerIntervalInSeconds { get; set; }
        public int PollingIntervalInSeconds { get; set; }
        public int DurationInSeconds { get; set; }
        public int UdpLocalClientTimeoutInSeconds { get; set; }
        public int RepeatIntervalInSeconds { get; set; }
        public int ReachableTimeoutInSeconds { get; set; }
        public int CompletionDelayInSeconds { get; set; }
        
        private static AutoDiscoverySettings DefaultAutoDiscoverySettings()
        {
            var defaultConfig = new AutoDiscoverySettings();

            defaultConfig.MulticastAddress = AutoDiscoveryDefaultSettings.AutoDiscoveryMulticastAddress;
            defaultConfig.MulticastPort = AutoDiscoveryDefaultSettings.AutoDiscoveryMulticastPort;
            defaultConfig.DurationInSeconds = AutoDiscoveryDefaultSettings.AutoDiscoveryDurationInSeconds;
            defaultConfig.PollingIntervalInSeconds = AutoDiscoveryDefaultSettings.AutoDiscoveryPollingIntervalInSeconds;
            defaultConfig.ListenerIntervalInSeconds = AutoDiscoveryDefaultSettings.AutoDiscoveryListenerIntervalInSeconds;
            defaultConfig.UdpLocalClientTimeoutInSeconds = AutoDiscoveryDefaultSettings.AutoDiscoveryUdpLocalClientTimeoutInSeconds;
            defaultConfig.LoopbackLogEnabled = AutoDiscoveryDefaultSettings.AutoDiscoveryLoopbackLogEnabled;
            defaultConfig.ReachableTimeoutInSeconds = AutoDiscoveryDefaultSettings.AutoDiscoveryReachableTimeoutInSeconds;
            defaultConfig.RepeatIntervalInSeconds = AutoDiscoveryDefaultSettings.AutoDiscoveryRepeatIntervalInSeconds;
            defaultConfig.CompletionDelayInSeconds = AutoDiscoveryDefaultSettings.AutoDiscoveryCompletionDelayInSeconds;
            
            // Remember to set local address/port in Default for TrackingService OR DataSourceService
            // defaultConfig.LocalAddress = NetworkTools.GetLocalIpAddress();
            // defaultConfig.LocalPort = AutoDiscoveryDefaultSettings.DataSourceAutoDiscoveryLocalPort;
            // defaultConfig.LocalPort = AutoDiscoveryDefaultSettings.TrackingServiceAutoDiscoveryLocalPort;

            return defaultConfig;
        }
    }


}

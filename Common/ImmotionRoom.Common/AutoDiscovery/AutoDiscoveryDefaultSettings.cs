namespace ImmotionAR.ImmotionRoom.AutoDiscovery
{
    public static class AutoDiscoveryDefaultSettings
    {
        public const string AutoDiscoveryMulticastAddress = "224.242.42.0";

        public const int AutoDiscoveryMulticastPort = 18500;

        public const int TrackingServiceAutoDiscoveryLocalPort = 18501;

        public const int DataSourceAutoDiscoveryLocalPort = 18502;

        public const bool AutoDiscoveryLoopbackLogEnabled = false;

        public const int AutoDiscoveryListenerIntervalInSeconds = 1;

        public const int AutoDiscoveryPollingIntervalInSeconds = 5;
        
        public const int AutoDiscoveryDurationInSeconds = 10;

        public const int AutoDiscoveryUdpLocalClientTimeoutInSeconds = 6;

        public const int AutoDiscoveryReachableTimeoutInSeconds = 6;

        public const int AutoDiscoveryRepeatIntervalInSeconds = 10;

        public const int AutoDiscoveryCompletionDelayInSeconds = 6;
    }
}

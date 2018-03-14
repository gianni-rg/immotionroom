namespace ImmotionAR.ImmotionRoom.DataSourceService.Model
{
    using AutoDiscovery;

    public class DataSourceConfiguration
    {
        public static readonly DataSourceConfiguration Default = DefaultDataSourceConfiguration();

        public string InstanceId { get; set; }
        public string LocalEndpoint { get; set; }
        public string SettingsStorageLocation { get; set; }

        public AutoDiscoverySettings AutoDiscovery { get; set; }

        public string DataStreamerEndpoint { get; set; }
        public int DataStreamerPort { get; set; }
        
        public string ControlApiEndpoint { get; set; }
        public int ControlApiPort { get; set; }
        public int TrackingServiceMonitorIntervalInSeconds { get; set; }


        public bool SkeletonDataRecorderEnabled { get; set; }
        public string DataRecorderSessionPath { get; set; }

        public bool ColorStreamRecorderEnabled { get; set; }
        public int ColorStreamRecorderFps { get; set; }
        public int ColorStreamRecorderWidth { get; set; }
        public int ColorStreamRecorderHeight { get; set; }
        public bool DepthStreamRecorderEnabled { get; set; }
        public int DepthStreamRecorderWidth { get; set; }
        public int DepthStreamRecorderHeight { get; set; }
        public int DepthStreamRecorderFps { get; set; }

        public int ClientListenerTimeoutInMilliseconds { get; set; }
        public int ClientListenerStartMaxRetries { get; set; }
        public int ClientListenerStartRetryIntervalInMilliseconds { get; set; }
        public int ReceivedCommandsCleanerIntervalInMinutes { get; set; }
        public int MaxMessageAliveTimeInSeconds { get; set; }
        public int ReceivedCommandsPollingIntervalInMilliseconds { get; set; }
        public int AutoDiscoveryDelayInMilliseconds { get; set; }
        
        public int SystemRebootDelayInMilliseconds { get; set; }
        public int GetLocalIpRetries { get; set; }
        public int GetLocalIpIntervalInSeconds { get; set; }
        public int NetworkAdapterIndex { get; set; }

        private static DataSourceConfiguration DefaultDataSourceConfiguration()
        {
            var defaultConfig = new DataSourceConfiguration();

            defaultConfig.AutoDiscovery = AutoDiscoverySettings.Default;
            defaultConfig.AutoDiscovery.LocalPort = AutoDiscoveryDefaultSettings.DataSourceAutoDiscoveryLocalPort;

            defaultConfig.DataStreamerPort = DataSourceDefaultSettings.DataStreamerPort;
            defaultConfig.ControlApiPort = DataSourceDefaultSettings.ControlApiPort;
            defaultConfig.TrackingServiceMonitorIntervalInSeconds = DataSourceDefaultSettings.TrackingServiceMonitorIntervalInSeconds;
            defaultConfig.SkeletonDataRecorderEnabled = DataSourceDefaultSettings.SkeletonDataRecorderEnabled;

            defaultConfig.ColorStreamRecorderFps = DataSourceDefaultSettings.ColorStreamRecorderFps;
            defaultConfig.ColorStreamRecorderHeight = DataSourceDefaultSettings.ColorStreamRecorderHeight;
            defaultConfig.ColorStreamRecorderWidth = DataSourceDefaultSettings.ColorStreamRecorderWidth;
            defaultConfig.ColorStreamRecorderEnabled = DataSourceDefaultSettings.ColorStreamRecorderEnabled;
            defaultConfig.DepthStreamRecorderEnabled = DataSourceDefaultSettings.DepthStreamRecorderEnabled;
            defaultConfig.DepthStreamRecorderFps = DataSourceDefaultSettings.DepthStreamRecorderFps;
            defaultConfig.DepthStreamRecorderHeight = DataSourceDefaultSettings.DepthStreamRecorderHeight;
            defaultConfig.DepthStreamRecorderWidth = DataSourceDefaultSettings.DepthStreamRecorderWidth;

            defaultConfig.ClientListenerTimeoutInMilliseconds = DataSourceDefaultSettings.ClientListenerTimeoutInMilliseconds;
            defaultConfig.ClientListenerStartMaxRetries = DataSourceDefaultSettings.ClientListenerStartMaxRetries;
            defaultConfig.ClientListenerStartRetryIntervalInMilliseconds = DataSourceDefaultSettings.ClientListenerStartRetryIntervalInMilliseconds;
            
            defaultConfig.ReceivedCommandsCleanerIntervalInMinutes = DataSourceDefaultSettings.ReceivedCommandsCleanerIntervalInMinutes;
            defaultConfig.MaxMessageAliveTimeInSeconds = DataSourceDefaultSettings.MaxMessageAliveTimeInSeconds;
            defaultConfig.ReceivedCommandsPollingIntervalInMilliseconds = DataSourceDefaultSettings.ReceivedCommandsPollingIntervalInMilliseconds;
            defaultConfig.SystemRebootDelayInMilliseconds = DataSourceDefaultSettings.SystemRebootDelayInMilliseconds;
            defaultConfig.AutoDiscoveryDelayInMilliseconds = DataSourceDefaultSettings.AutoDiscoveryDelayInMilliseconds;
            defaultConfig.GetLocalIpRetries = DataSourceDefaultSettings.GetLocalIpRetries;
            defaultConfig.GetLocalIpIntervalInSeconds = DataSourceDefaultSettings.GetLocalIpIntervalInSeconds;
            defaultConfig.NetworkAdapterIndex = DataSourceDefaultSettings.NetworkAdapterIndex;

            // Remember to set 
            // defaultConfig.InstanceId;
            // defaultConfig.LocalEndpoint;
            // defaultConfig.DataStreamerEndpoint
            // defaultConfig.ControlApiEndpoint
            // defaultConfig.DataRecorderSessionPath
            // defaultConfig.SettingsStorageLocation;

            return defaultConfig;
        }
    }
}

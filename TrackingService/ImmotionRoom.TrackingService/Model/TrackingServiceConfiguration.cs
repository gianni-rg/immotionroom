namespace ImmotionAR.ImmotionRoom.TrackingService.Model
{
    using AutoDiscovery;
    using TrackingEngine.Model;

    public class TrackingServiceConfiguration
    {
        public static TrackingServiceConfiguration Default
        {
            get { return DefaultTrackingServiceConfiguration(); }
        }

        public static SceneDescriptor DefaultScene
        {
            get { return DefaultTrackingServiceSceneConfiguration(); }
        }

        public string InstanceId { get; set; }
        public string LocalEndpoint { get; set; }

        public string SettingsStorageLocation { get; set; }

        public AutoDiscoverySettings AutoDiscovery { get; set; }

        public string DataStreamerEndpoint { get; set; }
        public int DataStreamerPort { get; set; }
        public int DataStreamerBacklog { get; set; }

        public int DataStreamerClientTimeoutInMilliseconds { get; set; }
        public int DataStreamerListenerMaxRetries { get; set; }
        public int DataStreamerListenerRetryIntervalInMilliseconds { get; set; }

        public int DataSourceReachableTimeoutInSeconds { get; set; }
        public int DataSourceMonitorIntervalInSeconds { get; set; }
        public int DataSourceApiMonitorIntervalInSeconds { get; set; }
        public int DataSourceUnreachableMaxRetries { get; set; }
        public int DataSourceUnreachableRetryIntervalInMilliseconds { get; set; }
        public int DataSourceAliveTimeInSeconds { get; set; }
        public string ControlApiEndpoint { get; set; }
        public int ControlApiPort { get; set; }

        public int AutomaticTrackingStopTimeoutInSeconds { get; set; }

        public int ActiveClientsMonitorIntervalInSeconds { get; set; }

        public int UpdateLoopFrameRate { get; set; }

        public int MinDataSourcesForPlay { get; set; }

        public int SystemRebootDelayInMilliseconds { get; set; }
        public int ReceivedCommandsCleanerIntervalInMinutes { get; set; }
        public int MaxMessageAliveTimeInSeconds { get; set; }
        public int ReceivedCommandsPollingIntervalInMilliseconds { get; set; }
        public int GetLocalIpRetries { get; set; }
        public int GetLocalIpIntervalInSeconds { get; set; }

        public SceneDescriptor Scene { get; set; }
        public int NetworkAdapterIndex { get; set; }

        private static TrackingServiceConfiguration DefaultTrackingServiceConfiguration()
        {
            var defaultConfig = new TrackingServiceConfiguration();

            defaultConfig.AutoDiscovery = AutoDiscoverySettings.Default;
            defaultConfig.AutoDiscovery.LocalPort = AutoDiscoveryDefaultSettings.TrackingServiceAutoDiscoveryLocalPort;

            defaultConfig.DataStreamerPort = TrackingServiceDefaultSettings.DataStreamerPort;
            defaultConfig.DataStreamerBacklog = TrackingServiceDefaultSettings.DataStreamerBacklog;
            defaultConfig.DataStreamerClientTimeoutInMilliseconds = TrackingServiceDefaultSettings.DataStreamerClientTimeoutInMilliseconds;
            defaultConfig.DataStreamerListenerMaxRetries = TrackingServiceDefaultSettings.DataStreamerListenerMaxRetries;
            defaultConfig.DataStreamerListenerRetryIntervalInMilliseconds = TrackingServiceDefaultSettings.DataStreamerListenerRetryIntervalInMilliseconds;

            defaultConfig.ControlApiPort = TrackingServiceDefaultSettings.ControlApiPort;
            defaultConfig.DataSourceReachableTimeoutInSeconds = TrackingServiceDefaultSettings.DataSourceReachableTimeoutInSeconds;
            defaultConfig.DataSourceMonitorIntervalInSeconds = TrackingServiceDefaultSettings.DataSourceMonitorIntervalInSeconds;
            defaultConfig.DataSourceApiMonitorIntervalInSeconds = TrackingServiceDefaultSettings.DataSourceApiMonitorIntervalInSeconds;
            defaultConfig.DataSourceUnreachableMaxRetries = TrackingServiceDefaultSettings.DataSourceUnreachableMaxRetries;
            defaultConfig.DataSourceUnreachableRetryIntervalInMilliseconds = TrackingServiceDefaultSettings.DataSourceUnreachableRetryIntervalInMilliseconds;
            defaultConfig.DataSourceAliveTimeInSeconds = TrackingServiceDefaultSettings.DataSourceAliveTimeInSeconds;
            defaultConfig.ActiveClientsMonitorIntervalInSeconds = TrackingServiceDefaultSettings.ActiveClientsMonitorIntervalInSeconds;
            defaultConfig.AutomaticTrackingStopTimeoutInSeconds = TrackingServiceDefaultSettings.AutomaticTrackingStopTimeoutInSeconds;
            defaultConfig.UpdateLoopFrameRate = TrackingServiceDefaultSettings.UpdateLoopFrameRate;
            defaultConfig.MinDataSourcesForPlay = TrackingServiceDefaultSettings.MinDataSourcesForPlay;

            defaultConfig.SystemRebootDelayInMilliseconds = TrackingServiceDefaultSettings.SystemRebootDelayInMilliseconds;
            defaultConfig.ReceivedCommandsCleanerIntervalInMinutes = TrackingServiceDefaultSettings.ReceivedCommandsCleanerIntervalInMinutes;
            defaultConfig.MaxMessageAliveTimeInSeconds = TrackingServiceDefaultSettings.MaxMessageAliveTimeInSeconds;
            defaultConfig.ReceivedCommandsPollingIntervalInMilliseconds = TrackingServiceDefaultSettings.ReceivedCommandsPollingIntervalInMilliseconds;
            defaultConfig.GetLocalIpRetries = TrackingServiceDefaultSettings.GetLocalIpRetries;
            defaultConfig.GetLocalIpIntervalInSeconds = TrackingServiceDefaultSettings.GetLocalIpIntervalInSeconds;
            defaultConfig.NetworkAdapterIndex = TrackingServiceDefaultSettings.NetworkAdapterIndex;

            defaultConfig.Scene = DefaultTrackingServiceSceneConfiguration();

            // Remember to set InstanceId, LocalEndpoint, StorageLocation
            // defaultConfig.InstanceId;
            // defaultConfig.LocalEndpoint;
            // defaultConfig.SettingsStorageLocation;

            return defaultConfig;
        }

        private static SceneDescriptor DefaultTrackingServiceSceneConfiguration()
        {
            var defaultScene = new SceneDescriptor();
            defaultScene.FloorClipPlane = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

            defaultScene.StageArea = new Boundaries
            {
                Center = new Vector3(TrackingServiceDefaultSettings.StageAreaCenterX, TrackingServiceDefaultSettings.StageAreaCenterY, TrackingServiceDefaultSettings.StageAreaCenterZ),
                Size = new Vector3(TrackingServiceDefaultSettings.StageAreaSizeX, TrackingServiceDefaultSettings.StageAreaSizeY, TrackingServiceDefaultSettings.StageAreaSizeZ)
            };

            defaultScene.GameArea = new Boundaries
            {
                Center = new Vector3(TrackingServiceDefaultSettings.GameAreaCenterX, TrackingServiceDefaultSettings.GameAreaCenterY, TrackingServiceDefaultSettings.GameAreaCenterZ),
                Size = new Vector3(TrackingServiceDefaultSettings.GameAreaSizeX, TrackingServiceDefaultSettings.GameAreaSizeY, TrackingServiceDefaultSettings.GameAreaSizeZ)
            };

            defaultScene.GameAreaInnerLimits = new Vector3(TrackingServiceDefaultSettings.GameAreaInnerLimitsX, TrackingServiceDefaultSettings.GameAreaInnerLimitsY, TrackingServiceDefaultSettings.GameAreaInnerLimitsZ);

            return defaultScene;
        }
    }
}
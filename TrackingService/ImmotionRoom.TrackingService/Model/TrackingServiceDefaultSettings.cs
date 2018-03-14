namespace ImmotionAR.ImmotionRoom.TrackingService.Model
{
    public static class TrackingServiceDefaultSettings
    {
        public const int DataStreamerPort = 8888;
        public const int DataStreamerBacklog = 10;
        public const int DataStreamerClientTimeoutInMilliseconds = 1000;
        public const int DataStreamerListenerMaxRetries = 3;
        public const int DataStreamerListenerRetryIntervalInMilliseconds = 2000;

        public const int ControlApiPort = 9090;

        public const int DataSourceMonitorIntervalInSeconds = 5;
        public const int DataSourceApiMonitorIntervalInSeconds = 15;

        public const int DataSourceReachableTimeoutInSeconds = 2;
        public const int DataSourceUnreachableMaxRetries = 1;
        public const int DataSourceUnreachableRetryIntervalInMilliseconds = 3000;
        public const int DataSourceAliveTimeInSeconds = 5;

        public const int ActiveClientsMonitorIntervalInSeconds = 15;

        public const int AutomaticTrackingStopTimeoutInSeconds = 1;

        public const int UpdateLoopFrameRate = 90;

        public const int MinDataSourcesForPlay = 3;

        public const int SystemRebootDelayInMilliseconds = 5000;

        public const int ReceivedCommandsCleanerIntervalInMinutes = 1;
        public const int MaxMessageAliveTimeInSeconds = 15;
        public const int ReceivedCommandsPollingIntervalInMilliseconds = 1000;

        public const int GetLocalIpRetries = 3;
        public const int GetLocalIpIntervalInSeconds = 60;
        public const int NetworkAdapterIndex = 0;

        public const float StageAreaCenterX = 0.0f;
        public const float StageAreaCenterY = 0.0f;
        public const float StageAreaCenterZ = 0.0f;
        public const float StageAreaSizeX = 0.0f;
        public const float StageAreaSizeY = 0.0f;
        public const float StageAreaSizeZ = 0.0f;

        public const float GameAreaCenterX = 0.0f;
        public const float GameAreaCenterY = 0.0f;
        public const float GameAreaCenterZ = 0.0f;

        public const float GameAreaSizeX = 0.0f;
        public const float GameAreaSizeY = 0.0f;
        public const float GameAreaSizeZ = 0.0f;

        public const float GameAreaInnerLimitsX = 0.0f;
        public const float GameAreaInnerLimitsY = 0.0f;
        public const float GameAreaInnerLimitsZ = 0.0f;
    }
}

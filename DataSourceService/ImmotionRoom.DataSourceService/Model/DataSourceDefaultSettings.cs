namespace ImmotionAR.ImmotionRoom.DataSourceService.Model
{
    public static class DataSourceDefaultSettings
    {
        public const int DataStreamerPort = 5555;
        
        public const int ControlApiPort = 9091;

        public const int TrackingServiceMonitorIntervalInSeconds = 15;

        public const int AutoDiscoveryRepeatIntervalInSeconds = 60;

        public const bool SkeletonDataRecorderEnabled = false;

        public const bool ColorStreamRecorderEnabled = false;
        public const int ColorStreamRecorderFps = 30;
        public const int ColorStreamRecorderWidth = 640;
        public const int ColorStreamRecorderHeight = 480;

        public const bool DepthStreamRecorderEnabled = false;
        public const int DepthStreamRecorderFps = 30;
        public const int DepthStreamRecorderWidth = 640;
        public const int DepthStreamRecorderHeight = 480;

        public const string DataRecorderSessionPath = "";

        public const int ClientListenerTimeoutInMilliseconds = 1000;
        public const int ClientListenerStartMaxRetries = 3;
        public const int ClientListenerStartRetryIntervalInMilliseconds = 2000;
        
        public const int ReceivedCommandsCleanerIntervalInMinutes = 1;
        public const int MaxMessageAliveTimeInSeconds = 15;
        public const int ReceivedCommandsPollingIntervalInMilliseconds = 1000;

        public const int SystemRebootDelayInMilliseconds = 5000;
        public const int AutoDiscoveryDelayInMilliseconds = 2000;

        public const int GetLocalIpRetries = 3;
        public const int GetLocalIpIntervalInSeconds = 60;
        public const int NetworkAdapterIndex = 0;
    }
}

namespace ImmotionAR.ImmotionRoom.TrackingService.Model
{
    public enum TrackingServiceState
    {
        Unknown = 0,
        AutoDiscovery = 1,
        Idle = 2,
        Calibration = 3,
        Running = 4,
        Starting = 5,
        Stopping = 6,
        Error = 7,
        DiagnosticMode = 8,
        Warning = 9,
    }
}

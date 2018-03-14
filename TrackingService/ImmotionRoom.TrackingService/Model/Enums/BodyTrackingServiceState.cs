namespace ImmotionAR.ImmotionRoom.TrackingService.Model
{
    public enum BodyTrackingServiceState
    {
        Undefined = 0,
        Idle = 1,
        Calibration = 2,
        Tracking = 3,
        Error = 4,
        Diagnostic = 5,
    }
}
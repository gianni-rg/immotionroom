namespace ImmotionAR.ImmotionRoom.TrackingService.Model
{
    public enum CommandType
    {
        Undefined = 0,
        EnableAutoDiscovery = 1,
        ServiceStatus = 2,
        StartTracking = 3,
        //StartCalibration = 4,
        StopTracking = 5,
        //StopCalibration = 6,
        ExecuteCalibrationStep = 7,
        SetMasterDataSource = 8,
        StartDiagnosticMode = 9,
        StopDiagnosticMode = 10,
        SystemReboot = 11,
        SetSceneDescriptor = 12,
    }
}
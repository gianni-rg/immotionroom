namespace ImmotionAR.ImmotionRoom.TrackingService.ControlClient.Model
{
    public enum TrackingServiceCalibrationSteps
    {
        Start = 0,
        StartCalibrateDataSourceWithMaster = 1,
        StopCalibrateDataSourceWithMaster = 2,
        StartCalibrateMaster = 3,
        StopCalibrateMaster = 4,
        End = 5,
    }
}
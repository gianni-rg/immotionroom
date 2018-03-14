namespace ImmotionAR.ImmotionRoom.TrackingService.Model
{
    public class ExecuteCalibrationStepEventArgs : CommandRequestEventArgs
    {
        public CalibrationParameters Parameters { get; set; }
    }
}

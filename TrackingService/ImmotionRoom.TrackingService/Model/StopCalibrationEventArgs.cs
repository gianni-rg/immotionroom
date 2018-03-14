namespace ImmotionAR.ImmotionRoom.TrackingService.Model
{
    public class StartCalibrationEventArgs : CommandRequestEventArgs
    {
        public CalibrationParameters Parameters { get; set; }
    }
}

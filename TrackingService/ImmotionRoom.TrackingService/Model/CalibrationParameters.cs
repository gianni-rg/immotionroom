namespace ImmotionAR.ImmotionRoom.TrackingService.Model
{
    public class CalibrationParameters
    {
        public TrackingServiceCalibrationSteps Step { get; set; }

        public float AdditionalMasterYRotation { get; set; }
        public float CalibratingUserHeight { get; set; }
        public bool CalibrateSlavesUsingCentroids { get; set; }
        public int LastButNthValidMatrix { get; set; }


        public string DataSource1 { get; set; }
        public string DataSource2 { get; set; }
    }
}
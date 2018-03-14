namespace ImmotionAR.ImmotionRoom.TrackingService.Model
{
    using System.Collections.Generic;
    using TrackingEngine.Model;
    using TrackingEngine.Walking;

    public class TrackingSessionConfiguration
    {
        public static readonly TrackingSessionConfiguration Default = DefaultTrackingSessionConfiguration();
        
        public WalkingDetectionConfiguration WalkingDetection { get; set; }
        public CalibrationParameters Calibration { get; set; }

        public TrackingSessionDataSourceConfiguration DataSourceTrackingSettings { get; set; }

        private static TrackingSessionConfiguration DefaultTrackingSessionConfiguration()
        {
            var defaultConfig = new TrackingSessionConfiguration();

            defaultConfig.WalkingDetection = new WalkingDetectionConfiguration
            {
                Enabled = TrackingSessionDefaultConfiguration.WalkingDetectionEnabled,
                WalkingDetector = PlayerWalkingDetectorTypes.KnaivePlayerWalkingDetector,
                Parameters = new Dictionary<string, string>(),
            };
            
            defaultConfig.WalkingDetection.Parameters[KnaivePlayerWalkingDetectorSettings.Knee_EstimatedFrameRate_Key] = TrackingServiceDefaultSettings.UpdateLoopFrameRate.ToString();

            defaultConfig.DataSourceTrackingSettings = new TrackingSessionDataSourceConfiguration()
            {
                BodyClippingEdgesEnabled = TrackingSessionDefaultConfiguration.BodyClippingEdgesEnabled,
                TrackJointRotation = TrackingSessionDefaultConfiguration.TrackJointRotation,
                HandsStatusEnabled = TrackingSessionDefaultConfiguration.HandsStatusEnabled,
            };

            return defaultConfig;
        }
    }
}

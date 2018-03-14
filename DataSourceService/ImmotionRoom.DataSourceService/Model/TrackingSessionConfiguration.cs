namespace ImmotionAR.ImmotionRoom.DataSourceService.Model
{
    public class TrackingSessionConfiguration
    {
        public static readonly TrackingSessionConfiguration Default = DefaultTrackingSessionConfiguration();

        public bool BodyClippingEdgesEnabled { get; set; }
        public bool TrackHandsStatus { get; set; }
        public bool TrackJointRotation { get; set; }

        private static TrackingSessionConfiguration DefaultTrackingSessionConfiguration()
        {
            var defaultConfig = new TrackingSessionConfiguration();

            defaultConfig.BodyClippingEdgesEnabled = TrackingSessionDefaultConfiguration.BodyClippingEdgesEnabled;
            defaultConfig.TrackHandsStatus = TrackingSessionDefaultConfiguration.TrackHandsStatus;
            defaultConfig.TrackJointRotation = TrackingSessionDefaultConfiguration.TrackJointRotation;

            return defaultConfig;
        }
    }
}

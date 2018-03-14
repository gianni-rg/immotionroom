namespace ImmotionAR.ImmotionRoom.TrackingService.ControlClient.Model
{
    using DataClient.Model;

    public class TrackingServiceSceneDescriptor
    {

        /// <summary>
        ///     Returns the floor plane, as detected by the master Data Source.
        ///     X,Y,Z represents the normal to the plane, while W is the height of the sensor in meters.
        ///     The plane is valid only when a Tracking Session is running. Otherwise, it will be all zeros.
        /// </summary>
        public TrackingServiceVector4 FloorClipPlane { get; set; }

        /// <summary>
        ///     The room center and size in meters.
        /// </summary>
        public TrackingServiceSceneBoundaries StageArea { get; set; }

        /// <summary>
        ///     The game area center and size in meters.
        /// </summary>
        public TrackingServiceSceneBoundaries GameArea { get; set; }

        /// <summary>
        ///     The game area inner limits (to start showing warning arrows)
        /// </summary>
        public TrackingServiceVector3 GameAreaInnerLimits { get; set; }

        public TrackingServiceSceneDescriptor()
        {
            FloorClipPlane = new TrackingServiceVector4();
            StageArea = new TrackingServiceSceneBoundaries();
            GameArea = new TrackingServiceSceneBoundaries();
            GameAreaInnerLimits = new TrackingServiceVector3();
        }
    }
}

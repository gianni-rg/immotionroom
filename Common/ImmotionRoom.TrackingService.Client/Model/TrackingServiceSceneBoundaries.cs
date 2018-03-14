namespace ImmotionAR.ImmotionRoom.TrackingService.ControlClient.Model
{
    using DataClient.Model;

    public class TrackingServiceSceneBoundaries
    {
        public TrackingServiceVector3 Center { get; set; }
        public TrackingServiceVector3 Size { get; set; }

        public TrackingServiceSceneBoundaries()
        {
            Center = new TrackingServiceVector3();
            Size = new TrackingServiceVector3();
        }
    }
}

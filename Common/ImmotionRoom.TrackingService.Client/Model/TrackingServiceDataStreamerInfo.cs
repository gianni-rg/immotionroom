namespace ImmotionAR.ImmotionRoom.TrackingService.ControlClient.Model
{
    using System.Collections.Generic;

    public class TrackingServiceDataStreamerInfo
    {
        public string Id { get; set; }
        //public byte UniqueId { get; set; }
        public string StreamEndpoint { get; set; }
        public int StreamPort { get; set; }
        public bool IsMaster { get; set; }
        public List<TrackingServiceSceneDataStreamModes> SupportedStreamModes { get; set; }

        public TrackingServiceDataStreamerInfo()
        {
            SupportedStreamModes = new List<TrackingServiceSceneDataStreamModes>();
        }
    }
}

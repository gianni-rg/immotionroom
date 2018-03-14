namespace ImmotionAR.ImmotionRoom.TrackingService.Model
{
    using System.Collections.Generic;

    public class SceneDataStreamerInfo
    {
        public string Id { get; set; }
        //public byte UniqueId { get; set; }
        public bool IsMaster { get; set; }
        public string StreamEndpoint { get; set; }
        public int StreamPort { get; set; }
        public List<TrackingServiceSceneDataStreamModes> SupportedStreamModes { get; set; }

        public SceneDataStreamerInfo()
        {
            SupportedStreamModes = new List<TrackingServiceSceneDataStreamModes>();
        }
    }
}

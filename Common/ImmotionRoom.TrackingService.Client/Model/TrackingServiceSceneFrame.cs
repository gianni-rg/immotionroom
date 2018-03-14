namespace ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model
{
    using System;
    using System.Collections.Generic;

    public class TrackingServiceSceneFrame
    {
        public byte Version { get; set; }
        public DateTime Timestamp { get; set; }
        public IList<TrackingServiceBodyData> Bodies { get; private set; }

        public bool ClippingEdgesEnabled { get; set; }
        public bool TrackHandsStatus { get; set; }
        public bool TrackJointRotation { get; set; }


        public TrackingServiceSceneFrame()
        {
            Version = 2;
            Bodies = new List<TrackingServiceBodyData>();
        }
    }
}

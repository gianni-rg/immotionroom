namespace ImmotionAR.ImmotionRoom.TrackingEngine.Model
{
    using System;
    using System.Collections.Generic;

    public class SceneFrame
    {
        public DateTime Timestamp { get; set; }
        public Vector4 FloorClipPlane { get; set; }
        public bool ClippingEdgesEnabled { get; set; }
        public bool TrackHandsStatus { get; set; }
        public bool TrackJointRotation { get; set; }

        public IList<BodyData> Bodies { get; private set; }

        public SceneFrame()
        {
            Bodies = new List<BodyData>();
        }
    }
}

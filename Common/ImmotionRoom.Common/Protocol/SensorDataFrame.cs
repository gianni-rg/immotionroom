namespace ImmotionAR.ImmotionRoom.Protocol
{
    using System.Collections.Generic;

    public class SensorDataFrame
    {
        public byte Version { get; private set; }
        public long RelativeTime { get; set; }
        public float FloorClipPlaneX { get; set; }

        public float FloorClipPlaneY { get; set; }

        public float FloorClipPlaneZ { get; set; }

        public float FloorClipPlaneW { get; set; }

        public bool ClippingEdgesEnabled { get; set; }
        public bool TrackHandsStatus { get; set; }
        public bool TrackJointRotation { get; set; }

        public IList<SensorBodyData> Bodies { get; private set; }

        public SensorDataFrame()
        {
            Version = 2;
            Bodies = new List<SensorBodyData>();
        }
    }
}

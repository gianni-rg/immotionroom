namespace ImmotionAR.ImmotionRoom.Protocol
{
    public class SensorBodyJointData
    {
        public SensorBodyJointTypes JointType { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        public float OrientationX { get; set; }
        public float OrientationY { get; set; }
        public float OrientationZ { get; set; }
        public float OrientationW { get; set; }

        public SensorTrackingState TrackingState { get; set; }
    }
}

namespace ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model
{
    public class TrackingServiceBodyJointData
    {
        public TrackingServiceBodyJointTypes JointType { get; set; }
        public float Confidence { get; set; }
        public TrackingServiceVector3 Position { get; set; }
        public TrackingServiceVector4 Orientation { get; set; }

        public TrackingServiceBodyJointData()
        {
            Position = new TrackingServiceVector3();
            Orientation = new TrackingServiceVector4();
        }
    }
}

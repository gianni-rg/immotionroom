namespace ImmotionAR.ImmotionRoom.Protocol
{
    public class SensorVideoStreamFrame
    {
        // FOR FUTURE USES... CAN BE USED TO SUPPORT DIFFERENT TYPES OF SensorVideoStreamFrame
        public byte Version { get; private set; }
        public long RelativeTime { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Depth { get; set; }
        public byte[] RawFrameData { get; set; }

        public SensorVideoStreamFrame()
        {
            Version = 2;
        }
    }
}

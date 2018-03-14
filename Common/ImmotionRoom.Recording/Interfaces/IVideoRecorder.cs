namespace ImmotionAR.ImmotionRoom.Recording.Interfaces
{
    using Model;
    using Protocol;

    public interface IVideoRecorder : IRecorder<SensorVideoStreamFrame>
    {
        StreamType StreamType { get; set; }
        int CapturedVideoFps { get; set; }
        int CapturedVideoWidth { get; set; }
        int CapturedVideoHeight { get; set; }
    }
}

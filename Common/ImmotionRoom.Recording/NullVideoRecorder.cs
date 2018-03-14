namespace ImmotionAR.ImmotionRoom.Recording
{
    using System.Threading.Tasks;
    using Interfaces;
    using Model;
    using Protocol;

    public class NullVideoRecorder : IVideoRecorder
    {
        #region Properties

        public bool IsRecording { get; private set; }
        public string DataRecorderSessionPath { get; set; }
        public StreamType StreamType { get; set; }
        public int CapturedVideoFps { get; set; }
        public int CapturedVideoWidth { get; set; }
        public int CapturedVideoHeight { get; set; }

        #endregion

        #region Methods

        public Task<string> StartRecordingAsync(string sessionId = null)
        {
            // Do nothing
            return Task.FromResult<string>(null);
        }

        public string StopRecording()
        {
            // Do nothing
            return null;
        }

        public void NewDataAvailableHandler(object sender, SensorVideoStreamFrame data)
        {
            // Do nothing
        }

        #endregion
    }
}

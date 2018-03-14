namespace ImmotionAR.ImmotionRoom.Recording.Interfaces
{
    using System.Threading.Tasks;

    public interface IRecorder<T>
    {
        Task<string> StartRecordingAsync(string sessionId = null);

        string StopRecording();

        bool IsRecording { get; }

        string DataRecorderSessionPath { get; set; }

        void NewDataAvailableHandler(object sender, T data);
    }
}

namespace ImmotionAR.ImmotionRoom.DataSourceService.Interfaces
{
    using System.Threading.Tasks;
    using Model;

    public interface IDataStreamerService
    {
        Task<bool> StartAsync(TrackingSessionConfiguration trackingSessionConfiguration);
        void Stop();

        void StartRecordingAsync();
        void StopRecording();
    }
}

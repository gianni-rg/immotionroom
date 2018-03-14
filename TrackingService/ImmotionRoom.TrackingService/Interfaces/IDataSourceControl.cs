namespace ImmotionAR.ImmotionRoom.TrackingService.Interfaces
{
    using System.Threading.Tasks;
    using Model;

    public interface IDataSourceControl
    {
        Task<bool> EnableAutoDiscoveryAsyncFor(string ip, int port);

        Task<bool> StartTrackingAsyncFor(TrackingSessionDataSourceConfiguration trackingSessionConfiguration, string ip, int port);

        Task<bool> StopTrackingAsyncFor(string ip, int port);

        Task<bool> GetStatusAsyncFor(string ip, int port);
    }
}

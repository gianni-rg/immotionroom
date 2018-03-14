namespace ImmotionAR.ImmotionRoom.DataSourceService.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using Model;

    public interface INetworkDiscoveryService
    {
        event EventHandler<TrackingServiceFoundEventArgs> TrackingServiceFound;
        event EventHandler DiscoveryCompleted;

        Task StartAsync();
        void Stop();

        Task StartDiscoveryAsync();
        void StopDiscovery();
    }
}

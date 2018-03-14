namespace ImmotionAR.ImmotionRoom.TrackingService.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using Model;

    public interface INetworkDiscoveryService
    {
        event EventHandler<DataSourceFoundEventArgs> DataSourceFound;
        event EventHandler DiscoveryCompleted;

        Task StartAsync();
        void Stop();

        Task StartDiscoveryAsync();
        void StopDiscovery();
    }
}

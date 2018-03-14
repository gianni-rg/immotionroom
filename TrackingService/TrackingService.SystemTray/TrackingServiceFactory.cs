namespace ImmotionAR.ImmotionRoom.TrackingService
{
    using Interfaces;
    using Model;
    using Networking;

    public class TrackingServiceFactory
    {
        public ITrackingService Create(TrackingServiceConfiguration configuration, DataSourceCollection knownDataSources)
        {
            return new TrackingService(configuration, knownDataSources,
                new TcpServerFactory(),
                new TcpClientFactory(),
                new UdpClientFactory(),
                new NetworkClientFactory()
                );
        }
    }
}

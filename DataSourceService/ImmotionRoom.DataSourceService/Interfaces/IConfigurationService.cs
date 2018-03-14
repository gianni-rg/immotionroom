namespace ImmotionAR.ImmotionRoom.DataSourceService.Interfaces
{
    using System.Threading.Tasks;
    using Model;

    public interface IConfigurationService
    {
        DataSourceConfiguration CurrentConfiguration { get; }
        TrackingServiceInfo TrackingService { get; set; }

        Task InitializeAsync();
        Task SaveCurrentConfigurationAsync();

        void LoadExternalConfiguration(DataSourceConfiguration configuration);
        void LoadInternalConfiguration(TrackingServiceInfo knownTrackingService);
    }
}

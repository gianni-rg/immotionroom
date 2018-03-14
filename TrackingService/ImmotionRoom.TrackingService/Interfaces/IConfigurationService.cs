namespace ImmotionAR.ImmotionRoom.TrackingService.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Model;
    using TrackingEngine.Model;

    public interface IConfigurationService
    {
        TrackingServiceConfiguration CurrentConfiguration { get; }
        CalibrationSettings CalibrationData { get; }
        IReadOnlyDictionary<string, DataSourceInfo> KnownDataSources { get; }
        string CurrentMasterDataSource { get; }

        Task InitializeAsync();
        Task SaveCurrentConfigurationAsync();

        bool AddDataSource(DataSourceInfo dataSourceInfo);
        bool RemoveDataSource(string dataSourceId);
        bool ClearKnownDataSources(bool clearMasterDataSource);
        bool ClearCalibrationData();

        bool SetMasterDataSource(string dataSourceId);
        void UpdateCalibrationData(CalibrationSettings calibrationData);
        void LoadExternalConfiguration(TrackingServiceConfiguration configuration);
        void LoadInternalConfiguration(DataSourceCollection knownDataSources);

        void SetSceneDescriptor(SceneDescriptor descriptor);
    }
}

namespace ImmotionAR.ImmotionRoom.TrackingService.Model
{
    using System.Collections.Generic;

    public class TrackingServiceStatus
    {
        public string Version { get; set; }
        public TrackingServiceState CurrentState { get; set; }
        public bool CalibrationDone { get; set; }
        public int MinDataSourcesForPlay { get; set; }
        public int DataFrameRate { get; set; }
        public SceneDescriptor Scene { get; set; }
        public IReadOnlyDictionary<string, SceneDataStreamerInfo> DataStreamers { get; set; }
        public IReadOnlyDictionary<string, DataSourceInfo> DataSources { get; set; }
        public string MasterDataStreamer { get; set; }
    }
}

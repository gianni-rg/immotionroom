namespace ImmotionAR.ImmotionRoom.TrackingService.ControlClient.Model
{
    using System.Collections.Generic;

    public class TrackingServiceStatus
    {
        public string Version { get; set; }
        public TrackingServiceState CurrentState { get; set; }
        public bool CalibrationDone { get; set; }
        public int MinDataSourcesForPlay { get; set; }
        public int DataFrameRate { get; set; }
        public TrackingServiceSceneDescriptor Scene { get; set; }
        public Dictionary<string, TrackingServiceDataStreamerInfo> DataStreamers { get; set; }
        public Dictionary<string, TrackingServiceDataSourceInfo> DataSources { get; set; }
        public string MasterDataStreamer { get; set; }
    }
}

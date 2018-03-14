namespace ImmotionAR.ImmotionRoom.DataSource.ControlClient.Model
{
    using System.Collections.Generic;

    public class DataSourceServiceStatus
    {
        public string Version { get; set; }
        public DataSourceState CurrentState { get; set; }
        public Dictionary<string, DataSourceInfo> DataSources { get; set; }
    }
}

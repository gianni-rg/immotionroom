namespace ImmotionAR.ImmotionRoom.DataSourceService.Model
{
    public class DataSourceServiceStatus
    {
        public string Version { get; set; }
        public DataSourceState CurrentState { get; set; }
        public string DataStreamerEndpoint { get; set; }
        public int DataStreamerPort { get; set; }
    }
}

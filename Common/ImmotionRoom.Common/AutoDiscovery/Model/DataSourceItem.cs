namespace ImmotionAR.ImmotionRoom.AutoDiscovery.Model
{
    public class DataSourceItem
    {
        public string Id { get; set; }
        public string ControlApiEndpoint { get; set; }
        public int ControlApiPort { get; set; }
        public string DataStreamerEndpoint { get; set; }
        public int DataStreamerPort { get; set; }
    }
}

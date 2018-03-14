namespace ImmotionAR.ImmotionRoom.DataSourceService.Model
{
    using System;

    public class TrackingServiceInfo
    {
        public string Id { get; set; }
        public string DataStreamEndpoint { get; set; }
        public int DataStreamPort { get; set; }
        public string ControlApiEndpoint { get; set; }
        public int ControlApiPort { get; set; }
        public DateTime? FirstTimeSeen { get; set; }
        public DateTime? LastSeen { get; set; }

        public bool IsReachable { get; set; }
    }
}

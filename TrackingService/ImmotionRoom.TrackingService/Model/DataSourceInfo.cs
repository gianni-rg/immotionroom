namespace ImmotionAR.ImmotionRoom.TrackingService.Model
{
    using System;

    public class DataSourceInfo
    {
        public string Id { get; set; }
        public byte UniqueId { get; set; }
        public string DataStreamEndpoint { get; set; }
        public int DataStreamPort { get; set; }
        public string ControlApiEndpoint { get; set; }
        public int ControlApiPort { get; set; }
        public DateTime? FirstTimeSeen { get; set; }
        public DateTime? LastSeen { get; set; }
        public bool IsReachable { get; set; }
        public bool IsMaster { get; set; }
    }
}

namespace ImmotionAR.ImmotionRoom.AutoDiscovery
{
    using System;

    public class DeviceInfo
    {
        public string Id { get; set; }
        public string LicenseId { get; set; }
        public string DataStreamerEndpoint { get; set; }
        public int DataStreamerPort { get; set; }
        public string ControlApiEndpoint { get; set; }
        public int ControlApiPort { get; set; }
        public DateTime FirstTimeSeen { get; set; }
        public DateTime? LastSeen { get; set; }
        public bool IsReachable { get; set; }
    }
}

namespace ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.DataStructures
{
    using System;

    /// <summary>
    /// Represent known informations about a certain data streaming service
    /// </summary>
    public class DataStreamingServiceInfo
    {
        /// <summary>
        /// String ID of the data streaming service
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// IP Address of the data streaming service data stream
        /// </summary>
        public string DataStreamEndpoint { get; set; }

        /// <summary>
        /// IP Port of the data streaming service data stream
        /// </summary>
        public int DataStreamPort { get; set; }

        /// <summary>
        /// IP Address of the control API to control the data streaming service behaviour
        /// </summary>
        public string ControlApiEndpoint { get; set; }

        /// <summary>
        /// IP Port of the control API to control the data streaming service behaviour
        /// </summary>
        public int ControlApiPort { get; set; }

        /// <summary>
        /// Fist time instant we got to know about this data streaming service
        /// </summary>
        public DateTime? FirstTimeSeen { get; set; }

        /// <summary>
        /// Last time instant we seen this data streaming service as up and connected
        /// </summary>
        public DateTime? LastSeen { get; set; }

        /// <summary>
        /// True if this data streaming service is currently reachable, false otherwise
        /// </summary>
        public bool IsReachable { get; set; }
    }
}

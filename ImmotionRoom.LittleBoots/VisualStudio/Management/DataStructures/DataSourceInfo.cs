namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.DataStructures
{
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.DataStructures;
    using System;

    /// <summary>
    /// Informations about a Data Source
    /// </summary>
    public class DataSourceInfo : DataStreamingServiceInfo
    {
        /// <summary>
        /// Byte numeric ID of the data source, guaranteed to be unique across the data sources network
        /// </summary>
        public byte UniqueId { get; set; }

        /// <summary>
        /// True if this is the master data source, false otherwise
        /// </summary>
        public bool IsMaster { get; set; }
    }
}

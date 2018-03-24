namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.DataStructures
{
    using AutoDiscovery;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.DataStructures;

    /// <summary>
    /// Configuration about a particular tracking system (the complete set of a tracking service and its serving data sources)
    /// </summary>
    public class TrackingSystemConfiguration
    {
        /// <summary>
        /// Settings for the Auto Discovery
        /// </summary>
        public AutoDiscoverySettings AutoDiscovery { get; set; }

        /// <summary>
        /// Info about the Tracking Service of the system
        /// </summary>
        public TrackingServiceInfo TrackingService { get; set; }

        /// <summary>
        /// Info about the data sources of the system
        /// </summary>
        public DataSourceCollection DataSources { get; set; }
    }
}

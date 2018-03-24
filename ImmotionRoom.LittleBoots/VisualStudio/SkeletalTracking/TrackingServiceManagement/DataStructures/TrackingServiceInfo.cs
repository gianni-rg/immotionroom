namespace ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.DataStructures
{
    using System;

    /// <summary>
    /// Informations about a Tracking Service
    /// </summary>
    public class TrackingServiceInfo : DataStreamingServiceInfo
    {
        /// <summary>
        /// ID of the master data source (null if no master configured)
        /// </summary>
        public string MasterDataSourceID { get; set; }

        /// <summary>
        /// True if the system is calibrated, false otherwise
        /// </summary>
        public bool IsCalibrated { get; set; }
    }
}

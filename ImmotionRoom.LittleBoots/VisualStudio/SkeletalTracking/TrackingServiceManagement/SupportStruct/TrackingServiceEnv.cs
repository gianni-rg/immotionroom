namespace ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.SupportStruct
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using ImmotionAR.ImmotionRoom.TrackingService.ControlClient.Model;

    /// <summary>
    /// Describes environment in which Tracking Service operate
    /// </summary>
    public class TrackingServiceEnv
    {
        /// <summary>
        /// Data sources of the tracking system.
        /// The list contains all the data sources' unique IDs
        /// </summary>
        public List<byte> DataSources { get; set; }

        /// <summary>
        /// Get minimum number of data sources tracking the player necessary before start a vr game using this system
        /// </summary>
        public int MinDataSourcesForPlayer { get; set; }

        /// <summary>
        /// Descriptor of the scene inside which the tracking happens
        /// </summary>
        public TrackingServiceSceneDescriptor SceneDescriptor { get; set; }
    }
}

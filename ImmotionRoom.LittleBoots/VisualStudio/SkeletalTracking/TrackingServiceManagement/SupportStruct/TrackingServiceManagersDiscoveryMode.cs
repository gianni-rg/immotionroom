using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.SupportStruct
{
    /// <summary>
    /// Enumerates the modalities with which the Tracking Service Managers should try to discover the underlying tracking service
    /// </summary>
    public enum TrackingServiceManagersDiscoveryMode
    {
        /// <summary>
        /// Tries to connect to the tracking service using persistent data stored by the program, containing IP and Port
        /// </summary>
        SettingsOnly,

        /// <summary>
        /// Tries to connect to the tracking service using persistent data stored by the program, containing IP and Port
        /// </summary>
        UserValuesOnly,

        /// <summary>
        /// Search Tracking Service using a network discovery
        /// </summary>
        DiscoveryOnly,

        /// <summary>
        /// Tries to connect to the tracking service using persistent data stored by the program, containing IP and Port. 
        /// If this fails, performs a network discovery
        /// </summary>
        SettingsThenDiscovery,

        /// <summary>
        /// Tries to connect to the tracking service using user-provided data, containing IP and Port. 
        /// If this fails, performs a network discovery
        /// </summary>
        UserValuesThenDiscovery
    }
}

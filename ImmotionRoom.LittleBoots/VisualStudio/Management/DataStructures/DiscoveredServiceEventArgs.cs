namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.DataStructures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Parameters for the event of a discovered service on the network (TrackingService or Data Source)
    /// </summary>
    public class DiscoveredServiceEventArgs : EventArgs
    {
        /// <summary>
        /// Error string of the discovering operation. Null if everything went well
        /// </summary>
        public string ErrorString { get; set; }

        /// <summary>
        /// Human readable name of the discovered service
        /// </summary>
        public string HumanReadableName { get; set; }

        /// <summary>
        /// String containing the Ip and port of the data streaming api of the found service
        /// </summary>
        public string DataIpPort { get; set; }
    
    }
}

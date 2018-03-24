namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.DataStructures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Arguments of the event relative to the discovering of all data sources of the network
    /// </summary>
    public class DiscoveredDataSourcesEventArgs : EventArgs
    {
        /// <summary>
        /// Error string of the discovering operation. Null if everything went well
        /// </summary>
        public string ErrorString { get; set; }

        /// <summary>
        /// Human readable name of the discovered service
        /// </summary>
        public string[] HumanReadableNames { get; set; }

        /// <summary>
        /// String containing the Ip and port of the data streaming api of the found service
        /// </summary>
        public string[] DataIpPorts { get; set; }
    }
}

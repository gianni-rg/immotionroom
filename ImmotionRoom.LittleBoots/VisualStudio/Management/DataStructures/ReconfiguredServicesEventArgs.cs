namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.DataStructures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Parameters for the event of a completed reconfiguration of all services on a network
    /// </summary>
    public class ReconfiguredServicesEventArgs : EventArgs
    {
        /// <summary>
        /// Error string of the discovering operation. Null if everything went well
        /// </summary>
        public string ErrorString { get; set; }

        /// <summary>
        /// Human readable names of all reconfigured services
        /// </summary>
        public string[] HumanReadableNames { get; set; }
    }
}

namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.DataStructures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Parameters for the event of a generic operation requested to an advanced tracking service manager
    /// </summary>
    public class AdvancedOperationEventArgs : EventArgs
    {
        /// <summary>
        /// Error string of the operation. Null if everything went well
        /// </summary>
        public string ErrorString { get; set; }

        /// <summary>
        /// Human readable name of the service affected by the operation, if any
        /// (null if not available)
        /// </summary>
        public string HumanReadableName { get; set; }
    }
}

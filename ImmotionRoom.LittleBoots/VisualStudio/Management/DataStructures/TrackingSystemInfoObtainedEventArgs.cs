namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.DataStructures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Parameters for the event of a fulfilled tracking system info request operation
    /// </summary>
    public class TrackingSystemInfoObtainedEventArgs : EventArgs
    {
        /// <summary>
        /// All informations obtained about the tracking system (tracking service + data sources)
        /// </summary>
        public TrackingSystemInfo TrackingSystemInformations { get; set; }
    }

}

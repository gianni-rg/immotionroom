namespace ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.SupportStruct
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Enumerates possible states of the tracking service manager.
    /// The names are self explanatory
    /// </summary>
    public enum TrackingServiceManagerState
    {
        NotConnected,
        Idle,
        Busy,
        Calibrating,
        Tracking,
        Diagnostic,
        Error
    }
}

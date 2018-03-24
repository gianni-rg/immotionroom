namespace ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.Common
{
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Set the log detail level for ImmotionRoom
    /// </summary>
    public class LogLevelSetting : MonoBehaviour
    {
        /// <summary>
        /// The desired detail level for the ImmotionRoom logger
        /// </summary>
        [Tooltip("The desired detail level for the ImmotionRoom logger")]
        public LogLevel DetailLevel;

        void Awake()
        {
            Log.Level = DetailLevel;
        }
    }
}

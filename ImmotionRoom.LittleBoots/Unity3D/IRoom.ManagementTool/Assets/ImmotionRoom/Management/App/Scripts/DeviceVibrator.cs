namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Makes each device to vibrate in the appropriate manner
    /// </summary>
    public class DeviceVibrator : Vibrator
    {
        /// <summary>
        /// Makes the device to vibrate
        /// </summary>
        public override void Vibrate()
        {
            //vibration is available only in Android
#if UNITY_ANDROID
            Handheld.Vibrate();
#endif
        }
    }
}

namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Base class for objects providing the functionality to make the device to vibrate
    /// </summary>
    public abstract class Vibrator : MonoBehaviour
    {
        /// <summary>
        /// Makes the device to vibrate
        /// </summary>
        public abstract void Vibrate();
    }
}

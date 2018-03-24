namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.HeadsetManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Behaviour that manages the communication of ImmotionRoom with a generic headset
    /// </summary>
    public abstract class HeadsetManager : MonoBehaviour,
        IHeadsetManager
    {
        #region IHeadsetManager members

        ///// <summary>
        ///// Get headset position, in its frame of reference
        ///// </summary>
        //public abstract Vector3 Position
        //{
        //    get;
        //}

        /// <summary>
        /// Get headset position, in Unity frame of reference (it's the position of the camera representing the headset, inside
        /// Unity scene)
        /// </summary>
        public abstract Vector3 PositionInGame
        {
            get;
        }

        ///// <summary>
        ///// Get headset orientation, in its frame of reference
        ///// </summary>
        //public abstract Quaternion Orientation
        //{
        //    get;
        //}

        /// <summary>
        /// Get headset orientation, in Unity frame of reference (it's the orientation of the camera representing the headset, inside
        /// Unity scene)
        /// </summary>
        public abstract Quaternion OrientationInGame
        {
            get;
        }

        ///// <summary>
        ///// Get or set positional tracking functionality on the headset.
        ///// If the headset does not support positional tracking, the value will always be false
        ///// </summary>
        //public abstract bool PositionalTrackingEnabled
        //{
        //    get;
        //    set;
        //}

        ///// <summary>
        ///// Get the user profile data the user has inserted inside headset configuration tool
        ///// </summary>
        //public abstract HeadsetUserProfileData UserProfileData
        //{
        //    get;
        //}

        /// <summary>
        /// Performs operations on the headset scripts, setting the correct flags so the hmd works ok with ImmotionRoom initialization
        /// </summary>
        public abstract void InitForIRoom();

        /// <summary>
        /// Resets headset orientation and position, considering current orientation as the zero orientation for the camera in Unity world.
        /// If current headset can't restore to zero orientation (e.g. Vive), returns the local orientation of the headset after the reset operation
        /// </summary>
        /// <returns>Get headset orientation, in root gameobject of VR headset frame of reference (e.g. the Camera Rig frame of reference, for Oculus environments), expected after a reset orientation</returns>
        public abstract Quaternion ResetView();

        #endregion
    }
}

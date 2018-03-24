namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.HeadsetManagement
{
    using UnityEngine;

    /// <summary>
    /// Interface for communication of ImmotionRoom with a generic headset
    /// </summary>
    public interface IHeadsetManager
    {
        ///// <summary>
        ///// Get headset position, in its frame of reference
        ///// </summary>
        //Vector3 Position
        //{
        //    get;
        //}

        /// <summary>
        /// Get headset position, in Unity frame of reference (it's the position of the camera representing the headset, inside
        /// Unity scene)
        /// </summary>
        Vector3 PositionInGame
        {
            get;
        }

        ///// <summary>
        ///// Get headset orientation, in its frame of reference
        ///// </summary>
        //Quaternion Orientation
        //{
        //    get;
        //}

        /// <summary>
        /// Get headset orientation, in Unity frame of reference (it's the orientation of the camera representing the headset, inside
        /// Unity scene)
        /// </summary>
        Quaternion OrientationInGame
        {
            get;
        }

        ///// <summary>
        ///// Get or set positional tracking functionality on the headset.
        ///// If the headset does not support positional tracking, the value will always be false
        ///// </summary>
        //bool PositionalTrackingEnabled
        //{
        //    get;
        //    set;
        //}

        ///// <summary>
        ///// Get the user profile data the user has inserted inside headset configuration tool
        ///// </summary>
        //HeadsetUserProfileData UserProfileData
        //{
        //    get;
        //}

        /// <summary>
        /// Performs operations on the headset scripts, setting the correct flags so the hmd works ok with ImmotionRoom initialization
        /// </summary>
        void InitForIRoom();

        /// <summary>
        /// Resets headset orientation and position, considering current orientation as the zero orientation for the camera in Unity world.
        /// If current headset can't restore to zero orientation (e.g. Vive), returns the local orientation of the headset after the reset operation
        /// </summary>
        /// <returns>Get headset orientation, in root gameobject of VR headset frame of reference (e.g. the Camera Rig frame of reference, for Oculus environments), expected after a reset orientation</returns>
        Quaternion ResetView();
    }
}

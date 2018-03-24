namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.HeadsetManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Handles communication of ImmotionRoom with Oculus Rift Headset
    /// </summary>
    public class ViveHmdManager : HeadsetManager
    {
        #region Public Unity properties

        /// <summary>
        /// True to track VR controllers, too. False otherwise
        /// </summary>
        [Tooltip("True to track VR controllers, too. False otherwise")]
        public bool TrackVRControllers;

        #endregion

        #region Private fields

        /// <summary>
        /// Reference to the SteamVR object representing the head of the player
        /// </summary>
        private GameObject m_headObject;

        #endregion

        #region Headset members

        /// <summary>
        /// Get headset position, in Unity frame of reference (it's the position of the camera representing the headset, inside
        /// Unity scene)
        /// </summary>
        public override Vector3 PositionInGame
        {
            get
            {
                return m_headObject.transform.position;
            }
        }
        

        /// <summary>
        /// Get headset orientation, in Unity frame of reference (it's the orientation of the camera representing the headset, inside
        /// Unity scene)
        /// </summary>
        public override Quaternion OrientationInGame
        {
            get 
            {
                return m_headObject.transform.rotation; 
            }
        }

        /// <summary>
        /// Performs operations on the headset scripts, setting the correct flags so the hmd works ok with ImmotionRoom initialization
        /// </summary>
        public override void InitForIRoom()
        {
            //disable controllers, if not required
            if(TrackVRControllers == false)
            {
                FindObjectOfType<SteamVR_ControllerManager>().enabled = false;
            }
        }

        /// <summary>
        /// Resets headset orientation and position, considering current orientation as the zero orientation for the camera in Unity world.
        /// If current headset can't restore to zero orientation (e.g. Vive), returns the local orientation of the headset after the reset operation
        /// </summary>
        /// <returns>Get headset orientation, in root gameobject of VR headset frame of reference (e.g. the Camera Rig frame of reference, for Oculus environments), expected after a reset orientation</returns>
        public override Quaternion ResetView()
        {
            SteamVR.instance.hmd.ResetSeatedZeroPose();

            return m_headObject.transform.localRotation; //Vive seems not to reset orientation after call to ResetSeatedZeroPose, so return local rotation of head
        }

        #endregion

        #region Behaviour methods

        void Start()
        {
            //find the Camera(head) object (i.e. the object getting Hmd data) and save its reference

            SteamVR_TrackedObject[] steamVRObjects = FindObjectsOfType<SteamVR_TrackedObject>();

            foreach (SteamVR_TrackedObject steamVRObject in steamVRObjects)
                if (steamVRObject.index == SteamVR_TrackedObject.EIndex.Hmd && steamVRObject.GetComponent<Camera>() != null)
                {
                    m_headObject = steamVRObject.gameObject;
                    break;
                }
        }

        #endregion
    }

}

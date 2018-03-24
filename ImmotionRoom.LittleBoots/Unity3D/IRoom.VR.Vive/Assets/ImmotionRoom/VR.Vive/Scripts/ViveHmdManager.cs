/************************************************************************************************************
 * 
 * Copyright (C) 2014-2016 ImmotionAR, a division of Beps Engineering. All rights reserved.
 * 
 * Licensed under the ImmotionAR ImmotionRoom SDK License (the "License");
 * you may not use the ImmotionAR ImmotionRoom SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 * 
 * You may obtain a copy of the License at
 * 
 * http://www.immotionar.com/legal/ImmotionRoomSDKLicense.PDF
 * 
 ************************************************************************************************************/
namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.HeadsetManagement
{
    using System;
    using System.Collections;
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
            //assign as head object the object with the SteamVR_Camera
            m_headObject = FindObjectOfType<SteamVR_Camera>().gameObject;

            //start find head coroutine
            StartCoroutine(FindHead());
        }

        #endregion

        #region Init methods

        /// <summary>
        /// Coroutine to find head of player
        /// </summary>
        IEnumerator FindHead()
        {
            //ok, let's explain all: we have pre-initialized head object as the camera and that's ok for all steam vr compatible headsets
            //Vive headset, instead, activates another camera, the one that has a SteamVR_TrackedObject script attached... let's see if we have to pick this one
            yield return new WaitForSeconds(0.1f);

            //look for a valid steam vr tracked head object...if we find it, replace m_headObject reference... otherwise keep the one you already have
            SteamVR_TrackedObject[] steamVRObjects = FindObjectOfType<SteamVR_ControllerManager>().gameObject.GetComponentsInChildren<SteamVR_TrackedObject>(true); //we have to do this way because FindObjectsOfType does not work with inactive objects and SteamVR deactivate the scripts for the initializations

            foreach (SteamVR_TrackedObject steamVRObject in steamVRObjects)
                if (steamVRObject.index == SteamVR_TrackedObject.EIndex.Hmd && steamVRObject.GetComponent<Camera>() != null && steamVRObject.gameObject.activeInHierarchy == true)
                {
                    m_headObject = steamVRObject.gameObject;
                    break;
            }

            yield break;
        }

        #endregion
    }

}

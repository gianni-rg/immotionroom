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
    using PlayerController;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Handles communication of ImmotionRoom with Cardboard or Daydream Headset
    /// </summary>
    public class GoogleVRHmdManager : HeadsetManager
    {
        #region Private fields

        /// <summary>
        /// Holds reference of the Google VR manager of the game
        /// </summary>
        GvrViewer m_gvrManager;

        /// <summary>
        /// Holds reference of the Google VR user head of the game
        /// </summary>
        GameObject m_gvrHead;

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

                return m_gvrHead.transform.position;
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
                return m_gvrHead.transform.rotation;
            }
        }

        /// <summary>
        /// Performs operations on the headset scripts, setting the correct flags so the hmd works ok with ImmotionRoom initialization
        /// </summary>
        public override void InitForIRoom()
        {
            //m_gvrHead.trackPosition = true;
            //m_gvrHead.trackRotation = true;
        }

        /// <summary>
        /// Resets headset orientation and position, considering current orientation as the zero orientation for the camera in Unity world.
        /// If current headset can't restore to zero orientation (e.g. Vive), returns the local orientation of the headset after the reset operation
        /// </summary>
        /// <returns>Get headset orientation, in root gameobject of VR headset frame of reference (e.g. the Camera Rig frame of reference, for Oculus environments), expected after a reset orientation</returns>
        public override Quaternion ResetView()
        {
            m_gvrManager.Recenter();

            return Quaternion.identity;
        }

        #endregion

        #region Behaviour methods

        void Start()
        {
            m_gvrManager = FindObjectOfType<GvrViewer>();
            m_gvrHead = m_gvrManager.transform.GetChild(0).gameObject;

            StartCoroutine(UpdateStereoCameras());
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Necessary to trigger Update stereo cameras when the player controller is initialized, or all FOV and camera planes changed applied by the player controller do not get considered by
        /// Google VR engine
        /// </summary>
        /// <returns></returns>
        IEnumerator UpdateStereoCameras()
        {
            //wait while the player is ready
            IroomPlayerController player = GetComponent<IroomPlayerController>();

            while(!player.IsVrReady)
            {
                yield return new WaitForSeconds(0.111f);        
            }

            //update stereo values
            GetComponentInChildren<StereoController>().UpdateStereoValues();

            //we're done :)
            yield break;
        }

        #endregion
    }

}

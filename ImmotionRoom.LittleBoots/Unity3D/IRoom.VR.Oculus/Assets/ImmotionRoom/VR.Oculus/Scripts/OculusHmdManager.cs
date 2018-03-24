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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Handles communication of ImmotionRoom with Oculus Rift Headset
    /// </summary>
    public class OculusHmdManager : HeadsetManager
    {
        #region Private fields

        /// <summary>
        /// Holds reference of the ovr manager of the game
        /// </summary>
        OVRManager m_ovrManager;

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
                return m_ovrManager.transform.position;
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
                return m_ovrManager.transform.GetChild(0).Find("CenterEyeAnchor").rotation;
            }
        }

        /// <summary>
        /// Performs operations on the headset scripts, setting the correct flags so the hmd works ok with ImmotionRoom initialization
        /// </summary>
        public override void InitForIRoom()
        {
            m_ovrManager.usePositionTracking = true;
            m_ovrManager.resetTrackerOnLoad = false;
        }

        /// <summary>
        /// Resets headset orientation and position, considering current orientation as the zero orientation for the camera in Unity world.
        /// If current headset can't restore to zero orientation (e.g. Vive), returns the local orientation of the headset after the reset operation
        /// </summary>
        /// <returns>Get headset orientation, in root gameobject of VR headset frame of reference (e.g. the Camera Rig frame of reference, for Oculus environments), expected after a reset orientation</returns>
        public override Quaternion ResetView()
        {
            OVRManager.display.RecenterPose();

            return Quaternion.identity;
        }

        #endregion

        #region Behaviour methods

        void Start()
        {
            m_ovrManager = FindObjectOfType<OVRManager>();
        }

        #endregion
    }

}

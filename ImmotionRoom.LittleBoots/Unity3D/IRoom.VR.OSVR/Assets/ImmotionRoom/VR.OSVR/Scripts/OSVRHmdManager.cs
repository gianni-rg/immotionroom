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
    using OSVR.Unity;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Handles communication of ImmotionRoom with OSVR Headset
    /// </summary>
    public class OSVRHmdManager : HeadsetManager
    {
        #region Private fields

        /// <summary>
        /// Holds reference of the OSVR Client Kit of the game
        /// </summary>
        private ClientKit m_osvrClientKit;

        /// <summary>
        /// Holds reference of the OSVR Display Controller of the game
        /// </summary>
        private DisplayController m_osvrDisplayController;

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

                return m_osvrDisplayController.transform.GetChild(0).position;
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
                return m_osvrDisplayController.transform.GetChild(0).rotation;
            }
        }

        /// <summary>
        /// Performs operations on the headset scripts, setting the correct flags so the hmd works ok with ImmotionRoom initialization
        /// </summary>
        public override void InitForIRoom()
        {
        }

        /// <summary>
        /// Resets headset orientation and position, considering current orientation as the zero orientation for the camera in Unity world.
        /// If current headset can't restore to zero orientation (e.g. Vive), returns the local orientation of the headset after the reset operation
        /// </summary>
        /// <returns>Get headset orientation, in root gameobject of VR headset frame of reference (e.g. the Camera Rig frame of reference, for Oculus environments), expected after a reset orientation</returns>
        public override Quaternion ResetView()
        {
            if (m_osvrDisplayController != null && m_osvrDisplayController.UseRenderManager)
            {
                m_osvrDisplayController.RenderManager.SetRoomRotationUsingHead();
            }
            else
            {
                m_osvrClientKit.context.SetRoomRotationUsingHead();
            }

            return Quaternion.identity;
        }

        #endregion

        #region Behaviour methods

        void Start()
        {
            m_osvrDisplayController = FindObjectOfType<DisplayController>();
            m_osvrClientKit = ClientKit.instance;
        }

        #endregion
    }

}

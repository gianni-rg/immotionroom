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
namespace ImmotionAR.ImmotionRoom.LittleBoots.IRoom.VR
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Tools;
    using System;
    using ImmotionAR.ImmotionRoom.LittleBoots.VR.PlayerController;
    using ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model;

    /// <summary>
    /// Creates spheres to represent the VR player's joints in Kinect frame of reference
    /// </summary>
    public class PlayerRawJointsVisualizer : MonoBehaviour
    {
        #region Public Unity properties

        /// <summary>
        /// Color of the spheres to attach
        /// </summary>
        [Tooltip("Color of the spheres to attach")]
        public Color SpheresColor;

        /// <summary>
        /// Size of the spheres to attach
        /// </summary>
        [Tooltip("Size of the spheres to attach")]
        public float SpheresSize;

        #endregion

        #region Private fields

        /// <summary>
        /// Reference to the player controller of the scene
        /// </summary>
        private IroomPlayerController m_playerController;

        /// <summary>
        /// Spheres controlled by this object
        /// </summary>
        private List<GameObject> m_spheres;

        #endregion

        #region Behaviour methods

        void Start()
        {
            m_spheres = new List<GameObject>();
            m_playerController = FindObjectOfType<IroomPlayerController>();

            //create the spheres to be used for the body joints
            CreateRightNumberOfSpheres();
        }

        // Update is called once per frame
        void Update()
        {
            //if a player exists, and it is active and tracked
            if (m_playerController != null && m_playerController.IsVrReady && m_playerController.LastTrackedBody != null)
            {
                //show all the spheres
                foreach (Transform child in transform)
                    child.gameObject.SetActive(true);

                //get the position of all joints and assign the corresponding sphere the right position
                int sphereId = 0;

                foreach (TrackingServiceBodyJointTypes bodyJointType in Enum.GetValues(typeof(TrackingServiceBodyJointTypes)))
                {
                    m_spheres[sphereId++].transform.position = m_playerController.LastTrackedBody.Joints[bodyJointType].ToUnityVector3NoScale();
                }
            }
            //else
            else
                //hide all the spheres
                foreach (Transform child in transform)
                    child.gameObject.SetActive(false);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Create the spheres to represent the body joints
        /// </summary>
        private void CreateRightNumberOfSpheres()
        {
            //create one sphere for each body joint
            for (int i = 0; i < Enum.GetValues(typeof(TrackingServiceBodyJointTypes)).Length; i++)
            {
                GameObject sphereGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphereGo.name = "FancyRawSphere";
                sphereGo.transform.SetParent(transform, false); //to not clutter the scene hierarchy, add the new cube as child of this object
                sphereGo.transform.localScale = SpheresSize * Vector3.one;
                sphereGo.GetComponent<Renderer>().material.color = SpheresColor;
                m_spheres.Add(sphereGo);
            }
        }

        #endregion

    }

}
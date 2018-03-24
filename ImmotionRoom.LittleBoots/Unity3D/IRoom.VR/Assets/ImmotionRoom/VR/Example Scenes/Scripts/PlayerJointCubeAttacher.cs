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
    using ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model;
    using ImmotionAR.ImmotionRoom.LittleBoots.Avateering;
    using System.Collections.Generic;
    using ImmotionAR.ImmotionRoom.LittleBoots.VR.PlayerController;

    /// <summary>
    /// Attaches a cube to the avatar of the VR player's selected joint
    /// </summary>
    public class PlayerJointCubeAttacher : MonoBehaviour
    {
        #region Public Unity properties

        /// <summary>
        /// Body joint to which attach a cube to
        /// </summary>
        [Tooltip("Body joint to which attach a cube to")]
        public TrackingServiceBodyJointTypes BodyJoint;

        /// <summary>
        /// Color of the cubes to attach
        /// </summary>
        [Tooltip("Color of the cubes to attach")]
        public Color CubeColor;

        /// <summary>
        /// Size of the cubes to attach
        /// </summary>
        [Tooltip("Size of the cubes to attach")]
        public float CubeSize;

        #endregion

        #region Private fields

        /// <summary>
        /// Reference to the player controller of the scene
        /// </summary>
        private IroomPlayerController m_playerController;

        /// <summary>
        /// Cube controlled by this object
        /// </summary>
        private GameObject m_cube;

        #endregion

        #region Behaviour methods

        void Start()
        {
            //get player controller reference
            m_playerController = FindObjectOfType<IroomPlayerController>();

            //create the cube
            m_cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            m_cube.name = "FancyCube";
            m_cube.transform.SetParent(transform, false); //to not clutter the scene hierarchy, add the new cube as child of this object
            m_cube.transform.localScale = CubeSize * Vector3.one;
            m_cube.GetComponent<Renderer>().material.color = CubeColor;
            m_cube.GetComponent<Collider>().enabled = false; //to avoid strange physics behaviour
        }

        // Update is called once per frame
        void Update()
        {
            //if a player exists, and it is active and tracked
            if (m_playerController != null && m_playerController.IsVrReady)
            {
                //show the cube                
                m_cube.SetActive(true);

                //set its position and rotation accordingly to the joint
                Transform jointPos = m_playerController.MainAvatar.GetJointTransform(BodyJoint);

                m_cube.transform.position = jointPos.position;
                m_cube.transform.rotation = jointPos.rotation;
            }
            //else
            else
                //hide the cube                
                m_cube.SetActive(false);
            
        }

        #endregion       

    }

}
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
    using ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Collisions;
    using System;
    using UnityEngine;

    /// <summary>
    /// Manages a cube that starts as blue and then changes color depending on the hands or foot of the player it gets touched by
    /// </summary>
    public class BlueCubes : MonoBehaviour
    {
        #region Constants definitions

        /// <summary>
        /// Possible colors of the cube
        /// </summary>
        static readonly Color[] CubeColors = new Color[] 
        {
            Color.red,
            Color.green,
            Color.white,
            Color.black
        };

        #endregion

        #region Public Unity Properties

        /// <summary>
        /// Speed with which the cube returns to blue color
        /// </summary>
        [Tooltip("Speed with which the cube returns to blue color")]
        public float ReturnToBlueSpeed = 10;

        #endregion

        #region Private fields

        /// <summary>
        /// Reference to the renderer of this object
        /// </summary>
        Renderer m_render;

        #endregion

        #region Behaviour methods

        // Use this for initialization
        void Start()
        {
            m_render = GetComponent<Renderer>();
            m_render.material.color = Color.blue;
        }

        // Update is called once per frame
        void Update()
        {
            //unoptimized, but it works: force the cube to return to blue color at each frame
            m_render.material.color = new Color(
                    Mathf.Max(0, m_render.material.color.r - ReturnToBlueSpeed * Time.deltaTime),
                    Mathf.Max(0, m_render.material.color.g - ReturnToBlueSpeed * Time.deltaTime),
                    Mathf.Min(1, m_render.material.color.b + ReturnToBlueSpeed * Time.deltaTime));
        }

        void OnTriggerEnter(Collider collider)
        {
            //if we are collided by a trigger, check if it is a hand or a foot, and set cube color accordingly

            if (AvatarCollidersProps.IsAvatarLeftHandCollider(collider))
            {
                m_render.material.color = CubeColors[0];
            }
            else if (AvatarCollidersProps.IsAvatarRightHandCollider(collider))
            {
                m_render.material.color = CubeColors[1];
            }              
            else if (AvatarCollidersProps.IsAvatarLeftFootCollider(collider))
            {
                m_render.material.color = CubeColors[2];
            }
            else if (AvatarCollidersProps.IsAvatarRightFootCollider(collider))
            {
                m_render.material.color = CubeColors[3];
            }

        }

        #endregion

    }

}
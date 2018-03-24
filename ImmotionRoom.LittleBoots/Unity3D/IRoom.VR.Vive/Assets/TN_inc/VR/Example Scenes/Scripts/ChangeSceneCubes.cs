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

#if UNITY_5_3_OR_NEWER
    using UnityEngine.SceneManagement;
#endif

    /// <summary>
    /// Manages a cube that when touched triggers a scene change
    /// </summary>
    public class ChangeSceneCubes : MonoBehaviour
    {
 
        #region Public Unity Properties

        /// <summary>
        /// Scene to load when the cube is touched by the player
        /// </summary>
        [Tooltip("Scene to load when the cube is touched by the player")]
        public string SceneToLoadName;

        #endregion

        #region Private fields

        /// <summary>
        /// Time the object was created
        /// </summary>
        private float m_initTime;

        #endregion

        #region Behaviour methods

        void Start()
        {
            m_initTime = Time.timeSinceLevelLoad;
        }

        void OnTriggerEnter(Collider collider)
        {
            //if we are collided by a trigger, check if it is a hand or a foot, and change scene if required
            //Notice that we check that the level has been loaded since at least 1 second, to avoid switching continuously
            //between scenes when a cube is touched

            if (Time.timeSinceLevelLoad - m_initTime > 1.0f && AvatarCollidersProps.IsAvatarCollider(collider))
            {
#if UNITY_5_3_OR_NEWER
                SceneManager.LoadScene(SceneToLoadName);
#else
                Application.LoadLevel(SceneToLoadName);
#endif
            }

        }

        #endregion

    }

}
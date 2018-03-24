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
namespace ImmotionAR.ImmotionRoom.LittleBoots.IRoom.SkeletalTracking
{
    using UnityEngine;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model;
    using ImmotionAR.ImmotionRoom.LittleBoots.Avateering;
    using System.Collections.Generic;

    /// <summary>
    /// Attaches a cube to the avatars' selected joint
    /// </summary>
    public class JointCubeAttacher : MonoBehaviour
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
        /// Cubes controlled by this object
        /// </summary>
        private List<GameObject> m_cubes;

        #endregion

        #region Behaviour methods

        void Start()
        {
            m_cubes = new List<GameObject>();
        }

        // Update is called once per frame
        void Update()
        {
            //get the body avatars in this frame
            BodyAvatarer[] avatarers = FindObjectsOfType<BodyAvatarer>();

            //we should have the same number of controlled cubes as the number of bodies in the scene
            EnsureRightNumberOfCubes(avatarers);

            //for each avatar, get the transform of the joint of interest and assign the corresponding cube the right position and orientation
            for(int i = 0; i < avatarers.Length; i++)
            {
                Transform jointPos = avatarers[i].GetJointTransform(BodyJoint);

                if (jointPos != null) //e.g. is null during avatar initialization!
                {
                    m_cubes[i].transform.position = jointPos.position;
                    m_cubes[i].transform.rotation = jointPos.rotation;
                }
            }
            
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Check we have the right amount of cubes, given the number of bodies in the scene
        /// </summary>
        /// <param name="avatarers">Bodies in the scene</param>
        private void EnsureRightNumberOfCubes(BodyAvatarer[] avatarers)
        {
            //if we have too many cubes
            if (m_cubes.Count > avatarers.Length)
            {
                //loop through the controlled cube array and remove the exceeding number of cubes
                //(remember that we have also to remove them from the Unity scene!)

                int elementsToRemove = m_cubes.Count - avatarers.Length;
                int originalCubesCount = m_cubes.Count;

                for (int i = 0; i < elementsToRemove; i++)
                {
                    Destroy(m_cubes[originalCubesCount - i - 1]);
                    m_cubes.RemoveAt(originalCubesCount - i - 1);
                }
            }
            //else, if we have too few cubes
            else if (m_cubes.Count < avatarers.Length)
            {
                //add the necessary number of cubes, both in the list and in Unity scene
                //remember to give them the correct color and dimension
                int elementsToAdd = avatarers.Length - m_cubes.Count;

                for (int i = 0; i < elementsToAdd; i++)
                {
                    GameObject cubeGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cubeGo.name = "FancyCube";
                    cubeGo.transform.SetParent(transform, false); //to not clutter the scene hierarchy, add the new cube as child of this object
                    cubeGo.transform.localScale = CubeSize * Vector3.one;
                    cubeGo.GetComponent<Renderer>().material.color = CubeColor;
                    m_cubes.Add(cubeGo);
                }
            }
        }

        #endregion

    }

}
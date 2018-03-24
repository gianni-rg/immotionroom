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
    using System.Collections.Generic;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.DataSourcesManagement;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Tools;
    using System;

    /// <summary>
    /// Creates spheres to represent the user joints in Kinect frame of reference
    /// </summary>
    public class RawJointsVisualizer : MonoBehaviour
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
        /// True if this behaviour is connected to the tracking service and initialized, false otherwise
        /// </summary>
        private bool m_ready;

        /// <summary>
        /// Provider of the scene data read from the tracking service
        /// </summary>
        private SceneDataProvider m_sceneDataProvider;

        /// <summary>
        /// Spheres controlled by this object
        /// </summary>
        private List<GameObject> m_spheres;

        #endregion

        #region Behaviour methods

        void Start()
        {
            m_spheres = new List<GameObject>();
            m_ready = false;

            StartCoroutine(ConnectToTrackingService());
        }

        void OnDestroy()
        {
            //remember to dispose the data provider!
            StopAllCoroutines();

            if (m_sceneDataProvider != null)
                m_sceneDataProvider.Dispose();
        }

        // Update is called once per frame
        void Update()
        {
            //if system is connected 
            if (m_ready)
            {
                //we should have the same number of controlled spheres as the number of joints in the scene
                EnsureRightNumberOfSpheres();

                //for each body, get the position of all joints and assign the corresponding sphere the right position
                int sphereId = 0;

                foreach(TrackingServiceBodyData body in m_sceneDataProvider.LastBodies)
                {
                    foreach (TrackingServiceBodyJointTypes bodyJointType in Enum.GetValues(typeof(TrackingServiceBodyJointTypes)))
                    {
                        m_spheres[sphereId++].transform.position = body.Joints[bodyJointType].ToUnityVector3NoScale();
                    }
                }
            }

        }

        #endregion

        #region Private methods

        IEnumerator ConnectToTrackingService()
        {
            //wait for tracking service connection and tracking
            while (!TrackingServiceManagerBasic.Instance.IsTracking)
                yield return new WaitForSeconds(0.1f);

            //create the scene data provider, waiting for it to begin
            m_sceneDataProvider = null;

            while ((m_sceneDataProvider = TrackingServiceManagerBasic.Instance.StartSceneDataProvider()) == null)
                yield return new WaitForSeconds(0.1f);

            //ok, now we have access to raw data! We can set the flag ready to true and return
            yield return 0;

            m_ready = true;

            yield break;
        }

        /// <summary>
        /// Check we have the right amount of cubes, given the number of bodies in the scene
        /// </summary>
        private void EnsureRightNumberOfSpheres()
        {
            //calc total number of joints
            int totalNumberOfJoints = m_sceneDataProvider.LastBodies.Count * Enum.GetValues(typeof(TrackingServiceBodyJointTypes)).Length;

            //if we have too many spheres
            if (m_spheres.Count > totalNumberOfJoints)
            {
                //loop through the controlled spheres array and remove the exceeding number of spheres
                //(remember that we have also to remove them from the Unity scene!)

                int elementsToRemove = m_spheres.Count - totalNumberOfJoints;
                int originalSpheresCount = m_spheres.Count;

                for (int i = 0; i < elementsToRemove; i++)
                {
                    Destroy(m_spheres[originalSpheresCount - i - 1]);
                    m_spheres.RemoveAt(originalSpheresCount - i - 1);
                }
            }
            //else, if we have too few spheres
            else if (m_spheres.Count < totalNumberOfJoints)
            {
                //add the necessary number of spheres, both in the list and in Unity scene
                //remember to give them the correct color and dimension
                int elementsToAdd = totalNumberOfJoints - m_spheres.Count;

                for (int i = 0; i < elementsToAdd; i++)
                {
                    GameObject sphereGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphereGo.name = "FancyRawSphere";
                    sphereGo.transform.SetParent(transform, false); //to not clutter the scene hierarchy, add the new cube as child of this object
                    sphereGo.transform.localScale = SpheresSize * Vector3.one;
                    sphereGo.GetComponent<Renderer>().material.color = SpheresColor;
                    m_spheres.Add(sphereGo);
                }
            }
        }

        #endregion

    }

}
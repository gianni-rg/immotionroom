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
    /// Creates spheres to represent the user joints in Kinect frame of reference, for a single specific body
    /// </summary>
    public class RawJointsVisualizerSelective : MonoBehaviour
    {
        #region Public Unity properties

        /// <summary>
        /// Body ID for which the skeleton has to be drawn
        /// </summary>
        [Tooltip("Body ID for which the skeleton has to be drawn")]
        public ulong BodyId;

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
        /// Provider of the body data read from the tracking service
        /// </summary>
        private BodyDataProvider m_bodyDataProvider;

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

            //create the spheres to be used for the body joints
            CreateRightNumberOfSpheres();

            //connect to tracking service
            StartCoroutine(ConnectToTrackingService());
        }

        void OnDestroy()
        {
            //remember to dispose the data provider!
            StopAllCoroutines();

            if (m_bodyDataProvider != null)
                m_bodyDataProvider.ActualSceneDataProvider.Dispose();
        }

        // Update is called once per frame
        void Update()
        {
            //if system is connected and the desired body is tracked (there is a body for the last frame)
            if (m_ready && m_bodyDataProvider.LastBody != null)
            {
                //show all the spheres
                foreach (Transform child in transform)
                    child.gameObject.SetActive(true);

                //for each body, get the position of all joints and assign the corresponding sphere the right position
                int sphereId = 0;

                foreach (TrackingServiceBodyJointTypes bodyJointType in Enum.GetValues(typeof(TrackingServiceBodyJointTypes)))
                {
                    m_spheres[sphereId++].transform.position = m_bodyDataProvider.LastBody.Joints[bodyJointType].ToUnityVector3NoScale();
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

        IEnumerator ConnectToTrackingService()
        {
            //wait for tracking service connection and tracking
            while (!TrackingServiceManagerBasic.Instance.IsTracking)
                yield return new WaitForSeconds(0.1f);

            //create the scene data provider, waiting for it to begin
            SceneDataProvider sceneDataProvider = null;

            while ((sceneDataProvider = TrackingServiceManagerBasic.Instance.StartSceneDataProvider()) == null)
                yield return new WaitForSeconds(0.1f);

            //create the body data provider for the desired body ID
            m_bodyDataProvider = new BodyDataProvider(sceneDataProvider, BodyId);

            //ok, now we have access to raw data! We can set the flag ready to true and return
            yield return 0;

            m_ready = true;

            yield break;
        }

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
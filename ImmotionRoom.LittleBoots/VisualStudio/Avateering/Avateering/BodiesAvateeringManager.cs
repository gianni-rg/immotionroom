namespace ImmotionAR.ImmotionRoom.LittleBoots.Avateering
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.DataSourcesManagement;
    using ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;

    /// <summary>
    /// Manages avateering of all the bodies seen by a Tracking Service
    /// </summary>
    public abstract class BodiesAvateeringManager : MonoBehaviour
    {
        #region Unity public properties

        /// <summary>
        /// True if the bodies has to be translated as users' world position, false otherwise.
        /// Translation position is evaluated in a way depending on implementation of the avatar
        /// </summary>
        [Tooltip("True to track avatars position, false otherwise")]
        public bool TrackPosition;

        /// <summary>
        /// True if the bodies have to cast/receive shadows, false otherwise
        /// </summary>
        [Tooltip("True if the body has to cast/receive shadows, false otherwise")]
        public bool ShadowsEnabled;

        #endregion

        #region Private fields

        /// <summary>
        /// Array of bodies managed by this behaviour
        /// </summary>
        private HashSet<ulong> m_managedBodiesId;

        /// <summary>
        /// Root object that holds all body avateerers behaviours
        /// </summary>
        private GameObject m_bodyAvatarersRoot;

        /// <summary>
        /// Object that retrieves bodies from the tracking service manager
        /// </summary>
        protected SceneDataProvider m_sceneData;

        #endregion

        #region Behaviour methods

        protected virtual void Awake()
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("Bodies Avateering Manager - Awaken");
            }

            m_managedBodiesId = new HashSet<ulong>();
            m_bodyAvatarersRoot = new GameObject("Bodies Avatars Root");
            m_bodyAvatarersRoot.transform.SetParent(transform, false);

            StartCoroutine(TrackingServiceConnect());
        }

        protected void OnDestroy()
        {
            StopAllCoroutines();

            if (m_sceneData != null)
                m_sceneData.Dispose();

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Bodies Avateering Manager - Destroyed");
            }
        }

        protected virtual void Update()
        {
            //get bodies from tracking service
            if(m_sceneData != null && m_sceneData.IsStillValid)
            {
                //extract current bodies' ids
                var presentBodies = m_sceneData.LastBodies;

                //if we have no bodies, delete all
                if (presentBodies == null)
                    m_managedBodiesId.Clear();
                //otherwise
                else
                {
                    //delete non-existing bodies
                    foreach (ulong id in m_managedBodiesId)
                        if (!presentBodies.Any(body => body.Id == id))
                        {
                            DeleteBodyAvatar(id);
                        }

                    //can't remove from hashset while iterating
                    m_managedBodiesId.RemoveWhere(id => !presentBodies.Any(body => body.Id == id));

                    //add new bodies
                    foreach (TrackingServiceBodyData body in presentBodies)
                        if (!m_managedBodiesId.Contains(body.Id))
                        {
                            m_managedBodiesId.Add(body.Id);
                            AddNewBodyAvatar(body.Id);
                        }

                }
            }
        }

        #endregion

        #region Body Avatars Management

        /// <summary>
        /// Adds new body avatar, to follow a particular body detected by the Tracking Service
        /// </summary>
        /// <param name="bodyId">Unique Body ID</param>
        private void AddNewBodyAvatar(ulong bodyId)
        {
            //create new gameobject for the avatar
            GameObject avatarGo = new GameObject("Body Avatar " + bodyId);
            avatarGo.transform.SetParent(m_bodyAvatarersRoot.transform, false);

            //add the particular avateerer for this body
            AddAvateerer(avatarGo, bodyId);

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Bodies Avateering Manager - Added new body avatar for body with ID {0}", bodyId);
            }
        }

        /// <summary>
        /// Adds a body avateerer for a body of interest to the provided game object
        /// </summary>
        /// <param name="avatarGo">Avatar Game Object the avatareer has to be attached to</param>
        /// <param name="bodyId">Unique Body ID</param>
        protected abstract void AddAvateerer(GameObject avatarGo, ulong bodyId);

        /// <summary>
        /// Removes an existing body avatar, because its body is not detected anymore by the Tracking Service
        /// </summary>
        /// <param name="bodyId">Unique Body ID</param>
        private void DeleteBodyAvatar(ulong bodyId)
        {
            //destroy the avatar gameobject

            Transform foundAvatarTransform = m_bodyAvatarersRoot.transform.Find("Body Avatar " + bodyId);

            if (foundAvatarTransform != null)
                Destroy(foundAvatarTransform.gameObject);

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Bodies Avateering Manager - Deleted body avatar for body with ID {0}", bodyId);
            }
        }

        #endregion

        #region Tracking Service Connection

        /// <summary>
        /// Connect to tracking service and initialize the scene data provider
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator TrackingServiceConnect()
        {
            //wait for tracking service connection and tracking
            while (!TrackingServiceManagerBasic.Instance.IsTracking)
                yield return new WaitForSeconds(0.1f);

            //create the body provider, waiting for it to begin
            while ((m_sceneData = TrackingServiceManagerBasic.Instance.StartSceneDataProvider()) == null)
                yield return new WaitForSeconds(0.1f);

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Bodies Avateering Manager - Connected to Tracking Service and Initialized");
            }

            yield break;
        }

        #endregion
    }
}

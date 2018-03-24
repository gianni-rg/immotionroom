namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedAvateering
{
    using ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Skeletals;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedManager;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.DataSourcesManagement;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using ImmotionAR.ImmotionRoom.TrackingService.ControlClient.Model;
    using ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Avateers a body using a bare-bones skeleton, for the management app
    /// </summary>
    class SkeletalBodyAvatererAdvanced : SkeletalBodyAvaterer
    {
        #region Public Unity Properties

        /// <summary>
        /// ID of the stream whose skeleton is to be shown
        /// </summary>
        [Tooltip("ID of the data stream whose skeletons are to be shown")]
        public string SceneStreamerInfoId;

        /// <summary>
        /// Streaming mode of the skeleton that is to be shown (e.g. world transform skeleton vs master transform skeleton)
        /// </summary>
        [Tooltip("Which kind of scene data stream modes are to be asked from the tracking service (e.g. world transform skeletons vs master transform skeletons)")]
        public TrackingServiceSceneDataStreamModes SceneStreamingMode;

        /// <summary>
        /// GameObjects to activate or deactivate when this skeletons reaches the kinect tracking area limits.
        /// When kinect communicate that this skeleton is near the tracking area borders, the object corresponding to the 
        /// trespassing side gets activated. When everything is fine, the object DOES NOT get deactivated. So you have to manually
        /// deactivate the objects by hand (like the <see cref="BodiesSkeletalsManagerAdvanced"/> does)
        /// Gameobject order is left, top, right, bottom.
        /// Can be null
        /// </summary>
        [Tooltip("GameObjects to activate or deactivate when one of the skeletons reaches the kinect tracking area limits. Gameobject order is left, top, right, bottom.")]
        public GameObject[] RedAlerts;

        #endregion

        #region BodyAvatarer members

        /// <summary>
        /// Coroutine to connect to an existing tracking service, waiting for its appropriate mode to start
        /// and start the Body Data Provider for the avatar of interest
        /// </summary>
        /// <returns></returns>
        public override IEnumerator TrackingServiceConnect()
        {
            //wait for tracking service connection and tracking
            while (!TrackingServiceManagerAdvanced.Instance.IsStreamingSkeletons)
                yield return new WaitForSeconds(0.1f);

            //create the body provider, waiting for it to begin
            SceneDataProvider sceneDataProvider = null;

            while ((sceneDataProvider = TrackingServiceManagerAdvanced.Instance.StartSceneDataProvider(SceneStreamerInfoId, SceneStreamingMode)) == null)
                yield return new WaitForSeconds(0.1f);

            m_bodyDataProvider = new BodyDataProvider(sceneDataProvider, BodyId);

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Skeletal Body Avatarer for Body Id {0} - Connected to Tracking Service and Initialized", BodyId);
            }

            yield break;
        }

        /// <summary>
        /// Updates the avatar, given new body data
        /// </summary>
        /// <param name="bodyData">New data with which the avatar should be updated</param>
        public override void RefreshAvatar(TrackingServiceBodyData bodyData)
        {
            //if there are red alerts
            if (RedAlerts != null && bodyData != null)
            {
                //see if the skeleton has trespassed one of the edges, and if it is so, activate the corresponding objects
                var clipEdges = bodyData.ClippedEdges;

                if (RedAlerts.Length >= 1 && ((clipEdges & TrackingServiceSceneClippedEdges.Left) == TrackingServiceSceneClippedEdges.Left))
                    RedAlerts[0].SetActive(true);
                if (RedAlerts.Length >= 2 && ((clipEdges & TrackingServiceSceneClippedEdges.Top) == TrackingServiceSceneClippedEdges.Top))
                    RedAlerts[1].SetActive(true);
                if (RedAlerts.Length >= 4 && ((clipEdges & TrackingServiceSceneClippedEdges.Right) == TrackingServiceSceneClippedEdges.Right))
                    RedAlerts[2].SetActive(true);
                if (RedAlerts.Length >= 3 && ((clipEdges & TrackingServiceSceneClippedEdges.Bottom) == TrackingServiceSceneClippedEdges.Bottom))
                    RedAlerts[3].SetActive(true);                
            }

            base.RefreshAvatar(bodyData);
        }

        #endregion
    }
}

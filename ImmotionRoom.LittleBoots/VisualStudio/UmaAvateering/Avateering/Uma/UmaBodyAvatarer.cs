namespace ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Uma
{
    using ImmotionAR.ImmotionRoom.LittleBoots.Avateering;
    using ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Uma.Generators;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.DataSourcesManagement;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Avateers a body using a UMA2 compatible avatar
    /// </summary>
    public class UmaBodyAvatarer : BodyAvatarer
    {
        #region Unity public properties

        /// <summary>
        /// Reference to the generator responsible to create the UMA-compatible avatar
        /// </summary>
        [Tooltip("Reference to the element responsible for Creation of the avatars")]
        public UmaBodyGenerator AvatarGenerator;

        /// <summary>
        /// True if colliders have to be attached to avatar hands and feet; false otherwise
        /// </summary>
        [Tooltip("True if colliders have to be attached to amanda hands and feet; false otherwise")]
        public bool AttachColliders;

        /// <summary>
        /// True to make the system try to make the body of the avatar the most similar possible to the user's body.
        /// False to use avatar prefab as is. Can be used only on UMA avatars (not UMA-like models)
        /// </summary>
        [Tooltip("True to make the system try to make the body of the avatar the most similar possible to the user's body. False to use avatar prefab as is. Can be used only on UMA avatars (not UMA-like models)")]
        public bool CalibratePhysiognomy;

        /// <summary>
        /// Make the avatar skinned mesh to be flagged with the flag UpdateWhenOffscreen, that makes the mesh rendered always, even when not seen from a camera.
        /// The advice is to let this flag on if avatar is used in VR, because of some bugs in UMA bounding box estimates
        /// </summary>
        [Tooltip("Make the avatar skinned mesh to be flagged with the flag UpdateWhenOffscreen, that makes the mesh rendered always, even when not seen from a camera. The advice is to let this flag on if avatar is used in VR, because of some bugs in UMA bounding box estimates")]
        public bool IgnoreBoundsCheck;

        /// <summary>
        /// Make the avatar hands to stay fixed at a zero orientation pose. This is useful to prevent all detection glitches on avatar hands
        /// </summary>
        [Tooltip("Make the avatar hands to stay fixed at a zero orientation pose. This is useful to prevent all detection glitches on avatar feet. Use only if tracking glitches make interaction unusable otherwise")]
        public bool LockHandsPose;

        /// <summary>
        /// Make the avatar feet to stay fixed at a zero orientation pose. This is useful to prevent all detection glitches on avatar feet
        /// </summary>
        [Tooltip("Make the avatar feet to stay fixed at a zero orientation pose. This is useful to prevent all detection glitches on avatar feet. Use only if tracking glitches make interaction unusable otherwise")]
        public bool LockFeetPose;

        #endregion

        #region BodyAvatarer members

        /// <summary>
        /// Transform object of the actual avatar
        /// (usually it is a child of this object)
        /// </summary>
        public override Transform BodyTransform
        {
            get
            {
                return transform.GetChild(0);
            }
        }

        /// <summary>
        /// Coroutine to connect to an existing tracking service, waiting for its appropriate mode to start
        /// and start the Body Data Provider for the avatar of interest
        /// </summary>
        /// <returns></returns>
        public override IEnumerator TrackingServiceConnect()
        {
            //wait for tracking service connection and tracking
            while (!TrackingServiceManagerBasic.Instance.IsTracking)
                yield return new WaitForSeconds(0.1f);

            //create the body provider, waiting for it to begin
            SceneDataProvider sceneDataProvider = null;

            while ((sceneDataProvider = TrackingServiceManagerBasic.Instance.StartSceneDataProvider()) == null)
                yield return new WaitForSeconds(0.1f);

            m_bodyDataProvider = new BodyDataProvider(sceneDataProvider, BodyId);

            if (Log.IsDebugEnabled)
            {
                Log.Debug("UMA Body Avatarer for Body Id {0} - Connected to Tracking Service and Initialized", BodyId);
            }

            yield break;
        }

        /// <summary>
        /// Coroutine to create and initialize the appropriate <see cref="Avatarer"/> object, connecting it to the tracking service's
        /// stream
        /// </summary>
        /// <returns></returns>
        public override IEnumerator CreateAvatareer()
        {
            //create the actual avatar (using the provided generator) and put it as child of current object
            GameObject umaInstance;
            IDictionary<UmaJointTypes, string> jointsMappingStrings;
            IDictionary<UmaJointTypes, Quaternion> jointsTRotationsMappings;

            AvatarGenerator.GenerateAvatar(out umaInstance, out jointsMappingStrings, out jointsTRotationsMappings); 
            umaInstance.transform.SetParent(transform, false);

            while (umaInstance.transform.childCount <= 0) // wait for uma creation process to finish (it will require some frame), before going on. Otherwise we can't get transform references
                yield return new WaitForSeconds(0.05f);

            //set uma root transform to 1 (otherwise global scaling won't work)
            Dictionary<UmaJointTypes, Transform> jointMappings = UmaBodyGenerator.GetJointMappingsTransforms(umaInstance, jointsMappingStrings);
            jointMappings[UmaJointTypes.Root].localScale = Vector3.one;

            //create the avateerer
            UmaAvatarer umaAvatarer = new UmaAvatarer(umaInstance, jointMappings, jointsTRotationsMappings, AttachColliders, ShadowsEnabled, IgnoreBoundsCheck, LockHandsPose, LockFeetPose);
            m_avatarer = umaAvatarer;

            //if it is required to modify the avatar to make it similar to user body, init it using the first body read from the provider
            if (CalibratePhysiognomy)
            {
                IUmaPhysioMatchingBridge umaBridge = AvatarGenerator.GetUmaMatchingBridge(umaInstance);//get the helper object to modify UMA avatar
                UmaPhysioMatcher avatarPhysioMatcher = new UmaPhysioMatcher(jointMappings, umaBridge); //create the helper object to modify the avatar accordingly to the user body characteristics
                
                //ask the matcher to assign this features to the avatar.
                //Notice that we perform this operations in the right order, because, for example, changing the avatar height will surely
                //change its arm length (all the avatar will be scaled)
                yield return StartCoroutine(avatarPhysioMatcher.MatchFeature(PhysioMatchingFeatures.Height, umaAvatarer.GetFeatureMeasure(PhysioMatchingFeatures.Height, m_bodyDataProvider.LastBody)));
                yield return StartCoroutine(avatarPhysioMatcher.MatchFeature(PhysioMatchingFeatures.LegsLength, umaAvatarer.GetFeatureMeasure(PhysioMatchingFeatures.LegsLength, m_bodyDataProvider.LastBody)));
                yield return StartCoroutine(avatarPhysioMatcher.MatchFeature(PhysioMatchingFeatures.ShouldersWidth, umaAvatarer.GetFeatureMeasure(PhysioMatchingFeatures.ShouldersWidth, m_bodyDataProvider.LastBody)));
                yield return StartCoroutine(avatarPhysioMatcher.MatchFeature(PhysioMatchingFeatures.ArmsLength, umaAvatarer.GetFeatureMeasure(PhysioMatchingFeatures.ArmsLength, m_bodyDataProvider.LastBody)));
                yield return StartCoroutine(avatarPhysioMatcher.MatchFeature(PhysioMatchingFeatures.ForeArmsLength, umaAvatarer.GetFeatureMeasure(PhysioMatchingFeatures.ForeArmsLength, m_bodyDataProvider.LastBody)));

                m_avatarer.Initialize();
            }    
            //else, perform a simple initialization of the avatar as-is
            else
                m_avatarer.Initialize();

            if (Log.IsDebugEnabled)
            {
                Log.Debug("UMA Body Avatarer for Body Id {0} - Created UMA Avatar", BodyId);
            }

            yield break;
        }

        #endregion

    }
}

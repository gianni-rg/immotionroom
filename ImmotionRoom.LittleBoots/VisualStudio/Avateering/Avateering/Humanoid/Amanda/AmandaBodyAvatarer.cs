namespace ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Humanoid.Amanda
{
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
    /// Avateers a body using Amanda avatar
    /// </summary>
    public class AmandaBodyAvatarer : BodyAvatarer
    {
        #region Unity public properties

        /// <summary>
        /// Reference to Amanda prefab
        /// </summary>
        [Tooltip("Reference to Amanda prefab")]
        public GameObject AmandaModel;

        /// <summary>
        /// True if colliders have to be attached to amanda hands and feet; false otherwise
        /// </summary>
        [Tooltip("True if colliders have to be attached to amanda hands and feet; false otherwise")]
        public bool AttachColliders;

        /// <summary>
        /// Reference to Amanda Android Material
        /// </summary>
        [Tooltip("Reference to Amanda material to be used on Android (PC one is too heavy). Leave null if substitution is not required")]
        public Material AndroidAmandaMaterial;

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
                Log.Debug("Amanda Body Avatarer for Body Id {0} - Connected to Tracking Service and Initialized", BodyId);
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
            //instantiate amanda prefab
            GameObject amandaInstance = Instantiate<GameObject>(AmandaModel);
            amandaInstance.transform.SetParent(transform, false);

//#if UNITY_ANDROID
            //standard amanda material is very buggy on Android...should be replaced
            if(Application.platform == RuntimePlatform.Android && AndroidAmandaMaterial != null)
                amandaInstance.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material = AndroidAmandaMaterial;
//#endif

            m_avatarer = new AmandaAvatarer(amandaInstance, AttachColliders, ShadowsEnabled);
            m_avatarer.Initialize(); //Amanda do not need an initialization using first user pose, so call simply Initialize

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Amanda Body Avatarer for Body Id {0} - Created Amanda Avatar", BodyId);
            }

            yield break;
        }

        #endregion
    }
}

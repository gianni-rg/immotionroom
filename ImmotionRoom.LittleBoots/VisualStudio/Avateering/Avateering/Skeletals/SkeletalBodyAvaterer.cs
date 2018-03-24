namespace ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Skeletals
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
    /// Avateers a body using a bare-bones skeleton
    /// </summary>
    public class SkeletalBodyAvaterer : BodyAvatarer
    {
        #region Unity public properties

        /// <summary>
        /// Material to draw the joints with
        /// </summary>
        [Tooltip("The material to draw the joints with")]
        public Material JointsMaterial;

        /// <summary>
        /// Material to draw joints lines with
        /// </summary>
        [Tooltip("The material to draw the limbs (the lines connecting the joints) with")]
        public Material LimbsMaterial;

        /// <summary>
        /// Positive color to be used in the joint drawing.
        /// </summary>
        [Tooltip("Positive color, to be used for joints with 100% confidence")]
        public Color PositiveColor;

        /// <summary>
        /// Negative color to be used in the joint drawing.
        /// This is the one used for low confidence joints
        /// </summary>
        [Tooltip("Negative color, to be used for joints with 0% confidence")]
        public Color NegativeColor;

        /// <summary>
        /// Color to be used to draw the bones of the skeleton
        /// </summary>
        [Tooltip("Limbs color, to be used to draw the skeleton bones")]
        public Color LimbsColor;

        /// <summary>
        /// Radius of the sphere representing each drawn joint
        /// </summary>
        [Tooltip("Radius of the sphere representing each drawn joint")]
        public float JointSphereRadius;

        /// <summary>
        /// Thickness of lines connecting consecutive joints
        /// </summary>
        [Tooltip("Thickness of lines representing the limbs")]
        public float ConnectingLinesThickness;

        /// <summary>
        /// True to add colliders for hands and feet, false otherwise
        /// </summary>
        [Tooltip("True to add colliders for hands and feet, false otherwise")]
        public bool AddColliders;

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
                Log.Debug("Skeletal Body Avatarer for Body Id {0} - Connected to Tracking Service and Initialized", BodyId);
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
            m_avatarer = new SkeletalAvatarer(gameObject, JointsMaterial, LimbsMaterial, PositiveColor, NegativeColor, LimbsColor, JointSphereRadius, ConnectingLinesThickness, AddColliders, ShadowsEnabled);
            m_avatarer.Initialize(); //Skeletals do not need an initialization using first user pose, so call simply Initialize

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Skeletal Body Avatarer for Body Id {0} - Created Skeleton", BodyId);
            }

            yield break;
        }

        #endregion

    }
}

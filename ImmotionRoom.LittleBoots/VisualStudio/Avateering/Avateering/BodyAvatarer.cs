namespace ImmotionAR.ImmotionRoom.LittleBoots.Avateering
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.DataSourcesManagement;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;

    /// <summary>
    /// Base class for all avateering behaviours
    /// </summary>
    public abstract class BodyAvatarer : MonoBehaviour,
        IBodyAvaterer
    {
        #region Public Unity Properties

        /// <summary>
        /// ID of the body data we want to visualize
        /// </summary>
        [Tooltip("ID of the body we want to draw as avatar")]
        public ulong BodyId;

        /// <summary>
        /// True if the body has to be translated as user's world position, false otherwise.
        /// Translation position is evaluated in a way depending on implementation of the avatar
        /// </summary>
        [Tooltip("True to track avatar position, false otherwise")]
        public bool TrackPosition;

        /// <summary>
        /// True if the body has to cast/receive shadows, false otherwise
        /// </summary>
        [Tooltip("True if the body has to cast/receive shadows, false otherwise")]
        public bool ShadowsEnabled;

        #endregion

        #region Private fields

        /// <summary>
        /// Object that performs actual avateering.
        /// Actual operation are performed there and not inside class methods to facilitate code obfuscation
        /// </summary>
        protected Avatarer m_avatarer;

        /// <summary>
        /// Provider of body data
        /// </summary>
        protected BodyDataProvider m_bodyDataProvider;

        /// <summary>
        /// True if start has been called on this behaviour, false otherwise
        /// </summary>
        protected bool m_started;

        /// <summary>
        /// True if this object is initialized, false otherwise
        /// </summary>
        protected bool m_initialized;

        /// <summary>
        /// Dictionary of joint poses to be injected in the avateering process
        /// </summary>
        protected Dictionary<TrackingServiceBodyJointTypes, Quaternion> m_injectedJointPoses;

        #endregion

        #region Behaviour methods

        protected void Awake()
        {
            m_initialized = false;
            m_injectedJointPoses = new Dictionary<TrackingServiceBodyJointTypes, Quaternion>();

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Body Avatarer for Body Id {0} - Awake", BodyId);
            }

        }

        protected void Start()
        {
            StartCoroutine(AvateeringStart());

            m_started = true;

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Body Avatarer for Body Id {0} - Start", BodyId);
            }
        }

        protected void OnDestroy()
        {
            if (m_bodyDataProvider != null && m_bodyDataProvider.ActualSceneDataProvider != null)
                m_bodyDataProvider.ActualSceneDataProvider.Dispose();

            StopAllCoroutines(); //we started coroutines during initialization

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Body Avatarer for Body Id {0} - Destroyed", BodyId);
            }
        }

        protected void Update()
        {
            if (m_initialized)
                RefreshAvatar(m_bodyDataProvider.LastBody);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Starts connection with Tracking Service and then initializes avateering and then triggers its update
        /// </summary>
        /// <returns></returns>
        IEnumerator AvateeringStart()
        {
            //start connection with tracking service 
            yield return StartCoroutine(TrackingServiceConnect());

            //initialize avateering
            yield return StartCoroutine(CreateAvatareer());

            //trigger initialization
            m_initialized = true;

            yield break;
        }

        /// <summary>
        /// Coroutine to connect to an existing tracking service, waiting for its appropriate mode to start
        /// and start the Body Data Provider for the avatar of interest
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator TrackingServiceConnect();

        /// <summary>
        /// Coroutine to create and initialize the appropriate <see cref="Avatarer"/> object, connecting it to the tracking service's
        /// stream
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator CreateAvatareer();

        #endregion

        #region IBodyAvatarer members

        /// <summary>
        /// Get if avatar has been fully constructed and initialized with actual body data from TrackingService
        /// </summary>
        public bool InitializedWithBodyData
        {
            get
            {
                return m_avatarer != null && m_avatarer.InitializedWithBodyData;
            }
        }

        /// <summary>
        /// Transform object of the actual avatar
        /// (usually it is a child of this object)
        /// </summary>
        public abstract Transform BodyTransform
        {
            get;
        }

        /// <summary>
        /// Gets the root joint transform of the controlled avatar object, in Unity frame of reference.
        /// Root corresponds to the main joint of the avatar (usually spine hip or spine mid point), the father all of others
        /// </summary>
        public Transform BodyRootJoint
        {
            get
            {
                if (m_avatarer != null)
                    return m_avatarer.BodyRootJoint;
                else
                    return null;
            }
        }

        /// <summary>
        /// Gets the position of the avatar corresponding to the mean points of the avatar's eyes, in Unity world coordinates.
        /// This is the position where it is ideal to have the VR camera attached to the avatar, in VR applications.
        /// Returns 0 if the avatar is still not ready
        /// </summary>
        public Vector3 BodyEyesCameraPosition
        {
            get
            {
                if (m_avatarer != null)
                    return m_avatarer.BodyEyesCameraPosition;
                else
                    return Vector3.zero;
            }
        }

        /// <summary>
        /// Gets or sets values in the dictionary of joint poses that have to be forced inside the avateering skeleton.
        /// This is useful, e.g., to force values on joints that have stable measurements read from external sensors.
        /// The provided rotation MUST BE expressed in Unity world frame of reference.
        /// At the moment the only overridable value is TrackingServiceBodyJointTypes.Neck
        /// </summary>
        public virtual Dictionary<TrackingServiceBodyJointTypes, Quaternion> InjectedJointPoses
        {
            get
            {
                return m_injectedJointPoses;
            }
            set
            {
                m_injectedJointPoses = value;
            }
        }

        ///// <summary>
        ///// Creates the avateering object, initializing all internal structures
        ///// </summary>
        //public void CreateAvatar()
        //{
        //    m_avatarer.Initialize();
        //}

        ///// <summary>
        ///// Creates the avateering object, initializing all internal structures
        ///// </summary>
        ///// <param name="bodyData">First frame skeleton data with which the avatar should be initialized</param>
        ///// <param name="physioMatcher">Element responsible of matching the avatar characteristics to user's data. Null if this kind of element is not necessary</param>
        //public void CreateAvatar(TrackingServiceBodyData bodyData, IAvatarPhysioMatcher physioMatcher = null)
        //{
        //    m_avatarer.Initialize(bodyData, physioMatcher, TrackPosition);
        //}

        /// <summary>
        /// Updates the avatar, given new body data
        /// </summary>
        /// <param name="bodyData">New data with which the avatar should be updated</param>
        public virtual void RefreshAvatar(TrackingServiceBodyData bodyData)
        {
            m_avatarer.Update(bodyData, m_injectedJointPoses, TrackPosition);
        }

        /// <summary>
        /// Resets the avatar manager, making it re-create the avatar from scratch to track a new user body
        /// </summary>
        /// <param name="newId">Id of the new body to track</param>
        public virtual void SetNewBodyId(ulong newId)
        {
            //if object has not even started, not only the reset is useless, but it's also dangerous because
            //it makes AvateeringStart to be called twice and so everything works really bad.
            //So, if object has been started already, reset everything 
            if (m_started)
            {
                //wipe everything and restart

                StopAllCoroutines();
                m_avatarer = null;
                m_initialized = false;
                BodyId = newId;
                m_injectedJointPoses = new Dictionary<TrackingServiceBodyJointTypes, Quaternion>();

                foreach (Transform child in transform) //delete all children (actual avatars)
                    if (child.GetInstanceID() != transform.GetInstanceID())
                        Destroy(child.gameObject);

                if (m_bodyDataProvider != null && m_bodyDataProvider.ActualSceneDataProvider.IsStillValid) //if we're already connected to Tracking Service
                    m_bodyDataProvider.ActualSceneDataProvider.Dispose();

                StartCoroutine(AvateeringStart()); //perform all tracking service connection and avateering initialization operations from scratch

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Body Avatarer for Body Id {0} - Reset requested", BodyId);
                }
            }
            //otherwise just set the new Id and let things go naturally from Start method
            else
                BodyId = newId;
        }

        /// <summary>
        /// Get the transform representing the desired human joint.
        /// If there is not an avatar joint representing exactly the desired joint, the most accurate representation possible will be returned.
        /// If the avatar is not initialized, null is returned
        /// </summary>
        /// <param name="jointType">Joint Type of Interest</param>
        /// <returns>Transform, inside Unity scene, representing the desired joint type</returns>
        public Transform GetJointTransform(TrackingServiceBodyJointTypes jointType)
        {
            if (m_avatarer != null)
                return m_avatarer.GetJointTransform(jointType);
            else
                return null;
        }

        #endregion
    }
}

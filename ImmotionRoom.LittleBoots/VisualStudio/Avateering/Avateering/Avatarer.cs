namespace ImmotionAR.ImmotionRoom.LittleBoots.Avateering
{
    using ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Base class for all objects the performs the actual avateering operations, given user skeleton data
    /// </summary>
    public abstract class Avatarer
    {
        /// <summary>
        /// True if Initialized has already been called, false otherwise
        /// </summary>
        protected bool m_initializedWithBodyData;

        /// <summary>
        /// Get if avatar has been fully constructed and initialized with actual body data from TrackingService
        /// </summary>
        public bool InitializedWithBodyData
        {
            get
            {
                return m_initializedWithBodyData;
            }
        }

        /// <summary>
        /// Gets the root joint transform of the controlled avatar object, in Unity frame of reference.
        /// Root corresponds to the main joint of the avatar (usually spine hip or spine mid point), the father all of others
        /// </summary>
        protected internal abstract Transform BodyRootJoint
        {
            get;
        }

        /// <summary>
        /// Gets the position of the avatar corresponding to the mean points of the avatar's eyes, in Unity world coordinates.
        /// This is the position where it is ideal to have the VR camera attached to the avatar, in VR applications
        /// </summary>
        protected internal abstract Vector3 BodyEyesCameraPosition
        {
            get;
        }

        /// <summary>
        /// Get the transform representing the desired human joint.
        /// If there is not an avatar joint representing exactly the desired joint, the most accurate representation possible will be returned
        /// </summary>
        /// <param name="jointType">Joint Type of Interest</param>
        /// <returns>Transform, inside Unity scene, representing the desired joint type</returns>
        public abstract Transform GetJointTransform(TrackingServiceBodyJointTypes jointType);

        ///// <summary>
        ///// Updates the avatar, given new body data
        ///// </summary>
        ///// <param name="bodyData">New data with which the avatar should be updated</param>
        //void RefreshAvatar(TrackingServiceBodyData bodyData);
 
        /// <summary>
        /// Initializes the avateering object, initializing all internal structures, using default user data
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Updates the avatar, given new body data
        /// </summary>
        /// <param name="bodyData">New data with which the avatar should be updated</param>
        /// <param name="injectedPoses">Poses to inject inside the final avatar, expressed as rotations in Unity world frame of reference. For this joints, the value read from the bodyData gets ignored, and the one from this dictionary gets used.</param>
        /// <param name="trackPosition">True if the avatar should match user position in world space, false to track only pose</param>
        public abstract void Update(TrackingServiceBodyData bodyData, Dictionary<TrackingServiceBodyJointTypes, Quaternion> injectedPoses, bool trackPosition = false);
    }
}

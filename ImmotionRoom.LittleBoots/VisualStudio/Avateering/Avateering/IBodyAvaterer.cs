namespace ImmotionAR.ImmotionRoom.LittleBoots.Avateering
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model;

    /// <summary>
    /// Interface with common methods of all behaviours offering Avateering capabilities
    /// </summary>
    internal interface IBodyAvaterer
    {
        ///// <summary>
        ///// Creates the avateering object, initializing all internal structures
        ///// </summary>
        //void CreateAvatar();

        ///// <summary>
        ///// Creates the avateering object, initializing all internal structures
        ///// </summary>
        ///// <param name="bodyData">First frame skeleton data with which the avatar should be initialized</param>
        ///// <param name="physioMatcher">Element responsible of matching the avatar characteristics to user's data. Null if this kind of element is not necessary</param>
        //void CreateAvatar(TrackingServiceBodyData bodyData, IAvatarPhysioMatcher physioMatcher = null);

        /// <summary>
        /// Get if avatar has been fully constructed and initialized with actual body data from TrackingService
        /// </summary>
        bool InitializedWithBodyData
        {
            get;
        }

        /// <summary>
        /// Transform object of the actual avatar
        /// (usually it is a child of this object)
        /// </summary>
        Transform BodyTransform
        {
            get;
        }

        /// <summary>
        /// Gets the root joint transform of the controlled avatar object, in Unity frame of reference.
        /// Root corresponds to the main joint of the avatar (usually spine hip or spine mid point), the father all of others
        /// </summary>
        Transform BodyRootJoint
        {
            get;
        }

        /// <summary>
        /// Gets the position of the avatar corresponding to the mean points of the avatar's eyes, in Unity world coordinates.
        /// This is the position where it is ideal to have the VR camera attached to the avatar, in VR applications
        /// </summary>
        Vector3 BodyEyesCameraPosition
        {
            get;
        }
            
        /// <summary>
        /// Gets or sets values in the dictionary of joint poses that have to be forced inside the avateering skeleton.
        /// This is useful, e.g., to force values on joints that have stable measurements read from external sensors.
        /// The provided rotation MUST BE expressed in Unity world frame of reference.
        /// At the moment the only overridable value is TrackingServiceBodyJointTypes.Neck
        /// </summary>
        Dictionary<TrackingServiceBodyJointTypes, Quaternion> InjectedJointPoses
        {
            get;
            set;
        }

        /// <summary>
        /// Updates the avatar, given new body data
        /// </summary>
        /// <param name="bodyData">New data with which the avatar should be updated</param>
        void RefreshAvatar(TrackingServiceBodyData bodyData);

        /// <summary>
        /// Resets the avatar manager, making it re-create the avatar from scratch to track a new user body
        /// </summary>
        /// <param name="newId">Id of the new body to track</param>
        void SetNewBodyId(ulong newId);

        /// <summary>
        /// Get the transform representing the desired human joint.
        /// If there is not an avatar joint representing exactly the desired joint, the most accurate representation possible will be returned
        /// If the avatar is not initialized, null is returned
        /// </summary>
        /// <param name="jointType">Joint Type of Interest</param>
        /// <returns>Transform, inside Unity scene, representing the desired joint type</returns>
        Transform GetJointTransform(TrackingServiceBodyJointTypes jointType);
    }
}

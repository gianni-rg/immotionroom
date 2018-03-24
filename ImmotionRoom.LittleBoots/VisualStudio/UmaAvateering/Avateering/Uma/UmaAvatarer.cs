namespace ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Uma
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using ImmotionAR.ImmotionRoom.LittleBoots.Avateering;
    using ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model;
    using UnityEngine;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Tools;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Collisions;

    /// <summary>
    /// Class to manage an Uma-like Avatar of the user body
    /// </summary>
    /// <remarks>
    /// The Avatar could be an actual UMA2 model or a model with the same skeletal structure of UMA avatars
    /// </remarks>
    internal class UmaAvatarer : Avatarer
    {
        #region Constants

        private static readonly Vector3 FootColliderCenter = new Vector3(-0.1f, -0.005f, 0.06f);
        private static readonly Vector3 FootColliderSize = new Vector3(0.289963f, 0.121f, 0.11f);
        private static readonly Vector3 HandsColliderCenter = new Vector3(-0.099f, 0f, -0.02f);
        private static readonly Vector2 HandsColliderRadiusHeight = new Vector2(0.07f, 0.24f);

        #endregion

        #region Private fields

        /// <summary>
        /// Avatar gameobject
        /// </summary>
        private GameObject m_avatarGo;

        /// <summary>
        /// True to keep hands rotation fixed at a zero value to increase usability of the system
        /// </summary>
        private bool m_lockHandsPose;

        /// <summary>
        /// True to keep feet rotation fixed at a zero value to increase usability of the system
        /// </summary>
        private bool m_lockFeetPose;

        /// <summary>
        /// Mappings from joint type to transform corresponding to this joint type. 
        /// This dictionary allows this class to handle avatars with joint names different from the standard ones
        /// </summary>
        private IDictionary<UmaJointTypes, Transform> m_jointsMapping;

        /// <summary>
        /// Mappings from joint type to the rotation, in global coordinates, that takes them to the standard avateering T pose
        /// This dictionary allows this class to handle avatars with joint frame of references different from the standard ones
        /// </summary>
        private IDictionary<UmaJointTypes, Quaternion> m_jointsGlobalTRotationMapping;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a Uma Avaterer, so the provided avatar can be moved accordingly to the actual player movements
        /// </summary>
        /// <param name="avatar">The avatar object to be managed. In case of UMA Avatars, this is the root object containing UmaDynamicAvatar component</param>
        /// <param name="jointsMapping">Mappings from joint type to transform corresponding to this joint type. This dictionary allows this class to handle avatars with joint names different from the standard ones</param>
        /// <param name="jointsGlobalTRotationMapping">Mappings from joint type to the rotation, in global coordinates, that takes them to the standard avateering T pose</param>
        /// <param name="addColliders">True if hands/feet colliders have to be attached to amanda, false otherwise</param>
        /// <param name="shadowsEnabled">True if the bodies have to cast/receive shadows, false otherwise</param>
        /// <param name="ignoreBoundsCheck">Make the avatar skinned mesh to be flagged with the flag UpdateWhenOffscreen, that makes the mesh rendered always, even when not seen from a camera</param>
        /// <param name="lockHandsPose">Make the avatar hands to stay fixed at a zero orientation. This is useful to prevent all detection glitches on avatar limbs</param>
        /// <param name="lockFeetPose">Make the avatar feet to stay fixed at a zero orientation. This is useful to prevent all detection glitches on avatar limbs</param>
        internal UmaAvatarer(GameObject avatar, IDictionary<UmaJointTypes, Transform> jointsMapping, IDictionary<UmaJointTypes, Quaternion> jointsGlobalTRotationMapping, bool addColliders, bool shadowsEnabled, bool ignoreBoundsCheck, bool lockHandsPose, bool lockFeetPose)
        {
            m_avatarGo = avatar;
            m_jointsMapping = jointsMapping;
            m_jointsGlobalTRotationMapping = jointsGlobalTRotationMapping;
            m_initializedWithBodyData = false;
            m_lockHandsPose = lockHandsPose;
            m_lockFeetPose = lockFeetPose;

            if (addColliders)
                AddInteractionsCollidersToAvatar();

            //if required, disable shadows from all renderable objects
            if (!shadowsEnabled)
            {
                Renderer[] renderables = avatar.transform.GetComponentsInChildren<Renderer>();

                foreach (Renderer renderable in renderables)
                {
                    renderable.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderable.receiveShadows = false;
                }
            }

            //if required, show always the avatars, even when not seen by a camera
            if (ignoreBoundsCheck)
            {
                SkinnedMeshRenderer[] renderables = avatar.transform.GetComponentsInChildren<SkinnedMeshRenderer>();

                foreach (SkinnedMeshRenderer renderable in renderables)
                {
                    renderable.updateWhenOffscreen = true;
                }
            }

            if (Log.IsDebugEnabled)
            {
                Log.Debug("UMA Avatarer created");
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Adds interaction colliders to hands and feets to current avatar
        /// </summary>
        private void AddInteractionsCollidersToAvatar()
        {
            //left foot
            GameObject colliderGo = new GameObject(AvatarCollidersProps.LeftFootColliders.ObjectName);
            //colliderGo.tag = AvatarCollidersProps.LeftFootColliders.ObjectTag;
            BoxCollider boxCollider = colliderGo.AddComponent<BoxCollider>();
            boxCollider.center = FootColliderCenter;
            boxCollider.size = FootColliderSize;
            boxCollider.isTrigger = true;
            Rigidbody colliderRb = colliderGo.AddComponent<Rigidbody>(); //add rigidbody too, or collision detection won't work
            colliderRb.isKinematic = false;
            colliderRb.useGravity = false;
            colliderGo.transform.SetParent(m_jointsMapping[UmaJointTypes.LeftFoot], false);
            colliderGo.transform.localRotation = Quaternion.AngleAxis(331, Vector3.up); //hack

            //right foot
            colliderGo = new GameObject(AvatarCollidersProps.RightFootColliders.ObjectName);
            //colliderGo.tag = AvatarCollidersProps.RightFootColliders.ObjectTag;
            boxCollider = colliderGo.AddComponent<BoxCollider>();
            boxCollider.center = FootColliderCenter;
            boxCollider.size = FootColliderSize;
            boxCollider.isTrigger = true;
            colliderRb = colliderGo.AddComponent<Rigidbody>(); //add rigidbody too, or collision detection won't work
            colliderRb.isKinematic = false;
            colliderRb.useGravity = false;
            colliderGo.transform.SetParent(m_jointsMapping[UmaJointTypes.RightFoot], false);
            colliderGo.transform.localRotation = Quaternion.AngleAxis(331, Vector3.up); //hack

            //left hand
            colliderGo = new GameObject(AvatarCollidersProps.LeftHandColliders.ObjectName);
            //colliderGo.tag = AvatarCollidersProps.LeftHandColliders.ObjectTag;
            CapsuleCollider capsuleCollider = colliderGo.AddComponent<CapsuleCollider>();
            capsuleCollider.center = HandsColliderCenter;
            capsuleCollider.radius = HandsColliderRadiusHeight.x;
            capsuleCollider.height = HandsColliderRadiusHeight.y;
            capsuleCollider.direction = 0;
            capsuleCollider.isTrigger = true;
            colliderRb = colliderGo.AddComponent<Rigidbody>(); //add rigidbody too, or collision detection won't work
            colliderRb.isKinematic = false;
            colliderRb.useGravity = false;
            colliderGo.transform.SetParent(m_jointsMapping[UmaJointTypes.LeftHand], false);

            //right hand
            colliderGo = new GameObject(AvatarCollidersProps.RightHandColliders.ObjectName);
            //colliderGo.tag = AvatarCollidersProps.RightHandColliders.ObjectTag;
            capsuleCollider = colliderGo.AddComponent<CapsuleCollider>();
            capsuleCollider.center = HandsColliderCenter;
            capsuleCollider.radius = HandsColliderRadiusHeight.x;
            capsuleCollider.height = HandsColliderRadiusHeight.y;
            capsuleCollider.direction = 0;
            capsuleCollider.isTrigger = true;
            colliderRb = colliderGo.AddComponent<Rigidbody>(); //add rigidbody too, or collision detection won't work
            colliderRb.isKinematic = false;
            colliderRb.useGravity = false;
            colliderGo.transform.SetParent(m_jointsMapping[UmaJointTypes.RightHand], false);
        }

        #endregion

        #region Avatarer members

        /// <summary>
        /// Gets the root joint transform of the controlled avatar object, in Unity frame of reference.
        /// Root corresponds to the main joint of the avatar (usually spine hip or spine mid point), the father all of others.
        /// This method can be used also to check if Initialized is called, because until full avatar initialization, it will always return null
        /// </summary>
        protected override Transform BodyRootJoint
        {
            get
            {
                return m_jointsMapping[UmaJointTypes.Hips];
            }
        }

        /// <summary>
        /// Gets the position of the avatar corresponding to the mean points of the avatar's eyes, in Unity world coordinates.
        /// This is the position where it is ideal to have the VR camera attached to the avatar, in VR applications
        /// </summary>
        protected override Vector3 BodyEyesCameraPosition
        {
            get
            {
                //assign the median position of the two eyes, if mappings are present
                if (m_jointsMapping.ContainsKey(UmaJointTypes.LeftEye) && m_jointsMapping.ContainsKey(UmaJointTypes.RightEye))
                    return (m_jointsMapping[UmaJointTypes.LeftEye].position + m_jointsMapping[UmaJointTypes.RightEye].position) / 2; //return eyes center
                //else, assign head position
                else
                    return m_jointsMapping[UmaJointTypes.Head].position;
            }
        }

        /// <summary>
        /// Get the transform representing the desired human joint.
        /// If there is not an avatar joint representing exactly the desired joint, the most accurate representation possible will be returned
        /// </summary>
        /// <param name="jointType">Joint Type of Interest</param>
        /// <returns>Transform, inside Unity scene, representing the desired joint type</returns>
        public override Transform GetJointTransform(TrackingServiceBodyJointTypes jointType)
        {
            return ProtectedGetJointTransform(jointType);
        }

        /// <summary>
        /// Initializes the avateering object, initializing all internal structures, using default user data
        /// </summary>
        public override void Initialize()
        {
            //do nothing, the avatar has been already created

            if (Log.IsDebugEnabled)
            {
                Log.Debug("UMA Avatarer initialized");
            }
        }

        /// <summary>
        /// Updates the avatar, given new body data
        /// </summary>
        /// <param name="bodyData">New data with which the avatar should be updated</param>
        /// <param name="injectedPoses">Poses to inject inside the final avatar, expressed as rotations in Unity world frame of reference. For this joints, the value read from the bodyData gets ignored, and the one from this dictionary gets used.</param>
        /// <param name="trackPosition">True if the avatar should match user position in world space, false to track only pose</param>
        public override void Update(TrackingServiceBodyData bodyData, Dictionary<TrackingServiceBodyJointTypes, Quaternion> injectedPoses, bool trackPosition = false)
        {
            if (bodyData == null)
                return;

            if (!m_initializedWithBodyData)
                m_initializedWithBodyData = true;

            ProtectedUpdate(bodyData, injectedPoses, trackPosition);
        }

        #endregion      

        #region Protected methods

        /// <summary>
        /// Updates the avatar, given new body data
        /// </summary>
        /// <param name="bodyData">New data with which the avatar should be updated. Assumed not null</param>
        /// <param name="injectedPoses">Poses to inject inside the final avatar, expressed as rotations in Unity world frame of reference. For this joints, the value read from the bodyData gets ignored, and the one from this dictionary gets used.</param>
        /// <param name="trackPosition">True if the avatar should match user position in world space, false to track only pose</param>
        protected void ProtectedUpdate(TrackingServiceBodyData bodyData, Dictionary<TrackingServiceBodyJointTypes, Quaternion> injectedPoses, bool trackPosition = false)
        {
            if (trackPosition)
            {
                //compute overall scale of avatar
                Vector3 avatarScale = (m_jointsMapping[UmaJointTypes.Position].parent.lossyScale);

                //compute avatar position, removing the overall scale and rotation (because TrackingService reports coordinates with scale of 1.0 and zero rotation wrt ImmotionRoom frame of reference)
                Vector3 unscaledDiff = (m_jointsMapping[UmaJointTypes.Hips].position - m_jointsMapping[UmaJointTypes.Position].parent.position);
                unscaledDiff.Scale(new Vector3(1 / avatarScale.x, 1 / avatarScale.y, 1 / avatarScale.z));
                unscaledDiff = Quaternion.Inverse(m_avatarGo.transform.rotation) * unscaledDiff;
                
                //calculate displacement of actual body position to avatar position:
                //0.1 because ToUnityVector3 applies a 10x on the coordinates
                //remember that both ImmotionRoom and UMA have center of reference on the floor, at the center of the feet
                Vector3 displ = (bodyData.Joints[TrackingServiceBodyJointTypes.SpineBase].ToUnityVector3() * 0.1f -
                                 unscaledDiff);

                //apply global rotation, then scale the traslation component, then move the avatar to the new position
                displ = m_avatarGo.transform.rotation * displ;
                displ.Scale(avatarScale); //to take in count avatar scaling
                m_jointsMapping[UmaJointTypes.Position].position += displ;
            }

            //calculate rotations. Remember that we compute all rotations considering the avatar in initial T position, standing
            //on XY plane. All joints frame of references, in this position, coincide with Unity main frame of reference:
            //y towards up, x towards right, z towards the forward direction

            //calculate global rotation of avatar around Y axis
            Quaternion globalOrientation = ComputeGlobalBodyOrientation(bodyData);
            ApplyJointRotation(UmaJointTypes.Hips, globalOrientation);

            //calculate spine rotation
            Quaternion spineRotation = ComputeAndApplyElbowLikeJointOrientation(bodyData, UmaJointTypes.SpineUp,
                globalOrientation, Vector3.up, TrackingServiceBodyJointTypes.SpineBase, TrackingServiceBodyJointTypes.SpineShoulder,
                new UmaJointTypes[] { UmaJointTypes.LowerBack, UmaJointTypes.Spine }, 0.75f);

            //calculate head rotation.

            //If it has been provided by the user, apply it
            if (injectedPoses != null && injectedPoses.ContainsKey(TrackingServiceBodyJointTypes.Neck))
            {
                //ApplyWorldJointRotation(UmaJointTypes.Neck, QuaternionExtensions.Pow(injectedPoses[TrackingServiceBodyJointTypes.Neck], 0.5f)); //rotate a bit the neck, too, to make rotation more natural
                ApplyWorldJointRotation(UmaJointTypes.Head, injectedPoses[TrackingServiceBodyJointTypes.Neck]);
            }
            else //not injected from outside
            {
                Quaternion headRotation = ComputeAndApplyElbowLikeJointOrientation(bodyData, UmaJointTypes.Neck,
                    spineRotation, Vector3.up, TrackingServiceBodyJointTypes.Neck, TrackingServiceBodyJointTypes.Head);

                //apply head (dummy) rotation: at the moment we do not use this joint, but we apply the rotation of its father joint,
                //so to put it in T position
                ApplyJointRotation(UmaJointTypes.Head, headRotation);
            }

            //calculate rotations for left arm
                
            //upper arm
            Quaternion leftUpperArmRotation = ComputeAndApplyShoulderLikeJointOrientation(bodyData, UmaJointTypes.LeftArm, spineRotation,
                                            Vector3.left, TrackingServiceBodyJointTypes.ShoulderLeft, TrackingServiceBodyJointTypes.ElbowLeft, TrackingServiceBodyJointTypes.WristLeft,
                                            new UmaJointTypes[] { UmaJointTypes.LeftShoulder }, 0.3f);

            //forearm
            Quaternion leftForearmRotation = ComputeAndApplyElbowLikeJointOrientation(bodyData, UmaJointTypes.LeftForeArm,
                leftUpperArmRotation, Vector3.left, TrackingServiceBodyJointTypes.ElbowLeft, TrackingServiceBodyJointTypes.WristLeft);

            //hand palm. Caution that its rotation may be fixed as requested by user
            Quaternion leftHandPalmRotation;

            //if hand locking feature had not been requested
            if (!m_lockHandsPose)
            {
                //hand palm (it is rpy joint, like the shoulder, so we can use the same method)
                //notice that we almost don't use clamp angle feature (we put it to 180°, because results are better like this)        
                leftHandPalmRotation = ComputeAndApplyShoulderLikeJointOrientation(bodyData, UmaJointTypes.LeftHand, leftForearmRotation,
                    Vector3.left, TrackingServiceBodyJointTypes.HandLeft, TrackingServiceBodyJointTypes.HandTipLeft, TrackingServiceBodyJointTypes.ThumbLeft, null, 0,
                    180, false, UmaJointTypes.LeftForeArmTwist);
            }
            //else, locking feature requested
            else
            {
                //hand palm rotation is the same of elbow
                leftHandPalmRotation = leftForearmRotation;
                ApplyJointRotation(UmaJointTypes.LeftHand, leftHandPalmRotation);
            }

            //hand fingers
            ComputeAndApplyHandFingersRotations(bodyData, true, leftHandPalmRotation);

            //calculate rotations for right arm

            //upper arm
            Quaternion rightUpperArmRotation = ComputeAndApplyShoulderLikeJointOrientation(bodyData, UmaJointTypes.RightArm, spineRotation,
                                            Vector3.right, TrackingServiceBodyJointTypes.ShoulderRight, TrackingServiceBodyJointTypes.ElbowRight, TrackingServiceBodyJointTypes.WristRight,
                                            new UmaJointTypes[] { UmaJointTypes.RightShoulder }, 0.3f);

            //forearm
            Quaternion rightForearmRotation = ComputeAndApplyElbowLikeJointOrientation(bodyData, UmaJointTypes.RightForeArm,
                rightUpperArmRotation, Vector3.right, TrackingServiceBodyJointTypes.ElbowRight, TrackingServiceBodyJointTypes.WristRight);

            //hand palm. Caution that its rotation may be fixed as requested by user
            Quaternion rightHandPalmRotation;

            //if hand locking feature had not been requested
            if (!m_lockHandsPose)
            {
                //hand palm (it is rpy joint, like the shoulder, so we can use the same method)
                //notice that we almost don't use clamp angle feature (we put it to 180°, because results are better like this)        
                rightHandPalmRotation = ComputeAndApplyShoulderLikeJointOrientation(bodyData, UmaJointTypes.RightHand, rightForearmRotation,
                    Vector3.right, TrackingServiceBodyJointTypes.HandRight, TrackingServiceBodyJointTypes.HandTipRight, TrackingServiceBodyJointTypes.ThumbRight, null, 0,
                    180, false, UmaJointTypes.RightForeArmTwist);
            }
            //else, locking feature requested
            else
            {
                //hand palm rotation is the same of elbow
                rightHandPalmRotation = rightForearmRotation;
                ApplyJointRotation(UmaJointTypes.RightHand, rightHandPalmRotation);
            }

            //hand fingers
            ComputeAndApplyHandFingersRotations(bodyData, false, rightHandPalmRotation);

            //calculate rotations for left leg

            //leg
            Quaternion leftLegRotation = ComputeAndApplyShoulderLikeJointOrientation(bodyData, UmaJointTypes.LeftUpLeg, spineRotation,
                                            Vector3.down, TrackingServiceBodyJointTypes.HipLeft, TrackingServiceBodyJointTypes.KneeLeft, TrackingServiceBodyJointTypes.AnkleLeft);

            //knee
            Quaternion leftKneeRotation = ComputeAndApplyElbowLikeJointOrientation(bodyData, UmaJointTypes.LeftLeg,
                leftLegRotation, Vector3.down, TrackingServiceBodyJointTypes.KneeLeft, TrackingServiceBodyJointTypes.AnkleLeft);

            //foot (it is rpy joint, like the shoulder, so we can use the same method). Pay attention that if it is looked, foot has simply to stay in T position
            Quaternion leftFootRotation;

            if (!m_lockFeetPose)
            {
                leftFootRotation = ComputeAndApplyElbowLikeJointOrientation(bodyData, UmaJointTypes.LeftFoot,
                    leftKneeRotation, Vector3.forward, TrackingServiceBodyJointTypes.AnkleLeft, TrackingServiceBodyJointTypes.FootLeft);
            }
            else
            {
                leftFootRotation = leftKneeRotation;
                ApplyJointRotation(UmaJointTypes.LeftFoot, leftKneeRotation);
            }

            //apply foot toe (dummy) rotation: at the moment we do not use this joint, but we apply the rotation of its father joint,
            //so to put it in T position
            ApplyJointRotation(UmaJointTypes.LeftToeBase, leftFootRotation);

            //calculate rotations for right leg

            //leg
            Quaternion rightLegRotation = ComputeAndApplyShoulderLikeJointOrientation(bodyData, UmaJointTypes.RightUpLeg, spineRotation,
                                            Vector3.down, TrackingServiceBodyJointTypes.HipRight, TrackingServiceBodyJointTypes.KneeRight, TrackingServiceBodyJointTypes.AnkleRight);

            //knee
            Quaternion rightKneeRotation = ComputeAndApplyElbowLikeJointOrientation(bodyData, UmaJointTypes.RightLeg,
                rightLegRotation, Vector3.down, TrackingServiceBodyJointTypes.KneeRight, TrackingServiceBodyJointTypes.AnkleRight);

            //foot (it is rpy joint, like the shoulder, so we can use the same method). Pay attention that if it is looked, foot has simply to stay in T position
            Quaternion rightFootRotation;

            if (!m_lockFeetPose)
            {
                rightFootRotation = ComputeAndApplyElbowLikeJointOrientation(bodyData, UmaJointTypes.RightFoot,
                    rightKneeRotation, Vector3.forward, TrackingServiceBodyJointTypes.AnkleRight, TrackingServiceBodyJointTypes.FootRight);
            }
            else
            {
                rightFootRotation = rightKneeRotation;
                ApplyJointRotation(UmaJointTypes.RightFoot, rightKneeRotation);
            }            

            //apply foot toe (dummy) rotation: at the moment we do not use this joint, but we apply the rotation of its father joint,
            //so to put it in T position
            ApplyJointRotation(UmaJointTypes.RightToeBase, rightFootRotation);
            
        }

        /// <summary>
        /// Apply the computed global rotation of a joint to the joint of the avatar.
        /// If the joint does not exist in the avatar mappings, the method does nothing.
        /// </summary>
        /// <param name="joint">Joint of interest</param>
        /// <param name="globalRotation">Global rotation to apply, in avateering root coordinates</param>
        protected void ApplyJointRotation(UmaJointTypes joint, Quaternion globalRotation)
        {
            if (m_jointsMapping.ContainsKey(joint) && m_jointsGlobalTRotationMapping.ContainsKey(joint))
                //apply the global rotation of the avatar. On this new mobile axes, apply the computed global rotation, and finally the rotation that
                //take this joint to the required T position.
                //In fact, we compute the rotation that takes the joint in T position and then, on the fixed global axes, we apply the found rotation
                m_jointsMapping[joint].rotation = m_jointsMapping[UmaJointTypes.Root].parent.rotation * globalRotation * m_jointsGlobalTRotationMapping[joint];
        }

        /// <summary>
        /// Apply the computed global rotation of a joint to the joint of the avatar.
        /// If the joint does not exist in the avatar mappings, the method does nothing.
        /// </summary>
        /// <param name="joint">Joint of interest</param>
        /// <param name="globalRotation">Global rotation to apply, in Unity world coordinates</param>
        protected void ApplyWorldJointRotation(UmaJointTypes joint, Quaternion globalRotation)
        {
            if (m_jointsMapping.ContainsKey(joint) && m_jointsGlobalTRotationMapping.ContainsKey(joint))
                //apply the global rotation of the avatar. On this new mobile axes, apply the computed global rotation, and finally the rotation that
                //take this joint to the required T position.
                //In fact, we compute the rotation that takes the joint in T position and then, on the fixed global axes, we apply the found rotation
                m_jointsMapping[joint].rotation = globalRotation * m_jointsGlobalTRotationMapping[joint];
        }

        /// <summary>
        /// Compute global body orientation of avateering body
        /// </summary>
        /// <param name="bodyData">New data with which the avatar should be updated. Assumed not null</param>
        /// <returns>Global body orientation</returns>
        protected Quaternion ComputeGlobalBodyOrientation(TrackingServiceBodyData bodyData)
        {
            //use shoulders angle
            return Quaternion.AngleAxis(
                -Mathf.Rad2Deg *UnityUtilities.BetweenJointsXZOrientation(bodyData.Joints[TrackingServiceBodyJointTypes.ShoulderLeft].ToUnityVector3(),
                                                                         bodyData.Joints[TrackingServiceBodyJointTypes.ShoulderRight].ToUnityVector3()),
                Vector3.up
            );
        }

        /// <summary>
        /// Compute the GLOBAL rotation of an avatar joint, given body pose of the body read from the tracking service.
        /// This rotation gets applied to the desired UMA avatar joint.
        /// This helper method regards joints that are like the shoulder (i.e. shoulders, wrists and ankles), i.e. that have three
        /// axes of rotations (roll, pitch and yaw) and whose child joint, if any, has only one axis of rotation (like the elbow)
        /// </summary>
        /// <remarks>
        /// See inline code comments to better understand this method
        /// </remarks>
        /// <param name="bodyData">Pose of the body, as read from the tracking service</param>
        /// <param name="shoulderUmaJointType">The body joint of the avatar we want to apply the rotation to</param>
        /// <param name="fatherJointGlobalRotation">Global rotation applied to the father joint of this joint</param>
        /// <param name="avatarRestOrientation">Orientation of this joint, at rest in T position. In case of shoulder this is the x axis vector. Should be a value corresponding to a standard versor (left, right, forward, back, up, down)</param>
        /// <param name="shoulderJointType">The body joint of the avatar we want to apply the rotation to, as seen by the Tracking Service</param>
        /// <param name="elbowJointType">The child joint of this joint, as seen by the Tracking Service</param>
        /// <param name="wristJointType">The grand child joint of this joint, as seen by the Tracking Service</param>
        /// <param name="companionFatherJoints">Ancestors of this joint, that share part of the rotation computed for this joint. They have to be passed in order inside this array. See next parameter description for details</param>
        /// <param name="companionsShare">Number in the range [0, 1] that expresses how much of the rotation the companions holds. Let's make an example: we're rotating the upper arm and we have the Arm joint, which is the main one rotating and the Shoulder one, that is its father and that serves to curve the back a little, following the arm movement. So, the companion of the Arm is the Shoulder joint. If Shoulder share is 0.3, 30% of the computed rotation will be given to Shoulder and 70% to Arm, to make the movement more natural</param>
        /// <param name="limitAngle">Limit angle of roll rotation of this joint, in degrees. See next comment for further descriptions</param>
        /// <param name="clampOnLimit">If true, roll angle will be clamped in range [-limitAngle, +limitAngle]; if false: if roll &gt; +limitAngle, then roll -= 180; if roll &lt; -limitAngle, then roll += 180. False must be used only for hands, to compensate erroneous thumb flipping detected by the tracking sensors</param>
        /// <param name="twistedJoint">Joint that has to copy the roll rotation of the computed joint. Used only in hands to assign the twist of the forearm so that if tollows the wrist</param>
        /// <param name="wristJimbalLockSetToZeroMagnitude">Threshold of the magnitude of the normalized position vector of the wrist in arm YZ frame of reference below which the arm is considered in jimbal lock and so the roll is set to 0</param>
        /// <returns>The computed global rotation</returns>
        protected Quaternion ComputeAndApplyShoulderLikeJointOrientation(TrackingServiceBodyData bodyData, UmaJointTypes shoulderUmaJointType, Quaternion fatherJointGlobalRotation,
                                                                         Vector3 avatarRestOrientation, TrackingServiceBodyJointTypes shoulderJointType, TrackingServiceBodyJointTypes elbowJointType,
                                                                         TrackingServiceBodyJointTypes wristJointType, UmaJointTypes[] companionFatherJoints = null, float companionsShare = 0.0f,
                                                                         float limitAngle = 90, bool clampOnLimit = true, UmaJointTypes? twistedJoint = null, float wristJimbalLockSetToZeroMagnitude = 0.265f)
        {
            //all the comments here regard the left shoulder case, because code was developed for this case, but actually this algorithm can
            //be applied to all joint that behave like the shoulder (like the leg one)
            //Obviously, when applied to legs, the axis to be considered are different from the ones specified in the comments

            //compute the rotation from the rest orientation to the actual orientation.
            //Rest orientation is the one of the Avatar in T position (e.g. for left arm is the Vector.Left value), 
            //while actual orientation is the one from the shoulder to the elbow        
            Quaternion jointYawPitch = UnityUtilities.GetRotationFromTo(avatarRestOrientation, Quaternion.Inverse(fatherJointGlobalRotation) * 
                (bodyData.Joints[elbowJointType].ToUnityVector3() - bodyData.Joints[shoulderJointType].ToUnityVector3()));

            //apply the found rotation to the avatar, so now we have the arm correctly oriented towards the elbow. The problem is that we have not
            //set correctly the roll of the arm, that must be calculated correctly to make the rest of the arm to move in a natural way
            ApplyJointRotation(shoulderUmaJointType, fatherJointGlobalRotation * jointYawPitch);

            //let's compute the roll: the wrist should be reachable by rotating the forearm only around wrist local y axis. The roll serves to rotate the upper arm
            // so that the elbow, rotating only around its y axis, can put the wrist in the right position
            //(remeber that elbow can rotate only around 1 axis, its y one)
            //So, compute wrist position in the new rotated upperarm frame of reference (elbow is the origin and the axes have the orientation of the yaw-pitch rotation calculated above)
            Vector3 transfWristPos = Quaternion.Inverse(fatherJointGlobalRotation * jointYawPitch) * (bodyData.Joints[wristJointType].ToUnityVector3() - bodyData.Joints[elbowJointType].ToUnityVector3());

            //remove X coordinate and re-put it in global frame of reference
            //Why we do this? Because the roll of the upper-arm is actually its rotation around its x axis (the one parallel to the human bone of the upper-arm:
            //when the avatar is in T position, it coincides in direction with the global X axis).
            //Removing x component, we obtain the wrist position on the YZ frame of reference of the rotated upperarm and can compute roll rotation that makes wrist position
            //of this new frame of reference coincide with our z axis. This will make possible for the elbow to reach the actual wrist position only rotating around one axis
            Vector2 transfWristPostYZ;
            
            if(avatarRestOrientation.x == -1) //rest position is negative horizontal (left shoulder case)
                transfWristPostYZ = new Vector2(transfWristPos.normalized.y, transfWristPos.normalized.z);
            else if(avatarRestOrientation.x == +1) //rest position is positive horizontal (right shoulder case)
                transfWristPostYZ = new Vector2(-transfWristPos.normalized.y, transfWristPos.normalized.z);
            else if (avatarRestOrientation.y == -1) //rest position is negative vertical (legs case)
                transfWristPostYZ = new Vector2(transfWristPos.normalized.x, -transfWristPos.normalized.z);
            else if (avatarRestOrientation.y == 1) //rest position is positive vertical (never used)
                transfWristPostYZ = new Vector2(transfWristPos.normalized.x, -transfWristPos.normalized.z);
            else //rest position is forward (never used)
                transfWristPostYZ = new Vector2(transfWristPos.normalized.z, transfWristPos.normalized.y);

            //if we are near the jimbal lock (shoulder, elbow and wrist are aligned), set the roll to 0, because at jimbal lock we are unable to determine
            //the correct roll angle of the arm (and this often leads to weird results)
            float jointRollAngle = (transfWristPostYZ.magnitude < wristJimbalLockSetToZeroMagnitude) ? 0 : -Mathf.Atan2(transfWristPostYZ.x, transfWristPostYZ.y) * Mathf.Rad2Deg;

            //check limits: if clamp was requested...
            if (clampOnLimit)
                //clamp the angle, because shoulder can rotate only between 90 and -90 degrees
                jointRollAngle = Mathf.Clamp(jointRollAngle, -limitAngle, limitAngle);
            //otherwise, we must flip
            else
            {
                //if we passed the limit, flip adding or removing 180 degrees
                if (jointRollAngle > limitAngle)
                    jointRollAngle -= 180;
                else if (jointRollAngle < -limitAngle)
                    jointRollAngle += 180;
            }

            //compute the rotation quaternion, as a rotation around the X axis
            Quaternion jointRoll = Quaternion.AngleAxis(jointRollAngle, -avatarRestOrientation);

            //apply the rotations: remember to apply them in the correct order.
            //In particular, the roll has been computed as a rotation around the Vector.Right axis, but it is actually the x axis
            //as resulted by the previous rotation (hence the post-multiplication).           
            Quaternion jointRotationNoFather = jointYawPitch * jointRoll;
            Quaternion jointRotation = fatherJointGlobalRotation * jointRotationNoFather;
            
            //if this joint has companion ancenstors, we have to blend the computed rotation among all the joints tree
            if (companionFatherJoints != null && companionFatherJoints.Length > 0)
            {
                //compute how much of the rotation is owed to this joint and to the ancestors
                float otherCompanionsShare = companionsShare / companionFatherJoints.Length;

                //apply the rotation to the ancestors... each one with its share.
                //Remember that proportion of quaternions is made with powers (e.g. q^0.5 is the half of a rotation).
                //NOTICE that father rotation is applied COMPLETELY to the companions, while only the current rotation is applied with a different share for each companion
                for (int i = 0; i < companionFatherJoints.Length; i++)
                    ApplyJointRotation(companionFatherJoints[i], fatherJointGlobalRotation * QuaternionExtensions.Pow(jointRotationNoFather, otherCompanionsShare * (i + 1)));

                //apply the rotation to this joint
                ApplyJointRotation(shoulderUmaJointType, jointRotation);
            }
            //else, simply apply the rotation
            else
                ApplyJointRotation(shoulderUmaJointType, jointRotation);

            //assign the roll rotation to the twisted joint, if any.
            //Notice that actually we assign half of the roll angle, because this is the right way in UMA to make the visual result
            //nice to see (the twist result is more natural)
            if (twistedJoint.HasValue)
                ApplyJointRotation(twistedJoint.Value, fatherJointGlobalRotation * QuaternionExtensions.Pow(jointRoll, 0.5f));

            //return the computed value, for further joints rotations
            return jointRotation;
        }

        /// <summary>
        /// Compute the GLOBAL rotation of an avatar joint, given body pose of the body read from the tracking service.
        /// This rotation gets applied to the desired UMA avatar joint.
        /// This helper method regards joints that are like the elbow (i.e. elbow, knee, foot), i.e. that have one
        /// axis of rotations.
        /// The method does not guarantee that rotations happen only around one main axis, but if you used the <see cref="ComputeAndApplyShoulderLikeJointOrientation"/> method
        /// to compute the rotation of the father, you're ok using this method
        /// </summary>
        /// <remarks>
        /// See inline code comments to better understand this method
        /// </remarks>
        /// <param name="bodyData">Pose of the body, as read from the tracking service</param>
        /// <param name="elbowUmaJointType">The body joint of the avatar we want to apply the rotation to</param>
        /// <param name="fatherJointGlobalRotation">Global rotation applied to the father joint of this joint</param>
        /// <param name="avatarRestOrientation">Orientation of this joint, at rest in T position. In case of elbow this is the x axis vector. Should be a value corresponding to a standard versor (left, right, forward, back, up, down)</param>
        /// <param name="elbowJointType">The body joint of the avatar we want to apply the rotation to, as seen by the Tracking Service</param>
        /// <param name="wristJointType">The child joint of this joint, as seen by the Tracking Service</param>       
        /// <param name="companionFatherJoints">Ancestors of this joint, that share part of the rotation computed for this joint. They have to be passed in order inside this array. See next parameter description for details</param>
        /// <param name="companionsShare">Number in the range [0, 1] that expresses how much of the rotation the companions holds. Let's make an example: we're rotating the upper arm and we have the Arm joint, which is the main one rotating and the Shoulder one, that is its father and that serves to curve the back a little, following the arm movement. So, the companion of the Arm is the Shoulder joint. If Shoulder share is 0.3, 30% of the computed rotation will be given to Shoulder and 70% to Arm, to make the movement more natural</param>
        /// <returns>The computed global rotation</returns>
        protected Quaternion ComputeAndApplyElbowLikeJointOrientation(TrackingServiceBodyData bodyData, UmaJointTypes elbowUmaJointType, Quaternion fatherJointGlobalRotation,
                                                                         Vector3 avatarRestOrientation, TrackingServiceBodyJointTypes elbowJointType,
                                                                         TrackingServiceBodyJointTypes wristJointType, UmaJointTypes[] companionFatherJoints = null, float companionsShare = 0.0f)
        {
            //all the comments here regard the left elbow case, because code was developed for this case, but actually this algorithm can
            //be applied to all joint that behave like the elbow (like the knee one)
            //Obviously, when applied to legs, the axis to be considered are different from the ones specified in the comments

            //Get wrist position in the frame of reference of the father joint, that we must have already computed.
            //This is because we've already performed a rotation with the fore-arm and we must continue that rotation, and not begin a new one,
            //so we must compute wrist position in the new rotated frame of reference
            Vector3 transfWristPos = Quaternion.Inverse(fatherJointGlobalRotation) * (bodyData.Joints[wristJointType].ToUnityVector3() - bodyData.Joints[elbowJointType].ToUnityVector3());

            //compute rotation that takes from the left axis to the newly found position. Remember that when the avatar is in T position, the forearm
            //at rest is parallel to the x axis: we are in the newly rotated system, and if the forearm doesn't move, it lies onto frame of reference x axis
            Quaternion foreArmRot = UnityUtilities.GetRotationFromTo(avatarRestOrientation, transfWristPos);

            //obtain rotation as a composition of computed rotation with father global one, then apply it
            Quaternion jointRotationNoFather = foreArmRot;
            Quaternion jointRotation = fatherJointGlobalRotation * jointRotationNoFather;

            //if this joint has companion ancenstors, we have to blend the computed rotation among all the joints tree
            if (companionFatherJoints != null && companionFatherJoints.Length > 0)
            {
                //compute how much of the rotation is owed to this joint and to the ancestors
                float otherCompanionsShare = companionsShare / companionFatherJoints.Length;

                //apply the rotation to the ancestors... each one with its share.
                //Remember that proportion of quaternions is made with powers (e.g. q^0.5 is the half of a rotation)
                //NOTICE that father rotation is applied COMPLETELY to the companions, while only the current rotation is applied with a different share for each companion
                for (int i = 0; i < companionFatherJoints.Length; i++)
                    ApplyJointRotation(companionFatherJoints[i], fatherJointGlobalRotation * QuaternionExtensions.Pow(jointRotationNoFather, otherCompanionsShare * (i + 1)));

                //apply the rotation to this joint
                ApplyJointRotation(elbowUmaJointType, jointRotation);
            }
            //else, simply apply the rotation
            else
                ApplyJointRotation(elbowUmaJointType, jointRotation);

            //return the computed value, for further joints rotations
            return jointRotation;
        }

        /// <summary>
        /// Compute the GLOBAL rotation of all the hand finger joints, given body pose of the body read from the tracking service.
        /// This rotation gets applied to the desired UMA avatar hand joints.
        /// This method does not apply rotations to finger independently: it is used for hand closed/hand open interaction only
        /// </summary>
        /// <remarks>
        /// See inline code comments to better understand this method
        /// </remarks>
        /// <param name="bodyData">Pose of the body, as read from the tracking service</param>
        /// <param name="isLeftHand">True if we want to avateer left hand, false for right hand</param>
        /// <param name="handJointGlobalRotation">Global rotation applied to the joint of the hand</param>
        protected void ComputeAndApplyHandFingersRotations(TrackingServiceBodyData bodyData, bool isLeftHand, Quaternion handJointGlobalRotation)
        {
            //express how much the hand is closed, in range [0, 1] (0 (hands full open) to 1 (full closed))
            float closingFactor;
            const float closingLowThresh = 0.12f, closingHighThresh = 0.215f; //threshold for hand open / close computation

            //if has been requested a fixed hand pose, just do nothing and apply a semi-opened hand rotation
            if (m_lockHandsPose)
            {
                closingFactor = closingLowThresh;
            }
            else
            {
                //pre-compute some values
                Vector3 thumbPos = isLeftHand ? bodyData.Joints[TrackingServiceBodyJointTypes.ThumbLeft].ToUnityVector3() : bodyData.Joints[TrackingServiceBodyJointTypes.ThumbRight].ToUnityVector3();
                Vector3 handTipPos = isLeftHand ? bodyData.Joints[TrackingServiceBodyJointTypes.HandTipLeft].ToUnityVector3() : bodyData.Joints[TrackingServiceBodyJointTypes.HandTipRight].ToUnityVector3();
                Vector3 handPos = isLeftHand ? bodyData.Joints[TrackingServiceBodyJointTypes.HandLeft].ToUnityVector3() : bodyData.Joints[TrackingServiceBodyJointTypes.HandRight].ToUnityVector3();
                float shoulderToElbowDistance = isLeftHand ?
                    (bodyData.Joints[TrackingServiceBodyJointTypes.ElbowLeft].ToUnityVector3() - bodyData.Joints[TrackingServiceBodyJointTypes.ShoulderLeft].ToUnityVector3()).magnitude :
                    (bodyData.Joints[TrackingServiceBodyJointTypes.ElbowRight].ToUnityVector3() - bodyData.Joints[TrackingServiceBodyJointTypes.ShoulderRight].ToUnityVector3()).magnitude;

                //algorithm overview: take positions of palm, hand tip and hand thumb.
                //If the hand is open, this point are distants, if it is closed, they are close each other

                //so, compute points baricenter
                Vector3 baricenter = (thumbPos + handTipPos + handPos) / 3;

                //compute distance of the various points from their baricenter
                float thumbDeviation = (thumbPos - baricenter).magnitude;
                float handTipDeviation = (handTipPos - baricenter).magnitude;
                float handDeviation = (handPos - baricenter).magnitude;

                //so, take the maximum of the deviations, divide by the length of arm (this is to have a proportional measure, indipendent from any measuring unit,
                //in particular because taking the absolute value of the deviation would lead to different thresholds depending on the distance of the hand from
                //the world origin. We have chosen arm measure, because it is more stable than hand or forearm ones),
                //then obtain a value from 0 (hands full open) to 1 (full closed)
                closingFactor = Mathf.Clamp(Mathf.Max(thumbDeviation, handTipDeviation, handTipDeviation) / shoulderToElbowDistance, closingLowThresh, closingHighThresh);
                closingFactor = 1 - (closingFactor - closingLowThresh) * (1 / (closingHighThresh - closingLowThresh));
            }

                //closing factor is used to compute rotations of fingers... and for the right hand, the angles are the opposite of the left case one
                if (!isLeftHand)
                    closingFactor = -closingFactor;            

            //for the first 4 fingers, it is relatively easy: use this closing measure to rotate all fingers joints.
            //Remember that in T position, all finger joints rotate on z (forward) axis to close themselves.
            //The rotation are composed as follows:
            // first finger joints aligns to hand and then closes itself
            // second finger joints align to first one and the closes itself
            // third finger joints align to second one and the closes itself
            Quaternion fingersRot = Quaternion.AngleAxis(closingFactor * 110, Vector3.forward); //110 is a const value to determine how much to close the fingers 
            Quaternion fingersRotartion = handJointGlobalRotation * fingersRot;

            if (isLeftHand)
            {
                ApplyJointRotation(UmaJointTypes.LeftHandIndex, fingersRotartion);
                ApplyJointRotation(UmaJointTypes.LeftHandIndex_1, fingersRotartion * fingersRot);
                ApplyJointRotation(UmaJointTypes.LeftHandIndex_2, fingersRotartion * fingersRot * fingersRot);
                ApplyJointRotation(UmaJointTypes.LeftHandMiddle, fingersRotartion);
                ApplyJointRotation(UmaJointTypes.LeftHandMiddle_1, fingersRotartion * fingersRot);
                ApplyJointRotation(UmaJointTypes.LeftHandMiddle_2, fingersRotartion * fingersRot * fingersRot);
                ApplyJointRotation(UmaJointTypes.LeftHandRing, fingersRotartion);
                ApplyJointRotation(UmaJointTypes.LeftHandRing_1, fingersRotartion * fingersRot);
                ApplyJointRotation(UmaJointTypes.LeftHandRing_2, fingersRotartion * fingersRot * fingersRot);
                ApplyJointRotation(UmaJointTypes.LeftHandLittle, fingersRotartion);
                ApplyJointRotation(UmaJointTypes.LeftHandLittle_1, fingersRotartion * fingersRot);
                ApplyJointRotation(UmaJointTypes.LeftHandLittle_2, fingersRotartion * fingersRot * fingersRot);
            }
            else
            {
                ApplyJointRotation(UmaJointTypes.RightHandIndex, fingersRotartion);
                ApplyJointRotation(UmaJointTypes.RightHandIndex_1, fingersRotartion * fingersRot);
                ApplyJointRotation(UmaJointTypes.RightHandIndex_2, fingersRotartion * fingersRot * fingersRot);
                ApplyJointRotation(UmaJointTypes.RightHandMiddle, fingersRotartion);
                ApplyJointRotation(UmaJointTypes.RightHandMiddle_1, fingersRotartion * fingersRot);
                ApplyJointRotation(UmaJointTypes.RightHandMiddle_2, fingersRotartion * fingersRot * fingersRot);
                ApplyJointRotation(UmaJointTypes.RightHandRing, fingersRotartion);
                ApplyJointRotation(UmaJointTypes.RightHandRing_1, fingersRotartion * fingersRot);
                ApplyJointRotation(UmaJointTypes.RightHandRing_2, fingersRotartion * fingersRot * fingersRot);
                ApplyJointRotation(UmaJointTypes.RightHandLittle, fingersRotartion);
                ApplyJointRotation(UmaJointTypes.RightHandLittle_1, fingersRotartion * fingersRot);
                ApplyJointRotation(UmaJointTypes.RightHandLittle_2, fingersRotartion * fingersRot * fingersRot);
            }

            //thumb is similar, but a bit more difficult, because in T position we chose to leave it a bit spread out and not aligned to the other fingers, so
            //we have to rotate it around a skew axis to make it perform a natural rotation
            //TODO: align the thumb, in T position, to the other fingers?
            Quaternion thumbRot = Quaternion.AngleAxis(-closingFactor * 48f, 3 * Vector3.up + Vector3.left);
            Quaternion thumbInnerRot = Quaternion.AngleAxis(closingFactor * 77, Vector3.forward);
            Quaternion thumbRotartion = handJointGlobalRotation * thumbRot;

            if (isLeftHand)
            {
                ApplyJointRotation(UmaJointTypes.LeftHandThumb, thumbRotartion);
                ApplyJointRotation(UmaJointTypes.LeftHandThumb_1, thumbRotartion * thumbRot);
                ApplyJointRotation(UmaJointTypes.LeftHandThumb_2, thumbRotartion * thumbRot * thumbRot);
            }
            else
            {
                ApplyJointRotation(UmaJointTypes.RightHandThumb, thumbRotartion);
                ApplyJointRotation(UmaJointTypes.RightHandThumb_1, thumbRotartion * thumbRot);
                ApplyJointRotation(UmaJointTypes.RightHandThumb_2, thumbRotartion * thumbRot * thumbRot);
            }
        }

        /// <summary>
        /// Get the transform representing the desired human joint.
        /// If there is not an avatar joint representing exactly the desired joint, the most accurate representation possible will be returned
        /// </summary>
        /// <param name="jointType">Joint Type of Interest</param>
        /// <returns>Transform, inside Unity scene, representing the desired joint type</returns>
        protected Transform ProtectedGetJointTransform(TrackingServiceBodyJointTypes jointType)
        {
            switch (jointType)
            {
                case TrackingServiceBodyJointTypes.AnkleLeft:
                    return m_jointsMapping[UmaJointTypes.LeftFoot];

                case TrackingServiceBodyJointTypes.AnkleRight:
                    return m_jointsMapping[UmaJointTypes.RightFoot];

                case TrackingServiceBodyJointTypes.ElbowLeft:
                    return m_jointsMapping[UmaJointTypes.LeftForeArm];

                case TrackingServiceBodyJointTypes.ElbowRight:
                    return m_jointsMapping[UmaJointTypes.RightForeArm];

                case TrackingServiceBodyJointTypes.FootLeft:
                    if (m_jointsMapping.ContainsKey(UmaJointTypes.LeftToeBase))
                        return m_jointsMapping[UmaJointTypes.LeftToeBase];
                    else
                        return m_jointsMapping[UmaJointTypes.LeftFoot];

                case TrackingServiceBodyJointTypes.FootRight:
                    if (m_jointsMapping.ContainsKey(UmaJointTypes.RightToeBase))
                        return m_jointsMapping[UmaJointTypes.RightToeBase];
                    else
                        return m_jointsMapping[UmaJointTypes.RightFoot];

                case TrackingServiceBodyJointTypes.HandLeft:
                    return m_jointsMapping[UmaJointTypes.LeftHandMiddle];

                case TrackingServiceBodyJointTypes.HandRight:
                    return m_jointsMapping[UmaJointTypes.RightHandMiddle];

                case TrackingServiceBodyJointTypes.HandTipLeft:
                    return m_jointsMapping[UmaJointTypes.LeftHandIndex_2];

                case TrackingServiceBodyJointTypes.HandTipRight:
                    return m_jointsMapping[UmaJointTypes.RightHandIndex_2];

                case TrackingServiceBodyJointTypes.Head:
                        return m_jointsMapping[UmaJointTypes.Head];

                case TrackingServiceBodyJointTypes.HipLeft:
                    return m_jointsMapping[UmaJointTypes.LeftUpLeg];

                case TrackingServiceBodyJointTypes.HipRight:
                    return m_jointsMapping[UmaJointTypes.RightUpLeg];

                case TrackingServiceBodyJointTypes.KneeLeft:
                    return m_jointsMapping[UmaJointTypes.LeftLeg];

                case TrackingServiceBodyJointTypes.KneeRight:
                    return m_jointsMapping[UmaJointTypes.RightLeg];

                case TrackingServiceBodyJointTypes.Neck:
                    return m_jointsMapping[UmaJointTypes.Neck];

                case TrackingServiceBodyJointTypes.ShoulderLeft:
                    return m_jointsMapping[UmaJointTypes.LeftArm];

                case TrackingServiceBodyJointTypes.ShoulderRight:
                    return m_jointsMapping[UmaJointTypes.RightArm];

                case TrackingServiceBodyJointTypes.SpineBase:
                    if (m_jointsMapping.ContainsKey(UmaJointTypes.LowerBack))
                        return m_jointsMapping[UmaJointTypes.LowerBack];
                    else
                        return m_jointsMapping[UmaJointTypes.SpineUp];

                case TrackingServiceBodyJointTypes.SpineMid:
                    if (m_jointsMapping.ContainsKey(UmaJointTypes.Spine))
                        return m_jointsMapping[UmaJointTypes.Spine];
                    else
                        return m_jointsMapping[UmaJointTypes.SpineUp];

                case TrackingServiceBodyJointTypes.SpineShoulder:
                    return m_jointsMapping[UmaJointTypes.SpineUp];

                case TrackingServiceBodyJointTypes.ThumbLeft:
                    return m_jointsMapping[UmaJointTypes.LeftHandThumb_2];

                case TrackingServiceBodyJointTypes.ThumbRight:
                    return m_jointsMapping[UmaJointTypes.RightHandThumb_2];

                case TrackingServiceBodyJointTypes.WristLeft:
                    return m_jointsMapping[UmaJointTypes.LeftHand];

                case TrackingServiceBodyJointTypes.WristRight:
                    return m_jointsMapping[UmaJointTypes.RightHand];

                default:
                    return m_jointsMapping[UmaJointTypes.Root];
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Get measure of feature for current body detected by the tracking service
        /// </summary>
        /// <param name="featureID">ID of the desired feature</param>
        /// <param name="bodyData">First frame skeleton data with which the avatar should be initialized</param>
        /// <returns>Desired feature dimension, in a way useful for a UMA Avatar</returns>
        /// <exception cref="ArgumentException">If the provided featureID does not correspond to a valid feature</exception>
        internal float GetFeatureMeasure(int featureID, TrackingServiceBodyData bodyData)
        {
            //calculate the requested feature
            switch (featureID)
            {
                //user height from neck to ankles
                case PhysioMatchingFeatures.Height:
                    {
                        float avgLegsLength = (UnityUtilities.BetweenJointsUnityDistance(bodyData, TrackingServiceBodyJointTypes.HipLeft, TrackingServiceBodyJointTypes.KneeLeft) +
                                   UnityUtilities.BetweenJointsUnityDistance(bodyData, TrackingServiceBodyJointTypes.KneeLeft, TrackingServiceBodyJointTypes.AnkleLeft) +
                                   UnityUtilities.BetweenJointsUnityDistance(bodyData, TrackingServiceBodyJointTypes.HipRight, TrackingServiceBodyJointTypes.KneeRight) +
                                   UnityUtilities.BetweenJointsUnityDistance(bodyData, TrackingServiceBodyJointTypes.KneeRight, TrackingServiceBodyJointTypes.AnkleRight)) / 2;

                        float userHeight = UnityUtilities.BetweenJointsUnityDistance(bodyData, TrackingServiceBodyJointTypes.Neck, TrackingServiceBodyJointTypes.SpineShoulder) +
                                           UnityUtilities.BetweenJointsUnityDistance(bodyData, TrackingServiceBodyJointTypes.SpineShoulder, TrackingServiceBodyJointTypes.SpineMid) +
                                           UnityUtilities.BetweenJointsUnityDistance(bodyData, TrackingServiceBodyJointTypes.SpineMid, TrackingServiceBodyJointTypes.SpineBase) +
                                           avgLegsLength;

                        return userHeight;
                    }

                //average length of legs from hips to ankles
                case PhysioMatchingFeatures.LegsLength:
                    {
                        float avgLegsLength = (UnityUtilities.BetweenJointsUnityDistance(bodyData, TrackingServiceBodyJointTypes.HipLeft, TrackingServiceBodyJointTypes.KneeLeft) +
                                   UnityUtilities.BetweenJointsUnityDistance(bodyData, TrackingServiceBodyJointTypes.KneeLeft, TrackingServiceBodyJointTypes.AnkleLeft) +
                                   UnityUtilities.BetweenJointsUnityDistance(bodyData, TrackingServiceBodyJointTypes.HipRight, TrackingServiceBodyJointTypes.KneeRight) +
                                   UnityUtilities.BetweenJointsUnityDistance(bodyData, TrackingServiceBodyJointTypes.KneeRight, TrackingServiceBodyJointTypes.AnkleRight)) / 2;

                        return avgLegsLength;
                    }

                //distance between the two shoulders 
                case PhysioMatchingFeatures.ShouldersWidth:
                    {
                        float shouldersWidth = UnityUtilities.BetweenJointsUnityDistance(bodyData, TrackingServiceBodyJointTypes.ShoulderRight, TrackingServiceBodyJointTypes.ShoulderLeft);

                        return shouldersWidth;
                    }

                //average arms length from shoulder to wrist
                case PhysioMatchingFeatures.ArmsLength:
                    {
                        float avgForeArmsLength = (UnityUtilities.BetweenJointsUnityDistance(bodyData, TrackingServiceBodyJointTypes.ElbowLeft, TrackingServiceBodyJointTypes.WristLeft) +
                                                   UnityUtilities.BetweenJointsUnityDistance(bodyData, TrackingServiceBodyJointTypes.ElbowRight, TrackingServiceBodyJointTypes.WristRight)) / 2;
                        float avgArmsLength = (UnityUtilities.BetweenJointsUnityDistance(bodyData, TrackingServiceBodyJointTypes.ShoulderLeft, TrackingServiceBodyJointTypes.ElbowLeft) +
                                               avgForeArmsLength +
                                               UnityUtilities.BetweenJointsUnityDistance(bodyData, TrackingServiceBodyJointTypes.ShoulderRight, TrackingServiceBodyJointTypes.ElbowRight) +
                                               avgForeArmsLength) / 2;

                        return avgArmsLength;
                    }

                //average forearms length from elbow to wrist
                case PhysioMatchingFeatures.ForeArmsLength:
                    {
                        float avgForeArmsLength = (UnityUtilities.BetweenJointsUnityDistance(bodyData, TrackingServiceBodyJointTypes.ElbowLeft, TrackingServiceBodyJointTypes.WristLeft) +
                                       UnityUtilities.BetweenJointsUnityDistance(bodyData, TrackingServiceBodyJointTypes.ElbowRight, TrackingServiceBodyJointTypes.WristRight)) / 2;

                        return avgForeArmsLength;
                    }

                //unknown measure
                default:
                    if (Log.IsErrorEnabled)
                    {
                        Log.Error("UmaAvatarer - Unknown feature ID: feature {0} does not exist", featureID);
                    }

                    throw new ArgumentException("UmaAvatarer - Unknown feature ID");
            }
        }

        #endregion

    }
}

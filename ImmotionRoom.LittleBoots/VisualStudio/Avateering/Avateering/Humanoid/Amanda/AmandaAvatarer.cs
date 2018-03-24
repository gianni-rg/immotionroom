namespace ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Humanoid.Amanda
{ 
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Tools;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Collisions;

    /// <summary>
    /// Class to manage an Amanda Avatar of the user body
    /// </summary>
    /// <remarks>
    /// This code of this class is a mess, because it is Amanda code we've always used, so for historical reasons, I'll leave it as-is :)
    /// Good ol' times spent with Amanda...
    /// </remarks>
    internal class AmandaAvatarer : HumanoidAvatarer
    {
        #region Constants

        private static readonly Vector3 FootColliderCenter = new Vector3(-0.002278498f, -0.009029807f, 0.07328056f);
        private static readonly Vector3 FootColliderSize = new Vector3(0.07903215f, 0.0564695f, 0.2039802f);
        private static readonly Vector3 HandsColliderCenter = new Vector3(0.002f, 0.009f, -0.082f);
        private static readonly Vector2 HandsColliderRadiusHeight = new Vector2(0.06266853f, 0.2017645f);

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="avatarModel">Avatar game object that has to be moved according to player movements</param>
        /// <param name="addColliders">True if hands/feet colliders have to be attached to amanda, false otherwise</param>
        /// <param name="shadowsEnabled">True if the bodies have to cast/receive shadows, false otherwise</param>
        internal AmandaAvatarer(GameObject avatarModel, bool addColliders, bool shadowsEnabled) :
            base(avatarModel)
        {
            //disable Animator object, if any (with animator, we can't move the skeleton)
            var animator = avatarModel.GetComponent<Animator>();

            if (animator != null)
                UnityEngine.Object.Destroy(animator);

            //attach colliders, if required
            if(addColliders)
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
                colliderGo.transform.SetParent(avatarModel.transform.Find("hip/L_leg/L_knee/L_ankle/L_foot/L_toes"), false);

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
                colliderGo.transform.SetParent(avatarModel.transform.Find("hip/R_leg/R_knee/R_ankle/R_foot/R_toes"), false);

                //left hand
                colliderGo = new GameObject(AvatarCollidersProps.LeftHandColliders.ObjectName);
                //colliderGo.tag = AvatarCollidersProps.LeftHandColliders.ObjectTag;
                CapsuleCollider capsuleCollider = colliderGo.AddComponent<CapsuleCollider>();
                capsuleCollider.center = HandsColliderCenter;
                capsuleCollider.radius = HandsColliderRadiusHeight.x;
                capsuleCollider.height = HandsColliderRadiusHeight.y;
                capsuleCollider.direction = 2;
                capsuleCollider.isTrigger = true;
                colliderRb = colliderGo.AddComponent<Rigidbody>(); //add rigidbody too, or collision detection won't work
                colliderRb.isKinematic = false;
                colliderRb.useGravity = false;
                colliderGo.transform.SetParent(avatarModel.transform.Find("hip/spine/chest/L_shoulder/L_arm/L_elbow/L_wrist/"), false);

                //right hand
                colliderGo = new GameObject(AvatarCollidersProps.RightHandColliders.ObjectName);
                //colliderGo.tag = AvatarCollidersProps.RightHandColliders.ObjectTag;
                capsuleCollider = colliderGo.AddComponent<CapsuleCollider>();
                capsuleCollider.center = HandsColliderCenter;
                capsuleCollider.radius = HandsColliderRadiusHeight.x;
                capsuleCollider.height = HandsColliderRadiusHeight.y;
                capsuleCollider.direction = 2;
                capsuleCollider.isTrigger = true;
                colliderRb = colliderGo.AddComponent<Rigidbody>(); //add rigidbody too, or collision detection won't work
                colliderRb.isKinematic = false;
                colliderRb.useGravity = false;
                colliderGo.transform.SetParent(avatarModel.transform.Find("hip/spine/chest/R_shoulder/R_arm/R_elbow/R_wrist/"), false);
            }

            //if required, disable shadows from all renderable objects
            if (!shadowsEnabled)
            {
                Renderer[] renderables = avatarModel.transform.GetComponentsInChildren<Renderer>();

                foreach (Renderer renderable in renderables)
                {
                    renderable.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderable.receiveShadows = false;
                }
            }

            if(Log.IsDebugEnabled)
            {
                Log.Debug("Amanda Avatarer created");
            }
        }

        #endregion

        #region Avatarer members

        /// <summary>
        /// Gets the root joint transform of the controlled avatar object, in Unity frame of reference.
        /// Root corresponds to the main joint of the avatar (usually spine hip or spine mid point), the father all of others
        /// </summary>
        protected internal override Transform BodyRootJoint
        {
            get
            {
                return m_avatarModel.transform.Find("hip");
            }
        }

        /// <summary>
        /// Gets the position of the avatar corresponding to the mean points of the avatar's eyes, in Unity world coordinates.
        /// This is the position where it is ideal to have the VR camera attached to the avatar, in VR applications
        /// </summary>
        protected internal override Vector3 BodyEyesCameraPosition
        {
            get
            {
                return (m_avatarModel.transform.Find("hip/spine/chest/neck/head/L_eye").position + 
                        m_avatarModel.transform.Find("hip/spine/chest/neck/head/R_eye").position) / 2; //return eyes center
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
            //do nothing, Amanda avatar is fixed

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Amanda Avatarer initialized");
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

            ProtectedUpdate(bodyData, trackPosition);
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Updates the avatar, given new body data
        /// </summary>
        /// <param name="bodyData">New data with which the avatar should be updated. Assumed not null</param>
        /// <param name="trackPosition">True if the avatar should match user position in world space, false to track only pose</param>
        protected void ProtectedUpdate(TrackingServiceBodyData bodyData, bool trackPosition = false)
        {
            //m_avatarModel.transform.Find("hip/L_leg").position = m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.HipLeft].ToUnityVector3();
            //m_avatarModel.transform.Find("hip/L_leg").localRotation = Quaternion.AngleAxis(, Vector3.left)
            //m_avatarModel.transform.Find("hip/L_leg/L_knee").position = m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.KneeLeft].ToUnityVector3();
            //m_avatarModel.transform.Find("hip/L_leg/L_knee/L_ankle").position = m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.AnkleLeft].ToUnityVector3();
            //m_avatarModel.transform.Find("hip/L_leg/L_knee/L_ankle/L_foot/L_toes").position = m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.FootLeft].ToUnityVector3();

            //m_avatarModel.transform.Find("hip/R_leg").position = m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.HipRight].ToUnityVector3();
            //m_avatarModel.transform.Find("hip/R_leg/R_knee").position = m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.KneeRight].ToUnityVector3();
            //m_avatarModel.transform.Find("hip/R_leg/R_knee/R_ankle").position = m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.AnkleRight].ToUnityVector3();
            //m_avatarModel.transform.Find("hip/R_leg/R_knee/R_ankle/R_foot/R_toes").position = m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.FootRight].ToUnityVector3();

            //Debug.Log("L_Knee" + (Mathf.Acos(
            //            (m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.KneeLeft].PositionY -
            //             m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.AnkleLeft].PositionY) /
            //             (m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.KneeLeft].ToVector3() -
            //              m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.AnkleLeft].ToVector3()).magnitude)
            //             * Mathf.Rad2Deg));

            //m_avatarModel.transform.Find("hip/L_leg").localRotation = Quaternion.Euler(360 - Mathf.Acos(
            //            (m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.HipLeft].PositionY -
            //             m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.KneeLeft].PositionY) /
            //             (m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.HipLeft].ToVector3() -
            //              m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.KneeLeft].ToVector3()).magnitude)
            //             * Mathf.Rad2Deg, 180.082f, 179.6165f);

            //m_avatarModel.transform.Find("hip/R_leg").localRotation = Quaternion.Euler(360 - Mathf.Acos(
            //            (m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.HipRight].PositionY -
            //             m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.KneeRight].PositionY) /
            //             (m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.HipRight].ToVector3() -
            //              m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.KneeRight].ToVector3()).magnitude)
            //             * Mathf.Rad2Deg, 180.082f, 179.6165f);

            //m_avatarModel.transform.Find("hip/L_leg/L_knee").localRotation = Quaternion.Euler(340 - m_avatarModel.transform.Find("hip/L_leg").localRotation.eulerAngles.x - Mathf.Acos(
            //            (m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.KneeLeft].PositionY -
            //             m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.AnkleLeft].PositionY) /
            //             (m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.KneeLeft].ToVector3() -
            //              m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.AnkleLeft].ToVector3()).magnitude)
            //             * Mathf.Rad2Deg, 0, 0);

            //m_avatarModel.transform.Find("hip/R_leg/R_knee").localRotation = Quaternion.Euler(340 - Mathf.Acos(
            //            (m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.KneeRight].PositionY -
            //             m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.AnkleRight].PositionY) /
            //             (m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.KneeRight].ToVector3() -
            //              m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.AnkleRight].ToVector3()).magnitude)
            //             * Mathf.Rad2Deg, 0, 0);

            //m_avatarModel.transform.Find("hip/L_leg").localRotation = Quaternion.FromToRotation(Vector3.up,
            //    m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.KneeLeft].ToUnityVector3() -
            //    m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.HipLeft].ToUnityVector3()) * Quaternion.AngleAxis(-90, Vector3.right);


            ////Debug.Log("Angle " + Quaternion.FromToRotation(Vector3.up,
            ////    m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.KneeLeft].ToUnityVector3() -
            ////    m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.HipLeft].ToUnityVector3()).eulerAngles.ToString());
            //Debug.Log("Diff is " + (m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.KneeLeft].ToUnityVector3() -
            //    m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.HipLeft].ToUnityVector3()).normalized);

            //Vector2 kneePolar = PolarFromCartesian(m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.KneeLeft].ToUnityVector3() -
            //    m_userBody.LastBodies[0].Joints[TrackingServiceBodyJointTypes.HipLeft].ToUnityVector3());

            //Debug.Log("KneePolar" + (kneePolar * Mathf.Rad2Deg).ToString("0.00"));

            //m_avatarModel.transform.Find("hip/L_leg").localRotation = 
            //                                                 // Quaternion.AngleAxis(kneePolar.x * Mathf.Rad2Deg, Vector3.up) *
            //                                                 //Quaternion.AngleAxis(kneePolar.y * Mathf.Rad2Deg, new Vector3(Mathf.Cos(kneePolar.x), 0, Mathf.Sin(kneePolar.x)))
            //                                                 Quaternion.AngleAxis(90, Vector3.up) *
            //                                                 Quaternion.AngleAxis(kneePolar.x * Mathf.Rad2Deg, Vector3.up) *
            //                                                 Quaternion.AngleAxis(kneePolar.y * Mathf.Rad2Deg, Vector3.forward)
            //                                                 ;

            //    Quaternion.FromToRotation(Quaternion.AngleAxis(90, Vector3.right) * Vector3.up,
            //    Quaternion.AngleAxis(90, Vector3.right) * .Joints[].ToVector3() -
            //   Quaternion.AngleAxis(90, Vector3.right) * m_userBody.LastBodies[0].Joints[.ToVector3());
            //m_avatarModel.transform.Find("hip/L_leg").localRotation = Quaternion.Euler(m_avatarModel.transform.Find("hip/L_leg").localRotation.eulerAngles.x,
            //    m_avatarModel.transform.Find("hip/L_leg").localRotation.eulerAngles.y,
            //    180);

            TrackingServiceBodyData currentUserBody = bodyData;

            //global rotation of the body
            Quaternion globalBodyRotation = ComputeBodyRotation(currentUserBody);
            Quaternion globalBodyRotationInverse = Quaternion.Inverse(globalBodyRotation);
            m_avatarModel.transform.Find("hip").localRotation = globalBodyRotation;

            //rotations of left leg joints 

            m_avatarModel.transform.Find("hip/L_leg").localRotation = ComputeLegsJointRotation(currentUserBody, TrackingServiceBodyJointTypes.HipLeft,
                                                              TrackingServiceBodyJointTypes.KneeLeft, globalBodyRotationInverse, -1000, 0, 0);

            m_avatarModel.transform.Find("hip/L_leg/L_knee").localRotation = ComputeKneesJointRotation(currentUserBody, TrackingServiceBodyJointTypes.HipLeft, TrackingServiceBodyJointTypes.KneeLeft,
                                                              TrackingServiceBodyJointTypes.AnkleLeft, globalBodyRotationInverse, -1000, 0, 0);
            m_avatarModel.transform.Find("hip/L_leg/L_knee/L_ankle").localRotation = ComputeFeetJointRotation(currentUserBody, TrackingServiceBodyJointTypes.KneeLeft, TrackingServiceBodyJointTypes.AnkleLeft,
                                                              TrackingServiceBodyJointTypes.FootLeft, globalBodyRotationInverse, -1000, 0, 0);
            //rotations of right leg joints 
            m_avatarModel.transform.Find("hip/R_leg").localRotation = ComputeLegsJointRotation(currentUserBody, TrackingServiceBodyJointTypes.HipRight,
                                                             TrackingServiceBodyJointTypes.KneeRight, globalBodyRotationInverse, -1000, -0, 0);
            m_avatarModel.transform.Find("hip/R_leg/R_knee").localRotation = ComputeKneesJointRotation(currentUserBody, TrackingServiceBodyJointTypes.HipRight, TrackingServiceBodyJointTypes.KneeRight,
                                                              TrackingServiceBodyJointTypes.AnkleRight, globalBodyRotationInverse, -1000, 0, 0);
            m_avatarModel.transform.Find("hip/R_leg/R_knee/R_ankle").localRotation = ComputeFeetJointRotation(currentUserBody, TrackingServiceBodyJointTypes.KneeRight, TrackingServiceBodyJointTypes.AnkleRight,
                                                              TrackingServiceBodyJointTypes.FootRight, globalBodyRotationInverse, -1000, 0, 0);
            //rotations of neck&head joints 
            m_avatarModel.transform.Find("hip/spine").localRotation = ComputeSpinesJointRotation(currentUserBody, TrackingServiceBodyJointTypes.SpineBase, TrackingServiceBodyJointTypes.SpineShoulder, globalBodyRotationInverse,
                                                              -1000, -1000, 0);
            m_avatarModel.transform.Find("hip/spine/chest/neck/head").localRotation = ComputeSpinesJointRotation(currentUserBody, TrackingServiceBodyJointTypes.SpineShoulder, TrackingServiceBodyJointTypes.Neck, globalBodyRotationInverse,
                                                             -1000, -1000, 0);

            //rotations of left arm
            m_avatarModel.transform.Find("hip/spine/chest/L_shoulder/L_arm").localRotation = Quaternion.Inverse(m_avatarModel.transform.Find("hip/spine/chest/L_shoulder").localRotation) * ComputeSpinesJointRotation(currentUserBody, TrackingServiceBodyJointTypes.ShoulderLeft,
                                                              TrackingServiceBodyJointTypes.ElbowLeft, globalBodyRotationInverse, -1000, -1000, -1000);

            m_avatarModel.transform.Find("hip/spine/chest/L_shoulder/L_arm").localRotation = Quaternion.Euler(
            m_avatarModel.transform.Find("hip/spine/chest/L_shoulder/L_arm").localRotation.eulerAngles.x,
            m_avatarModel.transform.Find("hip/spine/chest/L_shoulder/L_arm").localRotation.eulerAngles.y,
            0);

            m_avatarModel.transform.Find("hip/spine/chest/L_shoulder/L_arm/L_elbow").localRotation = ComputeElbowJointRotation(currentUserBody, TrackingServiceBodyJointTypes.ShoulderLeft, TrackingServiceBodyJointTypes.ElbowLeft,
                                                              TrackingServiceBodyJointTypes.WristLeft, Quaternion.Inverse(m_avatarModel.transform.Find("hip/spine/chest/L_shoulder/L_arm").rotation), -1000, -1000, -1000);


            Quaternion rotWrist = ComputeWristJointRotation(currentUserBody, true,
                                                             Quaternion.Inverse(m_avatarModel.transform.Find("hip/spine/chest/L_shoulder/L_arm/L_elbow").rotation), -1000, -1000, -1000);

            m_avatarModel.transform.Find("hip/spine/chest/L_shoulder/L_arm/L_elbow/L_wrist").localRotation = Quaternion.Euler(0, 0, rotWrist.eulerAngles.z);
            //m_avatarModel.transform.Find("hip/spine/chest/L_shoulder/L_arm/L_elbow").localRotation *= Quaternion.Euler(0, 0, rotWrist.eulerAngles.z);

            //   float halfRotWristZ = rotWrist.eulerAngles.z;

            //   halfRotWristZ = MathUtilities.AdjustOrientation(halfRotWristZ * Mathf.Deg2Rad, 0);
            //   halfRotWristZ *= Mathf.Rad2Deg / 2;

            //   m_avatarModel.transform.Find("hip/spine/chest/L_shoulder/L_arm/L_elbow/L_wrist").localRotation = Quaternion.Euler(
            //       m_avatarModel.transform.Find("hip/spine/chest/L_shoulder/L_arm/L_elbow/L_wrist").localRotation.eulerAngles.x,
            //       m_avatarModel.transform.Find("hip/spine/chest/L_shoulder/L_arm/L_elbow/L_wrist").localRotation.eulerAngles.y,
            //   halfRotWristZ);

            //   m_avatarModel.transform.Find("hip/spine/chest/L_shoulder/L_arm/L_elbow").localRotation = Quaternion.Euler(
            //m_avatarModel.transform.Find("hip/spine/chest/L_shoulder/L_arm/L_elbow").localRotation.eulerAngles.x,
            //m_avatarModel.transform.Find("hip/spine/chest/L_shoulder/L_arm/L_elbow").localRotation.eulerAngles.y,
            //m_avatarModel.transform.Find("hip/spine/chest/L_shoulder/L_arm/L_elbow").localRotation.eulerAngles.z + halfRotWristZ);

            //rotations of right arm
            m_avatarModel.transform.Find("hip/spine/chest/R_shoulder/R_arm").localRotation = Quaternion.Inverse(m_avatarModel.transform.Find("hip/spine/chest/R_shoulder").localRotation) * ComputeSpinesJointRotation(currentUserBody, TrackingServiceBodyJointTypes.ShoulderRight,
                                                              TrackingServiceBodyJointTypes.ElbowRight, globalBodyRotationInverse, -1000, -1000, -1000);


            m_avatarModel.transform.Find("hip/spine/chest/R_shoulder/R_arm").localRotation = Quaternion.Euler(
            m_avatarModel.transform.Find("hip/spine/chest/R_shoulder/R_arm").localRotation.eulerAngles.x,
            m_avatarModel.transform.Find("hip/spine/chest/R_shoulder/R_arm").localRotation.eulerAngles.y,
            0);

            m_avatarModel.transform.Find("hip/spine/chest/R_shoulder/R_arm/R_elbow").localRotation = ComputeElbowJointRotation(currentUserBody, TrackingServiceBodyJointTypes.ShoulderRight, TrackingServiceBodyJointTypes.ElbowRight,
                                                             TrackingServiceBodyJointTypes.WristRight, Quaternion.Inverse(m_avatarModel.transform.Find("hip/spine/chest/R_shoulder/R_arm").rotation), -1000, -1000, -1000);


            rotWrist = ComputeWristJointRotation(currentUserBody, false,
                                                             Quaternion.Inverse(m_avatarModel.transform.Find("hip/spine/chest/R_shoulder/R_arm/R_elbow").rotation), -1000, -1000, -1000);

            m_avatarModel.transform.Find("hip/spine/chest/R_shoulder/R_arm/R_elbow/R_wrist").localRotation = Quaternion.Euler(0, 0, rotWrist.eulerAngles.z);

            //   halfRotWristZ = rotWrist.eulerAngles.z;

            //   halfRotWristZ = MathUtilities.AdjustOrientation(halfRotWristZ * Mathf.Deg2Rad, 0);
            //   halfRotWristZ *= Mathf.Rad2Deg / 2;

            //   m_avatarModel.transform.Find("hip/spine/chest/R_shoulder/R_arm/R_elbow/R_wrist").localRotation = Quaternion.Euler(
            //       m_avatarModel.transform.Find("hip/spine/chest/R_shoulder/R_arm/R_elbow/R_wrist").localRotation.eulerAngles.x,
            //       m_avatarModel.transform.Find("hip/spine/chest/R_shoulder/R_arm/R_elbow/R_wrist").localRotation.eulerAngles.y,
            //   halfRotWristZ);

            //   m_avatarModel.transform.Find("hip/spine/chest/R_shoulder/R_arm/R_elbow").localRotation = Quaternion.Euler(
            //m_avatarModel.transform.Find("hip/spine/chest/R_shoulder/R_arm/R_elbow").localRotation.eulerAngles.x,
            //m_avatarModel.transform.Find("hip/spine/chest/R_shoulder/R_arm/R_elbow").localRotation.eulerAngles.y,
            //m_avatarModel.transform.Find("hip/spine/chest/R_shoulder/R_arm/R_elbow").localRotation.eulerAngles.z + halfRotWristZ);

            //   m_avatarModel.transform.Find("hip/spine/chest/R_shoulder/R_arm/R_elbow").localRotation = Quaternion.Euler(
            //m_avatarModel.transform.Find("hip/spine/chest/R_shoulder/R_arm/R_elbow").localRotation.eulerAngles.x,
            //m_avatarModel.transform.Find("hip/spine/chest/R_shoulder/R_arm/R_elbow").localRotation.eulerAngles.y,
            //halfRotWristZ);

            if (trackPosition)
            {

                if (trackPosition)
                {
                    //compute overall scale of avatar
                    Vector3 avatarScale = (m_avatarModel.transform.parent.lossyScale);

                    //compute avatar position, removing the overall scale and rotation (because TrackingService reports coordinates with scale of 1.0 and zero rotation wrt ImmotionRoom frame of reference)
                    Vector3 unscaledDiff = (m_avatarModel.transform.Find("hip/spine").position - m_avatarModel.transform.parent.position);
                    unscaledDiff.Scale(new Vector3(1 / avatarScale.x, 1 / avatarScale.y, 1 / avatarScale.z));
                    unscaledDiff = Quaternion.Inverse(m_avatarModel.transform.rotation) * unscaledDiff;

                    //calculate displacement of actual body position to avatar position:
                    //0.05 because 0.5 is for the mean and 0.1 because ToUnityVector3 applies a 10x on the coordinates
                    //remember that both ImmotionRoom and UMA have center of reference on the floor, at the center of the feet
                    Vector3 displ = ((bodyData.Joints[TrackingServiceBodyJointTypes.SpineMid].ToUnityVector3() + bodyData.Joints[TrackingServiceBodyJointTypes.SpineBase].ToUnityVector3()) * 0.05f -
                                     unscaledDiff);

                    //apply global rotation, then scale the traslation component, then move the avatar to the new position
                    displ = m_avatarModel.transform.rotation * displ;
                    displ.Scale(avatarScale); //to take in count avatar scaling
                    m_avatarModel.transform.position += displ;

                }
            }

#if !UNITY_ANDROID
            //deform the collider to take in count new body pose
            //Mesh colliderMesh = new Mesh();
            //m_avatarModel.m_avatarModel.transform.GetChild(1).gameObject.GetComponent<SkinnedMeshRenderer>().BakeMesh(colliderMesh);
            //m_avatarModel.m_avatarModel.transform.GetChild(1).gameObject.GetComponent<MeshCollider>().sharedMesh = null;
            //m_avatarModel.m_avatarModel.transform.GetChild(1).gameObject.GetComponent<MeshCollider>().sharedMesh = colliderMesh;
#endif
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Compute global Amanda body orientation
        /// </summary>
        /// <param name="currentBody">Body to compute the main orientation of</param>
        /// <returns>Global rotation to assign to Amanda body</returns>
        private Quaternion ComputeBodyRotation(TrackingServiceBodyData currentBody)
        {
            //use shoulder orientation (remember to add 180 degrees, because Unity system is left handed)
            return Quaternion.Euler(
                    90, //because of orientation of Amanda model body
                    180 + Mathf.Rad2Deg * UnityUtilities.BetweenJointsXZOrientation(currentBody.Joints[TrackingServiceBodyJointTypes.ShoulderLeft].ToVector3(),
                                                                                    currentBody.Joints[TrackingServiceBodyJointTypes.ShoulderRight].ToVector3()),
                    0);
        }

        /// <summary>
        /// Compute rotation to be applied to Amanda's leg joint, using informations of body skeleton tracked by a tracking source
        /// </summary>
        /// <param name="body">Tracked body</param>
        /// <param name="rootJoint">Root joint type of the hip</param>
        /// <param name="destinationJoint">Root joint type of the knee</param>
        /// <param name="bodyGlobalRotationInverse">Inverse of Y-axis global rotation of the body</param>
        /// <param name="fixedXAngle">Angle to override to the Euler x angle tracked by the skeleton; use -1000 to not override</param>
        /// <param name="fixedYAngle">Angle to override to the Euler y angle tracked by the skeleton; use -1000 to not override</param>
        /// <param name="fixedZAngle">Angle to override to the Euler z angle tracked by the skeleton; use -1000 to not override</param>
        /// <returns></returns>
        private Quaternion ComputeLegsJointRotation(TrackingServiceBodyData body, TrackingServiceBodyJointTypes rootJoint, TrackingServiceBodyJointTypes destinationJoint, Quaternion bodyGlobalRotationInverse,
                                             float fixedXAngle = -1000, float fixedYAngle = -1000, float fixedZAngle = -1000)
        {
            //we start from the standard direction of the leg (up in Amanda body) and calculate the rotation to take it from the stretched position to current position.
            //bodyGlobalRotationInverse is to match unity global coordinate system to amanda leg coordinate system

            if (body == null)
            {
                return Quaternion.identity;
            }

            Quaternion localRotation = Quaternion.FromToRotation(bodyGlobalRotationInverse * Vector3.up,
                                                                 bodyGlobalRotationInverse * body.Joints[destinationJoint].ToUnityVector3() -
                                                                 bodyGlobalRotationInverse * body.Joints[rootJoint].ToUnityVector3());

            localRotation = Quaternion.Euler(fixedXAngle == -1000 ? (180 - localRotation.eulerAngles.x) : fixedXAngle,
                fixedYAngle == -1000 ? localRotation.eulerAngles.y : fixedYAngle,
                fixedZAngle == -1000 ? localRotation.eulerAngles.z : fixedZAngle);

            return localRotation;
        }

        /// <summary>
        /// Compute rotation to be applied to Amanda's knee joint, using informations of body skeleton tracked by a tracking source
        /// </summary>
        /// <param name="body">Tracked body</param>
        /// <param name="rootJoint">Root joint type of the hip</param>
        /// <param name="startJoint">Root joint type of the knee</param>
        /// <param name="destinationJoint">Root joint type of the ankle</param>
        /// <param name="bodyGlobalRotationInverse">Inverse of Y-axis global rotation of the body</param>
        /// <param name="fixedXAngle">Angle to override to the Euler x angle tracked by the skeleton; use -1000 to not override</param>
        /// <param name="fixedYAngle">Angle to override to the Euler y angle tracked by the skeleton; use -1000 to not override</param>
        /// <param name="fixedZAngle">Angle to override to the Euler z angle tracked by the skeleton; use -1000 to not override</param>
        private Quaternion ComputeKneesJointRotation(TrackingServiceBodyData body, TrackingServiceBodyJointTypes rootJoint, TrackingServiceBodyJointTypes startJoint, TrackingServiceBodyJointTypes destinationJoint, Quaternion bodyGlobalRotationInverse,
                                             float fixedXAngle = -1000, float fixedYAngle = -1000, float fixedZAngle = -1000)
        {
            //we start from the direction of the leg and calculate the rotation to take it from the stretched position to current position.
            //bodyGlobalRotationInverse is to match unity global coordinate system to amanda leg coordinate system
            Quaternion localRotation = Quaternion.FromToRotation(bodyGlobalRotationInverse * body.Joints[startJoint].ToUnityVector3() -
                                                                 bodyGlobalRotationInverse * body.Joints[rootJoint].ToUnityVector3(),
                                                                 bodyGlobalRotationInverse * body.Joints[destinationJoint].ToUnityVector3() -
                                                                 bodyGlobalRotationInverse * body.Joints[startJoint].ToUnityVector3());

            //fix some value, if required
            localRotation = Quaternion.Euler(fixedXAngle == -1000 ? localRotation.eulerAngles.x : fixedXAngle,
                fixedYAngle == -1000 ? localRotation.eulerAngles.y : fixedYAngle,
                fixedZAngle == -1000 ? localRotation.eulerAngles.z : fixedZAngle);

            return localRotation;
        }

        /// <summary>
        /// Compute rotation to be applied to Amanda's knee joint, using informations of body skeleton tracked by a tracking source
        /// </summary>
        /// <param name="body">Tracked body</param>
        /// <param name="rootJoint">Root joint type of the hip</param>
        /// <param name="startJoint">Root joint type of the knee</param>
        /// <param name="destinationJoint">Root joint type of the ankle</param>
        /// <param name="bodyGlobalRotationInverse">Inverse of Y-axis global rotation of the body</param>
        /// <param name="fixedXAngle">Angle to override to the Euler x angle tracked by the skeleton; use -1000 to not override</param>
        /// <param name="fixedYAngle">Angle to override to the Euler y angle tracked by the skeleton; use -1000 to not override</param>
        /// <param name="fixedZAngle">Angle to override to the Euler z angle tracked by the skeleton; use -1000 to not override</param>
        private Quaternion ComputeFeetJointRotation(TrackingServiceBodyData body, TrackingServiceBodyJointTypes rootJoint, TrackingServiceBodyJointTypes startJoint, TrackingServiceBodyJointTypes destinationJoint, Quaternion bodyGlobalRotationInverse,
                                             float fixedXAngle = -1000, float fixedYAngle = -1000, float fixedZAngle = -1000)
        {
            //we start from the direction of the knee and calculate the rotation to take it from the stretched position to current position.
            //bodyGlobalRotationInverse is to match unity global coordinate system to amanda leg coordinate system
            Quaternion localRotation = Quaternion.FromToRotation(bodyGlobalRotationInverse * body.Joints[startJoint].ToUnityVector3() -
                                                                 bodyGlobalRotationInverse * body.Joints[rootJoint].ToUnityVector3(),
                                                                 bodyGlobalRotationInverse * body.Joints[destinationJoint].ToUnityVector3() -
                                                                 bodyGlobalRotationInverse * body.Joints[startJoint].ToUnityVector3());

            //fix angles. Notice the -42 because amanda model has the foot that has a rotation of 42 that has to be considered
            localRotation = Quaternion.Euler(fixedXAngle == -1000 ? Mathf.Min(57, Mathf.Max(30, localRotation.eulerAngles.x - 42.0f)) : fixedXAngle,
                fixedYAngle == -1000 ? localRotation.eulerAngles.y : fixedYAngle,
                fixedZAngle == -1000 ? localRotation.eulerAngles.z : fixedZAngle);

            return localRotation;
        }

        /// <summary>
        /// Compute rotation to be applied to Amanda's leg joint, using informations of body skeleton tracked by a tracking source
        /// </summary>
        /// <param name="body">Tracked body</param>
        /// <param name="rootJoint">Root joint type of the hip</param>
        /// <param name="destinationJoint">Root joint type of the knee</param>
        /// <param name="bodyGlobalRotationInverse">Inverse of Y-axis global rotation of the body</param>
        /// <param name="fixedXAngle">Angle to override to the Euler x angle tracked by the skeleton; use -1000 to not override</param>
        /// <param name="fixedYAngle">Angle to override to the Euler y angle tracked by the skeleton; use -1000 to not override</param>
        /// <param name="fixedZAngle">Angle to override to the Euler z angle tracked by the skeleton; use -1000 to not override</param>
        /// <returns></returns>
        private Quaternion ComputeSpinesJointRotation(TrackingServiceBodyData body, TrackingServiceBodyJointTypes rootJoint, TrackingServiceBodyJointTypes destinationJoint, Quaternion bodyGlobalRotationInverse,
                                             float fixedXAngle = -1000, float fixedYAngle = -1000, float fixedZAngle = -1000)
        {
            //we start from the standard direction of the leg (up in Amanda body) and calculate the rotation to take it from the stretched position to current position.
            //bodyGlobalRotationInverse is to match unity global coordinate system to amanda leg coordinate system
            Quaternion localRotation = Quaternion.FromToRotation(bodyGlobalRotationInverse * Vector3.up,
                                                                 bodyGlobalRotationInverse * body.Joints[destinationJoint].ToUnityVector3() -
                                                                 bodyGlobalRotationInverse * body.Joints[rootJoint].ToUnityVector3());

            localRotation = Quaternion.Euler(fixedXAngle == -1000 ? (localRotation.eulerAngles.x) : fixedXAngle,
                fixedYAngle == -1000 ? localRotation.eulerAngles.y : fixedYAngle,
                fixedZAngle == -1000 ? localRotation.eulerAngles.z : fixedZAngle);

            return localRotation;
        }

        /// <summary>
        /// Compute rotation to be applied to Amanda's leg joint, using informations of body skeleton tracked by a tracking source
        /// </summary>
        /// <param name="body">Tracked body</param>
        /// <param name="rootJoint">Root joint type of the hip</param>
        /// <param name="destinationJoint">Root joint type of the knee</param>
        /// <param name="bodyGlobalRotationInverse">Inverse of Y-axis global rotation of the body</param>
        /// <param name="fixedXAngle">Angle to override to the Euler x angle tracked by the skeleton; use -1000 to not override</param>
        /// <param name="fixedYAngle">Angle to override to the Euler y angle tracked by the skeleton; use -1000 to not override</param>
        /// <param name="fixedZAngle">Angle to override to the Euler z angle tracked by the skeleton; use -1000 to not override</param>
        /// <returns></returns>
        private Quaternion ComputeSpinesJointRotationR(TrackingServiceBodyData body, TrackingServiceBodyJointTypes rootJoint, TrackingServiceBodyJointTypes destinationJoint, Quaternion bodyGlobalRotationInverse,
                                             float fixedXAngle = -1000, float fixedYAngle = -1000, float fixedZAngle = -1000)
        {
            //we start from the standard direction of the leg (up in Amanda body) and calculate the rotation to take it from the stretched position to current position.
            //bodyGlobalRotationInverse is to match unity global coordinate system to amanda leg coordinate system
            Quaternion localRotation = Quaternion.FromToRotation(bodyGlobalRotationInverse * Vector3.right,
                                                                 bodyGlobalRotationInverse * body.Joints[destinationJoint].ToUnityVector3() -
                                                                 bodyGlobalRotationInverse * body.Joints[rootJoint].ToUnityVector3());

            localRotation = Quaternion.Euler(fixedXAngle == -1000 ? (localRotation.eulerAngles.x) : fixedXAngle,
                fixedYAngle == -1000 ? localRotation.eulerAngles.y : fixedYAngle,
                fixedZAngle == -1000 ? localRotation.eulerAngles.z : fixedZAngle);

            return localRotation;
        }

        /// <summary>
        /// Compute rotation to be applied to Amanda's knee joint, using informations of body skeleton tracked by a tracking source
        /// </summary>
        /// <param name="body">Tracked body</param>
        /// <param name="rootJoint">Root joint type of the hip</param>
        /// <param name="startJoint">Root joint type of the knee</param>
        /// <param name="destinationJoint">Root joint type of the ankle</param>
        /// <param name="bodyGlobalRotationInverse">Inverse of Y-axis global rotation of the body</param>
        /// <param name="fixedXAngle">Angle to override to the Euler x angle tracked by the skeleton; use -1000 to not override</param>
        /// <param name="fixedYAngle">Angle to override to the Euler y angle tracked by the skeleton; use -1000 to not override</param>
        /// <param name="fixedZAngle">Angle to override to the Euler z angle tracked by the skeleton; use -1000 to not override</param>
        private Quaternion ComputeElbowJointRotation(TrackingServiceBodyData body, TrackingServiceBodyJointTypes rootJoint, TrackingServiceBodyJointTypes startJoint, TrackingServiceBodyJointTypes destinationJoint, Quaternion bodyGlobalRotationInverse,
                                             float fixedXAngle = -1000, float fixedYAngle = -1000, float fixedZAngle = -1000)
        {
            //we start from the direction of the leg and calculate the rotation to take it from the stretched position to current position.
            //90-degrees rotation is to match unity global coordinate system to amanda knee coordinate system
            Quaternion localRotation = Quaternion.FromToRotation(bodyGlobalRotationInverse * body.Joints[startJoint].ToUnityVector3() -
                                                                   bodyGlobalRotationInverse * body.Joints[rootJoint].ToUnityVector3(),
                                                                   bodyGlobalRotationInverse * body.Joints[destinationJoint].ToUnityVector3() -
                                                                   bodyGlobalRotationInverse * body.Joints[startJoint].ToUnityVector3());

            //fix some value, if required
            localRotation = Quaternion.Euler(fixedXAngle == -1000 ? localRotation.eulerAngles.x : fixedXAngle,
                fixedYAngle == -1000 ? localRotation.eulerAngles.y : fixedYAngle,
                fixedZAngle == -1000 ? localRotation.eulerAngles.z : fixedZAngle);

            return localRotation;
        }

        /// <summary>
        /// Compute rotation to be applied to Amanda's knee joint, using informations of body skeleton tracked by a tracking source
        /// </summary>
        /// <param name="body">Tracked body</param>
        /// <param name="isLeft">True if wrist is left, false if it is right</param>
        /// <param name="bodyGlobalRotationInverse">Inverse of Y-axis global rotation of the body</param>
        /// <param name="fixedXAngle">Angle to override to the Euler x angle tracked by the skeleton; use -1000 to not override</param>
        /// <param name="fixedYAngle">Angle to override to the Euler y angle tracked by the skeleton; use -1000 to not override</param>
        /// <param name="fixedZAngle">Angle to override to the Euler z angle tracked by the skeleton; use -1000 to not override</param>
        private Quaternion ComputeWristJointRotation(TrackingServiceBodyData body, bool isLeft, Quaternion bodyGlobalRotationInverse,
                                             float fixedXAngle = -1000, float fixedYAngle = -1000, float fixedZAngle = -1000)
        {
            //we start from the direction of the leg and calculate the rotation to take it from the stretched position to current position.
            //90-degrees rotation is to match unity global coordinate system to amanda knee coordinate system
            //Vector3 palmDorsDirection = -Vector3.Cross(bodyGlobalRotationInverse * body.Joints[isLeft ? TrackingServiceBodyJointTypes.ShoulderLeft : TrackingServiceBodyJointTypes.ShoulderRight].ToUnityVector3() -
            //    bodyGlobalRotationInverse * body.Joints[isLeft ? TrackingServiceBodyJointTypes.ElbowLeft : TrackingServiceBodyJointTypes.ElbowRight].ToUnityVector3(),
            //    bodyGlobalRotationInverse * body.Joints[isLeft ? TrackingServiceBodyJointTypes.WristLeft : TrackingServiceBodyJointTypes.HandRight].ToUnityVector3() -
            //    bodyGlobalRotationInverse * body.Joints[isLeft ? TrackingServiceBodyJointTypes.ElbowLeft : TrackingServiceBodyJointTypes.ElbowRight].ToUnityVector3());
            //Vector3 expectedThumbDirection = Vector3.Cross(bodyGlobalRotationInverse * body.Joints[isLeft ? TrackingServiceBodyJointTypes.HandTipLeft : TrackingServiceBodyJointTypes.HandTipRight].ToUnityVector3() -
            //    bodyGlobalRotationInverse * body.Joints[isLeft ? TrackingServiceBodyJointTypes.HandLeft : TrackingServiceBodyJointTypes.WristRight].ToUnityVector3(),
            //                                          palmDorsDirection);
            Vector3 currentThumbDirection = bodyGlobalRotationInverse * body.Joints[isLeft ? TrackingServiceBodyJointTypes.ThumbLeft : TrackingServiceBodyJointTypes.ThumbRight].ToUnityVector3() -
                bodyGlobalRotationInverse * body.Joints[isLeft ? TrackingServiceBodyJointTypes.HandLeft : TrackingServiceBodyJointTypes.HandRight].ToUnityVector3();
            Quaternion localRotation = Quaternion.FromToRotation(isLeft ? Vector3.left : Vector3.up, //I DONT KNOW WHY IT WORKS LIKE THIS AND I DON'T GIVE A F...
                                                                 currentThumbDirection);

            //fix some value, if required
            localRotation = Quaternion.Euler(fixedXAngle == -1000 ? localRotation.eulerAngles.x : fixedXAngle,
                fixedYAngle == -1000 ? localRotation.eulerAngles.y : fixedYAngle,
                fixedZAngle == -1000 ? (localRotation.eulerAngles.z) : fixedZAngle);

            return localRotation;
        }

        private Vector2 PolarFromCartesian(Vector3 cartesianCoordinate)
        {
            if (cartesianCoordinate.x == 0f)
                cartesianCoordinate.x = Mathf.Epsilon;
            float radius = cartesianCoordinate.magnitude;

            float polar = Mathf.Atan(cartesianCoordinate.z / cartesianCoordinate.x);

            if (cartesianCoordinate.x < 0f)
                polar += Mathf.PI;

            float elevation = Mathf.Asin(cartesianCoordinate.y / radius);

            return new Vector2(polar, elevation);
        }

        /// <summary>
        /// Get the transform representing the desired human joint.
        /// If there is not an avatar joint representing exactly the desired joint, the most accurate representation possible will be returned
        /// </summary>
        /// <param name="jointType">Joint Type of Interest</param>
        /// <returns>Transform, inside Unity scene, representing the desired joint type</returns>
        protected Transform ProtectedGetJointTransform(TrackingServiceBodyJointTypes jointType)
        {
            switch(jointType)
            {
                case TrackingServiceBodyJointTypes.AnkleLeft:
                    return m_avatarModel.transform.Find("hip/L_leg/L_knee/L_ankle");

                case TrackingServiceBodyJointTypes.AnkleRight:
                    return m_avatarModel.transform.Find("hip/R_leg/R_knee/R_ankle");

                case TrackingServiceBodyJointTypes.ElbowLeft:
                    return m_avatarModel.transform.Find("hip/spine/chest/L_shoulder/L_arm/L_elbow");

                case TrackingServiceBodyJointTypes.ElbowRight:
                    return m_avatarModel.transform.Find("hip/spine/chest/R_shoulder/R_arm/R_elbow");

                case TrackingServiceBodyJointTypes.FootLeft:
                    return m_avatarModel.transform.Find("hip/L_leg/L_knee/L_ankle/L_foot");

                case TrackingServiceBodyJointTypes.FootRight:
                    return m_avatarModel.transform.Find("hip/R_leg/R_knee/R_ankle/R_foot");

                case TrackingServiceBodyJointTypes.HandLeft:
                    return m_avatarModel.transform.Find("hip/spine/chest/L_shoulder/L_arm/L_elbow/L_wrist/L_middle1"); //THERE IS NOT A HAND JOINT IN AMANDA

                case TrackingServiceBodyJointTypes.HandRight:
                    return m_avatarModel.transform.Find("hip/spine/chest/R_shoulder/R_arm/R_elbow/R_wrist/R_middle1"); //THERE IS NOT A HAND JOINT IN AMANDA

                case TrackingServiceBodyJointTypes.HandTipLeft:
                    return m_avatarModel.transform.Find("hip/spine/chest/L_shoulder/L_arm/L_elbow/L_wrist/L_point1/L_point2/L_point3/Joint_3"); 

                case TrackingServiceBodyJointTypes.HandTipRight:
                    return m_avatarModel.transform.Find("hip/spine/chest/R_shoulder/R_arm/R_elbow/R_wrist/R_point1/R_point2/R_point3/Joint_3_5"); 

                case TrackingServiceBodyJointTypes.Head:
                    return m_avatarModel.transform.Find("hip/spine/chest/neck/head");

                case TrackingServiceBodyJointTypes.HipLeft:
                    return m_avatarModel.transform.Find("hip/L_leg");

                case TrackingServiceBodyJointTypes.HipRight:
                    return m_avatarModel.transform.Find("hip/R_leg");

                case TrackingServiceBodyJointTypes.KneeLeft:
                    return m_avatarModel.transform.Find("hip/L_leg/L_knee");

                case TrackingServiceBodyJointTypes.KneeRight:
                    return m_avatarModel.transform.Find("hip/R_leg/R_knee");

                case TrackingServiceBodyJointTypes.Neck:
                    return m_avatarModel.transform.Find("hip/spine/chest/neck");

                case TrackingServiceBodyJointTypes.ShoulderLeft:
                    return m_avatarModel.transform.Find("hip/spine/chest/L_shoulder");

                case TrackingServiceBodyJointTypes.ShoulderRight:
                    return m_avatarModel.transform.Find("hip/spine/chest/R_shoulder");

                case TrackingServiceBodyJointTypes.SpineBase:
                    return m_avatarModel.transform.Find("hip/spine/kniferoot"); //it is the most similar to that joint

                case TrackingServiceBodyJointTypes.SpineMid:
                    return m_avatarModel.transform.Find("hip/spine/kniferoot"); //it is the most similar to that joint

                case TrackingServiceBodyJointTypes.SpineShoulder:
                    return m_avatarModel.transform.Find("hip/spine"); //it is the most similar to that joint

                case TrackingServiceBodyJointTypes.ThumbLeft:
                    return m_avatarModel.transform.Find("hip/spine/chest/L_shoulder/L_arm/L_elbow/L_wrist/L_thumb1/L_thumb2/L_thumb3/Joint_4"); 

                case TrackingServiceBodyJointTypes.ThumbRight:
                    return m_avatarModel.transform.Find("hip/spine/chest/R_shoulder/R_arm/R_elbow/R_wrist/R_thumb1/R_thumb2/R_thumb3/Joint_4_2"); 

                case TrackingServiceBodyJointTypes.WristLeft:
                    return m_avatarModel.transform.Find("hip/spine/chest/L_shoulder/L_arm/L_elbow/L_wrist");

                case TrackingServiceBodyJointTypes.WristRight:
                    return m_avatarModel.transform.Find("hip/spine/chest/R_shoulder/R_arm/R_elbow/R_wrist");     

                default:
                    return m_avatarModel.transform.Find("hip");
            }
        }

        #endregion
    }
}

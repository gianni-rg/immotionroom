namespace ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Skeletals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using ImmotionRoom.Tools.Unity3d.Tools;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Collisions;

    /// <summary>
    /// Class to manage a bare-bones skeletal Avatar of the user body
    /// </summary>
    internal class SkeletalAvatarer : Avatarer
    {
        #region Protected fields

        /// <summary>
        /// Material to draw the joints with
        /// </summary>
        protected Material m_jointsMaterial;

        /// <summary>
        /// Material to draw joints lines with
        /// </summary>
        protected Material m_limbsMaterial;

        /// <summary>
        /// Positive color to be used in the joint drawing.
        /// </summary>
        protected Color m_positiveColor;

        /// <summary>
        /// Negative color to be used in the joint drawing.
        /// This is the one used for low confidence joints
        /// </summary>
        protected Color m_negativeColor;

        /// <summary>
        /// Color to be used to draw the bones of the skeleton
        /// </summary>
        protected Color m_limbsColor;

        /// <summary>
        /// Radius of the sphere representing each drawn joint
        /// </summary>
        protected float m_jointSphereRadius;

        /// <summary>
        /// Thickness of lines connecting consecutive joints
        /// </summary>
        protected float m_connectingLinesThickness;

        /// <summary>
        /// True to add colliders for hands and feet, false otherwise
        /// </summary>
        protected bool m_addColliders;

        /// <summary>
        /// True if the body has to cast/receive shadows, false otherwise
        /// </summary>
        protected bool m_shadowsEnabled;

        /// <summary>
        /// Root game objects of the skeleton joints
        /// </summary>
        protected GameObject m_rootGameObject;

        #endregion
        
        #region Protected static properties

        // Dictionary Enum Optimization
        // See: http://www.codeproject.com/Articles/33528/Accelerating-Enum-Based-Dictionaries-with-Generic
        // See: http://www.somasim.com/blog/2015/08/c-performance-tips-for-unity-part-2-structs-and-enums/
        // See: http://stackoverflow.com/questions/7143948/efficiency-of-using-iequalitycomparer-in-dictionary-vs-hashcode-and-equals

        /// <summary>
        /// Maps each joint with the father joint int the bones tree of the human body
        /// </summary>
        protected static readonly Dictionary<TrackingServiceBodyJointTypes, TrackingServiceBodyJointTypes> BoneTreeMap = new Dictionary<TrackingServiceBodyJointTypes, TrackingServiceBodyJointTypes>(TrackingServiceBodyJointTypesComparer.Instance)
        {
            { TrackingServiceBodyJointTypes.FootLeft, TrackingServiceBodyJointTypes.AnkleLeft },
            { TrackingServiceBodyJointTypes.AnkleLeft, TrackingServiceBodyJointTypes.KneeLeft },
            { TrackingServiceBodyJointTypes.KneeLeft, TrackingServiceBodyJointTypes.HipLeft },
            { TrackingServiceBodyJointTypes.HipLeft, TrackingServiceBodyJointTypes.SpineBase },
            { TrackingServiceBodyJointTypes.FootRight, TrackingServiceBodyJointTypes.AnkleRight },
            { TrackingServiceBodyJointTypes.AnkleRight, TrackingServiceBodyJointTypes.KneeRight },
            { TrackingServiceBodyJointTypes.KneeRight, TrackingServiceBodyJointTypes.HipRight },
            { TrackingServiceBodyJointTypes.HipRight, TrackingServiceBodyJointTypes.SpineBase },
            { TrackingServiceBodyJointTypes.HandTipLeft, TrackingServiceBodyJointTypes.HandLeft },
            { TrackingServiceBodyJointTypes.ThumbLeft, TrackingServiceBodyJointTypes.HandLeft },
            { TrackingServiceBodyJointTypes.HandLeft, TrackingServiceBodyJointTypes.WristLeft },
            { TrackingServiceBodyJointTypes.WristLeft, TrackingServiceBodyJointTypes.ElbowLeft },
            { TrackingServiceBodyJointTypes.ElbowLeft, TrackingServiceBodyJointTypes.ShoulderLeft },
            { TrackingServiceBodyJointTypes.ShoulderLeft, TrackingServiceBodyJointTypes.SpineShoulder },
            { TrackingServiceBodyJointTypes.HandTipRight, TrackingServiceBodyJointTypes.HandRight },
            { TrackingServiceBodyJointTypes.ThumbRight, TrackingServiceBodyJointTypes.HandRight },
            { TrackingServiceBodyJointTypes.HandRight, TrackingServiceBodyJointTypes.WristRight },
            { TrackingServiceBodyJointTypes.WristRight, TrackingServiceBodyJointTypes.ElbowRight },
            { TrackingServiceBodyJointTypes.ElbowRight, TrackingServiceBodyJointTypes.ShoulderRight },
            { TrackingServiceBodyJointTypes.ShoulderRight, TrackingServiceBodyJointTypes.SpineShoulder },
            { TrackingServiceBodyJointTypes.SpineBase, TrackingServiceBodyJointTypes.SpineMid },
            { TrackingServiceBodyJointTypes.SpineMid, TrackingServiceBodyJointTypes.SpineShoulder },
            { TrackingServiceBodyJointTypes.SpineShoulder, TrackingServiceBodyJointTypes.Neck },
            { TrackingServiceBodyJointTypes.Neck, TrackingServiceBodyJointTypes.Head },
        };     

        /// <summary>
        /// All possible body joint types
        /// </summary>
        protected static readonly IEnumerable<TrackingServiceBodyJointTypes> JointTypes = TrackingServiceBodyJointTypes.GetValues(typeof(TrackingServiceBodyJointTypes)).Cast<TrackingServiceBodyJointTypes>();
        
        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="skeletonRoot">GameObject that will be the father of the skeleton</param>
        /// <param name="jointsMaterial">Material to draw the joints with</param>
        /// <param name="limbsMaterial">Material to draw joints lines with</param>
        /// <param name="positiveColor">Positive color to be used in the joint drawing</param>
        /// <param name="negativeColor">Negative color to be used in the joint drawing</param>
        /// <param name="limbsColor">Color to be used in the bones drawing</param>
        /// <param name="jointSphereRadius">Radius of the sphere representing each drawn joint</param>
        /// <param name="connectingLinesThickness">Thickness of lines connecting consecutive joints</param>
        /// <param name="addColliders">True to add colliders for hands and feets, false otherwise</param>
        /// <param name="shadowsEnabled">True if the body has to cast/receive shadows, false otherwise</param>
        internal SkeletalAvatarer(GameObject skeletonRoot,
                                  Material jointsMaterial, Material limbsMaterial, Color positiveColor, Color negativeColor, Color limbsColor,
                                  float jointSphereRadius, float connectingLinesThickness, bool addColliders, bool shadowsEnabled)
        {
            m_rootGameObject = new GameObject("Skeleton");
            m_rootGameObject.transform.SetParent(skeletonRoot.transform, false);
            m_jointsMaterial = jointsMaterial;
            m_limbsMaterial = limbsMaterial;
            m_positiveColor = positiveColor;
            m_negativeColor = negativeColor;
            m_limbsColor = limbsColor;
            m_jointSphereRadius = jointSphereRadius;
            m_connectingLinesThickness = connectingLinesThickness;
            m_addColliders = addColliders;
            m_shadowsEnabled = shadowsEnabled;

            if(Log.IsDebugEnabled)
            {
                Log.Debug("Skeletal Avatarer created");
            }
        }

        #endregion

        #region Avatarer members

        /// <summary>
        /// Gets the root transform of the controlled avatar object, in Unity frame of reference.
        /// Root corresponds to the main joint of the avatar (usually spine hip or spine mid point), the father all of others
        /// </summary>
        protected internal override Transform BodyRootJoint
        {
            get
            {
                return m_rootGameObject.transform.Find(TrackingServiceBodyJointTypes.SpineBase.ToString());
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
                return m_rootGameObject.transform.Find(TrackingServiceBodyJointTypes.Head.ToString()).position;
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
            //create the skeleton
            CreateSkeletonObject(m_addColliders);

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Skeletal Avatarer initialized");
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
        /// Creates the skeleton object, initializing all internal structures
        /// </summary>
        /// <param name="addColliders">True to add colliders for hands and feets, false otherwise</param>
        protected void CreateSkeletonObject(bool addColliders)
        {
            // Create all body joint objects as child and associate them a sphere and a connect line (for limbs)
            foreach (TrackingServiceBodyJointTypes jt in JointTypes)
            {
                GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                jointObj.GetComponent<MeshRenderer>().material = m_jointsMaterial;
                jointObj.GetComponent<Collider>().enabled = false; //disable colliders

                LineRenderer lr = jointObj.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.material = m_limbsMaterial;
                float lossyScaleMean = (Mathf.Abs(m_rootGameObject.transform.lossyScale.x) + Mathf.Abs(m_rootGameObject.transform.lossyScale.y) + Mathf.Abs(m_rootGameObject.transform.lossyScale.z)) / 3;
                lr.startWidth = lr.endWidth = m_connectingLinesThickness * lossyScaleMean;

                jointObj.transform.localScale = new Vector3(m_jointSphereRadius, m_jointSphereRadius, m_jointSphereRadius);
                jointObj.name = jt.ToString();
                jointObj.transform.SetParent(m_rootGameObject.transform, false);
            }

            //add collider objects to hands and feet, if required
            if(addColliders)
            {
                //left foot
                GameObject colliderGo = new GameObject(AvatarCollidersProps.LeftFootColliders.ObjectName);
                //colliderGo.tag = AvatarCollidersProps.LeftFootColliders.ObjectTag;
                SphereCollider sphereCollider = colliderGo.AddComponent<SphereCollider>();
                sphereCollider.center = Vector3.zero;
                sphereCollider.radius = 1;
                sphereCollider.isTrigger = true;
                Rigidbody colliderRb = colliderGo.AddComponent<Rigidbody>(); //add rigidbody too, or collision detection won't work
                colliderRb.isKinematic = false;
                colliderRb.useGravity = false;
                colliderGo.transform.SetParent(m_rootGameObject.transform.Find(TrackingServiceBodyJointTypes.FootLeft.ToString()), false);

                //right foot
                colliderGo = new GameObject(AvatarCollidersProps.RightFootColliders.ObjectName);
                //colliderGo.tag = AvatarCollidersProps.RightFootColliders.ObjectTag;
                sphereCollider = colliderGo.AddComponent<SphereCollider>();
                sphereCollider.center = Vector3.zero;
                sphereCollider.radius = 1;
                sphereCollider.isTrigger = true;
                colliderRb = colliderGo.AddComponent<Rigidbody>(); //add rigidbody too, or collision detection won't work
                colliderRb.isKinematic = false;
                colliderRb.useGravity = false;
                colliderGo.transform.SetParent(m_rootGameObject.transform.Find(TrackingServiceBodyJointTypes.FootRight.ToString()), false);

                //left hand
                colliderGo = new GameObject(AvatarCollidersProps.LeftHandColliders.ObjectName);
                //colliderGo.tag = AvatarCollidersProps.LeftHandColliders.ObjectTag;
                sphereCollider = colliderGo.AddComponent<SphereCollider>();
                sphereCollider.center = Vector3.zero;
                sphereCollider.radius = 1;
                sphereCollider.isTrigger = true;
                colliderRb = colliderGo.AddComponent<Rigidbody>(); //add rigidbody too, or collision detection won't work
                colliderRb.isKinematic = false;
                colliderRb.useGravity = false;
                colliderGo.transform.SetParent(m_rootGameObject.transform.Find(TrackingServiceBodyJointTypes.HandLeft.ToString()), false);

                //right hand
                colliderGo = new GameObject(AvatarCollidersProps.RightHandColliders.ObjectName);
                //colliderGo.tag = AvatarCollidersProps.RightHandColliders.ObjectTag;
                sphereCollider = colliderGo.AddComponent<SphereCollider>();
                sphereCollider.center = Vector3.zero;
                sphereCollider.radius = 1;
                sphereCollider.isTrigger = true;
                colliderRb = colliderGo.AddComponent<Rigidbody>(); //add rigidbody too, or collision detection won't work
                colliderRb.isKinematic = false;
                colliderRb.useGravity = false;
                colliderGo.transform.SetParent(m_rootGameObject.transform.Find(TrackingServiceBodyJointTypes.HandRight.ToString()), false);
            }

            //if required, disable shadows from all renderable objects
            if(!m_shadowsEnabled)
            {
                Renderer[] renderables = m_rootGameObject.transform.GetComponentsInChildren<Renderer>();

                foreach(Renderer renderable in renderables)
                {
                    renderable.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderable.receiveShadows = false;
                }
            }
        }

        /// <summary>
        /// Calculate the color the joint has to be drawn with
        /// </summary>
        /// <param name="jointData">Joint the calculus has to be made</param>
        /// <returns>Color to drawn the joint with</returns>
        protected Color CalculateJointColor(TrackingServiceBodyJointData jointData)
        {
            // Confidence-based: the more confidence is near to 1, the more the joint is positive-coloured; otherwise it gets negative-coloured
            return jointData.Confidence * m_positiveColor + (1 - jointData.Confidence) * m_negativeColor;
        }

        /// <summary>
        /// Updates the avatar, given new body data
        /// </summary>
        /// <param name="bodyData">New data with which the avatar should be updated. Assumed not null</param>
        /// <param name="injectedPoses">Poses to inject inside the final avatar, expressed as rotations in Unity world frame of reference. For this joints, the value read from the bodyData gets ignored, and the one from this dictionary gets used.</param>
        /// <param name="trackPosition">True if the avatar should match user position in world space, false to track only pose</param>
        protected void ProtectedUpdate(TrackingServiceBodyData bodyData, Dictionary<TrackingServiceBodyJointTypes, Quaternion> injectedPoses, bool trackPosition = false)
        {
            Vector3 globalTraslation = Vector3.zero;

            //this kind of skeletal drawing automatically takes in count traslation. If it was not required, put skeleton basin center
            //at coordinate (0, 1, 0)
            if (!trackPosition)
            {
                globalTraslation = new Vector3(0, 1, 0) - bodyData.Joints[TrackingServiceBodyJointTypes.SpineBase].ToUnityVector3() * 0.1f;
            }

            // Loop through body joints
            foreach (TrackingServiceBodyJointData jointData in bodyData.Joints.Values)
            {
                // Get joint sphere object and move it to detected position
                Transform jointObj = m_rootGameObject.transform.Find(jointData.JointType.ToString());
                jointObj.localPosition = globalTraslation + jointData.ToUnityVector3() * 0.1f; //Unity Vector3 multiplies by 10... we don't need that feature here                

                // Draw it with correct color
                GameObject jointGameObj = jointObj.gameObject;
                jointGameObj.GetComponent<Renderer>().material.color = CalculateJointColor(jointData);

                // Find if this joint has to be connected to a father joint with a line, and if it is so, connect them
                TrackingServiceBodyJointData targetJoint = null;
                LineRenderer lineRenderer = jointObj.GetComponent<LineRenderer>();

                if (BoneTreeMap.ContainsKey(jointData.JointType))
                {
                    targetJoint = bodyData.Joints[BoneTreeMap[jointData.JointType]];
                    lineRenderer.startColor = m_limbsColor;
                    lineRenderer.endColor = m_limbsColor;
                    lineRenderer.SetPosition(0, jointObj.position);
                    lineRenderer.SetPosition(1, m_rootGameObject.transform.TransformPoint(globalTraslation + targetJoint.ToUnityVector3() * 0.1f));
                }
                else
                {
                    lineRenderer.enabled = false;
                }
            }

            //if it was requested to override the orientation of the neck
            if (injectedPoses != null && injectedPoses.ContainsKey(TrackingServiceBodyJointTypes.Neck))
            {
                //get head and neck objects
                Transform neck = m_rootGameObject.transform.Find(TrackingServiceBodyJointTypes.Neck.ToString());
                Transform head = m_rootGameObject.transform.Find(TrackingServiceBodyJointTypes.Head.ToString());

                //compute distance between head and neck
                float neckLen = m_rootGameObject.transform.lossyScale.y * UnityUtilities.BetweenJointsDistance(bodyData, TrackingServiceBodyJointTypes.Neck, TrackingServiceBodyJointTypes.Head);

                //recalc the head as the point that is above the neck of a length of the neck length, with the required orientation
                head.position = neck.position + injectedPoses[TrackingServiceBodyJointTypes.Neck] * new Vector3(0, neckLen, 0);

                //change line renderer coordinates to show the limb correctly (we've moved head pos, so it is not anymore where we set it inside the loop)
                LineRenderer lineRenderer = neck.GetComponent<LineRenderer>();
                lineRenderer.SetPosition(1, head.position);
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
            return m_rootGameObject.transform.Find(jointType.ToString());
        }

        #endregion
    }
}

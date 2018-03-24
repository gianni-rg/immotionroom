namespace ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Uma.Generators
{
    using ImmotionAR.ImmotionRoom.LittleBoots.Avateering;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Abstract class for behaviours generating UMA-like Avatars inside the scene
    /// </summary>
    public abstract class UmaBodyGenerator : MonoBehaviour,
        IUmaBodyGenerator
    {
        //#region Constants

        ///// <summary>
        ///// Standard UMA joint mappings inside game object
        ///// </summary>
        //protected static readonly Dictionary<UmaJointTypes, string> StandardUmaJointMappingsDictionary = new Dictionary<UmaJointTypes, string>()
        //    {
        //        {UmaJointTypes.Root, "Root"},
        //        {UmaJointTypes.Position, "Root/Global/Position"},

        //        {UmaJointTypes.Hips, "Root/Global/Position/Hips"},
        //        {UmaJointTypes.LowerBack, "Root/Global/Position/Hips/LowerBack"},
        //        {UmaJointTypes.Spine, "Root/Global/Position/Hips/LowerBack/Spine"},

        //        {UmaJointTypes.Neck, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/Neck"},
        //        {UmaJointTypes.Head, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/Neck/Head"},

        //        {UmaJointTypes.RightShoulder, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder"},
        //        {UmaJointTypes.RightArm, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm"},
        //        {UmaJointTypes.RightForeArm, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm"},
        //        {UmaJointTypes.RightForeArmTwist, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightForeArmTwist"},
        //        {UmaJointTypes.RightHand, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightHand"},
        //        {UmaJointTypes.RightHandLittle, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightHand/RightHandFinger01_01"},
        //        {UmaJointTypes.RightHandRing, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightHand/RightHandFinger02_01"},
        //        {UmaJointTypes.RightHandMiddle, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightHand/RightHandFinger03_01"},
        //        {UmaJointTypes.RightHandIndex, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightHand/RightHandFinger04_01"},
        //        {UmaJointTypes.RightHandThumb, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightHand/RightHandFinger05_01"},

        //        {UmaJointTypes.LeftShoulder, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder"},
        //        {UmaJointTypes.LeftArm, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm"},
        //        {UmaJointTypes.LeftForeArm, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm"},
        //        {UmaJointTypes.LeftForeArmTwist, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftForeArmTwist"},
        //        {UmaJointTypes.LeftHand, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftHand"},
        //        {UmaJointTypes.LeftHandLittle, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftHand/LeftHandFinger01_01"},
        //        {UmaJointTypes.LeftHandRing, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftHand/LeftHandFinger02_01"},
        //        {UmaJointTypes.LeftHandMiddle, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftHand/LeftHandFinger03_01"},
        //        {UmaJointTypes.LeftHandIndex, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftHand/LeftHandFinger04_01"},
        //        {UmaJointTypes.LeftHandThumb, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftHand/LeftHandFinger05_01"},

        //        {UmaJointTypes.RightUpLeg, "Root/Global/Position/Hips/RightUpLeg"},
        //        {UmaJointTypes.RightLeg, "Root/Global/Position/Hips/RightUpLeg/RightLeg"},
        //        {UmaJointTypes.RightFoot, "Root/Global/Position/Hips/RightUpLeg/RightLeg/RightFoot"},
        //        {UmaJointTypes.RightToeBase, "Root/Global/Position/Hips/RightUpLeg/RightLeg/RightFoot/RightToeBase"},

        //        {UmaJointTypes.LeftUpLeg, "Root/Global/Position/Hips/LeftUpLeg"},
        //        {UmaJointTypes.LeftLeg, "Root/Global/Position/Hips/LeftUpLeg/LeftLeg"},
        //        {UmaJointTypes.LeftFoot, "Root/Global/Position/Hips/LeftUpLeg/LeftLeg/LeftFoot"},
        //        {UmaJointTypes.LeftToeBase, "Root/Global/Position/Hips/LeftUpLeg/LeftLeg/LeftFoot/LeftToeBase"},
        
        //    };

        //#endregion

        #region Unity public properties

        /// <summary>
        /// Reference to the object containing the Uma kit for avatar generation
        /// (the kit contains the UmaContext, UmaGenerator and all stuff necessary to generate UMA Avatars).
        /// Use null to load the default Uma kit.
        /// </summary>
        /// <remarks>
        /// See this great tutorial https://www.youtube.com/playlist?list=PLkDHFObfS19wRJ9vvaDTwCe-zRPv9jI25 for explanations on UMA
        /// and the use of an UmaKit object
        /// </remarks>
        [Tooltip("Reference to the Uma Kit (object holding UmaContext and UmaGenerator) in the scene or in the prefabs. Leave null to load the default kit.")]
        public GameObject UmaKit;

        /// <summary>
        /// True if the UmaKit is a prefab to be instantiated, false if it is an object inside the scene
        /// </summary>
        [Tooltip("True if the UmaKit is a prefab to be instantiated, false if it is an object inside the scene")]
        public bool UmaKitIsPrefab;

        #endregion

        #region Public Properties

        /// <summary>
        /// Standard UMA joint mappings inside game object
        /// </summary>
        public static Dictionary<UmaJointTypes, string> StandardUmaJointMappings
        {
            get
            {
                return new Dictionary<UmaJointTypes, string>()
                {
                    {UmaJointTypes.Root, "Root"},
                    {UmaJointTypes.Position, "Root/Global/Position"},

                    {UmaJointTypes.Hips, "Root/Global/Position/Hips"},
                    {UmaJointTypes.LowerBack, "Root/Global/Position/Hips/LowerBack"},
                    {UmaJointTypes.Spine, "Root/Global/Position/Hips/LowerBack/Spine"},
                    {UmaJointTypes.SpineUp, "Root/Global/Position/Hips/LowerBack/Spine/Spine1"},

                    {UmaJointTypes.Neck, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/Neck"},
                    {UmaJointTypes.Head, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/Neck/Head"},
                    {UmaJointTypes.LeftEye, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/Neck/Head/LeftEye"},
                    {UmaJointTypes.RightEye, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/Neck/Head/RightEye"},

                    {UmaJointTypes.RightShoulder, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder"},
                    {UmaJointTypes.RightArm, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm"},
                    {UmaJointTypes.RightForeArm, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm"},
                    {UmaJointTypes.RightForeArmTwist, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightForeArmTwist"},
                    {UmaJointTypes.RightHand, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightHand"},

                    {UmaJointTypes.RightHandLittle, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightHand/RightHandFinger01_01"},
                    {UmaJointTypes.RightHandLittle_1, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightHand/RightHandFinger01_01/RightHandFinger01_02"},
                    {UmaJointTypes.RightHandLittle_2, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightHand/RightHandFinger01_01/RightHandFinger01_02/RightHandFinger01_03"},
                    {UmaJointTypes.RightHandRing, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightHand/RightHandFinger02_01"},
                    {UmaJointTypes.RightHandRing_1, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightHand/RightHandFinger02_01/RightHandFinger02_02"},
                    {UmaJointTypes.RightHandRing_2, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightHand/RightHandFinger02_01/RightHandFinger02_02/RightHandFinger02_03"},
                    {UmaJointTypes.RightHandMiddle, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightHand/RightHandFinger03_01"},
                    {UmaJointTypes.RightHandMiddle_1, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightHand/RightHandFinger03_01/RightHandFinger03_02"},
                    {UmaJointTypes.RightHandMiddle_2, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightHand/RightHandFinger03_01/RightHandFinger03_02/RightHandFinger03_03"},
                    {UmaJointTypes.RightHandIndex, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightHand/RightHandFinger04_01"},
                    {UmaJointTypes.RightHandIndex_1, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightHand/RightHandFinger04_01/RightHandFinger04_02"},
                    {UmaJointTypes.RightHandIndex_2, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightHand/RightHandFinger04_01/RightHandFinger04_02/RightHandFinger04_03"},
                    {UmaJointTypes.RightHandThumb, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightHand/RightHandFinger05_01"},
                    {UmaJointTypes.RightHandThumb_1, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightHand/RightHandFinger05_01/RightHandFinger05_02"},
                    {UmaJointTypes.RightHandThumb_2, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightHand/RightHandFinger05_01/RightHandFinger05_02/RightHandFinger05_03"},

                    {UmaJointTypes.LeftShoulder, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder"},
                    {UmaJointTypes.LeftArm, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm"},
                    {UmaJointTypes.LeftForeArm, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm"},
                    {UmaJointTypes.LeftForeArmTwist, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftForeArmTwist"},
                    {UmaJointTypes.LeftHand, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftHand"},
                    
                    {UmaJointTypes.LeftHandLittle, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftHand/LeftHandFinger01_01"},
                    {UmaJointTypes.LeftHandLittle_1, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftHand/LeftHandFinger01_01/LeftHandFinger01_02"},
                    {UmaJointTypes.LeftHandLittle_2, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftHand/LeftHandFinger01_01/LeftHandFinger01_02/LeftHandFinger01_03"},
                    {UmaJointTypes.LeftHandRing, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftHand/LeftHandFinger02_01"},
                    {UmaJointTypes.LeftHandRing_1, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftHand/LeftHandFinger02_01/LeftHandFinger02_02"},
                    {UmaJointTypes.LeftHandRing_2, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftHand/LeftHandFinger02_01/LeftHandFinger02_02/LeftHandFinger02_03"},
                    {UmaJointTypes.LeftHandMiddle, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftHand/LeftHandFinger03_01"},
                    {UmaJointTypes.LeftHandMiddle_1, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftHand/LeftHandFinger03_01/LeftHandFinger03_02"},
                    {UmaJointTypes.LeftHandMiddle_2, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftHand/LeftHandFinger03_01/LeftHandFinger03_02/LeftHandFinger03_03"},
                    {UmaJointTypes.LeftHandIndex, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftHand/LeftHandFinger04_01"},
                    {UmaJointTypes.LeftHandIndex_1, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftHand/LeftHandFinger04_01/LeftHandFinger04_02"},
                    {UmaJointTypes.LeftHandIndex_2, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftHand/LeftHandFinger04_01/LeftHandFinger04_02/LeftHandFinger04_03"},
                    {UmaJointTypes.LeftHandThumb, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftHand/LeftHandFinger05_01"},
                    {UmaJointTypes.LeftHandThumb_1, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftHand/LeftHandFinger05_01/LeftHandFinger05_02"},
                    {UmaJointTypes.LeftHandThumb_2, "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftHand/LeftHandFinger05_01/LeftHandFinger05_02/LeftHandFinger05_03"},

                    {UmaJointTypes.RightUpLeg, "Root/Global/Position/Hips/RightUpLeg"},
                    {UmaJointTypes.RightLeg, "Root/Global/Position/Hips/RightUpLeg/RightLeg"},
                    {UmaJointTypes.RightFoot, "Root/Global/Position/Hips/RightUpLeg/RightLeg/RightFoot"},
                    {UmaJointTypes.RightToeBase, "Root/Global/Position/Hips/RightUpLeg/RightLeg/RightFoot/RightToeBase"},

                    {UmaJointTypes.LeftUpLeg, "Root/Global/Position/Hips/LeftUpLeg"},
                    {UmaJointTypes.LeftLeg, "Root/Global/Position/Hips/LeftUpLeg/LeftLeg"},
                    {UmaJointTypes.LeftFoot, "Root/Global/Position/Hips/LeftUpLeg/LeftLeg/LeftFoot"},
                    {UmaJointTypes.LeftToeBase, "Root/Global/Position/Hips/LeftUpLeg/LeftLeg/LeftFoot/LeftToeBase"},
        
                };

            }
        }

        /// <summary>
        /// List of UMA joints that are optional in the custom avatar provided to the system. If they are present,
        /// they are used to make the avateering more good-looking, otherwise they are simply ignored
        /// </summary>
        public static readonly UmaJointTypes[] OptionalJoints = new UmaJointTypes[]
        {   
            //medium part of the spine
            UmaJointTypes.Spine, 
            //lower part of the spine
            UmaJointTypes.LowerBack, 
            ////head joint
            //UmaJointTypes.Head, 
            //left eye
            UmaJointTypes.LeftEye,
            //right eye
            UmaJointTypes.RightEye,
            //right shoulder joint. This joint can be useful to make the avatar back a little curved when the arm is moving
            UmaJointTypes.RightShoulder,
            //right forearm twist joint, to make the forearm to twist with the wrist
            UmaJointTypes.RightForeArmTwist, 
            //left shoulder joint. This joint can be useful to make the avatar back a little curved when the arm is moving
            UmaJointTypes.LeftShoulder, 
            //left forearm twist joint, to make the forearm to twist with the wrist
            UmaJointTypes.LeftForeArmTwist, 
            //right foot toe
            UmaJointTypes.RightToeBase, 
            //left foot toe
            UmaJointTypes.LeftToeBase, 
        };
       
        #endregion

        #region Behaviour methods

        protected void Awake()
        {
            GameObject umaKit;

            //if UmaKit is null, instantiate standard one; if it is a prefab, instantiate it; otherwise, get simply its reference
            if (UmaKit == null)
            {
                umaKit = Instantiate<GameObject>(Resources.Load<GameObject>("UMABaseKit"));
            }
            else if (UmaKitIsPrefab)
            {
                umaKit = Instantiate<GameObject>(UmaKit);
            }
            else
                umaKit = UmaKit;

            //Assign UmaKit to the variable
            UmaKit = umaKit;

            if (Log.IsDebugEnabled)
                Log.Debug("Initialized Uma body generator");
        }

        #endregion

        #region IUmaBodyGenerator members

        /// <summary>
        /// Generates a UMA-like avatar
        /// </summary>
        /// <param name="umaAvatar">Out parameter, receiving the just created UMA-compatible avatar</param>
        /// <param name="jointsMapping">Out parameter, receiving the joint mappings for the created uma avatar</param>
        /// <param name="jointsGlobalTRotationMapping">Out parameter, receiving the joint to-T-rotation mappings for the created uma avatar</param>
        public abstract void GenerateAvatar(out GameObject umaAvatar, out IDictionary<UmaJointTypes, string> jointsMapping, out IDictionary<UmaJointTypes, Quaternion> jointsGlobalTRotationMapping);

        /// <summary>
        /// Get a UMA Bridge object, capable of moving UMA Dna Sliders on the Avatar
        /// </summary>
        /// <param name="umaInstanceGo">Avatar instance, generated by this generator, which we want to match with user's body</param>
        /// <returns>UMA Bridge object</returns>
        public abstract IUmaPhysioMatchingBridge GetUmaMatchingBridge(GameObject umaInstanceGo);

        #endregion

        #region Internal methods

        /// <summary>
        /// Helper method to get UMA joint mapppings inside game object, provided the string names of the joint children 
        /// </summary>
        /// <param name="umaAvatar">Uma Avatar to obtain the mapping from</param>
        /// <param name="jointMappingsStrings">Joint Mappings between Joint Type and string representing the name of the child of the game object that corresponds to that joint type</param>
        /// <returns>Dictionary mapping the joints types to the transform of that joints inside the umaAvatar gameobject</returns>
        /// <exception cref="ArgumentException">Thrown if joint mappings is invalid (one of the joint does not exist in the model)</exception>
        internal static Dictionary<UmaJointTypes, Transform> GetJointMappingsTransforms(GameObject umaAvatar, IDictionary<UmaJointTypes, string> jointMappingsStrings)
        {
            //get the child inside the gameobject transform
            Dictionary<UmaJointTypes, Transform> jointMappingTransforms = new Dictionary<UmaJointTypes, Transform>();

            foreach (var jointPair in jointMappingsStrings)
            {
                if (umaAvatar.transform.Find(jointPair.Value) == null)
                {
                    //if we haven't find the joint, but this is optional, prompt a warning and go on
                    if (OptionalJoints.Contains(jointPair.Key))
                    {
                        if (Log.IsWarnEnabled)
                            Log.Warning("Uma body Generator - One of the provided optional joint mappings is invalid: {0} joint does not exist", jointPair.Key.ToString());
                    }
                    //otherwise, we can't avateer, so throw an error
                    else
                    {
                        if (Log.IsErrorEnabled)
                            Log.Error("Uma body Generator - One of the provided joint mappings is invalid: {0} joint does not exist", jointPair.Key.ToString());

                        throw new ArgumentException("The provided UMA joint mappings are invalid");
                    }
                }
                else
                {
                    jointMappingTransforms[jointPair.Key] = umaAvatar.transform.Find(jointPair.Value);

                    Debug.Log("Pair: " + jointPair.Key.ToString() + " , " + jointPair.Value);
                }
            }

            return jointMappingTransforms;
        }

        /// <summary>
        /// Helper method to get UMA joint to-T-rotation mappings of a certain avatar game object, provided its avatar in T position
        /// </summary>
        /// <param name="umaAvatar">Uma Avatar to obtain the mapping from. Must be a model in T position</param>
        /// <param name="jointMappingsStrings">Joint Mappings between Joint Type and string representing the name of the child of the game object that corresponds to that joint type</param>
        /// <returns>Dictionary mapping the joints types to the quaternion of the global rotation of that joints inside the umaAvatar gameobject</returns>
        /// <exception cref="ArgumentException">Thrown if joint mappings is invalid (one of the joint does not exist in the model)</exception>
        protected internal static Dictionary<UmaJointTypes, Quaternion> GetJointGlobalTRotationsMappings(GameObject umaAvatar, IDictionary<UmaJointTypes, string> jointMappingsStrings)
        {
            //get the transforms inside the model, given the strings
            Dictionary<UmaJointTypes, Transform> jointMappingsTransforms = GetJointMappingsTransforms(umaAvatar, jointMappingsStrings);

            //get the global rotations of all transforms
            Dictionary<UmaJointTypes, Quaternion> jointRotationsMappings = new Dictionary<UmaJointTypes, Quaternion>();

            foreach (var jointPair in jointMappingsTransforms)
            {
                if (jointMappingsTransforms == null)
                {
                    //if we haven't find the joint, but this is optional, prompt a warning and go on
                    if (OptionalJoints.Contains(jointPair.Key))
                    {
                        if (Log.IsWarnEnabled)
                            Log.Warning("Uma body Generator - One of the provided optional joint transforms mappings is invalid: {0} joint does not exist", jointPair.Key.ToString());
                    }
                    //otherwise, we can't avateer, so throw an error
                    else
                    {
                        if (Log.IsErrorEnabled)
                            Log.Error("Uma body Generator - One of the provided joint transforms mappings is invalid: {0} joint does not exist", jointPair.Key.ToString());

                        throw new ArgumentException("The provided UMA joint transforms mappings are invalid");
                    }

                }
                else
                {
                    jointRotationsMappings[jointPair.Key] = jointPair.Value.rotation;

                    Debug.Log("Rotation Pair: " + jointPair.Key.ToString() + " , " + jointPair.Value.rotation.ToString("0.000"));
                }
            }

            return jointRotationsMappings;
        }

        #endregion

    }
}

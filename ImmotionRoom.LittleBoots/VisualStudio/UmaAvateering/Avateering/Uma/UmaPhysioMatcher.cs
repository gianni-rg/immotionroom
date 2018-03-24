using UnityEngine;
using System.Collections;

namespace ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Uma
{
    using UnityEngine;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using System.Collections.Generic;
    using System;
    using ImmotionAR.ImmotionRoom.LittleBoots.Avateering;

    /// <summary>
    /// Performs matching of requested feature of avatar with the user's body, modifying an Uma avatar
    /// </summary>
    internal class UmaPhysioMatcher : IAvatarPhysioMatcher
    {
        #region Constant Fields

        /// <summary>
        /// Constant proportional value to adjust length of the arm so to match better avatar arm length with real user arm length
        /// </summary>
        private const float ArmLengthProp = 0.97f;

        /// <summary>
        /// Constant proportional value to adjust length of the forearm so to match better avatar forearm length with real user forearm length
        /// </summary>
        private const float ForearmLengthProp = 0.92f;

        #endregion

        #region Private fields

        /// <summary>
        /// Tolerance of the matching of the measures
        /// </summary>
        private float m_tolerance;

        /// <summary>
        /// Maximum iterations of the convergence algorithm
        /// </summary>
        private int m_maxIters;

        /// <summary>
        /// Bridge of communication of this element with the actual UMA engine in Unity to change DNA slots values
        /// (remember that this DLL does not include a reference to UMA)
        /// </summary>
        private IUmaPhysioMatchingBridge m_umabridge;

        /// <summary>
        /// Mappings from Uma Joint Type to the transform of the corresponding joint inside the current avatar to be matched
        /// </summary>
        private IDictionary<UmaJointTypes, Transform> m_jointsMappingTransforms;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a physical features matcher for the avatar
        /// </summary>
        /// <param name="jointsMappingTransforms">Mappings of the joint type with the transform position of the joint</param>
        /// <param name="umabridge">Bridge of communication of this element with the actual UMA engine in Unity to change DNA slots values</param>
        /// <param name="tolerance">Tolerance for the converge algorithm (maximum difference between a limb dimension in the real world and in the avatar)</param>
        /// <param name="maxIters">Maximum iterations of the convergence algorithm</param>
        internal UmaPhysioMatcher(IDictionary<UmaJointTypes, Transform> jointsMappingTransforms, IUmaPhysioMatchingBridge umabridge, float tolerance = 0.015f, int maxIters = 8)
        {
            m_jointsMappingTransforms = jointsMappingTransforms;
            m_umabridge = umabridge;
            m_tolerance = tolerance;
            m_maxIters = maxIters;

            if (Log.IsDebugEnabled)
            {
                Log.Debug("UmaPhysioMatcher - Creation");
            }
        }

        #endregion

        #region IAvatarPhysioMatcher members

        /// <summary>
        /// Match a particular feature of the avatar with the user body in the physical world.
        /// So, for example, makes the avatar to match the user's body height.
        /// This is a coroutine, because the method could require various iterations steps
        /// </summary>
        /// <param name="featureID">ID of the feature to consider (e.g. height, arm length, etc...). The value depends on the particular implementation. It is advised the use of <see cref="PhysioMatchingFeatures"/> class constants</param>
        /// <param name="value">The value that feature must assume (i.e. the value of the feature for the user's body)</param>
        /// <returns></returns>
        public IEnumerator MatchFeature(int featureID, float value)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("UmaPhysioMatcher - Matching of feature {0}", featureID);
            }

            return MatchFeaturePrivate(featureID, value);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Match a particular feature of the avatar with the user body in the physical world.
        /// So, for example, makes the avatar to match the user's body height.
        /// This is a coroutine, because the method could require various iterations steps
        /// </summary>
        /// <param name="featureID">ID of the feature to consider (e.g. height, arm length, etc...). The value depends on the particular implementation. It is advised the use of <see cref="PhysioMatchingFeatures"/> class constants</param>
        /// <param name="value">The value that feature must assume (i.e. the value of the feature for the user's body)</param>
        /// <returns></returns>
        internal IEnumerator MatchFeaturePrivate(int featureID, float value)
        {
            //if this is a UMA avatar
            if (m_umabridge.IsUmaAvatar)
            {
                //if this is a UMA avatar, we can perform some fine tuning to make its height and its limbs to match the measures of the
                //actual user's ones.
                //Unluckily, UMA does not guarantee a relationship between the value in the range [0, 1] relative to a DNA slot and the 
                //actual length of a particular limb (the conversion is peformed by the UMA DNAConverters), so we'll perform a dicotomic search, until we reach a difference between the desired and
                //the actual values that stays below a certain tolerance threshold.
                //(TODO: see if there is a more efficient method)

                //get actual measure relative to the user's body
                float avatarMeasure = GetFeatureMeasure(featureID);

                //the value to increment/decrement the UMA Dna slider at each iteration
                float dicotomicValue = 1.0f;

                //current slider value for UMA Dna slot in analysis: we initialize to 0.5 just to give it a safe value
                //(will assume meaningful values after first loop iteration)
                float sliderValue = 0.5f;

                //check iterations: above 10 iterations of the convergence loop, exit
                int iterNum = 0;

                //loop until we reached the required tolerance, or if the tolerance has not been reached but the slider is reached its maximum value
                while ((avatarMeasure - value > m_tolerance && sliderValue > 0) || (value - avatarMeasure > m_tolerance && sliderValue < 1))
                {
                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("UmaPhysioMatcher - Matching of feature {0} with value {1}: iteration with avatar measure {2}, sliderValue {3}", featureID, value, avatarMeasure, sliderValue);
                    }

                    //cut in half the increment/decrement step, and set its right sign: if we should increase our avatar dimension,
                    //it must be positive, otherwise it must be negative
                    if (avatarMeasure < value && dicotomicValue < 0 || avatarMeasure > value && dicotomicValue > 0)
                        dicotomicValue = -dicotomicValue / 2;
                    else
                        dicotomicValue /= 2;

                    //perform the update of the avatar using this increment step and wait for the avatar update by UMA
                    sliderValue = m_umabridge.MatchingFeatureStepForUma(featureID, dicotomicValue);

                    while (m_umabridge.IsShapeDirty == true)
                        yield return 0; //wait until next frame

                    //get new measure of the avatar, to see if this matches the user's measure at the next loop iteration check
                    avatarMeasure = GetFeatureMeasure(featureID);

                    //hack: for differences between uma and kinect joints, I've seen esperimentally that modifying these measures, 
                    //the matching becomes better
                    if (featureID == PhysioMatchingFeatures.Height || featureID == PhysioMatchingFeatures.LegsLength)
                        avatarMeasure /= 0.9375f;

                    if (++iterNum > m_maxIters)
                        break;
                }

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("UmaPhysioMatcher - Matching of feature {0} with value {1}: finished with avatar measure {2}, sliderValue {3}", featureID, value, avatarMeasure, sliderValue);
                }
            }
            //else, if this only a UMA-compliant avatar
            else
            {
                //we can only change its scale to try to match it with the body height

                //if the feature requested is not the height, we can't do anything with a custom model, so break here
                if (featureID != PhysioMatchingFeatures.Height)
                    yield break;
                //else, if height matching was requested
                else
                {
                    //scale this body proportionally so the avatar height matches the player height
                    Vector3 newScale = m_jointsMappingTransforms[UmaJointTypes.Hips].localScale;
                    float avatarHeight = GetFeatureMeasure(PhysioMatchingFeatures.Height);
                    newScale.Scale(Vector3.one * (value / avatarHeight));
                    m_jointsMappingTransforms[UmaJointTypes.Hips].localScale = newScale;

                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("UmaPhysioMatcher - NON-Uma Matching of feature {0} with value {1}: avatar neck-ankles height was {2}, so scaled with {3}", featureID, value, GetFeatureMeasure(PhysioMatchingFeatures.Height), (value / GetFeatureMeasure(PhysioMatchingFeatures.Height)));
                    }
                }
            }
        }

        /// <summary>
        /// Get measure of feature for current avatar
        /// </summary>
        /// <param name="featureID">ID of the desired feature</param>
        /// <returns>Desired feature dimension</returns>
        private float GetFeatureMeasure(int featureID)
        {
            //get the global avatar scale: this should be not considered in the measurements, so we compute it here once for all
            Vector3 avatarScale = m_jointsMappingTransforms[UmaJointTypes.Position].parent.lossyScale;
            Vector3 avatarScaleInv = new Vector3(1 / avatarScale.x, 1 / avatarScale.y, 1 / avatarScale.z);

            //calculate the requested feature
            switch(featureID)
            {
                //user height from neck to ankles
                case PhysioMatchingFeatures.Height:
                    {
                        float avgLegsLength = (GetUnscaledJointDistance(UmaJointTypes.LeftUpLeg, UmaJointTypes.LeftLeg, avatarScaleInv) +
                                    GetUnscaledJointDistance(UmaJointTypes.LeftLeg, UmaJointTypes.LeftFoot, avatarScaleInv) +
                                    GetUnscaledJointDistance(UmaJointTypes.RightUpLeg, UmaJointTypes.RightLeg, avatarScaleInv) +
                                    GetUnscaledJointDistance(UmaJointTypes.RightLeg, UmaJointTypes.RightFoot, avatarScaleInv)) / 2;

                        float userHeight;

                        //if this avatar has all spine joints
                        if (m_jointsMappingTransforms.ContainsKey(UmaJointTypes.Spine) && m_jointsMappingTransforms.ContainsKey(UmaJointTypes.LowerBack))
                        {
                            userHeight = GetUnscaledJointDistance(UmaJointTypes.Neck, UmaJointTypes.SpineUp, avatarScaleInv) +
                                         GetUnscaledJointDistance(UmaJointTypes.SpineUp, UmaJointTypes.Spine, avatarScaleInv) +
                                         GetUnscaledJointDistance(UmaJointTypes.Spine, UmaJointTypes.LowerBack, avatarScaleInv) +
                                         avgLegsLength;
                        }
                        //else, user spine up and then compute spine base as the baricenter of the hip joints
                        else
                            userHeight = GetUnscaledJointDistance(UmaJointTypes.Neck, UmaJointTypes.SpineUp, avatarScaleInv) +
                                         (m_jointsMappingTransforms[UmaJointTypes.SpineUp].position - (m_jointsMappingTransforms[UmaJointTypes.LeftUpLeg].position + m_jointsMappingTransforms[UmaJointTypes.RightUpLeg].position) / 2).magnitude +
                                         avgLegsLength;

                        return userHeight;
                    }

                //average length of legs from hips to ankles
                case PhysioMatchingFeatures.LegsLength:
                    {
                        float avgLegsLength = (GetUnscaledJointDistance(UmaJointTypes.LeftUpLeg, UmaJointTypes.LeftLeg, avatarScaleInv) +
                                                GetUnscaledJointDistance(UmaJointTypes.LeftLeg, UmaJointTypes.LeftFoot, avatarScaleInv) +
                                                GetUnscaledJointDistance(UmaJointTypes.RightUpLeg, UmaJointTypes.RightLeg, avatarScaleInv) +
                                                GetUnscaledJointDistance(UmaJointTypes.RightLeg, UmaJointTypes.RightFoot, avatarScaleInv)) / 2;
           
                        return avgLegsLength;
                    }

                //distance between the two shoulders 
                case PhysioMatchingFeatures.ShouldersWidth:
                    {
                        float shouldersWidth = GetUnscaledJointDistance(UmaJointTypes.LeftArm, UmaJointTypes.RightArm, avatarScaleInv);

                        return shouldersWidth;
                    }

                //average arms length from shoulder to wrist
                case PhysioMatchingFeatures.ArmsLength:
                    {                        
                        float avgForeArmsLength = (GetUnscaledJointDistance(UmaJointTypes.LeftForeArm, UmaJointTypes.LeftHand, avatarScaleInv) +
                                                   GetUnscaledJointDistance(UmaJointTypes.RightForeArm, UmaJointTypes.RightHand, avatarScaleInv)) / 2;
                        float avgArmsLength = (GetUnscaledJointDistance(UmaJointTypes.LeftArm, UmaJointTypes.LeftForeArm, avatarScaleInv) +
                                               avgForeArmsLength +
                                               GetUnscaledJointDistance(UmaJointTypes.RightArm, UmaJointTypes.RightForeArm, avatarScaleInv) +
                                               avgForeArmsLength) / 2;
          
                        return avgArmsLength * ArmLengthProp;
                    }

                //average forearms length from elbow to wrist
                case PhysioMatchingFeatures.ForeArmsLength:
                    {                        
                        float avgForeArmsLength = (GetUnscaledJointDistance(UmaJointTypes.LeftForeArm, UmaJointTypes.LeftHand, avatarScaleInv) +
                                                   GetUnscaledJointDistance(UmaJointTypes.RightForeArm, UmaJointTypes.RightHand, avatarScaleInv)) / 2;
            
                        return avgForeArmsLength * ForearmLengthProp;
                    }

                //unknown measure
                default:
                    if (Log.IsErrorEnabled)
                    {
                        Log.Error("UmaPhysioMatcher - Unknown feature ID: feature {0} does not exist", featureID);
                    }

                    throw new ArgumentException("UmaPhysioMatcher - Unknown feature ID");
            }
        }

        /// <summary>
        /// Get distance between two avatar joints, ignoring the global scaling of the avatar itself
        /// </summary>
        /// <param name="joint1">First joint to consider</param>
        /// <param name="joint2">Second joint to consider</param>
        /// <param name="lossyScaleInv">Inverse of the global scale of the avatar</param>
        /// <returns>Unscaled distance between the joints</returns>
        private float GetUnscaledJointDistance(UmaJointTypes joint1, UmaJointTypes joint2, Vector3 lossyScaleInv)
        {
            //get position of joints
            Vector3 joint1UnscaledPos = m_jointsMappingTransforms[joint1].position,
                    joint2UnscaledPos = m_jointsMappingTransforms[joint2].position;

            //don't consider their global scale
            joint1UnscaledPos.Scale(lossyScaleInv);
            joint2UnscaledPos.Scale(lossyScaleInv);

            //return their distance
            return Vector3.Distance(joint1UnscaledPos, joint2UnscaledPos);
        }

        #endregion
    }

}
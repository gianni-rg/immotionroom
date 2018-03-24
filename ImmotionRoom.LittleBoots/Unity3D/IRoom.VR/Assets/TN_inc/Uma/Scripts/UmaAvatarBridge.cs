/************************************************************************************************************
 * 
 * Copyright (C) 2014-2016 ImmotionAR, a division of Beps Engineering. All rights reserved.
 * 
 * Licensed under the ImmotionAR ImmotionRoom SDK License (the "License");
 * you may not use the ImmotionAR ImmotionRoom SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 * 
 * You may obtain a copy of the License at
 * 
 * http://www.immotionar.com/legal/ImmotionRoomSDKLicense.PDF
 * 
 ************************************************************************************************************/
namespace ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Uma.Generators
{
    using UnityEngine;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using System.Collections.Generic;
    using UMA;
    using ImmotionAR.ImmotionRoom.LittleBoots.Avateering;
    using System;

    /// <summary>
    /// Bridge of a UMA (or UMA-compliant) humanoid avatar towards the Avateering plugin
    /// </summary>
    public class UmaAvatarBridge : IUmaPhysioMatchingBridge
    {
        #region Private fields

        /// <summary>
        /// Avateering control script of current avatar, if it is a UMA avatar (null if this is only a UMA compliant avatar)
        /// </summary>
        private UMADynamicAvatar m_umaAvatar;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs an avatar bridge
        /// </summary>
        /// <param name="umaAvatar">UMA Avatar script of the Avatar, if any</param>
        public UmaAvatarBridge(UMADynamicAvatar umaAvatar)
        {
            m_umaAvatar = umaAvatar;
        }

        #endregion

        #region IUmaPhysioMatchingBridge members

        /// <summary>
        /// True if the avatar is a UMA avatar, false if it is only a UMA-compliant avatar
        /// </summary>
        public bool IsUmaAvatar
        {
            get
            {
                return m_umaAvatar != null;
            }
        }

        /// <summary>
        /// Gets the IsShapeDirty flag on UMA avatars (always false for UMA compliant avatar)
        /// </summary>
        public bool IsShapeDirty
        {
            get
            {
                return IsUmaAvatar ? m_umaAvatar.umaData.isShapeDirty : false;
            }
        }

        /// <summary>
        /// Performs a matching step for the avatar body physio matching process, incrementing sliders of the UMA Dna of the required step.
        /// This method should do nothing for UMA compliant avatars
        /// </summary>
        /// <param name="featureID">Feature for which the slider has to be moved</param>
        /// <param name="incrementStep">Increment of the slider to apply</param>
        /// <returns>New value of the slider (that is clamped in the range [0, 1]</returns>
        public float MatchingFeatureStepForUma(int featureID, float incrementStep)
        {
            //this method is useless for UMA-compliant avatars
            if (!IsUmaAvatar)
                return 0;

            UMAData umaData = m_umaAvatar.umaData;
            UMADnaHumanoid umaDna = umaData.GetDna<UMADnaHumanoid>();
 
            //will hold the updated slider value to return
            float returnValue;

            //assign the desired value to the feature, using UMA sliders
            switch (featureID)
            {
                //user height
                case PhysioMatchingFeatures.Height:
                    umaDna.height += incrementStep;
                    returnValue = umaDna.height = Mathf.Clamp(umaDna.height, 0, 1);

                    break;

                //avatar legs
                case PhysioMatchingFeatures.LegsLength:
                    umaDna.legsSize += incrementStep;
                    returnValue = umaDna.legsSize = Mathf.Clamp(umaDna.legsSize, 0, 1);

                    break;

                //avatar shoulders
                case PhysioMatchingFeatures.ShouldersWidth:
                    umaDna.upperMuscle += incrementStep;
                    returnValue = umaDna.upperMuscle = Mathf.Clamp(umaDna.upperMuscle, 0, 1);
                    
                    break;

                //avatar arms
                case PhysioMatchingFeatures.ArmsLength:
                    umaDna.armLength += incrementStep;
                    returnValue = umaDna.armLength = Mathf.Clamp(umaDna.armLength, 0, 1);

                    break;

                //avatar forearms
                case PhysioMatchingFeatures.ForeArmsLength:
                    umaDna.forearmLength += incrementStep;
                    returnValue = umaDna.forearmLength = Mathf.Clamp(umaDna.forearmLength, 0, 1);

                    break;

                //unknown measure
                default:
                    if (Log.IsErrorEnabled)
                    {
                        Log.Error("UmaAvatarBridge - Unknown feature ID: feature {0} does not exist", featureID);
                    }

                    throw new ArgumentException("UmaAvatarBridge - Unknown feature ID");
            }

            //notify UMA that the dna is changed and the avatar has to be updated
            umaData.isShapeDirty = true;
            umaData.Dirty();

            //return current slider value
            return returnValue;
        }


        #endregion
    }

}
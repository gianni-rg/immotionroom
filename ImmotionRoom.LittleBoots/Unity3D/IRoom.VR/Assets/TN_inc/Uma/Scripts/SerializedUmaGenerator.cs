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

    /// <summary>
    /// Generates a UMA 2 avatar, that is serialized as a .asset file
    /// </summary>
    public class SerializedUmaGenerator : UmaBodyGenerator
    {
        #region Constants

        /// <summary>
        /// Name of the resource containing a standard UMA Avatar in T pose
        /// </summary>
        private string TAvatarResourceName = "Uma_T_Avatar";

        #endregion

        #region Unity public properties

        /// <summary>
        /// Serialized Avatar to load as user avatar
        /// </summary>
        [Tooltip("Serialized Avatar to load as user avatar")]
        public UMATextRecipe SerializedAvatar;

        #endregion

        #region Private fields

        /// <summary>
        /// UMA generator
        /// </summary>
        UMAGenerator m_umaGenerator;

        /// <summary>
        /// UMA context (slots, overlays, etc...)
        /// </summary>
        UMAContext m_umaContext;

        #endregion

        #region Behaviour methods

        protected new void Awake()
        {
            base.Awake();

            //check if UmaKit contains UmaGenerator and UmaContext. If not, throw an exception
            var generators = UmaKit.GetComponentsInChildren<UMAGenerator>();
            var contexts = UmaKit.GetComponentsInChildren<UMAContext>();

            if(generators == null || generators.Length == 0 || contexts == null || contexts.Length == 0)
            {
                if(Log.IsErrorEnabled)
                {
                    Log.Error("SerializedUmaGenerator - Bad UmaKit provided");
                }
            }
            else
            {
                m_umaGenerator = generators[0];
                m_umaContext = contexts[0];
            }

            if (Log.IsDebugEnabled)
            {
                Log.Debug("SerializedUmaGenerator - Correctly awaken");
            }

        }

        #endregion

        #region UmaBodyGenerator members

        /// <summary>
        /// Generates a UMA-like avatar
        /// </summary>
        /// <param name="umaAvatar">Out parameter, receiving the just created UMA-compatible avatar</param>
        /// <param name="jointsMapping">Out parameter, receiving the joint mappings for the created uma avatar</param>
        /// <param name="jointsGlobalTRotationMapping">Out parameter, receiving the joint to-T-rotation mappings for the created uma avatar</param>
        public override void GenerateAvatar(out GameObject umaAvatar, out IDictionary<UmaJointTypes, string> jointsMapping, out IDictionary<UmaJointTypes, Quaternion> jointsGlobalTRotationMapping)
        {            
            //create new gameobject for this avatar and append it as child of this object
            umaAvatar = new GameObject(SerializedAvatar.name);
            umaAvatar.transform.SetParent(transform, false);

            //deserialize the avatar into the gameobject
            UMADynamicAvatar dynamicAvatar = umaAvatar.AddComponent<UMADynamicAvatar>();
            dynamicAvatar.context = m_umaContext;
            dynamicAvatar.umaGenerator = m_umaGenerator;
            dynamicAvatar.Load(SerializedAvatar);

            //generate the mappings (standard, because we are using a standard UMA avatar)
            jointsMapping = UmaBodyGenerator.StandardUmaJointMappings;

            //get rotational mappings from the standard T avatar
            //TODO: CREATE A STANDARD DICTIONARY OF MAPPINGS FOR STANDARD UMA POSES
            jointsGlobalTRotationMapping = UmaBodyGenerator.GetJointGlobalTRotationsMappings(Resources.Load<GameObject>(TAvatarResourceName), jointsMapping);

            if (Log.IsDebugEnabled)
            {
                Log.Debug("SerializedUmaGenerator - Generated {0} ", SerializedAvatar.name);
            }
          
        }

        /// <summary>
        /// Get a UMA Bridge object, capable of moving UMA Dna Sliders on the Avatar
        /// </summary>
        /// <param name="umaInstanceGo">Avatar instance, generated by this generator, which we want to match with user's body</param>
        /// <returns>UMA Bridge object</returns>
        public override IUmaPhysioMatchingBridge GetUmaMatchingBridge(GameObject umaInstanceGo)
        {
            return new UmaAvatarBridge(umaInstanceGo.GetComponent<UMADynamicAvatar>());
        }

        #endregion

        
    }
}


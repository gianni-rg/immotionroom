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
    using System;
    using ImmotionAR.ImmotionRoom.LittleBoots.Avateering;

    /// <summary>
    /// Generates an avatar compliant with UMA2 skeleton system
    /// </summary>
    public class CustomUmaModelGenerator : UmaBodyGenerator, ISerializationCallbackReceiver
    {
        #region Unity public properties

        /// <summary>
        /// UMA-compliant Avatar to load as user avatar
        /// </summary>
        [Tooltip("UMA compliant model to use as user avatar")]
        public GameObject UmaCompliantAvatar;

        /// <summary>
        /// UMA-compliant Avatar, in T pose
        /// </summary>
        [Tooltip("UMA compliant model, in T pose, with spread arms and closed legs")]
        public GameObject UmaCompliantAvatarInTPose;

        /// <summary>
        /// Avatar joint mappings, specifying child transforms, inside this model, corresponding to the UMA joint types
        /// </summary>
        [Tooltip("Avatar joint mappings, specifying child transforms, inside this model, corresponding to the UMA joint types")]
        public Dictionary<UmaJointTypes, string> AvatarJointMappings;

        /// <summary>
        /// Filename of a filecontaining all the Avatar Joint Mappings to deserialize into the AvatarJointMappings field
        /// </summary>
        [Tooltip("Name of a file, contained in the Resources folder, containing all the Joint Mappings. The file format is: for each UMA joint, one line for the joint name in square brackets [<jointname>] and one for the actual joint path string")]
        public string AvatarJointMappingsFile;

        /// <summary>
        /// True to read the mappings from the file specified in AvatarJointMappingsFile field, false to use values from inspector
        /// </summary>
        [Tooltip("True to read the mappings from the file specified in AvatarJointMappingsFile field, false to use values from inspector")]
        public bool LoadJointMappingsFromFile;

        #endregion

        #region Helper public fields

        /// <summary>
        /// Helping field, to make correct Unity serialization of Dictionary values
        /// (Unity can serialize Lists but not dictionaries).
        /// Holds keys of AvatarJointMappings attribute
        /// </summary>
        public List<UmaJointTypes> m_jointMappingsKeys;

        /// <summary>
        /// Helping field, to make correct Unity serialization of Dictionary values
        /// (Unity can serialize Lists but not dictionaries).
        /// Holds values of AvatarJointMappings attribute
        /// </summary>
        public List<string> m_jointMappingsValues;

        #endregion

        #region Behaviour methods

        protected new void Awake()
        {
            //Don't call base, because we don't need the UMAKit here, we should only instantiate the model
            //base.Awake();

            //deserialize the joints mappings from file, if required
            if (LoadJointMappingsFromFile)
                DeserializeJointMappingsFile();

            if (Log.IsDebugEnabled)
            {
                Log.Debug("CustomUmaModelGenerator - Correctly awaken");
            }

        }

        #endregion

        /// <summary>
        /// Deserializes the joint mappings dictionary from a file with the following format:
        /// For each joint type to be mapped there are two lines:
        /// one for the joint name inside square brackets (e.g. [Neck])
        /// one for the joint mapping string (e.g. /Root/Position/Spine/Neck).
        /// The file MUST stay inside a Resources folder
        /// </summary>
        /// <exception cref="InvalidOperationException">If the file is an invalid format</exception>
        void DeserializeJointMappingsFile()
        {
            //clear current data
            AvatarJointMappings = new Dictionary<UmaJointTypes, string>();

            //load the text file from the resources and get all its lines
            TextAsset textFile = Resources.Load(AvatarJointMappingsFile) as TextAsset;

            if(textFile == null)
            {
                if (Log.IsErrorEnabled)
                {
                    Log.Error("CustomUmaModelGenerator - The joints mapping file does not exist");
                }

                throw new InvalidOperationException("Joints mapping file is inexistent");

            }

            string[] linesFromfile = textFile.text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            //check we have two lines for each joint to be read
            if (linesFromfile.Length % 2 == 1)
            {
                if (Log.IsErrorEnabled)
                {
                    Log.Error("CustomUmaModelGenerator - The joints mapping file is in an invalid format. Odd lines numbers");
                }

                throw new InvalidOperationException("Joints mapping file is invalid");

            }

            //for each lines pair
            for (int i = 0; i < linesFromfile.Length; i += 2)
            {
                //get key and value for dictionary
                string fileLineKey = linesFromfile[i];
                string fileLineVal = linesFromfile[i + 1];

                //check the key has the brackets
                if (fileLineKey[0] != '[' || fileLineKey[fileLineKey.Length - 1] != ']')
                {
                    if (Log.IsErrorEnabled)
                    {
                        Log.Error("CustomUmaModelGenerator - The joints mapping file is in an invalid format. No brakets for a key");
                    }

                    throw new InvalidOperationException("Joints mapping file is invalid");
                }

                //add the key+value pair to the dictionary
                AvatarJointMappings.Add((UmaJointTypes)Enum.Parse(typeof(UmaJointTypes), fileLineKey.Substring(1, fileLineKey.Length - 2)), fileLineVal);
            }

            if (Log.IsDebugEnabled)
            {
                Log.Debug("CustomUmaModelGenerator - Correctly deserialized joint mappings from file");
            }
        }

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
            umaAvatar = Instantiate<GameObject>(UmaCompliantAvatar);
            umaAvatar.transform.SetParent(transform, false);

            //get the mappings passed by the user
            jointsMapping = AvatarJointMappings;

            //get rotational mappings from the standard T avatar
            jointsGlobalTRotationMapping = UmaBodyGenerator.GetJointGlobalTRotationsMappings(UmaCompliantAvatarInTPose, jointsMapping);

            if (Log.IsDebugEnabled)
            {
                Log.Debug("CustomUmaModelGenerator - Generated {0} ", UmaCompliantAvatar.name);
            }
        }

        /// <summary>
        /// Get a UMA Bridge object, capable of moving UMA Dna Sliders on the Avatar
        /// </summary>
        /// <param name="umaInstanceGo">Avatar instance, generated by this generator, which we want to match with user's body</param>
        /// <returns>UMA Bridge object</returns>
        public override IUmaPhysioMatchingBridge GetUmaMatchingBridge(GameObject umaInstanceGo)
        {
            return new UmaAvatarBridge(null);
        }

        #endregion

        #region ISerializationCallbackReceiver members

        //code from http://docs.unity3d.com/ScriptReference/ISerializationCallbackReceiver.OnBeforeSerialize.html

        /// <summary>
        /// Called after deserialization of this object
        /// (serves to make Unity correctly handle dictionary values, otherwise values in the inspector gets lost at program execution)
        /// </summary>
        public void OnAfterDeserialize()
        {
            if (AvatarJointMappings == null)
                AvatarJointMappings = new Dictionary<UmaJointTypes, string>();
            else
                AvatarJointMappings.Clear();

            for (int i = 0; i < Mathf.Min(m_jointMappingsKeys.Count, m_jointMappingsValues.Count); i++)
                AvatarJointMappings.Add(m_jointMappingsKeys[i], m_jointMappingsValues[i]);
        }

        /// <summary>
        /// Called before deserialization of this object
        /// (serves to make Unity correctly handle dictionary values, otherwise values in the inspector gets lost at program execution)
        /// </summary>
        public void OnBeforeSerialize()
        {
            if (m_jointMappingsKeys == null)
                m_jointMappingsKeys = new List<UmaJointTypes>();

            if (m_jointMappingsValues == null)
                m_jointMappingsValues = new List<string>();

            if (AvatarJointMappings == null || AvatarJointMappings.Count == 0)
                AvatarJointMappings = UmaBodyGenerator.StandardUmaJointMappings;

            m_jointMappingsKeys.Clear();
            m_jointMappingsValues.Clear();
            foreach (var kvp in AvatarJointMappings)
            {
                m_jointMappingsKeys.Add(kvp.Key);
                m_jointMappingsValues.Add(kvp.Value);
            }

        }

        #endregion
    }
}


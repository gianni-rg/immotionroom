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
namespace ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Uma.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEditor;
    using UnityEngine;
    using ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Uma.Generators;

    /// <summary>
    /// Custom editor interface for objects of type <see cref="CustomUmaModelGenerator"/>
    /// </summary>
    [CustomEditor(typeof(CustomUmaModelGenerator), false)]
    public class CustomUmaModelGeneratorEditor : UmaBodyGeneratorEditor
    {
        #region Serialized Properties

        /// <summary>
        /// Serialized property of UmaCompliantAvatar of target
        /// </summary>
        private SerializedProperty m_umaCompliantAvatar;

        /// <summary>
        /// Serialized property of UmaCompliantAvatarInTPose of target
        /// </summary>
        private SerializedProperty m_umaCompliantAvatarInTPose;

        /// <summary>
        /// Serialized property of AvatarJointMappingsFile of target
        /// </summary>
        private SerializedProperty m_avatarJointMappingsFile;

        /// <summary>
        /// Serialized property of LoadJointMappingsFromFile of target
        /// </summary>
        private SerializedProperty m_loadJointMappingsFromFile;

        /// <summary>
        /// Serialized property of m_jointMappingsKeys of target
        /// </summary>
        public SerializedProperty m_jointMappingsKeys;

        /// <summary>
        /// Serialized property of m_jointMappingsValues of target
        /// </summary>
        public SerializedProperty m_jointMappingsValues;

        #endregion

        public override void OnEnable()
        {
            base.OnEnable();

            m_umaCompliantAvatar = serializedObject.FindProperty("UmaCompliantAvatar");
            m_umaCompliantAvatarInTPose = serializedObject.FindProperty("UmaCompliantAvatarInTPose");
            m_avatarJointMappingsFile = serializedObject.FindProperty("AvatarJointMappingsFile");
            m_loadJointMappingsFromFile = serializedObject.FindProperty("LoadJointMappingsFromFile");
            m_jointMappingsKeys = serializedObject.FindProperty("m_jointMappingsKeys");
            m_jointMappingsValues = serializedObject.FindProperty("m_jointMappingsValues");
        }

        /// <summary>
        /// Executed at script displaying
        /// </summary>
        public override void OnInspectorGUI()
        {
            //don't ask for Umakit, because it is not of interest for this generator
            //base.OnInspectorGUI();
            serializedObject.Update();

            //casted uma generator pointer
            CustomUmaModelGenerator customUmaGenerator = m_umaGenerator as CustomUmaModelGenerator;

            //holds label and tooltip of each control
            GUIContent labelTooltip;

            //Custom Avatar Model (remember to force the user to insert only prefabs instances)
            GUILayout.BeginVertical();
            labelTooltip = new GUIContent("Uma Compliant Avatar", "UMA compliant model to use as user avatar");
            GameObject oldReference = (GameObject)m_umaCompliantAvatar.objectReferenceValue; //save current avatar reference
            EditorGUILayout.PropertyField(m_umaCompliantAvatar, labelTooltip);

            if ((GameObject)m_umaCompliantAvatar.objectReferenceValue != null)
            {
                PrefabType avatarPrefabType = PrefabUtility.GetPrefabType((GameObject)m_umaCompliantAvatar.objectReferenceValue); //check it is a prefab

                if (avatarPrefabType != PrefabType.ModelPrefab && avatarPrefabType != PrefabType.Prefab) //if not, restore previous reference
                {
                    m_umaCompliantAvatar.objectReferenceValue = oldReference;
                }
            }

            GUILayout.EndVertical();

            if (m_umaCompliantAvatar.objectReferenceValue != null)
            {
                //UMA avatar in T pose
                GUILayout.BeginVertical();
                labelTooltip = new GUIContent("Uma Compliant Avatar in T Pose", "UMA compliant model, in T pose, with spread arms and closed legs");
                EditorGUILayout.PropertyField(m_umaCompliantAvatarInTPose, labelTooltip);
                GUILayout.EndVertical();

                //if use joints mappings read from file or assigned in inspector
                GUILayout.BeginVertical();
                labelTooltip = new GUIContent("Joint Mappings From File", "If the joint mappings have to be read from a file of specified in the inspector");
                EditorGUILayout.PropertyField(m_loadJointMappingsFromFile, labelTooltip);
                GUILayout.EndVertical();

                //if mappings have to be read from file
                if (m_loadJointMappingsFromFile.boolValue)
                {
                    GUILayout.BeginVertical();
                    labelTooltip = new GUIContent("Joint Mappings Filename", "Name of a file, contained in the Resources folder, containing all the Joint Mappings. The file format is: for each UMA joint, one line for the joint name in square brackets [<jointname>] and one for the actual joint path string");
                    EditorGUILayout.PropertyField(m_avatarJointMappingsFile, labelTooltip);
                    GUILayout.EndVertical();
                }
                //else if we have to specify them in the inspector
                else
                {
                    //Mapping of all joints
                    Dictionary<UmaJointTypes, string> avatarJointMappings = new Dictionary<UmaJointTypes, string>();

                    foreach (var jointMappingsPair in customUmaGenerator.AvatarJointMappings)
                    {
                        GUILayout.BeginVertical();
                        labelTooltip = new GUIContent(jointMappingsPair.Key.ToString(), "Avatar joint mapping, specifying child transform, inside this model, corresponding to this particular UMA joint type");
                        avatarJointMappings.Add(jointMappingsPair.Key, EditorGUILayout.TextField(labelTooltip, jointMappingsPair.Value));
                        GUILayout.EndVertical();
                    }

                    customUmaGenerator.AvatarJointMappings = avatarJointMappings;

                    //copy the read values to the serialized properties of the actual object (this is all stuff to be made because Unity can't handle Dictionaries)
                    m_jointMappingsKeys.arraySize = customUmaGenerator.AvatarJointMappings.Count;
                    m_jointMappingsValues.arraySize = customUmaGenerator.AvatarJointMappings.Count;
                    int idx = 0;

                    foreach (var jointMappingsPair in customUmaGenerator.AvatarJointMappings)
                    {
                        m_jointMappingsKeys.GetArrayElementAtIndex(idx).enumValueIndex = (int)jointMappingsPair.Key;
                        m_jointMappingsValues.GetArrayElementAtIndex(idx).stringValue = jointMappingsPair.Value;
                        idx++;
                    }
                }

            }

            serializedObject.ApplyModifiedProperties();
        }

    }
}

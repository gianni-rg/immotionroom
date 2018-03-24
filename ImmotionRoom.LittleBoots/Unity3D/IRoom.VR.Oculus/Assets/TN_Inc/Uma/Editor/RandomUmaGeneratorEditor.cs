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
    /// Custom editor interface for objects of type <see cref="RandomUmaGenerator"/>
    /// </summary>
    [CustomEditor(typeof(RandomUmaGenerator), false)]
    [CanEditMultipleObjects]
    public class RandomUmaGeneratorEditor : UmaBodyGeneratorEditor
    {
        #region Private fields

        /// <summary>
        /// Serialized version of the target of this editor, used to make input box for arrays in the inspector
        /// </summary>
        private SerializedObject m_serializedTarget;

        /// <summary>
        /// Serialized version of the random features property of the target, used to make input box for arrays in the inspector
        /// </summary>
        private SerializedProperty m_propertyRandomFeat;

        /// <summary>
        /// Serialized version of the custom recipes property of the target, used to make input box for arrays in the inspector
        /// </summary>
        private SerializedProperty m_propertyCustomRecipes;

        #endregion

        public override void OnEnable()
        {
            base.OnEnable();
            m_serializedTarget = new SerializedObject(target);
        }

        /// <summary>
        /// Executed at script displaying
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            //holds label and tooltip of each control
            GUIContent labelTooltip;

            ////casted uma generator pointer
            //RandomUmaGenerator randomUmaGenerator = m_umaGenerator as RandomUmaGenerator;

            //Pool of random features
            GUILayout.BeginVertical();
            labelTooltip = new GUIContent("Random Features Pool", "Sets of randomizable UMA features to construct the random avatars");
            m_propertyRandomFeat = m_serializedTarget.FindProperty("RandomFeaturesPool");
            EditorGUILayout.PropertyField(m_propertyRandomFeat, labelTooltip, true);
            GUILayout.EndVertical();

            //Pool of random features
            GUILayout.BeginVertical();
            labelTooltip = new GUIContent("Additional Recipes", "Additional UMA Recipes to construct the random avatars");
            m_propertyCustomRecipes = m_serializedTarget.FindProperty("AdditionalRecipes");
            EditorGUILayout.PropertyField(m_propertyCustomRecipes, labelTooltip, true);            
            GUILayout.EndVertical();

            m_serializedTarget.ApplyModifiedProperties();

        }
    }
}


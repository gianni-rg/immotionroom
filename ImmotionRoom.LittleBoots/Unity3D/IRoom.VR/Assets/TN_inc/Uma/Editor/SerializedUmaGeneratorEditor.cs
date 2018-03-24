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
    /// Custom editor interface for objects of type <see cref="SerializedUmaGenerator"/>
    /// </summary>
    [CustomEditor(typeof(SerializedUmaGenerator), false)]
    [CanEditMultipleObjects]
    public class SerializedUmaGeneratorEditor : UmaBodyGeneratorEditor
    {
        #region Serialized Properties

        /// <summary>
        /// Serialized property of SerializedAvatar of target
        /// </summary>
        private SerializedProperty m_serializedAvatar;

        #endregion

        public override void OnEnable()
        {
            base.OnEnable();

            m_serializedAvatar = serializedObject.FindProperty("SerializedAvatar");
        }

        /// <summary>
        /// Executed at script displaying
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            //holds label and tooltip of each control
            GUIContent labelTooltip;

            //Serialized Avatar
            GUILayout.BeginVertical();
            labelTooltip = new GUIContent("Serialized Avatar", "Serialized Avatar to load as user avatar");
            EditorGUILayout.PropertyField(m_serializedAvatar, labelTooltip);            
            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();

        }

    }
}


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
    /// Custom editor interface for objects of type <see cref="UmaBodyGenerator"/>
    /// </summary>
    [CustomEditor(typeof(UmaBodyGenerator), false)]
    [CanEditMultipleObjects]
    public class UmaBodyGeneratorEditor : Editor
    {
        #region Serialized Properties

        /// <summary>
        /// Reference to object edited
        /// </summary>
        protected UmaBodyGenerator m_umaGenerator;

        /// <summary>
        /// Serialized property of UmaKit of target
        /// </summary>
        protected SerializedProperty m_umaKit;

        /// <summary>
        /// Serialized property of UmaKitIsPrefab of target
        /// </summary>
        protected SerializedProperty m_umaKitIsPrefab;

        #endregion

        /// <summary>
        /// Executed at script start
        /// </summary>
        public virtual void OnEnable()
        {
            m_umaGenerator = (UmaBodyGenerator)target;

            m_umaKit = serializedObject.FindProperty("UmaKit");
            m_umaKitIsPrefab = serializedObject.FindProperty("UmaKitIsPrefab");
        }

        /// <summary>
        /// Executed at script displaying
        /// </summary>
        public override void OnInspectorGUI()
        {
            //see this for the explanation on why using SerializedProperties http://docs.unity3d.com/ScriptReference/Editor.html

            serializedObject.Update();

            //holds label and tooltip of each control
            GUIContent labelTooltip;

            //UMAKit (distinguish if to be instantiated or already in scene)
            GUILayout.BeginVertical();
            labelTooltip = new GUIContent("UMA Kit", "Reference to the Uma Kit (object holding UmaContext and UmaGenerator) in the scene or in the prefabs. Leave null to load the default kit.");
            EditorGUILayout.PropertyField(m_umaKit, labelTooltip);            
            if (m_umaGenerator.UmaKit != null)
            {
                PrefabType umaKitPrefabType = PrefabUtility.GetPrefabType(m_umaGenerator.UmaKit);
                m_umaKitIsPrefab.boolValue = (umaKitPrefabType == PrefabType.ModelPrefab) || (umaKitPrefabType == PrefabType.Prefab);                
            }
            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
}


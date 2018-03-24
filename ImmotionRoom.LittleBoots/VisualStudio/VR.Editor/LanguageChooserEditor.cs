namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.Editor
{ 
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEditor;
    using ImmotionAR.ImmotionRoom.LittleBoots.VR.Localization.UI;
    using UnityEngine;
    using ImmotionAR.ImmotionRoom.LittleBoots.VR.Localization;

    /// <summary>
    /// Custom editor interface for objects of type <see cref="LanguageChooser"/>
    /// </summary>
    [CustomEditor(typeof(LanguageChooser), false)]
    class LanguageChooserEditor : UnityEditor.Editor
    {
        /// <summary>
        /// Serialized property of m_currentLanguage of target 
        /// </summary>
        SerializedProperty m_currentLanguage;
 
        protected void OnEnable()
        {
            m_currentLanguage = serializedObject.FindProperty("m_currentLanguage");
            m_currentLanguage.intValue = (int)LanguageManager.Instance.CurrentLanguage;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUIContent labelTooltip;

            //key string of the text to show            
            labelTooltip = new GUIContent("Key string", "Key string to identify the localized string inside the localization dictionary");
            EditorGUILayout.PropertyField(m_currentLanguage, labelTooltip);
            LanguageManager.Instance.CurrentLanguage = (ManagedLanguage)m_currentLanguage.intValue;
            serializedObject.ApplyModifiedProperties();
        }


    }
}

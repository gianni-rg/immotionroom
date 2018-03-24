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
    /// Custom editor interface for objects of type <see cref="LanguageManagerLocalizedText"/>
    /// </summary>
    [CustomEditor(typeof(LanguageManagerLocalizedText), false)]
    [CanEditMultipleObjects]
    class LanguageManagerLocalizedTextEditor : UnityEditor.UI.TextEditor
    {
        /// <summary>
        /// Language to show the preview with
        /// </summary>
        ManagedLanguage m_previewLanguage;

        /// <summary>
        /// Serialized property of m_keyString of target (the key to retrieve the correct translation to show inside the label)
        /// </summary>
        SerializedProperty m_keyString;

        /// <summary>
        /// Serialized property of text to show (the actual text to show in the label)
        /// </summary>
        SerializedProperty m_Text;

        /// <summary>
        /// Serialized property of font data of target (all font parameters for the text label)
        /// </summary>
        SerializedProperty m_FontData;
 
        protected override void OnEnable()
        {
            base.OnEnable();

            m_previewLanguage = ManagedLanguage.enUS;

            m_keyString = serializedObject.FindProperty("m_keyString");
            m_Text = serializedObject.FindProperty("m_Text");
            m_FontData = serializedObject.FindProperty("m_FontData");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUIContent labelTooltip;

            //key string of the text to show
            labelTooltip = new GUIContent("Key string", "Key string to identify the localized string inside the localization dictionary");
            EditorGUILayout.PropertyField(m_keyString, labelTooltip);

            //ask which language to show the preview into (only in editor mode)
            if (!Application.isPlaying)
            {
                labelTooltip = new GUIContent("Preview Language", "Choose the language into which this preview has to be done. Notice that this will not define the global language of the running program neither the language in editor mode. It is for fast checks only");
                m_previewLanguage = (ManagedLanguage)EditorGUILayout.EnumPopup(labelTooltip, m_previewLanguage);
                LanguageManager.Instance.CurrentLanguage = m_previewLanguage;
            }

            //localize the text to show
            m_Text.stringValue = LanguageManager.Instance.GetLocalizedString(m_keyString.stringValue ?? "");

            //font and material appearence
            EditorGUILayout.PropertyField(m_FontData);
            AppearanceControlsGUI();

            serializedObject.ApplyModifiedProperties();
        }


    }
}

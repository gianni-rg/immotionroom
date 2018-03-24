namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEditor;
    using UnityEngine;
    using ImmotionAR.ImmotionRoom.LittleBoots.VR.Localization;
    using ImmotionAR.ImmotionRoom.LittleBoots.VR.Localization.Graphics;

    /// <summary>
    /// Custom editor interface for objects of type <see cref="LanguageManagerLocalizedTextMesh"/>
    /// </summary>
    [CustomEditor(typeof(LanguageManagerLocalizedTextMesh), false)]
    [CanEditMultipleObjects]
    class LanguageManagerLocalizedTextMeshEditor : UnityEditor.Editor
    {
        /// <summary>
        /// Language to show the preview with
        /// </summary>
        ManagedLanguage m_previewLanguage;

        /// <summary>
        /// Serialized property of m_keyString of target (the key to retrieve the correct translation to show inside the label)
        /// </summary>
        SerializedProperty m_keyString;

        protected void OnEnable()
        {
            m_previewLanguage = ManagedLanguage.enUS;

            m_keyString = serializedObject.FindProperty("m_keyString");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUIContent labelTooltip;

            //key string of the text to show
            labelTooltip = new GUIContent("Key string", "Key string to identify the localized string inside the localization dictionary");
            EditorGUILayout.PropertyField(m_keyString, labelTooltip);

            //ask which language to show the preview into and change text accordingly (only in editor mode)
            if (!Application.isPlaying)
            {
                labelTooltip = new GUIContent("Preview Language", "Choose the language into which this preview has to be done. Notice that this will not define the global language of the running program neither the language in editor mode. It is for fast checks only");
                m_previewLanguage = (ManagedLanguage)EditorGUILayout.EnumPopup(labelTooltip, m_previewLanguage);
                LanguageManager.Instance.CurrentLanguage = m_previewLanguage;
                (target as LanguageManagerLocalizedTextMesh).KeyString = m_keyString.stringValue ?? ""; //to force update of text using new value
            }

            serializedObject.ApplyModifiedProperties();
        }


    }
}

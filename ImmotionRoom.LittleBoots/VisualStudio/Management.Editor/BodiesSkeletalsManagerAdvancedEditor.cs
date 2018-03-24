namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.Editor
{
    using ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Editor;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedAvateering;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Custom editor interface for objects of type <see cref="BodiesSkeletalsManagerAdvanced"/>
    /// </summary>
    [CustomEditor(typeof(BodiesSkeletalsManagerAdvanced), false)]
    [CanEditMultipleObjects]
    public class BodiesSkeletalsManagerAdvancedEditor : BodiesSkeletalsManagerEditor
    {
        #region Serialized Properties

        /// <summary>
        /// Serialized property of SceneStreamerInfoId of target
        /// </summary>
        private SerializedProperty m_sceneStreamerInfoId;

        /// <summary>
        /// Serialized property of SceneStreamingMode of target
        /// </summary>
        private SerializedProperty m_sceneStreamingMode;

        /// <summary>
        /// Serialized property of RedAlerts of target
        /// </summary>
        private SerializedProperty m_redAlerts;
            
        #endregion

        /// <summary>
        /// Executed at script start
        /// </summary>
        public override void OnEnable()
        {
            base.OnEnable();

            m_sceneStreamerInfoId = serializedObject.FindProperty("SceneStreamerInfoId");
            m_sceneStreamingMode = serializedObject.FindProperty("SceneStreamingMode");
            m_redAlerts = serializedObject.FindProperty("RedAlerts");
        }

        /// <summary>
        /// Executed at script displaying
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //holds label and tooltip of each control
            GUIContent labelTooltip;

            //Streamer info ID
            GUILayout.BeginVertical();
            labelTooltip = new GUIContent("Scene StreamerInfo Id", "ID of the data stream whose skeletons are to be shown");
            EditorGUILayout.PropertyField(m_sceneStreamerInfoId, labelTooltip);            
            GUILayout.EndVertical();

            //Skeletas drawing mode
            GUILayout.BeginVertical();
            labelTooltip = new GUIContent("Scene Streaming Mode", "Which kind of scene data stream modes are to be asked from the tracking service (e.g. world transform skeletons vs master transform skeletons)");
            EditorGUILayout.PropertyField(m_sceneStreamingMode, labelTooltip);   
            GUILayout.EndVertical();

            //Red Alerts
            GUILayout.BeginVertical();
            labelTooltip = new GUIContent("Red Alerts", "GameObjects to activate or deactivate when one of the skeletons reaches the kinect tracking area limits. Gameobject order is left, top, right, bottom.");
            EditorGUILayout.PropertyField(m_redAlerts, labelTooltip);
            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();

            //show properties of the base class
            base.OnInspectorGUI();   
        }
    }
}


namespace ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEditor;
    using UnityEngine;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.SupportStruct;

    /// <summary>
    /// Custom editor interface for objects of type <see cref="TrackingServiceManagerBasic"/>
    /// </summary>
    [CustomEditor(typeof(TrackingServiceManagerBasic), false)]
    public class TrackingServiceManagerBasicEditor : Editor
    {
        #region Serialized properties

        /// <summary>
        /// Reference to object edited
        /// </summary>
        protected TrackingServiceManagerBasic m_trackingServiceManager;

        /// <summary>
        /// Serialized property of DiscoveryMode of target
        /// </summary>
        private SerializedProperty m_discoveryMode;

        /// <summary>
        /// Serialized property of AutoStartTracking of target
        /// </summary>
        private SerializedProperty m_autoStartTracking;

        /// <summary>
        /// Serialized property of UserProvidedId of target
        /// </summary>
        private SerializedProperty m_userProvidedId;

        /// <summary>
        /// Serialized property of UserProvidedControlApiEndpoint of target
        /// </summary>
        private SerializedProperty m_userProvidedControlApiEndpoint;

        /// <summary>
        /// Serialized property of UserProvidedControlApiPort of target
        /// </summary>
        private SerializedProperty m_userProvidedControlApiPort;

        #endregion

        /// <summary>
        /// Executed at script start
        /// </summary>
        public void OnEnable()
        {
            m_trackingServiceManager = (TrackingServiceManagerBasic)target;
            m_discoveryMode = serializedObject.FindProperty("DiscoveryMode");
            m_autoStartTracking = serializedObject.FindProperty("AutoStartTracking");
            m_userProvidedId = serializedObject.FindProperty("UserProvidedId");
            m_userProvidedControlApiEndpoint = serializedObject.FindProperty("UserProvidedControlApiEndpoint");
            m_userProvidedControlApiPort = serializedObject.FindProperty("UserProvidedControlApiPort");
        }
        
        /// <summary>
        /// Executed at script displaying
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //holds label and tooltip of each control
            GUIContent labelTooltip;

            //Autostart mode
            GUILayout.BeginVertical();
            labelTooltip = new GUIContent("Auto Start Tracking", "True if the system should start tracking as soon as the communication with the tracking service gets established; false otherwise");            
            EditorGUILayout.PropertyField(m_autoStartTracking, labelTooltip);
            //Debug.Log("Primary autostart" + m_trackingServiceManager.AutoStartTracking);
            GUILayout.EndVertical();
            GUILayout.Space(5);

            //Discovery mode
            GUILayout.BeginVertical();
            labelTooltip = new GUIContent("Discovery Mode", "The way this manager discovers the underlying tracking service");           
            EditorGUILayout.PropertyField(m_discoveryMode, labelTooltip);
            //Debug.Log("Primary mode" + m_trackingServiceManager.DiscoveryMode);
            GUILayout.EndVertical();
            GUILayout.Space(5);

            //if the user chose a discovey mode requiring additional user input
            if (m_trackingServiceManager.DiscoveryMode == TrackingServiceManagersDiscoveryMode.UserValuesOnly || m_trackingServiceManager.DiscoveryMode == TrackingServiceManagersDiscoveryMode.UserValuesThenDiscovery)
            {
                //ID of the Tracking Service (string)
                GUILayout.BeginVertical();
                labelTooltip = new GUIContent("TService ID", "Id of the underlying tracking service to connect to");                
                EditorGUILayout.PropertyField(m_userProvidedId, labelTooltip);
                //Debug.Log("Primary ID" + m_trackingServiceManager.Id);
                GUILayout.EndVertical();

                //IP Address of the Tracking Service (string)
                GUILayout.BeginVertical();
                labelTooltip = new GUIContent("TService IP Address", "IP Address of the underlying tracking service to connect to");                
                EditorGUILayout.PropertyField(m_userProvidedControlApiEndpoint, labelTooltip);
                //Debug.Log("Primary IP Address" + m_trackingServiceManager.Id);
                GUILayout.EndVertical();

                //IP Port of the Tracking Service (int)
                GUILayout.BeginVertical();
                labelTooltip = new GUIContent("TService IP Port", "IP Port of the underlying tracking service to connect to");                
                EditorGUILayout.PropertyField(m_userProvidedControlApiPort, labelTooltip);
                //Debug.Log("Primary port" + m_trackingServiceManager.ControlApiPort);
                GUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();

        }
    }
}

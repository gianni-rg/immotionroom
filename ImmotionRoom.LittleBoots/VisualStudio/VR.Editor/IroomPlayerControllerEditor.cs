namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.Editor
{
    using ImmotionAR.ImmotionRoom.LittleBoots.VR.PlayerController;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Custom editor interface for objects of type <see cref="IroomPlayerController"/>
    /// </summary>
    [CustomEditor(typeof(IroomPlayerController), false)]
    class IroomPlayerControllerEditor : UnityEditor.Editor
    {
        /// <summary>
        /// Serialized property of TranslationDetection of target 
        /// </summary>
        private SerializedProperty m_translationDetection;

        /// <summary>
        /// Serialized property of WalkingDetection of target 
        /// </summary>
        private SerializedProperty m_walkingDetection;

        /// <summary>
        /// Serialized property of FpsBehaviour of target 
        /// </summary>
        private SerializedProperty m_fpsBehaviour;

        /// <summary>
        /// Serialized property of RoomScaleIsPure of target 
        /// </summary>
        private SerializedProperty m_roomScaleIsPure;

        /// <summary>
        /// Serialized property of InitWithFootOnFloor of target 
        /// </summary>
        private SerializedProperty m_initWithFootOnFloor;

        /// <summary>
        /// Serialized property of InitWithFootOnFloorAdditionalFactor of target 
        /// </summary>
        private SerializedProperty m_initWithFootOnFloorAdditionalFactor;

        /// <summary>
        /// Serialized property of DetectedWalkingSpeedMultiplier of target 
        /// </summary>
        private SerializedProperty m_detectedWalkingSpeedMultiplier;

        /// <summary>
        /// Serialized property of GravityMultiplier of target 
        /// </summary>
        private SerializedProperty m_gravityMultiplier;

        /// <summary>
        /// Serialized property of CamerasNearPlane of target 
        /// </summary>
        private SerializedProperty m_camerasNearPlane;

        /// <summary>
        /// Serialized property of EyesCameraDisplacement of target 
        /// </summary>
        private SerializedProperty m_eyesCameraDisplacement;

        /// <summary>
        /// Serialized property of AllowDebugControls of target 
        /// </summary>
        private SerializedProperty m_allowDebugControls;

        /// <summary>
        /// Serialized property of DebugControlsSpeed of target 
        /// </summary>
        private SerializedProperty m_debugControlsSpeed;

        protected void OnEnable()
        {
            m_translationDetection = serializedObject.FindProperty("TranslationDetection");
            m_walkingDetection = serializedObject.FindProperty("WalkingDetection");
            m_fpsBehaviour = serializedObject.FindProperty("FpsBehaviour");
            m_roomScaleIsPure = serializedObject.FindProperty("RoomScaleIsPure");
            m_initWithFootOnFloor = serializedObject.FindProperty("InitWithFootOnFloor");
            m_initWithFootOnFloorAdditionalFactor = serializedObject.FindProperty("InitWithFootOnFloorAdditionalFactor");
            m_detectedWalkingSpeedMultiplier = serializedObject.FindProperty("DetectedWalkingSpeedMultiplier");
            m_gravityMultiplier = serializedObject.FindProperty("GravityMultiplier");
            m_camerasNearPlane = serializedObject.FindProperty("CamerasNearPlane");
            m_eyesCameraDisplacement = serializedObject.FindProperty("EyesCameraDisplacement");
            m_allowDebugControls = serializedObject.FindProperty("AllowDebugControls");
            m_debugControlsSpeed = serializedObject.FindProperty("DebugControlsSpeed");           
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUIContent labelTooltip;

            //translation detection            
            labelTooltip = new GUIContent("Translation detection", "True if the system has to detect the movement of the user inside the room, false otherwise");
            EditorGUILayout.PropertyField(m_translationDetection, labelTooltip);

            //walking detection
            labelTooltip = new GUIContent("Walking detection", "True if the system has to use the walk-in-place locomotion system, false otherwise");
            EditorGUILayout.PropertyField(m_walkingDetection, labelTooltip);

            //detected walking speed multiplier (has sense only if walking detection is enabled)
            if(m_walkingDetection.boolValue)
            {
                labelTooltip = new GUIContent("Detected walking speed multiplier", "How much the detected speed on the player affects the actual speed of the walking character");
                EditorGUILayout.PropertyField(m_detectedWalkingSpeedMultiplier, labelTooltip);
            }

            //fps behaviour
            labelTooltip = new GUIContent("FPS Behaviour", "False if the system has to guarantee a pure mapping between the real and the virtual world, making the player to ignore the physics and the collisions during his movements. True to have FPS videogame behaviour");
            EditorGUILayout.PropertyField(m_fpsBehaviour, labelTooltip);

            //pure room-scale? (has sense only if there is not fps behaviour)
            if (m_fpsBehaviour.boolValue == false)
            {
                labelTooltip = new GUIContent("Pure room scale", "If this flags is true, room scale is so pure that no kind of collision is detected. If it is false, the controller is guaranteed to stay attached to the floor");
                EditorGUILayout.PropertyField(m_roomScaleIsPure, labelTooltip);
            }
            //gravity influence (has sense only if fps behaviour)
            else
            {
                labelTooltip = new GUIContent("Gravity multiplier", "How much the gravity of the physics system has to influence the player");
                EditorGUILayout.PropertyField(m_gravityMultiplier, labelTooltip);
            }

            //debug controls
            labelTooltip = new GUIContent("Allow debug controls", "If true, input from keyboard, mouse and gamepad will be used to move the player. This is useful especially as debug controls. At the moment the only supported control is keyboard");
            EditorGUILayout.PropertyField(m_allowDebugControls, labelTooltip);

            //debug controls speed (if required)
            if (m_allowDebugControls.boolValue)
            {
                labelTooltip = new GUIContent("Debug controls speed", "Speed of player when moved using debug controls, in unit/s");
                EditorGUILayout.PropertyField(m_debugControlsSpeed, labelTooltip);
            }

            //foot on floor initialization
            labelTooltip = new GUIContent("Init with feet on floor", "Set to true if you want the system to init avatar position with foot on the floor of the virtual world; false otherwise");
            EditorGUILayout.PropertyField(m_initWithFootOnFloor, labelTooltip);

            //foot on floor tuning param (has sense only if foot on floor has been requested)
            if (m_initWithFootOnFloor.boolValue)
            {
                labelTooltip = new GUIContent("Foot On Floor additional value", "Additional (unscaled) distance to add after foot on floor initialization, for fine tuning");
                EditorGUILayout.PropertyField(m_initWithFootOnFloorAdditionalFactor, labelTooltip);
            }

            //cameras near plane
            labelTooltip = new GUIContent("Cameras near plane", "Value to set the eyes cameras near plane to, in unscaled coordinates");
            EditorGUILayout.PropertyField(m_camerasNearPlane, labelTooltip);

            //eyes camera displacement
            labelTooltip = new GUIContent("Eyes-camera displacement", "Displacement, in eyes cameras coordinate system, from avatar eyes position to actual camera placement. This is useful to tune visualization of undesired elements of the avatar face");
            EditorGUILayout.PropertyField(m_eyesCameraDisplacement, labelTooltip);            

            serializedObject.ApplyModifiedProperties();
        }


    }
}

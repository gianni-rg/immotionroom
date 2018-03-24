namespace ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEditor;
    using UnityEngine;
    using ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Skeletals;

    /// <summary>
    /// Custom editor interface for objects of type <see cref="BodiesSkeletalsManager"/>
    /// </summary>
    [CustomEditor(typeof(BodiesSkeletalsManager), false)]
    [CanEditMultipleObjects]
    public class BodiesSkeletalsManagerEditor : Editor
    {
        #region Serialized Properties

        /// <summary>
        /// Reference to object edited
        /// </summary>
        protected BodiesSkeletalsManager m_bodiesSkeletalsManager;

        /// <summary>
        /// Serialized property of TrackPosition of target
        /// </summary>
        private SerializedProperty m_trackPosition;

        /// <summary>
        /// Serialized property of SkeletalDrawingMode of target
        /// </summary>
        private SerializedProperty m_skeletalDrawingMode;

        /// <summary>
        /// Serialized property of JointsMaterial of target
        /// </summary>
        private SerializedProperty m_jointsMaterial;

        /// <summary>
        /// Serialized property of LimbsMaterial of target
        /// </summary>
        private SerializedProperty m_limbsMaterial;

        /// <summary>
        /// Serialized property of PositiveColor of target
        /// </summary>
        private SerializedProperty m_positiveColor;

        /// <summary>
        /// Serialized property of NegativeColor of target
        /// </summary>
        private SerializedProperty m_negativeColor;

        /// <summary>
        /// Serialized property of LimbsColor of target
        /// </summary>
        private SerializedProperty m_limbsColor;

        /// <summary>
        /// Serialized property of JointSphereRadius of target
        /// </summary>
        private SerializedProperty m_jointSphereRadius;

        /// <summary>
        /// Serialized property of ConnectingLinesThickness of target
        /// </summary>
        private SerializedProperty m_connectingLinesThickness;

        /// <summary>
        /// Serialized property of AddColliders of target
        /// </summary>
        private SerializedProperty m_addColliders;

        /// <summary>
        /// Serialized property of ShadowsEnabled of target
        /// </summary>
        private SerializedProperty m_shadowsEnabled;

        #endregion

        /// <summary>
        /// Executed at script start
        /// </summary>
        public virtual void OnEnable()
        {
            m_bodiesSkeletalsManager = (BodiesSkeletalsManager)target;
            m_trackPosition = serializedObject.FindProperty("TrackPosition");
            m_skeletalDrawingMode = serializedObject.FindProperty("SkeletalDrawingMode");
            m_jointsMaterial = serializedObject.FindProperty("JointsMaterial");
            m_limbsMaterial = serializedObject.FindProperty("LimbsMaterial");
            m_positiveColor = serializedObject.FindProperty("PositiveColor");
            m_negativeColor = serializedObject.FindProperty("NegativeColor");
            m_limbsColor = serializedObject.FindProperty("LimbsColor");
            m_jointSphereRadius = serializedObject.FindProperty("JointSphereRadius");
            m_connectingLinesThickness = serializedObject.FindProperty("ConnectingLinesThickness");
            m_addColliders = serializedObject.FindProperty("AddColliders");
            m_shadowsEnabled = serializedObject.FindProperty("ShadowsEnabled");
        }

        /// <summary>
        /// Executed at script displaying
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //holds label and tooltip of each control
            GUIContent labelTooltip;

            //Track position yes/no
            GUILayout.BeginVertical();
            labelTooltip = new GUIContent("Track Position", "True to track avatar position, false otherwise");
            EditorGUILayout.PropertyField(m_trackPosition, labelTooltip);            
            GUILayout.EndVertical();

            //Skeletas drawing mode
            GUILayout.BeginVertical();
            labelTooltip = new GUIContent("Skeletals Drawing mode", "Colouring mode of the various skeletons managed by this object");            
            EditorGUILayout.PropertyField(m_skeletalDrawingMode, labelTooltip);   
            GUILayout.EndVertical();

            //if the user chose a skeletal drawing mode requiring the user to input the colors
            if (m_bodiesSkeletalsManager.SkeletalDrawingMode == SkeletalsDrawingMode.FixedColors)
            {
                //Positive color
                GUILayout.BeginVertical();
                labelTooltip = new GUIContent("Positive Color", "Positive color, to be used for joints with 100% confidence");                
                EditorGUILayout.PropertyField(m_positiveColor, labelTooltip);   
                GUILayout.EndVertical();

                //Negative color
                GUILayout.BeginVertical();
                labelTooltip = new GUIContent("Negative Color", "Negative color, to be used for joints with 0% confidence");                
                EditorGUILayout.PropertyField(m_negativeColor, labelTooltip);   
                GUILayout.EndVertical();
            }

            //Joints material
            GUILayout.BeginVertical();
            labelTooltip = new GUIContent("Joints Material", "The material to draw the joints with");            
            EditorGUILayout.PropertyField(m_jointsMaterial, labelTooltip);   
            GUILayout.EndVertical();

            //Joints Sphere Radius
            GUILayout.BeginVertical();
            labelTooltip = new GUIContent("Joints Sphere Radius", "Radius of the sphere representing each drawn joint");            
            EditorGUILayout.PropertyField(m_jointSphereRadius, labelTooltip);   
            GUILayout.EndVertical();

            //Limbs material
            GUILayout.BeginVertical();
            labelTooltip = new GUIContent("Limbs Material", "The material to draw the limbs (the lines connecting the joints) with");            
            EditorGUILayout.PropertyField(m_limbsMaterial, labelTooltip);  
            GUILayout.EndVertical();

            //Limbs color
            GUILayout.BeginVertical();
            labelTooltip = new GUIContent("Bones Color", "Color to be used to draw skeleton bones");
            EditorGUILayout.PropertyField(m_limbsColor, labelTooltip);
            GUILayout.EndVertical();

            //Limbs thickness
            GUILayout.BeginVertical();
            labelTooltip = new GUIContent("Limbs Thickness", "Thickness of lines representing the limbs");           
            EditorGUILayout.PropertyField(m_connectingLinesThickness, labelTooltip);  
            GUILayout.EndVertical();

            //colliders yes/no
            GUILayout.BeginVertical();
            labelTooltip = new GUIContent("Attach Colliders", "True to add colliders for hands and feet, false otherwise");
            EditorGUILayout.PropertyField(m_addColliders, labelTooltip);
            GUILayout.EndVertical();

            //shadows yes/no
            GUILayout.BeginVertical();
            labelTooltip = new GUIContent("Shadows Enabled", "True if the body has to cast/receive shadows, false otherwise");
            EditorGUILayout.PropertyField(m_shadowsEnabled, labelTooltip);
            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
}


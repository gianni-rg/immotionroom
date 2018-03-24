﻿namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.ScreenManagers
{
    using UnityEngine;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedManager;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using UnityEngine.UI;
    using ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.DataSourcesManagement;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.Common;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Tools;
    using System.Linq;

    //TODO: when tracking system will permit girello configuration, change this behaviour to change Girello data at runtime
    /// <summary>
    /// Manages GirelloConfiguration scene behaviour, for mobile system (e.g. Android)
    /// </summary>
    public partial class GirelloConfigurationMobile : MonoBehaviour
    {
        #region Public Unity Properties

        /// <summary>
        /// Material to draw the bounding box contours with
        /// </summary>
        [Tooltip("Material to draw the bounding box contours with")]
        public Material BoundsLinesMaterial;

        /// <summary>
        /// Color into which the external game area limits have to be drawn
        /// </summary>
        [Tooltip("Color into which the external game area limits have to be drawn")]
        public Color ExternalGameAreaColor = Color.red;

        /// <summary>
        /// Color into which the internal game area limits have to be drawn
        /// </summary>
        [Tooltip("Color into which the internal game area limits have to be drawn")]
        public Color InternalGameAreaColor = Color.yellow;

        /// <summary>
        /// Proportion of inner bounds sides wrt outer bounds sides
        /// </summary>
        [Tooltip("Proportion of inner bounds sides wrt outer bounds sides")]
        public float InnerToOuterBoundsProportion = 0.775f;

        #endregion

        #region Private Fields

        /// <summary>
        /// Internal implementation of the class
        /// </summary>
        private GirelloConfigurationMobileInternal m_internalImplementation;

        #endregion

        #region Behaviour methods

        void Awake()
        {
            m_internalImplementation = new GirelloConfigurationMobileInternal(this);
        }

        void Start()
        {
            m_internalImplementation.Start();
        }

        void OnDestroy()
        {
            m_internalImplementation.OnDestroy();
        }

        void Update()
        {
            m_internalImplementation.Update();
        }

        #endregion

        #region Buttons events methods

        /// <summary>
        /// Triggered when the OK button gets clicked
        /// </summary>
        public void OnOkButtonClicked()
        {
            m_internalImplementation.OnOkButtonClicked();
        }

        /// <summary>
        /// Triggered when the LeftLimit button gets clicked
        /// </summary>
        public void OnLeftLimitButtonClicked()
        {
            m_internalImplementation.OnLeftLimitButtonClicked();             
        }

        /// <summary>
        /// Triggered when the FrontLimit button gets clicked
        /// </summary>
        public void OnFrontLimitButtonClicked()
        {
            m_internalImplementation.OnFrontLimitButtonClicked();
        }

        /// <summary>
        /// Triggered when the BackLimit button gets clicked
        /// </summary>
        public void OnBackLimitButtonClicked()
        {
            m_internalImplementation.OnBackLimitButtonClicked();   
        }

        /// <summary>
        /// Triggered when the RightLimit button gets clicked
        /// </summary>
        public void OnRightLimitButtonClicked()
        {
            m_internalImplementation.OnRightLimitButtonClicked(); 
        }

        /// <summary>
        /// Triggered when the RESET button gets clicked
        /// </summary>
        public void OnResetLimitsButtonClicked()
        {
            m_internalImplementation.OnResetLimitsButtonClicked();
        }

        #endregion
    }

}
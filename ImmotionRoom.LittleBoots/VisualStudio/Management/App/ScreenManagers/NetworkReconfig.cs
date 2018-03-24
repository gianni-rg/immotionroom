﻿namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.ScreenManagers
{
    using UnityEngine;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedManager;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using UnityEngine.UI;

    /// <summary>
    /// Manages NetworkReconfig scene behaviour
    /// </summary>
    public partial class NetworkReconfig : MonoBehaviour
    {
        #region Private Fields

        /// <summary>
        /// Internal implementation of the class
        /// </summary>
        private NetworkReconfigInternal m_internalImplementation;

        #endregion

        #region Behaviour methods

        void Awake()
        {
            m_internalImplementation = new NetworkReconfigInternal(this);  
        }

        void Start()
        { 
            m_internalImplementation.Start();
        }

        void OnDestroy()
        {
            m_internalImplementation.OnDestroy();
        }

        #endregion

        #region Misc methods

        /// <summary>
        /// Triggered when the OK button gets clicked
        /// </summary>
        public void OnOkButtonClicked()
        {
            m_internalImplementation.OnOkButtonClicked();
        }

        #endregion

    }

}
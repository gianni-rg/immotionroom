namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.ScreenManagers
{
    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections;
    using System.Linq;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedManager;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.DataStructures;

    /// <summary>
    /// Manages MasterSetting scene behaviour
    /// </summary>
    public partial class MasterSetting : MonoBehaviour
    {
        #region Private Fields

        /// <summary>
        /// Internal implementation of the class
        /// </summary>
        private MasterSettingInternal m_internalImplementation;

        #endregion

        #region Behaviour methods

        void Awake()
        {
            m_internalImplementation = new MasterSettingInternal(this);
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
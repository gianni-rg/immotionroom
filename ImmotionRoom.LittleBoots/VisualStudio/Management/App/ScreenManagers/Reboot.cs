namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.ScreenManagers
{
    using UnityEngine;
    using System.Collections;

    /// <summary>
    /// Manages Reboot scene behaviour
    /// </summary>
    public partial class Reboot : MonoBehaviour
    {
        #region Private Fields

        /// <summary>
        /// Internal implementation of the class
        /// </summary>
        private RebootInternal m_internalImplementation;

        #endregion

        #region Behaviour methods

        void Awake()
        {
            m_internalImplementation = new RebootInternal(this);
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
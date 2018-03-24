namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.Localization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Simple behaviour to change program global language
    /// </summary>
    public class LanguageChooser : MonoBehaviour
    {
        #region Unity Public properties

        /// <summary>
        /// Current language the program has to use for localization purposes. 
        /// This field is used only inside the Editor script to ensure serialization of this datum.
        /// </summary>
        [Tooltip("Key string to identify the localized string inside the localization dictionary")]
        [SerializeField]
        private ManagedLanguage m_currentLanguage;

        #endregion

        #region Public methods

        /// <summary>
        /// Asks the underlying LanguageManager to change current language
        /// </summary>
        /// <param name="newLanguage">New Language to set</param>
        public void ChangeLanguage(ManagedLanguage newLanguage)
        {
            m_currentLanguage = newLanguage;
            LanguageManager.Instance.CurrentLanguage = newLanguage;
        }

        #endregion
    }
}

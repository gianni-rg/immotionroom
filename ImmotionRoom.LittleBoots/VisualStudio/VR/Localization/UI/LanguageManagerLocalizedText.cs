namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.Localization.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Represents a standard UI.Text element, where Text gets localized using the LanguageManager
    /// </summary>
    public class LanguageManagerLocalizedText : Text
    {
        #region Unity Public properties

        /// <summary>
        /// Key string to identify the localized string inside the localization dictionary to show inside this text box.
        /// Notice that this field is read only at startup. For further modifications, use KeyString public property
        /// </summary>
        [Tooltip("Key string to identify the localized string inside the localization dictionary")]
        [SerializeField]
        public string m_keyString;

        #endregion

        #region Public Properties

        /// <summary>
        /// Get or set the key string of the localized string to display inside this text box.
        /// After key change, the shown text gets modified
        /// </summary>
        public string KeyString
        {
            get
            {
                return m_keyString;
            }
            set
            {
                m_keyString = value;
                text = LanguageManager.Instance.GetLocalizedString(m_keyString);
            }
        }

        #endregion

        #region Behaviour methods

        new void Awake()
        {
            //to ensure consistency of dev values inserted in inspector with actual state of object
            KeyString = m_keyString;

            //register to language changed events
            LanguageManager.Instance.LanguageManagerChanged += LanguageManagerCurrentLangChanged;
            base.Awake();      
        }

        new void OnDestroy()
        {
            //unregister to language changed events
            LanguageManager.Instance.LanguageManagerChanged -= LanguageManagerCurrentLangChanged;
        }

        #endregion

        #region Language Manager events handling

        /// <summary>
        /// React to a language change event
        /// </summary>
        /// <param name="eventArgs"></param>
        void LanguageManagerCurrentLangChanged(LanguageManagerChangedEventArgs eventArgs)
        {
            //do nothing in editor mode, to not conflict with LanguageManagerLocalizedTextEditor
            if (Application.isPlaying)
            {
                this.text = LanguageManager.Instance.GetLocalizedString(m_keyString);
            }
        }

        #endregion
    }
}

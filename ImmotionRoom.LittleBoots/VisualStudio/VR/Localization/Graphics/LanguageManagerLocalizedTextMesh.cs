namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.Localization.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Represents an add-on for a TextMesh element, where TextMesh text gets localized using the LanguageManager.
    /// Add this script to a gameobject holding a TextMesh element to obtain localization of TextMesh
    /// </summary>
    [RequireComponent(typeof(TextMesh))]
    public class LanguageManagerLocalizedTextMesh : MonoBehaviour
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

        #region Private fields

        /// <summary>
        /// Reference to the text mesh object we're gonna managing
        /// </summary>
        private TextMesh m_textMesh;

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

                if(Application.isPlaying)
                    m_textMesh.text = LanguageManager.Instance.GetLocalizedString(m_keyString);
                else //if we're here, the editor script is trying to make us show a preview... but we've not set m_textMesh in the awake... so always call getcomponent
                    GetComponent<TextMesh>().text = LanguageManager.Instance.GetLocalizedString(m_keyString);
            }
        }

        #endregion

        #region Behaviour methods

        void Awake()
        {
            //get reference to the TextMesh
            m_textMesh = GetComponent<TextMesh>();

            //to ensure consistency of dev values inserted in inspector with actual state of object
            KeyString = m_keyString;

            //register to language changed events
            LanguageManager.Instance.LanguageManagerChanged += LanguageManagerCurrentLangChanged;
        }

        void OnDestroy()
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
                m_textMesh.text = LanguageManager.Instance.GetLocalizedString(m_keyString);
            }
        }

        #endregion
    }
}

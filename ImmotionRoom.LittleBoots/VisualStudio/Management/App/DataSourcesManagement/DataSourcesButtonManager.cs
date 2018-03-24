namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.DataSourcesManagement
{
    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedManager;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.DataStructures;
    using UnityEngine.Events;
    using System.Linq;
    
    /// <summary>
    /// Manages a set of buttons relative to the data sources. When this object gets enabled, it
    /// gets all data sources names and add as child of this gameobject a button for each of the data sources connected to the system,
    /// with the button label equal to the data source name
    /// </summary>
    public partial class DataSourcesButtonManager : MonoBehaviour
    {
        #region Public Unity properties

        /// <summary>
        /// Template of buttons to be added for the data sources
        /// </summary>
        [Tooltip("Button template: for each Data Source will be created a button using this prefab")]
        public GameObject ButtonTemplate;

        /// <summary>
        /// Color with which write the label of the master data source button
        /// </summary>
        [Tooltip("Color with which write the label of the master data source button")]
        public Color MasterColor = new Color(0, 128 / 255.0f, 255 / 255.0f);

        /// <summary>
        /// True to put to on the master data source button and the other to off; false to leave every button to off (used only if template is toggle)
        /// </summary>
        [Tooltip("True to put to on the master data source button and the other to off; false to leave every button to off (used only if template is toggle)")]
        public bool MasterToggleOn = true;

        /// <summary>
        /// Optional ToggleGroup to give radio button functionalities to the created buttons (used only if template is toggle)
        /// </summary>
        [Tooltip("Optional ToggleGroup to make only one of the buttons selectable at a time (used only if template is toggle)")]
        public ToggleGroup RadioButtonsToggleGroup;

        /// <summary>
        /// Callback to execute when any of the button gets pressed (leave to null if not used).
        /// It will receive new statuses for the Toggle buttons and simple events with always true parameters for pressures of the Button buttons.
        /// First parameters passed is the label of the button, the second one is the new status of the button.
        /// </summary>
        [Tooltip("Callback to execute when any of the button gets pressed (leave to null if not used)")]
        public UnityAction<string, bool> ButtonsPressedCallback;

        #endregion

        #region Private fields

        /// <summary>
        /// Internal implementation of the class
        /// </summary>
        DataSourcesButtonManagerInternal m_internalImplementation;

        /// <summary>
        /// Reference to the button that refers to the master data source (if any)
        /// </summary>
        private GameObject m_masterButton;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the label of current master button, if any (otherwise returns Empty string)
        /// </summary>
        public string MasterButtonLabel
        {
            get
            {
                return m_masterButton == null ? string.Empty : (m_masterButton.GetComponent<Toggle>() != null ?
                    m_masterButton.transform.Find("Label").GetComponent<Text>().text :
                    m_masterButton.transform.Find("Text").GetComponent<Text>().text);
            }
        }

        /// <summary>
        /// Gets the managed button with the provided label
        /// </summary>
        /// <param name="label">Label of interest</param>
        /// <returns>Button corresponding to that label, or null if no such button</returns>
        public GameObject this[string label]
        {
            get
            {
                return transform.Find(label).gameObject;
            }
        }

        #endregion

        #region Behaviour methods

        void Awake()
        {
            m_internalImplementation = new DataSourcesButtonManagerInternal(this);
        }

        void OnEnable()
        {
            m_internalImplementation.OnEnable();
        }

        void OnDestroy()
        {
            m_internalImplementation.OnDestroy();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Add a button to the control of the data sources / tracking services
        /// </summary>
        /// <param name="label">Label to write on the button</param>
        /// <param name="isMaster">True if this button is relative to a master data source, false otherwise</param>
        internal void AddButton(string label, bool isMaster)
        {
            m_internalImplementation.AddButton(label, isMaster);
        }

        #endregion
    }

}

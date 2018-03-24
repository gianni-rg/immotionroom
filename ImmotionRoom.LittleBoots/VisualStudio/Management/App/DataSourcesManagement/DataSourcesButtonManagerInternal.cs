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
        /// <summary>
        /// Contains the actual definition of the DataSourcesButtonManager, for obfuscation purposes
        /// </summary>
        private partial class DataSourcesButtonManagerInternal
        {
            #region Private fields

            /// <summary>
            /// The Data Sources Button Manager that contains this object
            /// </summary>
            private DataSourcesButtonManager m_dsButtonManager;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="buttonsManager">Enclosing instance, whose code has to be implemented</param>
            internal DataSourcesButtonManagerInternal(DataSourcesButtonManager buttonsManager)
            {
                m_dsButtonManager = buttonsManager;
            }

            #endregion

            #region Behaviour methods

            internal void OnEnable()
            {
                //clear all buttons added in the editor
                foreach (Transform buttonTransform in m_dsButtonManager.transform)
                    if (buttonTransform.GetInstanceID() != m_dsButtonManager.transform.GetInstanceID())
                        Destroy(buttonTransform.gameObject);

                //add a button for each data source, in alphabetical order
                foreach (DataSourceInfo dataSourceInfo in TrackingServiceManagerAdvanced.Instance.DataSourcesInfo.Values.OrderBy(dataSourceInfo => (dataSourceInfo.IsMaster)).ThenBy(dataSourceInfo => (dataSourceInfo.Id)))
                {
                    AddButton(dataSourceInfo.Id, dataSourceInfo.IsMaster);
                }
            }

            internal void OnDestroy()
            {
                //remove all event listeners from the data sources buttons (if any)
                foreach (Transform buttonTransform in m_dsButtonManager.transform)
                    if (buttonTransform.GetComponent<Button>())
                        buttonTransform.GetComponent<Button>().onClick.RemoveAllListeners();
                    else if (buttonTransform.GetComponent<Toggle>())
                        buttonTransform.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
            }

            #endregion

            #region Internal methods

            /// <summary>
            /// Add a button to the control of the data sources / tracking services
            /// </summary>
            /// <param name="label">Label to write on the button</param>
            /// <param name="isMaster">True if this button is relative to a master data source, false otherwise</param>
            internal void AddButton(string label, bool isMaster)
            {
                Transform buttonsRootTransform = m_dsButtonManager.transform;

                GameObject newRadioButton = Instantiate<GameObject>(m_dsButtonManager.ButtonTemplate);
                newRadioButton.name = label;
                newRadioButton.transform.SetParent(buttonsRootTransform, false);

                //if the created button is a toggle
                if (newRadioButton.GetComponent<Toggle>() != null)
                {
                    //set the label according to the data source name
                    newRadioButton.transform.Find("Label").GetComponent<Text>().text = label;

                    //if this is the master, write the name in a different color and save its reference
                    if (isMaster)
                    {
                        newRadioButton.transform.Find("Label").GetComponent<Text>().color = m_dsButtonManager.MasterColor;
                        m_dsButtonManager.m_masterButton = newRadioButton;
                    }

                    //set the toggle group, so the toggles acquire radio button behaviour
                    newRadioButton.GetComponent<Toggle>().group = m_dsButtonManager.RadioButtonsToggleGroup;

                    //select the button if we have a master data source (and this functionality had been requested)
                    if (isMaster && m_dsButtonManager.MasterToggleOn)
                    {
                        newRadioButton.GetComponent<Toggle>().isOn = true;
                    }

                    //register for state changes of the button
                    newRadioButton.GetComponent<Toggle>().onValueChanged.AddListener((newStatus) =>
                    {
                        if (m_dsButtonManager.ButtonsPressedCallback != null) //checked here because at script start this field may still be not filled (another behaviour can fill it later in the program)
                            m_dsButtonManager.ButtonsPressedCallback(newRadioButton.transform.Find("Label").GetComponent<Text>().text, newStatus);
                    });
                }
                //if the crated button is a simple button
                else if (newRadioButton.GetComponent<Button>() != null)
                {
                    //set the label according to the data source name
                    newRadioButton.transform.Find("Text").GetComponent<Text>().text = label;

                    //if this is the master, write the name in a different color
                    if (isMaster)
                    {
                        newRadioButton.transform.Find("Text").GetComponent<Text>().color = m_dsButtonManager.MasterColor;
                        m_dsButtonManager.m_masterButton = newRadioButton;
                    }

                    //register for pressure changes of the button
                    newRadioButton.GetComponent<Button>().onClick.AddListener(() => { m_dsButtonManager.ButtonsPressedCallback(newRadioButton.transform.Find("Text").GetComponent<Text>().text, true); });
                }
            }

            #endregion
        }

    }

}

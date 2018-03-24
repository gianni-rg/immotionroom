namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.ScreenManagers
{
    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections;
    using System.Linq;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedManager;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.DataStructures;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils.MessageBoxes;

    /// <summary>
    /// Manages MasterSetting scene behaviour
    /// </summary>
    public partial class MasterSetting : MonoBehaviour
    {
        /// <summary>
        /// Contains the actual definition of the MasterSetting, for obfuscation purposes
        /// </summary>
        private class MasterSettingInternal
        {
            #region Private fields

            /// <summary>
            /// The MasterSetting object that contains this object
            /// </summary>
            private MasterSetting m_enclosingInstance;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="enclosingInstance">Enclosing instance, whose code has to be implemented</param>
            internal MasterSettingInternal(MasterSetting enclosingInstance)
            {
                m_enclosingInstance = enclosingInstance;
            }

            #endregion

            #region Behaviour methods

            internal void Start()
            {
                //register to master changed event
                TrackingServiceManagerAdvanced.Instance.NewMasterDataSourceSet += OnNewMasterDataSourceSet;
            }

            internal void OnDestroy()
            {
                if (TrackingServiceManagerAdvanced.Instance != null)
                    TrackingServiceManagerAdvanced.Instance.NewMasterDataSourceSet -= OnNewMasterDataSourceSet;
            }

            #endregion

            #region Tracking Service Manager events

            /// <summary>
            /// Event handler called when the new master data source setting operation finishes
            /// </summary>
            /// <param name="eventArgs">Result of the operation</param>
            private void OnNewMasterDataSourceSet(AdvancedOperationEventArgs eventArgs)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("MasterSetting - New master data source setting opertion terminated with result {0}", eventArgs.ErrorString ?? "SUCCESS");
                }

                //we're not in waiting stage anymore (command has been processed by the underlying tracking service)
                FindObjectOfType<WaitManager>().WaitingState = false;

                //if everything went well, go forward in the wizard or return to main menu
                if (eventArgs.ErrorString == null)
                    ScenesManager.Instance.PopOrNextInWizard("VisualizerScene");
                //otherwise, show a message box and abort wizard, if any
                else
                    MessageBox.Show("Error", "Configuration failed: " + eventArgs.ErrorString + ".\nPlease retry to set a master data source", new UnityEngine.Events.UnityAction(() => { TrackingServiceManagerAdvanced.Instance.ForceStateToIdle(); ScenesManager.Instance.StopWizard(); }),
                        FindObjectsOfType<Selectable>());
            }

            #endregion

            #region Misc methods

            /// <summary>
            /// Triggered when the OK button gets clicked
            /// </summary>
            internal void OnOkButtonClicked()
            {
                //ask tracking service to set master data source according to label of the selected button
                //if no button has been selected, trigger an error and abort operation
                var activeToggles = m_enclosingInstance.GetComponent<ToggleGroup>().ActiveToggles();

                if (activeToggles.Count() == 0)
                {
                    MessageBox.Show("Error", "No Data Source has been selected.\nPlease retry selecting one or abort using the back button", new UnityEngine.Events.UnityAction(() => { }), // TrackingServiceManagerAdvanced.Instance.ForceStateToIdle(); ScenesManager.Instance.StopWizard(); }),
                        FindObjectsOfType<Selectable>());

                    if (Log.IsErrorEnabled)
                    {
                        Log.Error("MasterSetting - User has selected no data source");
                    }
                }
                else
                {
                    //we're in waiting stage (waiting for the command to be processed by the underlying tracking service)
                    FindObjectOfType<WaitManager>().WaitingState = true;

                    Toggle selectedRadioButton = activeToggles.First();
                    string selectedDataSourceID = selectedRadioButton.transform.Find("Label").GetComponent<Text>().text;
                    TrackingServiceManagerAdvanced.Instance.SetNewMasterDataSourceAsync(selectedDataSourceID);

                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("MasterSetting - Setting {0} data source as master", selectedDataSourceID);
                    }
                }

            }

            #endregion
        }

    }

}

namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.ScreenManagers
{
    using UnityEngine;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedManager;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.DataStructures;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using UnityEngine.UI;
    using System.Linq;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.App.DataSourcesManagement;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils.MessageBoxes;

    /// <summary>
    /// Manages Reboot scene behaviour
    /// </summary>
    public partial class Reboot : MonoBehaviour
    {
        /// <summary>
        /// Contains the actual definition of the Reboot, for obfuscation purposes
        /// </summary>
        private class RebootInternal
        {
            #region Private fields

            /// <summary>
            /// The MasterSetting object that contains this object
            /// </summary>
            private Reboot m_enclosingInstance;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="enclosingInstance">Enclosing instance, whose code has to be implemented</param>
            internal RebootInternal(Reboot enclosingInstance)
            {
                m_enclosingInstance = enclosingInstance;
            }

            #endregion

            #region Behaviour methods

            internal void Start()
            {
                //add a button about the tracking service
                if (TrackingServiceManagerAdvanced.Instance.TrackingServiceInfo != null)
                    FindObjectOfType<DataSourcesButtonManager>().AddButton(TrackingServiceManagerAdvanced.Instance.TrackingServiceInfo.Id, false);

                //register to data source reboot completed event
                TrackingServiceManagerAdvanced.Instance.ServiceRebootCompleted += OnDataSourceRebootCompleted;
            }

            internal void OnDestroy()
            {
                if (TrackingServiceManagerAdvanced.Instance != null)
                    TrackingServiceManagerAdvanced.Instance.ServiceRebootCompleted -= OnDataSourceRebootCompleted;
            }

            #endregion

            #region Tracking Service Manager events

            /// <summary>
            /// Event handler called when the data source reboot operation finishes
            /// </summary>
            /// <param name="eventArgs">Result of the operation</param>
            private void OnDataSourceRebootCompleted(AdvancedOperationEventArgs eventArgs)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Reboot - Data Source reboot operation terminated with result {0}", eventArgs.ErrorString ?? "SUCCESS");
                }

                //we're not in waiting stage anymore (command has been processed by the underlying tracking service)
                FindObjectOfType<WaitManager>().WaitingState = false;

                //if everything went well, return to main menu
                if (eventArgs.ErrorString == null)
                    ScenesManager.Instance.PopScene();
                //otherwise, show a message box and exit
                else
                    MessageBox.Show("Error", "Reboot failed: " + eventArgs.ErrorString + ".\nThe PC may be in an unknown state", new UnityEngine.Events.UnityAction(() => { TrackingServiceManagerAdvanced.Instance.ForceStateToIdle(); ScenesManager.Instance.PopScene(); }),
                        FindObjectsOfType<Selectable>());
            }

            #endregion

            #region Misc methods

            /// <summary>
            /// Triggered when the OK button gets clicked
            /// </summary>
            internal void OnOkButtonClicked()
            {
                //we're in waiting stage (waiting for the command to be processed by the underlying tracking service)
                FindObjectOfType<WaitManager>().WaitingState = true;

                //ask tracking service to reboot a data source according to label of the selected button
                //if no button has been selected, exit doing nothing
                var activeToggles = m_enclosingInstance.GetComponent<ToggleGroup>().ActiveToggles();

                if (activeToggles.Count() == 0)
                {
                    if (Log.IsErrorEnabled)
                    {
                        Log.Error("Reboot - User has selected no data source");
                    }

                    ScenesManager.Instance.PopScene();
                }
                else
                {
                    Toggle selectedRadioButton = activeToggles.First();
                    string selectedDataSourceID = selectedRadioButton.transform.Find("Label").GetComponent<Text>().text;
                    TrackingServiceManagerAdvanced.Instance.RebootServiceAsync(selectedDataSourceID);

                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("Reboot - Rebooting data source {0}", selectedDataSourceID);
                    }
                }

            }

            #endregion
        }

    }

}

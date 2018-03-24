namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.ScreenManagers
{
    using UnityEngine;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedManager;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using UnityEngine.UI;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils.MessageBoxes;

    /// <summary>
    /// Manages Merging scene behaviour
    /// </summary>
    public partial class Merging : MonoBehaviour
    {
        /// <summary>
        /// Contains the actual definition of the Merging, for obfuscation purposes
        /// </summary>
        private class MergingInternal
        {
            #region Private fields

            /// <summary>
            /// True if the user is exiting from this scene using the back button, false otherwise (with the ok button)
            /// </summary>
            private bool m_canceled = false;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            internal MergingInternal()
            {
            }

            #endregion

            #region Behaviour methods

            internal void Start()
            {
                //set back button behaviour to stop tracking
                ScenesManager.Instance.SetBackButtonBehaviour((obj) =>
                {
                    m_canceled = true;
                    OnOkButtonClicked();
                });

                //register to tracking service events
                TrackingServiceManagerAdvanced.Instance.TrackingStarted += OnTrackingStarted;
                TrackingServiceManagerAdvanced.Instance.OperativeStatusStopped += OnOperativeStatusStopped;

                //start tracking at start
                TrackingServiceManagerAdvanced.Instance.RequestTrackingStart();

                //we're in waiting stage (waiting for the command to be processed by the underlying tracking service)
                FindObjectOfType<WaitManager>().WaitingState = true;
            }

            internal void OnDestroy()
            {
                if (TrackingServiceManagerAdvanced.Instance != null)
                {
                    //unregister to tracking service events
                    TrackingServiceManagerAdvanced.Instance.TrackingStarted -= OnTrackingStarted;
                    TrackingServiceManagerAdvanced.Instance.OperativeStatusStopped -= OnOperativeStatusStopped;

                    //stop tracking at exit
                    if (TrackingServiceManagerAdvanced.Instance.IsStreamingSkeletons)
                        TrackingServiceManagerAdvanced.Instance.RequestCurrentOperativeStatusStop();
                }
            }

            #endregion

            #region Tracking Service Manager events

            /// <summary>
            /// Event called when the tracking start operation gets executed
            /// </summary>
            /// <param name="eventArgs">Arguments with result of the operation</param>
            private void OnTrackingStarted(DataStructures.AdvancedOperationEventArgs eventArgs)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Merging - Tracking start operation terminated with result {0}", eventArgs.ErrorString ?? "SUCCESS");
                }

                //we're not in waiting stage anymore (command has been processed by the underlying tracking service)
                FindObjectOfType<WaitManager>().WaitingState = false;

                if (eventArgs.ErrorString != null)
                    MessageBox.Show("Error", "Can't start tracking: " + eventArgs.ErrorString + ".\nPlease retry later", new UnityEngine.Events.UnityAction(() => { TrackingServiceManagerAdvanced.Instance.ForceStateToIdle(); ScenesManager.Instance.StopWizard(); }),
                            FindObjectsOfType<Selectable>());
            }

            /// <summary>
            /// Event called when the tracking stop operation gets executed
            /// </summary>
            /// <param name="eventArgs">Arguments with result of the operation</param>
            private void OnOperativeStatusStopped(DataStructures.AdvancedOperationEventArgs eventArgs)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Merging - Tracking stop operation terminated with result {0}", eventArgs.ErrorString ?? "SUCCESS");
                }

                //we're not in waiting stage anymore (command has been processed by the underlying tracking service)
                FindObjectOfType<WaitManager>().WaitingState = false;

                if (eventArgs.ErrorString != null)
                    MessageBox.Show("Error", "Can't stop tracking: " + eventArgs.ErrorString + ".\nThe system may be in an unknown state", new UnityEngine.Events.UnityAction(() => { TrackingServiceManagerAdvanced.Instance.ForceStateToIdle(); ScenesManager.Instance.StopWizard(); }),
                            FindObjectsOfType<Selectable>());
                else if (!m_canceled)
                    ScenesManager.Instance.PopOrNextInWizard("GirelloConfiguration");
                else
                    //if user pressed back button, return to previous scene
                    ScenesManager.Instance.PopScene();
            }

            #endregion

            #region Misc methods

            /// <summary>
            /// Triggered when the OK button gets clicked
            /// </summary>
            internal void OnOkButtonClicked()
            {
                //stop tracking
                if (TrackingServiceManagerAdvanced.Instance.IsStreamingSkeletons)
                {
                    //we're in waiting stage (waiting for the command to be processed by the underlying tracking service)
                    FindObjectOfType<WaitManager>().WaitingState = true;

                    TrackingServiceManagerAdvanced.Instance.RequestCurrentOperativeStatusStop();
                }
            }

            #endregion
        }

    }
}

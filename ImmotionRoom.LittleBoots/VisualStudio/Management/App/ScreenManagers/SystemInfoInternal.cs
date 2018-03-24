namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.ScreenManagers
{
    using UnityEngine;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedManager;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using UnityEngine.UI;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils.VisualConsole;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils;

    /// <summary>
    /// Manages SystemInfo scene behaviour
    /// </summary>
    public partial class SystemInfo : MonoBehaviour
    {
        /// <summary>
        /// Contains the actual definition of the SystemInfo, for obfuscation purposes
        /// </summary>
        private class SystemInfoInternal
        {
            #region Private fields

            /// <summary>
            /// Current console manager
            /// </summary>
            private ConsoleManager m_consoleManager;

            #endregion

             #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            internal SystemInfoInternal()
            {
            }

            #endregion

            #region Behaviour methods

            internal void Start()
            {
                //gets reference to the console manager
                m_consoleManager = FindObjectOfType<ConsoleManager>();

                //start the get system status operation
                TrackingServiceManagerAdvanced.Instance.SystemStatusRequestCompleted += OnSystemStatusRequestCompleted;
                TrackingServiceManagerAdvanced.Instance.GetSystemStatusAsync();

                //we're in waiting stage (waiting for the command to be processed by the underlying tracking service)
                FindObjectOfType<WaitManager>().WaitingState = true;

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("SystemInfo - Start called. Get Status triggered.");
                }

                //write about the operation on the console
                m_consoleManager.WriteHighlightInfoString("Retrieving system status. Please wait...");
            }

            internal void OnDestroy()
            {
                //it is not necessary to check for non existent events, so remove all
                //http://stackoverflow.com/questions/20888206/is-it-necessary-to-check-if-a-handler-exists-in-a-delegate-chain-before-removing
                if (TrackingServiceManagerAdvanced.Instance != null)
                {
                    TrackingServiceManagerAdvanced.Instance.SystemStatusRequestCompleted -= OnSystemStatusRequestCompleted;
                }
            }

            #endregion

            #region Tracking Service Manager events

            /// <summary>
            /// Event called when the get status operation on the tracking system terminates
            /// </summary>
            /// <param name="eventArgs">Arguments with result of the operation</param>
            private void OnSystemStatusRequestCompleted(DataStructures.TrackingSystemInfoObtainedEventArgs eventArgs)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("SystemInfo - System status obtained.");
                }

                //we're not in waiting stage anymore (command has been processed by the underlying tracking service)
                FindObjectOfType<WaitManager>().WaitingState = false;

                //write on the console the results of the system info operation.
                //Remember to write info only for reachable services (unreachable ones have not been contacted, and so we don't have valid data
                m_consoleManager.WriteHighlightInfoString(" \nTracking Service Data:");

                //if tracking service is reachable
                if (eventArgs.TrackingSystemInformations.IsReachable)
                {
                    m_consoleManager.WriteInfoString(string.Format("Tracking Service: {0}\n\tVersion: {1}\n\tControl Api at: {2}:{3}\n\tReachable: {4}\n\tState: {5}\n\tMaster Data Source: {6}\n\tCalibrated: {7}",
                        eventArgs.TrackingSystemInformations.TrackingServiceId, eventArgs.TrackingSystemInformations.Version, eventArgs.TrackingSystemInformations.ControlApiEndpoint, eventArgs.TrackingSystemInformations.ControlApiPort, eventArgs.TrackingSystemInformations.IsReachable,
                        eventArgs.TrackingSystemInformations.CurrentState.ToString(), (eventArgs.TrackingSystemInformations.MasterDataSource != null && eventArgs.TrackingSystemInformations.MasterDataSource.Length > 0 ? eventArgs.TrackingSystemInformations.MasterDataSource : "- Not Set -"), eventArgs.TrackingSystemInformations.CalibrationDone));

                    //write data sources data
                    m_consoleManager.WriteHighlightInfoString(" \nData Sources Data:");

                    foreach (var dataSourceInfo in eventArgs.TrackingSystemInformations.DataSourcesInfo)
                    {
                        //if data source is reachable
                        if (dataSourceInfo.IsReachable)
                            m_consoleManager.WriteInfoString(string.Format("Data Source: {0}\n\tVersion: {1}\n\tControl Api at: {2}:{3}\n\tReachable: {4}\n\tState: {5}",
                                dataSourceInfo.DataSourceId, dataSourceInfo.Version, dataSourceInfo.ControlApiEndpoint, dataSourceInfo.ControlApiPort, dataSourceInfo.IsReachable, dataSourceInfo.CurrentState));
                        else
                            m_consoleManager.WriteInfoString(string.Format("Data Source: {0}\n\tControl Api at: {1}:{2}\n\tReachable: {3}",
                                dataSourceInfo.DataSourceId, dataSourceInfo.ControlApiEndpoint, dataSourceInfo.ControlApiPort, dataSourceInfo.IsReachable));
                    }
                }
                //tracking service not reachable
                else
                {
                    m_consoleManager.WriteInfoString(string.Format("Tracking Service: {0}\n\tControl Api at: {1}:{2}\n\tReachable: {3}",
                    eventArgs.TrackingSystemInformations.TrackingServiceId, eventArgs.TrackingSystemInformations.ControlApiEndpoint, eventArgs.TrackingSystemInformations.ControlApiPort, eventArgs.TrackingSystemInformations.IsReachable));
                }
            }

            #endregion

            #region Misc events

            /// <summary>
            /// Triggered when the OK button gets clicked
            /// </summary>
            internal void OnOkButtonClicked()
            {
                //if we're int this scene, then surely we are not during a wizard
                ScenesManager.Instance.PopScene();
            }

            #endregion
        }

    }
}

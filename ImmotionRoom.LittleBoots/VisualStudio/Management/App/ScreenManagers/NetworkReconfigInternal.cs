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
    /// Manages NetworkReconfig scene behaviour
    /// </summary>
    public partial class NetworkReconfig : MonoBehaviour
    {
        /// <summary>
        /// Contains the actual definition of the NetworkReconfig, for obfuscation purposes
        /// </summary>
        private class NetworkReconfigInternal
        {
            #region Private fields

            /// <summary>
            /// Current console manager
            /// </summary>
            private ConsoleManager m_consoleManager;

            /// <summary>
            /// True if a discovery is already happened. This flag is useful because tracking service and data sources discovery happens
            /// asynchronously, so we don't know which one finishes before. So the first one that finishes sets the flag, and the second one
            /// triggers network reconfig
            /// </summary>
            private bool m_discoveredFlag;

            /// <summary>
            /// True if there has been any error during the operations so far
            /// </summary>
            private bool m_errorFlag;

            //TODO: fix this hack
            /// <summary>
            /// Current disovery try number. At startup first discovery may fail, still don't know why
            /// </summary>
            private int m_tryNumber;

            /// <summary>
            /// The NetworkReconfig object that contains this object
            /// </summary>
            private NetworkReconfig m_enclosingInstance;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="enclosingInstance">Enclosing instance, whose code has to be implemented</param>
            internal NetworkReconfigInternal(NetworkReconfig enclosingInstance)
            {
                m_enclosingInstance = enclosingInstance;
            }

            #endregion

            #region Behaviour methods

            internal void Start()
            {
                // Disable screen dimming
                Screen.sleepTimeout = SleepTimeout.NeverSleep;

                //gets reference to the console manager
                m_consoleManager = FindObjectOfType<ConsoleManager>();

                //if we are at startup connection, destroy the OK button, because we will automatically switch to main screen after a timeout
                //and change title to initialization
                if (ScenesManager.Instance.IsRoot())
                {
                    GameObject.Find("Title").GetComponent<Text>().text = "Initialization";
                    Destroy(GameObject.Find("Buttons Panel"));
                }
                //else add to instruction label that we're also configuring the system and not only detecting them
                else
                {
                    GameObject.Find("Instructions").GetComponent<Text>().text += " and configures them";
                }

                //set waiting state (this will handle back button and ok buttons activation)
                FindObjectOfType<WaitManager>().WaitingState = true;

                //start the discovery operation of tracking service and data sources
                TrackingServiceManagerAdvanced.Instance.TrackingServiceDiscovered += OnTrackingServiceDiscovered;
                TrackingServiceManagerAdvanced.Instance.DataSourcesDiscovered += OnDataSourcesDiscovered;

                //trigger discovery... we do it in a coroutine, so the engine will show the UI and then waits inside the already
                //loaded UI
                //m_enclosingInstance.StartCoroutine(StartDiscovery());
                StartDiscovery();
            }

            internal void OnDestroy()
            {
                //m_enclosingInstance.StopAllCoroutines();

                //it is not necessary to check for non existent events, so remove all
                //http://stackoverflow.com/questions/20888206/is-it-necessary-to-check-if-a-handler-exists-in-a-delegate-chain-before-removing
                if (TrackingServiceManagerAdvanced.Instance != null)
                {
                    TrackingServiceManagerAdvanced.Instance.TrackingServiceDiscovered -= OnTrackingServiceDiscovered;
                    TrackingServiceManagerAdvanced.Instance.DataSourcesDiscovered -= OnDataSourcesDiscovered;
                    TrackingServiceManagerAdvanced.Instance.NetworkReconfigured -= OnNetworkReconfigured;
                }
            }

            #endregion

            #region Tracking Service Manager events

            /// <summary>
            /// Event called when the tracking service gets discovered
            /// </summary>
            /// <param name="eventArgs">Arguments with result of the operation</param>
            private void OnTrackingServiceDiscovered(DataStructures.DiscoveredServiceEventArgs eventArgs)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("NetworkReconfig - Tracking service discovery finished with result {0} for service {1} at {2} ", (eventArgs.ErrorString ?? "SUCCESS"), eventArgs.HumanReadableName, eventArgs.DataIpPort);
                }

                //write to console and log error, if any
                if (eventArgs.ErrorString == null)
                    m_consoleManager.WriteInfoString(string.Format("Found Tracking Service {0} at {1}", eventArgs.HumanReadableName, eventArgs.DataIpPort));
                else
                {
                    m_consoleManager.WriteErrorString(string.Format("Can't connect to a Tracking Service. The system reports {0}", eventArgs.ErrorString));
                    m_errorFlag = true;
                }

                //if it is the second discovery operation to finish, call the method to go on
                if (m_discoveredFlag == true)
                    FinishDiscovery();
                //else, set the flag so the next discovery operation will trigger the end
                else
                    m_discoveredFlag = true; 
            }

            /// <summary>
            /// Event called when all data sources of the network get discovered
            /// </summary>
            /// <param name="eventArgs">Arguments with result of the operation</param>
            private void OnDataSourcesDiscovered(DataStructures.DiscoveredDataSourcesEventArgs eventArgs)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("NetworkReconfig - Data sources discovery finished with result {0}", (eventArgs.ErrorString ?? "SUCCESS"));

                    if (eventArgs.DataIpPorts != null)
                        for (int i = 0; i < eventArgs.DataIpPorts.Length; i++)
                        {
                            Log.Debug(string.Format("Found Data Source {0} at {1}", eventArgs.HumanReadableNames[i], eventArgs.DataIpPorts[i]));
                        }
                }

                //write to console
                if (eventArgs.ErrorString == null)
                {
                    if (eventArgs.DataIpPorts != null)
                        for (int i = 0; i < eventArgs.DataIpPorts.Length; i++)
                        {
                            m_consoleManager.WriteInfoString(string.Format("Found Data Source {0} at {1}", eventArgs.HumanReadableNames[i], eventArgs.DataIpPorts[i]));
                        }
                }
                else
                {
                    m_consoleManager.WriteErrorString(string.Format("Can't connect to the Data Sources. The system reports {0}", eventArgs.ErrorString));
                    m_errorFlag = true;
                }

                if (m_discoveredFlag == true)
                    FinishDiscovery();
                //else, set the flag so the next discovery operation will trigger the end
                else
                    m_discoveredFlag = true;

            }

            /// <summary>
            /// Event called when all data sources of the network finished the reconfiguring operation
            /// </summary>
            /// <param name="eventArgs">Arguments with result of the operation</param>
            private void OnNetworkReconfigured(DataStructures.ReconfiguredServicesEventArgs eventArgs)
            {
                if (Log.IsDebugEnabled)
                {
                    foreach (string id in eventArgs.HumanReadableNames)
                    {
                        Log.Debug("NetworkReconfig - Reconfigured {0}", id);
                    }

                    Log.Debug("NetworkReconfig - Network reconfiguration finished with result {0}", (eventArgs.ErrorString ?? "SUCCESS"));

                }

                //write onto the visual console the result of the operation
                if (eventArgs.ErrorString == null)
                    m_consoleManager.WriteHighlightInfoString("Network reconfiguration successfully finished");
                else
                {
                    m_errorFlag = true; // to avoid going to the next screen on OK pressure if the reconfig went wrong  
                    m_consoleManager.WriteErrorString(eventArgs.ErrorString);
                    m_consoleManager.WriteHighlightInfoString("Discovery finished with errors.\nCheck services are up and connected and try again");
                }

                //re-enable ok button (re-config is performed only if requested by the user, so we surely are not in the initialization screen)
                //and re-enable back button behaviour
                //re-set waiting state (this will handle back button and ok buttons activation)
                FindObjectOfType<WaitManager>().WaitingState = false;
            }

            #endregion

            #region Misc methods

            /// <summary>
            /// Starts the discovery at the beginning of the program
            /// </summary>
            /// <returns></returns>
            private void StartDiscovery()
            {
                //wait for unity to show the ui
                //yield return new WaitForSeconds(0.1f);

                //if we are at startup connection, try to connect using last saved settings for faster startup times
                //(absolutely don't do it if an explicit discovery has been requested)
                if (ScenesManager.Instance.IsRoot())
                {
                    TrackingServiceManagerAdvanced.Instance.NetworkDiscoveryConnUsingSettingsAsync();
                    //write about the operation on the console
                    m_consoleManager.WriteHighlightInfoString("Network connection using last saved settings. Please wait...");
                }
                else
                {
                    TrackingServiceManagerAdvanced.Instance.NetworkDiscoveryAsync();
                    //write about the operation on the console
                    m_consoleManager.WriteHighlightInfoString("Network discovery has started. Please wait...");
                }

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("NetworkReconfig - Start called. Network discovery triggered.");
                }

                //this is the first discovery try
                m_tryNumber = 0;

                //yield break;
            }

            /// <summary>
            /// Perform the operation after the discovery 
            /// </summary>
            private void FinishDiscovery()
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("NetworkReconfig - Discovery finished");
                }

                //prompt on console results of the discovery operation.
                //if we were at startup and discovery failed, try to re-perform it (this is a hack due to a bug that make fail first
                //discovery in build)
                //TODO: FIX BUG and remove hack
                if (m_errorFlag && m_tryNumber == 0 && ScenesManager.Instance.IsRoot())
                {
                    m_consoleManager.WriteHighlightInfoString("Discovery finished with errors. This may happen at startup. Retrying with a complete discovery...");

                    //reset all flags and re-launch discovery
                    m_tryNumber++;
                    m_errorFlag = false;
                    m_discoveredFlag = false;
                    TrackingServiceManagerAdvanced.Instance.NetworkDiscoveryAsync();

                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("NetworkReconfig - Retrying startup discovery");
                    }

                    return; //to avoid switching to next screen with the following lines
                }
                else if (m_errorFlag)
                    m_consoleManager.WriteHighlightInfoString("Discovery finished with errors.\nCheck services are up and try again");
                else
                    m_consoleManager.WriteHighlightInfoString("Discovery finished");

                //if this is the discovery performed at app startup, re-enable back-button and pass to the main first screen after a little delay
                if (ScenesManager.Instance.IsRoot())
                {
                    //re-set waiting state (this will handle back button and ok buttons activation)
                    FindObjectOfType<WaitManager>().WaitingState = false;

                    m_enclosingInstance.StartCoroutine(WaitAndGoToFirstScreen());
                }
                //else, if an error occurred, simply re-enable OK button
                else if (m_errorFlag)
                {
                    //re-set waiting state (this will handle back button and ok buttons activation)
                    FindObjectOfType<WaitManager>().WaitingState = false;
                }
                //else, if everything went fine and we are here because user expressely asked for a network reconfiguration, perform the reconfig
                else
                {
                    //perform system network re-configuration
                    TrackingServiceManagerAdvanced.Instance.NetworkReconfigured += OnNetworkReconfigured;
                    TrackingServiceManagerAdvanced.Instance.NetworkReconfigAsync();

                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("NetworkReconfig - Network reconfiguration started");
                    }

                    m_consoleManager.WriteHighlightInfoString("Network reconfiguration has started. Please wait...");
                }
            }

            /// <summary>
            /// Wait a second and then go to the App first screen
            /// </summary>
            /// <returns></returns>
            private IEnumerator WaitAndGoToFirstScreen()
            {
                if(Application.platform == RuntimePlatform.Android)
                    yield return new WaitForSeconds(1.916f); //magical number :)
                else
                    yield return new WaitForSeconds(2.476f); //magical number... wait more on pc because operation is faster and you have less time to read :)

                ScenesManager.Instance.GoToSceneAndForget("FirstScreen");
            }

            /// <summary>
            /// Triggered when the OK button gets clicked
            /// </summary>
            internal void OnOkButtonClicked()
            {
                //if everything went fine, go on in the wizard or return to main menu, depending on current mode
                if (!m_errorFlag)
                    ScenesManager.Instance.PopOrNextInWizard("MasterSetting");
                //if there is an error, exit from wizard, aborting everything
                else
                    ScenesManager.Instance.StopWizard();
            }

            #endregion
        }

    }
}

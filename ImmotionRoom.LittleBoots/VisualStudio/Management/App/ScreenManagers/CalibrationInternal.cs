namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.ScreenManagers
{
    using UnityEngine;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedManager;
    using ImmotionAR.ImmotionRoom.TrackingService.ControlClient.Model;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using UnityEngine.UI;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedAvateering;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.DataSourcesManagement;
    using ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Tools;
    using System.Collections.Generic;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.App.DataSourcesManagement;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils.MessageBoxes;

    /// <summary>
    /// Manages Calibration scene behaviour
    /// </summary>
    public partial class Calibration : MonoBehaviour
    {
        /// <summary>
        /// Contains the actual definition of the Calibration, for obfuscation purposes
        /// </summary>
        private class CalibrationInternal
        {
            #region Constants

            /// <summary>
            /// List of available positve colors
            /// </summary>
            protected static readonly Color[] PositiveColors = new Color[] { Color.green, Color.white, new Color(0, 1, 0, 0.5f), new Color(1, 1, 1, 0.5f) };

            /// <summary>
            /// List of available negative colors
            /// </summary>
            protected static readonly Color[] NegativeColors = new Color[] { Color.red, Color.black, new Color(1, 0, 0, 0.5f), new Color(0, 0, 0, 0.5f) };

            /// <summary>
            /// Stable Joints to be used to compute calibration score.
            /// These are the joints for which the computation of the score is the most stable
            /// </summary>
            protected static readonly List<TrackingServiceBodyJointTypes> StableJoints = new List<TrackingServiceBodyJointTypes>
                                                                                        {
                                                                                           TrackingServiceBodyJointTypes.Neck,
                                                                                           TrackingServiceBodyJointTypes.SpineShoulder,
                                                                                           TrackingServiceBodyJointTypes.ShoulderLeft,
                                                                                           TrackingServiceBodyJointTypes.ShoulderRight,
                                                                                           TrackingServiceBodyJointTypes.SpineMid
                                                                                        };

            /// <summary>
            /// Maximum distance of two joints of two different skeletons, to be considered as matched 
            /// (i.e. a master and a raw corrensponding skeletons, that during calibration process should superimpose each other)
            /// </summary>
            protected const float MaxMatchedSkeletonsJointsDistance = 0.1631f;

            /// <summary>
            /// Maximum distance of two joints of two different skeletons, to be considered as perfectly matched. 
            /// Below this threshold, the two joints will be considered as a perfect match
            /// (i.e. a master and a raw corrensponding skeletons, that during calibration process should superimpose each other)
            /// </summary>
            protected const float MinMatchedSkeletonsJointsDistance = 0.0610f;

            /// <summary>
            /// Color of the buttons of the already calibrated data sources
            /// </summary>
            protected static readonly Vector3 calibratedButtonColor = new Vector3(184, 255, 141).normalized;

            #endregion

            #region Private fields

            /// <summary>
            /// Saves the scene providers created by coroutine <see cref="CheckCalibrationQuality"/>.
            /// These have to be disposed at scene end or at coroutine exit.
            /// We need this mechanism because if user exits abruptly from the scene, we have no time to use a cancelation token inside the coroutine body
            /// </summary>
            private List<SceneDataProvider> m_allocatedSceneProviders;

            /// <summary>
            /// True if the user is exiting from this scene using the back button, false otherwise (with the ok button)
            /// </summary>
            private bool m_canceled = false;

            /// <summary>
            /// The Calibration object that contains this object
            /// </summary>
            private Calibration m_enclosingInstance;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="enclosingInstance">Enclosing instance, whose code has to be implemented</param>
            internal CalibrationInternal(Calibration enclosingInstance)
            {
                m_enclosingInstance = enclosingInstance;
            }

            #endregion

            #region Behaviour methods

            internal void Start()
            {
                //set back button behaviour to stop calibration
                ScenesManager.Instance.SetBackButtonBehaviour((obj) =>
                {
                    m_canceled = true;
                    OnOkButtonClicked();
                });

                //register to tracking service events
                TrackingServiceManagerAdvanced.Instance.CalibrationStarted += OnCalibrationStarted;
                TrackingServiceManagerAdvanced.Instance.CalibrationStepPerformed += OnCalibrationStepPerformed;
                TrackingServiceManagerAdvanced.Instance.OperativeStatusStopped += OnOperativeStatusStopped;

                //register to the buttons pressures events
                FindObjectOfType<DataSourcesButtonManager>().ButtonsPressedCallback = OnDataSourceButtonClicked;

                //start tracking at start
                StartCalibrationStep(TrackingService.ControlClient.Model.TrackingServiceCalibrationSteps.Start, string.Empty);

                //allocate scene providers list
                m_allocatedSceneProviders = new List<SceneDataProvider>();

                //we're in waiting stage (waiting for calibration start command to be processed)
                FindObjectOfType<WaitManager>().WaitingState = true;
            }

            internal void OnDestroy()
            {
                if (TrackingServiceManagerAdvanced.Instance != null)
                {
                    m_enclosingInstance.StopAllCoroutines(); //we have only one coroutine and this method works better than StopCoroutine

                    //dispose the scene providers
                    foreach (SceneDataProvider sdp in m_allocatedSceneProviders)
                        if (sdp != null)
                            sdp.Dispose();

                    //unregister to tracking service events
                    TrackingServiceManagerAdvanced.Instance.CalibrationStarted -= OnCalibrationStarted;
                    TrackingServiceManagerAdvanced.Instance.CalibrationStepPerformed -= OnCalibrationStepPerformed;
                    TrackingServiceManagerAdvanced.Instance.OperativeStatusStopped -= OnOperativeStatusStopped;

                    //stop tracking at exit
                    if (TrackingServiceManagerAdvanced.Instance.IsStreamingSkeletons)
                        TrackingServiceManagerAdvanced.Instance.RequestCurrentOperativeStatusStop();
                }
            }

            #endregion

            #region TrackingService Calibration methods

            /// <summary>
            /// Event called when the calibration start operation gets executed
            /// </summary>
            /// <param name="eventArgs">Arguments with result of the operation</param>
            private void OnCalibrationStarted(DataStructures.AdvancedOperationEventArgs eventArgs)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Calibration - Calibration start operation terminated with result {0}", eventArgs.ErrorString ?? "SUCCESS");
                }

                //we're not in waiting stage anymore
                FindObjectOfType<WaitManager>().WaitingState = false;

                if (eventArgs.ErrorString != null)
                    MessageBox.Show("Error", "Can't start calibration: " + eventArgs.ErrorString + ".\nPlease retry to calibrate", new UnityEngine.Events.UnityAction(() => { TrackingServiceManagerAdvanced.Instance.ForceStateToIdle(); ScenesManager.Instance.StopWizard(); }),
                            FindObjectsOfType<Selectable>());
            }

            /// <summary>
            /// Event called when the calibration step start operation gets executed
            /// </summary>
            /// <param name="eventArgs">Arguments with result of the operation</param>
            private void OnCalibrationStepPerformed(DataStructures.AdvancedOperationEventArgs eventArgs)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Calibration - Calibration step start operation terminated with result {0}", eventArgs.ErrorString ?? "SUCCESS");
                }

                //we're not in waiting stage anymore
                FindObjectOfType<WaitManager>().WaitingState = false;

                if (eventArgs.ErrorString != null)
                    MessageBox.Show("Error", "Can't start calibration step: " + eventArgs.ErrorString + ".\nPlease retry to calibrate", new UnityEngine.Events.UnityAction(() => { TrackingServiceManagerAdvanced.Instance.ForceStateToIdle(); ScenesManager.Instance.StopWizard(); }),
                            FindObjectsOfType<Selectable>());
            }

            /// <summary>
            /// Event called when the calibration stop operation gets executed
            /// </summary>
            /// <param name="eventArgs">Arguments with result of the operation</param>
            private void OnOperativeStatusStopped(DataStructures.AdvancedOperationEventArgs eventArgs)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Calibration - Calibration stop operation terminated with result {0}", eventArgs.ErrorString ?? "SUCCESS");
                }

                //we're not in waiting stage anymore
                FindObjectOfType<WaitManager>().WaitingState = false;

                if (eventArgs.ErrorString != null)
                    MessageBox.Show("Error", "Can't stop calibration: " + eventArgs.ErrorString + ".\nThe system may be in an unknown state", new UnityEngine.Events.UnityAction(() => { TrackingServiceManagerAdvanced.Instance.ForceStateToIdle(); ScenesManager.Instance.StopWizard(); }),
                            FindObjectsOfType<Selectable>());
                else if (!m_canceled)
                    //if everything went well, return to main menu or go to merging scene (if were during a wizard)
                    ScenesManager.Instance.PopOrNextInWizard("Merging");
                else
                    //if user pressed back button, return to previous scene
                    ScenesManager.Instance.PopScene();
            }

            /// <summary>
            /// Helper methods that fills all data necessary to perform a calibration step and then calls the Tracking Service Manager Advanced
            /// to start the operation
            /// </summary>
            /// <param name="calibrationStep">Requested calibration step</param>
            /// <param name="dataSourceID">ID of the data source that has to be calibrate (String.empty if this param is not necessary)</param>
            private void StartCalibrationStep(TrackingService.ControlClient.Model.TrackingServiceCalibrationSteps calibrationStep, string dataSourceID)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Calibration - Starting calibration step", calibrationStep.ToString());
                }

                //we're in waiting stage (waiting for the command to be processed by the underlying tracking service)
                FindObjectOfType<WaitManager>().WaitingState = true;

                //fill the data with the required step and data source id and with public unity params
                CalibrationParameters calibParameters = new CalibrationParameters
                {
                    Step = calibrationStep,
                    CalibrateSlavesUsingCentroids = m_enclosingInstance.CalibrateSlavesUsingCentroids,
                    CalibratingUserHeight = m_enclosingInstance.CalibratingUserHeight,
                    AdditionalMasterYRotation = m_enclosingInstance.AdditionalMasterYRotation,
                    LastButNthValidMatrix = m_enclosingInstance.LastButNthValidMatrix,
                    DataSource1 = dataSourceID
                };

                //performs the request to the manager
                if (calibrationStep == TrackingService.ControlClient.Model.TrackingServiceCalibrationSteps.Start)
                    TrackingServiceManagerAdvanced.Instance.RequestCalibrationStart(calibParameters);
                else
                    TrackingServiceManagerAdvanced.Instance.RequestCalibrationStepStart(calibParameters);
            }

            #endregion

            #region Buttons events methods

            /// <summary>
            /// Callback executed when one of the data sources buttons of the screen gets pressed
            /// </summary>
            /// <param name="dataSourceLabel">Label of the data source button pressed</param>
            /// <param name="newStatus">New status of the button</param>
            internal void OnDataSourceButtonClicked(string dataSourceLabel, bool newStatus)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Calibration - Calibration for {0} is now {1}", dataSourceLabel, newStatus ? "ON" : "OFF");
                }

                //check if this is the master data source or not and if we are starting or stopping the tracking and then start performing the calibration step required
                string masterLabel = FindObjectOfType<DataSourcesButtonManager>().MasterButtonLabel;
                bool isMaster = dataSourceLabel == masterLabel;

                if (isMaster)
                    StartCalibrationStep(newStatus ? TrackingServiceCalibrationSteps.StartCalibrateMaster : TrackingServiceCalibrationSteps.StopCalibrateMaster, dataSourceLabel);
                else
                    StartCalibrationStep(newStatus ? TrackingServiceCalibrationSteps.StartCalibrateDataSourceWithMaster : TrackingServiceCalibrationSteps.StopCalibrateDataSourceWithMaster, dataSourceLabel);

                //if data source is non-master
                if (!isMaster)
                {
                    //if we are activating it
                    if (newStatus == true)
                    {
                        //show master raw stream + slave raw stream & slave master transform stream
                        AddSkeletalVisualizer(dataSourceLabel, TrackingServiceSceneDataStreamModes.Raw, false);
                        AddSkeletalVisualizer(dataSourceLabel, TrackingServiceSceneDataStreamModes.MasterTransform, false);
                        AddSkeletalVisualizer(masterLabel, TrackingServiceSceneDataStreamModes.Raw, true);
                    }
                    //else, if we are removing it
                    else
                    {
                        //destroy all
                        RemoveSkeletalVisualizer(dataSourceLabel, TrackingServiceSceneDataStreamModes.Raw);
                        RemoveSkeletalVisualizer(dataSourceLabel, TrackingServiceSceneDataStreamModes.MasterTransform);
                        RemoveSkeletalVisualizer(masterLabel, TrackingServiceSceneDataStreamModes.Raw);
                    }
                }
                //else, if it is master
                else
                {
                    //if we are activating it
                    if (newStatus == true)
                    {
                        //show master raw stream + master world transform stream
                        AddSkeletalVisualizer(masterLabel, TrackingServiceSceneDataStreamModes.Raw, true);
                        AddSkeletalVisualizer(masterLabel, TrackingServiceSceneDataStreamModes.WorldTransform, true);
                    }
                    //else, if we are removing it
                    else
                    {
                        //destroy all
                        RemoveSkeletalVisualizer(masterLabel, TrackingServiceSceneDataStreamModes.Raw);
                        RemoveSkeletalVisualizer(masterLabel, TrackingServiceSceneDataStreamModes.WorldTransform);
                    }
                }

                //show or hide the calibration quality UI element, depending on if we are starting or stopping a calibration step.
                //start or stop the coroutine that calculate the calibration quality to be shown to the user
                m_enclosingInstance.CalibrationQualityPanel.SetActive(newStatus);

                if (newStatus == true)
                {
                    m_enclosingInstance.StartCoroutine(CheckCalibrationQuality(dataSourceLabel, masterLabel, isMaster));
                }
                else
                {
                    m_enclosingInstance.StopAllCoroutines(); //we have only one coroutine and this method works better than StopCoroutine

                    //don't forget to dispose the scene providers allocated by the coroutine
                    foreach (SceneDataProvider sdp in m_allocatedSceneProviders)
                        if (sdp != null)
                            sdp.Dispose();

                    m_allocatedSceneProviders.Clear();

                    //the data source is now calibrated... set its button to green
                    Toggle dsToggle = FindObjectOfType<DataSourcesButtonManager>()[dataSourceLabel].GetComponent<Toggle>();
                    var colors = dsToggle.colors; //the .colors property values can't be changed, so we have to do this workaround
                    colors.normalColor = colors.highlightedColor = new Color(calibratedButtonColor.x, calibratedButtonColor.y, calibratedButtonColor.z);
                    dsToggle.colors = colors;
                }

            }

            /// <summary>
            /// Triggered when the OK button gets clicked
            /// </summary>
            internal void OnOkButtonClicked()
            {
                //stop calibration
                if (TrackingServiceManagerAdvanced.Instance.IsStreamingSkeletons)
                {
                    //we're in waiting stage (waiting for the command to be processed by the underlying tracking service)
                    FindObjectOfType<WaitManager>().WaitingState = true;

                    TrackingServiceManagerAdvanced.Instance.RequestCurrentOperativeStatusStop();
                }
            }

            #endregion

            #region Calibration Quality Management

            /// <summary>
            /// Coroutine that calculates calibration quality at each frame, reading data from the skeletons of calibrating data sources
            /// </summary>
            /// <param name="slaveId">Slave data source ID of the calibrating data source. Parameter not used for master calibration</param>
            /// <param name="masterId">Master data source ID</param>
            /// <param name="isMaster">True if we're performing master calibration, false otherwise</param>
            /// <returns></returns>
            private IEnumerator CheckCalibrationQuality(string slaveId, string masterId, bool isMaster)
            {
                //init the calibration quality rectangle to red
                m_enclosingInstance.CalibrationQualityPanel.GetComponent<Image>().color = Color.red;

                //if we are calibrating master data source, show bad calibration until some skeletons appear on the world transformed data source
                //that are different from the raw ones
                if (isMaster)
                {
                    //open the data streams: master raw and world transform
                    SceneDataProvider sdpRaw = TrackingServiceManagerAdvanced.Instance.StartSceneDataProvider(masterId, TrackingServiceSceneDataStreamModes.Raw);
                    SceneDataProvider sdpWorld = TrackingServiceManagerAdvanced.Instance.StartSceneDataProvider(masterId, TrackingServiceSceneDataStreamModes.WorldTransform);

                    if (sdpRaw == null || sdpWorld == null)
                        yield break;

                    //add to the list to be cleared at co-routine exit
                    m_allocatedSceneProviders.Add(sdpRaw);
                    m_allocatedSceneProviders.Add(sdpWorld);

                    //use a 2-step check, because sometimes, when fast movement happens, the two seen skeletons are different for one frame
                    //(maybe because of non-synchronization, one of the provider goes to the following frame). It never happens that this 
                    //glitch happen for two consecutive frames
                    bool lastFrameCheck = false;

                    //while forever
                    while (sdpRaw.IsStillValid && sdpWorld.IsStillValid)
                    {
                        //wait for the next iteration
                        //(this serves also to give the Scene Data Provider time to read the first skeletons
                        yield return new WaitForSeconds(0.1f);

                        //take the first skeleton in the raw stream, if any... and find if the first body in the world transform
                        // has not the same coordinate for the spine mid. If this is different, everything has been calibrated, so exit the loop
                        //Notice that we can use this method because:
                        //- we are using only one data source, so we are sure we have exactly the same skeletons in both streams, in the same order
                        //- while the master is not calibrated, all body streams are identical, so the check on a single joint like spine mid serves for the purpose
                        //Remember the above comment: we use double check because of some issues we have on single frames
                        if (sdpRaw.LastBodies.Count > 0 && sdpWorld.LastBodies.Count > 0)
                        {                            
                            if (Vector3.Distance(sdpRaw.LastBodies[0].Joints[TrackingServiceBodyJointTypes.SpineMid].ToVector3(),
                                                 sdpWorld.LastBodies[0].Joints[TrackingServiceBodyJointTypes.SpineMid].ToVector3()) > 0.04f)
                            {
                                if (lastFrameCheck)
                                    break;
                                else
                                    lastFrameCheck = true;
                            }
                            else
                                lastFrameCheck = false;
                        }
                    }

                    //we are calibrated. Set color to green, make the phone vibrate, dispose everything and exit
                    m_enclosingInstance.CalibrationQualityPanel.GetComponent<Image>().color = Color.green;

                    if (m_enclosingInstance.DeviceVibrationProvider != null)
                        m_enclosingInstance.DeviceVibrationProvider.Vibrate();

                    yield return new WaitForSeconds(1.3f); //to make the user see that everything is alright
                    sdpRaw.Dispose();
                    sdpWorld.Dispose();
                    m_enclosingInstance.GetComponent<ToggleGroup>().SetAllTogglesOff(); //make the button return to off

                    yield break;
                }
                //else, if we are calibrating a slave to the master, we have to check that certain key joints are becoming very close
                //in the master transform of the slave data source wrt the raw stream of the master data source
                else
                {
                    //open the data streams: master raw and slave master transform
                    SceneDataProvider sdpMasterRaw = TrackingServiceManagerAdvanced.Instance.StartSceneDataProvider(masterId, TrackingServiceSceneDataStreamModes.Raw);
                    SceneDataProvider sdpSlaveMasterTransform = TrackingServiceManagerAdvanced.Instance.StartSceneDataProvider(slaveId, TrackingServiceSceneDataStreamModes.MasterTransform);

                    if (sdpMasterRaw == null || sdpSlaveMasterTransform == null)
                        yield break;

                    //add to the list to be cleared at co-routine exit
                    m_allocatedSceneProviders.Add(sdpMasterRaw);
                    m_allocatedSceneProviders.Add(sdpSlaveMasterTransform);

                    //init distance to 1 (maximum distance, bodies too far)
                    float distance = 1;

                    //init auto stop timer to zero
                    float autoStopTimer = 0;

                    //while forever
                    while (sdpMasterRaw.IsStillValid && sdpSlaveMasterTransform.IsStillValid)
                    {
                        //wait for some time... don't update every frame
                        //(this serves also to give the Scene Data Provider time to read the first skeletons
                        yield return new WaitForSeconds(0.1f);

                        //if there are bodies in the two streams
                        if (sdpMasterRaw.LastBodies.Count > 0 && sdpSlaveMasterTransform.LastBodies.Count > 0)
                        {
                            //for each skeleton in the slave master transform
                            foreach (TrackingServiceBodyData slaveBody in sdpSlaveMasterTransform.LastBodies)
                            {
                                //TODO: IN TEORIA DOVREI SAPERE DA SOTTO CHE BODY SI STA CALIBRANDO, SENNO' ALTRI BODIES POSSONO DARMI FASTIDIO, PERCHE' MAGARI SONO IN POSE STRANE

                                //find the nearest body in the master stream and compute how distant they are.
                                //we base the calculations using a single body, so if we find two bodies (master raw and slave master-transformed) that are near
                                //enough, just take that distance and exit
                                if (FindNearestBodyDistance(slaveBody, sdpMasterRaw.LastBodies, out distance))
                                {
                                    break;
                                }
                                //if we don't find a body near to this one, just set too much distance
                                else
                                    distance = 1;
                            }
                        }
                        //no valid bodies in one or more streams, just set too much distance
                        else
                            distance = 1;

                        //compute a score of similarliness of this 2 skeletons, in the range [0, 1],
                        //using computed distance and constants of euristhics of valid range of distances
                        float finalScore = distance;

                        finalScore = 1 - (finalScore - MinMatchedSkeletonsJointsDistance) / (MaxMatchedSkeletonsJointsDistance - MinMatchedSkeletonsJointsDistance);
                        finalScore = Mathf.Clamp(finalScore, 0, 1);

                        //if final score is below autostep threshold, reset the timer
                        if (finalScore < m_enclosingInstance.CalibrationAutoStopScoreThreshold)
                            autoStopTimer = 0;
                        //else, if it is above it, increment the auto stop timer counter. If the counter gets above the time threshold, stop the calibration
                        else
                        {
                            autoStopTimer += 0.1f; //remember that we don't use delta time, because we sleep 0.1 sec every loop iteration

                            if (autoStopTimer >= m_enclosingInstance.CalibrationAutoStopTime)
                            {
                                //make the phone vibrate

                                if (m_enclosingInstance.DeviceVibrationProvider != null)
                                    m_enclosingInstance.DeviceVibrationProvider.Vibrate();

                                //reset toggles and exit from this coroutine
                                yield return new WaitForSeconds(0.7f); //to make the user see that everything is alright

                                m_enclosingInstance.GetComponent<ToggleGroup>().SetAllTogglesOff(); //the cleanup will be performed by the handler of the toggle off
                                yield break;
                            }
                        }

                        //set color of the quality according to the found score
                        m_enclosingInstance.CalibrationQualityPanel.GetComponent<Image>().color = new Color(1 - finalScore, finalScore, 0.01f);
                    }

                }

            }

            /// <summary>
            /// Given a body in the slave master-transformed stream, finds the nearest body in the master raw stream and return their
            /// mean distance.
            /// Distances are performed using stable joints
            /// </summary>
            /// <param name="slaveBody">Body inside the slave master-transformed stream that has to be matched</param>
            /// <param name="masterBodies">List of bodies found in the master raw stream</param>
            /// <param name="distance">Out parameter that will receive the mean distance between the slave body and the nearest master body</param>
            /// <returns>True if a valid body has been found, false if all master bodies are too distant from the provided one</returns>
            private bool FindNearestBodyDistance(TrackingServiceBodyData slaveBody, IList<TrackingServiceBodyData> masterBodies, out float distance)
            {
                //set min distance to the max allowed valid distance between joints
                //and nearest body found in the master stream to null
                float minDistance = MaxMatchedSkeletonsJointsDistance;
                TrackingServiceBodyData nearestBody = null;

                //foreach master body in the master raw stream
                foreach (TrackingServiceBodyData masterBody in masterBodies)
                {
                    //loop for all the joint types that are considered stable, and, among all stable joints that have valid tracking in both
                    //skeletons (confidence above threshold), take the one with the greatest distance

                    float sumDistance = 0;
                    int numValidJoints = 0;

                    foreach (TrackingServiceBodyJointTypes stableJointType in StableJoints)
                    {
                        if (slaveBody.Joints[stableJointType].Confidence > 0.5f && masterBody.Joints[stableJointType].Confidence > 0.5f)
                        {
                            sumDistance = Mathf.Max(sumDistance, Vector3.Distance(slaveBody.Joints[stableJointType].ToVector3(), masterBody.Joints[stableJointType].ToVector3()));
                            numValidJoints++;
                        }

                    }

                    //if at least 2 joint have been considered in the previous loop
                    //(we check this, to avoid to take confidence data from a skeleton with noone or only one correctly tracked joint)
                    if (numValidJoints >= 2)
                    {
                        //if this distance is below the current min distance, save current data
                        if (sumDistance < minDistance)
                        {
                            minDistance = sumDistance;
                            nearestBody = masterBody;
                        }
                    }

                }

                //if we have found a body in the master raw stream that is quite close to the slave master-trasformed one,
                //return its score
                if (nearestBody != null)
                {
                    distance = minDistance;
                    return true;
                }
                //else, if we have found not, return failure
                else
                {
                    distance = 1;
                    return false;
                }

            }

            #endregion

            #region Skeletons Visualization Management

            /// <summary>
            /// Create a unique name for a body visualizer that has to show the skeletons as read from a particular data source
            /// </summary>
            /// <param name="dataSourceLabel">Name of the data source</param>
            /// <param name="streamingMode">Type of skeletons to show (e.g. raw vs world transform)</param>
            /// <returns>Creates a uniqe name for a body visualizer</returns>
            private string CreateSkeletalVisualizerName(string dataSourceLabel, TrackingServiceSceneDataStreamModes streamingMode)
            {
                return string.Format("{0}_{1}", dataSourceLabel, streamingMode);
            }

            /// <summary>
            /// Returns if the calibration manager is already showing a particular type of skeletons
            /// </summary>
            /// <param name="dataSourceLabel">Name of the data source</param>
            /// <param name="streamingMode">Type of skeletons to show (e.g. raw vs world transform)</param>
            /// <returns>True if the object is already present, false otherwise</returns>
            private bool HasSkeletalVisualizer(string dataSourceLabel, TrackingServiceSceneDataStreamModes streamingMode)
            {
                return m_enclosingInstance.transform.Find(CreateSkeletalVisualizerName(dataSourceLabel, streamingMode)) != null;
            }

            /// <summary>
            /// Adds a body visualizer that has to show the skeletons as read from a particular data source.
            /// If this object already exists, the method does nothing
            /// </summary>
            /// <param name="dataSourceLabel">Name of the data source</param>
            /// <param name="streamingMode">Type of skeletons to show (e.g. raw vs world transform)</param>
            /// <param name="isMaster">If the data source is master; false otherwise</param>
            private void AddSkeletalVisualizer(string dataSourceLabel, TrackingServiceSceneDataStreamModes streamingMode, bool isMaster)
            {
                if (HasSkeletalVisualizer(dataSourceLabel, streamingMode))
                    return;

                //create a skeletal visualizer
                GameObject bodiesVisualizerGo = Instantiate<GameObject>(m_enclosingInstance.BodyDrawerPrefab.gameObject);
                BodiesSkeletalsManagerAdvanced bodiesSkeletalsManagerAdv = bodiesVisualizerGo.GetComponent<BodiesSkeletalsManagerAdvanced>();
                bodiesSkeletalsManagerAdv.SceneStreamerInfoId = dataSourceLabel;
                bodiesSkeletalsManagerAdv.SceneStreamingMode = streamingMode;
                bodiesSkeletalsManagerAdv.SkeletalDrawingMode = Avateering.Skeletals.SkeletalsDrawingMode.FixedColors;
                //choose the appropriate colors
                int colorIdx = isMaster ? 1 : 0;
                //if this is the raw stream of a non-master data source, draw it more transparent, so it clutters less the scene
                if (!isMaster && streamingMode == TrackingServiceSceneDataStreamModes.Raw)
                {
                    colorIdx += 2;
                    bodiesSkeletalsManagerAdv.LimbsColor = new Color(0, 0, 0, 0.5f);
                }
                else
                    bodiesSkeletalsManagerAdv.LimbsColor = new Color(0, 0, 0, 1.0f);
                bodiesSkeletalsManagerAdv.PositiveColor = PositiveColors[colorIdx];
                bodiesSkeletalsManagerAdv.NegativeColor = NegativeColors[colorIdx];
                bodiesSkeletalsManagerAdv.JointsMaterial = m_enclosingInstance.SkeletalJointsMaterial;
                bodiesSkeletalsManagerAdv.LimbsMaterial = m_enclosingInstance.SkeletalBonesMaterial;
                bodiesVisualizerGo.name = CreateSkeletalVisualizerName(dataSourceLabel, streamingMode);
                bodiesVisualizerGo.transform.SetParent(m_enclosingInstance.transform, false);
                bodiesVisualizerGo.SetActive(true);
            }

            /// <summary>
            /// Removes a body visualizer that shows the skeletons as read from a particular data source.
            /// If this object does not exists, the method does nothing
            /// </summary>
            /// <param name="dataSourceLabel">Name of the data source</param>
            /// <param name="streamingMode">Type of skeletons to show (e.g. raw vs world transform)</param>
            private void RemoveSkeletalVisualizer(string dataSourceLabel, TrackingServiceSceneDataStreamModes streamingMode)
            {
                if (HasSkeletalVisualizer(dataSourceLabel, streamingMode))
                    Destroy(m_enclosingInstance.transform.Find(CreateSkeletalVisualizerName(dataSourceLabel, streamingMode)).gameObject);
            }

            #endregion
        }
    }
}

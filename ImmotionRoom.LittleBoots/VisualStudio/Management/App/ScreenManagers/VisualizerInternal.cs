namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.ScreenManagers
{
    using UnityEngine;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedManager;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using UnityEngine.UI;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedAvateering;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.DataSourcesManagement;
    using ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Tools;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.App.DataSourcesManagement;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils.MessageBoxes;

    /// <summary>
    /// Manages Visualizer scene behaviour
    /// </summary>
    public partial class Visualizer : MonoBehaviour
    {
        /// <summary>
        /// Contains the actual definition of the Visualizer, for obfuscation purposes
        /// </summary>
        private class VisualizerInternal
        {
            #region Private fields

            /// <summary>
            /// Private counter of the total number of created visualizers
            /// </summary>
            private int m_createdVisualizers = 0;

            /// <summary>
            /// True if the user is exiting from this scene using the back button, false otherwise (with the ok button)
            /// </summary>
            private bool m_canceled = false;

            /// <summary>
            /// The Visualizer object that contains this object
            /// </summary>
            private Visualizer m_enclosingInstance;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="enclosingInstance">Enclosing instance, whose code has to be implemented</param>
            internal VisualizerInternal(Visualizer enclosingInstance)
            {
                m_enclosingInstance = enclosingInstance;
            }

            #endregion

            #region Behaviour methods

            internal void Start()
            {
                //set back button behaviour to stop diagnostic mode
                ScenesManager.Instance.SetBackButtonBehaviour((obj) =>
                {
                    m_canceled = true;
                    OnOkButtonClicked();
                });

                //clear all the list of existing visualization panels
                foreach (Transform t in m_enclosingInstance.VisualizationPanel.transform)
                    if (t.GetInstanceID() != m_enclosingInstance.VisualizationPanel.transform.GetInstanceID())
                        Destroy(t.gameObject);

                //register to tracking service events
                TrackingServiceManagerAdvanced.Instance.DiagnosticStarted += OnDiagnosticModeStarted;
                TrackingServiceManagerAdvanced.Instance.OperativeStatusStopped += OnOperativeStatusStopped;

                //register to the buttons pressures events
                FindObjectOfType<DataSourcesButtonManager>().ButtonsPressedCallback = OnDataSourceToggleValueChanged;

                //start tracking at start
                TrackingServiceManagerAdvanced.Instance.RequestDiagnosticModeStart();

                //we're in waiting stage (waiting for the command to be processed by the underlying tracking service)
                FindObjectOfType<WaitManager>().WaitingState = true;
            }

            internal void OnDestroy()
            {
                if (TrackingServiceManagerAdvanced.Instance != null)
                {
                    //unregister to tracking service events
                    TrackingServiceManagerAdvanced.Instance.DiagnosticStarted -= OnDiagnosticModeStarted;
                    TrackingServiceManagerAdvanced.Instance.OperativeStatusStopped -= OnOperativeStatusStopped;

                    //stop tracking at exit
                    if (TrackingServiceManagerAdvanced.Instance.IsStreamingSkeletons)
                        TrackingServiceManagerAdvanced.Instance.RequestCurrentOperativeStatusStop();
                }
            }

            #endregion

            #region TrackingService Tracking Methods

            /// <summary>
            /// Event called when the tracking start operation gets executed
            /// </summary>
            /// <param name="eventArgs">Arguments with result of the operation</param>
            private void OnDiagnosticModeStarted(DataStructures.AdvancedOperationEventArgs eventArgs)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Visualizer - Diagnostic start operation terminated with result {0}", eventArgs.ErrorString ?? "SUCCESS");
                }

                //we're not in waiting stage anymore (command has been processed by the underlying tracking service)
                FindObjectOfType<WaitManager>().WaitingState = false;

                if (eventArgs.ErrorString != null)
                    MessageBox.Show("Error", "Can't start diagnostic mode: " + eventArgs.ErrorString + ".\nPlease retry again later", new UnityEngine.Events.UnityAction(() => { TrackingServiceManagerAdvanced.Instance.ForceStateToIdle(); ScenesManager.Instance.StopWizard(); }),
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
                    Log.Debug("Visualizer - Diagnostic stop operation terminated with result {0}", eventArgs.ErrorString ?? "SUCCESS");
                }

                //we're not in waiting stage anymore (command has been processed by the underlying tracking service)
                FindObjectOfType<WaitManager>().WaitingState = false;

                if (eventArgs.ErrorString != null)
                    MessageBox.Show("Error", "Can't stop diagnostic mode: " + eventArgs.ErrorString + ".\nThe system may be in an unknown state", new UnityEngine.Events.UnityAction(() => { TrackingServiceManagerAdvanced.Instance.ForceStateToIdle(); ScenesManager.Instance.StopWizard(); }),
                            FindObjectsOfType<Selectable>());
                else if (!m_canceled)
                    ScenesManager.Instance.PopOrNextInWizard("Calibration");
                else
                    //if user pressed back button, return to previous scene
                    ScenesManager.Instance.PopScene();
            }

            #endregion

            #region Buttons events methods

            /// <summary>
            /// Triggered when the OK button gets clicked
            /// </summary>
            internal void OnOkButtonClicked()
            {
                //stop diagnostic mode
                if (TrackingServiceManagerAdvanced.Instance.IsStreamingSkeletons)
                {
                    //we're in waiting stage (waiting for the command to be processed by the underlying tracking service)
                    FindObjectOfType<WaitManager>().WaitingState = true;

                    TrackingServiceManagerAdvanced.Instance.RequestCurrentOperativeStatusStop();
                }
            }

            /// <summary>
            /// Callback called each time one of the toggle button of the data sources changes its status
            /// </summary>
            /// <param name="dataSourceName">Label of the toggle button (corresponds to a Data Source name)</param>
            /// <param name="newStatus">New status of the toggle</param>
            internal void OnDataSourceToggleValueChanged(string dataSourceName, bool newStatus)
            {
                //if the button has been pressed, we have to add the visualization of a new data source
                if (newStatus)
                {
                    //create a new panel to show the skeletons from this data source (use the prefab because it already contains all
                    //the necessary stuff for the UI)
                    GameObject newPanel = Instantiate(m_enclosingInstance.CameraChildPanel);

                    //set it as a child of the visualization root and assign the name corresponding to this datasource
                    newPanel.transform.SetParent(m_enclosingInstance.VisualizationPanel.transform, false);
                    newPanel.name = dataSourceName;
                    newPanel.transform.Find("Name").GetComponent<Text>().text = dataSourceName;

                    //for UI reasons, change parameters of the grid layout to keep the various visualizations square-ish
                    //(otherwise, keeping the number of rows and column fixed, continuing adding data sources would make all the visualization panels
                    //too stretched)
                    AdjustGridLayout(false);

                    //add the camera and the skeleton drawer for this data source and fix all related stuff
                    m_enclosingInstance.StartCoroutine(AddCameraAndSkeletalVisualizer(dataSourceName));

                    //we're in waiting stage (waiting for the visualization stuff to initialize)
                    FindObjectOfType<WaitManager>().WaitingState = true;
                }
                else
                {
                    //remove the data source panel, if any
                    if (m_enclosingInstance.VisualizationPanel.transform.Find(dataSourceName))
                        Destroy(m_enclosingInstance.VisualizationPanel.transform.Find(dataSourceName).gameObject);

                    //for UI reasons, change parameters of the grid layout to keep the various visualizations square-ish
                    //(otherwise, keeping the number of rows and column fixed, removing and adding data sources would make all the visualization panels
                    //too stretched)
                    AdjustGridLayout(true);

                    //remove the camera and the skeleton drawer for this data source and fix all related stuff
                    m_enclosingInstance.StartCoroutine(RemoveCameraAndSkeletalVisualizer(dataSourceName));

                    //we're in waiting stage (waiting for the visualization stuff to initialize)
                    FindObjectOfType<WaitManager>().WaitingState = true;
                }
            }

            #endregion

            #region Private methods

            /// <summary>
            /// Adjusts the grid layout of the visualization panels, after the number of the visualization panels has changed
            /// </summary>
            /// <param param name="isRemoving">True if we are adjusting the layout because we're removing an element, false otherwise</param>
            private void AdjustGridLayout(bool isRemoving)
            {
                //if we're removing elements, it is likely that the Destroy has not been applied yet, so childCount returns 1 child more that we have to subtract)
                int children = m_enclosingInstance.VisualizationPanel.transform.childCount - (isRemoving ? 1 : 0);

                //So, if we have 1 element, we have 1 column (and 1 row)
                //for 2 element, we have 2 columns (and 1 row)
                //for 3, 4, 5, 6, element we have 3 columns... (and two rows)
                //for 7, 8 element we have 4 columns (and two rows)
                if (children <= 1)
                    m_enclosingInstance.VisualizationPanel.Columns = 1;
                else if (children <= 2)
                    m_enclosingInstance.VisualizationPanel.Columns = 2;
                else if (children <= 6)
                    m_enclosingInstance.VisualizationPanel.Columns = 3;
                else if (children <= 14)
                    m_enclosingInstance.VisualizationPanel.Columns = Mathf.CeilToInt(children / 2.0f); //generic two rows case
                else if (children <= 28)
                    m_enclosingInstance.VisualizationPanel.Columns = Mathf.CeilToInt(children / 3.0f); //three rows
                else
                    m_enclosingInstance.VisualizationPanel.Columns = Mathf.CeilToInt(children / 4.0f); //four rows
            }

            /// <summary>
            /// Adds a body visualizer that has to show the skeletons as read from a particular data source.
            /// If this object already exists, the method does nothing
            /// </summary>
            /// <param name="dataSourceLabel">Name of the data source</param>
            private IEnumerator AddCameraAndSkeletalVisualizer(string dataSourceLabel)
            {
                //if it already exists, return
                if (m_enclosingInstance.transform.Find(dataSourceLabel))
                    yield break;

                //find the UI panel corresponding to this data source (it has been added by the companion method)
                GameObject cameraVisualizationPanel = m_enclosingInstance.VisualizationPanel.transform.Find(dataSourceLabel).gameObject;

                //create the parent gameobject, with the name of the data source
                GameObject parentGo = new GameObject(dataSourceLabel);
                parentGo.transform.SetParent(m_enclosingInstance.transform, false);

                //create a skeletal visualizer for that data source (raw stream)
                GameObject bodiesVisualizerGo = Instantiate<GameObject>(m_enclosingInstance.BodyDrawerPrefab.gameObject);
                BodiesSkeletalsManagerAdvanced bodiesSkeletalsManagerAdv = bodiesVisualizerGo.GetComponent<BodiesSkeletalsManagerAdvanced>();
                bodiesSkeletalsManagerAdv.SceneStreamerInfoId = dataSourceLabel;
                bodiesSkeletalsManagerAdv.SceneStreamingMode = TrackingService.ControlClient.Model.TrackingServiceSceneDataStreamModes.Raw;
                bodiesSkeletalsManagerAdv.SkeletalDrawingMode = Avateering.Skeletals.SkeletalsDrawingMode.Standard;
                bodiesSkeletalsManagerAdv.JointsMaterial = m_enclosingInstance.SkeletalJointsMaterial;
                bodiesSkeletalsManagerAdv.LimbsMaterial = m_enclosingInstance.SkeletalBonesMaterial;
                bodiesSkeletalsManagerAdv.RedAlerts = new GameObject[] { cameraVisualizationPanel.transform.GetChild(0).gameObject, cameraVisualizationPanel.transform.GetChild(1).gameObject, cameraVisualizationPanel.transform.GetChild(2).gameObject, cameraVisualizationPanel.transform.GetChild(3).gameObject };
                bodiesVisualizerGo.name = "Bodies Manager";
                bodiesVisualizerGo.transform.SetParent(parentGo.transform, false);
                bodiesVisualizerGo.transform.localPosition = new Vector3(0, 0, m_createdVisualizers * 25); //set a distance from one visualizer to the others
                bodiesVisualizerGo.transform.localScale = new Vector3(-1, 1, 1); //to flip the skeletons, so that they appear like in a mirror... so it is more useful for the user using the app
                bodiesVisualizerGo.SetActive(true);

                //get a scene data provider to see the skeleton seen by this data source
                SceneDataProvider sdp = TrackingServiceManagerAdvanced.Instance.StartSceneDataProvider(dataSourceLabel, TrackingService.ControlClient.Model.TrackingServiceSceneDataStreamModes.Raw);

                if (sdp == null)
                    yield break;

                //wait until the gridlayout updates itself: this is necessary because we have added a new panel visualization for the new data source
                //(in the method OnDataSourceToggleValueChanged), but that panel has still not the correct position into the screen until the AutoGridLayout
                //behaviour will not update itself.
                //Notice that we don't wait for the end of frame but for some time because StartSceneDataProvider on Android takes a little to initialize itself
                yield return new WaitForSeconds(0.2f);

                //ok, now we'll calculate some euristhics on the skeletons seen at this frame by the data source.
                //We'll calculate the overall baricenter of the skeletons, the baricenter of all heads and the baricenter of all the spine-mids
                //(code is self-explanatory)
                Vector3 jointBaricenter = Vector3.zero;
                int jointCount = 0;
                Vector3 headsBaricenter = Vector3.zero, spineMidBaricenter = Vector3.zero;
                int headsSpineCount = 0;
                int trials = 9;

                //try for 9 times to find a body (that is, a second. If you don't find it in one second, ignore everything)
                while (trials-- > 0)
                {
                    //if you find it
                    if (sdp.LastBodies != null && sdp.LastBodies.Count > 0)
                    {
                        //do the initialization using euristhics
                        foreach (TrackingServiceBodyData body in sdp.LastBodies)
                            foreach (var jointPair in body.Joints)
                            {
                                jointBaricenter += jointPair.Value.ToVector3();
                                jointCount++;

                                if (jointPair.Key == TrackingServiceBodyJointTypes.Head)
                                {
                                    headsBaricenter += jointPair.Value.ToVector3();
                                    headsSpineCount++;
                                }
                                else if (jointPair.Key == TrackingServiceBodyJointTypes.SpineMid)
                                {
                                    spineMidBaricenter += jointPair.Value.ToVector3();
                                    headsSpineCount++;
                                }

                            }

                        jointBaricenter /= jointCount;
                        headsBaricenter /= (headsSpineCount / 2);
                        spineMidBaricenter /= (headsSpineCount / 2);

                        //exit the loop
                        break;
                    }
                    //else, if there are no bodies, wait a bit and then try again
                    else
                        yield return new WaitForSeconds(0.11f);
                }

                //free the provider
                sdp.Dispose();

                //create a ortho camera, with a script to move the camera.
                //Set camera data to show optimally the skeletons in the scene:
                //- set position at the overall joints baricenter.
                //- set initial ortographic size as the mean height of the skeletons (that is the double of the distance of the head from the spine mid... plus some tolerance)
                GameObject cameraGo = new GameObject("Camera");
                cameraGo.transform.SetParent(parentGo.transform, false);
                cameraGo.transform.localPosition = new Vector3(jointBaricenter.x, jointBaricenter.y, m_createdVisualizers * 25); //set a distance from one visualizer to the others, on the Z plane
                cameraGo.transform.localRotation = Quaternion.AngleAxis(180, Vector3.up);
                Camera camera = cameraGo.AddComponent<Camera>();
                camera.backgroundColor = new Color(0, 0.5f, 1.0f); //ImmotionAR azure color
                camera.orthographic = true;
                camera.orthographicSize = Mathf.Max(0.5f, (headsBaricenter.y - spineMidBaricenter.y) * 2.31f); //use a minimum value of 0.5f to not make the camera zoom to much (and remember that the other value is 0 if we have no skeleton).
                camera.farClipPlane = 25; //so one visualizer will not see what the others see

                //ok, now we have all the stuff made, but we have still to make one thing: each visualizer will only have a portion on
                //the screen onto which it can draw... and after the addition of each visualizer, it is possible that every existing
                //visualizer has changed its dedicated screen area (because of this addition, maybe all the areas have been shrinked),
                //so, re-update the screen layout of the data sources visualization, to take in count of this removal
                AdjustCameraViewports();

                //add the script to move the camera using the fingers (assigning the correct screen rect of competence
                RectTransform cameraVisualizationPanelRectTransform = cameraVisualizationPanel.GetComponent<RectTransform>();
                MoveCameraWithFingers moveCamera = cameraGo.AddComponent<MoveCameraWithFingers>();
                moveCamera.CameraControlRectangle = cameraVisualizationPanelRectTransform;

                //increment the number of visualizers created
                m_createdVisualizers++;

                //we're not in waiting stage anymore (visualization stuff has been initialized)
                FindObjectOfType<WaitManager>().WaitingState = false;

                yield break;
            }

            /// <summary>
            /// Adds a body visualizer that has to show the skeletons as read from a particular data source.
            /// If this object already exists, the method does nothing
            /// </summary>
            /// <param name="dataSourceLabel">Name of the data source</param>
            private IEnumerator RemoveCameraAndSkeletalVisualizer(string dataSourceLabel)
            {
                //delete the skeletal visualizer corresponding to that data source
                if (m_enclosingInstance.transform.Find(dataSourceLabel))
                    Destroy(m_enclosingInstance.transform.Find(dataSourceLabel).gameObject);

                //wait until the gridlayout updates itself: this is necessary because we have removed a new panel visualization for the new data source
                //(in the method OnDataSourceToggleValueChanged), but that panel has still not the correct position into the screen until the AutoGridLayout
                //behaviour will not update itself
                yield return new WaitForEndOfFrame();

                //Re-update the screen layout of the data sources visualization, to take in count of this removal
                AdjustCameraViewports();

                //we're not in waiting stage anymore (visualization stuff has been removed)
                FindObjectOfType<WaitManager>().WaitingState = false;

                yield break;
            }

            /// <summary>
            /// Updates the rectangles on the screen dedicated to each camera, after an addition or a removal of a visualization panel
            /// </summary>
            private void AdjustCameraViewports()
            {
                //for each panel in the visualization panel
                for (int i = 0; i < m_enclosingInstance.VisualizationPanel.transform.childCount; i++)
                {
                    //get the rect transform of it
                    Transform cameraVisualizationPanel = m_enclosingInstance.VisualizationPanel.transform.GetChild(i);
                    RectTransform cameraVisualizationPanelRectTransform = cameraVisualizationPanel.GetComponent<RectTransform>();
                    //get the camera corresponding to it (each camera corresponds to a panel it draws into)
                    Camera camera = m_enclosingInstance.transform.GetChild(i).Find("Camera").GetComponent<Camera>();

                    //obtain the world coordinates of the panel rect
                    Vector3[] worldCorners = new Vector3[4];
                    cameraVisualizationPanelRectTransform.GetWorldCorners(worldCorners);

                    //transform the panel rect coordinates to camera view port coordinates
                    camera.rect = new Rect((worldCorners[0].x) / Screen.width,
                                           (worldCorners[0].y) / Screen.height,
                                           (worldCorners[2].x - worldCorners[0].x) / Screen.width,
                                           (worldCorners[2].y - worldCorners[0].y) / Screen.height);
                }
            }

            #endregion
        }

    }

}

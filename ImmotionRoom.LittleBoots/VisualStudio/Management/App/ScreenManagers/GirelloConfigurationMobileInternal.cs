namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.ScreenManagers
{
    using UnityEngine;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedManager;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using UnityEngine.UI;
    using ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.DataSourcesManagement;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.Common;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Tools;
    using System.Linq;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.App.DataSourcesManagement;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils.MessageBoxes;

    /// <summary>
    /// Manages GirelloConfiguration scene behaviour, for mobile system (e.g. Android)
    /// </summary>
    public partial class GirelloConfigurationMobile : MonoBehaviour
    {
        /// <summary>
        /// Contains the actual definition of the GirelloConfigurationMobile, for obfuscation purposes
        /// </summary>
        private class GirelloConfigurationMobileInternal
        {
            #region Private fields

            /// <summary>
            /// External bounds of the Game Area
            /// </summary>
            private BoundsManager m_externalGirelloBounds;

            /// <summary>
            /// Internal bounds of the Game Area
            /// </summary>
            private BoundsManager m_internalGirelloBounds;

            /// <summary>
            /// Scene data stream from the tracking service
            /// </summary>
            private SceneDataProvider m_sceneDataProvider;

            /// <summary>
            /// Current body used to set the limits. It is the one that at the start of the scene is nearest to the origin of world frame of reference
            /// </summary>
            private BodyDataProvider m_currentBody;

            /// <summary>
            /// The GirelloConfigurationMobile object that contains this object
            /// </summary>
            private GirelloConfigurationMobile m_enclosingInstance;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="enclosingInstance">Enclosing instance, whose code has to be implemented</param>
            internal GirelloConfigurationMobileInternal(GirelloConfigurationMobile enclosingInstance)
            {
                m_enclosingInstance = enclosingInstance;
            }

            #endregion

            #region Behaviour methods

            internal void Start()
            {
                //set back button behaviour to stop tracking
                ScenesManager.Instance.SetBackButtonBehaviour((obj) =>
                {
                    //request tracking stop
                    TrackingServiceManagerAdvanced.Instance.RequestCurrentOperativeStatusStop();

                    //we're in waiting stage (command has to be processed by the underlying tracking service)
                    FindObjectOfType<WaitManager>().WaitingState = true;
                });

                //register to tracking service events
                TrackingServiceManagerAdvanced.Instance.TrackingStarted += OnTrackingStarted;
                TrackingServiceManagerAdvanced.Instance.NewGirelloDescriptionSet += OnNewGirelloDescriptionSet;
                TrackingServiceManagerAdvanced.Instance.OperativeStatusStopped += OnOperativeStatusStopped;

                //we're in waiting stage (waiting for the command to be processed by the underlying tracking service)
                FindObjectOfType<WaitManager>().WaitingState = true;

                //start tracking at start
                TrackingServiceManagerAdvanced.Instance.RequestTrackingStart();
            }

            internal void OnDestroy()
            {
                if (TrackingServiceManagerAdvanced.Instance != null)
                {
                    //dispose the scene streaming
                    if (m_sceneDataProvider != null)
                        m_sceneDataProvider.Dispose();

                    //unregister to tracking service events
                    TrackingServiceManagerAdvanced.Instance.TrackingStarted -= OnTrackingStarted;
                    TrackingServiceManagerAdvanced.Instance.NewGirelloDescriptionSet -= OnNewGirelloDescriptionSet;
                    TrackingServiceManagerAdvanced.Instance.OperativeStatusStopped -= OnOperativeStatusStopped;

                    //stop tracking at exit
                    if (TrackingServiceManagerAdvanced.Instance.IsStreamingSkeletons)
                        TrackingServiceManagerAdvanced.Instance.RequestCurrentOperativeStatusStop();
                }
            }

            internal void Update()
            {
                //if the system is already initialized and we have lost the body we were tracking, look for a new body near to the origin
                if (m_currentBody != null && m_currentBody.LastBody == null)
                {
                    m_currentBody = null;
                    m_enclosingInstance.StartCoroutine(GetMainBody());
                }

            }

            #endregion

            #region TrackingService Tracking methods

            /// <summary>
            /// Event called when the tracking start operation gets executed
            /// </summary>
            /// <param name="eventArgs">Arguments with result of the operation</param>
            private void OnTrackingStarted(DataStructures.AdvancedOperationEventArgs eventArgs)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("GirelloConfiguration - Tracking start operation terminated with result {0}", eventArgs.ErrorString ?? "SUCCESS");
                }

                //we're not in waiting stage anymore (command has been processed by the underlying tracking service)
                FindObjectOfType<WaitManager>().WaitingState = false;

                if (eventArgs.ErrorString != null)
                    MessageBox.Show("Error", "Can't start tracking: " + eventArgs.ErrorString + ".\nPlease retry the operation", new UnityEngine.Events.UnityAction(() => { TrackingServiceManagerAdvanced.Instance.ForceStateToIdle(); ScenesManager.Instance.StopWizard(); }),
                            FindObjectsOfType<Selectable>());
                else
                {
                    //load Game area data from the tracking service
                    TrackingServiceVector3 GameAreaCenter = TrackingServiceManagerAdvanced.Instance.TrackingServiceEnvironment.SceneDescriptor.GameArea.Center;
                    TrackingServiceVector3 GameAreaOuterSize = TrackingServiceManagerAdvanced.Instance.TrackingServiceEnvironment.SceneDescriptor.GameArea.Size;
                    TrackingServiceVector3 GameAreaInnerExtents = TrackingServiceManagerAdvanced.Instance.TrackingServiceEnvironment.SceneDescriptor.GameAreaInnerLimits;

                    //add the bounds managers
                    //TODO: ENABLE GIRELLO ON Y DIRECTION (at moment it is defaulted at 3.3 meters, with a little displacement to not show always bottom face warning)
                    GameObject manGo = new GameObject("External Girello Manager");
                    manGo.transform.SetParent(m_enclosingInstance.transform, false);
                    m_externalGirelloBounds = manGo.AddComponent<BoundsManager>();
                    m_externalGirelloBounds.BoundsCenter = new Vector3(GameAreaCenter.X, 1.645f, GameAreaCenter.Z);
                    m_externalGirelloBounds.BoundsExtents = new Vector3(GameAreaOuterSize.X, 3.3f, GameAreaOuterSize.Z) / 2;
                    m_externalGirelloBounds.BoundsLinesMaterial = m_enclosingInstance.BoundsLinesMaterial;
                    m_externalGirelloBounds.BoundsLinesColor = m_enclosingInstance.ExternalGameAreaColor;
                    manGo = new GameObject("Internal Girello Manager");
                    manGo.transform.SetParent(m_enclosingInstance.transform, false);
                    m_internalGirelloBounds = manGo.AddComponent<BoundsManager>();
                    m_internalGirelloBounds.BoundsCenter = new Vector3(GameAreaCenter.X, 1.645f, GameAreaCenter.Z);
                    m_internalGirelloBounds.BoundsExtents = new Vector3(GameAreaInnerExtents.X, 1.65f, GameAreaInnerExtents.Z);
                    m_internalGirelloBounds.BoundsLinesMaterial = m_enclosingInstance.BoundsLinesMaterial;
                    m_internalGirelloBounds.BoundsLinesColor = m_enclosingInstance.InternalGameAreaColor;

                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("GirelloConfiguration - Created Girello Managers");
                    }

                    //start coroutine to get the first body near the origin
                    m_enclosingInstance.StartCoroutine(GetMainBody());
                }
            }

            /// <summary>
            /// Event called when the tracking stop operation gets executed
            /// </summary>
            /// <param name="eventArgs">Arguments with result of the operation</param>
            private void OnOperativeStatusStopped(DataStructures.AdvancedOperationEventArgs eventArgs)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("GirelloConfiguration - Tracking stop operation terminated with result {0}", eventArgs.ErrorString ?? "SUCCESS");
                }

                //we're not in waiting stage anymore (command has been processed by the underlying tracking service)
                FindObjectOfType<WaitManager>().WaitingState = false;

                if (eventArgs.ErrorString != null)
                    MessageBox.Show("Error", "Can't stop tracking: " + eventArgs.ErrorString + ".\nThe system may be in an unknown state", new UnityEngine.Events.UnityAction(() => { TrackingServiceManagerAdvanced.Instance.ForceStateToIdle(); TrackingServiceManagerAdvanced.Instance.ForceStateToIdle(); ScenesManager.Instance.StopWizard(); }),
                            FindObjectsOfType<Selectable>());
                else
                    ScenesManager.Instance.StopWizard();
            }

            /// <summary>
            /// Event called when the set new girello operation gets executed
            /// </summary>
            /// <param name="eventArgs">Arguments with result of the operation</param>
            private void OnNewGirelloDescriptionSet(DataStructures.AdvancedOperationEventArgs eventArgs)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("GirelloConfiguration - Set new girello operation terminated with result {0}", eventArgs.ErrorString ?? "SUCCESS");
                }

                //we're not in waiting stage anymore (command has been processed by the underlying tracking service)
                FindObjectOfType<WaitManager>().WaitingState = false;

                if (eventArgs.ErrorString != null)
                    MessageBox.Show("Error", "Can't set new girello: " + eventArgs.ErrorString + ".\nPlease retry setting game area", new UnityEngine.Events.UnityAction(() => { TrackingServiceManagerAdvanced.Instance.ForceStateToIdle(); ScenesManager.Instance.StopWizard(); }),
                            FindObjectsOfType<Selectable>());
                else
                {
                    //if girello set went ok, request tracking stop
                    TrackingServiceManagerAdvanced.Instance.RequestCurrentOperativeStatusStop();

                    //we're again in waiting stage (command has to be processed by the underlying tracking service)
                    FindObjectOfType<WaitManager>().WaitingState = true;
                }
            }

            #endregion

            #region Buttons events methods

            /// <summary>
            /// Triggered when the OK button gets clicked
            /// </summary>
            internal void OnOkButtonClicked()
            {
                ////uncomment to show girello data and then stop tracking when ok
                //MessageBox.Show("Girello data", string.Format("GameArea.Center: {0:0.000}\nGameArea.Size: {1:0.000}\nGameArea.InnerLimits: {2:0.000}", m_externalGirelloBounds.BoundsCenter,
                //                m_externalGirelloBounds.BoundsExtents * 2, m_internalGirelloBounds.BoundsExtents), 
                //                new UnityEngine.Events.UnityAction(() => 
                //                {
                //                    //stop tracking
                //                    if (TrackingServiceManagerAdvanced.Instance.IsStreamingSkeletons)
                //                    {
                //                        //we're in waiting stage (waiting for the command to be processed by the underlying tracking service)
                //                        FindObjectOfType<WaitManager>().WaitingState = true;

                //                        TrackingServiceManagerAdvanced.Instance.RequestCurrentOperativeStatusStop();
                //                    }
                //                }),
                //                new Color(0, 0.5f, 1.0f),
                //                Color.white,
                //                FindObjectsOfType<Selectable>());    

                //ask the underlying tracking service to set the new girello
                //we're in waiting stage (waiting for the command to be processed by the underlying tracking service)
                FindObjectOfType<WaitManager>().WaitingState = true;

                TrackingServiceManagerAdvanced.Instance.SetNewGirelloAsync(m_externalGirelloBounds.BoundsCenter, m_externalGirelloBounds.BoundsExtents * 2, m_internalGirelloBounds.BoundsExtents);

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("GirelloConfiguration - Setting new Girello data: GameArea.Center: {0:0.000}\nGameArea.Size: {1:0.000}\nGameArea.InnerLimits: {2:0.000}", m_externalGirelloBounds.BoundsCenter,
                                m_externalGirelloBounds.BoundsExtents * 2, m_internalGirelloBounds.BoundsExtents);
                }
            }

            /// <summary>
            /// Triggered when the LeftLimit button gets clicked
            /// </summary>
            internal void OnLeftLimitButtonClicked()
            {
                //set new limit using spine mid joint
                if (TrackingServiceManagerAdvanced.Instance.IsStreamingSkeletons && m_currentBody != null && m_currentBody.ActualSceneDataProvider.IsStillValid && m_currentBody.LastBody != null)
                {
                    m_externalGirelloBounds.SetNewBoundLimit(BoundsGrowingType.LeftLimit, m_currentBody.LastBody.Joints[TrackingServiceBodyJointTypes.SpineMid].ToUnityVector3NoScale());
                    UpdateInnerBounds();
                }
            }

            /// <summary>
            /// Triggered when the FrontLimit button gets clicked
            /// </summary>
            internal void OnFrontLimitButtonClicked()
            {
                //set new limit using spine mid joint
                if (TrackingServiceManagerAdvanced.Instance.IsStreamingSkeletons && m_currentBody != null && m_currentBody.ActualSceneDataProvider.IsStillValid && m_currentBody.LastBody != null)
                {
                    m_externalGirelloBounds.SetNewBoundLimit(BoundsGrowingType.FrontLimit, m_currentBody.LastBody.Joints[TrackingServiceBodyJointTypes.SpineMid].ToUnityVector3NoScale());
                    UpdateInnerBounds();
                }
            }

            /// <summary>
            /// Triggered when the BackLimit button gets clicked
            /// </summary>
            internal void OnBackLimitButtonClicked()
            {
                //set new limit using spine mid joint
                if (TrackingServiceManagerAdvanced.Instance.IsStreamingSkeletons && m_currentBody != null && m_currentBody.ActualSceneDataProvider.IsStillValid && m_currentBody.LastBody != null)
                {
                    m_externalGirelloBounds.SetNewBoundLimit(BoundsGrowingType.BackLimit, m_currentBody.LastBody.Joints[TrackingServiceBodyJointTypes.SpineMid].ToUnityVector3NoScale());
                    UpdateInnerBounds();
                }
            }

            /// <summary>
            /// Triggered when the RightLimit button gets clicked
            /// </summary>
            internal void OnRightLimitButtonClicked()
            {
                //set new limit using spine mid joint
                if (TrackingServiceManagerAdvanced.Instance.IsStreamingSkeletons && m_currentBody != null && m_currentBody.ActualSceneDataProvider.IsStillValid && m_currentBody.LastBody != null)
                {
                    m_externalGirelloBounds.SetNewBoundLimit(BoundsGrowingType.RightLimit, m_currentBody.LastBody.Joints[TrackingServiceBodyJointTypes.SpineMid].ToUnityVector3NoScale());
                    UpdateInnerBounds();
                }
            }

            /// <summary>
            /// Triggered when the RESET button gets clicked
            /// </summary>
            internal void OnResetLimitsButtonClicked()
            {
                //reset limits
                m_externalGirelloBounds.SetNullLimits();
                UpdateInnerBounds();
            }

            #endregion

            #region Private methods

            /// <summary>
            /// Update the inner bounds, after the modification of the outer bounds, to mantain everything coherent
            /// </summary>
            private void UpdateInnerBounds()
            {
                m_internalGirelloBounds.BoundsCenter = m_externalGirelloBounds.BoundsCenter;
                //at the moment don't touch the y component and leave it to the initial value
                m_internalGirelloBounds.BoundsExtents = new Vector3(m_externalGirelloBounds.BoundsExtents.x * m_enclosingInstance.InnerToOuterBoundsProportion,
                                                                    m_internalGirelloBounds.BoundsExtents.y,
                                                                    m_externalGirelloBounds.BoundsExtents.z * m_enclosingInstance.InnerToOuterBoundsProportion);
            }

            /// <summary>
            /// Coroutine to get the body nearest to the origin of world frame of reference
            /// </summary>
            /// <returns></returns>
            private IEnumerator GetMainBody()
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("GirelloConfiguration - Entering Body Obtaining Method");
                }

                //disable all buttons regarding the limits (without a skeleton, they are useless)
                Button[] limitsButtons = FindObjectsOfType<Button>().Where(button => (button.transform.GetChild(0).GetComponent<Text>().text != "OK" && button.transform.GetChild(0).GetComponent<Text>().text != "RESET")).ToArray();

                foreach (Button button in limitsButtons)
                    button.interactable = false;

                yield return new WaitForSeconds(1.0f); //wait a bit to let the tracking stabilize itself

                if (m_sceneDataProvider == null) //start the stream of merging skeletons
                    m_sceneDataProvider = TrackingServiceManagerAdvanced.Instance.StartSceneDataProvider(TrackingServiceConstants.MergedStreamName, TrackingService.ControlClient.Model.TrackingServiceSceneDataStreamModes.WorldTransform);

                //until we have a valid tracking service, look if have a skeleton near the origin
                while (TrackingServiceManagerAdvanced.Instance.IsStreamingSkeletons && m_currentBody == null)
                {
                    //loop for all the found skeletons
                    foreach (TrackingServiceBodyData body in m_sceneDataProvider.LastBodies)
                        //if one is near the origin of the X,Z plane, get its provider and exit
                        if (Mathf.Abs(body.Joints[TrackingServiceBodyJointTypes.SpineMid].Position.X) < 0.75f &&
                           Mathf.Abs(body.Joints[TrackingServiceBodyJointTypes.SpineMid].Position.Z) < 0.75f)
                        {
                            if (Log.IsDebugEnabled)
                            {
                                Log.Debug("GirelloConfiguration - Using Body with ID {0}", body.Id);
                            }

                            m_currentBody = new BodyDataProvider(m_sceneDataProvider, body.Id);
                            break;
                        }

                    yield return new WaitForSeconds(0.2f);
                }

                //we have the body, reenable the buttons and return
                foreach (Button button in limitsButtons)
                    button.interactable = true;

                yield break;
            }

            #endregion
        }

    }

}

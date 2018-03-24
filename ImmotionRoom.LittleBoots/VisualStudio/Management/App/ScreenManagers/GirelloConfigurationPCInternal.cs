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
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.App.DataSourcesManagement;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils.MessageBoxes;

    
    /// <summary>
    /// Manages GirelloConfiguration scene behaviour for non-mobile systems
    /// </summary>
    public partial class GirelloConfigurationPC : MonoBehaviour
    {
        /// <summary>
        /// Contains the actual definition of the GirelloConfigurationPC, for obfuscation purposes
        /// </summary>
        private class GirelloConfigurationPCInternal
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
            /// Which side of the bounding box are we moving with the keyboard. 
            /// </summary>
            private BoundsGrowingType m_currentBoundingBoxMovingSize = BoundsGrowingType.None;

            /// <summary>
            /// The GirelloConfigurationPC object that contains this object
            /// </summary>
            private GirelloConfigurationPC m_enclosingInstance;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="enclosingInstance">Enclosing instance, whose code has to be implemented</param>
            internal GirelloConfigurationPCInternal(GirelloConfigurationPC enclosingInstance)
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
                //if the system is initialized 
                if (m_externalGirelloBounds != null)
                {
                    //if user presses the arrow keys, move the bounding box sides

                    //so get which arrows have been pressed
                    Vector2 movement = Vector2.zero;

                    if (Input.GetKey(KeyCode.A))
                        movement.x -= 1;
                    if (Input.GetKey(KeyCode.D))
                        movement.x += 1;
                    if (Input.GetKey(KeyCode.W))
                        movement.y += 1;
                    if (Input.GetKey(KeyCode.S))
                        movement.y -= 1;

                    movement *= Time.deltaTime * 1.5f; //1.5 is a constant to make the speed acceptable. Notice that if no keys have been pressed, we have still 0 here

                    //if we have a selected size to move and there has been a movement
                    if (m_currentBoundingBoxMovingSize != BoundsGrowingType.None && (movement.x != 0 || movement.y != 0))
                    {
                        //ask the bound manager to move the appropriate side of the external bounds
                        if (m_currentBoundingBoxMovingSize == BoundsGrowingType.LeftLimit)
                        {
                            m_externalGirelloBounds.SetNewBoundLimit(BoundsGrowingType.LeftLimit, new Vector3(m_externalGirelloBounds.BoundsCenter.x - m_externalGirelloBounds.BoundsExtents.x + movement.x, 0, 0));
                        }
                        else if (m_currentBoundingBoxMovingSize == BoundsGrowingType.RightLimit)
                        {
                            m_externalGirelloBounds.SetNewBoundLimit(BoundsGrowingType.RightLimit, new Vector3(m_externalGirelloBounds.BoundsCenter.x + m_externalGirelloBounds.BoundsExtents.x + movement.x, 0, 0));
                        }
                        else if (m_currentBoundingBoxMovingSize == BoundsGrowingType.FrontLimit)
                        {
                            m_externalGirelloBounds.SetNewBoundLimit(BoundsGrowingType.FrontLimit, new Vector3(0, 0, m_externalGirelloBounds.BoundsCenter.z + m_externalGirelloBounds.BoundsExtents.z + movement.y));
                        }
                        else if (m_currentBoundingBoxMovingSize == BoundsGrowingType.BackLimit)
                        {
                            m_externalGirelloBounds.SetNewBoundLimit(BoundsGrowingType.BackLimit, new Vector3(0, 0, m_externalGirelloBounds.BoundsCenter.z - m_externalGirelloBounds.BoundsExtents.z + movement.y));
                        }

                        //update the inner bounds accordingly
                        UpdateInnerBounds();
                    }

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
                    MessageBox.Show("Error", "Can't stop tracking: " + eventArgs.ErrorString + ".\nThe system may be in an unknown state", new UnityEngine.Events.UnityAction(() => { TrackingServiceManagerAdvanced.Instance.ForceStateToIdle(); ScenesManager.Instance.StopWizard(); }),
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
            internal void OnLeftLimitButtonClicked(bool newStatus)
            {
                //set the correct size to move: if the button is clicked, select the left side, otherwise return to unselected stage
                if (newStatus == true)
                    m_currentBoundingBoxMovingSize = BoundsGrowingType.LeftLimit;
                else
                    m_currentBoundingBoxMovingSize = BoundsGrowingType.None;
            }

            /// <summary>
            /// Triggered when the FrontLimit button gets clicked
            /// </summary>
            internal void OnFrontLimitButtonClicked(bool newStatus)
            {
                //set the correct size to move: if the button is clicked, select the front side, otherwise return to unselected stage
                if (newStatus == true)
                    m_currentBoundingBoxMovingSize = BoundsGrowingType.FrontLimit;
                else
                    m_currentBoundingBoxMovingSize = BoundsGrowingType.None;
            }

            /// <summary>
            /// Triggered when the BackLimit button gets clicked
            /// </summary>
            internal void OnBackLimitButtonClicked(bool newStatus)
            {
                //set the correct size to move: if the button is clicked, select the back side, otherwise return to unselected stage
                if (newStatus == true)
                    m_currentBoundingBoxMovingSize = BoundsGrowingType.BackLimit;
                else
                    m_currentBoundingBoxMovingSize = BoundsGrowingType.None;
            }

            /// <summary>
            /// Triggered when the RightLimit button gets clicked
            /// </summary>
            internal void OnRightLimitButtonClicked(bool newStatus)
            {
                //set the correct size to move: if the button is clicked, select the right side, otherwise return to unselected stage
                if (newStatus == true)
                    m_currentBoundingBoxMovingSize = BoundsGrowingType.RightLimit;
                else
                    m_currentBoundingBoxMovingSize = BoundsGrowingType.None;
            }

            /// <summary>
            /// Triggered when the RESET button gets clicked
            /// </summary>
            internal void OnResetLimitsButtonClicked()
            {
                //reset limits to infinit
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

            #endregion
        }

    }
}

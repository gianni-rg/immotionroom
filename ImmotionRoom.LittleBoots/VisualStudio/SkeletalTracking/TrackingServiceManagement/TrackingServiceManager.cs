namespace ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement
{   
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.DataSourcesManagement;
    using ImmotionAR.ImmotionRoom.TrackingService.ControlClient.Model;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using ImmotionAR.ImmotionRoom.AutoDiscovery;
    using ImmotionAR.ImmotionRoom.TrackingService.ControlClient;
    using ImmotionAR.ImmotionRoom.Helpers;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.SupportStruct;
    using UnityEngine;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.DataStructures;

    /// <summary>
    /// Base class, representing the base behaviour of a TrackingService Manager
    /// </summary>
    public partial class TrackingServiceManager : EventBasedMonoBehaviour
    {
        #region Constants

        private const string BodyDataSourceRootGameObjectName = "Scene Data Sources Root";

        /// <summary>
        /// Name of the special file of tracking service settings. If this file is present, settings present in this file will
        /// be used and no discovery/user values/Unity settings gets used to find the tracking service.
        /// (this is particularly useful for localhost tracking service)
        /// File format is (one info per line, in text format):
        /// TRACKING SERVICE NAME
        /// TRACKING SERVICE CONTROL IP
        /// TRACKING SERVICE CONTROL PORT
        /// LOCAL DATA SOURCE SERVICE NAME
        /// LOCAL DATA SOURCE SERVICE CONTROL IP
        /// LOCAL DATA SOURCE SERVICE CONTROL PORT
        /// LOCAL DATA SOURCE SERVICE DATA IP
        /// LOCAL DATA SOURCE SERVICE DATA PORT
        /// </summary>
        public const string SpecialFileSettingsName = @".\TrackingServiceSettings.txt";

        #endregion

        #region Nested Classes

        /// <summary>
        /// Handles discovery and communication with the underlying TrackingService.
        /// 
        /// See TrackingServiceDiscoveryCommunicator.cs for the implementation
        /// </summary>
        protected internal partial class TrackingServiceDiscoveryCommunicator
        {

        }

        /// <summary>
        /// Implements management of <see cref="SceneDataSource"/> objects.
        /// This class is used to manage the data source for <see cref="TrackingServiceManager"/>.
        /// Dispose method should be called when an object of this class is no longer needed
        /// 
        /// See SceneDataSourcesManager.cs for the implementation
        /// </summary>
        protected internal partial class SceneDataSourcesManager
        {

        }

        /// <summary>
        /// Stores <see cref="TrackingServiceManager"/> settings, across multiple game sessions.
        /// These settings regard how the underlying tracking service can be contacted
        /// 
        /// See TrackingServiceSettingsManager.cs for the implementation
        /// </summary>
        protected internal partial class TrackingServiceSettingsManager
        {

        }

        #endregion

        #region Private fields

        /// <summary>
        /// Enables discovery and communication with underlying tracking service
        /// </summary>
        protected TrackingServiceDiscoveryCommunicator m_TrackingServiceCommunicator;

        /// <summary>
        /// Manager of the <see cref="SceneDataSource"/> objects that this tracking service manager handle
        /// </summary>
        protected SceneDataSourcesManager m_sceneDataSourcesManager;

        /// <summary>
        /// Saves and restores Tracking Service settings across different games sessions
        /// </summary>
        protected TrackingServiceSettingsManager m_settingsManager;

        #endregion

        #region Public fields

        /// <summary>
        /// Gets current manager state
        /// </summary>
        public TrackingServiceManagerState CurrentState { get; protected set; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets if this object is connected to an underlying tracking service
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return CurrentState != TrackingServiceManagerState.NotConnected && CurrentState != TrackingServiceManagerState.Error;
            }
        }

        #endregion

        #region Behavior Methods

        protected virtual void Awake()
        {
            // Create the Scene Data Sources Manager, that will handle all scene data streams management for us
            m_sceneDataSourcesManager = new SceneDataSourcesManager(gameObject, BodyDataSourceRootGameObjectName);

            //create the settings manager
            m_settingsManager = new TrackingServiceSettingsManager();

            //create the discoverer and communicator with underlying tracking service
            m_TrackingServiceCommunicator = new TrackingServiceDiscoveryCommunicator(this);

            //set initial state to not connected (to underlying tracking service)
            CurrentState = TrackingServiceManagerState.NotConnected;
        }

        protected void Start()
        {            
        }

        protected new void Update()
        {
            // Required, to enable events handling
            base.Update();
        }

        protected override void OnDestroy()
        {
            //must call this for correct functioning of the behaviour
            base.OnDestroy();

            //destroy all scene data sources created for this session
            m_sceneDataSourcesManager.Dispose();

            //if we were during a discovery, stop it (otherwise, it does nothing)
            m_TrackingServiceCommunicator.StopDiscovery();
        }

        #endregion

        #region Tracking Service Discovery&Initialization Methods

        /// <summary>
        /// Callback to be called when the process of discovery+initialization of the underlying Tracking Service finishes
        /// </summary>
        /// <param name="trackingServiceStatus">Resulting status of the discovery+initialization operation</param>
        /// <param name="trackingServiceInfo">Info about the found tracking service, if any (otherwise it is null)</param>
        protected virtual void TrackingServiceDiscoveryInitCompleted(TrackingServiceStatusResponse trackingServiceStatus, TrackingServiceInfo trackingServiceInfo)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManager - TrackingServiceDiscoveryInitCompleted called");
            }

            //If everything went well, we're in idle state, otherwise we're in error state
            if (trackingServiceStatus.IsError)
                CurrentState = TrackingServiceManagerState.Error;
            else if (CurrentState == TrackingServiceManagerState.NotConnected) //modify status only from not connected to idle
                CurrentState = TrackingServiceManagerState.Idle;
        }

        #endregion

        #region Operations and Statuses methods

        /// <summary>
        /// Requires this object to make the underlying tracking sevice to change its status
        /// </summary>
        /// <param name="newStatus">New status to enter into</param>
        /// <param name="newStatusParams">Optional parameters for the new status; used at moment only for Calibration state</param>
        /// <exception cref="InvalidOperationException">If tracking service is not in idle stage</exception>
        /// <exception cref="ArgumentException">If any of the arguments provided are invalid</exception>
        protected void StartNewOperativeStatus(TrackingServiceManagerState newStatus, object newStatusParams)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManager for Tracking service {0} - Requested {1} as new status", m_settingsManager.TrackingServiceId, newStatus.ToString());
            }

            //check that we are in idle state to start a new state
            if(CurrentState != TrackingServiceManagerState.Idle)
            {
                if (Log.IsErrorEnabled)
                {
                    Log.Error("TrackingServiceManager for Tracking service {0} - Requested to start a new state from a tracking service in non-idle state", m_settingsManager.TrackingServiceId);
                }

                throw new InvalidOperationException("Requested to start a new state from a tracking service in non-idle state");
            }

            //take different actions depending on the new required state
            switch(newStatus)
            {
                case TrackingServiceManagerState.Idle:
                case TrackingServiceManagerState.Error:
                case TrackingServiceManagerState.NotConnected:

                    if (Log.IsErrorEnabled)
                    {
                        Log.Error("TrackingServiceManager for Tracking service {0} - Requested to start a non startable state", m_settingsManager.TrackingServiceId);
                    }

                    throw new ArgumentException("Requested to start an invalid Tracking Service Manager state");

                //start calibration of ImmotionRoom
                case TrackingServiceManagerState.Calibrating:

                    //get calibration params
                    CalibrationParameters calibrationParameters = newStatusParams as CalibrationParameters;

                    if(calibrationParameters == null)
                    {
                        if (Log.IsErrorEnabled)
                        {
                            Log.Error("TrackingServiceManager for Tracking service {0} - Requested to start calibration without calibration parameters", m_settingsManager.TrackingServiceId);
                        }

                        throw new ArgumentException("Requested to start calibration without calibration parameters");
                    }

                    //request calibration start and then call the OperativeStatusStart callback to perform all operations
                    m_TrackingServiceCommunicator.TrackingServiceController.StartCalibrationAsync(calibrationParameters, result => OperativeStatusStart("Calibration",
                                                                                                  result, TrackingServiceManagerState.Calibrating));

                    break;

                //start scene/people tracking inside ImmotionRoom
                case TrackingServiceManagerState.Tracking:

                    //request tracking start and then call the OperativeStatusStart callback to perform all operations
                    m_TrackingServiceCommunicator.TrackingServiceController.StartTrackingAsync(new TrackingSessionConfiguration() { DataSourceTrackingSettings = new TrackingSessionDataSourceConfiguration() { BodyClippingEdgesEnabled = false, HandsStatusEnabled = false, TrackJointRotation = false } },
                                                                                               result => OperativeStatusStart("Tracking",
                                                                                               result, TrackingServiceManagerState.Tracking));

                    break;

                //start diagnostic mode of ImmotionRoom
                case TrackingServiceManagerState.Diagnostic:

                    //request diagnostic start and then call the OperativeStatusStart callback to perform all operations
                    m_TrackingServiceCommunicator.TrackingServiceController.StartDiagnosticModeAsync(new TrackingSessionConfiguration() { DataSourceTrackingSettings = new TrackingSessionDataSourceConfiguration() { BodyClippingEdgesEnabled = false, HandsStatusEnabled = false, TrackJointRotation = false } },
                                                                                                     result => OperativeStatusStart("Diagnostic",
                                                                                                     result, TrackingServiceManagerState.Diagnostic));

                    break;

                //we should never fall inside this state
                default:
                    throw new Exception("WTF?");

            }
        }

        /// <summary>
        /// Requires this object to make the underlying tracking sevice to stop current operative status and return to idle stage
        /// </summary>
        /// <exception cref="InvalidOperationException">If tracking service is in idle or invalid stage</exception>
        protected void StopCurrentOperativeStatus()
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManager for Tracking service {0} - Requested to stop current status", m_settingsManager.TrackingServiceId);
            }

            //take different actions depending on current state
            switch (CurrentState)
            {
                case TrackingServiceManagerState.Idle:
                case TrackingServiceManagerState.Error:
                case TrackingServiceManagerState.NotConnected:

                    if (Log.IsErrorEnabled)
                    {
                        Log.Error("TrackingServiceManager for Tracking service {0} - Requested to stop an invalid or idle state", m_settingsManager.TrackingServiceId);
                    }

                    throw new ArgumentException("Requested to stop an invalid or idle Tracking Service Manager state");

                //stop calibration of ImmotionRoom
                case TrackingServiceManagerState.Calibrating:

                    //request calibration stop and then call the OperativeStatusStop callback to perform all operations
                    m_TrackingServiceCommunicator.TrackingServiceController.ExecuteCalibrationStepAsync(new CalibrationParameters { Step = TrackingServiceCalibrationSteps.End }, 
                                                                                                  result => OperativeStatusStop("Calibration", result));

                    break;

                //stop scene/people tracking inside ImmotionRoom
                case TrackingServiceManagerState.Tracking:

                    //request tracking stop and then call the OperativeStatusStop callback to perform all operations
                    m_TrackingServiceCommunicator.TrackingServiceController.StopTrackingAsync(result => OperativeStatusStop("Tracking", result));

                    break;

                //stop diagnostic mode of ImmotionRoom
                case TrackingServiceManagerState.Diagnostic:

                    //request diagnostic stop and then call the OperativeStatusStop callback to perform all operations
                    m_TrackingServiceCommunicator.TrackingServiceController.StopDiagnosticModeAsync(result => OperativeStatusStop("Diagnostic", result));

                    break;

                //we should never fall inside this state
                default:
                    throw new Exception("WTF?");

            }
        }

        /// <summary>
        /// Performs operations necessary after the start of a new operative status (e.g. tracking, calibration, ...)
        /// request made to the underlying tracking service  
        /// </summary>
        /// <remarks>
        /// This methods body gets executed only on the UI thread
        /// </remarks>
        /// <param name="operationTagName">Human readable name of the new status (used for logging)</param>
        /// <param name="trackingServiceResultStatus">Result of the new operative status, as returned by the underlying tracking service</param>
        /// <param name="newStatus">New Status to activate on this object if the operations are successful</param>
        virtual protected void OperativeStatusStart(string operationTagName, OperationResponse trackingServiceResultStatus, TrackingServiceManagerState newStatus)
        {
            ExecuteOnUI(() =>
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("TrackingServiceManager - {0} Start. Result is {1}", operationTagName, trackingServiceResultStatus.IsError ? string.Format("Failure: {0}", trackingServiceResultStatus.ErrorDescription) : "Success");
                }

                //if operation start request has been processed well
                if (!trackingServiceResultStatus.IsError)
                {
                    //get tracking service status to see if the request has been accepted and perfomed 
                    m_TrackingServiceCommunicator.TrackingServiceController.GetStatusAsync(result3 => ExecuteOnUI(() =>
                    {
                        if (Log.IsDebugEnabled)
                        {
                            Log.Debug("TrackingServiceManager - {0} Get Status. Result is {1}", operationTagName, result3.IsError ? string.Format("Failure: {0}", result3.ErrorDescription) : "Success");
                        }

                        //check in the status response, if the system has started the desired state, and if not, set error flags
                        //(we're here if get status request was successful, but the returned status is not the one we were expecting)
                        if(!result3.IsError &&
                            ((newStatus == TrackingServiceManagerState.Diagnostic && result3.Status.CurrentState != TrackingServiceState.DiagnosticMode) ||
                            (newStatus == TrackingServiceManagerState.Calibrating && result3.Status.CurrentState != TrackingServiceState.Calibration) ||
                            (newStatus == TrackingServiceManagerState.Tracking && result3.Status.CurrentState != TrackingServiceState.Running)))
                        {
                            result3.IsError = true;
                            result3.ErrorDescription = "Failed to start the desired state";
                        }

                        //pass this status to the function that will performs all the actions necessary to take in count the new status change
                        this.OperativeStartPrep(result3, newStatus);
                    }));
                }
                //else, if request went bad, now Tracking Service is in error state. 
                else
                {
                    CurrentState = TrackingServiceManagerState.Error;

                    if (Log.IsErrorEnabled)
                    {
                        Log.Error("TrackingServiceManager - {0} Start FAILED", operationTagName);
                    }     
                }
            });
        }

        /// <summary>
        /// Performs operation consequent to a new operative status performed by the underlying tracking service
        /// </summary>
        /// <param name="statusStartResponse">Status of the request (to tell if the new status start was successful)</param>
        /// <param name="newStatus">New Status to activate on this object if the operations are successful</param>
        protected virtual void OperativeStartPrep(TrackingServiceStatusResponse statusStartResponse, TrackingServiceManagerState newStatus)
        {
            //if everything went well
            if (!statusStartResponse.IsError)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("TrackingServiceManager - Starting new status went ok. Creating Scene Data Sources");
                }

                //initialize the Scene Data Sources Manager to take in count new status data sources
                m_sceneDataSourcesManager.Initialize(statusStartResponse.Status.DataStreamers, statusStartResponse.Status.DataSources); ;

                //set new status
                CurrentState = newStatus;
            }
            //else, we are in error state
            else
            {
                if (Log.IsErrorEnabled)
                {
                    Log.Error("TrackingServiceManager - Failure: {0}", statusStartResponse.ErrorDescription);
                }

                CurrentState = TrackingServiceManagerState.Error;           
            }
        }

        /// <summary>
        /// Performs operations necessary after the stop of current operative status (e.g. tracking, calibration, ...)
        /// request made to the underlying tracking service  
        /// </summary>
        /// <remarks>
        /// This methods body gets executed only on the UI thread
        /// </remarks>
        /// <param name="operationTagName">Human readable name of the new status (used for logging)</param>
        /// <param name="trackingServiceResultStatus">Result of stopping current operative status, as returned by the underlying tracking service</param>
        virtual protected void OperativeStatusStop(string operationTagName, OperationResponse trackingServiceResultStatus)
        {
            ExecuteOnUI(() =>
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("TrackingServiceManager - {0} Stop. Result is {1}", operationTagName, trackingServiceResultStatus.IsError ? string.Format("Failure: {0}", trackingServiceResultStatus.ErrorDescription) : "Success");
                }

                // Call the callback to perform right operations for the end of this status
                this.OperativeStopPrep(trackingServiceResultStatus);
            });
        }

        /// <summary>
        /// Performs operation consequent to end of current operative status performed by the underlying tracking service
        /// </summary>
        /// <param name="statusStopResponse">Status of the request (to tell if the status stop was successful)</param>
        protected virtual void OperativeStopPrep(OperationResponse statusStopResponse)
        {
            //if request went well
            if (!statusStopResponse.IsError)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("TrackingServiceManager - Destroying Scene Data Sources");
                }

                // Destroy all known body data streamers
                m_sceneDataSourcesManager.Reset();

                CurrentState = TrackingServiceManagerState.Idle;
            }
            //otherwise, an error occurred
            else
            {
                if (Log.IsErrorEnabled)
                {
                    Log.Error("TrackingServiceManager - Failure: {0}", statusStopResponse.ErrorDescription);
                }

                CurrentState = TrackingServiceManagerState.Error;
            }
            
        }

        #endregion

        #region Data Source Management Methods

        /// <summary>
        /// Get data source string name associated with the desired unique id
        /// </summary>
        /// <param name="dataSourceUniqueId">byte ID of the data source</param>
        /// <returns>String representation of desired data source</returns>
        public string GetDataSourceNameFromByteId(byte dataSourceUniqueId)
        {
            return m_sceneDataSourcesManager.GetDataSourceNameFromByteId(dataSourceUniqueId);
        }

        #endregion

        #region Data streams offering management methods

        /// <summary>
        /// Give the caller an object through which can receive the desired scene stream data at each frame.
        /// After the object has been used, the Dispose method should be called on the provided object
        /// If no Dispose gets called on the provided object, the stream gets closed at the end of the program.
        /// For more info, see <see cref="SceneDataProvider" /> class documentation
        /// </summary>
        /// <param name="streamerInfoId">ID of the body data streamer of interest</param>
        /// <param name="streamingMode">Streaming mode of interest</param>
        /// <returns>
        /// Proxy object with properties exposing each frame updated info about scene data streaming, or null if the
        /// required stream does not exists
        /// </returns>
        protected SceneDataProvider StartDataProvider(string streamerInfoId, TrackingServiceSceneDataStreamModes streamingMode)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManager - Requested data provider for stream {0} with streaming mode {1}", streamerInfoId, streamingMode);
            }

            return m_sceneDataSourcesManager.StartDataProvider(streamerInfoId, streamingMode);
        }

        #endregion

        #region Singleton-like implementation

        /// <summary>
        /// Gets the first running instance of the TrackingServiceManager. If it does not exists, creates an instance of the 
        /// <see cref="TrackingServiceManagerBasic"/> class
        /// </summary>
        /// <returns>Instance of the TrackingServiceManager</returns>
        public static TrackingServiceManager Instance
        {
            get
            {
                // Search an object of type TrackingServiceManagerBasic. If we find it, return it.
                // Otherwise, let's create a new gameobject, add a TrackingServiceManagerBasic to it and return it.
                var instance = FindObjectOfType<TrackingServiceManager>();

                if (instance != null)
                {
                    return instance;
                }
                else
                    return TrackingServiceManagerBasic.Instance;
            }
        }

        #endregion
    }
}

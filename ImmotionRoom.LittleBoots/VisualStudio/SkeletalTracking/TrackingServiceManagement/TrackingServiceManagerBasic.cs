namespace ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using ImmotionAR.ImmotionRoom.TrackingService.ControlClient.Model;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.SupportStruct;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.DataSourcesManagement;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.Common;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.DataStructures;
    using System.IO;

    /// <summary>
    /// Offers basic Tracking Service Management for applications
    /// </summary>
    public class TrackingServiceManagerBasic : TrackingServiceManager
    {
        #region Behaviour public properties

        /// <summary>
        /// The way this manager discovers the underlying tracking service
        /// </summary>
        [Tooltip("Defines the way this manager discovers the underlying tracking service")]
        public TrackingServiceManagersDiscoveryMode DiscoveryMode = TrackingServiceManagersDiscoveryMode.SettingsThenDiscovery;

        /// <summary>
        /// True if the system should start tracking as soon as the communication with the tracking service gets established; false othwerise
        /// </summary>
        [Tooltip("If the system should start tracking as soon as the communication with the tracking service gets established")]
        public bool AutoStartTracking = true;

        /// <summary>
        /// Id of the underlying tracking service to connect to. Leave this field empty to trigger auto discovery
        /// </summary>
        [Tooltip("Id of the underlying tracking service to connect to")]
        public string UserProvidedId;

        /// <summary>
        /// IP Address of the underlying tracking service to connect to. 
        /// </summary>
        [Tooltip("IP Address of the underlying tracking service to connect to")]
        public string UserProvidedControlApiEndpoint; 

        /// <summary>
        /// IP Port of the underlying tracking service to connect to. 
        /// </summary>
        [Tooltip("IP Port of the underlying tracking service to connect to")]
        public int UserProvidedControlApiPort;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets if the TrackingServiceManager is tracking the users bodies inside the system (it is in tracking mode)
        /// </summary>
        public bool IsTracking
        {
            get 
            { 
                return CurrentState == TrackingServiceManagerState.Tracking; 
            }
        }

        /// <summary>
        /// Get Tracking Service Environment 
        /// </summary>
        public TrackingServiceEnv TrackingServiceEnvironment { get; private set; }

        #endregion

        #region Behavior Methods

        protected new void Awake()
        {
            base.Awake();           
        }

        protected new void Start()
        {
            base.Start();

            //try to perform tracking service connection using special file: if the special file does not exist or if something goes wrong...
            if (!DiscoverTrackingServiceUsingSpecialFile())
                //Start standard discovery and initialization
                DiscoveryAndInitializeTrackingServiceAsync(0);
        }

        protected new void Update()
        {
            // Required, to enable events handling
            base.Update();
        }

        protected override void OnDestroy()
        {
            //stop discovery&init routines
            StopAllCoroutines();

            //must call this for correct functioning of the behaviour
            base.OnDestroy();

            //if we're during a tracking operation, stop it
            if(IsTracking)
                StopCurrentOperativeStatus();

        }

        #endregion

        #region Tracking Service Discovery&Initialization Methods

        /// <summary>
        /// Checks if special file with system settings was given by the user. If the answer is yes, connects to the provided 
        /// tracking service
        /// </summary>
        /// <returns>True if tracking service discovery using special file was required and performed, false otherwise</returns>
        private bool DiscoverTrackingServiceUsingSpecialFile()
        {
            //check use of tracking service settings special file.
            //If this file is present (on PC), the system is considered all local.
            //The problem is that we can't perform a classical discovery on localhost, so we have to read this settings from file
            //and try to connect to a tracking service with this data
            try
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("TrackingServiceManagerBasic - Trying to find a valid Special file {0}", SpecialFileSettingsName);
                }

                //if we are on PC (on android localhost connected kinect has no sense)
                if (Application.platform != RuntimePlatform.Android)
                {
                    //If there is a file with the special name for this purposes
                    if (File.Exists(SpecialFileSettingsName))
                    {
                        //the file has to contain eight lines:
                        //TRACKING SERVICE ID (string)
                        //TRACKING SERVICE CONTROL IP (string)
                        //TRACKING SERVICE CONTROL PORT (int)
                        //LOCAL DATA SOURCE SERVICE NAME
                        //LOCAL DATA SOURCE SERVICE CONTROL IP
                        //LOCAL DATA SOURCE SERVICE CONTROL PORT
                        //LOCAL DATA SOURCE SERVICE DATA IP
                        //LOCAL DATA SOURCE SERVICE DATA PORT
                        string[] fileLines = File.ReadAllLines(SpecialFileSettingsName);
                        int portNum;

                        if (fileLines.Length == 8 && int.TryParse(fileLines[2], out portNum))
                        {
                            m_settingsManager.Initialize(fileLines[0], fileLines[1], portNum);

                            //if everything went right, try to initialize the tracking service using found data
                            m_TrackingServiceCommunicator.InitializeAsync(m_settingsManager, TrackingServiceDiscoveryInitCompleted);

                            if (Log.IsDebugEnabled)
                            {
                                Log.Debug("TrackingServiceManagerBasic - Initialization with special file performed correctly");
                            }

                            return true;
                        }

                    }
                }
            }
            catch (Exception)
            {
                //do nothing, this just mean that something in the reading of the special file went wrong... just go on with a normal discovery
            }

            //if we are here, initialization using special file was not requested...
            return false;
        }

        /// <summary>
        /// Tries to discover and initialize the communication with the underlying tracking service.
        /// 
        /// If the function fails, it fails silently
        /// </summary>
        /// <param name="tryNum">Number of current try, 0 is for the first try, and so on</param>
        void DiscoveryAndInitializeTrackingServiceAsync(int tryNum)
        {
            if(Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManagerBasic {0} - DiscoveryAndInitializeTrackingServiceAsync called with try number {1}", m_settingsManager.TrackingServiceId, tryNum);
            }
            
            //if this is the first try
            if (tryNum == 0)
                switch (DiscoveryMode)
                {
                    //use saved settings at the first try
                    case TrackingServiceManagersDiscoveryMode.SettingsOnly:
                    case TrackingServiceManagersDiscoveryMode.SettingsThenDiscovery:

                        if (Log.IsDebugEnabled)
                        {
                            Log.Debug("TrackingServiceManagerBasic {0} - Performing initialization with Settings", m_settingsManager.TrackingServiceId);
                        }

                        m_TrackingServiceCommunicator.InitializeAsync(m_settingsManager, TrackingServiceDiscoveryInitCompleted);
                        break;

                    //use user values at the first try
                    case TrackingServiceManagersDiscoveryMode.UserValuesOnly:
                    case TrackingServiceManagersDiscoveryMode.UserValuesThenDiscovery:

                        if (Log.IsDebugEnabled)
                        {
                            Log.Debug("TrackingServiceManagerBasic {0} - Performing initialization with User values", m_settingsManager.TrackingServiceId);
                        }

                        m_settingsManager.Initialize(UserProvidedId, UserProvidedControlApiEndpoint, UserProvidedControlApiPort);
                        m_TrackingServiceCommunicator.InitializeAsync(m_settingsManager, TrackingServiceDiscoveryInitCompleted);
                        break;

                    //use autodiscovery at the first try
                    case TrackingServiceManagersDiscoveryMode.DiscoveryOnly:

                        if (Log.IsDebugEnabled)
                        {
                            Log.Debug("TrackingServiceManagerBasic {0} - Performing initialization with Discovery", m_settingsManager.TrackingServiceId);
                        }

                        m_TrackingServiceCommunicator.DiscoverAndInitializeAsync(m_settingsManager, TrackingServiceDiscoveryInitCompleted);
                        break;
                }

            //if this is the second try
            else if (tryNum == 1)
                switch (DiscoveryMode)
                {
                    //use autodiscovery at the second try
                    case TrackingServiceManagersDiscoveryMode.DiscoveryOnly:
                    case TrackingServiceManagersDiscoveryMode.SettingsThenDiscovery:
                    case TrackingServiceManagersDiscoveryMode.UserValuesThenDiscovery:

                        if (Log.IsDebugEnabled)
                        {
                            Log.Debug("TrackingServiceManagerBasic {0} - Performing initialization with Discovery", m_settingsManager.TrackingServiceId);
                        }

                        m_TrackingServiceCommunicator = new TrackingServiceDiscoveryCommunicator(this); //re-create it to remove spurious initialization
                        m_TrackingServiceCommunicator.DiscoverAndInitializeAsync(m_settingsManager, TrackingServiceDiscoveryInitCompleted);
                        break;
                }

        }

        /// <summary>
        /// Callback to be called when the process of discovery+initialization of the underlying Tracking Service finishes
        /// </summary>
        /// <param name="trackingServiceStatus">Resulting status of the discovery+initialization operation</param>
        /// <param name="trackingServiceInfo">Info about the found tracking service, if any (otherwise it is null)</param>
        protected override void TrackingServiceDiscoveryInitCompleted(TrackingServiceStatusResponse trackingServiceStatus, TrackingServiceInfo trackingServiceInfo)
        {
            base.TrackingServiceDiscoveryInitCompleted(trackingServiceStatus, trackingServiceInfo);

            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManagerBasic {0} - TrackingServiceDiscoveryInitCompleted called", m_settingsManager.TrackingServiceId);
            }
            
            //If everything went bad, try to init one more time
            //TODO: THIS LOOPS FOREVER UNTIL THE DISCOVERY FINDS A TRACKING SERVICE... IS THIS CORRECT?
            if (CurrentState == TrackingServiceManagerState.Error)
            {
                CurrentState = TrackingServiceManagerState.NotConnected; //re-start as we were not connected
                DiscoveryAndInitializeTrackingServiceAsync(1);
            }                
            //else, if everything wen well and autostart was requested, start StartTracking operations
            else if (AutoStartTracking)
                RequestTrackingStart();
        }

        #endregion

        #region Tracking Methods

        /// <summary>
        /// Request to the TrackingService to enter tracking mode.
        /// Request may not be fullfilled, e.g. because the system is already in tracking mode or because of webapi errors
        /// </summary>
        /// <exception cref="InvalidOperationException">If tracking service is not in idle stage</exception>
        public void RequestTrackingStart()
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManagerBasic - Start tracking request received");
            }

            StartNewOperativeStatus(TrackingServiceManagerState.Tracking, null);
        }

        /// <summary>
        /// Request to the TrackingService to exit tracking mode
        /// Request may not be fullfilled, e.g. because the system is already in another mode
        /// </summary>
        /// <exception cref="InvalidOperationException">If tracking service is not in tracking stage</exception>
        public void RequestTrackingStop()
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManagerBasic - Stop tracking request received");
            }

            StopCurrentOperativeStatus();
        }

        /// <summary>
        /// Performs operation consequent to a new operative status performed by the underlying tracking service
        /// </summary>
        /// <param name="statusStartResponse">Status of the request (to tell if the new status start was successful)</param>
        /// <param name="newStatus">New Status to activate on this object if the operations are successful</param>
        /// <exception cref="SystemException">In case of failure</exception>
        protected override void OperativeStartPrep(TrackingServiceStatusResponse statusStartResponse, TrackingServiceManagerState newStatus)
        {
            base.OperativeStartPrep(statusStartResponse, newStatus);

            //if everything went well, grab data to give outside
            if (!statusStartResponse.IsError)
            {
                TrackingServiceEnvironment = new TrackingServiceEnv()
                {
                    DataSources = statusStartResponse.Status.DataSources.Values.Select(dataSource => dataSource.UniqueId).ToList(),
                    MinDataSourcesForPlayer = statusStartResponse.Status.MinDataSourcesForPlay,
                    SceneDescriptor = statusStartResponse.Status.Scene
                };
            }
            
        }

        #endregion

        #region Data streams offering management methods

        /// <summary>
        /// Give the caller an object through which can receive the scene stream data at each frame.
        /// After the object has been used, the Dispose method should be called on the provided object
        /// If no Dispose gets called on the provided object, the stream gets closed at the end of the program.
        /// For more info, see <see cref="SceneDataProvider" /> class documentation
        /// </summary>
        /// <returns>
        /// Proxy object with properties exposing each frame updated info about scene data streaming, or null if the
        /// required stream does not exists. The stream regards ALWAYS the merged data stream, in world coordinates
        /// </returns>
        public SceneDataProvider StartSceneDataProvider()
        {
            return base.StartDataProvider(TrackingServiceConstants.MergedStreamName, TrackingServiceSceneDataStreamModes.WorldTransform);
        }

        #endregion

        #region Singleton-like implementation

        /// <summary>
        /// Gets a running instance of the TrackingServiceManagerBasic
        /// </summary>
        /// <returns>Instance of the TrackingServiceManagerBasic</returns>
        public static new TrackingServiceManagerBasic Instance
        {
            get
            {
                // Search an object of type TrackingServiceManagerBasic. If we find it, return it.
                // Otherwise, let's create a new gameobject, add a TrackingServiceManagerBasic to it and return it.
                var instance = FindObjectOfType<TrackingServiceManagerBasic>();

                if (instance != null)
                {
                    return instance;
                }

                var instanceGo = new GameObject();
                instanceGo.name = "Tracking Service Manager Basic";

                instanceGo.SetActive(false); //deactivate the object so we can set its properties before the Awake
                instance = instanceGo.AddComponent<TrackingServiceManagerBasic>();
                instance.AutoStartTracking = true;
                instance.DiscoveryMode = TrackingServiceManagersDiscoveryMode.SettingsThenDiscovery;
                instanceGo.SetActive(true); //re-activate the object 

                return instance;
            }
        }

        #endregion
    }
}

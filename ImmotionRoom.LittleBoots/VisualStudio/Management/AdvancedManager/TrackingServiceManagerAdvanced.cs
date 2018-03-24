namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedManager
{ 
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement;
    using UnityEngine;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.SupportStruct;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.DataStructures;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using ImmotionAR.ImmotionRoom.TrackingService.ControlClient.Model;
    using ImmotionAR.ImmotionRoom.DataSource.ControlClient.Model;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.DataStructures;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedManager.Reconfiguration;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.Common;
    using ImmotionAR.ImmotionRoom.TrackingService.ControlClient;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.DataSourcesManagement;
    using System.IO;
    using ImmotionAR.ImmotionRoom.AutoDiscovery.Model;

    /// <summary>
    /// Advanced tracking service manager, with the ability to enter calibration / diagnostic mode and to see all the streams
    /// of each data source
    /// </summary>
    public partial class TrackingServiceManagerAdvanced : TrackingServiceManager
    {
        #region Nested Classes

        /// <summary>
        /// Handles discovery and communication with the underlying TrackingService, with special features for the Advanced manager.
        /// 
        /// See TrackingServiceDiscoveryCommunicatorAdvanced.cs for the implementation
        /// </summary>
        protected partial class TrackingServiceDiscoveryCommunicatorAdvanced : TrackingServiceDiscoveryCommunicator
        {
            
        }

        /// <summary>
        /// Handles discovery and communication with the underlying Data Sources.
        /// 
        /// See DataSourcesDiscoveryCommunicator.cs for the implementation
        /// </summary>
        protected internal partial class DataSourcesDiscoveryCommunicator
        {

        }

        //TODO: QUESTA E TRACKINGSERVICESETTINGSMANAGER IN FONDO POTREBBERO ESSERE DUE FIGLIE DELLA STESSA CLASSE BASE
        /// <summary>
        /// Stores Data Sources settings, across multiple game sessions.
        /// These settings regard how the underlying data source services can be contacted
        /// 
        /// See DataSourcesSettingsManager.cs for the implementation
        /// </summary>
        protected internal partial class DataSourcesSettingsManager
        {
            
        }

        #endregion

        #region Public Events and Delegates

        /// <summary>
        /// Delegates of operations of discovering of network services
        /// </summary>
        /// <param name="eventArgs">Result of discovering operation</param>
        public delegate void DiscoveredServiceHandler(DiscoveredServiceEventArgs eventArgs);

        /// <summary>
        /// Delegates of operations of discovering of data sources services
        /// </summary>
        /// <param name="eventArgs">Result of discovering operation</param>
        public delegate void DiscoveredDataSourcesHandler(DiscoveredDataSourcesEventArgs eventArgs);
        
        /// <summary>
        /// Delegates of operations of reconfigured all services on a network (so they get to know each other)
        /// </summary>
        /// <param name="eventArgs">Result of reconfiguration operation</param>
        public delegate void ReconfiguredServicesHandler(ReconfiguredServicesEventArgs eventArgs);

        /// <summary>
        /// Delegates of operations of tracking system information requested
        /// </summary>
        /// <param name="eventArgs">Result of tracking system info request operation</param>
        public delegate void TrackingSystemInfoObtainedHandler(TrackingSystemInfoObtainedEventArgs eventArgs);
        
        /// <summary>
        /// Delegates of operations of generic simple operation requested to this advanced manager (e.g. set new master data source)
        /// </summary>
        /// <param name="eventArgs">Result of requested operation</param>
        public delegate void AdvancedOperationHandler(AdvancedOperationEventArgs eventArgs);

        /// <summary>
        /// Event triggered when a tracking service discovering operation terminates
        /// </summary>
        public event DiscoveredServiceHandler TrackingServiceDiscovered;

        /// <summary>
        /// Event triggered when a data source discovering operation terminates (notice that it gets triggered for each found datasource)
        /// </summary>
        public event DiscoveredServiceHandler DataSourceDiscovered;

        /// <summary>
        /// Event triggered when all data sources discovering operation terminates
        /// </summary>
        public event DiscoveredDataSourcesHandler DataSourcesDiscovered;

        /// <summary>
        /// Event triggered when all services on a network gets reconfigured, so they get to know each other 
        /// (all the data sources get to know the Tracking Service and vice versa)
        /// </summary>
        public event ReconfiguredServicesHandler NetworkReconfigured;

        /// <summary>
        /// Event triggered when set new girello / scene description operation terminates
        /// </summary>
        public event AdvancedOperationHandler NewGirelloDescriptionSet;

        /// <summary>
        /// Event triggered when set new master data source operation terminates
        /// </summary>
        public event AdvancedOperationHandler NewMasterDataSourceSet;

        /// <summary>
        /// Event triggered when a reboot data source / tracking service operation terminates
        /// </summary>
        public event AdvancedOperationHandler ServiceRebootCompleted;
        
        /// <summary>
        /// Event triggered when a start tracking request operation terminates
        /// </summary>
        public event AdvancedOperationHandler TrackingStarted;

        /// <summary>
        /// Event triggered when a start diagnostic mode request operation terminates
        /// </summary>
        public event AdvancedOperationHandler DiagnosticStarted;

        /// <summary>
        /// Event triggered when a start calibration request operation terminates
        /// </summary>
        public event AdvancedOperationHandler CalibrationStarted;

        /// <summary>
        /// Event triggered when a calibration step request operation terminates
        /// </summary>
        public event AdvancedOperationHandler CalibrationStepPerformed;

        /// <summary>
        /// Event triggered when a diagnostic / calibration / tracking status stop request operation terminates
        /// </summary>
        public event AdvancedOperationHandler OperativeStatusStopped;

        /// <summary>
        /// Event triggered when the tracking system status request gets fulfilled
        /// </summary>
        public event TrackingSystemInfoObtainedHandler SystemStatusRequestCompleted;

        #endregion

        #region Private fields

        /// <summary>
        /// Enables discovery and communication with underlying data sources
        /// </summary>
        protected DataSourcesDiscoveryCommunicator m_dataSourcesCommunicator;

        /// <summary>
        /// Holds references to data about found data sources during various sessions
        /// </summary>
        protected DataSourcesSettingsManager m_dataSourcesSettings;

        /// <summary>
        /// Object responsible to perform network reconfiguration (it is null if the system is not reconfigurating)
        /// </summary>
        NetworkReconfigurator m_networkReconfigurator;

        /// <summary>
        /// Last performed tracking system info (null if it has never been requested)
        /// </summary>
        TrackingSystemInfo m_lastTrackingSystemInfo;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets information about current tracking service
        /// </summary>
        public TrackingServiceInfo TrackingServiceInfo { get; internal set; }

        /// <summary>
        /// Gets information about current detected data services
        /// </summary>
        public DataSourceCollection DataSourcesInfo { get; internal set; }

        /// <summary>
        /// Get Tracking Service Environment (scene description, etc...)
        /// </summary>
        public TrackingServiceEnv TrackingServiceEnvironment { get; private set; }

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
        /// Gets if the TrackingServiceManager is calibrating the system (it is in calibration mode)
        /// </summary>
        public bool IsCalibrating
        {
            get
            {
                return CurrentState == TrackingServiceManagerState.Calibrating;
            }
        }

        /// <summary>
        /// Gets if the TrackingServiceManager is in diagnostic mode
        /// </summary>
        public bool IsInDiagnosticMode
        {
            get
            {
                return CurrentState == TrackingServiceManagerState.Diagnostic;
            }
        }

        /// <summary>
        /// Gets if the TrackingServiceManager is in diagnostic, calibration or tracking mode
        /// (i.e. in any state that can stream skeletons)
        /// </summary>
        public bool IsStreamingSkeletons
        {
            get
            {
                return CurrentState == TrackingServiceManagerState.Diagnostic ||
                       CurrentState == TrackingServiceManagerState.Calibrating ||
                       CurrentState == TrackingServiceManagerState.Tracking;
            }
        }

        #endregion

        #region Behavior Methods

        protected override void Awake()
        {
            base.Awake();

            //create the advanced tracking service communicator (the base class create the standard one, with limited functionalities)
            m_TrackingServiceCommunicator = new TrackingServiceDiscoveryCommunicatorAdvanced(this);

            //create the discoverer and communicator with underlying data sources
            m_dataSourcesCommunicator = new DataSourcesDiscoveryCommunicator(this);

            //create the settings manager for the data sources
            m_dataSourcesSettings = new DataSourcesSettingsManager();
        }

        protected override void OnDestroy()
        {
            //must call this for correct functioning of the behaviour
            base.OnDestroy();

            //if we were during a discovery, stop it (otherwise, it does nothing)
            m_dataSourcesCommunicator.StopDiscovery();
        }

        #endregion

        #region Network Discovery&Reconfig methods

        /// <summary>
        /// Asks the Tracking Service Manager to perform a discovery of all the tracking services and data sources on its local area network.
        /// When the discovery terminates, events TrackingServiceDiscovered, DataSourceDiscovered and DataSourcesDiscovered get triggered
        /// </summary>
        public void NetworkDiscoveryAsync()
        {
            //it is as we are still not connected
            CurrentState = TrackingServiceManagerState.NotConnected;

            //we are re-creating the whole network, so create two new communicators (the old ones are useless now)
            m_TrackingServiceCommunicator = new TrackingServiceDiscoveryCommunicatorAdvanced(this);
            m_dataSourcesCommunicator = new DataSourcesDiscoveryCommunicator(this);

            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManagerAdvanced - Network discovery requested");
            }

            //try to perform tracking service and data sources connection using special file: if the special file does not exist or if something goes wrong...
            if (!DiscoverServicesUsingSpecialFile())
            {
                //perform standard discovery
                m_TrackingServiceCommunicator.DiscoverAndInitializeAsync(m_settingsManager, TrackingServiceDiscoveryInitCompleted);
                m_dataSourcesCommunicator.DiscoverAndInitializeAsync(m_dataSourcesSettings, DataSourcesServiceDiscoveryInitCompleted);
            }
        }

        /// <summary>
        /// Asks the Tracking Service Manager to try to connect to all the tracking services and data sources saved as settings in a previous session.
        /// When the connection terminates, events TrackingServiceDiscovered, DataSourceDiscovered and DataSourcesDiscovered get triggered
        /// </summary>
        public void NetworkDiscoveryConnUsingSettingsAsync()
        {
            //it is as we are still not connected
            CurrentState = TrackingServiceManagerState.NotConnected;

            //we are re-creating the whole network, so create two new communicators (the old ones are useless now)
            m_TrackingServiceCommunicator = new TrackingServiceDiscoveryCommunicatorAdvanced(this);
            m_dataSourcesCommunicator = new DataSourcesDiscoveryCommunicator(this);

            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManagerAdvanced - Network discovery requested");
            }

            (m_TrackingServiceCommunicator as TrackingServiceDiscoveryCommunicatorAdvanced).InitializeeAsync(m_settingsManager, TrackingServiceDiscoveryInitCompleted);
            m_dataSourcesCommunicator.IntializeAsync(m_dataSourcesSettings, DataSourcesServiceDiscoveryInitCompleted);
        }

        /// <summary>
        /// Asks the Tracking Service Manager to perform a network reconfiguration of all the tracking services and data sources on its local area network.
        /// After this call, all services on the network get aware of each other.
        /// When the discovery terminates, event NetworkReconfigured gets triggered
        /// </summary>
        public void NetworkReconfigAsync()
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManagerAdvanced - Network reconfiguration requested");
            }

            //create the reconfigurator
            m_networkReconfigurator = new NetworkReconfigurator((m_TrackingServiceCommunicator.TrackingServiceInfo != null ? 1 : 0) + m_dataSourcesCommunicator.DataSourcesInfos.Length, NetworkReconfigurationCompleted);
            
            //trigger the reconfiguration of tracking service and wait for its completion
            (m_TrackingServiceCommunicator as TrackingServiceDiscoveryCommunicatorAdvanced).ReconfigAsync(m_networkReconfigurator.TrackingServiceReconfigCompletedCallback);

            //trigger the reconfiguration of data sources and wait for its completion
            m_dataSourcesCommunicator.ReconfigAsync(m_networkReconfigurator.DataSourceReconfigCompletedCallback);
        }

        #endregion

        #region Girello / Scene Description methods

        /// <summary>
        /// Asks the Tracking Service Manager to set a new girello of the tracking service
        /// When the operation terminates, event NewGirelloDescriptionSet gets triggered
        /// </summary>
        /// <param name="girelloCenter">Center of the girello game area</param>
        /// <param name="girelloSize">Size of the girello game area bounding box</param>
        /// <param name="girelloInnerAreaExtents">Extents (half of the size) of the girello inner game area bounding box</param>
        public void SetNewGirelloAsync(Vector3 girelloCenter, Vector3 girelloSize, Vector3 girelloInnerAreaExtents)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManagerAdvanced - Requested to change Girello with data {0}, {1}, {2}", girelloCenter, girelloSize, girelloInnerAreaExtents);
            }

            //ask to perform this operation to the appropriate communicator.
            //Notice that we pass only girello data and set the rest to default
            //TODO: IN THE FUTURE THIS HAS TO BE FIXED... WHY DON'T I USE ALL THE DATA AND SET THEM CORRECTLY
            // FURTHERMORE, SOME OF THIS DATA (LIKE FLOORCLIPPLANE) HAS TO BE COMPUTED BY CALIBRATION STAGE AND NOT BE PASSED BY THE USER
            (m_TrackingServiceCommunicator as TrackingServiceDiscoveryCommunicatorAdvanced).SetSceneDescriptor(
                new TrackingServiceSceneDescriptor() 
                {
                    FloorClipPlane = new TrackingService.DataClient.Model.TrackingServiceVector4(),
                    StageArea = new TrackingServiceSceneBoundaries(),
                    GameArea = new TrackingServiceSceneBoundaries() 
                        {
                            Center = new TrackingService.DataClient.Model.TrackingServiceVector3()
                                {
                                    X = girelloCenter.x, 
                                    Y = girelloCenter.y, 
                                    Z = girelloCenter.z
                                },
                            Size  = new TrackingService.DataClient.Model.TrackingServiceVector3()
                                {
                                    X = girelloSize.x, 
                                    Y = girelloSize.y, 
                                    Z = girelloSize.z
                                }
                        },
                    GameAreaInnerLimits = new TrackingService.DataClient.Model.TrackingServiceVector3()
                                {
                                    X = girelloInnerAreaExtents.x, 
                                    Y = girelloInnerAreaExtents.y, 
                                    Z = girelloInnerAreaExtents.z
                                }
                },
                TrackingServiceNewSceneDescriptorCompleted);
        }

        /// <summary>
        /// Callback called when the operation of setting a new scene descriptor finishes
        /// </summary>
        /// <param name="response">Result of the operation</param>
        private void TrackingServiceNewSceneDescriptorCompleted(TrackingService.ControlClient.Model.OperationResponse response)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManagerAdvanced - new scene descriptor operation has terminated with result: {0}", response.IsError ? response.ErrorDescription : "SUCCESS");
            }

            //trigger the event, notifying the registered listener about the end of this operation
            if (NewGirelloDescriptionSet != null)
            {
                var copiedEvent = NewGirelloDescriptionSet;

                ExecuteOnUI(() =>
                {
                    copiedEvent(new AdvancedOperationEventArgs() { ErrorString = response.IsError ? response.ErrorDescription : null, HumanReadableName = response.ID });
                });
            }

        }

        #endregion

        #region Set New Master methods

        /// <summary>
        /// Internal helper method, called whenever we got to know about a new master data source:
        /// essentially it spreads this knowledge from TrackingServiceInfo to DataSourcesInfo, keeping the data coherent
        /// </summary>
        private void UpdateInfosForNewMasterDataSource()
        {
            if (DataSourcesInfo == null || TrackingServiceInfo == null)
                return;

            //update the dictionary, setting master data source flag to true only for the master data source contained in TrackingServiceInfo
            List<string> keys = DataSourcesInfo.Keys.ToList();

            foreach(string key in keys)
            {
                if (TrackingServiceInfo.MasterDataSourceID != null && TrackingServiceInfo.MasterDataSourceID == key)
                    DataSourcesInfo[key].IsMaster = true;
                else
                    DataSourcesInfo[key].IsMaster = false;
            }
        }

        /// <summary>
        /// Asks the Tracking Service Manager to set a new master data source
        /// When the operation terminates, event NewMasterDataSourceSet gets triggered
        /// </summary>
        /// <param name="newMasterDataSourceID">Id of the new master data source</param>
        public void SetNewMasterDataSourceAsync(string newMasterDataSourceID)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManagerAdvanced - Requested {0} as new data source", newMasterDataSourceID);
            }

            //ask to perform this operation to the communicator
            (m_TrackingServiceCommunicator as TrackingServiceDiscoveryCommunicatorAdvanced).SetMasterDataSource(newMasterDataSourceID, NewMasterDataSourceSettingCompleted);

            //set new master data source inside private data            
            TrackingServiceInfo.MasterDataSourceID = newMasterDataSourceID;
            UpdateInfosForNewMasterDataSource();
        }

        /// <summary>
        /// Callback called when the operation of setting of new master data source finishes
        /// </summary>
        /// <param name="response">Result of the operation</param>
        private void NewMasterDataSourceSettingCompleted(TrackingService.ControlClient.Model.OperationResponse response)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManagerAdvanced - New master data source operation has terminated with result: {0}", response.IsError ? response.ErrorDescription : "SUCCESS");
            }

            //trigger the event, notifying the registered listener about the end of this operation
            if(NewMasterDataSourceSet != null)
            {
                var copiedEvent = NewMasterDataSourceSet;

                ExecuteOnUI(() =>
                {
                    copiedEvent(new AdvancedOperationEventArgs() { ErrorString = response.IsError ? response.ErrorDescription : null, HumanReadableName = TrackingServiceInfo.MasterDataSourceID });
                });
            }

            //if operation was not successful, reset master data source inside private data
            if (response.IsError)
            {
                TrackingServiceInfo.MasterDataSourceID = null;
                UpdateInfosForNewMasterDataSource();
            }

            //TODO: FIX THIS HACK PERFORMING A GET STATUS AFTER set master
            //(set master nulls always the calibration)
            (m_TrackingServiceCommunicator as TrackingServiceDiscoveryCommunicatorAdvanced).TrackingServiceInfo.IsCalibrated = false;
            TrackingServiceInfo.IsCalibrated = false;
           
        }

        #endregion

        #region Reboot Data Source methods

        /// <summary>
        /// Asks the Tracking Service Manager to reboot a data source or the tracking service
        /// When the operation terminates, event DataSourceRebootCompleted gets triggered
        /// </summary>
        /// <param name="serviceID">Id of the service to reboot</param>
        public void RebootServiceAsync(string serviceID)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManagerAdvanced - Requested to reboot data source {0}", serviceID);
            }

            //ask to perform this operation to the appropriate communicator
            if (TrackingServiceInfo.Id == serviceID)
                (m_TrackingServiceCommunicator as TrackingServiceDiscoveryCommunicatorAdvanced).RebootTrackingServiceAsync(TrackingServiceRebootingCompleted);
            else
                m_dataSourcesCommunicator.RebootDataSourceAsync(serviceID, DataSourceRebootingCompleted);
        }

        /// <summary>
        /// Callback called when the operation of rebooting a data source finishes
        /// </summary>
        /// <param name="response">Result of the operation</param>
        private void DataSourceRebootingCompleted(DataSource.ControlClient.Model.OperationResponse response)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManagerAdvanced - data source rebooting operation has terminated with result: {0}", response.IsError ? response.Error : "SUCCESS");
            }

            //trigger the event, notifying the registered listener about the end of this operation
            if (ServiceRebootCompleted != null)
            {
                var copiedEvent = ServiceRebootCompleted;

                ExecuteOnUI(() =>
                {
                    copiedEvent(new AdvancedOperationEventArgs() { ErrorString = response.IsError ? response.Error : null, HumanReadableName = response.ID });
                });
            }

        }

        /// <summary>
        /// Callback called when the operation of rebooting a tracking service finishes
        /// </summary>
        /// <param name="response">Result of the operation</param>
        private void TrackingServiceRebootingCompleted(TrackingService.ControlClient.Model.OperationResponse response)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManagerAdvanced - tracking service rebooting operation has terminated with result: {0}", response.IsError ? response.ErrorDescription : "SUCCESS");
            }

            //trigger the event, notifying the registered listener about the end of this operation
            if (ServiceRebootCompleted != null)
            {
                var copiedEvent = ServiceRebootCompleted;

                ExecuteOnUI(() =>
                {
                    copiedEvent(new AdvancedOperationEventArgs() { ErrorString = response.IsError ? response.ErrorDescription : null, HumanReadableName = response.ID });
                });
            }

        }

        #endregion

        #region Tracking Service Discovery&Initialization Methods

        /// <summary>
        /// Checks if special file with system settings was given by the user. If the answer is yes, connects to the provided 
        /// tracking service and local data source
        /// </summary>
        /// <returns>True if tracking service and data source discovery using special file was required and performed, false otherwise</returns>
        private bool DiscoverServicesUsingSpecialFile()
        {
            //check use of tracking service settings special file.
            //If this file is present (on PC), the system is considered all local.
            //The problem is that we can't perform a classical discovery on localhost, so we have to read this settings from file
            //and try to connect to a tracking service with this data
            try
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("TrackingServiceManagerAdvanced - Trying to find a valid Special file {0}", SpecialFileSettingsName);
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
                        int tsPortNum, dsCtrlPortNum, dsDataPortNum;

                        if (fileLines.Length == 8 && int.TryParse(fileLines[2], out tsPortNum) && int.TryParse(fileLines[5], out dsCtrlPortNum) && int.TryParse(fileLines[7], out dsDataPortNum))
                        {
                            //if everything went right, try to initialize the tracking service and data source using found data
                            m_settingsManager.Initialize(fileLines[0], fileLines[1], tsPortNum);
                            (m_TrackingServiceCommunicator as TrackingServiceDiscoveryCommunicatorAdvanced).InitializeeAsync(m_settingsManager, TrackingServiceDiscoveryInitCompleted);
                            m_dataSourcesSettings.Initialize(new Dictionary<string, DataSourceItem>() { { fileLines[3], new DataSourceItem() { Id = fileLines[3], ControlApiEndpoint = fileLines[4], ControlApiPort = dsCtrlPortNum, DataStreamerEndpoint = fileLines[6], DataStreamerPort = dsDataPortNum } } }); //set the data of the data source read from the file
                            m_dataSourcesCommunicator.IntializeAsync(m_dataSourcesSettings, DataSourcesServiceDiscoveryInitCompleted);

                            if (Log.IsDebugEnabled)
                            {
                                Log.Debug("TrackingServiceManagerAdvanced - Initialization with special file performed correctly");
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
        /// Callback to be called when the process of discovery+initialization of the underlying Tracking Service finishes
        /// </summary>
        /// <param name="TrackingServiceStatus">Resulting status of the discovery+initialization operation</param>
        /// <param name="trackingServiceInfo">Info about the found tracking service, if any (otherwise it is null)</param>
        protected override void TrackingServiceDiscoveryInitCompleted(TrackingServiceStatusResponse trackingServiceStatus, TrackingServiceInfo trackingServiceInfo)
        {
            base.TrackingServiceDiscoveryInitCompleted(trackingServiceStatus, trackingServiceInfo);

            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManagerAdvanced - TrackingServiceDiscoveryInitCompleted called");
            }

            //if everything went well, copy the data of the found tracking service
            //(note the use of deep copy, to take ownership of this data structure, so that if it gets modified outside, nothing changes here)
            if (!trackingServiceStatus.IsError)
            {
                TrackingServiceInfo = new TrackingServiceInfo
                {
                    ControlApiEndpoint = trackingServiceInfo.ControlApiEndpoint,
                    ControlApiPort = trackingServiceInfo.ControlApiPort,
                    DataStreamEndpoint = trackingServiceInfo.DataStreamEndpoint,
                    DataStreamPort = trackingServiceInfo.DataStreamPort,
                    FirstTimeSeen = trackingServiceInfo.FirstTimeSeen,
                    Id = trackingServiceInfo.Id,
                    IsReachable = trackingServiceInfo.IsReachable,
                    LastSeen = trackingServiceInfo.LastSeen,
                    MasterDataSourceID = trackingServiceInfo.MasterDataSourceID,
                    IsCalibrated = trackingServiceInfo.IsCalibrated
                };

                UpdateInfosForNewMasterDataSource(); //we have a new master, update info
            }

            //trigger the tracking source found event
            if (TrackingServiceDiscovered != null)
            {
                var copiedEvent = TrackingServiceDiscovered;

                if (trackingServiceStatus.IsError)
                    copiedEvent(new DiscoveredServiceEventArgs()
                    {
                        ErrorString = trackingServiceStatus.ErrorDescription,
                        HumanReadableName = trackingServiceInfo != null ? trackingServiceInfo.Id : "",
                        DataIpPort = trackingServiceInfo != null ? string.Format("{0}:{1}", trackingServiceInfo.ControlApiEndpoint, trackingServiceInfo.ControlApiPort) : ""
                    });
                else
                    copiedEvent(new DiscoveredServiceEventArgs()
                    {
                        ErrorString = null,
                        HumanReadableName = trackingServiceInfo.Id,
                        DataIpPort = string.Format("{0}:{1}", trackingServiceInfo.ControlApiEndpoint, trackingServiceInfo.ControlApiPort)
                    });
            }

        }

        /// <summary>
        /// Callback to be called when the process of discovery+initialization of the underlying Data sources finishes
        /// </summary>
        /// <param name="TrackingServiceStatus">Resulting status of the discovery+initialization operation, with all data sources data</param>
        protected virtual void DataSourcesServiceDiscoveryInitCompleted(DataSourceServiceStatusResponse dataSourceServiceStatus)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManagerAdvanced - DataSourcesServiceDiscoveryInitCompleted called");
            }

            //If everything went well, we're in idle state, otherwise we're in error state
            if (dataSourceServiceStatus.IsError)
                CurrentState = TrackingServiceManagerState.Error;
            else if (CurrentState == TrackingServiceManagerState.NotConnected) //modify status only from not connected to idle
                CurrentState = TrackingServiceManagerState.Idle;

            //if everything went well
            if (!dataSourceServiceStatus.IsError)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("TrackingServiceManagerAdvanced - All Data Sources successfully discovered");
                }

                //copy the data of the found data services
                //(note the use of deep copy, to take ownership of this data structure, so that if it gets modified outside, nothing changes here)
                //(and, there are two classes called DataSourceInfo, so we must here copy the data from one type to the other)
                DataSourcesInfo = new DataSourceCollection();

                foreach(var dataSourceInfoPair in dataSourceServiceStatus.Status.DataSources)
                {
                    DataSourcesInfo.Add(dataSourceInfoPair.Value.Id, new ImmotionAR.ImmotionRoom.LittleBoots.Management.DataStructures.DataSourceInfo
                    {
                        ControlApiEndpoint = dataSourceInfoPair.Value.ControlApiEndpoint,
                        ControlApiPort = dataSourceInfoPair.Value.ControlApiPort,
                        DataStreamEndpoint = dataSourceInfoPair.Value.DataStreamEndpoint,
                        DataStreamPort = dataSourceInfoPair.Value.DataStreamPort,
                        FirstTimeSeen = dataSourceInfoPair.Value.FirstTimeSeen,
                        Id = dataSourceInfoPair.Value.Id,
                        IsMaster = false, //at the moment we don't know who the master is
                        IsReachable = dataSourceInfoPair.Value.IsReachable,
                        LastSeen = dataSourceInfoPair.Value.LastSeen,
                        UniqueId = 0 //we don't know it
                    });

                    //in the meanwhile, trigger the data source found event
                    if(DataSourceDiscovered != null)
                    {
                        var copiedEvent = DataSourceDiscovered;
                        copiedEvent(new DiscoveredServiceEventArgs() { ErrorString = null, HumanReadableName = dataSourceInfoPair.Value.Id, DataIpPort = string.Format("{0}:{1}", dataSourceInfoPair.Value.DataStreamEndpoint, dataSourceInfoPair.Value.DataStreamPort) });
                    }

                }

                UpdateInfosForNewMasterDataSource(); //we have new data sources... we should update info to set which one is the master (using data found for the tracking service)

                //trigger the data sources found event
                if(DataSourcesDiscovered != null)
                {
                    var copiedEvent = DataSourcesDiscovered;

                    //put objects in the right format
                    string[] dataSourcesNames = new string[DataSourcesInfo.Count];
                    string[] dataSourcesIpPorts = new string[DataSourcesInfo.Count];

                    int idx = 0;

                    foreach(var dataSourceInfoPair in DataSourcesInfo)
                    {
                        dataSourcesNames[idx] = dataSourceInfoPair.Key;
                        dataSourcesIpPorts[idx++] = string.Format("{0}:{1}", dataSourceInfoPair.Value.DataStreamEndpoint, dataSourceInfoPair.Value.DataStreamPort);
                    }

                    //trigger the event
                    copiedEvent(new DiscoveredDataSourcesEventArgs() { ErrorString = null, HumanReadableNames = dataSourcesNames, DataIpPorts = dataSourcesIpPorts });
                }
            }
            //else, if something went wrong, return an event with an error description
            else
            {
                if (DataSourceDiscovered != null)
                {
                    var copiedEvent = DataSourceDiscovered;
                    copiedEvent(new DiscoveredServiceEventArgs() { ErrorString = dataSourceServiceStatus.Error, HumanReadableName = "", DataIpPort = ""});
                }

                if(DataSourcesDiscovered != null)
                {
                    var copiedEvent = DataSourcesDiscovered;
                    copiedEvent(new DiscoveredDataSourcesEventArgs() { ErrorString = dataSourceServiceStatus.Error, HumanReadableNames = null, DataIpPorts = null });
                }
            }
        }

        #endregion

        #region Network Reconfiguration Methods

        /// <summary>
        /// Callback called when the network reconfigurator finishes its async operation
        /// </summary>
        /// <param name="eventArgs">Result of the network reconfig operation</param>
        private void NetworkReconfigurationCompleted(ReconfiguredServicesEventArgs eventArgs)
        {
            if (eventArgs.ErrorString != null)
                CurrentState = TrackingServiceManagerState.Error;

            ExecuteOnUI(() =>
                {
                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("TrackingServiceManagerAdvanced - Network reconfiguration completed with status {0}", eventArgs.ErrorString ?? "SUCCESS");
                    }

                    //trigger the event // Gianni FIX
                    var copiedEvent = NetworkReconfigured;
                    if (copiedEvent != null)
                    {
                        copiedEvent(eventArgs);
                    }

                    //kill the re-configurator (we don't need it anymore)
                    m_networkReconfigurator = null;
                });
        }

        #endregion

        #region Operations and Statuses methods

        /// <summary>
        /// Request the tracking service to exit from any non-idle status it is in (i.e. diagnostic, calibration, merging).
        /// The caller can check the success of the operation checking the IsIdle flag in polling or listening for the
        /// OperativeStatusStopped event
        /// </summary>
        /// <exception cref="InvalidOperationException">If tracking service is in idle or invalid stage</exception>
        public void RequestCurrentOperativeStatusStop()
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManagerAdvanced - Stop Current Operative Status request received");
            }

            //use base methods to stop current operative status
            base.StopCurrentOperativeStatus();
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
        /// <exception cref="SystemException">In case of failure</exception>
        protected override void OperativeStatusStart(string operationTagName, ImmotionAR.ImmotionRoom.TrackingService.ControlClient.Model.OperationResponse trackingServiceResultStatus, TrackingServiceManagerState newStatus)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManagerAdvanced - Start {0} Operative Status request completed", operationTagName);
            }

            //call base methods
            base.OperativeStatusStart(operationTagName, trackingServiceResultStatus, newStatus);

            //if we failed, trigger failing event. If we succeed, wait the call to OperativeStartPrep to see if we have actually succeded (at this point
            //there is still a call to GetStatus to make, so we're not sure we're activated the new status)
            if (trackingServiceResultStatus.IsError)
                RaiseOperativeStartEvent(newStatus, trackingServiceResultStatus.ErrorDescription);
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

            //if everything went well, grab data of environment to give outside
            if (!statusStartResponse.IsError)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("TrackingServiceManagerAdvanced - Reading data about the Tracking Environment");
                }

                TrackingServiceEnvironment = new TrackingServiceEnv()
                {
                    DataSources = statusStartResponse.Status.DataSources.Values.Select(dataSource => dataSource.UniqueId).ToList(),
                    MinDataSourcesForPlayer = statusStartResponse.Status.MinDataSourcesForPlay,
                    SceneDescriptor = statusStartResponse.Status.Scene
                };
                
            }

            //anyway, trigger finish operation event
            RaiseOperativeStartEvent(newStatus, statusStartResponse.IsError ? statusStartResponse.ErrorDescription : null);
        }

        /// <summary>
        /// Raise the event relative to the finish of the request operation of new status start
        /// </summary>
        /// <param name="newStatus">New status that has been started</param>
        /// <param name="errorString">Error string of the operation: if it is null, no error occurred</param>
        void RaiseOperativeStartEvent(TrackingServiceManagerState newStatus, string errorString)
        {
            //select the appropriate event of status start depending ot the type of status started
            AdvancedOperationHandler copiedEvent;

            switch (newStatus)
            {
                case TrackingServiceManagerState.Diagnostic:
                    copiedEvent = DiagnosticStarted;
                    break;

                case TrackingServiceManagerState.Calibrating:
                    copiedEvent = CalibrationStarted;
                    break;

                case TrackingServiceManagerState.Tracking:
                    copiedEvent = TrackingStarted;
                    break;

                default:
                    copiedEvent = null;
                    break;
            }

            ExecuteOnUI(() =>
            {
                //trigger the event with the result of the operation
                if (copiedEvent != null)
                    copiedEvent(new AdvancedOperationEventArgs() { HumanReadableName = TrackingServiceInfo.Id, ErrorString = errorString });
            });
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
        /// <exception cref="SystemException">In case of failure</exception>
        protected override void OperativeStatusStop(string operationTagName, ImmotionAR.ImmotionRoom.TrackingService.ControlClient.Model.OperationResponse trackingServiceResultStatus)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManagerAdvanced - Stop {0} Operative Status request completed", operationTagName);
            }

            if (!trackingServiceResultStatus.IsError)
                //TODO: FIX THIS HACK PERFORMING A GET STATUS AFTER CALIBRATION
                //set that calibration has been made
                if (CurrentState == TrackingServiceManagerState.Calibrating)
                {
                    (m_TrackingServiceCommunicator as TrackingServiceDiscoveryCommunicatorAdvanced).TrackingServiceInfo.IsCalibrated = true;
                    TrackingServiceInfo.IsCalibrated = true;
                }

            //call base methods
            base.OperativeStatusStop(operationTagName, trackingServiceResultStatus);

            //trigger the appropriate event of status stop
            //select the appropriate event of status start depending ot the type of status started
            AdvancedOperationHandler copiedEvent = OperativeStatusStopped;

            ExecuteOnUI(() =>
                {
                    //trigger the event with the result of the operation
                    if (copiedEvent != null)
                        copiedEvent(new AdvancedOperationEventArgs() { HumanReadableName = TrackingServiceInfo.Id, ErrorString = trackingServiceResultStatus.IsError ? trackingServiceResultStatus.ErrorDescription : null });
                });
        }

        #endregion

        #region Tracking Methods

        /// <summary>
        /// Request to the TrackingService to enter tracking mode.
        /// Request may not be fullfilled, e.g. because the system is already in tracking mode or because of webapi errors.
        /// The caller can check the success of the operation checking the IsTracking flag in polling or listening for the
        /// TrackingStarted event
        /// </summary>
        /// <exception cref="InvalidOperationException">If tracking service is not in idle stage</exception>
        public void RequestTrackingStart()
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManagerAdvanced - Start tracking request received");
            }

            StartNewOperativeStatus(TrackingServiceManagerState.Tracking, null);
        }

        #endregion

        #region Diagnostic Methods

        /// <summary>
        /// Request to the TrackingService to enter diagnostic mode.
        /// Request may not be fullfilled, e.g. because the system is already in diagnostic mode or because of webapi errors.
        /// The caller can check the success of the operation checking the IsInDiagnosticMode flag in polling or listening for the
        /// DiagnosticStarted event
        /// </summary>
        /// <exception cref="InvalidOperationException">If tracking service is not in idle stage</exception>
        public void RequestDiagnosticModeStart()
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManagerAdvanced - Start diagnostic mode request received");
            }

            StartNewOperativeStatus(TrackingServiceManagerState.Diagnostic, null);
        }

        #endregion

        #region Calibration Methods

        /// <summary>
        /// Request to the TrackingService to enter calibration mode.
        /// Request may not be fullfilled, e.g. because the system is already in calibration mode or because of webapi errors.
        /// The caller can check the success of the operation checking the IsCalibrating flag in polling or listening for the
        /// CalibrationStarted event
        /// </summary>
        /// <param name="calibrationParams">Calibration parameters</param>
        /// <exception cref="InvalidOperationException">If tracking service is not in idle stage</exception>
        public void RequestCalibrationStart(CalibrationParameters calibrationParams)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManagerAdvanced - Start calibration request received");
            }

            StartNewOperativeStatus(TrackingServiceManagerState.Calibrating, calibrationParams); //remember to pass calibration parameters
        }

        /// <summary>
        /// Request the start of a new calibration step.
        /// When the operation is performed (successfully or with failure), the event CalibrationStepPerformed is triggered with
        /// the operation results
        /// </summary>
        /// <param name="calibrationParams">Parameters of calibration, including the calibration step requested</param>
        /// <exception cref="InvalidOperationException">In case of the system is not in calibration state or the params are invalid</exception>
        public void RequestCalibrationStepStart(CalibrationParameters calibrationParams)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManagerAdvanced - Start calibration request received");
            }

            //check that we are in idle state to start a new state
            if (CurrentState != TrackingServiceManagerState.Calibrating)
            {
                if (Log.IsErrorEnabled)
                {
                    Log.Error("TrackingServiceManagerAdvanced for Tracking service {0} - Requested to start a new calibration step from a tracking service in non-calibration state", TrackingServiceInfo.Id);
                }

                throw new InvalidOperationException("Requested to start a new calibration step from a tracking service in non-calibration state");
            }

            if (calibrationParams == null)
            {
                if (Log.IsErrorEnabled)
                {
                    Log.Error("TrackingServiceManagerAdvanced for Tracking service {0} - Requested to start calibration step without calibration parameters", TrackingServiceInfo.Id);
                }

                throw new ArgumentException("Requested to start calibration step without calibration parameters");
            }

            //request calibration start and then call the OperativeStatusStart callback to perform all operations
            (m_TrackingServiceCommunicator as TrackingServiceDiscoveryCommunicatorAdvanced).TrackingServiceController.StartCalibrationAsync(calibrationParams, result => CalibrationStepStarting(calibrationParams, result));
        }

        /// <summary>
        /// Private callback called when the underlying tracking service has answered to the request of the start of a new calibration stage
        /// </summary>
        /// <param name="calibrationParams">Calibration parameters</param>
        /// <param name="trackingServiceResultStatus">Status of the request (to tell if the operation was successful)</param>
        private void CalibrationStepStarting(CalibrationParameters calibrationParams, ImmotionAR.ImmotionRoom.TrackingService.ControlClient.Model.OperationResponse trackingServiceResultStatus)
        {
            ExecuteOnUI(() =>
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("TrackingServiceManagerAdvanced - Calibration step {0} Start. Result is {1}", calibrationParams.Step.ToString(), trackingServiceResultStatus.IsError ? string.Format("Failure: {0}", trackingServiceResultStatus.ErrorDescription) : "Success");
                }

                //if operation start request has been processed well
                if (!trackingServiceResultStatus.IsError)
                {
                    //get tracking service status to see if the request has been accepted and perfomed 
                    (m_TrackingServiceCommunicator as TrackingServiceDiscoveryCommunicatorAdvanced).TrackingServiceController.GetStatusAsync(result3 => ExecuteOnUI(() =>
                    {
                        if (Log.IsDebugEnabled)
                        {
                            Log.Debug("TrackingServiceManagerAdvanced - {0} Step Get Status. Result is {1}", calibrationParams.Step.ToString(), trackingServiceResultStatus.IsError ? string.Format("Failure: {0}", trackingServiceResultStatus.ErrorDescription) : "Success");
                        }

                        //set status accordingly to the error status
                        if(result3.IsError)
                            CurrentState = TrackingServiceManagerState.Error;

                        //trigger the new calibration step performed event 
                        if (CalibrationStepPerformed != null)
                        {
                            var copiedEvent = CalibrationStepPerformed;

                            copiedEvent(new AdvancedOperationEventArgs() { HumanReadableName = TrackingServiceInfo.Id, ErrorString = result3.IsError ? result3.ErrorDescription : null });
                        }
                    }));
                }
                //else, if request went bad, now Tracking Service is in error state. 
                else
                {
                    CurrentState = TrackingServiceManagerState.Error;

                    if (Log.IsErrorEnabled)
                    {
                        Log.Error("TrackingServiceManagerAdvanced - {0} Step Start FAILED", calibrationParams.Step.ToString());
                    }

                    //trigger the new calibration step performed event (to signal error)
                    if (CalibrationStepPerformed != null)
                    {
                        var copiedEvent = CalibrationStepPerformed;

                        copiedEvent(new AdvancedOperationEventArgs() { HumanReadableName = TrackingServiceInfo.Id, ErrorString = trackingServiceResultStatus.IsError ? trackingServiceResultStatus.ErrorDescription : null });
                    }
                }

            });

        }

        #endregion

        #region Status Request Methods

        /// <summary>
        /// Asks the Tracking Service Manager to ask the underlying tracking service and data sources their current status
        /// When the operation terminates, event SystemStatusRequestCompleted gets triggered
        /// </summary>
        public void GetSystemStatusAsync()
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManagerAdvanced - Requested to obtain current status");
            }

            //ask to perform this operation to the appropriate communicator
            (m_TrackingServiceCommunicator as TrackingServiceDiscoveryCommunicatorAdvanced).GetTrackingServiceStatusAsync(GetTrackingServiceStatusCompleted);
        }

        /// <summary>
        /// Callback called when the operation of obtain status of tracking service gets completed
        /// </summary>
        /// <param name="response">Result of the operation</param>
        private void GetTrackingServiceStatusCompleted(TrackingServiceStatusResponse response)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManagerAdvanced - tracking service status grabbing operation has terminated with result: {0}", response.IsError ? response.ErrorDescription : "SUCCESS");
            }

            //fill the data of tracking system info, mixing infos from the current Tracking Service Info and the ones
            //arrived from the status request
            m_lastTrackingSystemInfo = new TrackingSystemInfo();
            m_lastTrackingSystemInfo.TrackingServiceId = TrackingServiceInfo.Id;
            m_lastTrackingSystemInfo.ControlApiEndpoint = TrackingServiceInfo.ControlApiEndpoint;
            m_lastTrackingSystemInfo.ControlApiPort = TrackingServiceInfo.ControlApiPort;
            m_lastTrackingSystemInfo.IsReachable = response.Status != null;            
            //if the request has been successful, get all the info regarding the tracking service and the data source from its data
            //then try to obtain info for data sources too
            if (!response.IsError && response.Status != null)
            {
                m_lastTrackingSystemInfo.CurrentState = response.Status.CurrentState;
                m_lastTrackingSystemInfo.CalibrationDone = response.Status.CalibrationDone;
                m_lastTrackingSystemInfo.MasterDataSource = response.Status.MasterDataStreamer;
                m_lastTrackingSystemInfo.Version = response.Status.Version;

                //add info about the data sources, getting them from the status
                m_lastTrackingSystemInfo.DataSourcesInfo = new TrackingSystemInfo.TrackingSystemDataSourceInfo[response.Status.DataSources.Count];

                int idx = 0;

                foreach (var dataSourcePair in response.Status.DataSources)
                {
                    m_lastTrackingSystemInfo.DataSourcesInfo[idx] = new TrackingSystemInfo.TrackingSystemDataSourceInfo()
                    {
                        DataSourceId = dataSourcePair.Key,
                        ControlApiEndpoint = dataSourcePair.Value.ControlApiEndpoint,
                        ControlApiPort = dataSourcePair.Value.ControlApiPort
                    };

                    idx++;
                }

                //see if the data sources are reachable.
                //The get status on the tracking service has told us only which data sources are connected to the tracking service,
                //but now we want to know which one of them is currently reachable, and so we have to probe them through
                //the data sources communicator

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("TrackingServiceManagerAdvanced - launching get status on data sources to see if they are reachable");
                }

                m_dataSourcesCommunicator.GetDataSourcesStatusAsync(GetDataSourcesStatusCompleted);
            }
            //else, in case of error, trigger the status request operation finish event, returning a non-reachable tracking service
            else if (SystemStatusRequestCompleted != null)
            {
                var copiedEvent = SystemStatusRequestCompleted;

                ExecuteOnUI(() =>
                {
                    copiedEvent(new TrackingSystemInfoObtainedEventArgs() { TrackingSystemInformations = m_lastTrackingSystemInfo });
                });
            }

        }

        /// <summary>
        /// Callback called when the operation of obtain status of data sources complets
        /// </summary>
        /// <param name="response">Result of the operations, one for each data source for which the status has been requested</param>
        private void GetDataSourcesStatusCompleted(DataSourceServiceStatusResponse[] response)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("TrackingServiceManagerAdvanced - get status on data sources operation has terminated");
            }

            //loop for all the data sources that have answered and adjust the status accordingly in the tracking system info structure
            foreach (DataSourceServiceStatusResponse dataSourceResponse in response)
            {
                Debug.Log("DDD " + dataSourceResponse.ID);
                //search the data source it correspons to
                TrackingSystemInfo.TrackingSystemDataSourceInfo dsInfo = m_lastTrackingSystemInfo.DataSourcesInfo.FirstOrDefault(dataSourceInfo => (dataSourceInfo.DataSourceId == dataSourceResponse.ID));

                //if we find it, adjust its data
                if (dsInfo != null)
                {
                    dsInfo.IsReachable = !dataSourceResponse.IsError && dataSourceResponse.Status != null;

                    if (dsInfo.IsReachable)
                    {
                        dsInfo.CurrentState = dataSourceResponse.Status.CurrentState;
                        dsInfo.Version = dataSourceResponse.Status.Version;
                    }
                }

            }

            //trigger the get status completion event
            if (SystemStatusRequestCompleted != null)
            {
                var copiedEvent = SystemStatusRequestCompleted;

                ExecuteOnUI(() =>
                {
                    copiedEvent(new TrackingSystemInfoObtainedEventArgs() { TrackingSystemInformations = m_lastTrackingSystemInfo });
                });
            }
        }

        #endregion

        #region Misc Methods

        /// <summary>
        /// Force current state of the Manager to Idle.
        /// This is useful to recover from non-fatal errors
        /// </summary>
        public void ForceStateToIdle()
        {
            CurrentState = TrackingServiceManagerState.Idle;
        }

        #endregion

        #region Data streams offering management methods

        /// <summary>
        /// Give the caller an object through which can receive the scene stream data at each frame.
        /// After the object has been used, the Dispose method should be called on the provided object
        /// If no Dispose gets called on the provided object, the stream gets closed at the end of the program.
        /// For more info, see <see cref="SceneDataProvider" /> class documentation
        /// </summary>
        /// <param name="streamerInfoId">ID of the body data streamer of interest</param>
        /// <param name="streamingMode">Streaming mode of interest</param>
        /// <returns>
        /// Proxy object with properties exposing each frame updated info about scene data streaming, or null if the
        /// required stream does not exists.
        /// </returns>
        public SceneDataProvider StartSceneDataProvider(string streamerInfoId, TrackingServiceSceneDataStreamModes streamingMode)
        {
            return base.StartDataProvider(streamerInfoId, streamingMode);
        }

        #endregion

        #region Singleton-like implementation

        //inspiration from http://wiki.unity3d.com/index.php?title=Singleton

        /// <summary>
        /// True if the application is quitting, false otherwise
        /// </summary>
        private static bool m_applicationIsQuitting = false;

        /// <summary>
        /// Singleton instance
        /// </summary>
        private static TrackingServiceManagerAdvanced m_instance = null;

        void OnApplicationQuit()
        {
            m_applicationIsQuitting = true;
        }

        /// <summary>
        /// Gets the first running instance of the TrackingServiceManagerAdvanced. If it does not exists, creates an instance of the 
        /// <see cref="TrackingServiceManagerAdvanced"/> class
        /// </summary>
        /// <returns>Instance of the TrackingServiceManagerAdvanced</returns>
        public static new TrackingServiceManagerAdvanced Instance
        {
            get
            {
                //if we already have an instance, return it
                if (m_instance != null)
                {
                    return m_instance;
                }
                else if (m_applicationIsQuitting) //can't allocate objects during a destroy operation
                    return null;

                var instanceGo = new GameObject();
                instanceGo.name = "Tracking Service Manager Advanced";

                instanceGo.SetActive(false); //deactivate the object so we can set its properties before the Awake
                m_instance = instanceGo.AddComponent<TrackingServiceManagerAdvanced>();
                instanceGo.AddComponent<DoNotDestroy>(); //won't destroy forever
                instanceGo.SetActive(true); //re-activate the object 

                return m_instance;
            }
        }

        #endregion
    }
}

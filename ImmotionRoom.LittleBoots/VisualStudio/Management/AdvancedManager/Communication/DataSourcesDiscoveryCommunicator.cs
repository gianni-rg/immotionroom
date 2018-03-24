namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using ImmotionAR.ImmotionRoom.AutoDiscovery;
    using ImmotionAR.ImmotionRoom.TrackingService.ControlClient;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using ImmotionAR.ImmotionRoom.Helpers;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.Common;
    using ImmotionAR.ImmotionRoom.TrackingService.ControlClient.Model;
    using ImmotionAR.ImmotionRoom.DataSource.ControlClient.Model;
    using ImmotionAR.ImmotionRoom.DataSource.ControlClient;
    using ImmotionAR.ImmotionRoom.AutoDiscovery.Model;
    using System.Threading;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement;

    public partial class TrackingServiceManagerAdvanced : TrackingServiceManager
    {

        /// <summary>
        /// Handles discovery and communication with the underlying DataSources
        /// </summary>
        protected internal partial class DataSourcesDiscoveryCommunicator
        {
            #region Delegates definition

            /// <summary>
            /// Signature of method to be called when the data sources discovery is completed and this object has been initialized.
            /// The parameter sets if the discovery and / or initialization went good or bad
            /// </summary>
            internal delegate void DataSourcesDiscoveryInitCompleted(DataSourceServiceStatusResponse dataSourceStatus);

            /// <summary>
            /// Signature of method to be called when the data sources get status is completed.
            /// </summary>
            internal delegate void DataSourcesGetStatusCompleted(DataSourceServiceStatusResponse[] dataSourcesStatus);

            #endregion

            #region Private Fields

            /// <summary>
            /// Tracking Service enclosing this instance
            /// </summary>
            private TrackingServiceManagerAdvanced m_outerTrackingService;

            /// <summary>
            /// Discovers data sources
            /// </summary>
            private DataSourceDiscoverer m_dataSourcesDiscoverer;

            /// <summary>
            /// Array of objects enabling communication with underlying data sources
            /// </summary>
            private DataSourceControlClient[] m_dataSourcesWebApiClients;

            /// <summary>
            /// Info about the found data sources
            /// </summary>
            private DataSourceInfo[] m_dataSourcesInfos;

            /// <summary>
            /// Reference to the Data Sources Settings manager provided by the user
            /// </summary>
            protected DataSourcesSettingsManager m_dataSourcesSettingsManager;

            /// <summary>
            /// Array of objects to handle parallel for loop on data sources
            /// </summary>
            private EventWaitHandle[] m_synchronizationStruct;

            /// <summary>
            /// Reference to the Data Source init completed callback provided by the user
            /// </summary>
            private DataSourcesDiscoveryInitCompleted m_dataSourcesInitCompletedCallback;

            /// <summary>
            /// Holds references to the status of the data sources, computed during a call to method <see cref="GetDataSourcesStatusAsync"/>
            /// </summary>
            private DataSourceServiceStatusResponse[] m_lastDataSourcesGetStatusResults;

            #endregion

            #region Internal properties

            /// <summary>
            /// Gets the objects that actually communicate with the underlying data sources.
            /// If this value is null, this object hasn't been correctly initialized yet
            /// </summary>
            internal DataSourceControlClient[] DataSourcesControllers
            {
                get
                {
                    return m_dataSourcesWebApiClients;
                }
            }

            /// <summary>
            /// Gets info about the managed data sources on the network
            /// </summary>
            internal DataSourceInfo[] DataSourcesInfos
            {
                get
                {
                    return m_dataSourcesInfos;
                }
            }

            #endregion

            #region Constructor

            /// <summary>
            /// Construct a communication object with the Tracking Service
            /// </summary>
            /// <param name="outerInstance">Enclosing instance</param>
            internal DataSourcesDiscoveryCommunicator(TrackingServiceManagerAdvanced outerInstance)
            {
                m_outerTrackingService = outerInstance;
            }

            #endregion

            #region Discovery Methods

            /// <summary>
            /// Performs Data sources Discovery, to find them and start communication with them. Then initialize all internal data.
            /// After that, calls the provided callback.
            /// </summary>
            /// <param name="settingsManager">Manager of the settings of the Data Sources</param>
            /// <param name="discoveryCompletedCallback">Callback to be called when the discovery+initialization operation gets completed</param>
            /// <exception cref="InvalidOperationException">If this object was already been initialized</exception>
            internal void DiscoverAndInitializeAsync(DataSourcesSettingsManager settingsManager, DataSourcesDiscoveryInitCompleted discoveryInitCompletedCallback)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("DataSourcesDiscoveryCommunicator - Starting DataSources discovery");
                }

                //throw exception if this object is already initialized
                if (m_dataSourcesWebApiClients != null)
                {
                    if (Log.IsErrorEnabled)
                    {
                        Log.Error("DataSourcesDiscoveryCommunicator - Trying To Discover and Initialize an already initialized object");
                    }

                    throw new InvalidOperationException("Can't initialize an already initialized DataSourcesDiscoveryCommunicator");
                }

                m_dataSourcesSettingsManager = settingsManager;
                m_dataSourcesInitCompletedCallback = discoveryInitCompletedCallback;

                //load default settings for the auto discovery operation, then get local IP data
                var autoDiscoverySettings = AutoDiscoverySettings.Default;
                autoDiscoverySettings.LocalAddress = NetworkTools.GetLocalIpAddress().ToString();
                autoDiscoverySettings.LocalPort = AutoDiscoveryDefaultSettings.DataSourceAutoDiscoveryLocalPort;

                //start an async discovery operation
                m_dataSourcesDiscoverer = new DataSourceDiscoverer(autoDiscoverySettings);
                m_dataSourcesDiscoverer.DiscoveryCompleted += Discoverer_OnDiscoveryCompleted;
                m_dataSourcesDiscoverer.StartDataSourcesDiscoveryAsync();
            }

            /// <summary>
            /// Stops current discovery, if any
            /// </summary>
            internal void StopDiscovery()
            {
                if (m_dataSourcesDiscoverer != null)
                {
                    m_dataSourcesDiscoverer.DiscoveryCompleted -= Discoverer_OnDiscoveryCompleted;
                    m_dataSourcesDiscoverer.StopDataSourcesDiscovery();
                }
            }


            /// <summary>
            /// Callback called when the discovery finishes
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="args"></param>
            private void Discoverer_OnDiscoveryCompleted(object sender, DataSourcesDiscoveryCompletedEventArgs args)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("DataSourcesDiscoveryCommunicator - Discovery completed");
                }

                //clean everything 
                m_dataSourcesDiscoverer.DiscoveryCompleted -= Discoverer_OnDiscoveryCompleted;
                m_dataSourcesDiscoverer = null;

                //if we didn't found a data source, return error
                if (args.Result.DataSources == null || args.Result.DataSources.Count == 0)
                {
                    if (Log.IsErrorEnabled)
                    {
                        Log.Error("DataSourcesDiscoveryCommunicator - No Data Sources found with discovery");
                    }

                    //call the callback with an error message
                    m_outerTrackingService.ExecuteOnUI(() =>
                    {
                        m_dataSourcesInitCompletedCallback(new DataSourceServiceStatusResponse() { IsError = true, Status = null, ErrorCode = 1111, Error = "No Data Sources Found", ID = "no_f" });
                    });

                    return;
                }

                //else, if we found at least one

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("DataSourcesDiscoveryCommunicator - Found {0} DataSources: ", args.Result.DataSources.Count);

                    foreach(var dataSourceFound in args.Result.DataSources)
                        Log.Debug("DataSourcesDiscoveryCommunicator - Found {0} DataSource on {1}:{2} ", dataSourceFound.Value.Id, dataSourceFound.Value.ControlApiEndpoint, dataSourceFound.Value.ControlApiPort);
                }

                //call initialization method
                InitializeAsync(args.Result.DataSources);
            }

            #endregion

            #region Initialization Methods

            /// <summary>
            /// Initializes this object using the data obtained from a saved discovery
            /// </summary>
            /// <param name="settingsManager">Data about the found data sources, saved in settings</param>
            /// <param name="discoveryCompletedCallback">Callback to be called when the discovery+initialization operation gets completed</param>
            internal void IntializeAsync(DataSourcesSettingsManager settingsManager, DataSourcesDiscoveryInitCompleted discoveryInitCompletedCallback)
            {
                //load data sources from the settings
                Dictionary<string, DataSourceItem> foundDataSources = settingsManager.DataSources;

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("DataSourcesDiscoveryCommunicator - Loaded data sources from settings");

                    if (foundDataSources != null)
                        foreach(var dataSourceFound in foundDataSources)
                            Log.Debug("DataSourcesDiscoveryCommunicator - Found {0} DataSource on {1}:{2} ", dataSourceFound.Value.Id, dataSourceFound.Value.ControlApiEndpoint, dataSourceFound.Value.ControlApiPort);
                }

                //save the settings manager reference
                m_dataSourcesSettingsManager = settingsManager;
                m_dataSourcesInitCompletedCallback = discoveryInitCompletedCallback;

                //call the private method
                InitializeAsync(foundDataSources);
            }

            /// <summary>
            /// Initializes this object using the data obtained from the discovery,
            /// </summary>
            /// <param name="foundDataSources">Data about the found data sources</param>
            private void InitializeAsync(Dictionary<string, DataSourceItem> foundDataSources)
            {
                //save the provided data in the settings manager
                m_outerTrackingService.ExecuteOnUI(() =>
                {
                    m_dataSourcesSettingsManager.Initialize(foundDataSources);
                });

                //check for null
                if (foundDataSources == null || foundDataSources.Count == 0)
                {
                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("DataSourcesDiscoveryCommunicator - Trying to initialize data sources with a null array. Are you trying a first time settings initialization?");
                    }

                    m_outerTrackingService.ExecuteOnUI(() =>
                    {
                        m_dataSourcesInitCompletedCallback(new DataSourceServiceStatusResponse() { IsError = true, Status = null, ErrorCode = 1111, Error = "No Data Sources in settings", ID = "no_f" });
                    });

                    return;
                }

                //TODO: this function and all this class has serious synchronization issues, due to the fact that await and async
                //can't be used in C# and the underlying API do not expose the Task so that it can be cancelled or waited

                //synchronization in loop from http://stackoverflow.com/questions/263116/c-waiting-for-all-threads-to-complete

                //allocate the array for all the data source communication objects
                m_dataSourcesWebApiClients = new DataSourceControlClient[foundDataSources.Count];

                //allocate synchronization structs 
                m_synchronizationStruct = new EventWaitHandle[foundDataSources.Count];

                //allocate space for info about the found data sources
                m_dataSourcesInfos = new DataSourceInfo[foundDataSources.Count];

                int idx = 0;

                //for each found data source
                foreach (var foundDataSourcePair in foundDataSources)
                {
                    //try to estabilish a communication with it
                    m_dataSourcesWebApiClients[idx] = new DataSourceControlClient(foundDataSourcePair.Value.ControlApiEndpoint, foundDataSourcePair.Value.ControlApiPort);

                    //set mutex, so that all the async operations for all data sources can be waited together
                    m_synchronizationStruct[idx] = new EventWaitHandle(false, EventResetMode.ManualReset);

                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("DataSourcesDiscoveryCommunicator - Data Source Control API Client created for Data Source {0}", foundDataSourcePair.Key);
                    }

                    //get status of communication estabilished
                    int idxx = idx; //we must copy this datum, or when the next callback will be executed, it will take the last value of idx
                    m_dataSourcesWebApiClients[idxx].GetStatusAsync(result => DataSourceDiscoveryInitCompleted(idxx, foundDataSourcePair.Value, result));

                    idx++;
                }

                //wait for all the operation started in the loop to finish.
                //If not everyone completed, reset everything and return error
                if (!Mutex.WaitAll(m_synchronizationStruct, new TimeSpan(0, 0, AutoDiscoverySettings.Default.UdpLocalClientTimeoutInSeconds + 10)))
                {
                    //release all handles
                    foreach (EventWaitHandle waitHandle in m_synchronizationStruct)
                        waitHandle.Close();

                    m_dataSourcesWebApiClients = null;

                    if (Log.IsErrorEnabled)
                    {
                        Log.Error("DataSourcesDiscoveryCommunicator - Some Data Sources time-outed");
                    }

                    //call the callback with an error message
                    m_outerTrackingService.ExecuteOnUI(() =>
                    {
                        m_dataSourcesInitCompletedCallback(new DataSourceServiceStatusResponse() { IsError = true, Status = null, ErrorCode = 1111, Error = "Some data sources went on timeout", ID = "no_f" });
                    });
                }
                //else, all data sources have answered
                else
                {
                    //release all handles
                    foreach (EventWaitHandle waitHandle in m_synchronizationStruct)
                        waitHandle.Close();

                    //check if connections are all ok
                    for (int i = 0; i < idx; i++)
                    {
                        DataSourceInfo dataSourceInfo = m_dataSourcesInfos[i];
                        DataSourceControlClient dataSourceWebApiClient = m_dataSourcesWebApiClients[i];

                        //null is the sign that something went wrong (it's set by DataSourceDiscoveryInitCompleted)
                        if (dataSourceWebApiClient == null)
                        {
                            //invalid all communications, and set the initialization as false
                            m_dataSourcesWebApiClients = null;

                            if (Log.IsErrorEnabled)
                            {
                                Log.Error("DataSourcesDiscoveryCommunicator - DataSource {0} at {1}:{2} failed to connect", dataSourceInfo.Id, dataSourceInfo.ControlApiEndpoint, dataSourceInfo.ControlApiPort);
                            }

                            //call the callback with an error message
                            m_outerTrackingService.ExecuteOnUI(() =>
                                {
                                    m_dataSourcesInitCompletedCallback(new DataSourceServiceStatusResponse() { IsError = true, Status = null, ErrorCode = 1111, Error = string.Format("Data source {0} at {1}:{2} failed to connect", dataSourceInfo.Id, dataSourceInfo.ControlApiEndpoint, dataSourceInfo.ControlApiPort), ID = "no_f" });
                                });

                            //exit
                            return;
                        }
                    }

                    //if we are here, at last, everything went well. Call the callback with success status

                    //generate the required dictionary
                    Dictionary<string, DataSourceInfo> foundDataSourcesInfoDictionary = new Dictionary<string, DataSourceInfo>();

                    for (int i = 0; i < idx; i++)
                        foundDataSourcesInfoDictionary[m_dataSourcesInfos[i].Id] = m_dataSourcesInfos[i];

                    //call the callback
                    m_outerTrackingService.ExecuteOnUI(() =>
                        {
                            m_dataSourcesInitCompletedCallback(new DataSourceServiceStatusResponse() { IsError = false, Error = "", ErrorCode = 0, ID = "", Status = new DataSourceServiceStatus() { CurrentState = DataSourceState.Idle, DataSources = foundDataSourcesInfoDictionary } });
                        }
                    );

                }
                    
            }

            /// <summary>
            /// Handles the end of a communication operation of a single data source
            /// </summary>
            /// <param name="idx">Incremental id of this data source inside the arrays of this class</param>
            /// <param name="dataSourceData">Result data of the data source discovery operation</param>
            /// <param name="dataSourceStatus">Status of communication estabilishment of this object with the underlying data source</param>
            private void DataSourceDiscoveryInitCompleted(int idx, DataSourceItem dataSourceData, DataSourceServiceStatusResponse dataSourceStatus)
            {
                //debug status of operation
                if (dataSourceStatus == null || dataSourceStatus.IsError)
                {
                    if (Log.IsErrorEnabled)
                    {
                        Log.Error("DataSourcesDiscoveryCommunicator - Unable to contact Data Source {0}. Is it running?", dataSourceData.Id);
                    }

                    //null the communication element, because it's useless now (che connection does not exist)
                    if(m_dataSourcesWebApiClients != null) //it is null if we have time-outed the connection from the data sources and we've received this event after too much time (look what happens after Mutex.WaitAll)
                        m_dataSourcesWebApiClients[idx] = null;  
                }
                else
                {
                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("DataSourcesDiscoveryCommunicator - Api Communication open with Data Source {0}", dataSourceData.Id);
                    }

                }

                //anyway, save the data of this connection
                var ds = new DataSourceInfo
                {
                    Id = dataSourceData.Id,
                    ControlApiPort = dataSourceData.ControlApiPort,
                    DataStreamEndpoint = dataSourceData.DataStreamerEndpoint,
                    DataStreamPort = dataSourceData.DataStreamerPort,
                    ControlApiEndpoint = dataSourceData.ControlApiEndpoint,
                };

                m_dataSourcesInfos[idx] = ds;

                //reset mutex, for synchronization purposes (this thread has finished)
                try
                {
                    m_synchronizationStruct[idx].Set();
                }
                catch(Exception)
                {
                    //just here if the handle has been closed
                }
            }

            #endregion

            #region Reconfig methods

            /// <summary>
            /// Asks the underlying data sources to reconfigure themselves, getting to know all data sources on its network
            /// </summary>
            /// <param name="completionCallback">Callback to be called when the reconfiguration (of each single data source) ends</param>
            internal void ReconfigAsync(DataSourceControlClient.RequestCompleted completionCallback)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("DataSourcesDiscoveryCommunicator - Starting Data Sources network reconfiguration");
                }

                //calls the underlying method onto all the data sources (notice that we actually call this anonymous lambda, to return the correct ID of the tracking service,
                //that it is not present in the return data of the web api controller)
                for (int i = 0; i < m_dataSourcesWebApiClients.Length; i++)
                {
                    DataSourceControlClient dataSourceWebApiClient = m_dataSourcesWebApiClients[i];
                     
                    int idx = i; //for synchronization purposes on next call to m_dataSourcesInfos[0].Id inside the lambda
                    dataSourceWebApiClient.EnableAutoDiscoveryAsync((requestCompleted) =>
                    {
                        completionCallback(new ImmotionAR.ImmotionRoom.DataSource.ControlClient.Model.OperationResponse() { ID = m_dataSourcesInfos[idx].Id, IsError = requestCompleted.IsError, Error = requestCompleted.Error, ErrorCode = requestCompleted.ErrorCode });
                    });
                }
                    
            }

            #endregion

            #region Data Source Reboot methods

            /// <summary>
            /// Reboot a particular data source machine
            /// </summary>
            /// <param name="dataSourceID">ID of the data source, whose PC has to be rebooted</param>
            /// <param name="completionCallback">Callback to be called after the reboot request has been completed</param>
            internal void RebootDataSourceAsync(string dataSourceID, DataSourceControlClient.RequestCompleted completionCallback)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("DataSourcesDiscoveryCommunicator - Starting Data Source {0} rebooting", dataSourceID);
                }

                //search the data source with the requested ID and launch its rebooting operation
                bool found = false;

                for (int i = 0; i < m_dataSourcesInfos.Length; i++)
                {
                    if (m_dataSourcesInfos[i].Id == dataSourceID)
                    {
                        found = true;

                        m_dataSourcesWebApiClients[i].SystemRebootAsync((requestCompleted) =>
                        {
                            completionCallback(new ImmotionAR.ImmotionRoom.DataSource.ControlClient.Model.OperationResponse() { ID = m_dataSourcesInfos[i].Id, IsError = requestCompleted.IsError, Error = requestCompleted.Error, ErrorCode = requestCompleted.ErrorCode });
                        });

                        break;
                    }
                }

                //if no datasource has been found, call the callback with error message
                if (!found)
                {
                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("DataSourcesDiscoveryCommunicator - Rebooting of Data Source {0} failed: no such data source exists", dataSourceID);
                    }

                    completionCallback(new ImmotionAR.ImmotionRoom.DataSource.ControlClient.Model.OperationResponse() { ID = dataSourceID, IsError = true, Error = "Reboot failed. No such data source exists", ErrorCode = 0 });
                }
            }

            #endregion

            #region Get Status Methods

            /// <summary>
            /// Get the status of the underlying data sources
            /// </summary>
            /// <remarks>
            /// Do not call this method during an initialization because its uses the same internal synchronization constructs
            /// </remarks>
            /// <param name="completionCallback">Callback to be called after the status request has been completed</param>
            internal void GetDataSourcesStatusAsync(DataSourcesGetStatusCompleted completionCallback)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("DataSourcesDiscoveryCommunicator - Starting Data sources status request operation");
                }

                //allocate array to store all data sources statuses data
                m_lastDataSourcesGetStatusResults = new DataSourceServiceStatusResponse[m_dataSourcesWebApiClients.Length];

                //loop all data source communication objects
                for(int i = 0; i < m_dataSourcesWebApiClients.Length; i++)
                {
                    var dataSourceApiClient = m_dataSourcesWebApiClients[i];
 
                    //set mutex, so that all the async operations for all data sources can be waited together
                    m_synchronizationStruct[i] = new EventWaitHandle(false, EventResetMode.ManualReset);

                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("DataSourcesDiscoveryCommunicator - Starting get status operation for Data Source {0}", m_dataSourcesInfos[i].Id);
                    }

                    //get status of communication estabilished
                    int idxx = i; //we must copy this datum, or when the next callback will be executed, it will take the last value of idx
                    m_dataSourcesWebApiClients[idxx].GetStatusAsync(result => DataSourceGetStatusCompleted(idxx, result));
                }
                
                //wait for all the operation started in the loop to finish.
                Mutex.WaitAll(m_synchronizationStruct, new TimeSpan(0, 0, AutoDiscoverySettings.Default.UdpLocalClientTimeoutInSeconds + 10));

                //release all handles
                foreach (EventWaitHandle waitHandle in m_synchronizationStruct)
                    waitHandle.Close();

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("DataSourcesDiscoveryCommunicator - Data Sources get status finished");
                }

                //call the callback passing the obtained data
                completionCallback(m_lastDataSourcesGetStatusResults);
            }

            /// <summary>
            /// Handles the end of a get status operation on a single data source
            /// </summary>
            /// <param name="idx">Incremental id of this data source inside the arrays of this class</param>
            /// <param name="dataSourceStatus">Status of the get status operation with the underlying data source</param>
            private void DataSourceGetStatusCompleted(int idx, DataSourceServiceStatusResponse dataSourceStatus)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("DataSourcesDiscoveryCommunicator -  Data Source {0} get status completed", m_dataSourcesInfos[idx].Id);
                }
                
                //save the returned status inside internal data
                m_lastDataSourcesGetStatusResults[idx] = dataSourceStatus;
                m_lastDataSourcesGetStatusResults[idx].ID = m_dataSourcesInfos[idx].Id;

                //reset mutex, for synchronization purposes (this thread has finished)
                try
                {
                    m_synchronizationStruct[idx].Set();
                }
                catch(Exception)
                {
                    //just here if the handle has been closed
                }
            }

            #endregion
        
        }

    }

}
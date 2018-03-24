namespace ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement
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
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.DataStructures;

    public partial class TrackingServiceManager : EventBasedMonoBehaviour
    {

        /// <summary>
        /// Handles discovery and communication with the underlying TrackingService
        /// </summary>
        protected internal partial class TrackingServiceDiscoveryCommunicator
        {
            #region Delegates definition

            /// <summary>
            /// Signature of method to be called when the tracking discovery is completed and this object has been initialized.
            /// The parameter sets if the discovery and / or initialization went good or bad
            /// </summary>
            public delegate void TrackingServiceDiscoveryInitCompleted(TrackingServiceStatusResponse TrackingServiceStatus, TrackingServiceInfo trackingServiceInfo);

            #endregion

            #region Private Fields

            /// <summary>
            /// Tracking Service enclosing this instance
            /// </summary>
            protected TrackingServiceManager m_outerTrackingService;

            /// <summary>
            /// Discovers tracking service
            /// </summary>
            protected TrackingServiceDiscoverer m_TrackingServiceDiscoverer;

            /// <summary>
            /// Enables communication with underlying tracking service
            /// </summary>
            protected TrackingServiceControlClient m_TrackingServiceWebApiClient;

            /// <summary>
            /// Info about the found tracking service
            /// </summary>
            protected TrackingServiceInfo m_trackingServiceInfo;

            /// <summary>
            /// Reference to the Tracking Service Settings manager provided by the user
            /// </summary>
            protected TrackingServiceSettingsManager m_trackingServiceSettingsManager;

            /// <summary>
            /// Reference to the Tracking Service init completed callback provided by the user
            /// </summary>
            protected TrackingServiceDiscoveryInitCompleted m_trackingServiceInitCompletedCallback;

            #endregion

            #region Internal properties

            /// <summary>
            /// Gets the object that actually communicates with the underlying tracking service.
            /// If this value is null, this object hasn't been correctly initialized yet
            /// </summary>
            protected internal TrackingServiceControlClient TrackingServiceController
            {
                get
                {
                    return m_TrackingServiceWebApiClient;
                }
            }

            /// <summary>
            /// Gets info about the current tracking service (maybe null if no tracking service yet)
            /// </summary>
            public TrackingServiceInfo TrackingServiceInfo
            {
                get
                {
                    return m_trackingServiceInfo;
                }
            }

            #endregion

            #region Constructor

            /// <summary>
            /// Construct a communication object with the Tracking Service
            /// </summary>
            /// <param name="outerInstance">Enclosing instance</param>
            protected internal TrackingServiceDiscoveryCommunicator(TrackingServiceManager outerInstance)
            {
                m_outerTrackingService = outerInstance;
            }

            #endregion

            #region Discovery Methods

            /// <summary>
            /// Performs Tracking Service Discovery, to find it and start communication with it. Then initialize all internal data and
            /// saves found Tracking Service Settings for future sessions. After that, calls the provided callback.
            /// </summary>
            /// <param name="settingsManager">Manager of the settings of the Tracking Service</param>
            /// <param name="discoveryInitCompletedCallback">Callback to be called when the discovery+initialization operation gets completed</param>
            /// <exception cref="InvalidOperationException">If this object was already been initialized</exception>
            public void DiscoverAndInitializeAsync(TrackingServiceSettingsManager settingsManager,
                                                     TrackingServiceDiscoveryInitCompleted discoveryInitCompletedCallback)
            {
                DiscoverAndInitializeAsyncInternal(settingsManager, discoveryInitCompletedCallback);
            }

            /// <summary>
            /// Performs Tracking Service Discovery, to find it and start communication with it. Then initialize all internal data and
            /// saves found Tracking Service Settings for future sessions. After that, calls the provided callback.
            /// </summary>
            /// <param name="settingsManager">Manager of the settings of the Tracking Service</param>
            /// <param name="discoveryInitCompletedCallback">Callback to be called when the discovery+initialization operation gets completed</param>
            /// <exception cref="InvalidOperationException">If this object was already been initialized</exception>
            internal void DiscoverAndInitializeAsyncInternal(TrackingServiceSettingsManager settingsManager,
                                                     TrackingServiceDiscoveryInitCompleted discoveryInitCompletedCallback)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("TrackingServiceDiscoveryCommunicator - Starting TrackingService discovery");
                }

                //throw exception if this object is already initialized
                if (m_TrackingServiceWebApiClient != null)
                {
                    if (Log.IsErrorEnabled)
                    {
                        Log.Error("TrackingServiceDiscoveryCommunicator - Trying To Discover and Initialize an already initialized object");
                    }

                    throw new InvalidOperationException("Can't initialize an already initialized TrackingServiceDiscoveryCommunicator");
                }

                m_trackingServiceSettingsManager = settingsManager;
                m_trackingServiceInitCompletedCallback = discoveryInitCompletedCallback;

                //load default settings for the auto discovery operation, then get local IP data
                var autoDiscoverySettings = AutoDiscoverySettings.Default;
                autoDiscoverySettings.LocalAddress = NetworkTools.GetLocalIpAddress().ToString();
                autoDiscoverySettings.LocalPort = AutoDiscoveryDefaultSettings.TrackingServiceAutoDiscoveryLocalPort;

                //start an async discovery operation
                m_TrackingServiceDiscoverer = new TrackingServiceDiscoverer(autoDiscoverySettings);
                m_TrackingServiceDiscoverer.DiscoveryCompleted += Discoverer_OnDiscoveryCompleted;
                m_TrackingServiceDiscoverer.StartTrackingServiceDiscoveryAsync();
            }

            /// <summary>
            /// Stops current discovery, if any
            /// </summary>
            internal void StopDiscovery()
            {
                if (m_TrackingServiceDiscoverer != null)
                {
                    m_TrackingServiceDiscoverer.DiscoveryCompleted -= Discoverer_OnDiscoveryCompleted;
                    m_TrackingServiceDiscoverer.StopTrackingServiceDiscovery();
                }
            }


            /// <summary>
            /// Callback called when the discovery finishes
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="args"></param>
            private void Discoverer_OnDiscoveryCompleted(object sender, TrackingServiceDiscoveryCompletedEventArgs args)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("TrackingServiceDiscoveryCommunicator - Discovery completed");
                }

                //clean everything 
                m_TrackingServiceDiscoverer.DiscoveryCompleted -= Discoverer_OnDiscoveryCompleted;
                m_TrackingServiceDiscoverer = null;

                //if we didn't found a tracking service, return error
                if (string.IsNullOrEmpty(args.Result.Id))
                {
                    if (Log.IsErrorEnabled)
                    {
                        Log.Error("TrackingServiceDiscoveryCommunicator - No Tracking Service found with discovery");
                    }

                    //call the callback with an error message
                    m_outerTrackingService.ExecuteOnUI(() =>
                    {
                        m_trackingServiceInitCompletedCallback(new TrackingServiceStatusResponse() { IsError = true, Status = null, ErrorCode = 1111, ErrorDescription = "No Tracking Service Found", ID = "no_f" }, null);
                    });

                    return;
                }

                //else, if we found it

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("TrackingServiceDiscoveryCommunicator - TrackingService {0} found on {1}:{2}", args.Result.Id, args.Result.ControlApiEndpoint, args.Result.ControlApiPort);
                }
                
                //call initialization method
                InitializeAsync(args.Result.Id, args.Result.ControlApiEndpoint, args.Result.ControlApiPort, args.Result.DataStreamerEndpoint, args.Result.DataStreamerPort);

            }

            #endregion

            #region Initialization Methods

            /// <summary>
            /// Initialize all internal data and
            /// saves provided Tracking Service Settings for future sessions. After that, calls the provided callback.
            /// </summary>
            /// <param name="settingsManager">Settings from which take the data for initialization</param>
            /// <param name="discoveryInitCompletedCallback">Callback to be called when the initialization operation gets completed</param>
            /// <exception cref="InvalidOperationException">If this object was already been initialized</exception>
            protected internal virtual void InitializeAsync(TrackingServiceSettingsManager settingsManager,
                                     TrackingServiceDiscoveryInitCompleted discoveryInitCompletedCallback)
            {
                //throw exception if this object is already initialized
                if (m_TrackingServiceWebApiClient != null)
                {
                    if (Log.IsErrorEnabled)
                    {
                        Log.Error("TrackingServiceDiscoveryCommunicator - Trying To Discover and Initialize an already initialized object");
                    }

                    throw new InvalidOperationException("Can't initialize an already initialized TrackingServiceDiscoveryCommunicator");
                }

                //save the provided data into internal structures
                m_trackingServiceSettingsManager = settingsManager;
                m_trackingServiceInitCompletedCallback = discoveryInitCompletedCallback;

                //call private initialization method
                this.InitializeAsync(settingsManager.TrackingServiceId, settingsManager.TrackingServiceControlApiEndpoint, settingsManager.TrackingServiceControlApiPort, "", 0);
            }

            /// <summary>
            /// Initializes this object using the data obtained from the settings or from the discovery,
            /// </summary>
            /// <param name="trackingServiceId">Tracking Service ID</param>
            /// <param name="trackingServiceControlApiEndpoint">TrackingService Control API IP Address</param>
            /// <param name="trackingServiceControlApiPort">TrackingService Control API IP Port</param>
            /// <param name="trackingServiceDataStreamerApiEndpoint">TrackingService Data Streamer API IP Address</param>
            /// <param name="trackingServiceDataStreamerApiPort">TrackingService Data Streamer API IP Port</param>
            private void InitializeAsync(string trackingServiceId, string trackingServiceControlApiEndpoint, int trackingServiceControlApiPort, 
                                         string trackingServiceDataStreamerApiEndpoint, int trackingServiceDataStreamerApiPort)
            {
                //check for invalid data
                if (trackingServiceId == null || trackingServiceId.Length == 0)
                {
                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("TrackingServiceDiscoveryCommunicator - Trying to initialize tracking service with a void string. Are you trying a first time settings initialization?");
                    }

                    m_outerTrackingService.ExecuteOnUI(() =>
                    {
                        m_trackingServiceInitCompletedCallback(new TrackingServiceStatusResponse() { IsError = true, Status = null, ErrorCode = 1111, ErrorDescription = "No Tracking service in settings", ID = "no_f" }, m_trackingServiceInfo);
                    });

                    return;
                }


                //save the provided data in the settings manager
                m_outerTrackingService.ExecuteOnUI(() =>
                {
                    m_trackingServiceSettingsManager.Initialize(trackingServiceId, trackingServiceControlApiEndpoint, trackingServiceControlApiPort);
                });

                //try to estabilish a communication with the found Tracking Service
                m_TrackingServiceWebApiClient = new TrackingServiceControlClient(trackingServiceControlApiEndpoint, trackingServiceControlApiPort);

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("TrackingServiceDiscoveryCommunicator - TrackingService Control API Client created");
                }

                //get status of communication estabilished
                m_TrackingServiceWebApiClient.GetStatusAsync(result => m_outerTrackingService.ExecuteOnUI(() =>
                {
                    if (result == null || result.IsError)
                    {
                        if (Log.IsErrorEnabled)
                        {
                            Log.Error("TrackingServiceDiscoveryCommunicator - Unable to contact Tracking Service. Is it running?");
                        }

                        //set the communicator to null, so the system can be re-initialized (remember that if it gets a value, it can't be
                        //re-initialized)
                        m_TrackingServiceWebApiClient = null;

                    }
                    else
                        m_trackingServiceInfo = new TrackingServiceInfo
                        {
                            Id = trackingServiceId,
                            ControlApiEndpoint = trackingServiceControlApiEndpoint,
                            ControlApiPort = trackingServiceControlApiPort,
                            DataStreamEndpoint = trackingServiceDataStreamerApiEndpoint,
                            DataStreamPort = trackingServiceDataStreamerApiPort,
                            MasterDataSourceID = (result.Status.MasterDataStreamer == null || result.Status.MasterDataStreamer.Length == 0) ? null : result.Status.MasterDataStreamer,
                            IsCalibrated = result.Status.CalibrationDone,
                        };

                    //call the callback, providing it current status
                    m_trackingServiceInitCompletedCallback(result, m_trackingServiceInfo);
                    
                }));
            }

            #endregion
        }

    }

}
namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement;
    using ImmotionAR.ImmotionRoom.TrackingService.ControlClient;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using ImmotionAR.ImmotionRoom.TrackingService.ControlClient.Model;

    public partial class TrackingServiceManagerAdvanced : TrackingServiceManager
    {

        /// <summary>
        /// Handles discovery and communication with the underlying DataSources
        /// </summary>
        protected partial class TrackingServiceDiscoveryCommunicatorAdvanced : TrackingServiceManager.TrackingServiceDiscoveryCommunicator
        {
            #region Internal properties

            /// <summary>
            /// Gets the object that actually communicates with the underlying tracking service.
            /// If this value is null, this object hasn't been correctly initialized yet
            /// </summary>
            internal new TrackingServiceControlClient TrackingServiceController
            {
                get
                {
                    return m_TrackingServiceWebApiClient;
                }
            }

            #endregion

            #region Constructor

            /// <summary>
            /// Construct a communication object with the Tracking Service
            /// </summary>
            /// <param name="outerInstance">Enclosing instance</param>
            internal TrackingServiceDiscoveryCommunicatorAdvanced(TrackingServiceManager outerInstance) :
                base(outerInstance)
            {
            }

            #endregion

            #region TrackingServiceDiscoveryCommunicator methods

            /// <summary>
            /// Initialize all internal data and
            /// saves provided Tracking Service Settings for future sessions. After that, calls the provided callback.
            /// </summary>
            /// <param name="settingsManager">Settings from which take the data for initialization</param>
            /// <param name="discoveryCompletedCallback">Callback to be called when the initialization operation gets completed</param>
            /// <exception cref="InvalidOperationException">If this object was already been initialized</exception>
            internal void InitializeeAsync(TrackingServiceSettingsManager settingsManager,
                                                   TrackingServiceDiscoveryInitCompleted discoveryInitCompletedCallback)
            {                
                //call base methods (this method is here just to make the method of the base class accessible by classes of this package)
                base.InitializeAsync(settingsManager, discoveryInitCompletedCallback);
            }

            #endregion

            #region Reconfig methods

            /// <summary>
            /// Asks the underlying tracking service to reconfigure itself, getting to know all data sources on its network
            /// </summary>
            /// <param name="completionCallback">Callback to be called when the reconfiguration ends</param>
            internal void ReconfigAsync(TrackingServiceControlClient.RequestCompleted completionCallback)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("TrackingServiceDiscoveryCommunicatorAdvanced - Starting TrackingService network reconfiguration");
                }

                //calls the underlying method (notice that we actually call this anonymous lambda, to return the correct ID of the tracking service,
                //that it is not present in the return data of the web api controller)
                m_TrackingServiceWebApiClient.EnableAutoDiscoveryAsync((requestCompleted) =>
                {
                    completionCallback(new OperationResponse() { ID = m_trackingServiceInfo.Id, IsError = requestCompleted.IsError, ErrorDescription = requestCompleted.ErrorDescription, ErrorCode = requestCompleted.ErrorCode });
                },
                new AutoDiscoveryParameters { ClearMasterDataSource = false, ClearCalibrationData = false });
            }

            #endregion

            #region Set Master methods

            /// <summary>
            /// Asks the underlying tracking service to set a master data source
            /// </summary>
            /// <param name="masterDataSourceID">The data source to set as new Master Data Source</param>
            /// <param name="completionCallback">Callback to be called when the reconfiguration ends</param>
            internal void SetMasterDataSource(string masterDataSourceID, TrackingServiceControlClient.RequestCompleted completionCallback)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("TrackingServiceDiscoveryCommunicatorAdvanced - Asked to set datasource {0} as new master", masterDataSourceID);
                }

                //calls the underlying method (notice that we actually call this anonymous lambda, to return the correct ID of the tracking service,
                //that it is not present in the return data of the web api controller)
                m_TrackingServiceWebApiClient.SetMasterDataSourceAsync(masterDataSourceID, (requestCompleted) =>
                {
                    completionCallback(new OperationResponse() { ID = m_trackingServiceInfo.Id, IsError = requestCompleted.IsError, ErrorDescription = requestCompleted.ErrorDescription, ErrorCode = requestCompleted.ErrorCode });
                });
            }

            #endregion

            #region Data Source Reboot methods

            /// <summary>
            /// Reboot the tracking service machine
            /// </summary>
            /// <param name="completionCallback">Callback to be called after the reboot request has been completed</param>
            internal void RebootTrackingServiceAsync(TrackingServiceControlClient.RequestCompleted completionCallback)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("TrackingServiceDiscoveryCommunicatorAdvanced - Starting Tracking Service {0} rebooting", m_trackingServiceInfo.Id);
                }
 
                m_TrackingServiceWebApiClient.SystemRebootAsync((requestCompleted) =>
                {
                    completionCallback(new ImmotionAR.ImmotionRoom.TrackingService.ControlClient.Model.OperationResponse() { ID = m_trackingServiceInfo.Id, IsError = requestCompleted.IsError, ErrorDescription = requestCompleted.ErrorDescription, ErrorCode = requestCompleted.ErrorCode });
                });

            }

            #endregion

            #region Set Girello methods

            /// <summary>
            /// Asks the underlying tracking service to set a new girello bounds
            /// </summary>
            /// <param name="newSceneDescriptor">The description of the scene the tracking will happen inside (Girello, Room, etc...)</param>
            /// <param name="completionCallback">Callback to be called when the reconfiguration of the girello ends</param>
            internal void SetSceneDescriptor(TrackingServiceSceneDescriptor newSceneDescriptor, TrackingServiceControlClient.RequestCompleted completionCallback)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("TrackingServiceDiscoveryCommunicatorAdvanced - Asked to set new scene description");
                }

                //calls the underlying method (notice that we actually call this anonymous lambda, to return the correct ID of the tracking service,
                //that it is not present in the return data of the web api controller)
                m_TrackingServiceWebApiClient.SetSceneDescriptorAsync(newSceneDescriptor, (requestCompleted) =>
                {
                    completionCallback(new OperationResponse() { ID = m_trackingServiceInfo.Id, IsError = requestCompleted.IsError, ErrorDescription = requestCompleted.ErrorDescription, ErrorCode = requestCompleted.ErrorCode });
                });
            }

            #endregion

            #region Get Status Methods

            /// <summary>
            /// Get the status of the underlying tracking service
            /// </summary>
            /// <param name="completionCallback">Callback to be called after the status request has been completed</param>
            internal void GetTrackingServiceStatusAsync(TrackingServiceControlClient.TrackingServiceStatusCompleted completionCallback)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("TrackingServiceDiscoveryCommunicatorAdvanced - Starting Tracking Service {0} status request operation", m_trackingServiceInfo.Id);
                }

                if (m_TrackingServiceWebApiClient != null)
                    m_TrackingServiceWebApiClient.GetStatusAsync(completionCallback);
                else
                    completionCallback(new TrackingServiceStatusResponse() { ErrorCode = -1, ErrorDescription = "No connected tracking service", ID = "", IsError = true, Status = null });
            }

            #endregion
        }
    }
}

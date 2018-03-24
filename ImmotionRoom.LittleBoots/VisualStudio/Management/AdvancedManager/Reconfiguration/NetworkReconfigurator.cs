namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedManager.Reconfiguration
{
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.DataStructures;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Performs reconfiguration of a network of tracking services and data sources, so that at the end of the operation, every
    /// service get to know about each of the others, so they can communicate for tracking purposes
    /// </summary>
    internal class NetworkReconfigurator
    {
        /// <summary>
        /// Number of all devices on the network that have to be reconfigured
        /// </summary>
        private int m_devicesOnNetwork;

        /// <summary>
        /// Number of all devices on the network for which the reconfiguration operation has finished
        /// </summary>
        private int m_reconfiguredDevicesOnNetwork;

        /// <summary>
        /// Current compound state of the reconfiguration operation
        /// </summary>
        private ReconfiguredServicesEventArgs m_operationStatus;

        /// <summary>
        /// Callback to trigger when the reconfiguration ends
        /// </summary>
        private TrackingServiceManagerAdvanced.ReconfiguredServicesHandler m_finishedReconfigurationCallback;

        #region Constructor

        /// <summary>
        /// Construct a network reconfigurator object
        /// </summary>
        /// <param name="devicesOnNetwork">Total number of devices on the network to be reconfigured</param>
        /// <param name="finishedCallback">Callback to call when the reconfiguration ends</param>
        internal NetworkReconfigurator(int devicesOnNetwork, TrackingServiceManagerAdvanced.ReconfiguredServicesHandler finishedCallback)
        {
            m_devicesOnNetwork = devicesOnNetwork;
            m_reconfiguredDevicesOnNetwork = 0;
            m_operationStatus = new ReconfiguredServicesEventArgs() {ErrorString = null, HumanReadableNames = new string[m_devicesOnNetwork]};
            m_finishedReconfigurationCallback = finishedCallback;

            if (Log.IsDebugEnabled)
            {
                Log.Debug("NetworkReconfigurator - Creation");
            }
        }

        #endregion

        #region Network Reconfiguration Methods

        /// <summary>
        /// Callback called when the tracking service reconfiguration ends
        /// </summary>
        /// <param name="response">Completion status of the operation</param>
        internal void TrackingServiceReconfigCompletedCallback(ImmotionAR.ImmotionRoom.TrackingService.ControlClient.Model.OperationResponse response)
        {
            ServiceReconfigCompleted(response.ID, response.IsError ? response.ErrorDescription : null);

            if (Log.IsDebugEnabled)
            {
                Log.Debug("NetworkReconfigurator - Tracking service {0} reconfiguration completed with result {1}", response.ID, response.IsError ? "FAILURE" : "SUCCESS");
            }
        }

        /// <summary>
        /// Callback called when a data source reconfiguration ends
        /// </summary>
        /// <param name="response">Completion status of the operation</param>
        internal void DataSourceReconfigCompletedCallback(ImmotionAR.ImmotionRoom.DataSource.ControlClient.Model.OperationResponse response)
        {
            ServiceReconfigCompleted(response.ID, response.IsError ? response.Error : null);

            if (Log.IsDebugEnabled)
            {
                Log.Debug("NetworkReconfigurator - Data source {0} reconfiguration completed with result {1}", response.ID, response.IsError ? "FAILURE" : "SUCCESS");
            }
        }

        /// <summary>
        /// Function called when any of the network services finishes its reconfiguration operation
        /// </summary>
        /// <param name="serviceID">ID of the service of interest</param>
        /// <param name="errorString">Error string of the operation. Null if no error</param>
        private void ServiceReconfigCompleted(string serviceID, string errorString)
        {
            Monitor.Enter(this); //synchronize the calls, because they can happen simultaneously

            //one more service has been configured
            m_operationStatus.HumanReadableNames[m_reconfiguredDevicesOnNetwork] = serviceID;
            m_reconfiguredDevicesOnNetwork++;

            //record error, if any, concatenating to current error string
            if(errorString != null)
            {
                if(m_operationStatus.ErrorString == null)
                    m_operationStatus.ErrorString = "";

                m_operationStatus.ErrorString += string.Format("\n{0}:{1}", serviceID, errorString);
            }
                
            //if we have reconfigured all devices, call the finished callback
            if(m_reconfiguredDevicesOnNetwork >= m_devicesOnNetwork)
            {
                m_finishedReconfigurationCallback(m_operationStatus);
            }

            Monitor.Exit(this);
        }

        #endregion
    }
}

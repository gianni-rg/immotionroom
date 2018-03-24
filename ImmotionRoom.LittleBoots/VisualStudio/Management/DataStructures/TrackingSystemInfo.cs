namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.DataStructures
{
    using ImmotionAR.ImmotionRoom.DataSource.ControlClient.Model;
    using ImmotionAR.ImmotionRoom.TrackingService.ControlClient.Model;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Collects info about the whole tracking system (tracking service + data sources).
    /// This info are the one to be shown when a system get status is performed
    /// </summary>
    public class TrackingSystemInfo
    {
        /// <summary>
        /// Info about a particular data source 
        /// </summary>
        public class TrackingSystemDataSourceInfo
        {
            /// <summary>
            /// String ID of the Data Source
            /// </summary>
            public string DataSourceId { get; set; }

            /// <summary>
            /// IP Address of the control API to control the data source behaviour
            /// </summary>
            public string ControlApiEndpoint { get; set; }

            /// <summary>
            /// IP Port of the control API to control the data source behaviour
            /// </summary>
            public int ControlApiPort { get; set; }

            /// <summary>
            /// Current state of the data source.
            /// Contains meaningful values only if <see cref="IsReachable"/> is true
            /// (otherwise the service is unreachable and we can't obtain valid informations)
            /// </summary>
            public DataSourceState CurrentState { get; set; }

            /// <summary>
            /// True if this data streaming service is currently reachable, false otherwise
            /// </summary>
            public bool IsReachable { get; set; }

            /// <summary>
            /// Version of this service
            /// Contains meaningful values only if <see cref="IsReachable"/> is true
            /// (otherwise the service is unreachable and we can't obtain valid informations)
            /// </summary>
            public string Version { get; set; }
        }

        /// <summary>
        /// String ID of the Tracking Service
        /// </summary>
        public string TrackingServiceId { get; set; }

        /// <summary>
        /// IP Address of the control API to control the tracking service behaviour
        /// </summary>
        public string ControlApiEndpoint { get; set; }

        /// <summary>
        /// IP Port of the control API to control the tracking service behaviour
        /// </summary>
        public int ControlApiPort { get; set; }

        /// <summary>
        /// True if this data streaming service is currently reachable, false otherwise
        /// </summary>
        public bool IsReachable { get; set; }

        /// <summary>
        /// Version of this service
        /// Contains meaningful values only if <see cref="IsReachable"/> is true
        /// (otherwise the service is unreachable and we can't obtain valid informations)
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// True if the tracking system is calibrated, false otherwise
        /// Contains meaningful values only if <see cref="IsReachable"/> is true
        /// (otherwise the service is unreachable and we can't obtain valid informations)
        /// </summary>
        public bool CalibrationDone { get; set; }

        /// <summary>
        /// Current state of the Tracking Service of the system
        /// Contains meaningful values only if <see cref="IsReachable"/> is true
        /// (otherwise the service is unreachable and we can't obtain valid informations)
        /// </summary>
        public TrackingServiceState CurrentState { get; set; }

        /// <summary>
        /// Name of the master data source (null if the master is not configured)
        /// Contains meaningful values only if <see cref="IsReachable"/> is true
        /// (otherwise the service is unreachable and we can't obtain valid informations)
        /// </summary>
        public string MasterDataSource { get; set; }

        /// <summary>
        /// Info about the data sources of the system
        /// Contains meaningful values only if <see cref="IsReachable"/> is true
        /// (otherwise the service is unreachable and we can't obtain valid informations)
        /// </summary>
        public TrackingSystemDataSourceInfo[] DataSourcesInfo { get; set; }
    }
}

namespace ImmotionAR.ImmotionRoom.TrackingEngine.Calibration
{
    using System;
    using System.Collections.Generic;
    using Interfaces;
    using Logger;

    /// <summary>
    ///     Calibrates multiple DataSources, using single body informations.
    ///     All the DataSource are calibrated in a many-to-one fashion: there is a master local DataSource
    ///     and 1 or more slave remote DataSource, connected through the net.
    ///     This functions perform the calibration of every single slave with respect to the the local master,
    ///     (ignoring all other slaves data while calibrating each of the slave).
    ///     With this class, you can chose with the keyboard which slave can be calibrated with the master in each moment
    ///     (e.g. the key '1' activates/deactivates calibration update of first slave DataSource)
    /// </summary>
    public class DataSourcesCalibratorSelective : DataSourcesCalibrator
    {
        #region Private fields
        /// <summary>
        ///     Saves which calibrators are active at this time
        /// </summary>
        private readonly Dictionary<string, bool> m_CalibratorsActive;

        /// <summary>
        ///     Master calibrator activation status
        /// </summary>
        private bool m_MasterCalibratorActive;
        #endregion

        #region Constructor
        public DataSourcesCalibratorSelective(string masterDataSourceId, IBodyDataProvider bodyDataProvider) : base(masterDataSourceId, bodyDataProvider)
        {
            m_Logger = LoggerService.GetLogger(typeof(DataSourcesCalibratorSelective));
            m_CalibratorsActive = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            m_MasterCalibratorActive = false;
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Update function executed at each frame rendering
        /// </summary>
        public override void Update(double deltaTime)
        {
            foreach (string dataSourceId in m_BodyDataProvider.DataSources.Keys)
            {
                if (!m_Calibrators.ContainsKey(dataSourceId))
                {
                    m_Calibrators.Add(dataSourceId, new MasterSlaveDataSourcesCalibrator(m_MasterDataSourceId, dataSourceId, m_BodyDataProvider, m_KeyJoints, LastButNthValidMatrix, CalibrateSlavesUsingCentroids));
                }

                if (!m_CalibratorsActive.ContainsKey(dataSourceId))
                {
                    m_CalibratorsActive.Add(dataSourceId, false);
                }

                if (m_CalibratorsActive[dataSourceId])
                {
                    m_Calibrators[dataSourceId].Update(deltaTime);
                }
            }

            if (m_MasterCalibratorActive)
            {
                m_MasterCalibrator.Update(deltaTime);
            }
        }

        public bool SetCalibratorStatus(string dataSourceId, bool newStatus)
        {
            if (dataSourceId == m_MasterDataSourceId)
            {
                m_MasterCalibratorActive = newStatus;

                //reset master calibrator at each new activation
                if (m_MasterCalibratorActive)
                {
                    m_MasterCalibrator.Reset();
                }

                if (m_Logger != null && m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("SetCalibratorStatus() - Master Calibrator activation status is now  {0}", m_MasterCalibratorActive);
                }

                return true;
            }
            else
            {
                if (!m_CalibratorsActive.ContainsKey(dataSourceId))
                {
                    return false;
                }

                m_CalibratorsActive[dataSourceId] = newStatus;

                //reset the slave calibrator at each new activation
                if (m_CalibratorsActive[dataSourceId])
                {
                    m_Calibrators[dataSourceId].Reset();
                }

                if (m_Logger != null && m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("SetCalibratorStatus() - Calibrator[{0}] activation status is now {1}", dataSourceId, m_CalibratorsActive[dataSourceId]);
                }

                return true;
            }
        }
        #endregion
    }
}
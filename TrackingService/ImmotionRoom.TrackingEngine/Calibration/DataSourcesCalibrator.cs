namespace ImmotionAR.ImmotionRoom.TrackingEngine.Calibration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Interfaces;
    using Logger;
    using Model;

    /// <summary>
    ///     Calibrates multiple DataSources, using single body information.
    ///     All the DataSource are calibrated in a many-to-one fashion: there is a master local DataSource
    ///     and 1 or more slave remote DataSource, connected through the net.
    ///     This functions perform the calibration of every single slave with respect to the the local master,
    ///     (ignoring all other slaves data while calibrating each of the slave).
    /// </summary>
    public abstract class DataSourcesCalibrator
    {
        protected ILogger m_Logger;

        /// <summary>
        ///     The master DataSource skeletal manager
        /// </summary>
        protected readonly string m_MasterDataSourceId;

        /// <summary>
        ///     The slave DataSources skeletal managers (one for each slave DataSource)
        /// </summary>
        protected readonly IBodyDataProvider m_BodyDataProvider;

        /// <summary>
        ///     Key joints used to perform calibration
        /// </summary>
        protected BodyJointTypes[] m_KeyJoints =
        {
            //BodyJointTypes.Head,
            BodyJointTypes.Neck,
            BodyJointTypes.SpineMid,
            BodyJointTypes.SpineShoulder,
            BodyJointTypes.ShoulderLeft,
            BodyJointTypes.ShoulderRight,
            BodyJointTypes.ElbowLeft,
            BodyJointTypes.ElbowRight,
            //BodyJointTypes.WristLeft,
            //BodyJointTypes.WristRight
        };

        ///// <summary>
        /////     Name of the file where the found calibration matrices have to be saved
        ///// </summary>
        //public string CalibrationFileName;

        /// <summary>
        ///     Number of last valid calibration matrices to discard. It is used to discard the last matrices, which are dirty
        ///     ones.
        ///     It serves to take the last but n valid calibration matrix for each slave DataSource.
        ///     We don't want the last matrix, because it could be a dirty result, including the movement of the player to
        ///     deactivate the calibration program, so we take the last but n matrix.
        /// </summary>
        public int LastButNthValidMatrix;

        /// <summary>
        ///     Additional rotation around the Y axis, in the degrees, that must be included in the master calibration matrix
        ///     This is used to make possible to put orientation 0 to a position that is far from frontal from the master
        ///     DataSource.
        /// </summary>
        public float AdditionalMasterYRotationAngle;

        /// <summary>
        ///     Height, in meters, of user performing the calibration sequence, used to apply scale correction on DataSource data.
        ///     Set to 0 to not perform this correction
        /// </summary>
        public float CalibratingUserHeight;

        /// <summary>
        /// If true, calibration of slaves will be performed using tracked joints centroid and not all the joints positions. This will make the calibration process more noisy, slow and imprecise,
        /// but can help in calibrating slave tracking boxes that are in positions where the standard calibration algorithm will prove unreliable (e.g. facing kinects)
        /// </summary>
        public bool CalibrateSlavesUsingCentroids;

        /// <summary>
        ///     Vector of calibrators.
        ///     Each calibrator calibrates a slave DataSource wrt the master one
        /// </summary>
        protected Dictionary<string, MasterSlaveDataSourcesCalibrator> m_Calibrators;

        /// <summary>
        ///     Calibrator of the master DataSource: calibrates the master DataSource wrt the world reference frame
        /// </summary>
        protected MasterDataSourceCalibrator m_MasterCalibrator;

        #region Public properties

        /// <summary>
        ///     Gets the master slave calibrators
        /// </summary>
        /// <value>The master slave calibrators</value>
        public Dictionary<string, MasterSlaveDataSourcesCalibrator> MasterSlaveCalibrators
        {
            get { return m_Calibrators; }
        }

        /// <summary>
        ///     Gets the master calibrator.
        /// </summary>
        /// <value>The master calibrator.</value>
        public MasterDataSourceCalibrator MasterCalibrator
        {
            get { return m_MasterCalibrator; }
        }
        
        #endregion

        public DataSourcesCalibrator(string masterDataSourceId, IBodyDataProvider bodyDataProvider)
        {
            m_Logger = LoggerService.GetLogger(typeof(DataSourcesCalibrator));

            Helpers.Requires.NotNull(masterDataSourceId, "masterDataSourceId");
            Helpers.Requires.NotNull(bodyDataProvider, "bodyDataProvider");
            
            m_MasterDataSourceId = masterDataSourceId;
            m_BodyDataProvider = bodyDataProvider;

            m_MasterCalibrator = new MasterDataSourceCalibrator(masterDataSourceId, m_BodyDataProvider, AdditionalMasterYRotationAngle, CalibratingUserHeight);
            m_MasterCalibrator.Logger = m_Logger;

            m_Calibrators = new Dictionary<string, MasterSlaveDataSourcesCalibrator>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Update function executed at each frame rendering
        /// </summary>
        public virtual void Update(double deltaTime)
        {
            // Cycle through the calibrators and update them
            foreach (string dataSourceId in m_BodyDataProvider.DataSources.Keys)
            {
                if (!m_Calibrators.ContainsKey(dataSourceId))
                {
                    m_Calibrators.Add(dataSourceId, new MasterSlaveDataSourcesCalibrator(m_MasterDataSourceId, dataSourceId, m_BodyDataProvider, m_KeyJoints, LastButNthValidMatrix, CalibrateSlavesUsingCentroids));
                }

                m_Calibrators[dataSourceId].Update(deltaTime);
            }

            m_MasterCalibrator.Update(deltaTime);
        }

        /// <summary>
        ///     Function executed at program termination
        /// </summary>
        /// <exception cref="IOException">Called if the writing to file goes bad</exception>
        public CalibrationSettings SaveCalibrationData()
        {
            //save calibration matrices of all DataSources
            //get all clean calibration matrices
            //(clean because they don't contain last data, which is dirty)
            
            var matrices = new Dictionary<string, Matrix4x4>(m_Calibrators.Count, StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < m_Calibrators.Count; i++)
            {
                MasterSlaveDataSourcesCalibrator calibrator = m_Calibrators.ElementAt(i).Value;
                matrices[m_Calibrators.ElementAt(i).Key] = calibrator.LastCleanCalibrationMatrix;
            }

            //put all calibration data inside the appropriate data structure

            var calibrationData = new CalibrationSettings();
            calibrationData.MasterToWorldCalibrationMatrix = MasterCalibrator.CalibrationMatrix;

            if (m_Logger != null && m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("DataSourceCalibrator.SaveData() - MasterToWorldMatrix:\n{0}", calibrationData.MasterToWorldCalibrationMatrix);
            }

            foreach (var matrix in matrices)
            {
                calibrationData.SlaveToMasterCalibrationMatrices.AddOrUpdate(matrix.Key, matrix.Value, (key, existing) => matrix.Value);
            }
            calibrationData.CalibrationDone = true;

            return calibrationData;
        }
    }
}
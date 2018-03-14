namespace ImmotionAR.ImmotionRoom.TrackingEngine.Model
{
    /// <summary>
    ///     Contains all data about calibration of a system consisting of a master DataSource and multiple slave DataSources
    /// </summary>
    public class DataSourcesCalibrationData
    {
        #region Private fields

        /// <summary>
        ///     The matrix that represent the trasformation that maps from master DataSource space to World-Space
        /// </summary>
        private Matrix4x4 m_MasterToWorldCalibrationMatrix;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the slave to master calibration matrices.
        /// </summary>
        /// <value>The slave to master calibration matrices.</value>
        public Matrix4x4[] SlaveToMasterCalibrationMatrices { get; set; }

        /// <summary>
        ///     Gets or sets the master to world calibration matrix.
        /// </summary>
        /// <value>The master to world calibration matrix.</value>
        public Matrix4x4 MasterToWorldCalibrationMatrix
        {
            get { return m_MasterToWorldCalibrationMatrix; }
            set { m_MasterToWorldCalibrationMatrix = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataSourcesCalibrationData" /> class.
        /// </summary>
        public DataSourcesCalibrationData()
        {
            m_MasterToWorldCalibrationMatrix = Matrix4x4.Identity;
            SlaveToMasterCalibrationMatrices = new Matrix4x4[0];
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataSourcesCalibrationData" /> class.
        /// </summary>
        /// <param name="slaveToMasterCalibrationMatrices">Slave DataSources calibration matrices</param>
        /// <param name="masterToWorldCalibrationMatrix">Master DataSource calibration matrix</param>
        public DataSourcesCalibrationData(Matrix4x4[] slaveToMasterCalibrationMatrices, Matrix4x4 masterToWorldCalibrationMatrix)
        {
            SlaveToMasterCalibrationMatrices = slaveToMasterCalibrationMatrices;
            m_MasterToWorldCalibrationMatrix = masterToWorldCalibrationMatrix;
        }

        #endregion
    }
}
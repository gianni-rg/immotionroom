namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.ScreenManagers
{
    using UnityEngine;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedAvateering;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils;

    /// <summary>
    /// Manages Calibration scene behaviour
    /// </summary>
    public partial class Calibration : MonoBehaviour
    {

        #region Public Unity Properties

        /// <summary>
        /// Additional rotation around Y axis, in radians, to be added to found calibration matrix.
        /// This is used to make possible to put orientation 0 to a position that is far from frontal from the master DataSource.
        /// </summary>
        [Tooltip("Additional rotation around Y axis, in radians, to be added to found calibration matrix")]
        public float AdditionalMasterYRotation = 0;

        /// <summary>
        /// True to use calibration methods that uses skeletons centroids, false otherwise
        /// </summary>
        [Tooltip("True to use calibration methods that uses skeletons centroids, false otherwise")]
        public bool CalibrateSlavesUsingCentroids = false;

        /// <summary>
        /// Expected height for the user performing the calibration sequence.
        /// This is useful to correct some errors in DataSource measuring scale.
        /// If it is == 0, this correction is not applied
        /// </summary>
        [Tooltip("Expected height for the user performing the calibration sequence. Leave to 0 to don't apply this correction")]
        public float CalibratingUserHeight = 0;

        /// <summary>
        /// Number of last valid calibration matrices to discard in master/slave calibration, because considered unstable due to
        /// user movement to stop the operation
        /// </summary>
        [Tooltip("Number of last valid calibration matrices to discard in master/slave calibration")]
        public int LastButNthValidMatrix = 10;

        /// <summary>
        /// Object to be used to show the different skeletons during the calibration process
        /// </summary>
        [Tooltip("Object to be used to show the different skeletons during the calibration process")]
        public BodiesSkeletalsManagerAdvanced BodyDrawerPrefab;

        /// <summary>
        /// Joints material to draw skeletal joints with
        /// </summary>
        [Tooltip("Joints material to draw skeletal joints with")]
        public Material SkeletalJointsMaterial;

        /// <summary>
        /// Bones material to draw skeletal bones with
        /// </summary>
        [Tooltip("Bones material to draw skeletal bones with")]
        public Material SkeletalBonesMaterial;

        /// <summary>
        /// UI Element dedicated to show the calibration quality of current calibration
        /// </summary>
        [Tooltip("UI Element dedicated to show the calibration quality of current calibration")]
        public GameObject CalibrationQualityPanel;

        /// <summary>
        /// Score threshold for Auto Stop feature of master-slave calibration.
        /// If calibration score remains above this threshold for the amount of time specified by CalibratinAutoStopTime param, it gets autostopped
        /// </summary>
        [Tooltip("Score threshold for Auto Stop feature of master-slave calibration, in range [0, 1]")]
        public float CalibrationAutoStopScoreThreshold = 0.75f;

        /// <summary>
        /// Time threshold for Auto Stop feature of master-slave calibration.
        /// If calibration score remains above the CalibrationAutoStopScoreThreshold threshold for the amount for this time, it gets autostopped
        /// </summary>
        [Tooltip("Time threshold for Auto Stop feature of master-slave calibration, in seconds")]
        public float CalibrationAutoStopTime = 2.0f;

        /// <summary>
        /// Object responsible for making vibrate the device onto which the calibration is performing
        /// </summary>
        [Tooltip("Object responsible for making vibrate the device onto which the calibration is performing")]
        public Vibrator DeviceVibrationProvider = null;

        #endregion

        #region Private Fields

        /// <summary>
        /// Internal implementation of the class
        /// </summary>
        CalibrationInternal m_internalImplementation;

        #endregion

        #region Behaviour methods

        void Awake()
        {
            m_internalImplementation = new CalibrationInternal(this);
        }

        void Start()
        {
            m_internalImplementation.Start();
        }

        void OnDestroy()
        {
            m_internalImplementation.OnDestroy();
        }

        #endregion

        #region Buttons events methods

        /// <summary>
        /// Callback executed when one of the data sources buttons of the screen gets pressed
        /// </summary>
        /// <param name="dataSourceLabel">Label of the data source button pressed</param>
        /// <param name="newStatus">New status of the button</param>
        private void OnDataSourceButtonClicked(string dataSourceLabel, bool newStatus)
        {
            m_internalImplementation.OnDataSourceButtonClicked(dataSourceLabel, newStatus);            
        }

        /// <summary>
        /// Triggered when the OK button gets clicked
        /// </summary>
        public void OnOkButtonClicked()
        {
            m_internalImplementation.OnOkButtonClicked();
        }

        #endregion
    }

}
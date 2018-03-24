namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.Calibration
{  
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.DataSourcesManagement;
    using ImmotionAR.ImmotionRoom.LittleBoots.VR.Calibration.UI;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using ImmotionAR.ImmotionRoom.LittleBoots.VR.HeadsetManagement;

    /// <summary>
    /// Behaviour performing calibration between current player and the ImmotionRoom system, so that Player can play in VR using ImmotionRoom
    /// </summary>
    [RequireComponent(typeof(HeadsetManager))]
    public partial class IroomPlayerCalibrator : MonoBehaviour
    {
        #region Public Unity properties

        /// <summary>
        /// If true, the player controller will try to re-use last session iroom/hmd calibration data, to shorten calibration procedure.
        /// </summary>
        [Tooltip("If true, the player controller will calibrate only once and then it will try to re-use last session iroom/hmd calibration data, to shorten calibration procedure.")]
        public bool KeepCalibrationData = true;

        /// <summary>
        /// Object to construct to show calibration process on the screen
        /// </summary>
        [Tooltip("Object to construct to show calibration process on the screen. Must contain an object implementing IRoomCalibGuiManager behaviour")]
        public GameObject CalibrationGui;

        /// <summary>
        /// Time that the system should take a break between different stages of calibration to let the user see that he's performed well.
        /// (e.g. to read a "Very good!" message)
        /// </summary>
        [Tooltip("Time that the system should take a break between different stages of calibration to let the user see that he's performed well")]
        public float UserPerformedCorrectlyWaitingTime = 1.2f;

        /// <summary>
        /// Orientation tolerance, in radians, that has to be used to detect if user is facing the master data source.
        /// So, if user orientation, around Y-axis, is below +-m_zeroOrientationTolerance, it is considered as facing the master data source
        /// </summary>
        [Tooltip("Orientation tolerance, in radians, that has to be used to detect if user is facing the master data source")]
        public float ZeroOrientationTolerance = Mathf.Deg2Rad * 21.5f;

        /// <summary>
        /// Seconds that the calibrating user has to stand still after he's returned to initial orientation,
        /// so that the calibration can be actually performed 
        /// </summary>
        [Tooltip("Seconds that the calibrating user has to stand still after he's returned to initial orientation, so that the calibration can be actually performed")]
        public float InitializingStandingTime = 0.66f;

        /// <summary>
        /// Seconds of fading of the calibration canvas after calibration has been performed
        /// </summary>
        [Tooltip("Seconds of fading of the calibration canvas after calibration has been performed")]
        public float CalibrationDoneTime = 1.3f;

        #endregion

        #region Private Fields

        /// <summary>
        /// Internal implementation of the class
        /// </summary>
        private IroomPlayerCalibratorInternal m_internalImplementation;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets if calibration has been correctly performed
        /// </summary>
        public bool CalibrationDone
        {
            get
            {
                return m_internalImplementation.CalibrationDone;
            }
        }

        /// <summary>
        /// Gets the found calibration data. Returns null if no calibration has been done yet
        /// </summary>
        public IroomHeadsetCalibrationData CalibrationData
        {
            get
            {
                return m_internalImplementation.CalibrationData;
            }
        }

        #endregion

        #region Behaviour methods

        void Awake()
        {
            m_internalImplementation = new IroomPlayerCalibratorInternal(this);
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

        #region Public methods

        /// <summary>
        /// Asks the system to re-calibrate itself
        /// </summary>
        public void ReCalibrate()
        {
            m_internalImplementation.ReCalibrate();
        }

        /// <summary>
        /// Signal to this object that the player controller has already made all its operations, so the system can make
        /// the calibration GUI disappear with a fading effect
        /// </summary>
        public void SignalPlayerReady()
        {
            m_internalImplementation.SignalPlayerReady();
        }

        #endregion
    }
}

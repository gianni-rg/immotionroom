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
    using ImmotionAR.ImmotionRoom.LittleBoots.VR.Calibration.DataManagement;

    /// <summary>
    /// Behaviour performing calibration between current player and the ImmotionRoom system, so that Player can play in VR using ImmotionRoom
    /// </summary>
    [RequireComponent(typeof(HeadsetManager))]
    public partial class IroomPlayerCalibrator : MonoBehaviour
    {
        /// <summary>
        /// Contains the actual implementation of the IroomPlayerCalibrator, for obfuscation purposes
        /// </summary>
        private class IroomPlayerCalibratorInternal
        {

            #region Private fields

            /// <summary>
            /// Data provider of current scene as seen by the Tracking Service
            /// </summary>
            private SceneDataProvider m_sceneDataProvider;

            /// <summary>
            /// Actual calibrator of ImmotionRoom w.r.t. the headset
            /// </summary>
            private IroomHeadsetCalibrator m_calibrator;

            /// <summary>
            /// Actual calibration GUI manager
            /// </summary>
            private GameObject m_calibrationGuiManager;

            /// <summary>
            /// True if the player controller already signaled that it's ready and so the calibration GUI can start fading
            /// </summary>
            private bool m_playerReady;

            /// <summary>
            /// The IroomPlayerCalibrator object that contains this object
            /// </summary>
            private IroomPlayerCalibrator m_enclosingInstance;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="enclosingInstance">Enclosing instance, whose code has to be implemented</param>
            internal IroomPlayerCalibratorInternal(IroomPlayerCalibrator enclosingInstance)
            {
                m_enclosingInstance = enclosingInstance;
            }

            #endregion

            #region Internal properties

            /// <summary>
            /// Gets if calibration has been correctly performed
            /// </summary>
            internal bool CalibrationDone
            {
                get
                {
                    return m_calibrator != null && m_calibrator.CalibrationDone;
                }
            }

            /// <summary>
            /// Gets the found calibration data. Returns null if no calibration has been done yet
            /// </summary>
            internal IroomHeadsetCalibrationData CalibrationData
            {
                get
                {
                    if (m_calibrator == null)
                        return null;
                    else
                        return m_calibrator.CalibrationData;
                }
            }

            #endregion

            #region Behaviour methods

            internal void Start()
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("IroomPlayerCalibrator - Started");
                }

                //if last calibration data has not be used, delete it
                if (!m_enclosingInstance.KeepCalibrationData)
                    CalibrationDataManager.OnlineSessionCalibrationData = null;

                m_enclosingInstance.StartCoroutine(Initialize());
            }

            internal void OnDestroy()
            {
                m_enclosingInstance.StopAllCoroutines();

                if (m_sceneDataProvider != null)
                    m_sceneDataProvider.Dispose();
            }

            #endregion

            #region Internal methods

            /// <summary>
            /// Asks the system to re-calibrate itself
            /// </summary>
            internal void ReCalibrate()
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("IroomPlayerCalibrator - Asked to re-calibrate");
                }

                //if last calibration data has not be used, delete it
                if (!m_enclosingInstance.KeepCalibrationData)
                    CalibrationDataManager.OnlineSessionCalibrationData = null;

                m_calibrator.Reset(CalibrationDataManager.OnlineSessionCalibrationData);

                //start system calibration
                m_enclosingInstance.StartCoroutine(Calibrate());
            }

            /// <summary>
            /// Signal to this object that the player controller has already made all its operations, so the system can make
            /// the calibration GUI disappear with a fading effect
            /// </summary>
            internal void SignalPlayerReady()
            {
                m_playerReady = true;
            }

            #endregion

            #region Private coroutines

            /// <summary>
            /// Connect to tracking service and initialize the scene data provider.
            /// Then create the calibrator object and calibration gui
            /// </summary>
            /// <returns></returns>
            private IEnumerator Initialize()
            {
                //create calibration gui (notice that instantiate a script means instantiating all its belonging object)
                m_calibrationGuiManager = Instantiate<GameObject>(m_enclosingInstance.CalibrationGui);

                //wait for tracking service connection and tracking
                while (!TrackingServiceManagerBasic.Instance.IsTracking)
                    yield return new WaitForSeconds(0.1f);

                //create the body provider, waiting for it to begin
                while ((m_sceneDataProvider = TrackingServiceManagerBasic.Instance.StartSceneDataProvider()) == null)
                    yield return new WaitForSeconds(0.1f);

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("IroomPlayerCalibrator - Connected to Tracking Service and Initialized");
                }

                yield return new WaitForSeconds(0.1f); //to give time to initialize the data provider

                //create actual calibrator
                m_calibrator = new IroomHeadsetCalibrator(m_sceneDataProvider, TrackingServiceManagerBasic.Instance.TrackingServiceEnvironment,
                                                          m_calibrationGuiManager.GetComponentInChildren<IRoomCalibGuiManager>(), m_enclosingInstance.GetComponent<HeadsetManager>(), m_enclosingInstance.UserPerformedCorrectlyWaitingTime, m_enclosingInstance.ZeroOrientationTolerance,
                                                          m_enclosingInstance.InitializingStandingTime, m_enclosingInstance.CalibrationDoneTime,
                                                          CalibrationDataManager.OnlineSessionCalibrationData);

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("IroomPlayerCalibrator - Created Calibrator and Calibration GUI");
                }

                //start system calibration
                m_enclosingInstance.StartCoroutine(Calibrate());

                yield break;
            }

            /// <summary>
            /// Calibrate the system
            /// </summary>
            /// <returns></returns>
            private IEnumerator Calibrate()
            {
                //the player is still not ready 
                m_playerReady = false;

                //update the calibrator, if any, until the system is calibrated
                while (m_calibrator != null && !m_calibrator.CalibrationDone)
                {
                    m_calibrator.Update(Time.deltaTime);
                    yield return 0; //wait until next frame
                }

                //save found calibration data
                CalibrationDataManager.OnlineSessionCalibrationData = m_calibrator.CalibrationData;

                //when it is calibrated, wait until it exits. Notice that we don't update the calibrator if
                //we don't have the signal that the player is ready to play, so that fading ends only while init operations are finished
                while (m_calibrator != null && !m_calibrator.CalibrationExited)
                {
                    if (m_playerReady)
                        m_calibrator.Update(Time.deltaTime);

                    yield return 0; //wait until next frame
                }

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("IroomPlayerCalibrator - System calibrated");
                }

                yield break;
            }

            #endregion
        }

    }
}

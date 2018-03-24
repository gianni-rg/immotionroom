namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.Calibration
{
    using UnityEngine;
    using UnityEngine.UI;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d;
    using System.Collections.Generic;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Tools;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.DataSourcesManagement;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.SupportStruct;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.LittleBoots.VR.HeadsetManagement;
    using System;
    using ImmotionAR.ImmotionRoom.LittleBoots.VR.Calibration.UI;

    /// <summary>
    /// Class whose goal is to perform the calibration between a the reference system of ImmotionRoom and the one of a generic headset,
    /// so that a player can live inside a virtual reality world.
    /// The main frame of reference is the one of the headset... so the class finds the matrix that maps ImmotionRoom reference frame
    /// to the headset one
    /// </summary>
    internal class IroomHeadsetCalibrator
    {
        #region Constants definition

        /// <summary>
        /// Maximum distance of tracked body from the world space origin, to be considered valid
        /// (this avoids tracking body of spectators of the system if they are far away)
        /// </summary>
        private const float MaxInitialXZDistanceFromOrigin = 1.0f;

        #endregion

        #region Private fields

        /// <summary>
        /// Current status of the calibrator
        /// </summary>
        private IroomCalibratorStatus m_status;

        /// <summary>
        /// Actual tracker of people that uses a system of kinects
        /// </summary>
        private SceneDataProvider m_sceneData;

        /// <summary>
        /// Provider of the tracking data for current calibrating body (if any)
        /// </summary>
        private BodyDataProvider m_calibratingBody;

        /// <summary>
        /// Object analyzing calibrating body skeleton, to return useful data during calibration process
        /// </summary>
        private CalibratingBodyAnalyzer m_calibratingBodyAnalyzer;

        /// <summary>
        /// Time accumulator member, for various purposes
        /// </summary>
        private float m_currentOperatingTime;

        /// <summary>
        /// Environment data about the tracking service tracking operation. This is useful especially to get
        /// the minimum number of tracking boxes in the system tracking the player to consider valid the calibration.
        /// Until we don't find a body tracked by at least this number of data sources, calibration is not even started
        /// </summary>
        private TrackingServiceEnv m_trackingServiceEnvironment;

        /// <summary>
        /// Manager of the UI used to display informations during calibration stage
        /// </summary>
        private IRoomCalibGuiManager m_calibrationGuiManager;

        /// <summary>
        /// Scene headset we're calibrating wrt ImmotionRoom system
        /// </summary>
        private IHeadsetManager m_headset;

        #endregion

        #region Private customization fields

        /// <summary>
        /// Time that the system should take a break between different stages of calibration to let the user see that he's performed well.
        /// (e.g. to read a "Very good!" message)
        /// </summary>
        private float m_userPerformedCorrectlyWaitingTime;

        /// <summary>
        /// Orientation tolerance, in radians, that has to be used to detect if user is facing the master data source.
        /// So, if user orientation, around Y-axis, is below +-m_zeroOrientationTolerance, it is considered as facing the master data source
        /// </summary>
        private float m_zeroOrientationTolerance;

        /// <summary>
        /// Seconds that the calibrating user has to stand still after he's returned to initial orientation,
        /// so that the calibration can be actually performed 
        /// </summary>
        private float m_initializingStandingTime;

        /// <summary>
        /// Seconds of fading of the calibration canvas after calibration has been performed
        /// </summary>
        private float m_calibrationDoneTime ;

        #endregion

        #region Private found Calibration data

        /// <summary>
        /// Found calibration data;
        /// </summary>
        private IroomHeadsetCalibrationData m_calibrationData;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets if calibration has been correctly performed
        /// </summary>
        public bool CalibrationDone
        {
            get
            {
                return m_status == IroomCalibratorStatus.Done;
            }
        }

        /// <summary>
        /// Gets if calibration has been correctly performed and we've finished all GUI operations about it
        /// </summary>
        public bool CalibrationExited
        {
            get
            {
                return m_status == IroomCalibratorStatus.Exit;
            }
        }

        /// <summary>
        /// Gets the found calibration data. Returns null if no calibration has been done yet
        /// </summary>
        public IroomHeadsetCalibrationData CalibrationData
        {
            get
            {
                return m_calibrationData;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="IroomHeadsetCalibrator"/> class.
        /// </summary>
        /// <param name="sceneData">Actual tracker of people that uses a system of kinects</param>
        /// <param name="trackingServiceEnvironment">Environment data about the tracking service tracking operation. This is useful especially to get the minimum number of tracking boxes in the system tracking the player to consider valid the calibration</param>
        /// <param name="calibrationGuiManager">Manager of the UI used to display informations during calibration stage</param>
        /// <param name="headset">Scene headset we're calibrating wrt ImmotionRoom system</param>
        /// <param name="userPerformedCorrectlyWaitingTime">Time that the system should take a break between different stages of calibration to let the user see that he's performed well</param>
        /// <param name="zeroOrientationTolerance">Orientation tolerance, in radians, that has to be used to detect if user is facing the master data source</param>
        /// <param name="initializingStandingTime">Seconds that the calibrating user has to stand still after he's returned to initial orientation, so that the calibration can be actually performed</param>
        /// <param name="calibrationDoneTime">Seconds of fading of the calibration canvas after calibration has been performed</param>
        /// <param name="toRecoverData">Previous calibration data to re-use. If this value is != null, the calibrator will verify if this data can still be used and if the answer is yes, it will re-use it</param>
        internal IroomHeadsetCalibrator(SceneDataProvider sceneData, TrackingServiceEnv trackingServiceEnvironment, IRoomCalibGuiManager calibrationGuiManager, IHeadsetManager headset,
                                        float userPerformedCorrectlyWaitingTime, float zeroOrientationTolerance, float initializingStandingTime, float calibrationDoneTime,
                                        IroomHeadsetCalibrationData toRecoverData = null)
        {
            if(Log.IsDebugEnabled)
            {
                Log.Debug("IroomHeadsetCalibrator - Constructing");
            }

            //initialize everything
            m_sceneData = sceneData;
            m_trackingServiceEnvironment = trackingServiceEnvironment;
            m_calibrationGuiManager = calibrationGuiManager;
            m_headset = headset;
            m_userPerformedCorrectlyWaitingTime = userPerformedCorrectlyWaitingTime;
            m_zeroOrientationTolerance = zeroOrientationTolerance;
            m_initializingStandingTime = initializingStandingTime;
            m_calibrationDoneTime = calibrationDoneTime;

            this.Reset(toRecoverData);
        }

        #endregion 

        #region Reset method

        /// <summary>
        /// Reset this calibrator instance
        /// </summary>
        /// <param name="toRecoverData">Previous calibration data to re-use. If this value is != null, the calibrator will verify if this data can still be used and if the answer is yes, it will re-use it</param>
        internal void Reset(IroomHeadsetCalibrationData toRecoverData)
        {
            //reset all data (we simply delete all the current calibration data and reset the GUI, but we keep the provider and the headset)
            m_calibratingBody = null; //we don't follow current body anymore
            m_calibratingBodyAnalyzer = null;
            m_status = IroomCalibratorStatus.None;
            m_currentOperatingTime = 0;
            m_calibrationData = null;
            m_calibrationGuiManager.Activate();
            m_calibrationGuiManager.ResetGui();

            //if recovery data was provided
            if (toRecoverData != null && m_sceneData.IsStillValid)
            {
                //get calibration body and see if it is still valid
                m_calibratingBody = new BodyDataProvider(m_sceneData, toRecoverData.UserBodyId);
                m_calibratingBodyAnalyzer = new CalibratingBodyAnalyzer(m_calibratingBody);

                //if it is still valid and tracked by enough data sources
                if (m_calibratingBody.LastBody != null && m_calibratingBody.LastBody.DataSources.Count >= m_trackingServiceEnvironment.MinDataSourcesForPlayer)
                {
                    //we're calibrated. Just go to final stage and wait for avatar initialization
                    m_calibrationData = toRecoverData;
                    m_status = IroomCalibratorStatus.Calibrating;                    
                    m_calibrationGuiManager.GoToStatus(m_status);
                    m_currentOperatingTime = 0;

                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("IroomHeadsetCalibrator - Reset performed using still valid body ID {0}", m_calibratingBody.LastBody.Id);
                    }

                    return;
                }
                //else, revert assigned variables and perform standard calibration
                else
                {
                    m_calibratingBody = null;
                    m_calibratingBodyAnalyzer = null;
                }
            }

            m_calibrationGuiManager.InitWaitingBodyTrackingInfo(m_trackingServiceEnvironment); //to initialize the gui            
            m_calibrationGuiManager.GoToStatus(IroomCalibratorStatus.WaitingForBody); //first state here is waiting for body            
            m_status = IroomCalibratorStatus.WaitingForBody;

            if(Log.IsDebugEnabled)
            {
                Log.Debug("IroomHeadsetCalibrator - Reset performed. Now waiting for a body tracked by {0} data sources", m_trackingServiceEnvironment.MinDataSourcesForPlayer);
            }
        }

        #endregion

        #region Update methods

        /// <summary>
        /// Update the calibrator with last skeletal and oculus data.
        /// Calibration happens in a 4-step fashion:
        /// Step 1: User must show himself to all kinects of the system
        /// Step 2: User must return to look at master kinect
        /// Step 3: User must stand erect and still for a little time
        /// Step 4: System computes calibration data and then calibration end
        /// </summary>
        /// <param name="timeDelta">Time delta from last call of Update</param>
        internal void Update(float timeDelta)
        {

            switch (m_status)
            {
                case IroomCalibratorStatus.WaitingForBody:
                    WaitingForBodyUpdate(timeDelta);
                    break;

                case IroomCalibratorStatus.EndOfWaiting:
                    EndOfWaitingUpdate(timeDelta);
                    break;

                case IroomCalibratorStatus.RotatingBodyToOrigin:
                    RotatingBodyUpdate(timeDelta);
                    break;

                case IroomCalibratorStatus.EndOfRotating:
                    EndOfRotatingBodyUpdate(timeDelta);
                    break;

                case IroomCalibratorStatus.BodyStandingStill:
                    BodyStandingStillUpdate(timeDelta);
                    break;

                case IroomCalibratorStatus.InitializingWithBody:
                    InitializingUpdate(timeDelta);
                    break;

                case IroomCalibratorStatus.EndOfInitialization:
                    EndOfInitializingUpdate(timeDelta);
                    break;

                case IroomCalibratorStatus.Calibrating:
                    CalibratingUpdate(timeDelta);
                    break;

                case IroomCalibratorStatus.Done:
                    DoneUpdate(timeDelta);
                    break;
            }
        }

        /// <summary>
        /// Performs the update of current object, during waiting stage, where a body that gets tracked by all data sources 
        /// has to be found, and that is near enough to the system origin (so that it can't be just a spectator). 
        /// The first body that reaches this goal, is considered the body wearing the headset
        /// </summary>
        /// <param name="timeDelta">Time delta from last call of WaitingUpdate</param>
        private void WaitingForBodyUpdate(float timeDelta)
        {
            //get list of body tracked by the system in this frame (if any)
            IList<TrackingServiceBodyData> foundBodies = m_sceneData.LastBodies;

            if (!m_sceneData.IsStillValid || foundBodies == null || foundBodies.Count == 0)
                return;

            //we must find exactly one body tracked by all data sources and then switch to initialization stage, so loop each tracked body
            //and see which body is the best in respecting this pre-requisites

            TrackingServiceBodyData bestTrackedBody = null;
            int bestTrackedBodyNumOfDataSources = 0;
            
            foreach (TrackingServiceBodyData body in foundBodies)
            {
                if (body.Joints.Count == 0)
                    continue;

                //consider only bodies near the world origin (we consider only the XZ plane). Ignore this body if too far
                Vector3 BodyPositionXYZ = body.Joints[TrackingServiceBodyJointTypes.SpineMid].ToVector3();
                Vector2 BodyPositionXZ = new Vector2(BodyPositionXYZ.x, BodyPositionXYZ.z);

                if (BodyPositionXZ.magnitude > MaxInitialXZDistanceFromOrigin)
                    continue;

                //if a body is ok for the position, check the merging bodies number

                //save the body with the greatest number of tracking data sources
                if (body.NumberOfMergedBodies > bestTrackedBodyNumOfDataSources) 
                {
                    bestTrackedBodyNumOfDataSources = body.NumberOfMergedBodies;
                    bestTrackedBody = body;
                }

            }

            //update calibration canvas showing the data about the best tracked body
            m_calibrationGuiManager.ShowWaitingBodyTrackingInfo(bestTrackedBody, m_trackingServiceEnvironment.MinDataSourcesForPlayer);

            //if the best body respects the tracking requisites, save a reference to the found body (it will be the body used for calibration)
            //and go to next stage
            if (bestTrackedBodyNumOfDataSources >= m_trackingServiceEnvironment.MinDataSourcesForPlayer)
            {
                if(Log.IsDebugEnabled)
                {
                    Log.Debug("IroomHeadsetCalibrator - Body with ID {0} has been tracked by enough data sources. Switching to {1} stage.", bestTrackedBody.Id, IroomCalibratorStatus.EndOfWaiting.ToString());
                }

                m_calibratingBody = new BodyDataProvider(m_sceneData, bestTrackedBody.Id);
                m_calibratingBodyAnalyzer = new CalibratingBodyAnalyzer(m_calibratingBody);
                m_status = IroomCalibratorStatus.EndOfWaiting;
                m_calibrationGuiManager.GoToStatus(m_status);
                m_currentOperatingTime = 0;

                //show on the GUI that user has performed well
                m_calibrationGuiManager.ShowVeryGoodMessage();
            }
        }

        /// <summary>
        /// Update method for a generic stage when the system should make a pause for some time and then go to a certain status
        /// (used very often during transitions between different stages)
        /// </summary>
        /// <param name="timeDelta">Time delta from last call of WaitAndChangeStatusUpdate</param>
        /// <param name="waitingTime">Total time we have to wait</param>
        /// <param name="toGoStatus">Status to go after we have waited for the required time</param>
        private void WaitAndChangeStatusUpdate(float timeDelta, float waitingTime, IroomCalibratorStatus toGoStatus)
        {
            m_currentOperatingTime += timeDelta;

            if (m_currentOperatingTime >= waitingTime)
            {
                if(Log.IsDebugEnabled)
                {
                    Log.Debug("IroomHeadsetCalibrator - Waited for {0}s. Switching to {1} stage.", waitingTime, toGoStatus.ToString());
                }

                m_currentOperatingTime = 0;

                m_status = toGoStatus;
                m_calibrationGuiManager.GoToStatus(toGoStatus);
            }

        }

        /// <summary>
        /// Performs the update of current object, during waiting after a valid body has to be found.
        /// We just wait a bit doing nothing, to let the user see on the GUI that a body has been found
        /// </summary>
        /// <param name="timeDelta">Time delta from last call of EndOfWaitingUpdate</param>
        private void EndOfWaitingUpdate(float timeDelta)
        {
            WaitAndChangeStatusUpdate(timeDelta, m_userPerformedCorrectlyWaitingTime, IroomCalibratorStatus.RotatingBodyToOrigin);
        }

        /// <summary>
        /// Performs the update of current object, during rotating stage, where the calibrating user should return his body
        /// to an orientation that faces the master kinect
        /// </summary>
        /// <param name="timeDelta">Time delta from last call of RotatingBodyUpdate</param>
        private void RotatingBodyUpdate(float timeDelta)
        {
            //if the person can't be found anymore, go back to waiting stage
            if (!m_sceneData.IsStillValid || m_calibratingBody.LastBody == null)
            {
                if(Log.IsDebugEnabled)
                {
                    Log.Debug("IroomHeadsetCalibrator - Body tracking lost. Returning to initial stage");
                }

                m_calibrationGuiManager.InitWaitingBodyTrackingInfo(m_trackingServiceEnvironment); //to initialize the gui            
                m_calibrationGuiManager.GoToStatus(IroomCalibratorStatus.WaitingForBody); //first state here is waiting for body            
                m_status = IroomCalibratorStatus.WaitingForBody;

                return;
            }

            //get orientation of current body
            float angle = m_calibratingBodyAnalyzer.GetCalibratingUserRotationAngle();

            //if orientation is inside tolerance, go to next stage
            if (Mathf.Abs(angle) < m_zeroOrientationTolerance)
            {
                if(Log.IsDebugEnabled)
                {
                    Log.Debug("IroomHeadsetCalibrator - Body is now orientated corretly (it's at {0} degrees). Going to {1} stage", angle * Mathf.Rad2Deg, IroomCalibratorStatus.EndOfRotating.ToString());
                }

                m_currentOperatingTime = 0;
                m_status = IroomCalibratorStatus.EndOfRotating;
                m_calibrationGuiManager.GoToStatus(m_status);
                m_calibrationGuiManager.ShowVeryGoodMessage();
                m_calibrationGuiManager.ShowCalibratingBodyOrientationsDir(0); //show on the GUI that orientation is correct
            }
            //else, if it is wrong, show directions he has to rotate towards
            else
                m_calibrationGuiManager.ShowCalibratingBodyOrientationsDir(angle);

        }

        /// <summary>
        /// Performs the update of current object, during waiting after the user has rotated to face the correct direction
        /// </summary>
        /// <param name="timeDelta">Time delta from last call of EndOfRotatingBodyUpdate</param>
        private void EndOfRotatingBodyUpdate(float timeDelta)
        {
            //check that calibrating body is still valid
            if (!CheckCurrentBodyStatus())
                return;

            //perform a waiting operation
            WaitAndChangeStatusUpdate(timeDelta, m_userPerformedCorrectlyWaitingTime, IroomCalibratorStatus.BodyStandingStill);
        }

        /// <summary>
        /// Performs the update of current object, during the stage the user has faced the correct direction
        /// and begins to read the new screen instructions
        /// </summary>
        /// <param name="timeDelta">Time delta from last call of BodyStandingStillUpdate</param>
        private void BodyStandingStillUpdate(float timeDelta)
        {
            //check that calibrating body is still valid
            if (!CheckCurrentBodyStatus())
                return;

            //perform a waiting operation
            WaitAndChangeStatusUpdate(timeDelta, m_userPerformedCorrectlyWaitingTime, IroomCalibratorStatus.InitializingWithBody);
        }

        /// <summary>
        /// Performs the update of current object, during initializing stage, where the body found in waiting stage
        /// must stand still and erect for a certain amount of time, so that calibration can begin.
        /// </summary>
        /// <param name="timeDelta">Time delta from last call of InitializingUpdate</param>
        private void InitializingUpdate(float timeDelta)
        {
            //check that calibrating body is still valid
            if (!CheckCurrentBodyStatus())
                return;

            //if the body have been found, check if stands still and erect
            bool playerIsStillAndErect = m_calibratingBodyAnalyzer.CalibratingUserStandingStill();

            // show information about calibration on the GUI
            m_calibrationGuiManager.ShowCurrentStateProgressBarValue(Mathf.Min(1.0f, m_currentOperatingTime / m_initializingStandingTime));

            //if movement has been found, reset user current standing time
            if (playerIsStillAndErect)
                m_currentOperatingTime = 0;
            //if no movement has been found, increment user current standing time and check if he stood still for enough
            //time to go to the actual calibration stage
            else
            {
                m_currentOperatingTime += timeDelta;

                if (m_currentOperatingTime >= m_initializingStandingTime)
                {
                    if(Log.IsDebugEnabled)
                    {
                        Log.Debug("IroomHeadsetCalibrator - Body has been standing still for enough time. Going to {0} stage", IroomCalibratorStatus.EndOfInitialization.ToString());
                    }

                    m_status = IroomCalibratorStatus.EndOfInitialization; //change status
                    m_calibrationGuiManager.GoToStatus(m_status);
                    m_currentOperatingTime = 0;

                    //show on the GUI that user has performed well
                    m_calibrationGuiManager.ShowVeryGoodMessage();
                }
            }
        }

        /// <summary>
        /// Performs the update of current object, during the stage the user has completed all the step before the actual
        /// calibration algo can be started, and he has to perform a break to read the GUI
        /// </summary>
        /// <param name="timeDelta">Time delta from last call of EndOfInitializingUpdate</param>
        private void EndOfInitializingUpdate(float timeDelta)
        {
            //check that calibrating body is still valid
            if (!CheckCurrentBodyStatus())
                return;

            WaitAndChangeStatusUpdate(timeDelta, m_userPerformedCorrectlyWaitingTime, IroomCalibratorStatus.Calibrating);
        }

        /// <summary>
        /// Calculates calibration of the tracking service system with respect to headset and then gives user the time to handle the transition.
        /// The system transitions immediately to next step
        /// </summary>
        /// <param name="timeDelta">Time delta from last call of CalibratingUpdate</param>
        private void CalibratingUpdate(float timeDelta)
        {
            //if we're here and calib is null, it does mean that we're performing a standard calibration, so go on
            if(m_calibrationData == null)
            {
                //check that calibrating body is still valid
                if (!CheckCurrentBodyStatus())
                    return;

                //calibrate everything at first frame of this state
                CalibrateKinectOculus();
            }
            //if it is not null, it is because we're try to re-use previously saved calibration data
            else
            {
                //perform simpler calibration using provided data
                CalibrateKinectOculusSavedCalib();
            }

            //change immediately go to next state  
            m_status = IroomCalibratorStatus.Done;
            m_calibrationGuiManager.GoToStatus(m_status);
               
            m_currentOperatingTime = 0;

            if(Log.IsDebugEnabled)
            {
                Log.Debug("IroomHeadsetCalibrator - Calibration performed. Calibration matrix is:\n{0}\nUser height is: {1}", m_calibrationData.CalibrationMatrix, m_calibrationData.UserHeight);
            }
        }

        /// <summary>
        /// Performs the update of current object, when the object has finished calibrating and only a transition to make the 
        /// calibration GUI has to be performed
        /// </summary>
        /// <param name="timeDelta">Time delta from last call of EndOfInitializingUpdate</param>
        private void DoneUpdate(float timeDelta)
        {
            m_currentOperatingTime += timeDelta;

            //after a bunch of seconds, calibrate and set calibration to done state
            if (m_currentOperatingTime >= m_calibrationDoneTime)
            {
                m_calibrationGuiManager.MakeGuiFade(1.0f);
                DoneWithCalibration();
            }
            //while this time has not passed, just make the GUI slowly disappear
            else
            {
                m_calibrationGuiManager.MakeGuiFade(m_currentOperatingTime / m_calibrationDoneTime);
            }
            
        }

        /// <summary>
        /// Set everything that has to be set once the calibration has been done
        /// (e.g. the calibration screen gets deleted)
        /// </summary>
        private void DoneWithCalibration()
        {
            if(Log.IsDebugEnabled)
            {
                Log.Debug("IroomHeadsetCalibrator - Calibration completed");
            }

            m_status = IroomCalibratorStatus.Exit;
            m_calibrationGuiManager.Deactivate();
        }

        #endregion

        #region Private helper methods

        /// <summary>
        /// Checks if current calibrating body still exists and if still has correct orientation (he's pointing the master kinect).
        /// If checks fail, the system is forced to return to the appropriate statuses
        /// </summary>
        /// <returns>True if current body is ok, false otherwise</returns>
        private bool CheckCurrentBodyStatus()
        {
            //if the person can't be found anymore, go back to waiting stage
            if (!m_sceneData.IsStillValid || m_calibratingBody.LastBody == null)
            {
                if(Log.IsDebugEnabled)
                {
                    Log.Debug("IroomHeadsetCalibrator - Body tracking lost. Returning to initial stage");
                }

                m_currentOperatingTime = 0;
                m_calibrationGuiManager.InitWaitingBodyTrackingInfo(m_trackingServiceEnvironment); //to initialize the gui            
                m_calibrationGuiManager.GoToStatus(IroomCalibratorStatus.WaitingForBody); //first state here is waiting for body            
                m_status = IroomCalibratorStatus.WaitingForBody;

                return false;
            }

            //if tracking body has not correct orientation any more, go back to rotation stage
            float angle = m_calibratingBodyAnalyzer.GetCalibratingUserRotationAngle();

            if (Mathf.Abs(angle) > m_zeroOrientationTolerance)
            {
                if(Log.IsDebugEnabled)
                {
                    Log.Debug("IroomHeadsetCalibrator - Body lost required orientation. Returning to rotation stage");
                }

                m_currentOperatingTime = 0;
                m_status = IroomCalibratorStatus.RotatingBodyToOrigin;
                m_calibrationGuiManager.GoToStatus(m_status);
                return false;
            }

            //everything is ok, return true
            return true;
        }

        #endregion 

        #region Actual calibration methods

        /// <summary>
        /// Calibrates the kinect system with the oculus goggles
        /// </summary>
        private void CalibrateKinectOculus()
        {
            //code inside this class, like this name has been leaved with reference to Kinect and Oculus, as in original formulation

            //ok, let's begin calibration of master kinect

            //recenter the Rift: now the user head is in the origin, with orientation 0 
            //(or at least it should be so, but in some headsets it doesn't work, so get returned actual headset rotation matrix)
            Quaternion headsetResetRotation = m_headset.ResetView();

            //create calibration data struct
            m_calibrationData = new IroomHeadsetCalibrationData();

            //add the calibrated body inside the struct
            m_calibrationData.UserBodyId = m_calibratingBody.LastBody.Id;

            //if kinect calibration has been done well, the situation is now the following:
            //XZ plane of kinects is parallel to the one of the Oculus Goggles, so it is perfect
            //forward vector of kinects (Z axis) points to a certain direction, while forward vector of Oculus is (0, 0, 1)
            //origin point of Kinects frame of reference is on certain point on the floor, while Oculus one is on the user head. 
            //Notice that in reality, Oculus position is the one inside the Unity scene (if someone has moved the CameraRig, Oculus is not in the origin anymore)

            //so, we must find orientation of Z axis of kinects in Oculus reference frame
            //and the origin of Kinect system in Oculus reference frame

            //at first, compute matrix representing orientation of Z axis using player shoulders.
            //Moltiplicate it by the inverse of the actual reset rotation of the headset (should be zero for most of them, but Vive is not like this, so...)
            //(notice that we take only the Y component of this last rotation)
            Matrix4x4 rotationMatrix = m_calibratingBodyAnalyzer.GetCalibratingUserRotationMatrix() * 
                                       Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(headsetResetRotation.eulerAngles.y, Vector3.up), Vector3.one).inverse;

            //set the found calibration matrix, so it can be used by the outer world
            m_calibrationData.CalibrationRotationMatrix = rotationMatrix.inverse;

            //ok, now let's find user height 
            m_calibrationData.UserHeight = m_calibratingBodyAnalyzer.GetCalibratingUserHeight();

            //find position of body in the xy plane for kinects
            //use head point for this purpose
            Vector3 headPos = m_calibratingBody.LastBody.Joints[TrackingServiceBodyJointTypes.Head].ToUnityVector3NoScale();

            //calculate translation between oculus and kinects as follows (remember that oculus are recentered, so they stay in their origin)
            //xz translation is the just found spineMidPos
            //y translation is the player height minus the distance between the top of the head and the eyes
            m_calibrationData.CalibrationTranslationMatrix = Matrix4x4.TRS(m_headset.PositionInGame - new Vector3(headPos.x, (headPos.y - CalibratingBodyAnalyzer.HeadToEyeHeight), headPos.z), Quaternion.identity, Vector3.one);

            //calculate compound calibration matrix as the composition of the two
            m_calibrationData.CalibrationMatrix = m_calibrationData.CalibrationTranslationMatrix * m_calibrationData.CalibrationRotationMatrix;
        }

        /// <summary>
        /// Calibrates the kinect system with the oculus goggles, using previous calibration data
        /// </summary>
        private void CalibrateKinectOculusSavedCalib()
        {
            //since the old calibration data is still ok, we have only to recompute translation matrix,
            //because we don't know where this new player controller has been put in the scene.
            //(since we don't reset oculus orientation, old rotation matrix is still ok)

            //find position of body in the xy plane for kinects
            //use head point for this purpose
            Vector3 headPos = m_calibratingBody.LastBody.Joints[TrackingServiceBodyJointTypes.Head].ToUnityVector3NoScale();

            //calculate translation between oculus and kinects as follows (remember that oculus are recentered, so they stay in their origin)
            //xz translation is the just found spineMidPos
            //y translation is the player height minus the distance between the top of the head and the eyes
            m_calibrationData.CalibrationTranslationMatrix = Matrix4x4.TRS(m_headset.PositionInGame - new Vector3(headPos.x, (headPos.y - CalibratingBodyAnalyzer.HeadToEyeHeight), headPos.z), Quaternion.identity, Vector3.one);

            //calculate compound calibration matrix as the composition of the two
            m_calibrationData.CalibrationMatrix = m_calibrationData.CalibrationTranslationMatrix * m_calibrationData.CalibrationRotationMatrix;
        }

        #endregion
    }

}

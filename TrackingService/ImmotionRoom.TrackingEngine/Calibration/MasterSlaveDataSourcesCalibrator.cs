namespace ImmotionAR.ImmotionRoom.TrackingEngine.Calibration
{
    using System.Collections.Generic;
    using Model;
    using ImmotionAR.ImmotionRoom.TrackingEngine.Interfaces;
    using Tools;

    /// <summary>
    ///     Calibrates a slave DataSource with the master DataSource, using single body informations.
    /// </summary>
    public class MasterSlaveDataSourcesCalibrator
    {
        /// <summary>
        ///     Seconds that the calibrating user has to stand still so that the calibration can begin
        /// </summary>
        private const float InitializingStandingTime = 0.9f; // GIANNI: was. 2.0

        /// <summary>
        ///     Tolerance, in meters, of user movements to be still considered "still" (no pun intended)
        ///     during initialization stage
        /// </summary>
        private const float StandingMovementTolerance = 0.35f; // GIANNI: was 0.15

        /// <summary>
        ///     Squared value of StandingMovementTolerance
        /// </summary>
        private const float SquaredStandingMovementTolerance = StandingMovementTolerance*StandingMovementTolerance;

        /// <summary>
        ///     Time between consecutive sampling of joints during the tracking/calibration stage
        /// </summary>
        private const float JointsSamplingTime = 0.109f;

        /// <summary>
        ///     The master DataSource skeletal tracker
        /// </summary>
        private readonly string m_MasterDataSourceId;

        /// <summary>
        ///     The slave DataSource skeletal tracker
        /// </summary>
        private readonly string m_SlaveDataSourceId;

        /// <summary>
        ///     Body, on the master DataSource, used for calibration
        /// </summary>
        private BodyData m_MasterBody;

        /// <summary>
        ///     Body, on the slave DataSource, used for calibration
        /// </summary>
        private BodyData m_SlaveBody;

        /// <summary>
        ///     Object that computes the RT matrix between the two found skeletons
        /// </summary>
        private readonly BodySkeletonAligner m_Aligner;

        /// <summary>
        ///     Time accumulator member.
        ///     In initialization stage, it represents the time since the beginning of the initialization stage that
        ///     the user has stand still in front of the DataSources.
        ///     In tracking stage, it represents the time since the last moment when the system took joints positions
        ///     to perform the calibration
        /// </summary>
        private double m_CurrentOperatingTime;

        /// <summary>
        ///     State of current calibrator
        /// </summary>
        private CalibrationSteps m_Status;

        /// <summary>
        /// If true, calibration will be performed using tracked joints centroid and not all the joints positions. This will make the calibration process more noisy, slow and imprecise,
        /// but can help in calibrating slave tracking boxes that are in positions where the standard calibration algorithm will prove unreliable (e.g. front-facing kinects)
        /// </summary>
        private bool m_useCentroids;

        /// <summary>
        ///     Number of last valid calibration matrices to discard
        /// </summary>
        private readonly int m_LastButNthValidMatrix;

        /// <summary>
        ///     Calibration matrices queues.
        ///     It serves to take the last but n valid calibration matrix for each slave DataSource.
        ///     We don't want the last matrix, because it could be a dirty result, including the movement of the player to
        ///     deactivate the calibration program, so we take the last but n matrix.
        /// </summary>
        private MatricesQueue m_CalibrationMatrixQueue;

        private readonly IBodyDataProvider m_BodyDataProvider;

        #region Public properties

        /// <summary>
        ///     Gets the state of the DataSources calibrator
        /// </summary>
        /// <value>The status of the DataSources calibrator</value>
        public CalibrationSteps Status
        {
            get { return m_Status; }
        }

        /// <summary>
        ///     Gets the last calibration matrix.
        /// </summary>
        /// <value>The last calibration matrix</value>
        public Matrix4x4 LastCalibrationMatrix
        {
            get { return m_Aligner.LastAlignmentMatrix; }
        }

        /// <summary>
        ///     Gets the last skeletal data read from the master DataSource.
        /// </summary>
        /// <value>The last master body</value>
        public BodyData LastMasterBody
        {
            get { return m_MasterBody; }
        }

        /// <summary>
        ///     Gets the last skeletal data read from the slave DataSource.
        /// </summary>
        /// <value>The last slave body</value>
        public BodyData LastSlaveSkeleton
        {
            get { return m_SlaveBody; }
        }

        /// <summary>
        ///     Returns last clean calibration matrix, i.e. the last but n calibration matrix.
        ///     It serves to take the last but n valid calibration matrix for each slave DataSource.
        ///     We don't want the last matrix, because it could be a dirty result, including the movement of the player to
        ///     deactivate the calibration program, so we take the last but n matrix.
        /// </summary>
        public Matrix4x4 LastCleanCalibrationMatrix
        {
            get { return m_CalibrationMatrixQueue.GetOldestValue(); }
        }

        #endregion

        /// <summary>
        ///     Initializes a new instance of the <see cref="MasterSlaveDataSourcesCalibrator" /> class.
        /// </summary>
        /// <param name="masterDataSourceId">The master DataSource skeletal tracker</param>
        /// <param name="slaveDataSourceId">The slave DataSource skeletal tracker</param>
        /// <param name="keyJoints">Joints on which the alignment between bodies has to be performed</param>
        /// <param name="lastButNthValidMatrix">Number of last valid calibration matrices to discard</param>
        public MasterSlaveDataSourcesCalibrator(string masterDataSourceId, string slaveDataSourceId, IBodyDataProvider bodyDataProvider, BodyJointTypes[] keyJoints, int lastButNthValidMatrix, bool useCentroids)
        {
            m_MasterDataSourceId = masterDataSourceId;
            m_SlaveDataSourceId = slaveDataSourceId;
            m_LastButNthValidMatrix = lastButNthValidMatrix;
            m_Aligner = new BodySkeletonAligner(keyJoints);
            m_BodyDataProvider = bodyDataProvider;
            m_useCentroids = useCentroids;
            Reset();
        }

        /// <summary>
        ///     Reset this calibrator, as it were created from scratch.
        ///     DataSources sources remain unchanged
        /// </summary>
        public void Reset()
        {
            m_MasterBody = null;
            m_SlaveBody = null;
            m_Aligner.Reset();
            m_Status = CalibrationSteps.WaitingForBody;
            m_CurrentOperatingTime = 0;
            m_CalibrationMatrixQueue = new MatricesQueue(m_LastButNthValidMatrix + 1);
        }

        /// <summary>
        ///     Update the calibrator with last skeletal data.
        ///     If it is un-initialized, it tries to initialize itself using skeletal data from both DataSource.
        ///     If it is initialized, it gets joints calibration data and performs calibration matrix computation every X seconds.
        /// </summary>
        /// <param name="timeDelta">Time delta from last call of Update</param>
        public void Update(double timeDelta)
        {
            switch (m_Status)
            {
                case CalibrationSteps.WaitingForBody:
                    WaitingUpdate(timeDelta);
                    break;
                case CalibrationSteps.InitializingWithBody:
                    InitializingUpdate(timeDelta);
                    break;
                case CalibrationSteps.Tracking:
                    TrackingUpdate(timeDelta);
                    break;
            }
        }

        /// <summary>
        ///     Performs the update of current object, during waiting stage, where a common body between slave and master
        ///     DataSource has to be found.
        /// </summary>
        /// <param name="timeDelta">Time delta from last call of WaitingUpdate</param>
        public void WaitingUpdate(double timeDelta)
        {
            IList<BodyData> foundMasterBodies = m_BodyDataProvider.DataSources[m_MasterDataSourceId].Bodies;
            IList<BodyData> foundSlaveSkeletons = m_BodyDataProvider.DataSources[m_SlaveDataSourceId].Bodies;

            //OLD ALGO: ONE BODY ALLOWED
            ////we must find exactly one only body in both DataSources, that is in tracking stage
            //BodyData foundMasterBody = null;
            //BodyData foundSlaveSkeleton = null;
            //int mastersFound = 0, slavesFound = 0;

            //if (foundMasterBodies == null || foundSlaveSkeletons == null)
            //    return;

            //foreach (BodyData body in foundMasterBodies)
            //{
            //    //if (body.IsTracked) 
            //    // GIANNI: this check is not needed anymore because only tracked bodies are retrieved from DataSources
            //    //{
            //    foundMasterBody = body;
            //    mastersFound++;
            //    //}
            //}

            //foreach (BodyData body in foundSlaveSkeletons)
            //{
            //    //if (BodyData.TrackingState == SkeletonTrackingState.Tracked)
            //    // GIANNI: this check is not needed anymore because only tracked bodies are retrieved from DataSources
            //    //{
            //    foundSlaveSkeleton = body;
            //    slavesFound++;
            //    //}
            //}

            //if (mastersFound == 1 && slavesFound == 1)
            //{
            //    m_MasterBody = foundMasterBody;
            //    m_SlaveBody = foundSlaveSkeleton;
            //    m_Status = CalibrationSteps.InitializingWithBody;
            //    m_CurrentOperatingTime = 0;
            //}

            //NEW ALGO: FIND NEAREST BODY
            //we must find nearest body in both DataSources, that is in tracking stage
            BodyData foundMasterBody = null;
            BodyData foundSlaveBody = null;
            float minMasterDistance = float.MaxValue, minSlaveDistance = float.MaxValue;

            if (foundMasterBodies == null || foundSlaveSkeletons == null)
                return;

            foreach (BodyData body in foundMasterBodies)
            {
                if (new Vector2(body.Joints[BodyJointTypes.SpineMid].Position.X, body.Joints[BodyJointTypes.SpineMid].Position.Z).Magnitude < minMasterDistance)
                {
                    foundMasterBody = body;
                    minMasterDistance = new Vector2(body.Joints[BodyJointTypes.SpineMid].Position.X, body.Joints[BodyJointTypes.SpineMid].Position.Z).Magnitude;
                }
            }

            foreach (BodyData body in foundSlaveSkeletons)
            {
                if (new Vector2(body.Joints[BodyJointTypes.SpineMid].Position.X, body.Joints[BodyJointTypes.SpineMid].Position.Z).Magnitude < minSlaveDistance)
                {
                    foundSlaveBody = body;
                    minSlaveDistance = new Vector2(body.Joints[BodyJointTypes.SpineMid].Position.X, body.Joints[BodyJointTypes.SpineMid].Position.Z).Magnitude;
                }
            }

            if (foundMasterBody != null && foundSlaveBody != null)
            {
                m_MasterBody = foundMasterBody;
                m_SlaveBody = foundSlaveBody;
                m_Status = CalibrationSteps.InitializingWithBody;
                m_CurrentOperatingTime = 0;
            }
        }

        /// <summary>
        ///     Performs the update of current object, during initializing stage, where the common body found in waiting stage
        ///     must stand still for a certain amount of time, so that calibration can begin.
        ///     The stillness is avaluated around the key joints
        /// </summary>
        /// <param name="timeDelta">Time delta from last call of WaitingUpdate</param>
        public void InitializingUpdate(double timeDelta)
        {
            //get new position of skeletons
            BodyData masterBody = GetBodyWithId(m_BodyDataProvider.DataSources[m_MasterDataSourceId].Bodies, m_MasterBody.Id);
            BodyData slaveSkeleton = GetBodyWithId(m_BodyDataProvider.DataSources[m_SlaveDataSourceId].Bodies, m_SlaveBody.Id);

            //if the skeletons can't be found, go back to waiting stage
            if (masterBody == null || slaveSkeleton == null)
            {
                m_Status = CalibrationSteps.WaitingForBody;
                return;
            }

            //if the BodyData have been found,
            //cycle through the key joints and check if the user stands still wrt the last frame
            bool movementFound = false;
            foreach (BodyJointTypes jt in m_Aligner.KeyJoints)
            {
                if (!(Vector3.Distance(FancyUtilities.GetVector3FromJoint(masterBody.Joints[jt], 1.0f), FancyUtilities.GetVector3FromJoint(m_MasterBody.Joints[jt], 1.0f)) < SquaredStandingMovementTolerance
                      && Vector3.Distance(FancyUtilities.GetVector3FromJoint(slaveSkeleton.Joints[jt], 1.0f), FancyUtilities.GetVector3FromJoint(m_SlaveBody.Joints[jt], 1.0f)) < SquaredStandingMovementTolerance))
                    movementFound = true;
            }

            //if movement has been found, reset user current standing time
            if (movementFound)
                m_CurrentOperatingTime = 0;
                //if no movement has been found, increment user current standing time and check if he stood still for enough
                //time to go to the calibration stage
            else
            {
                m_CurrentOperatingTime += timeDelta;

                if (m_CurrentOperatingTime >= InitializingStandingTime)
                {
                    m_Status = CalibrationSteps.Tracking;
                    m_CurrentOperatingTime = 0;
                }
            }

            //update last bodies
            m_MasterBody = masterBody;
            m_SlaveBody = slaveSkeleton;
        }

        // <summary>
        /// Performs the calibration of the two skeletons
        /// </summary>
        /// <param name="timeDelta">Time delta from last call of WaitingUpdate</param>
        public void TrackingUpdate(double timeDelta)
        {
            //get new position of skeletons
            BodyData masterBody = GetBodyWithId(m_BodyDataProvider.DataSources[m_MasterDataSourceId].Bodies, m_MasterBody.Id);
            BodyData slaveSkeleton = GetBodyWithId(m_BodyDataProvider.DataSources[m_SlaveDataSourceId].Bodies, m_SlaveBody.Id);
            
            //if the skeletons can't be found, go back to waiting stage
            if (masterBody == null || slaveSkeleton == null)
            {
                m_Status = CalibrationSteps.WaitingForBody;
                return;
            }

            //check if it is time to get new position of joints for calibration. 
            //If it so, perform the aligment between the two skeletons
            m_CurrentOperatingTime += timeDelta;

            if (m_CurrentOperatingTime >= JointsSamplingTime)
            {
                m_CurrentOperatingTime = 0;

                m_Aligner.AddKeyJoints(masterBody, slaveSkeleton, m_useCentroids);
                m_Aligner.ComputeAlignmentMatrix();

                m_CalibrationMatrixQueue.PushNewValue(m_Aligner.LastAlignmentMatrix);
            }

            //update last bodies
            m_MasterBody = masterBody;
            m_SlaveBody = slaveSkeleton;
        }

        /// <summary>
        ///     Gets the body with the provided tracking id, inside the bodies array.
        /// </summary>
        /// <returns>The desired body, or null if the body has not been found</returns>
        /// <param name="bodies">Tracked bodies array</param>
        /// <param name="id">Identifier of body of interest</param>
        private static BodyData GetBodyWithId(IList<BodyData> bodies, ulong id)
        {
            if (bodies == null)
                return null;

            foreach (BodyData body in bodies)
                if (body.Id == id)
                    return body;

            return null;
        }
    }
}
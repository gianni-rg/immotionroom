namespace ImmotionAR.ImmotionRoom.TrackingEngine.Calibration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Logger;
    using ImmotionAR.ImmotionRoom.TrackingEngine.Interfaces;
    using ImmotionAR.ImmotionRoom.TrackingEngine.Model;
    using ImmotionAR.ImmotionRoom.TrackingEngine.Tools;

    /// <summary>
    /// Master DataSource calibrator.
    /// Calibrates the master DataSource wrt a world reference frame, whose:
    /// - Origin stands on the floor, in a point chosen by the user (the point where the user stands still during calibration)
    /// - Orientation is coherent with the floor (XZ plane lies on the floor), with Y-axis 180 angle that matches the orientation
    ///   chosen by the user (the orienation at which the user stands still during calibration). That is, the frame of reference is right-handed,
    ///   with Y axis pointing up and Z axis pointing to the direction opposite to the one the user is facing during the calibration stage;
    /// To calibrate the master DataSource, user must stand still for 3 seconds. After that time, the DataSource is calibrated and the system does nothing
    /// </summary>
    public class MasterDataSourceCalibrator
    {
        protected ILogger m_Logger;

        /// <summary>
        /// Seconds that the calibrating user has to stand still so that the calibration can begin
        /// </summary>
        private const float InitializingStandingTime = 3.0f;

        /// <summary>
        /// Tolerance, in meters, of user movements to be still considered "still" (no pun intended)
        /// during initialization stage
        /// </summary>
        private const float StandingMovementTolerance = 0.15f;

        /// <summary>
        /// Squared value of StandingMovementTolerance
        /// </summary>
        private const float SquaredStandingMovementTolerance = StandingMovementTolerance * StandingMovementTolerance;

        /// <summary>
        /// Distance, in meters, from the DataSource head joint to actual top of the head
        /// </summary>
        private const float HeadJointToHeadTopDistance = 0.1f;

        /// <summary>
        /// Distance, in meters, from the DataSource foot joint to DataSource ankle joint
        /// </summary>
        private const float FootToAnkleJointDistance = 0.1f;

        /// <summary>
        /// Key joints used to calibrate the master DataSource
        /// </summary>
        private static readonly BodyJointTypes[] KeyJoints = new BodyJointTypes[] 
												   {
														BodyJointTypes.ShoulderLeft,
														BodyJointTypes.ShoulderRight,
														BodyJointTypes.SpineMid,
														BodyJointTypes.SpineShoulder
												   };

        /// <summary>
        /// The master DataSource skeletal tracker
        /// </summary>
        private readonly string m_MasterDataSourceId;

        /// <summary>
        /// Body, on the master DataSource, used for calibration
        /// </summary>
        private BodyData m_MasterBody;

        /// <summary>
        /// Time accumulator member.
        /// In initialization stage, it represents the time since the beginning of the initialization stage that 
        /// the user has stand still in front of the DataSources.
        /// </summary>
        private double m_CurrentOperatingTime;

        /// <summary>
        /// Status of current calibrator
        /// </summary>
        private CalibrationSteps m_Status;

        /// <summary>
        /// Found calibration matrix for master DataSource
        /// </summary>
        private Matrix4x4 m_CalibrationMatrix;

        /// <summary>
        /// Additional rotation around Y axis, in radians, to be added to found calibration matrix.
        /// This is used to make possible to put orientation 0 to a position that is far from frontal from the master DataSource.
        /// </summary>
        private readonly float m_AdditionalRotAngle;

        /// <summary>
        /// Expected height for the user performing the calibration sequence.
        /// This is useful to correct some errors in DataSource measuring scale.
        /// If it is == 0, this correction is not applied
        /// </summary>
        private readonly float m_ExpectedHeight;

        private readonly IBodyDataProvider m_BodyDataProvider;

        #region Public properties

        /// <summary>
        /// Gets the status of the DataSource calibrator
        /// </summary>
        /// <value>The status of the DataSource calibrator</value>
        public CalibrationSteps Status
        {
            get
            {
                return this.m_Status;
            }
        }

        /// <summary>
        /// Gets the found calibration matrix.
        /// </summary>
        /// <value>The calibration matrix</value>
        public Matrix4x4 CalibrationMatrix
        {
            get
            {
                return m_CalibrationMatrix;
            }
        }

        /// <summary>
        /// Gets the last skeletal data read from the master DataSource.
        /// </summary>
        /// <value>The last master body</value>
        public BodyData LastMasterBody
        {
            get
            {
                return m_MasterBody;
            }
        }

        public ILogger Logger
        {
            set { m_Logger = value; }
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MasterSlaveDataSourcesCalibrator"/> class.
        /// </summary>
        /// <param name="masterDataSourceId">The master DataSource skeletal tracker</param>
        /// <param name="additionalYRotation">Additional rotation around Y axis, in radians, to be added to found calibration matrix</param>
        /// <param name="expectedUserHeight">Expected height for the user performing the calibration sequence</param> 
        public MasterDataSourceCalibrator(string masterDataSourceId, IBodyDataProvider bodyDataProvider, float additionalYRotation, float expectedUserHeight)
        {
            m_MasterDataSourceId = masterDataSourceId;
            m_AdditionalRotAngle = additionalYRotation;
            m_ExpectedHeight = expectedUserHeight;
            m_BodyDataProvider = bodyDataProvider;
            this.Reset();
        }

        /// <summary>
        /// Reset this calibrator instance
        /// </summary>
        public void Reset()
        {
            m_MasterBody = null;
            m_Status = CalibrationSteps.WaitingForBody;
            m_CurrentOperatingTime = 0;
            m_CalibrationMatrix = Matrix4x4.Identity;
        }

        /// <summary>
        /// Update the calibrator with last skeletal data.
        /// If it is un-initialized, it tries to initialize itself using skeletal data from both DataSource.
        /// If it is initialized, it performs calibration matrix computation and then do nothing
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
                //			case CalibrationSteps.Done:
                //				break;
            }
        }

        /// <summary>
        /// Performs the update of current object, during waiting stage, where a body between on the master
        /// DataSource has to be found.
        /// </summary>
        /// <param name="timeDelta">Time delta from last call of WaitingUpdate</param>
        public void WaitingUpdate(double timeDelta)
        {
            // GIANNI TODO: improve this, just temporary in the porting
            BodyData[] foundMasterBodies = m_BodyDataProvider.DataSources[m_MasterDataSourceId].Bodies.ToArray();

            //OLD ALGO: WE HAD TO FIND ONE ONLY BODY
            ////we must find exactly one only body, that is in tracking stage
            //BodyData foundMasterBody = null;
            //int mastersFound = 0;

            //if (foundMasterBodies == null)
            //    return;

            //foreach (BodyData body in foundMasterBodies)
            //{
            //    //if (body.IsTracked)
            //    // GIANNI: this check is not needed anymore because only tracked bodies are retrieved from DataSources
            //    //{
            //        foundMasterBody = body;
            //        mastersFound++;
            //    //}
            //}

            //if (mastersFound == 1)
            //{
            //    m_MasterBody = foundMasterBody;
            //    m_Status = CalibrationSteps.InitializingWithBody;
            //    m_CurrentOperatingTime = 0;
            //}

            //NEW ALGO:TAKE NEAREST BODY
            //we must find exactly one only body, that is in tracking stage
            BodyData foundMasterBody = null;
            float minDistance = float.MaxValue;

            if (foundMasterBodies == null)
                return;

            foreach (BodyData body in foundMasterBodies)
            {
                if (new Vector2(body.Joints[BodyJointTypes.SpineMid].Position.X, body.Joints[BodyJointTypes.SpineMid].Position.Z).Magnitude < minDistance)
                {
                    foundMasterBody = body;
                    minDistance = new Vector2(body.Joints[BodyJointTypes.SpineMid].Position.X, body.Joints[BodyJointTypes.SpineMid].Position.Z).Magnitude;
                }
            }

            if (foundMasterBody != null)
            {
                m_MasterBody = foundMasterBody;
                m_Status = CalibrationSteps.InitializingWithBody;
                m_CurrentOperatingTime = 0;
            }

        }

        /// <summary>
        /// Performs the update of current object, during initializing stage, where the body found in waiting stage
        /// must stand still for a certain amount of time, so that calibration can begin.
        /// The stillness is avaluated around the key joints
        /// </summary>
        /// <param name="timeDelta">Time delta from last call of WaitingUpdate</param>
        public void InitializingUpdate(double timeDelta)
        {
            //get new position of skeletons
            BodyData masterBody = GetBodyWithId(m_BodyDataProvider.DataSources[m_MasterDataSourceId].Bodies, m_MasterBody.Id);

            //if the skeletons can't be found, go back to waiting stage
            if (masterBody == null)
            {
                m_Status = CalibrationSteps.WaitingForBody;
                return;
            }

            //if the skeleton have been found,
            //cycle through the key joints and check if the user stands still wrt the last frame
            bool movementFound = false;
            foreach (BodyJointTypes jt in KeyJoints)
            {
                if (!(Vector3.Distance(FancyUtilities.GetVector3FromJoint(masterBody.Joints[jt], 1.0f), FancyUtilities.GetVector3FromJoint(m_MasterBody.Joints[jt], 1.0f)) < SquaredStandingMovementTolerance))
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
                    CalibrateMasterDataSource();
                    m_Status = CalibrationSteps.Done;
                }
            }

            //update last bodies
            m_MasterBody = masterBody;
        }

        /// <summary>
        /// Gets the body with the provided tracking id, inside the bodies array.
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

        /// <summary>
        /// Calibrates the master DataSource, using still body data read from its skeletal tracker
        /// </summary>
        private void CalibrateMasterDataSource()
        {
            //ok, let's begin calibration of master DataSource

            //take position of keypoints
            Vector3 spineMid = FancyUtilities.GetVector3FromJoint(m_MasterBody.Joints[BodyJointTypes.SpineMid], 1.0f),
                    spineShoulder = FancyUtilities.GetVector3FromJoint(m_MasterBody.Joints[BodyJointTypes.SpineShoulder], 1.0f),
                    shoulderLeft = FancyUtilities.GetVector3FromJoint(m_MasterBody.Joints[BodyJointTypes.ShoulderLeft], 1.0f),
                    shoulderRight = FancyUtilities.GetVector3FromJoint(m_MasterBody.Joints[BodyJointTypes.ShoulderRight], 1.0f);

            //let's translate keypoints lying in spine, so that spine shoulder stays at shoulder points midpoint:
            //doing so, the 4 keypoints will form a unique plane 
            Vector3 shoulderMidPoint = (shoulderLeft + shoulderRight) / 2;
            Vector3 spineMidAdj = spineMid + shoulderMidPoint - spineShoulder/*,
				spineShoulderAdj = shoulderMidPoint*/;

            //now we have 3 keypoints forming a plane: shoulderLeft, shoulderRight, spineMidAdj;
            //spineShoulderAdj lies in this plane (so all four points are coplanar), but it's useless for the next computations.
            //Notice that we are looking for a plane, because using this plane orientation we'll calibrate the master DataSource

            //calculate distance between shoulders and between spine points
            float shoulderLength = Vector3.Distance(shoulderLeft, shoulderRight),
                  spineLength = Vector3.Distance(spineMid, spineShoulder);

            //using all previous data, calculate the keypoints of the ideal skeleton.
            //The ideal skeleton is the one that we would have if the master DataSource would be perfectly aligned to the floor
            //and perfectly oriented towards the user. 
            //Notice that we are ignoring translation for now, so Y can be 0
            Vector3 shoulderLeftIdeal = new Vector3(-shoulderLength * 0.5f, 0, 0),
                    shoulderRightIdeal = new Vector3(+shoulderLength * 0.5f, 0, 0),
                    spineMidIdeal = new Vector3(0, -spineLength, 0);

            //calculate rotation between observed points and ideal points to get calibration rotation
            Matrix4x4 rotTransf = PointsTransformCalculator.FindRTmatrix(
                new List<Vector3>() { shoulderLeft, shoulderRight, spineMidAdj },
                new List<Vector3>() { shoulderLeftIdeal, shoulderRightIdeal, spineMidIdeal });

            //remove translational component, that is wrong at this point
            rotTransf.M03 = rotTransf.M13 = rotTransf.M23 = 0;

            //add additional rotation and store results in calibration matrix
            m_CalibrationMatrix = rotTransf * Matrix4x4.TRS(Vector3.Zero, Quaternion.AngleAxis(m_AdditionalRotAngle, Vector3.Up), Vector3.One);

            //now that we have the rotation, we must apply the optional scale factor,
            //to correct scale of DataSource measurement.

            //make a confrontation between expected user height (Y difference between head and feet) and found user height.
            //Notice that we actually use ankles and not feet, because ankle joints are stabler
            Vector3 headPos = FancyUtilities.GetVector3FromJoint(m_MasterBody.Joints[BodyJointTypes.Head], 1.0f);
            Vector3 leftAnklePos = FancyUtilities.GetVector3FromJoint(m_MasterBody.Joints[BodyJointTypes.AnkleLeft], 1.0f);
            Vector3 rightAnklePos = FancyUtilities.GetVector3FromJoint(m_MasterBody.Joints[BodyJointTypes.AnkleRight], 1.0f);

            float scaleUnit = m_ExpectedHeight == 0 ? 1.0f :
                    m_ExpectedHeight / (m_CalibrationMatrix.MultiplyPoint(headPos).Y -
                                        0.5f * (m_CalibrationMatrix.MultiplyPoint(leftAnklePos).Y + m_CalibrationMatrix.MultiplyPoint(rightAnklePos).Y) +
                                        HeadJointToHeadTopDistance + FootToAnkleJointDistance);

            m_CalibrationMatrix = m_CalibrationMatrix * Matrix4x4.TS(Vector3.Zero, new Vector3(scaleUnit, scaleUnit, scaleUnit));

            //now that we have everything rotated and scaled, we have to find the translation

            //to find translation, we need only one point to match between real and ideal.
            //we will use the spineMid. We have already this point, but we must calculate its Y position, to perform this calculation
            //its Y position is the distance, onto the Y axis, from the feet to the spineMid

            //calulate rotated versions of the SpineMid and the feet 
            Vector3 spineMidRotated = m_CalibrationMatrix.MultiplyPoint3x4(spineMid),
                    leftFootRotated = m_CalibrationMatrix.MultiplyPoint3x4(leftAnklePos),
                    rightFootRotated = m_CalibrationMatrix.MultiplyPoint3x4(rightAnklePos);

            //create ideal position of rotated spineMid, with feet at the origin (we average Y position of feet to reduce noise)
            Vector3 spineMidRotatedIdeal = new Vector3(0, spineMidRotated.Y - (leftFootRotated.Y + rightFootRotated.Y) / 2, 0);

            //calulate translation as a difference between ideal and real
            Vector3 translation = spineMidRotatedIdeal - spineMidRotated;

            //adjust all things so that feet stay at Y = 0
            //(this is necessary because using spineMid leads some error in this direction)
            translation.Y = -(leftFootRotated.Y + rightFootRotated.Y) / 2 + FootToAnkleJointDistance;

            //calculate translation matrix
            Matrix4x4 translTransf = translation.ToTranslationMatrix();

            //make a composition between found transformation and translation and we find the final matrix
            m_CalibrationMatrix = translTransf * m_CalibrationMatrix;
        }
    }

    //ALGORITMO:
    //- prendi i 2 punti spalle
    //- prendi punti spinemid e spineshoulder
    //- trasla i punti spine in modo che spine shoulder finisca al baricentro del segmento degli altri due
    //- a questo punto usa i 3 punti: spalla sinistra, spalla destra e spine mid modificato.
    //
    //Per i punti modello:
    //- calcola distanza spalla sinistra e spalla destra
    //- i punti spalla si muovono di conseguenza: entrambi avranno Z = 0, X = +-dist(spalle / 2), Y = 0
    //- spine mid si trova in (0, - dist(spalle, spine mid modificato), 0)
    //
    //-trova R tra questi punti
    //
    //- per T, trova distanza tra spine_mid ruotato e spine mid desiderato, che è (0, (spine_mid ruotato).Y - piedi.Y , 0)
}
namespace ImmotionAR.ImmotionRoom.TrackingEngine.Walking
{
    using System;
    using System.Collections.Generic;
    using Model;

    /// <summary>
    ///     Defines an object that can detects walking movement of the player
    ///     using naive heuristics on player knees movements
    /// </summary>
    internal class KnaivePlayerWalkingDetector : PlayerWalkingDetector
    {
        /// <summary>
        ///     Parameters to perform detection
        /// </summary>
        private KnaivePlayerWalkingDetectorParams m_detectionParameters;

        /// <summary>
        ///     Knee walking detectors: one for each knee (0 is for left knee)
        /// </summary>
        private readonly IKneeWalkingDetector[] m_kneeWalkingDetectors;

        /// <summary>
        ///     Index of last knee performing a valid step: 0 for left knee, 1 for right knee, -1 if no knee initialized
        /// </summary>
        private int m_lastDetectedStepIdx;

        /// <summary>
        ///     Time, since the beginning of the program, when we performed the last leg switch
        ///     (when we passed from following left leg to right leg or viceversa)
        /// </summary>
        private TimeSpan m_timeOfLastLegSwitch;

        /// <summary>
        ///     Body joints positions read at last timeframe
        /// </summary>
        private Dictionary<BodyJointTypes, Vector3> m_lastFrameBodyJoints;

        /// <summary>
        ///     Filtered direction of walking, computed from directions of knees
        /// </summary>
        private float m_filteredHorizontalAngle;

        /// <summary>
        ///     Filtered modulus of walking speed, computed as running average of knees speed
        /// </summary>
        private float m_filteredSpeedModulus;

        /// <summary>
        ///     Reference player position, to be matched against current position to obtain if the player is moving its body
        /// </summary>
        private Vector2 m_lastReferenceXZPlayerPosition;

        /// <summary>
        ///     Last time of the beginning of detection of the player moving or the player staying still
        /// </summary>
        private TimeSpan m_lastMovementNonMovementBeginTime;

        /// <summary>
        ///     Current status of player moving
        /// </summary>
        private PlayerMovingStatus m_movingStatus;

        #region Constructor

        /// <summary>
        ///     Creates a player walking movement detector
        /// </summary>
        /// <param name="detectionParameters">Parameters for player walking detection</param>
        public KnaivePlayerWalkingDetector(KnaivePlayerWalkingDetectorParams detectionParameters)
        {
            //initialize everything

            m_detectionParameters = detectionParameters;
            m_kneeWalkingDetectors = new IKneeWalkingDetector[]
            {
                new SpikesKneeWalkingDetector(true, m_detectionParameters.KneeDetectionParams),
                new SpikesKneeWalkingDetector(false, m_detectionParameters.KneeDetectionParams)
            };
            m_lastDetectedStepIdx = -1;

            m_timeOfLastLegSwitch = new TimeSpan(0);

            m_currentDetection = new PlayerWalkingDetection { BodyId = 0, Timestamp = new TimeSpan(0), EstimatedWalkSpeed = Vector3.Zero};

            m_lastFrameBodyJoints = new Dictionary<BodyJointTypes, Vector3>();

            m_filteredHorizontalAngle = 0;
            m_filteredSpeedModulus = 0;

            m_movingStatus = PlayerMovingStatus.NonMoving;
        }

        #endregion

        #region PlayerWalkingDetector Methods

        public override void LoadSettings(Dictionary<string, string> runtimeParameters)
        {
            
        }

        /// <summary>
        ///     Perform new detection of walking movement, because new joint data is arrived.
        ///     It is advised to call this function at a very regular interval
        /// </summary>
        /// <param name="timestamp">Time since a common reference event, like the start of the program</param>
        /// <param name="body">New body joint data</param>
        public override void UpdateDetection(TimeSpan timestamp, BodyData body)
        {
            //if we have an exactly IDENTIC body since the last frame, we have performed an update too fast and we are obtaining the same
            //kinect data of the last frame, so do nothing
            //(notice that due to noise and float approximation, we can't obtain identical data between two frames, even if the user is still)
            if (CompareBodiesAndUpdate(body, ref m_lastFrameBodyJoints))
            {
                return;
            }

            //compare current position of user with reference position... if the difference is above a threshold, then this as a movement.
            //If the player is moving since too much time, mark the flag as player moving and do not consider leg walking detection until the movement
            //is finished.
            //viceversa, if we detect non movement since too much time, remove the moving flag
            Vector2 newPlayerXZPosition = new Vector2(body.Joints[BodyJointTypes.SpineMid].Position.X, body.Joints[BodyJointTypes.SpineMid].Position.Z);

            //if comparison of current position results in a movement detection for this frame
            if ((newPlayerXZPosition - m_lastReferenceXZPlayerPosition).Magnitude > m_detectionParameters.PlayerMovementDetectionThresh)
            {
                if (m_movingStatus == PlayerMovingStatus.NonMoving)
                {
                    m_lastMovementNonMovementBeginTime = timestamp;
                    m_movingStatus = PlayerMovingStatus.NonMovingWithMovements;
                }
                else if (m_movingStatus == PlayerMovingStatus.NonMovingWithMovements)
                {
                    if ((timestamp - m_lastMovementNonMovementBeginTime).TotalMilliseconds*0.001f > m_detectionParameters.PlayerMovementDetectionTimeThreshold)
                        m_movingStatus = PlayerMovingStatus.Moving;
                }
                else if (m_movingStatus == PlayerMovingStatus.MovingWithNonMovements)
                {
                    m_movingStatus = PlayerMovingStatus.Moving;
                }
                //else do nothing
            }
            else //if the displacement is too low and we have detected player stillness
            {
                if (m_movingStatus == PlayerMovingStatus.Moving)
                {
                    m_lastMovementNonMovementBeginTime = timestamp;
                    m_movingStatus = PlayerMovingStatus.MovingWithNonMovements;
                }
                else if (m_movingStatus == PlayerMovingStatus.MovingWithNonMovements)
                {
                    if ((timestamp - m_lastMovementNonMovementBeginTime).TotalMilliseconds*0.001f > m_detectionParameters.PlayerMovementDetectionTimeThreshold)
                        m_movingStatus = PlayerMovingStatus.NonMoving;
                }
                else if (m_movingStatus == PlayerMovingStatus.NonMovingWithMovements)
                {
                    m_movingStatus = PlayerMovingStatus.NonMoving;
                }
                //else do nothing
            }

            //update reference position with a running average
            m_lastReferenceXZPlayerPosition = m_detectionParameters.PlayerMovementDetectionRunningAvgAlpha*newPlayerXZPosition + (1 - m_detectionParameters.PlayerMovementDetectionRunningAvgAlpha)*m_lastReferenceXZPlayerPosition;

            //for each leg
            for (var legIdx = 0; legIdx <= 1; legIdx++)
            {
                //update its detector
                m_kneeWalkingDetectors[legIdx].UpdateDetection(timestamp, body);
            }

            //if the player is moving, do not follow any leg and do not detect any walking action
            if (m_movingStatus == PlayerMovingStatus.Moving || m_movingStatus == PlayerMovingStatus.MovingWithNonMovements)
                m_lastDetectedStepIdx = -1;
            else
            {
                //if we are looking for any leg
                if (m_lastDetectedStepIdx == -1)
                {
                    //set following leg to the first one that is walking
                    if (m_kneeWalkingDetectors[0].CurrentDetection.IsWalking)
                        m_lastDetectedStepIdx = 0;
                    else if (m_kneeWalkingDetectors[1].CurrentDetection.IsWalking)
                        m_lastDetectedStepIdx = 1;

                    //if we've found a walking leg, get its angle as first moving angle (only if estimation of walking angle is from knee)
                    if (m_detectionParameters.WalkingAngleEstimationType == WalkingDirectionEstimator.Knee && m_lastDetectedStepIdx != -1)
                        m_filteredHorizontalAngle = m_kneeWalkingDetectors[m_lastDetectedStepIdx].CurrentDetection.HorizontalKneeAngle;
                }
                //else, if we are following a leg
                else
                {
                    //if the leg we are following was moving at last frame, it means we are following a continuative movement
                    if (m_kneeWalkingDetectors[m_lastDetectedStepIdx].PreviousDetection.IsWalking)
                    {
                        //m_lastDetectedStepIdx = m_lastDetectedStepIdx;   

                        //if this leg is not walking anymore, switch leg
                        if (!m_kneeWalkingDetectors[m_lastDetectedStepIdx].CurrentDetection.IsWalking)
                        {
                            m_lastDetectedStepIdx = m_lastDetectedStepIdx == 0 ? 1 : 0;
                            m_timeOfLastLegSwitch = timestamp;
                        }
                        //else, in case of walking angle estimated using knee info, filter horizontal angle if this is a continuous tracking and assign if it is a first time tracking
                        else if (m_detectionParameters.WalkingAngleEstimationType == WalkingDirectionEstimator.Knee)
                            if (!m_kneeWalkingDetectors[m_lastDetectedStepIdx].PreviousDetection.IsWalking)
                                m_filteredHorizontalAngle = m_kneeWalkingDetectors[m_lastDetectedStepIdx].CurrentDetection.HorizontalKneeAngle;
                            else
                            //remember to consider valid angles only during the rising stage (a falling knee performs unreliable fast movements)
                                m_filteredHorizontalAngle = m_kneeWalkingDetectors[m_lastDetectedStepIdx].CurrentDetection.KneeEstimatedStatus != KneeWalkingStatus.Falling ?
                                    m_filteredHorizontalAngle*(1 - m_detectionParameters.WalkingAngleRunningAvgAlpha) + m_detectionParameters.WalkingAngleRunningAvgAlpha*m_kneeWalkingDetectors[m_lastDetectedStepIdx].CurrentDetection.HorizontalKneeAngle
                                    : m_filteredHorizontalAngle;
                    }
                    //else, it means we're trying to follow a leg that has not moved yet
                    else
                    {
                        //if we are waiting for it since too much, reset the system
                        if ((timestamp - m_timeOfLastLegSwitch).TotalMilliseconds*0.001f > m_detectionParameters.StillResetTime)
                            m_lastDetectedStepIdx = -1;
                    }
                }
            }

            //if walking angle detection must be made using pelvis angle, take direction perpendicular to the line connecting the two hips (and compute running average)
            if (m_detectionParameters.WalkingAngleEstimationType == WalkingDirectionEstimator.Pelvis)
                m_filteredHorizontalAngle = m_filteredHorizontalAngle*(1 - m_detectionParameters.WalkingAngleRunningAvgAlpha) + m_detectionParameters.WalkingAngleRunningAvgAlpha*((float)-Math.PI/2 + MathUtilities.BetweenJointsXZOrientation(body.Joints[BodyJointTypes.HipLeft].ToVector3(), body.Joints[BodyJointTypes.HipRight].ToVector3()));

            //if walking angle detection must be made using shoulders angle, take direction perpendicular to the line connecting the two shoulders (and compute running average)
            else if (m_detectionParameters.WalkingAngleEstimationType == WalkingDirectionEstimator.Shoulders)
                m_filteredHorizontalAngle = m_filteredHorizontalAngle*(1 - m_detectionParameters.WalkingAngleRunningAvgAlpha) + m_detectionParameters.WalkingAngleRunningAvgAlpha*((float)-Math.PI/2 + MathUtilities.BetweenJointsXZOrientation(body.Joints[BodyJointTypes.ShoulderLeft].ToVector3(), body.Joints[BodyJointTypes.ShoulderRight].ToVector3()));

            //compute walking direction, depending on if we are following a leg or not
            //remember to filter speed modulus as running average of computed speeds
            if (m_lastDetectedStepIdx == -1)
            {
                //we are still
                m_filteredSpeedModulus = m_filteredSpeedModulus*(1 - m_detectionParameters.WalkingMagnitudeRunningAvgAlpha);
                m_currentDetection = new PlayerWalkingDetection {BodyId = body.Id, Timestamp = timestamp, IsWalking = false, IsMoving = m_movingStatus == PlayerMovingStatus.Moving || m_movingStatus == PlayerMovingStatus.MovingWithNonMovements, EstimatedWalkSpeed = Vector3.Zero};
            }
            else
            {
                //compute walking direction
                //m_currentDetection = new PlayerWalkingDetection() { Timestamp = timestamp, IsWalking = true, EstimatedWalkSpeed = m_kneeWalkingDetectors[m_lastDetectedStepIdx].CurrentDetection.EstimatedWalkSpeed * new Vector3(Mathf.Cos(m_kneeWalkingDetectors[m_lastDetectedStepIdx].CurrentDetection.HorizontalKneeAngle), 0, Mathf.Sin(m_kneeWalkingDetectors[m_lastDetectedStepIdx].CurrentDetection.HorizontalKneeAngle)) };
                m_filteredSpeedModulus = m_filteredSpeedModulus*(1 - m_detectionParameters.WalkingMagnitudeRunningAvgAlpha) + m_detectionParameters.WalkingMagnitudeRunningAvgAlpha*m_kneeWalkingDetectors[m_lastDetectedStepIdx].CurrentDetection.EstimatedWalkSpeed;

                //Debug.Log("m_filteredSpeedModulus" + m_filteredSpeedModulus);
                if (m_filteredSpeedModulus > 0.175f) //TODO: THIS IS A FAST HACK
                    m_filteredSpeedModulus = 0.175f;

                m_currentDetection = new PlayerWalkingDetection { BodyId = body.Id, Timestamp = timestamp, IsWalking = true, IsMoving = false, EstimatedWalkSpeed = m_filteredSpeedModulus*new Vector3((float)Math.Cos(m_filteredHorizontalAngle), 0, (float)Math.Sin(m_filteredHorizontalAngle))};
            }
        }

        /// <summary>
        ///     Serialize object info into a dictionary, for debugging purposes
        /// </summary>
        /// <returns>Object serialization into a dictionary of dictionaries (infos are subdivided into groups)</returns>
        public override Dictionary<string, Dictionary<string, string>> DictionarizeInfo()
        {
            return new Dictionary<string, Dictionary<string, string>>
            {
                {"Left Walking Detector", m_kneeWalkingDetectors[0].DictionarizeInfo()},
                {
                    "Master Player Walking Detector Data", new Dictionary<string, string>
                    {
                        {"Current detection", m_currentDetection.ToString()},
                        {"Following leg", m_lastDetectedStepIdx == -1 ? "Any" : (m_lastDetectedStepIdx == 0 ? "Left" : "Right")},
                        {"Time of last leg switch", m_timeOfLastLegSwitch.ToString()},
                        {"Filtered Walking angle", (MathConstants.Rad2Deg*m_filteredHorizontalAngle).ToString("+000;_000;+000")}
                    }
                },
                {"Right Walking Detector", m_kneeWalkingDetectors[1].DictionarizeInfo()}
            };
        }

        #endregion
    }
}

namespace ImmotionAR.ImmotionRoom.TrackingEngine.Walking
{
    using System;
    using System.Collections.Generic;
    using Model;

    //TODO: al momento non controlli che lui non faccia movimenti orizzontali strani con la gamba! (la fa oscillare a destra e sinistra)
    //TODO: al momento se si pausa durante la discesa, consideri il passo come finito! Non va proprio benissimo
    //TODO: non c'è nessun controllo con l'orientamento corrente del giocatore (che puoi vedere ad es. dalle spalle)

    /// <summary>
    ///     Walking detector that bases its detection on spikes burst on speed or acceleration of the player knee
    /// </summary>
    internal class SpikesKneeWalkingDetector : KneeWalkingDetector
    {
        /// <summary>
        ///     History of last data of the joint of interest
        /// </summary>
        private readonly DataHistoryList<BodyJointCineticInfo> m_jointHistory;

        /// <summary>
        ///     History of last detections of current object.
        ///     Notice that m_currentDetection is NOT included in this list
        /// </summary>
        private readonly DataHistoryList<KneeWalkingDetection> m_walkDetectionsHistory;

        #region Constructor

        /// <summary>
        ///     Creates a walking movement detector that uses bursts on player knee movment for its analysis
        /// </summary>
        /// <param name="isLeft">True if detection should regard left knee; false for right knee</param>
        /// <param name="detectionParameters">Parameters for walking detection</param>
        public SpikesKneeWalkingDetector(bool isLeft, KneeWalkingDetectorParams detectionParameters) :
            base(isLeft, detectionParameters)
        {
            m_jointHistory = new DataHistoryList<BodyJointCineticInfo>(1);
            //make room for enough history to be able to compute the time since which the leg is moving or stays still
            m_walkDetectionsHistory = new DataHistoryList<KneeWalkingDetection>((int)Math.Round(Math.Max(detectionParameters.TimeToTriggerMovement, detectionParameters.TimeToTriggerStillness)*detectionParameters.EstimatedFrameRate));
        }

        #endregion

        #region KneeWalkingDetector Methods

        /// <summary>
        ///     Get past frame analysis result of this object about user walking gesture
        /// </summary>
        public override KneeWalkingDetection PreviousDetection
        {
            get { return m_walkDetectionsHistory.Count > 0 ? m_walkDetectionsHistory[0] : new KneeWalkingDetection(); }
        }

        /// <summary>
        ///     Perform new detection of walking movement, because new joint data is arrived.
        ///     It is advised to call this function at a very regular interval
        /// </summary>
        /// <param name="timestamp">Time since a common reference event, like the start of the program</param>
        /// <param name="body">New body joint data</param>
        public override void UpdateDetection(TimeSpan timestamp, BodyData body)
        {
            //compute cinetic data about joint of history
            //If it is first read frame, make some measures and then wait for new data,
            //otherwise begin knee movement analysis
            BodyJointCineticInfo newInfo;

            if (m_jointHistory.Count == 0)
            {
                newInfo = new BodyJointCineticInfo(timestamp, body, m_kneeJointType, m_hipJointType);

                //compute a fake detection status (nothing has been actually detected)
                m_currentDetection = new KneeWalkingDetection
                {
                    IsWalking = false,
                    KneeRawStatus = KneeWalkingStatus.Still,
                    KneeEstimatedStatus = KneeWalkingStatus.Still,
                    Timestamp = timestamp,
                    EstimatedWalkSpeed = 0,
                    EstimatedWalkSpeedCarrier = 0,
                    HorizontalKneeAngle = 0,
                    LastDetectedWalkInstant = new TimeSpan(0),
                    LastDetectedWalkStart = new TimeSpan(0)
                };
            }
            else
            {
                //uncomment this to compute forward direction using head-neck joints
                //newInfo = new BodyJointCineticInfo(timestamp, body, m_jointHistory[0], MathUtilities.BetweenJointsXZOrientation(
                //            body.Joints[BodyJointTypes.Neck].ToVector3(), body.Joints[BodyJointTypes.Head].ToVector3()));

                //uncomment this to compute forward direction using hips joints
                newInfo = new BodyJointCineticInfo(timestamp, body, m_jointHistory[0], (float)-Math.PI/2 + MathUtilities.BetweenJointsXZOrientation(body.Joints[BodyJointTypes.HipLeft].ToVector3(), body.Joints[BodyJointTypes.HipRight].ToVector3()));

                //save last frame current detection as last value in history
                m_walkDetectionsHistory.PushNewValue(m_currentDetection);

                //make different analysis, depending on last detected state
                switch (m_currentDetection.KneeEstimatedStatus)
                {
                    //the knee was still
                    case KneeWalkingStatus.Still:
                    case KneeWalkingStatus.Unknown:
                        UpdateStillKnee(newInfo);
                        break;

                    //the knee was rising
                    case KneeWalkingStatus.Rising:
                        UpdateRisingKnee(newInfo);
                        break;

                    //the knee has stopped during a rising movement
                    case KneeWalkingStatus.RisingStill:
                        UpdateRisingStillKnee(newInfo);
                        break;

                    //the knee was falling
                    case KneeWalkingStatus.Falling:
                        UpdateFallingKnee(newInfo);
                        break;
                }
            }

            //save current joint info in history
            m_jointHistory.PushNewValue(newInfo);
        }

        /// <summary>
        ///     Serialize object info into a dictionary, for debugging purposes
        /// </summary>
        /// <returns>Object serialization into a dictionary of dictionaries (infos are subdivided into groups)</returns>
        public override Dictionary<string, string> DictionarizeInfo()
        {
            return new Dictionary<string, string>
            {
                {"Knee Joint Cinetic Info", (m_jointHistory.Count > 0 ? m_jointHistory[0] : new BodyJointCineticInfo()).ToString()},
                {"Current Detection", m_currentDetection.ToString()}
            };
        }

        #endregion

        #region Knee walking detection methods

        /// <summary>
        ///     Updates detection, knowing that at last frame the knee was still
        /// </summary>
        /// <param name="newKneeInfo">Informations about the knee joint at current frame</param>
        private void UpdateStillKnee(BodyJointCineticInfo newKneeInfo)
        {
            //if we are still, we are waiting for a Rising movement trigger, in speed or acceleration

            //compute physical quantity of interest, in xz plane
            Vector2 xzQuantity = m_walkingDetectionParams.UseAcceleration ? new Vector2(newKneeInfo.InstantAcceleration.X, newKneeInfo.InstantAcceleration.Z) : new Vector2(newKneeInfo.InstantSpeed.X, newKneeInfo.InstantSpeed.Z);
            float yQuantity = m_walkingDetectionParams.UseAcceleration ? newKneeInfo.InstantAcceleration.Y : newKneeInfo.InstantSpeed.Y;

            //if we detected a Rising trigger, and the leg has moved by a reasonable angle from its vertical position (to avoid 
            //detecting noise movements)
            if ((xzQuantity.Magnitude > m_walkingDetectionParams.StillToRisingThreshold || Math.Abs(yQuantity) > m_walkingDetectionParams.StillToRisingThreshold) &&
                xzQuantity.Magnitude < m_walkingDetectionParams.SpikeNoiseThreshold &&
                Math.PI/2 + newKneeInfo.Angle.Y > m_walkingDetectionParams.StillAngleThreshold &&
                yQuantity > 0)
            {
                //initialize current detection results
                m_currentDetection = new KneeWalkingDetection
                {
                    IsWalking = false,
                    KneeRawStatus = KneeWalkingStatus.Rising,
                    KneeEstimatedStatus = KneeWalkingStatus.Still,
                    Timestamp = newKneeInfo.Time,
                    EstimatedWalkSpeed = 0,
                    EstimatedWalkSpeedCarrier = 0,
                    HorizontalKneeAngle = newKneeInfo.Angle.X,
                    LastDetectedWalkInstant = m_walkDetectionsHistory[0].LastDetectedWalkInstant,
                    LastDetectedWalkStart = m_walkDetectionsHistory[0].LastDetectedWalkStart
                };

                //if we are detecting a Rising movement since enough time, trigger leg walking result
                for (var i = 0; i < m_walkDetectionsHistory.Count; i++)
                {
                    if (m_walkDetectionsHistory[i].KneeRawStatus != KneeWalkingStatus.Rising ||
                        MathUtilities.AdjustedAnglesAbsDifference(m_walkDetectionsHistory[i].HorizontalKneeAngle, newKneeInfo.Angle.X) > m_walkingDetectionParams.RisingAngleTolerance) //stop if we detect a past non-Rising frames (we are looking for continuative Rising movement)
                        break;
                    if ((newKneeInfo.Time - m_walkDetectionsHistory[i].Timestamp).TotalMilliseconds*0.001f > m_walkingDetectionParams.TimeToTriggerMovement)
                    {
                        m_currentDetection.IsWalking = true;
                        m_currentDetection.KneeEstimatedStatus = KneeWalkingStatus.Rising;
                        m_currentDetection.EstimatedWalkSpeedCarrier = ComputeSpeedCarrier(newKneeInfo);
                        m_currentDetection.LastDetectedWalkInstant = m_currentDetection.Timestamp;
                        m_currentDetection.LastDetectedWalkStart = m_currentDetection.Timestamp;
                        break;
                    }
                }
            }
            //else (no Rising trigger)
            else
            {
                m_currentDetection = new KneeWalkingDetection
                {
                    IsWalking = false,
                    KneeRawStatus = KneeWalkingStatus.Still,
                    KneeEstimatedStatus = KneeWalkingStatus.Still,
                    Timestamp = newKneeInfo.Time,
                    EstimatedWalkSpeed = 0,
                    EstimatedWalkSpeedCarrier = 0,
                    HorizontalKneeAngle = m_walkDetectionsHistory[0].HorizontalKneeAngle,
                    LastDetectedWalkInstant = m_walkDetectionsHistory[0].LastDetectedWalkInstant,
                    LastDetectedWalkStart = m_walkDetectionsHistory[0].LastDetectedWalkStart
                };
            }
        }

        /// <summary>
        ///     Updates detection, knowing that at last frame the knee was rising
        /// </summary>
        /// <param name="newKneeInfo">Informations about the knee joint at current frame</param>
        private void UpdateRisingKnee(BodyJointCineticInfo newKneeInfo)
        {
            //if we are rising, we must check if the leg is pausing itself, in speed or acceleration, or if it began to go down

            //compute physical quantity of interest, in xz plane
            Vector2 xzQuantity = m_walkingDetectionParams.UseAcceleration ? new Vector2(newKneeInfo.InstantAcceleration.X, newKneeInfo.InstantAcceleration.Z)
                : new Vector2(newKneeInfo.InstantSpeed.X, newKneeInfo.InstantSpeed.Z);
            float yQuantity = m_walkingDetectionParams.UseAcceleration ? newKneeInfo.InstantAcceleration.Y
                : newKneeInfo.InstantSpeed.Y;

            //if we detected that the leg movement is becoming little 
            if (xzQuantity.Magnitude < m_walkingDetectionParams.AnyStateToStillThreshold &&
                Math.Abs(yQuantity) < m_walkingDetectionParams.AnyStateToStillThreshold)
            {
                //if joint vertical angle is very little, we are very likely to be in a situation where we are wrongly in a rising
                //state and we should be in a still state
                var shouldBeStill = Math.PI/2 + newKneeInfo.Angle.Y <= m_walkingDetectionParams.StillAngleThreshold;

                //initialize current detection results
                m_currentDetection = new KneeWalkingDetection
                {
                    IsWalking = true,
                    KneeRawStatus = shouldBeStill ? KneeWalkingStatus.Still : KneeWalkingStatus.RisingStill,
                    KneeEstimatedStatus = KneeWalkingStatus.Rising,
                    Timestamp = newKneeInfo.Time,
                    EstimatedWalkSpeed = m_walkingDetectionParams.AlmostStillSpeed,
                    EstimatedWalkSpeedCarrier = m_walkDetectionsHistory[0].EstimatedWalkSpeedCarrier,
                    HorizontalKneeAngle = newKneeInfo.Angle.X,
                    LastDetectedWalkInstant = newKneeInfo.Time,
                    LastDetectedWalkStart = m_walkDetectionsHistory[0].LastDetectedWalkStart
                };

                //if we are detecting stillness since enough time, trigger leg still result.
                //Remember to distinguish if we are in a rising still movment or if we are in a wrong-raising-movement detection
                for (var i = 0; i < m_walkDetectionsHistory.Count; i++)
                {
                    if (m_walkDetectionsHistory[i].KneeRawStatus != KneeWalkingStatus.RisingStill && m_walkDetectionsHistory[i].KneeRawStatus != KneeWalkingStatus.Still) //stop if we detect a past non-stillness frames (we are looking for continuative stillness)
                        break;
                    if ((newKneeInfo.Time - m_walkDetectionsHistory[i].Timestamp).TotalMilliseconds*0.001f > m_walkingDetectionParams.TimeToTriggerStillness)
                    {
                        m_currentDetection.IsWalking = !shouldBeStill;
                        m_currentDetection.KneeEstimatedStatus = shouldBeStill ? KneeWalkingStatus.Still : KneeWalkingStatus.RisingStill;
                        m_currentDetection.EstimatedWalkSpeed = 0;
                        m_currentDetection.EstimatedWalkSpeedCarrier = shouldBeStill ? 0 : m_walkDetectionsHistory[0].EstimatedWalkSpeedCarrier;
                        m_currentDetection.LastDetectedWalkInstant = shouldBeStill ? m_walkDetectionsHistory[0].Timestamp : m_currentDetection.Timestamp;
                        m_currentDetection.LastDetectedWalkStart = m_walkDetectionsHistory[0].LastDetectedWalkStart;
                        break;
                    }
                }
            }
            //if we detected that we are falling
            else if (xzQuantity.Magnitude > m_walkingDetectionParams.AnyStateToFallingThreshold &&
                     yQuantity < 0) //notice that we look that the trigger is towards the floor, because player can be oriented anywhere, so we don't have a right direction for falling in xz plane
            {
                //initialize current detection results
                m_currentDetection = new KneeWalkingDetection
                {
                    IsWalking = true,
                    KneeRawStatus = KneeWalkingStatus.Falling,
                    KneeEstimatedStatus = KneeWalkingStatus.Rising,
                    Timestamp = newKneeInfo.Time,
                    EstimatedWalkSpeed = m_walkDetectionsHistory[0].EstimatedWalkSpeed,
                    EstimatedWalkSpeedCarrier = m_walkDetectionsHistory[0].EstimatedWalkSpeedCarrier,
                    HorizontalKneeAngle = newKneeInfo.Angle.X,
                    LastDetectedWalkInstant = newKneeInfo.Time,
                    LastDetectedWalkStart = m_walkDetectionsHistory[0].LastDetectedWalkStart
                };

                //if we are detecting falling movement since enough time, trigger leg falling result
                for (var i = 0; i < m_walkDetectionsHistory.Count; i++)
                {
                    if (m_walkDetectionsHistory[i].KneeRawStatus != KneeWalkingStatus.Falling) //stop if we detect a past non-stillness frames (we are looking for continuative stillness)
                        break;
                    if ((newKneeInfo.Time - m_walkDetectionsHistory[i].Timestamp).TotalMilliseconds*0.001f > m_walkingDetectionParams.TimeToTriggerMovement)
                    {
                        m_currentDetection.KneeEstimatedStatus = KneeWalkingStatus.Falling;
                        break;
                    }
                }
            }
            //else (no stillness or falling trigger)
            else
            {
                //compute current detection
                m_currentDetection = new KneeWalkingDetection
                {
                    IsWalking = true,
                    KneeRawStatus = KneeWalkingStatus.Rising,
                    KneeEstimatedStatus = KneeWalkingStatus.Rising,
                    Timestamp = newKneeInfo.Time,
                    EstimatedWalkSpeed = ComputeCurrentWalkingSpeed(KneeWalkingStatus.Rising, m_walkDetectionsHistory[0], newKneeInfo),
                    EstimatedWalkSpeedCarrier = m_walkDetectionsHistory[0].EstimatedWalkSpeedCarrier,
                    HorizontalKneeAngle = newKneeInfo.Angle.X,
                    LastDetectedWalkInstant = newKneeInfo.Time,
                    LastDetectedWalkStart = m_walkDetectionsHistory[0].LastDetectedWalkStart
                };
            }
        }

        /// <summary>
        ///     Updates detection, knowing that at last frame the knee was rising
        /// </summary>
        /// <param name="newKneeInfo">Informations about the knee joint at current frame</param>
        private void UpdateRisingStillKnee(BodyJointCineticInfo newKneeInfo)
        {
            //if we are still during rising, we must check if the leg is restarting itself, in speed or acceleration, or if it began to go down.

            //compute physical quantity of interest, in xz plane
            Vector2 xzQuantity = m_walkingDetectionParams.UseAcceleration ? new Vector2(newKneeInfo.InstantAcceleration.X, newKneeInfo.InstantAcceleration.Z) : new Vector2(newKneeInfo.InstantSpeed.X, newKneeInfo.InstantSpeed.Z);
            float yQuantity = m_walkingDetectionParams.UseAcceleration ? newKneeInfo.InstantAcceleration.Y : newKneeInfo.InstantSpeed.Y;

            //if we detected that the leg movement is becoming big 
            if ((xzQuantity.Magnitude > m_walkingDetectionParams.StillToRisingThreshold || Math.Abs(yQuantity) > m_walkingDetectionParams.StillToRisingThreshold) && yQuantity > 0)
            {
                //initialize current detection results
                m_currentDetection = new KneeWalkingDetection
                {
                    IsWalking = true,
                    KneeRawStatus = KneeWalkingStatus.Rising,
                    KneeEstimatedStatus = KneeWalkingStatus.RisingStill,
                    Timestamp = newKneeInfo.Time,
                    EstimatedWalkSpeed = m_walkingDetectionParams.AlmostStillSpeed,
                    EstimatedWalkSpeedCarrier = m_walkDetectionsHistory[0].EstimatedWalkSpeedCarrier,
                    HorizontalKneeAngle = newKneeInfo.Angle.X,
                    LastDetectedWalkInstant = newKneeInfo.Time,
                    LastDetectedWalkStart = m_walkDetectionsHistory[0].LastDetectedWalkStart
                };

                //if we are detecting rising since enough time, trigger leg rising result
                for (var i = 0; i < m_walkDetectionsHistory.Count; i++)
                {
                    if (m_walkDetectionsHistory[i].KneeRawStatus != KneeWalkingStatus.Rising || MathUtilities.AdjustedAnglesAbsDifference(m_walkDetectionsHistory[i].HorizontalKneeAngle, newKneeInfo.Angle.X) > m_walkingDetectionParams.RisingAngleTolerance) //stop if we detect a past non-Rising frames (we are looking for continuative Rising movement)
                        break;

                    if ((newKneeInfo.Time - m_walkDetectionsHistory[i].Timestamp).TotalMilliseconds*0.001f > m_walkingDetectionParams.TimeToTriggerMovement)
                    {
                        m_currentDetection.IsWalking = true;
                        m_currentDetection.KneeEstimatedStatus = KneeWalkingStatus.Rising;
                        m_currentDetection.EstimatedWalkSpeedCarrier = ComputeSpeedCarrier(newKneeInfo);
                        m_currentDetection.LastDetectedWalkInstant = m_currentDetection.Timestamp;
                        m_currentDetection.LastDetectedWalkStart = m_walkDetectionsHistory[0].LastDetectedWalkStart;
                        break;
                    }
                }
            }
            //if we detected that we are falling
            else if (xzQuantity.Magnitude > m_walkingDetectionParams.AnyStateToFallingThreshold &&
                     yQuantity < 0) //notice that we look that the trigger is towards the floor, because player can be oriented anywhere, so we don't have a right direction for falling in xz plane
            {
                //initialize current detection results
                m_currentDetection = new KneeWalkingDetection
                {
                    IsWalking = true,
                    KneeRawStatus = KneeWalkingStatus.Falling,
                    KneeEstimatedStatus = KneeWalkingStatus.RisingStill,
                    Timestamp = newKneeInfo.Time,
                    EstimatedWalkSpeed = m_walkDetectionsHistory[0].EstimatedWalkSpeed,
                    EstimatedWalkSpeedCarrier = m_walkDetectionsHistory[0].EstimatedWalkSpeedCarrier,
                    HorizontalKneeAngle = newKneeInfo.Angle.X,
                    LastDetectedWalkInstant = newKneeInfo.Time,
                    LastDetectedWalkStart = m_walkDetectionsHistory[0].LastDetectedWalkStart
                };

                //if we are detecting falling movement since enough time, trigger leg falling result
                for (var i = 0; i < m_walkDetectionsHistory.Count; i++)
                {
                    if (m_walkDetectionsHistory[i].KneeRawStatus != KneeWalkingStatus.Falling) //stop if we detect a past non-stillness frames (we are looking for continuative stillness)
                        break;
                    if ((newKneeInfo.Time - m_walkDetectionsHistory[i].Timestamp).TotalMilliseconds*0.001f > m_walkingDetectionParams.TimeToTriggerMovement)
                    {
                        m_currentDetection.KneeEstimatedStatus = KneeWalkingStatus.Falling;
                        break;
                    }
                }
            }
            //if we detected that we are wrongly on the risingstill stage, while we should be in the still stage
            else if (Math.PI/2 + newKneeInfo.Angle.Y <= m_walkingDetectionParams.StillAngleThreshold)
            {
                //initialize current detection results
                m_currentDetection = new KneeWalkingDetection
                {
                    IsWalking = true,
                    KneeRawStatus = KneeWalkingStatus.Still,
                    KneeEstimatedStatus = KneeWalkingStatus.RisingStill,
                    Timestamp = newKneeInfo.Time,
                    EstimatedWalkSpeed = ComputeCurrentWalkingSpeed(KneeWalkingStatus.RisingStill, m_walkDetectionsHistory[0], newKneeInfo),
                    EstimatedWalkSpeedCarrier = m_walkDetectionsHistory[0].EstimatedWalkSpeedCarrier,
                    HorizontalKneeAngle = newKneeInfo.Angle.X,
                    LastDetectedWalkInstant = newKneeInfo.Time,
                    LastDetectedWalkStart = m_walkDetectionsHistory[0].LastDetectedWalkStart
                };

                //if we are detecting stillness since enough time, trigger leg still result
                for (var i = 0; i < m_walkDetectionsHistory.Count; i++)
                {
                    if (m_walkDetectionsHistory[i].KneeRawStatus != KneeWalkingStatus.Still) //stop if we detect a past non-stillness frames (we are looking for continuative stillness)
                        break;
                    if ((newKneeInfo.Time - m_walkDetectionsHistory[i].Timestamp).TotalMilliseconds*0.001f > m_walkingDetectionParams.TimeToTriggerStillness)
                    {
                        m_currentDetection.IsWalking = false;
                        m_currentDetection.KneeEstimatedStatus = KneeWalkingStatus.Still;
                        m_currentDetection.EstimatedWalkSpeed = m_currentDetection.EstimatedWalkSpeedCarrier = 0;
                        m_currentDetection.LastDetectedWalkInstant = m_walkDetectionsHistory[0].Timestamp;
                        break;
                    }
                }
            }
            //else (no rising, stillness or falling trigger)
            else
            {
                //compute current detection
                m_currentDetection = new KneeWalkingDetection
                {
                    IsWalking = true,
                    KneeRawStatus = KneeWalkingStatus.RisingStill,
                    KneeEstimatedStatus = KneeWalkingStatus.RisingStill,
                    Timestamp = newKneeInfo.Time,
                    EstimatedWalkSpeed = ComputeCurrentWalkingSpeed(KneeWalkingStatus.RisingStill, m_walkDetectionsHistory[0], newKneeInfo),
                    EstimatedWalkSpeedCarrier = m_walkDetectionsHistory[0].EstimatedWalkSpeedCarrier,
                    HorizontalKneeAngle = newKneeInfo.Angle.X,
                    LastDetectedWalkInstant = newKneeInfo.Time,
                    LastDetectedWalkStart = m_walkDetectionsHistory[0].LastDetectedWalkStart
                };
            }
        }

        /// <summary>
        ///     Updates detection, knowing that at last frame the knee was falling
        /// </summary>
        /// <param name="newKneeInfo">Informations about the knee joint at current frame</param>
        private void UpdateFallingKnee(BodyJointCineticInfo newKneeInfo)
        {
            //if we are falling, we must check if the leg stops itself, in speed or acceleration

            //compute physical quantity of interest, in xz plane
            Vector2 xzQuantity = m_walkingDetectionParams.UseAcceleration ? new Vector2(newKneeInfo.InstantAcceleration.X, newKneeInfo.InstantAcceleration.Z) : new Vector2(newKneeInfo.InstantSpeed.X, newKneeInfo.InstantSpeed.Z);

            //if we detected that the leg movement is becoming little 
            if (xzQuantity.Magnitude < m_walkingDetectionParams.AnyStateToStillThreshold || Math.PI/2 + newKneeInfo.Angle.Y <= m_walkingDetectionParams.StillAngleThreshold)
            {
                //initialize current detection results
                m_currentDetection = new KneeWalkingDetection
                {
                    IsWalking = true,
                    KneeRawStatus = KneeWalkingStatus.Still,
                    KneeEstimatedStatus = KneeWalkingStatus.Falling,
                    Timestamp = newKneeInfo.Time,
                    EstimatedWalkSpeed = m_walkingDetectionParams.AlmostStillSpeed,
                    EstimatedWalkSpeedCarrier = m_walkDetectionsHistory[0].EstimatedWalkSpeedCarrier,
                    HorizontalKneeAngle = newKneeInfo.Angle.X,
                    LastDetectedWalkInstant = newKneeInfo.Time,
                    LastDetectedWalkStart = m_walkDetectionsHistory[0].LastDetectedWalkStart
                };

                //if we are detecting stillness since enough time, trigger leg still result
                for (var i = 0; i < m_walkDetectionsHistory.Count; i++)
                {
                    if (m_walkDetectionsHistory[i].KneeRawStatus != KneeWalkingStatus.Still) //stop if we detect a past non-stillness frames (we are looking for continuative stillness)
                        break;
                    if ((newKneeInfo.Time - m_walkDetectionsHistory[i].Timestamp).TotalMilliseconds*0.001f > m_walkingDetectionParams.TimeToTriggerStillness)
                    {
                        m_currentDetection.IsWalking = false;
                        m_currentDetection.KneeEstimatedStatus = KneeWalkingStatus.Still;
                        m_currentDetection.EstimatedWalkSpeed = m_currentDetection.EstimatedWalkSpeedCarrier = 0;
                        m_currentDetection.LastDetectedWalkInstant = m_walkDetectionsHistory[0].Timestamp;
                        break;
                    }
                }
            }
            //else (no stillness trigger)
            else
            {
                //compute current detection
                m_currentDetection = new KneeWalkingDetection
                {
                    IsWalking = true,
                    KneeRawStatus = KneeWalkingStatus.Falling,
                    KneeEstimatedStatus = KneeWalkingStatus.Falling,
                    Timestamp = newKneeInfo.Time,
                    EstimatedWalkSpeed = ComputeCurrentWalkingSpeed(KneeWalkingStatus.Falling, m_walkDetectionsHistory[0], newKneeInfo),
                    EstimatedWalkSpeedCarrier = m_walkDetectionsHistory[0].EstimatedWalkSpeedCarrier,
                    HorizontalKneeAngle = newKneeInfo.Angle.X,
                    LastDetectedWalkInstant = newKneeInfo.Time,
                    LastDetectedWalkStart = m_walkDetectionsHistory[0].LastDetectedWalkStart
                };
            }
        }

        /// <summary>
        ///     Compute speed carrier for current walk movement
        /// </summary>
        /// <param name="newKneeInfo">Current knee informations</param>
        /// <returns>Estimated speed carrier</returns>
        private float ComputeSpeedCarrier(BodyJointCineticInfo newKneeInfo)
        {
            return newKneeInfo.InstantSpeed.Magnitude*m_walkingDetectionParams.TriggerToSpeedMultiplier;
        }

        /// <summary>
        ///     Estimates current walking speed from current knee status.
        ///     This helper function has to be called only when the player is walking
        /// </summary>
        /// <param name="kneeWalkingStatus">Current status of the kne</param>
        /// <param name="lastDetectedWalk">Last walk detection info</param>
        /// <param name="newKneeInfo">Info about knee at present time</param>
        /// <returns>Module of estimated player speed due to this knee movement</returns>
        private float ComputeCurrentWalkingSpeed(KneeWalkingStatus kneeWalkingStatus, KneeWalkingDetection lastDetectedWalk, BodyJointCineticInfo newKneeInfo)
        {
            if (kneeWalkingStatus == KneeWalkingStatus.Rising)
                return lastDetectedWalk.EstimatedWalkSpeedCarrier;
            if (kneeWalkingStatus == KneeWalkingStatus.RisingStill)
                return 0;
            if (kneeWalkingStatus == KneeWalkingStatus.Falling)
                return lastDetectedWalk.EstimatedWalkSpeedCarrier*m_walkingDetectionParams.FallingToRisingSpeedMultiplier;
            return 0;
        }

        #endregion
    }
}
namespace ImmotionAR.ImmotionRoom.DataSourceSensor.Kinect1
{
    using System;
    using System.Collections.Generic;
    using DataSourceService.Model;
    using Logger;
    using Microsoft.Kinect;
    using Protocol;
    using Common.Helpers.Math;

    internal class SkeletonDataProcessor : BaseStreamProcessor<SensorDataFrame, SkeletonFrame, DataSourceDataAvailableEventArgs>
    {
        #region Private fields

        private Skeleton[] m_Bodies;

        #endregion

        #region Constructor

        public SkeletonDataProcessor(TrackingSessionConfiguration trackingConfiguration) : base(LoggerService.GetLogger<SkeletonDataProcessor>(), trackingConfiguration)
        {
        }

        #endregion

        #region Private methods

        protected override void ProcessData()
        {
            if (Data != null)
            {
                OnDataAvailable(Data);
            }
        }

        internal override void SetData(SkeletonFrame frame)
        {
            Data = MapToSensorDataFrameEntity(frame);
        }

        private void OnDataAvailable(SensorDataFrame data)
        {
            base.OnDataAvailable(new DataSourceDataAvailableEventArgs(data));
        }

        private SensorDataFrame MapToSensorDataFrameEntity(SkeletonFrame frame)
        {
            var sensorData = new SensorDataFrame
            {
                FloorClipPlaneX = frame.FloorClipPlane.Item1,
                FloorClipPlaneY = frame.FloorClipPlane.Item2,
                FloorClipPlaneZ = frame.FloorClipPlane.Item3,
                FloorClipPlaneW = frame.FloorClipPlane.Item4,
                ClippingEdgesEnabled = m_TrackingConfiguration.BodyClippingEdgesEnabled,
                TrackHandsStatus = m_TrackingConfiguration.TrackHandsStatus,
                TrackJointRotation = m_TrackingConfiguration.TrackJointRotation,
            };

            if (m_Bodies == null || m_Bodies.Length != frame.SkeletonArrayLength)
            {
                m_Bodies = new Skeleton[frame.SkeletonArrayLength];
            }
            
            frame.CopySkeletonDataTo(m_Bodies);

            foreach (var b in m_Bodies)
            {
                if (b.TrackingState != SkeletonTrackingState.Tracked)
                {
                    continue;
                }

                var bodyData = new SensorBodyData
                {
                    TrackingId = (ulong)b.TrackingId,
                };

                if(m_TrackingConfiguration.BodyClippingEdgesEnabled)
                {
                    bodyData.ClippedEdges = (SceneClippedEdges) b.ClippedEdges;
                }

                // Kinect v1 does not track hands, so confidence always 0.0 and State = NotTracked
                if(m_TrackingConfiguration.TrackHandsStatus)
                {
                    bodyData.LeftHand = new SensorHandData() { Confidence = 0.0f, State = SensorHandState.NotTracked };
                    bodyData.RightHand = new SensorHandData() { Confidence = 0.0f, State = SensorHandState.NotTracked };
                }

                foreach (Joint joint in b.Joints)
                {
                    var jointData = new SensorBodyJointData
                    {
                        JointType = MapToSensorBodyJointTypes(joint.JointType),
                        PositionX = joint.Position.X,
                        PositionY = joint.Position.Y,
                        PositionZ = joint.Position.Z,
                        TrackingState = (SensorTrackingState) joint.TrackingState
                    };

                    if (m_TrackingConfiguration.TrackJointRotation)
                    {
                        jointData.OrientationX = b.BoneOrientations[joint.JointType].AbsoluteRotation.Quaternion.X;
                        jointData.OrientationY = b.BoneOrientations[joint.JointType].AbsoluteRotation.Quaternion.Y;
                        jointData.OrientationZ = b.BoneOrientations[joint.JointType].AbsoluteRotation.Quaternion.Z;
                        jointData.OrientationW = b.BoneOrientations[joint.JointType].AbsoluteRotation.Quaternion.W;
                    }

                    bodyData.Joints.Add(jointData.JointType, jointData);
                }

                // Add dummy joints for those not available in Kinect v1, but used by ImmotionRoom
                AddMissingJoints(bodyData.Joints);
                    
                sensorData.Bodies.Add(bodyData);
            }

            return sensorData;
        }

        private void AddMissingJoints(IDictionary<SensorBodyJointTypes, SensorBodyJointData> joints)
        {       
            //adjust spine and neck joints of Kinect v1 to match their Kinect v2 counterparts
            AdjustK1SpineJoints(joints);

            //adjust shoulders joints of Kinect v1 to match their Kinect v2 counterparts
            AdjustK1ShouldersJoints(joints);

            //adjust hands joints of Kinect v1 to match their Kinect v2 counterparts
            AdjustK1HandsJoints(joints);
        }

        /// <summary>
        /// Adjust spine joints of Kinect v1 so that they match with their Kinect v2 counterpart
        /// </summary>
        /// <param name="joints">Dictionary of body joints read from the sensor</param>
        private void AdjustK1SpineJoints(IDictionary<SensorBodyJointTypes, SensorBodyJointData> joints)
        {
            //in Kinect v1, the base spine joint is high wrt the position of Kinect v2. So we prolungate a bit the spine to lower the position of this joint
            const float prolungationFactor = -1.11f;
            SensoryJointUtilities.ExtendJointsSegment(joints[SensorBodyJointTypes.SpineMid], joints[SensorBodyJointTypes.SpineBase], prolungationFactor, joints[SensorBodyJointTypes.SpineBase]);

            //in Kinect v1, the spine mid point is not that in "mid" position of the spine. So, try to make it distant the same between the spine base and spine shoulder joints

            //so, calculate length of the vectors between base spine and spine mid and between spine mid and spine shoulder
            float baseToMid = SensoryJointUtilities.JointsDistance(joints[SensorBodyJointTypes.SpineBase], joints[SensorBodyJointTypes.SpineMid]);
            float midToShoulders = SensoryJointUtilities.JointsDistance(joints[SensorBodyJointTypes.SpineShoulder], joints[SensorBodyJointTypes.SpineMid]);

            //compute the mid of this measure, this should be the length where the joint stays
            float halfSpineLength = (baseToMid + midToShoulders) / 2;

            //compute how much length we have to add to base to mid segment (that is always way smaller than midToShoulders) to get to this half position
            float upperIncrement = midToShoulders - halfSpineLength;
            float upperPercentage = upperIncrement / midToShoulders;

            if (upperPercentage < 0) //should never happen because mid-to-base segment is always very little... but just in case...
                upperPercentage = 0;

            //compute final position of mid joint, interpolating mid and shoulder. Basically we're moving the mid joint on the line between spine shoulder and present spine mid joint
            SensoryJointUtilities.JointsPositionLerp(joints[SensorBodyJointTypes.SpineMid], joints[SensorBodyJointTypes.SpineShoulder], (1 - upperPercentage), joints[SensorBodyJointTypes.SpineMid]);

            //in Kinect v1, we don't have neck. We infer it continuing the spine line (from spine mid to spine shoulder) and taking a length proportional to current head length
            const float neckLengthFactor = 0.33f;

            float headLength = SensoryJointUtilities.JointsDistance(joints[SensorBodyJointTypes.SpineShoulder], joints[SensorBodyJointTypes.Head]);
            float neckLength = headLength * neckLengthFactor;

            //ok, this has to be explained: we're going to make longer the line between spine mid and spine shoulders by neckLength meters. So, we're proportionally going to move the point further by a factor neck_length / currentSegment_length
            float neckLengthPropToHalfDistance = neckLength / halfSpineLength;

            SensoryJointVector3 spineShoulderSpineMidDiff = SensoryJointUtilities.JointsPositionDiff(joints[SensorBodyJointTypes.SpineShoulder], joints[SensorBodyJointTypes.SpineMid]);
            SensoryJointVector3 neckPosition = SensoryJointUtilities.JointsPositionAddVector(joints[SensorBodyJointTypes.SpineShoulder], spineShoulderSpineMidDiff * neckLengthPropToHalfDistance);

            joints.Add(SensorBodyJointTypes.Neck,
                new SensorBodyJointData()
                {
                    PositionX = neckPosition.x,
                    PositionY = neckPosition.y,
                    PositionZ = neckPosition.z,
                    TrackingState = SensorTrackingState.Inferred,
                    JointType = SensorBodyJointTypes.Neck
                }
            );
        }

        /// <summary>
        /// Adjust shoulders joints of Kinect v1 so that they match with their Kinect v2 counterpart
        /// </summary>
        /// <param name="joints">Dictionary of body joints read from the sensor</param>
        private void AdjustK1ShouldersJoints(IDictionary<SensorBodyJointTypes, SensorBodyJointData> joints)
        {
            //in Kinect v1, shoulders are too "low" on the vertical plane. So, we rise them by a certain length on the same direction that has the spine
            const float shouldersRiseFactor = 0.115f; //proportional to length between new spine mid and spine shoulder

            //as before for the neck, compute this measure in term of half-spine proportion
            SensoryJointVector3 spineShoulderSpineMidDiff = SensoryJointUtilities.JointsPositionDiff(joints[SensorBodyJointTypes.SpineShoulder], joints[SensorBodyJointTypes.SpineMid]);

            SensoryJointUtilities.JointsPositionIncrementWithVector(joints[SensorBodyJointTypes.ShoulderLeft], spineShoulderSpineMidDiff * shouldersRiseFactor);
            SensoryJointUtilities.JointsPositionIncrementWithVector(joints[SensorBodyJointTypes.ShoulderRight], spineShoulderSpineMidDiff * shouldersRiseFactor);
        }

        /// <summary>
        /// Adjust hands joints of Kinect v1 so that they match with their Kinect v2 counterpart
        /// </summary>
        /// <param name="joints">Dictionary of body joints read from the sensor</param>
        private void AdjustK1HandsJoints(IDictionary<SensorBodyJointTypes, SensorBodyJointData> joints)
        {
            //for each hand
            for (int handIdx = 0; handIdx <= 1; handIdx++)
            {
                //set the constant depending of we're handling left or right part of the body
                SensorBodyJointTypes handJointType = handIdx == 0 ? SensorBodyJointTypes.HandLeft : SensorBodyJointTypes.HandRight;
                SensorBodyJointTypes wristJointType = handIdx == 0 ? SensorBodyJointTypes.WristLeft : SensorBodyJointTypes.WristRight;
                SensorBodyJointTypes elbowJointType = handIdx == 0 ? SensorBodyJointTypes.ElbowLeft : SensorBodyJointTypes.ElbowRight;
                SensorBodyJointTypes shoulderJointType = handIdx == 0 ? SensorBodyJointTypes.ShoulderLeft : SensorBodyJointTypes.ShoulderRight;
                SensorBodyJointTypes handTipJointType = handIdx == 0 ? SensorBodyJointTypes.HandTipLeft : SensorBodyJointTypes.HandTipRight;
                SensorBodyJointTypes thumbJointType = handIdx == 0 ? SensorBodyJointTypes.ThumbLeft : SensorBodyJointTypes.ThumbRight;

                //in Kinect v1, there are no "hand tips". We just re-create them prolonging the segment from wrist to hand
                const float handTipProlungationFactor = 0.89f;

                //compute difference vector between hand joint and wrist joint and take the best that we can about hand orientation
                SensoryJointVector3 handWristDiff = SensoryJointUtilities.JointsPositionDiff(joints[handJointType], joints[wristJointType]);
                float handWristDiffMagnitude = handWristDiff.Magnitude;

                //estimate the hand tip prolonging the segment
                SensoryJointVector3 handTipPosition = SensoryJointUtilities.JointsPositionAddVector(joints[handJointType], handWristDiff * handTipProlungationFactor);

                joints.Add(handTipJointType,
                    new SensorBodyJointData()
                    {
                        PositionX = handTipPosition.x,
                        PositionY = handTipPosition.y,
                        PositionZ = handTipPosition.z,
                        TrackingState = SensorTrackingState.Inferred,
                        JointType = handTipJointType
                    }
                );

                //in Kinect v2, there are no "thumbs". Re-create them as best as we can, since having only hand and wrist position is impossible to reconstruct thumb orientation. 
                //We estimate a joint position so that the hand appears as locked straight with arm

                //calculate arm and forearm vector
                SensoryJointVector3 forearmVector = SensoryJointUtilities.JointsPositionDiff(joints[wristJointType], joints[elbowJointType]);
                SensoryJointVector3 armVector = SensoryJointUtilities.JointsPositionDiff(joints[elbowJointType], joints[shoulderJointType]);

                //estimate palm position as cross product between arm and forearm vectors. This way we obtained how would be the palm if the hand would be just fixed as the extension of the forearm
                //(i.e. like if the person could not move his hand)
                SensoryJointVector3 palmVector = SensoryJointVector3.CrossProduct(forearmVector, armVector);

                SensoryJointVector3 thumbVector;

                //if estimated palm is parallel and contrary to hand orientation means that kinect has reported a bad measurements and is considering the hand as penetrating its own forearm
                if (SensoryJointVector3.DotProduct(handWristDiff.Normalized, forearmVector.Normalized) < -0.72f)
                {
                    //re-set hand tip position. This strange case often happens when the users puts the hand palm perpendicular to the forearm: the sensor sees the hand as rotated by 180°, while
                    //it is rotated only 90°. So put hand tip rotated as 90°
                    joints[handTipJointType].PositionX = joints[handJointType].PositionX - palmVector.x;
                    joints[handTipJointType].PositionY = joints[handJointType].PositionY - palmVector.y;
                    joints[handTipJointType].PositionZ = joints[handJointType].PositionZ - palmVector.z;

                    //make cross product between hand vector and inverted palm vector and find thumb position
                    thumbVector = SensoryJointVector3.CrossProduct(handWristDiff, palmVector * -1);
                }
                //else, if we can trust the detected hand pose from the sensor
                else
                {
                    //make cross product between hand vector and palm vector and find thumb position
                    thumbVector = SensoryJointVector3.CrossProduct(handWristDiff, palmVector);
                }

                //normalize thumb
                thumbVector = thumbVector.Normalized;

                //add the thumb joint, giving it a fixed length

                SensoryJointVector3 thumbPosition = SensoryJointUtilities.JointsPositionAddVector(joints[handJointType], thumbVector * 0.041f);

                joints.Add(thumbJointType,
                    new SensorBodyJointData()
                    {
                        PositionX = thumbPosition.x,
                        PositionY = thumbPosition.y,
                        PositionZ = thumbPosition.z,
                        TrackingState = SensorTrackingState.Inferred,
                        JointType = thumbJointType
                    }
                );

            }

        }

        // TODO: verify mapping!
        private SensorBodyJointTypes MapToSensorBodyJointTypes(JointType jointType)
        {
            switch(jointType)
            {
                case JointType.HipCenter:
                    return SensorBodyJointTypes.SpineBase;

                case JointType.Spine:
                    return SensorBodyJointTypes.SpineMid;

                case JointType.ShoulderCenter:
                    return SensorBodyJointTypes.SpineShoulder;
                    
                default:
                    return (SensorBodyJointTypes)jointType;
            }
        }

        
        #endregion
    }
}

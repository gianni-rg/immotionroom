namespace ImmotionAR.ImmotionRoom.DataSourceSensor.Kinect2
{
    using DataSourceService.Model;
    using Logger;
    using Microsoft.Kinect;
    using Protocol;

    internal class SkeletonDataProcessor : BaseStreamProcessor<SensorDataFrame, BodyFrame, DataSourceDataAvailableEventArgs>
    {
        #region Private fields

        private Body[] m_Bodies;

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

        internal override void SetData(BodyFrame frame)
        {
            Data = MapToSensorDataFrameEntity(frame);
        }

        private void OnDataAvailable(SensorDataFrame data)
        {
            base.OnDataAvailable(new DataSourceDataAvailableEventArgs(data));
        }

        private SensorDataFrame MapToSensorDataFrameEntity(BodyFrame frame)
        {
            var sensorData = new SensorDataFrame
            {
                FloorClipPlaneX = frame.FloorClipPlane.X,
                FloorClipPlaneY = frame.FloorClipPlane.Y,
                FloorClipPlaneZ = frame.FloorClipPlane.Z,
                FloorClipPlaneW = frame.FloorClipPlane.W,
                ClippingEdgesEnabled = m_TrackingConfiguration.BodyClippingEdgesEnabled,
                TrackHandsStatus = m_TrackingConfiguration.TrackHandsStatus,
                TrackJointRotation = m_TrackingConfiguration.TrackJointRotation,
            };

            if (m_Bodies == null || m_Bodies.Length != frame.BodyCount)
            {
                m_Bodies = new Body[frame.BodyCount];
            }

            frame.GetAndRefreshBodyData(m_Bodies);

            foreach (var b in m_Bodies)
            {
                if (!b.IsTracked)
                {
                    continue;
                }

                var bodyData = new SensorBodyData
                {
                    TrackingId = b.TrackingId,
                };

                if(m_TrackingConfiguration.BodyClippingEdgesEnabled)
                {
                    bodyData.ClippedEdges = (SceneClippedEdges) b.ClippedEdges;
                }

                if(m_TrackingConfiguration.TrackHandsStatus)
                {
                    bodyData.LeftHand = new SensorHandData() { Confidence = b.HandLeftConfidence == TrackingConfidence.High ? 1.0f : b.HandLeftState == HandState.NotTracked ? 0.0f : 0.1f, State = (SensorHandState) b.HandLeftState };
                    bodyData.RightHand = new SensorHandData() { Confidence = b.HandRightConfidence == TrackingConfidence.High ? 1.0f : b.HandRightState == HandState.NotTracked ? 0.0f : 0.1f, State = (SensorHandState) b.HandRightState };
                }

                foreach (var joint in b.Joints)
                {
                    var jointData = new SensorBodyJointData
                    {
                        JointType = (SensorBodyJointTypes) joint.Key,
                        PositionX = joint.Value.Position.X,
                        PositionY = joint.Value.Position.Y,
                        PositionZ = joint.Value.Position.Z,
                        TrackingState = (SensorTrackingState) joint.Value.TrackingState
                    };

                    if (m_TrackingConfiguration.TrackJointRotation)
                    {
                        jointData.OrientationX = b.JointOrientations[joint.Key].Orientation.X;
                        jointData.OrientationY = b.JointOrientations[joint.Key].Orientation.Y;
                        jointData.OrientationZ = b.JointOrientations[joint.Key].Orientation.Z;
                        jointData.OrientationW = b.JointOrientations[joint.Key].Orientation.W;
                    }
                    
                    bodyData.Joints.Add(jointData.JointType, jointData);
                }

                sensorData.Bodies.Add(bodyData);
            }

            return sensorData;
        }

        #endregion
    }
}

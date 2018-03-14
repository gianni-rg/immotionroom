namespace ImmotionAR.ImmotionRoom.TrackingService.Infrastructure.Network
{
    using System;
    using System.Collections.Generic;
    using ControlClient.Model;
    using DataClient.Model;
    using Model;
    using Protocol;
    using TrackingEngine.Model;
    using TrackingEngine.Walking;
    using TrackingServiceSceneDataStreamModes = ControlClient.Model.TrackingServiceSceneDataStreamModes;
    using TrackingServiceStatus = Model.TrackingServiceStatus;
    using TrackingSessionConfiguration = DataSource.ControlClient.Model.TrackingSessionConfiguration;
    using TrackingSessionDataSourceConfiguration = Model.TrackingSessionDataSourceConfiguration;

    public static class Mappers
    {
        public static SceneFrame ConvertToModel(this SensorDataFrame data, byte dataSourceId)
        {
            if (data == null)
            {
                return null;
            }

            var frame = new SceneFrame();
            frame.Timestamp = DateTime.UtcNow;

            frame.ClippingEdgesEnabled = data.ClippingEdgesEnabled;
            frame.TrackHandsStatus = data.TrackHandsStatus;
            frame.TrackJointRotation = data.TrackJointRotation;

            frame.FloorClipPlane = new Vector4(data.FloorClipPlaneX, data.FloorClipPlaneY, data.FloorClipPlaneZ, data.FloorClipPlaneW);

            foreach (var body in data.Bodies)
            {
                var bodyData = body.ConvertToModel(dataSourceId);
                frame.Bodies.Add(bodyData);
            }

            return frame;
        }

        public static TrackingSessionConfiguration ConvertToWebModel(this TrackingSessionDataSourceConfiguration data)
        {
            if (data == null)
            {
                return null;
            }

            var webModel = new TrackingSessionConfiguration();
            webModel.BodyClippingEdgesEnabled = data.BodyClippingEdgesEnabled;
            webModel.HandsStatusEnabled = data.HandsStatusEnabled;
            webModel.TrackJointRotation = data.TrackJointRotation;

            return webModel;
        }

        private static BodyData ConvertToModel(this SensorBodyData body, byte dataSourceId)
        {
            if (body == null)
            {
                return null;
            }

            var bodyData = new BodyData(body.TrackingId, new List<byte> {dataSourceId});

            bodyData.LeftHand = body.LeftHand.ConvertToModel();
            bodyData.RightHand = body.RightHand.ConvertToModel();
            bodyData.ClippedEdges = (FrameClippedEdges)body.ClippedEdges;

            foreach (var jointData in body.Joints.Values)
            {
                var frameJointData = new BodyJointData(new Vector3(jointData.PositionX, jointData.PositionY, jointData.PositionZ), new Vector4(jointData.OrientationX, jointData.OrientationY, jointData.OrientationZ, jointData.OrientationW), GetJointConfidence(jointData), (BodyJointTypes) jointData.JointType);
                bodyData.Joints.Add(frameJointData.JointType, frameJointData);
            }

            return bodyData;
        }

        private static BodyHandData ConvertToModel(this SensorHandData hand)
        {
            if (hand == null)
            {
                return null;
            }

            var handData = new BodyHandData();
            handData.Confidence = hand.Confidence;
            handData.State = (BodyHandState) hand.State;

            return handData;
        }

        public static TrackingServiceSceneFrame ConvertToTrackingServiceSceneFrame(this SceneFrame data, Matrix4x4 transform)
        {
            if (data == null)
            {
                return null;
            }

            var frame = new TrackingServiceSceneFrame();
            frame.Timestamp = data.Timestamp;

            frame.Version = 2;

            frame.ClippingEdgesEnabled = data.ClippingEdgesEnabled;
            frame.TrackHandsStatus = data.TrackHandsStatus;
            frame.TrackJointRotation = data.TrackJointRotation;

            foreach (var body in data.Bodies)
            {
                var bodyData = body.ConvertToTrackingServiceBodyData(transform);
                frame.Bodies.Add(bodyData);
            }
            return frame;
        }

        /// <summary>
        ///     Gets the joint confidence.
        /// </summary>
        /// <returns>The joint confidence</returns>
        /// <param name="joint">Joint</param>
        private static float GetJointConfidence(SensorBodyJointData joint)
        {
            switch (joint.TrackingState)
            {
                case SensorTrackingState.Tracked:
                    return 1.0f;
                case SensorTrackingState.Inferred:
                    return 0.1f;
                default:
                    return 0.0f;
            }
        }

        private static TrackingServiceBodyData ConvertToTrackingServiceBodyData(this BodyData body, Matrix4x4 transform)
        {
            if (body == null)
            {
                return null;
            }

            var bodyData = new TrackingServiceBodyData();
            bodyData.Id = body.Id;
            bodyData.DataSources = body.DataSources;

            bodyData.ClippedEdges = (TrackingServiceSceneClippedEdges) body.ClippedEdges;
            bodyData.LeftHand = body.LeftHand.ConvertToTrackingServiceHandData(transform);
            bodyData.RightHand = body.RightHand.ConvertToTrackingServiceHandData(transform);

            foreach (var jointData in body.Joints.Values)
            {
                // Transform joint using the provided transformation. Otherwise use RAW values.
                var jointPosition = jointData.Position;
                if (!transform.IsIdentity)
                {
                    jointPosition = transform.MultiplyPoint3x4(jointData.Position);
                }

                var frameJointData = new TrackingServiceBodyJointData
                {
                    Confidence = jointData.Confidence,
                    JointType = (TrackingServiceBodyJointTypes) jointData.JointType
                };

                frameJointData.Position.X = jointPosition.X;
                frameJointData.Position.Y = jointPosition.Y;
                frameJointData.Position.Z = jointPosition.Z;

                // GIANNI TODO: Transform orientation using the provided transformation. Otherwise use RAW values.
                frameJointData.Orientation.X = jointData.Orientation.X;
                frameJointData.Orientation.Y = jointData.Orientation.Y;
                frameJointData.Orientation.Z = jointData.Orientation.Z;
                frameJointData.Orientation.W = jointData.Orientation.W;

                bodyData.Joints.Add(frameJointData.JointType, frameJointData);
            }
            
            foreach (var bodyGesture in body.Gestures)
            {
                var gesture = bodyGesture.Value.ConvertToTrackingServiceBodyGesture();
                bodyData.Gestures.Add(gesture.GestureType, gesture);
            }

            return bodyData;
        }

        private static TrackingServiceHandData ConvertToTrackingServiceHandData(this BodyHandData hand, Matrix4x4 transform)
        {
            if (hand == null)
            {
                return null;
            }

            var handData = new TrackingServiceHandData();
            handData.Confidence = hand.Confidence;
            handData.State = (TrackingServiceHandState) hand.State;

            return handData;
        }

        private static TrackingServiceBodyGesture ConvertToTrackingServiceBodyGesture(this BodyGesture gesture)
        {
            if (gesture == null)
            {
                return null;
            }

            TrackingServiceBodyGesture trackingServiceGesture;
            switch (gesture.GestureType)
            {
                case BodyGestureTypes.Walking:
                    var modelGesture = (PlayerWalkingDetection)gesture;
                    var bodyGesture = new TrackingServiceWalkingGesture();
                    bodyGesture.BodyId = modelGesture.BodyId;
                    bodyGesture.Timestamp = modelGesture.Timestamp;
                    bodyGesture.EstimatedWalkSpeed = new TrackingServiceVector3() { X= modelGesture.EstimatedWalkSpeed.X, Y= modelGesture.EstimatedWalkSpeed.Y, Z=modelGesture.EstimatedWalkSpeed.Z};
                    bodyGesture.IsMoving = modelGesture.IsMoving;
                    bodyGesture.IsWalking = modelGesture.IsWalking;
                    trackingServiceGesture = bodyGesture;
                    break;

                default:
                    return null;
            }
            


            return trackingServiceGesture;
        }

        public static ControlClient.Model.TrackingServiceStatus ConvertToWebModel(this TrackingServiceStatus model)
        {
            if (model == null)
            {
                return null;
            }

            var webModel = new ControlClient.Model.TrackingServiceStatus()
            {
                Version = model.Version,
                CurrentState = (ControlClient.Model.TrackingServiceState)model.CurrentState,
                CalibrationDone = model.CalibrationDone,
                MasterDataStreamer = model.MasterDataStreamer,
                MinDataSourcesForPlay = model.MinDataSourcesForPlay,
                DataFrameRate = model.DataFrameRate,
                Scene = model.Scene.ConvertToWebModel(),
            };
            webModel.DataSources = new Dictionary<string, TrackingServiceDataSourceInfo>();
            foreach (var dataSourcePair in model.DataSources)
            {
                webModel.DataSources.Add(dataSourcePair.Key, dataSourcePair.Value.ConvertToWebModel());
            }
            webModel.DataStreamers = new Dictionary<string, TrackingServiceDataStreamerInfo>();
            foreach (var dataStreamerPair in model.DataStreamers)
            {
                webModel.DataStreamers.Add(dataStreamerPair.Key, dataStreamerPair.Value.ConvertToWebModel());
            }

            return webModel;
        }

        private static ControlClient.Model.TrackingServiceSceneDescriptor ConvertToWebModel(this SceneDescriptor model)
        {
            if (model == null)
            {
                return null;
            }

            var webModel = new ControlClient.Model.TrackingServiceSceneDescriptor()
            {
                FloorClipPlane = new TrackingServiceVector4 { X = model.FloorClipPlane.X, Y = model.FloorClipPlane.Y , Z = model.FloorClipPlane.Z, W = model.FloorClipPlane.W},
                GameArea = new TrackingServiceSceneBoundaries
                {
                    Center = new TrackingServiceVector3 { X = model.GameArea.Center.X, Y = model.GameArea.Center.Y, Z = model.GameArea.Center.Z },
                    Size = new TrackingServiceVector3 { X = model.GameArea.Size.X, Y = model.GameArea.Size.Y, Z = model.GameArea.Size.Z }
                },
                StageArea= new TrackingServiceSceneBoundaries
                {
                    Center = new TrackingServiceVector3 { X = model.StageArea.Center.X, Y = model.StageArea.Center.Y, Z = model.StageArea.Center.Z },
                    Size = new TrackingServiceVector3 { X = model.StageArea.Size.X, Y = model.StageArea.Size.Y, Z = model.StageArea.Size.Z }
                },
                GameAreaInnerLimits = new TrackingServiceVector3 { X = model.GameAreaInnerLimits.X, Y = model.GameAreaInnerLimits.Y, Z = model.GameAreaInnerLimits.Z }

            };

            return webModel;
        }

        private static ControlClient.Model.TrackingServiceDataSourceInfo ConvertToWebModel(this DataSourceInfo model)
        {
            if (model == null)
            {
                return null;
            }

            var webModel = new ControlClient.Model.TrackingServiceDataSourceInfo()
            {
               UniqueId = model.UniqueId,
               Id = model.Id,
               IsMaster = model.IsMaster,
               ControlApiEndpoint = model.ControlApiEndpoint,
               ControlApiPort = model.ControlApiPort,
               DataStreamEndpoint = model.DataStreamEndpoint,
               DataStreamPort = model.DataStreamPort,
               FirstTimeSeen = model.FirstTimeSeen,
               IsReachable = model.IsReachable,
               LastSeen = model.LastSeen,
            };

            return webModel;
        }

        private static ControlClient.Model.TrackingServiceDataStreamerInfo ConvertToWebModel(this SceneDataStreamerInfo model)
        {
            if (model == null)
            {
                return null;
            }

            var webModel = new ControlClient.Model.TrackingServiceDataStreamerInfo()
            {
                Id = model.Id,
                IsMaster = model.IsMaster,
                StreamEndpoint = model.StreamEndpoint,
                StreamPort = model.StreamPort,
            };

            webModel.SupportedStreamModes = new List<TrackingServiceSceneDataStreamModes>();
            foreach (var trackingServiceSceneDataStreamMode in model.SupportedStreamModes)
            {
                webModel.SupportedStreamModes.Add((ControlClient.Model.TrackingServiceSceneDataStreamModes)trackingServiceSceneDataStreamMode);
            }

            return webModel;
        }
    }
}

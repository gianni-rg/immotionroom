namespace ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;

    public class TrackingServiceSceneFrameSerializer
    {
        #region Public methods

        public byte[] Serialize(TrackingServiceSceneFrame frame)
        {
            if (frame == null)
            {
                throw new ArgumentNullException("frame");
            }

            if (frame.Version == 1)
            {
                return SerializeVersion1(frame);
            }

            if (frame.Version == 2)
            {
                return SerializeVersion2(frame);
            }

            throw new InvalidOperationException("Unsupported TrackingServiceSceneFrame version");
        }

        public TrackingServiceSceneFrame Deserialize(byte[] serializedBytes)
        {
            if (serializedBytes == null)
            {
                throw new ArgumentNullException("serializedBytes");
            }

            var frameVersion = serializedBytes[0];

            if (frameVersion == 1)
            {
                return DeserializeVersion1(serializedBytes);
            }

            if (frameVersion == 2)
            {
                return DeserializeVersion2(serializedBytes);
            }

            throw new InvalidOperationException("Unsupported TrackingServiceSceneFrame version");
        }

        #endregion

        #region Private methods

        private byte[] SerializeVersion1(TrackingServiceSceneFrame frame)
        {
            if (frame == null)
            {
                throw new ArgumentNullException("frame");
            }

            var totalSize = 2*sizeof (byte) + sizeof (long) + (sizeof (ulong) + 2*sizeof (byte) + 3*sizeof (float))*frame.Bodies.Count;
            for (var i = 0; i < frame.Bodies.Count; i++)
            {
                totalSize += (sizeof (byte) + 4*sizeof (float))*frame.Bodies[i].Joints.Count;
                totalSize += frame.Bodies[i].DataSources.Count; // length of DataSources array
            }

            byte[] serializedData;
            using (var memStream = new MemoryStream(totalSize))
            {
                using (var w = new BinaryWriter(memStream))
                {
                    w.Write(frame.Version);
                    w.Write(frame.Timestamp.Ticks);
                    w.Write((byte) frame.Bodies.Count); // HERE WE LIMIT TO A MAX OF 256 BODIES!

                    for (var i = 0; i < frame.Bodies.Count; i++)
                    {
                        w.Write(frame.Bodies[i].Id);
                        w.Write((byte) frame.Bodies[i].DataSources.Count); // HERE WE LIMIT TO A MAX OF 256 BODIES/DATASOURCES!
                        foreach (var dataSourceId in frame.Bodies[i].DataSources)
                        {
                            w.Write(dataSourceId);
                        }
                        w.Write(frame.Bodies[i].Position.X);
                        w.Write(frame.Bodies[i].Position.Y);
                        w.Write(frame.Bodies[i].Position.Z);
                        w.Write((byte) frame.Bodies[i].Joints.Count); // HERE WE LIMIT TO A MAX OF 256 JOINTS!

                        foreach (var jointData in frame.Bodies[i].Joints.Values)
                        {
                            w.Write((byte) jointData.JointType); // HERE WE LIMIT TO A MAX OF 256 JOINT TYPES!
                            w.Write(jointData.Confidence);
                            w.Write(jointData.Position.X);
                            w.Write(jointData.Position.Y);
                            w.Write(jointData.Position.Z);
                        }
                    }

                    w.Flush();
                }

                serializedData = memStream.ToArray();
            }

            if (totalSize != serializedData.Length)
            {
                throw new SerializationException("totalSize != serializedData.Length");
            }

            return serializedData;
        }

        private TrackingServiceSceneFrame DeserializeVersion1(byte[] serializedBytes)
        {
            if (serializedBytes == null)
            {
                throw new ArgumentNullException("serializedBytes");
            }

            var frame = new TrackingServiceSceneFrame();

            using (var memStream = new MemoryStream())
            {
                using (var r = new BinaryReader(memStream))
                {
                    memStream.Write(serializedBytes, 0, serializedBytes.Length);
                    memStream.Seek(0, SeekOrigin.Begin);

                    // At the moment is not used... for future use
                    var version = r.ReadByte();

                    // It seems there is a BUG in UNITY/Mono?! We have to read as UInt and then cast to Long to avoid exception
                    frame.Timestamp = new DateTime((long) r.ReadUInt64());
                    //frame.Timestamp = new DateTime(r.ReadInt64());

                    var bodyCount = r.ReadByte(); // HERE WE LIMIT TO A MAX OF 256 BODIES!

                    for (byte i = 0; i < bodyCount; i++)
                    {
                        var bodyData = new TrackingServiceBodyData();

                        bodyData.Id = r.ReadUInt64();
                        int dataSourcesCount = r.ReadByte(); // HERE WE LIMIT TO A MAX OF 256 BODIES/DATASOURCES!
                        for (var j = 0; j < dataSourcesCount; j++)
                        {
                            bodyData.DataSources.Add(r.ReadByte());
                        }
                        bodyData.Position.X = r.ReadSingle();
                        bodyData.Position.Y = r.ReadSingle();
                        bodyData.Position.Z = r.ReadSingle();

                        frame.Bodies.Add(bodyData);

                        var jointsCount = r.ReadByte(); // HERE WE LIMIT TO A MAX OF 256 JOINTS!

                        for (byte j = 0; j < jointsCount; j++)
                        {
                            var jointData = new TrackingServiceBodyJointData();

                            jointData.JointType = (TrackingServiceBodyJointTypes) r.ReadByte(); // HERE WE LIMIT TO A MAX OF 256 JOINT TYPES!
                            jointData.Confidence = r.ReadSingle();
                            jointData.Position.X = r.ReadSingle();
                            jointData.Position.Y = r.ReadSingle();
                            jointData.Position.Z = r.ReadSingle();

                            frame.Bodies[i].Joints.Add(jointData.JointType, jointData);
                        }
                    }
                }
            }

            return frame;
        }

        private byte[] SerializeVersion2(TrackingServiceSceneFrame frame)
        {
            if (frame == null)
            {
                throw new ArgumentNullException("frame");
            }

            var totalSize = 3*sizeof (byte) + sizeof (long) + (sizeof (ulong) + 3*sizeof (byte) + 3*sizeof (float))*frame.Bodies.Count;

            for (var i = 0; i < frame.Bodies.Count; i++)
            {
                totalSize += (sizeof (byte) + 4*sizeof (float))*frame.Bodies[i].Joints.Count;
                totalSize += frame.Bodies[i].DataSources.Count*sizeof (byte); // length of DataSources array (each item is 1 byte)
                foreach (var gesture in frame.Bodies[i].Gestures)
                {
                    totalSize += GetGestureSizeFromType(gesture.Value.GestureType);
                }

                if (frame.TrackJointRotation)
                {
                    totalSize += 4*sizeof (float)*frame.Bodies[i].Joints.Count;
                }

                if (frame.TrackHandsStatus)
                {
                    totalSize += 2*sizeof (float) + 2*sizeof (byte);
                }

                if (frame.ClippingEdgesEnabled)
                {
                    totalSize += 1*sizeof (byte);
                }
            }

            byte[] serializedData;
            using (var memStream = new MemoryStream(totalSize))
            {
                using (var w = new BinaryWriter(memStream))
                {
                    w.Write(frame.Version);

                    byte featureFlag = 0x0;
                    featureFlag |= (byte) (frame.ClippingEdgesEnabled ? 0x1 : 0x0);
                    featureFlag |= (byte) (frame.TrackHandsStatus ? 0x2 : 0x0);
                    featureFlag |= (byte) (frame.TrackJointRotation ? 0x4 : 0x0);
                    w.Write(featureFlag);

                    w.Write(frame.Timestamp.Ticks);

                    //w.Write(frame.Scene.FloorClipPlane.X);
                    //w.Write(frame.Scene.FloorClipPlane.Y);
                    //w.Write(frame.Scene.FloorClipPlane.Z);
                    //w.Write(frame.Scene.FloorClipPlane.W);
                    //w.Write(frame.Scene.GameArea.Center.X);
                    //w.Write(frame.Scene.GameArea.Center.Y);
                    //w.Write(frame.Scene.GameArea.Center.Z);
                    //w.Write(frame.Scene.GameArea.Size.X);
                    //w.Write(frame.Scene.GameArea.Size.Y);
                    //w.Write(frame.Scene.GameArea.Size.Z);
                    //w.Write(frame.Scene.StageArea.Center.X);
                    //w.Write(frame.Scene.StageArea.Center.Y);
                    //w.Write(frame.Scene.StageArea.Center.Z);
                    //w.Write(frame.Scene.StageArea.Size.X);
                    //w.Write(frame.Scene.StageArea.Size.Y);
                    //w.Write(frame.Scene.StageArea.Size.Z);

                    w.Write((byte) frame.Bodies.Count); // HERE WE LIMIT TO A MAX OF 256 BODIES!

                    for (var i = 0; i < frame.Bodies.Count; i++)
                    {
                        w.Write(frame.Bodies[i].Id);
                        w.Write((byte) frame.Bodies[i].DataSources.Count); // HERE WE LIMIT TO A MAX OF 256 DATASOURCES!

                        foreach (var dataSourceId in frame.Bodies[i].DataSources)
                        {
                            w.Write(dataSourceId);
                        }

                        if (frame.ClippingEdgesEnabled)
                        {
                            w.Write((byte) frame.Bodies[i].ClippedEdges);
                        }

                        if (frame.TrackHandsStatus)
                        {
                            w.Write(frame.Bodies[i].LeftHand.Confidence);
                            w.Write((byte) frame.Bodies[i].LeftHand.State);
                            w.Write(frame.Bodies[i].RightHand.Confidence);
                            w.Write((byte) frame.Bodies[i].RightHand.State);
                        }

                        w.Write(frame.Bodies[i].Position.X);
                        w.Write(frame.Bodies[i].Position.Y);
                        w.Write(frame.Bodies[i].Position.Z);

                        w.Write((byte) frame.Bodies[i].Joints.Count); // HERE WE LIMIT TO A MAX OF 256 JOINTS!

                        foreach (var jointData in frame.Bodies[i].Joints.Values)
                        {
                            w.Write((byte) jointData.JointType); // HERE WE LIMIT TO A MAX OF 256 JOINT TYPES!
                            w.Write(jointData.Confidence);
                            w.Write(jointData.Position.X);
                            w.Write(jointData.Position.Y);
                            w.Write(jointData.Position.Z);

                            if (frame.TrackJointRotation)
                            {
                                w.Write(jointData.Orientation.X);
                                w.Write(jointData.Orientation.Y);
                                w.Write(jointData.Orientation.Z);
                                w.Write(jointData.Orientation.W);
                            }
                        }

                        w.Write((byte) frame.Bodies[i].Gestures.Count); // HERE WE LIMIT TO A MAX OF 256 GESTURE TYPES!
                        foreach (var gesture in frame.Bodies[i].Gestures)
                        {
                            SerializeGesture(gesture.Value, w);
                        }
                    }

                    w.Flush();
                }

                serializedData = memStream.ToArray();
            }

            if (totalSize != serializedData.Length)
            {
                throw new SerializationException("totalSize != serializedData.Length");
            }

            return serializedData;
        }

        private TrackingServiceSceneFrame DeserializeVersion2(byte[] serializedBytes)
        {
            if (serializedBytes == null)
            {
                throw new ArgumentNullException("serializedBytes");
            }

            var frame = new TrackingServiceSceneFrame();

            using (var memStream = new MemoryStream())
            {
                using (var r = new BinaryReader(memStream))
                {
                    memStream.Write(serializedBytes, 0, serializedBytes.Length);
                    memStream.Seek(0, SeekOrigin.Begin);

                    var version = r.ReadByte(); // Ignore. It is Version 2 by construction

                    var featureFlag = r.ReadByte();
                    frame.ClippingEdgesEnabled = (featureFlag & 0x1) == 1;
                    frame.TrackHandsStatus = (featureFlag & 0x2) == 2;
                    frame.TrackJointRotation = (featureFlag & 0x4) == 4;

                    // It seems there is a BUG in UNITY/Mono?! We have to read as UInt and then cast to Long to avoid exception
                    frame.Timestamp = new DateTime((long) r.ReadUInt64());
                    //frame.Timestamp = new DateTime(r.ReadInt64());

                    //frame.Scene.FloorClipPlane.X = r.ReadSingle();
                    //frame.Scene.FloorClipPlane.Y = r.ReadSingle();
                    //frame.Scene.FloorClipPlane.Z = r.ReadSingle();
                    //frame.Scene.FloorClipPlane.W = r.ReadSingle();
                    //frame.Scene.GameArea.Center.X = r.ReadSingle();
                    //frame.Scene.GameArea.Center.Y = r.ReadSingle();
                    //frame.Scene.GameArea.Center.Z = r.ReadSingle();
                    //frame.Scene.GameArea.Size.X = r.ReadSingle();
                    //frame.Scene.GameArea.Size.Y = r.ReadSingle();
                    //frame.Scene.GameArea.Size.Z = r.ReadSingle();
                    //frame.Scene.StageArea.Center.X = r.ReadSingle();
                    //frame.Scene.StageArea.Center.Y = r.ReadSingle();
                    //frame.Scene.StageArea.Center.Z = r.ReadSingle();
                    //frame.Scene.StageArea.Size.X = r.ReadSingle();
                    //frame.Scene.StageArea.Size.Y = r.ReadSingle();
                    //frame.Scene.StageArea.Size.Z = r.ReadSingle();

                    var bodyCount = r.ReadByte(); // HERE WE LIMIT TO A MAX OF 256 BODIES!

                    for (byte i = 0; i < bodyCount; i++)
                    {
                        var bodyData = new TrackingServiceBodyData();

                        bodyData.Id = r.ReadUInt64();
                        int dataSourcesCount = r.ReadByte(); // HERE WE LIMIT TO A MAX OF 256 DATASOURCES!
                        for (var j = 0; j < dataSourcesCount; j++)
                        {
                            bodyData.DataSources.Add(r.ReadByte());
                        }

                        if (frame.ClippingEdgesEnabled)
                        {
                            bodyData.ClippedEdges = (TrackingServiceSceneClippedEdges)r.ReadByte();
                        }

                        if (frame.TrackHandsStatus)
                        {
                            bodyData.LeftHand = new TrackingServiceHandData();
                            bodyData.LeftHand.Confidence = r.ReadSingle();
                            bodyData.LeftHand.State = (TrackingServiceHandState) r.ReadByte();

                            bodyData.RightHand = new TrackingServiceHandData();
                            bodyData.RightHand.Confidence = r.ReadSingle();
                            bodyData.RightHand.State = (TrackingServiceHandState) r.ReadByte();
                        }

                        bodyData.Position.X = r.ReadSingle();
                        bodyData.Position.Y = r.ReadSingle();
                        bodyData.Position.Z = r.ReadSingle();

                        frame.Bodies.Add(bodyData);

                        var jointsCount = r.ReadByte(); // HERE WE LIMIT TO A MAX OF 256 JOINTS!

                        for (byte j = 0; j < jointsCount; j++)
                        {
                            var jointData = new TrackingServiceBodyJointData();

                            jointData.JointType = (TrackingServiceBodyJointTypes) r.ReadByte(); // HERE WE LIMIT TO A MAX OF 256 JOINT TYPES!
                            jointData.Confidence = r.ReadSingle();
                            jointData.Position.X = r.ReadSingle();
                            jointData.Position.Y = r.ReadSingle();
                            jointData.Position.Z = r.ReadSingle();

                            if (frame.TrackJointRotation)
                            {
                                jointData.Orientation.X = r.ReadSingle();
                                jointData.Orientation.Y = r.ReadSingle();
                                jointData.Orientation.Z = r.ReadSingle();
                                jointData.Orientation.W = r.ReadSingle();
                            }

                            frame.Bodies[i].Joints.Add(jointData.JointType, jointData);
                        }

                        var gesturesCount = r.ReadByte(); // HERE WE LIMIT TO A MAX OF 256 GESTURE TYPES!

                        for (var j = 0; j < gesturesCount; j++)
                        {
                            var gesture = DeserializeGesture(r);
                            frame.Bodies[i].Gestures.Add(gesture.GestureType, gesture);
                        }
                    }
                }
            }

            return frame;
        }

        private void SerializeGesture(TrackingServiceBodyGesture gesture, BinaryWriter writer)
        {
            writer.Write((byte) gesture.GestureType);

            switch (gesture.GestureType)
            {
                case TrackingServiceBodyGestureTypes.Walking:

                    var walkingGesture = (TrackingServiceWalkingGesture) gesture;
                    writer.Write(walkingGesture.EstimatedWalkSpeed.X);
                    writer.Write(walkingGesture.EstimatedWalkSpeed.Y);
                    writer.Write(walkingGesture.EstimatedWalkSpeed.Z);

                    byte flags = 0x0;
                    flags |= (byte) (walkingGesture.IsMoving ? 0x1 : 0x0);
                    flags |= (byte) (walkingGesture.IsWalking ? 0x2 : 0x0);
                    writer.Write(flags);

                    return;

                default:
                    throw new InvalidOperationException("Unsupported Gesture");
            }
        }

        private TrackingServiceBodyGesture DeserializeGesture(BinaryReader reader)
        {
            var type = (TrackingServiceBodyGestureTypes) reader.ReadByte();

            switch (type)
            {
                case TrackingServiceBodyGestureTypes.Walking:

                    var gesture = new TrackingServiceWalkingGesture();
                    // gesture.BodyId  --> is assigned from the TrackingServiceBodyData object which contains this gesture
                    // gesture.Timestamp --> is assigned from the TrackingServiceSceneFrame object which contains this body gesture
                    gesture.EstimatedWalkSpeed.X = reader.ReadSingle();
                    gesture.EstimatedWalkSpeed.Y = reader.ReadSingle();
                    gesture.EstimatedWalkSpeed.Z = reader.ReadSingle();

                    var flags = reader.ReadByte();
                    gesture.IsMoving = (flags & 0x01) == 1;
                    gesture.IsWalking = (flags & 0x02) == 2;

                    return gesture;

                default:
                    throw new InvalidOperationException("Unsupported Gesture");
            }
        }

        private int GetGestureSizeFromType(TrackingServiceBodyGestureTypes type)
        {
            switch (type)
            {
                case TrackingServiceBodyGestureTypes.Walking:
                    return 2*sizeof (byte) + 3*sizeof (float);

                default:
                    return 0;
            }
        }

        #endregion
    }
}

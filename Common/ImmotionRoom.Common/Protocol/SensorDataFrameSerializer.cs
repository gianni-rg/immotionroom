namespace ImmotionAR.ImmotionRoom.Protocol
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;

    public class SensorDataFrameSerializer
    {
        #region Public methods
        public byte[] Serialize(SensorDataFrame frame)
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

            throw new InvalidOperationException("Unsupported SensorDataFrame version");
        }

        public SensorDataFrame Deserialize(byte[] serializedBytes)
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

            throw new InvalidOperationException("Unsupported SensorDataFrame version");
        } 
        #endregion

        #region Private methods
        private byte[] SerializeVersion1(SensorDataFrame frame)
        {
            var totalSize = 2 * sizeof(byte) + 4 * sizeof(float) + (sizeof(ulong) + sizeof(byte)) * frame.Bodies.Count;
            for (var i = 0; i < frame.Bodies.Count; i++)
            {
                // AT THE MOMENT WE ARE NOT USING Orientation INFO
                totalSize += (2 * sizeof(byte) + 3 * sizeof(float) /*+ 4 * sizeof(float)*/) * frame.Bodies[i].Joints.Count;
            }

            byte[] serializedData;

            using (var memStream = new MemoryStream(totalSize))
            {
                using (var w = new BinaryWriter(memStream))
                {
                    w.Write(frame.Version);
                    w.Write(frame.FloorClipPlaneX);
                    w.Write(frame.FloorClipPlaneY);
                    w.Write(frame.FloorClipPlaneZ);
                    w.Write(frame.FloorClipPlaneW);
                    w.Write((byte)frame.Bodies.Count); // HERE WE LIMIT TO A MAX OF 256 BODIES!

                    for (var i = 0; i < frame.Bodies.Count; i++)
                    {
                        w.Write(frame.Bodies[i].TrackingId);
                        w.Write((byte)frame.Bodies[i].Joints.Count); // HERE WE LIMIT TO A MAX OF 256 JOINTS!

                        foreach (var jointData in frame.Bodies[i].Joints.Values)
                        {
                            w.Write((byte)jointData.JointType); // HERE WE LIMIT TO A MAX OF 256 JOINT TYPES!
                            w.Write((byte)jointData.TrackingState); // HERE WE LIMIT TO A MAX OF 256 TRACKING STATE TYPES!
                            w.Write(jointData.PositionX);
                            w.Write(jointData.PositionY);
                            w.Write(jointData.PositionZ);
                            // AT THE MOMENT WE ARE NOT USING Orientation INFO
                            //w.Write(jointData.OrientationX);
                            //w.Write(jointData.OrientationY);
                            //w.Write(jointData.OrientationZ);
                            //w.Write(jointData.OrientationW);
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

        private SensorDataFrame DeserializeVersion1(byte[] serializedBytes)
        {
            if (serializedBytes == null)
            {
                throw new ArgumentNullException("serializedBytes");
            }

            var frame = new SensorDataFrame();

            using (var memStream = new MemoryStream())
            {
                using (var r = new BinaryReader(memStream))
                {
                    memStream.Write(serializedBytes, 0, serializedBytes.Length);
                    memStream.Seek(0, SeekOrigin.Begin);

                    // At the moment is not used... for future use
                    var version = r.ReadByte();
                    
                    frame.FloorClipPlaneX = r.ReadSingle();
                    frame.FloorClipPlaneY = r.ReadSingle();
                    frame.FloorClipPlaneZ = r.ReadSingle();
                    frame.FloorClipPlaneW = r.ReadSingle();

                    var bodyCount = r.ReadByte(); // HERE WE LIMIT TO A MAX OF 256 BODIES!

                    for (byte i = 0; i < bodyCount; i++)
                    {
                        var bodyData = new SensorBodyData();

                        bodyData.TrackingId = r.ReadUInt64();

                        frame.Bodies.Add(bodyData);

                        var jointsCount = r.ReadByte(); // HERE WE LIMIT TO A MAX OF 256 JOINTS!

                        for (byte j = 0; j < jointsCount; j++)
                        {
                            var jointData = new SensorBodyJointData();

                            jointData.JointType = (SensorBodyJointTypes)r.ReadByte(); // HERE WE LIMIT TO A MAX OF 256 JOINT TYPES!
                            jointData.TrackingState = (SensorTrackingState)r.ReadByte(); // HERE WE LIMIT TO A MAX OF 256 TRACKING STATE TYPES!
                            jointData.PositionX = r.ReadSingle();
                            jointData.PositionY = r.ReadSingle();
                            jointData.PositionZ = r.ReadSingle();

                            // AT THE MOMENT WE ARE NOT USING Orientation INFO
                            //jointData.OrientationX = r.ReadSingle();
                            //jointData.OrientationY = r.ReadSingle();
                            //jointData.OrientationZ = r.ReadSingle();
                            // jointData.OrientationW = r.ReadSingle();

                            frame.Bodies[i].Joints.Add(jointData.JointType, jointData);
                        }
                    }
                }
            }

            return frame;
        }

        private byte[] SerializeVersion2(SensorDataFrame frame)
        {
            var totalSize = 3 * sizeof(byte) + 4 * sizeof(float) + (sizeof(ulong) + 1*sizeof(byte)) * frame.Bodies.Count;
            for (var i = 0; i < frame.Bodies.Count; i++)
            {
                totalSize += (2 * sizeof(byte) + 3 * sizeof(float)) * frame.Bodies[i].Joints.Count;

                if (frame.TrackJointRotation)
                {
                    totalSize += (4 * sizeof(float)) * frame.Bodies[i].Joints.Count;
                }

                if (frame.TrackHandsStatus)
                {
                    totalSize += (2 * sizeof(float) + 2 * sizeof(byte));
                }

                if (frame.ClippingEdgesEnabled)
                {
                    totalSize += (1 * sizeof(byte));
                }
            }

            byte[] serializedData;

            using (var memStream = new MemoryStream(totalSize))
            {
                using (var w = new BinaryWriter(memStream))
                {
                    w.Write(frame.Version);
                    w.Write(frame.FloorClipPlaneX);
                    w.Write(frame.FloorClipPlaneY);
                    w.Write(frame.FloorClipPlaneZ);
                    w.Write(frame.FloorClipPlaneW);
                    
                    byte featureFlag = 0x0;
                    featureFlag |= (byte) (frame.ClippingEdgesEnabled ? 0x1 : 0x0);
                    featureFlag |= (byte) (frame.TrackHandsStatus ? 0x2 : 0x0);
                    featureFlag |= (byte) (frame.TrackJointRotation ? 0x4 : 0x0);
                    w.Write(featureFlag);

                    w.Write((byte)frame.Bodies.Count); // HERE WE LIMIT TO A MAX OF 256 BODIES!
                    
                    for (var i = 0; i < frame.Bodies.Count; i++)
                    {
                        w.Write(frame.Bodies[i].TrackingId);

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

                        w.Write((byte)frame.Bodies[i].Joints.Count); // HERE WE LIMIT TO A MAX OF 256 JOINTS!
                        
                        foreach (var jointData in frame.Bodies[i].Joints.Values)
                        {
                            w.Write((byte)jointData.JointType); // HERE WE LIMIT TO A MAX OF 256 JOINT TYPES!
                            w.Write((byte)jointData.TrackingState); // HERE WE LIMIT TO A MAX OF 256 TRACKING STATE TYPES!
                            w.Write(jointData.PositionX);
                            w.Write(jointData.PositionY);
                            w.Write(jointData.PositionZ);

                            if (frame.TrackJointRotation)
                            {
                                w.Write(jointData.OrientationX);
                                w.Write(jointData.OrientationY);
                                w.Write(jointData.OrientationZ);
                                w.Write(jointData.OrientationW);
                            }
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

        private SensorDataFrame DeserializeVersion2(byte[] serializedBytes)
        {
            if (serializedBytes == null)
            {
                throw new ArgumentNullException("serializedBytes");
            }

            var frame = new SensorDataFrame();

            using (var memStream = new MemoryStream())
            {
                using (var r = new BinaryReader(memStream))
                {
                    memStream.Write(serializedBytes, 0, serializedBytes.Length);
                    memStream.Seek(0, SeekOrigin.Begin);

                    var version = r.ReadByte();

                    frame.FloorClipPlaneX = r.ReadSingle();
                    frame.FloorClipPlaneY = r.ReadSingle();
                    frame.FloorClipPlaneZ = r.ReadSingle();
                    frame.FloorClipPlaneW = r.ReadSingle();

                    var featureFlag = r.ReadByte();
                    frame.ClippingEdgesEnabled = (featureFlag & 0x1) == 1;
                    frame.TrackHandsStatus = (featureFlag & 0x2) == 2;
                    frame.TrackJointRotation = (featureFlag & 0x4) == 4;
                    
                    var bodyCount = r.ReadByte(); // HERE WE LIMIT TO A MAX OF 256 BODIES!

                    for (byte i = 0; i < bodyCount; i++)
                    {
                        var bodyData = new SensorBodyData();

                        bodyData.TrackingId = r.ReadUInt64();

                        if (frame.ClippingEdgesEnabled)
                        {
                            bodyData.ClippedEdges = (SceneClippedEdges) r.ReadByte();
                        }

                        if (frame.TrackHandsStatus)
                        {
                            bodyData.LeftHand = new SensorHandData();
                            bodyData.LeftHand.Confidence = r.ReadSingle();
                            bodyData.LeftHand.State = (SensorHandState)r.ReadByte();

                            bodyData.RightHand = new SensorHandData();
                            bodyData.RightHand.Confidence = r.ReadSingle();
                            bodyData.RightHand.State = (SensorHandState) r.ReadByte();
                        }

                        frame.Bodies.Add(bodyData);

                        var jointsCount = r.ReadByte(); // HERE WE LIMIT TO A MAX OF 256 JOINTS!

                        for (byte j = 0; j < jointsCount; j++)
                        {
                            var jointData = new SensorBodyJointData();

                            jointData.JointType = (SensorBodyJointTypes)r.ReadByte(); // HERE WE LIMIT TO A MAX OF 256 JOINT TYPES!
                            jointData.TrackingState = (SensorTrackingState)r.ReadByte(); // HERE WE LIMIT TO A MAX OF 256 TRACKING STATE TYPES!
                            jointData.PositionX = r.ReadSingle();
                            jointData.PositionY = r.ReadSingle();
                            jointData.PositionZ = r.ReadSingle();

                            if (frame.TrackJointRotation)
                            {
                                jointData.OrientationX = r.ReadSingle();
                                jointData.OrientationY = r.ReadSingle();
                                jointData.OrientationZ = r.ReadSingle();
                                jointData.OrientationW = r.ReadSingle();
                            }

                            frame.Bodies[i].Joints.Add(jointData.JointType, jointData);
                        }
                    }
                }
            }

            return frame;
        } 
        #endregion
    }
}

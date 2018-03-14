namespace ImmotionAR.ImmotionRoom.DataSourceSensor.Kinect1
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using DataSourceService.Interfaces;
    using DataSourceService.Model;
    using Logger;
    using Protocol;

    // WARNING: VERY DIRTY CODE FOR SavedSession Replay
    // NOTE: Saved Session v1 are supported, but it is better to convert them to Session v2 for better performance
    //       You can convert them using the Command Line tool "RecordedSessionConverter"

    public class FakeKinect1DataSource : IDataSourceSensor
    {
        private const string SessionFileNameFormat = "{0}_{1}.ses";

        #region Events

        public event EventHandler<SensorStatusChangedEventArgs> SensorStatusChanged;
        public event EventHandler<DataSourceDataAvailableEventArgs> SkeletonDataAvailable;
        public event EventHandler<DataSourceImageDataAvailableEventArgs> ColorDataAvailable;
        public event EventHandler<DataSourceImageDataAvailableEventArgs> DepthDataAvailable;

        #endregion

        #region Private fields

        private readonly ILogger m_Logger;
        private bool m_Running;


        private SensorBodyData[] m_FakeBodies;

        private int m_SessionFiles;
        #endregion

        #region Properties
        public string SavedSessionId { get; set; }
        public string SavedSessionPath { get; set; }

        public bool SourceEnabled { get; set; }
        public bool SkeletonStreamEnabled { get; set; }
        public bool ColorStreamEnabled { get; set; }
        public bool DepthStreamEnabled { get; set; }

        #endregion

        #region Constructor

        public FakeKinect1DataSource()
        {
            m_Logger = LoggerService.GetLogger<FakeKinect1DataSource>();
        }

        #endregion

        #region Methods

        public bool Start(TrackingSessionConfiguration trackingSessionConfiguration)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Warn("FAKE KINECT IN USE");
            }

            if (!string.IsNullOrEmpty(SavedSessionPath) && !string.IsNullOrEmpty(SavedSessionId))
            {
                ReplayFakeSession(SavedSessionPath, SavedSessionId, 2);
                return true;
            }

            OnSensorStatusChanged(true);

            m_Running = true;

            // Prepare FAKE body data
            m_FakeBodies = new SensorBodyData[1];
            m_FakeBodies[0] = new SensorBodyData();
            m_FakeBodies[0].TrackingId = 42420;

            var jointTypes = (SensorBodyJointTypes[])Enum.GetValues(typeof(SensorBodyJointTypes));
            foreach (var jointType in jointTypes)
            {
                m_FakeBodies[0].Joints.Add(jointType, new SensorBodyJointData { JointType = jointType, TrackingState = SensorTrackingState.NotTracked });
            }

            //m_FakeBodies[1] = new SensorBodyData();
            //m_FakeBodies[1].TrackingId = 42421;
            //foreach (var jointType in jointTypes)
            //{
            //    m_FakeBodies[1].Joints.Add(jointType, new SensorBodyJointData() { JointType = jointType, TrackingState = TrackingState.NotTracked });
            //}

            //m_FakeBodies[2] = new SensorBodyData();
            //m_FakeBodies[2].TrackingId = 42422;
            //foreach (var jointType in jointTypes)
            //{
            //    m_FakeBodies[2].Joints.Add(jointType, new SensorBodyJointData() { JointType = jointType, TrackingState = TrackingState.NotTracked });
            //}

            Task.Run(async () =>
            {
                var r = new Random((int)DateTime.UtcNow.Ticks);

                var stayStill = false;
                var stillStart = DateTime.UtcNow;
                SensorBodyData stillBody = null;

                while (m_Running)
                {
                    var stayStillNext = r.Next(0, 50);

                    if (!stayStill && stayStillNext > 25)
                    {
                        stayStill = true;
                        stillStart = DateTime.UtcNow;
                    }

                    if (stayStill && (DateTime.UtcNow - stillStart).TotalSeconds > 5)
                    {
                        stayStill = false;
                    }

                    var sensorData = new SensorDataFrame
                    {
                        FloorClipPlaneX = (float)r.NextDouble(),
                        FloorClipPlaneY = (float)r.NextDouble(),
                        FloorClipPlaneZ = (float)r.NextDouble(),
                        FloorClipPlaneW = (float)r.NextDouble(),
                        ClippingEdgesEnabled = true,
                        TrackHandsStatus = false,
                        TrackJointRotation = true,
                    };

                    foreach (var b in m_FakeBodies)
                    {
                        // ALL FAKE BODY DATA ARE TRACKED
                        //if (!b.IsTracked)
                        //{
                        //    continue;
                        //}
                        SensorBodyData bodyData;

                        if (stayStill && stillBody != null)
                        {
                            bodyData = stillBody;
                        }
                        else
                        {
                            bodyData = new SensorBodyData
                            {
                                TrackingId = b.TrackingId,
                                ClippedEdges = SceneClippedEdges.None,
                            };

                            var nextInferredRandom = r.Next(0, 2);
                            bodyData.LeftHand = new SensorHandData()
                            {
                                Confidence = nextInferredRandom == 2 ? 1.0f : nextInferredRandom == 1 ? 0.1f : 0.0f,
                                State = nextInferredRandom == 2 ? SensorHandState.Open : nextInferredRandom == 1 ? SensorHandState.Closed : SensorHandState.NotTracked
                            };

                            nextInferredRandom = r.Next(0, 2);
                            bodyData.RightHand = new SensorHandData()
                            {
                                Confidence = nextInferredRandom == 2 ? 1.0f : nextInferredRandom == 1 ? 0.1f : 0.0f,
                                State = nextInferredRandom == 2 ? SensorHandState.Open : nextInferredRandom == 1 ? SensorHandState.Closed : SensorHandState.NotTracked
                            };

                            foreach (var joint in b.Joints)
                            {
                                nextInferredRandom = r.Next(0, 2);

                                var jointData = new SensorBodyJointData
                                {
                                    JointType = joint.Key,
                                    PositionX = (float)r.NextDouble(),
                                    PositionY = (float)r.NextDouble(),
                                    PositionZ = (float)r.NextDouble(),

                                    OrientationX = (float)r.NextDouble(),
                                    OrientationY = (float)r.NextDouble(),
                                    OrientationZ = (float)r.NextDouble(),
                                    OrientationW = (float)r.NextDouble(),

                                    TrackingState = nextInferredRandom == 2 ? SensorTrackingState.Tracked : nextInferredRandom == 1 ? SensorTrackingState.Inferred : SensorTrackingState.NotTracked
                                };

                                bodyData.Joints.Add(jointData.JointType, jointData);
                            }

                            stillBody = bodyData;
                        }

                        sensorData.Bodies.Add(bodyData);
                    }

                    OnSkeletonDataAvailable(sensorData);

                    await Task.Delay(33).ConfigureAwait(false);
                }
            });

            return true;
        }

        public bool Stop()
        {
            m_Running = false;
            return true;
        }

        #endregion

        #region Private methods

        private void OnSensorStatusChanged(bool isActive)
        {
            var localHandler = SensorStatusChanged;
            if (localHandler != null)
            {
                localHandler(this, new SensorStatusChangedEventArgs(isActive));
            }
        }

        private void OnSkeletonDataAvailable(SensorDataFrame data)
        {
            var localHandler = SkeletonDataAvailable;
            if (localHandler != null)
            {
                localHandler(this, new DataSourceDataAvailableEventArgs(data));
            }
        }

        private void OnColorDataAvailable(SensorVideoStreamFrame data)
        {
            var localHandler = ColorDataAvailable;
            if (localHandler != null)
            {
                localHandler(this, new DataSourceImageDataAvailableEventArgs(data));
            }
        }

        private void OnDepthDataAvailable(SensorVideoStreamFrame data)
        {
            var localHandler = DepthDataAvailable;
            if (localHandler != null)
            {
                localHandler(this, new DataSourceImageDataAvailableEventArgs(data));
            }
        }


        private void ReplayFakeSession(string path, string sessionId, int version)
        {
            var serializer = new SensorDataFrameSerializer();
            m_SessionFiles = 1;

            Task.Run(async () =>
            {
                OnSensorStatusChanged(true);
                m_Running = true;

                var sessionFileName = string.Format(SessionFileNameFormat, sessionId, m_SessionFiles);
                sessionFileName = Path.Combine(path, sessionFileName);

                bool changeFile = false;
                BinaryReader binaryReader = new BinaryReader(File.Open(sessionFileName, FileMode.Open, FileAccess.Read));

                while (m_Running)
                {
                    while (!changeFile)
                    {
                        try
                        {
                            byte[] sensorData = null;
                            int dataLen;

                            if (version == 1)
                            {
                                var startFrameBytes = binaryReader.ReadBytes(2 * sizeof(byte) + 4 * sizeof(float));

                                if (startFrameBytes.Length > 0)
                                {
                                    if (startFrameBytes[0] != version)
                                    {
                                        throw new InvalidDataException("Version mismatch");
                                    }

                                    var bodyCount = startFrameBytes[4 * sizeof(float) + 1];
                                    dataLen = 2 * sizeof(byte) + 4 * sizeof(float) + (sizeof(ulong) + sizeof(byte) + (2 * sizeof(byte) + 3 * sizeof(float)) * 25) * bodyCount;
                                    binaryReader.BaseStream.Seek(-(2 * sizeof(byte) + 4 * sizeof(float)), SeekOrigin.Current);
                                    sensorData = binaryReader.ReadBytes(dataLen);
                                }
                                else
                                {
                                    // Change file, try to move to the next file of the session (if any)
                                    sessionFileName = string.Format(SessionFileNameFormat, sessionId, m_SessionFiles + 1);
                                    sessionFileName = Path.Combine(path, sessionFileName);
                                    if (!File.Exists(sessionFileName))
                                    {
                                        // All files for the specified session processed, loop from beginning
                                        m_SessionFiles = 0;
                                    }
                                    changeFile = true;
                                }
                            }
                            else if (version == 2)
                            {
                                try
                                {
                                    dataLen = binaryReader.ReadInt32();
                                    sensorData = binaryReader.ReadBytes(dataLen);
                                    if (sensorData.Length == 0)
                                    {
                                        // Change file, try to move to the next file of the session (if any)
                                        changeFile = true;
                                    }
                                }
                                catch (EndOfStreamException)
                                {
                                    // Change file, try to move to the next file of the session (if any)
                                    changeFile = true;
                                }

                                if (changeFile)
                                {
                                    sessionFileName = string.Format(SessionFileNameFormat, sessionId, m_SessionFiles + 1);
                                    sessionFileName = Path.Combine(path, sessionFileName);
                                    if (!File.Exists(sessionFileName))
                                    {
                                        // All files for the specified session processed, loop from beginning
                                        m_SessionFiles = 0;
                                    }
                                }
                            }

                            if (changeFile)
                            {
                                break;
                            }

                            OnSkeletonDataAvailable(serializer.Deserialize(sensorData));

                            await Task.Delay(33).ConfigureAwait(false);
                        }
                        catch (InvalidOperationException)
                        {
                            // Ignore bad frame
                        }
                    }

                    binaryReader.Close();

                    m_SessionFiles++;
                    sessionFileName = string.Format(SessionFileNameFormat, sessionId, m_SessionFiles);
                    sessionFileName = Path.Combine(path, sessionFileName);
                    if (!File.Exists(sessionFileName))
                    {
                        m_SessionFiles = 1;
                        sessionFileName = string.Format(SessionFileNameFormat, sessionId, m_SessionFiles);
                        sessionFileName = Path.Combine(path, sessionFileName);
                    }

                    binaryReader = new BinaryReader(File.Open(sessionFileName, FileMode.Open, FileAccess.Read));
                    changeFile = false;
                }
            });
        }


        #endregion
    }
}

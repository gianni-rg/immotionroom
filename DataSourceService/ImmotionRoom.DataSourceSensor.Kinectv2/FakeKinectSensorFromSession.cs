// -----------------------------------------------------------------------
// <copyright file="FakeKinectSensor.cs" company="ImmotionAR">
// Copyright (C) 2015 ImmotionAR. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace ImmotionAR.ImmotionRoom.DataSourceSensor.Kinect2
{
    using System;
    using System.Threading.Tasks;
    using DataSourceService.Interfaces;
    using DataSourceService.Model;
    using Logger;
    using Protocol;

    public class FakeKinectSensorFromSession : IDataSourceSensor
    {
        #region Events

        public event EventHandler<SensorStatusChangedEventArgs> SensorStatusChanged;
        public event EventHandler<DataSourceDataAvailableEventArgs> DataAvailable;

        #endregion

        #region Private fields

        protected readonly ILogger m_Logger;
        private bool m_Running;

        private SensorBodyData[] m_FakeBodies;

        #endregion

        #region Properties

        public bool SourceEnabled { get; set; }

        #endregion

        #region Constructor

        public FakeKinectSensorFromSession()
        {
            m_Logger = LoggerService.GetLogger<FakeKinectSensorFromSession>();
        }

        #endregion

        #region Methods

        public bool IsActive { get; private set; }
        public string SessionFile { get; set; }

        public bool Start()
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Warn("FAKE KINECT IN USE - Session: {0}", SessionFile);
            }

            OnSensorStatusChanged(true);

            m_Running = true;

            Task.Run(async () =>
            {
                while (m_Running)
                {
                    int stayStillNext = r.Next(0, 50);

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
                        FloorClipPlaneX = (float) r.NextDouble(),
                        FloorClipPlaneY = (float) r.NextDouble(),
                        FloorClipPlaneZ = (float) r.NextDouble(),
                        FloorClipPlaneW = (float) r.NextDouble(),
                    };

                    foreach (SensorBodyData b in m_FakeBodies)
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
                            };

                            foreach (var joint in b.Joints)
                            {
                                int nextInferredRandom = r.Next(0, 2);

                                var jointData = new SensorBodyJointData
                                {
                                    JointType = joint.Key,
                                    PositionX = (float) r.NextDouble(),
                                    PositionY = (float) r.NextDouble(),
                                    PositionZ = (float) r.NextDouble(),

                                    // AT THE MOMENT WE ARE NOT USING Orientation INFO
                                    //OrientationX = b.JointOrientations[joint.Key].Orientation.X,
                                    //OrientationY = b.JointOrientations[joint.Key].Orientation.Y,
                                    //OrientationZ = b.JointOrientations[joint.Key].Orientation.Z,
                                    //OrientationW = b.JointOrientations[joint.Key].Orientation.W,

                                    TrackingState = nextInferredRandom == 2 ? TrackingState.Tracked : nextInferredRandom == 1 ? TrackingState.Inferred : TrackingState.NotTracked,
                                };

                                bodyData.Joints.Add(jointData.JointType, jointData);
                            }

                            stillBody = bodyData;
                        }

                        sensorData.Bodies.Add(bodyData);
                    }

                    OnDataAvailable(sensorData);

                    await Task.Delay(33);
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
            IsActive = isActive;

            EventHandler<SensorStatusChangedEventArgs> localHandler = SensorStatusChanged;
            if (localHandler != null)
            {
                localHandler(this, new SensorStatusChangedEventArgs(isActive));
            }
        }

        private void OnDataAvailable(SensorDataFrame data)
        {
            EventHandler<DataSourceDataAvailableEventArgs> localHandler = DataAvailable;
            if (localHandler != null)
            {
                localHandler(this, new DataSourceDataAvailableEventArgs(data));
            }
        }

        #endregion
    }
}

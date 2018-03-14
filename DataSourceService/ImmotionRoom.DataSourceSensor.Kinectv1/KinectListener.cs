namespace ImmotionAR.ImmotionRoom.DataSourceSensor.Kinect1
{
    using DataSourceService.Model;
    using Logger;
    using Microsoft.Kinect;

    // Inspired to KinectListener in Coding4Fun.Kinect.KinectService project
    // Copyright (C) Microsoft Corporation.
    // This source is subject to the Microsoft Public License (Ms-PL).
    // Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
    // All other rights reserved.

    internal abstract class KinectListener
    {
        protected static readonly object LockObj = new object();
        protected readonly ILogger m_Logger;

        internal KinectSensor Kinect { get; set; }
        internal bool SourceEnabled { get; set; }

        protected KinectListener(ILogger logger)
        {
            m_Logger = logger;
        }

        internal abstract bool Start(TrackingSessionConfiguration trackingConfiguration);

        internal abstract bool Stop();
    }
}

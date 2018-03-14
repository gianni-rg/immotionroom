namespace ImmotionAR.ImmotionRoom.DataSourceSensor.Kinect1
{
    using System;
    using DataSourceService.Model;
    using Logger;
    using Microsoft.Kinect;
    using Protocol;

    internal class MultiStreamListener : KinectListener
    {
        #region Events

        internal event EventHandler<DataSourceDataAvailableEventArgs> SkeletonDataAvailable;
        internal event EventHandler<DataSourceImageDataAvailableEventArgs> ColorDataAvailable;
        internal event EventHandler<DataSourceImageDataAvailableEventArgs> DepthDataAvailable;

        #endregion

        #region Private fields

        private static readonly object RawDataLockDepth = new object();
        private static readonly object RawDataLockSkeleton = new object();
        private static readonly object RawDataLockColor = new object();
        private SkeletonDataProcessor m_SkeletonDataProcessor;
        private ColorStreamProcessor m_ColorStreamProcessor;
        private DepthStreamProcessor m_DepthStreamProcessor;

        #endregion

        #region Constructor

        internal MultiStreamListener() : base(LoggerService.GetLogger<MultiStreamListener>())
        {
        }

        internal bool SkeletonStreamEnabled { get; set; }
        internal bool ColorStreamEnabled { get; set; }
        internal bool DepthStreamEnabled { get; set; }

        #endregion

        #region Methods

        internal override bool Start(TrackingSessionConfiguration trackingConfiguration)
        {
            // Open the reader for the enabled data streams

            if (SkeletonStreamEnabled)
            {
                //var skeletonStreamParams = new TransformSmoothParameters() { };
                Kinect.SkeletonStream.Enable(/* skeletonStreamParams */);
                Kinect.SkeletonFrameReady += Sensor_SkeletonFrameReady;
                m_SkeletonDataProcessor = new SkeletonDataProcessor(trackingConfiguration);
                m_SkeletonDataProcessor.DataAvailable += SkeletonDataProcessor_DataAvailable;
                m_SkeletonDataProcessor.Start();
            }

            if (ColorStreamEnabled)
            {
                Kinect.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                Kinect.ColorFrameReady += Sensor_ColorFrameReady;
                m_ColorStreamProcessor = new ColorStreamProcessor(trackingConfiguration);
                m_ColorStreamProcessor.DataAvailable += ColorStreamProcessor_DataAvailable;
                m_ColorStreamProcessor.Start();
            }

            if (DepthStreamEnabled)
            {
                Kinect.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                Kinect.DepthFrameReady += Sensor_DepthFrameReady;
                m_DepthStreamProcessor = new DepthStreamProcessor(trackingConfiguration);
                m_DepthStreamProcessor.DataAvailable += DepthStreamProcessor_DataAvailable;
                m_DepthStreamProcessor.Start();
            }

            return true;
        }

        internal override bool Stop()
        {
            if (m_SkeletonDataProcessor != null)
            {
                m_SkeletonDataProcessor.DataAvailable -= SkeletonDataProcessor_DataAvailable;
                m_SkeletonDataProcessor.Stop();
            }

            if (m_ColorStreamProcessor != null)
            {
                m_ColorStreamProcessor.DataAvailable -= ColorStreamProcessor_DataAvailable;
                m_ColorStreamProcessor.Stop();
            }

            if (m_DepthStreamProcessor != null)
            {
                m_DepthStreamProcessor.DataAvailable -= DepthStreamProcessor_DataAvailable;
                m_DepthStreamProcessor.Stop();
            }

            if (Kinect != null)
            {
                Kinect.SkeletonStream.Disable();
                Kinect.SkeletonFrameReady -= Sensor_SkeletonFrameReady;

                Kinect.ColorStream.Disable();
                Kinect.ColorFrameReady -= Sensor_ColorFrameReady;

                Kinect.DepthStream.Disable();
                Kinect.DepthFrameReady -= Sensor_DepthFrameReady;
            }

            return true;
        }

        #endregion

        #region Event Handlers

        private void Sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            var validDepth = false;

            try
            {
                lock (RawDataLockDepth)
                {
                    using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
                    {
                        if (depthFrame != null)
                        {
                            m_DepthStreamProcessor.SetData(depthFrame);
                            validDepth = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ignore if the frame is no longer available
            }

            if (validDepth)
            {
                m_DepthStreamProcessor.DataReady();
            }
        }

        private void Sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            var validColor = false;

            try
            {
                lock (RawDataLockColor)
                {
                    using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
                    {
                        if (colorFrame != null)
                        {
                            m_ColorStreamProcessor.SetData(colorFrame);
                            validColor = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ignore if the frame is no longer available
            }

            if (validColor)
            {
                m_ColorStreamProcessor.DataReady();
            }
        }

        private void Sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            var validSkeleton = false;

            try
            {
                lock (RawDataLockSkeleton)
                {
                    using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
                    {
                        if (skeletonFrame != null)
                        {
                            m_SkeletonDataProcessor.SetData(skeletonFrame);
                            validSkeleton = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ignore if the frame is no longer available
            }

            if (validSkeleton)
            {
                m_SkeletonDataProcessor.DataReady();
            }
        }

        private void SkeletonDataProcessor_DataAvailable(object sender, DataSourceDataAvailableEventArgs e)
        {
            OnSkeletonFrameAvailable(e.Data);
        }

        private void ColorStreamProcessor_DataAvailable(object sender, DataSourceImageDataAvailableEventArgs e)
        {
            OnColorFrameAvailable(e.Data);
        }

        private void DepthStreamProcessor_DataAvailable(object sender, DataSourceImageDataAvailableEventArgs e)
        {
            OnDepthFrameAvailable(e.Data);
        }

        #endregion

        #region Private methods

        private void OnSkeletonFrameAvailable(SensorDataFrame data)
        {
            var localHandler = SkeletonDataAvailable;
            if (localHandler != null)
            {
                localHandler(this, new DataSourceDataAvailableEventArgs(data));
            }
        }

        private void OnColorFrameAvailable(SensorVideoStreamFrame data)
        {
            var localHandler = ColorDataAvailable;
            if (localHandler != null)
            {
                localHandler(this, new DataSourceImageDataAvailableEventArgs(data));
            }
        }

        private void OnDepthFrameAvailable(SensorVideoStreamFrame data)
        {
            var localHandler = DepthDataAvailable;
            if (localHandler != null)
            {
                localHandler(this, new DataSourceImageDataAvailableEventArgs(data));
            }
        }
        #endregion
    }
}

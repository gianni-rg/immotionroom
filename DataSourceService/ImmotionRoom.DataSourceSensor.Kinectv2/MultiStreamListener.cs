namespace ImmotionAR.ImmotionRoom.DataSourceSensor.Kinect2
{
    using System;
    using DataSourceService.Model;
    using Logger;
    using Microsoft.Kinect;
    using Protocol;

    // Partially inspired to xxxListener in Coding4Fun.Kinect.KinectService project and KinectFusionExplorer-WPF Sample.
    // Copyright (C) Microsoft Corporation.
    // This source is subject to the Microsoft Public License (Ms-PL).
    // Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
    // All other rights reserved.

    internal class MultiStreamListener : KinectListener
    {
        #region Events

        internal event EventHandler<DataSourceDataAvailableEventArgs> SkeletonDataAvailable;
        internal event EventHandler<DataSourceImageDataAvailableEventArgs> ColorDataAvailable;
        internal event EventHandler<DataSourceImageDataAvailableEventArgs> DepthDataAvailable;

        #endregion

        #region Private fields

        private static readonly object RawDataLock = new object();
        private MultiSourceFrameReader m_MultiFrameReader;
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
            // Get the Kinect Sensor object
            Kinect = KinectSensor.GetDefault();

            if (Kinect == null)
            {
                if (m_Logger.IsErrorEnabled)
                {
                    m_Logger.Error("Kinect sensor not found");
                }

                return false;
            }

            Kinect.Open();

            // Open the reader for the enabled data streams
            var enabledTypes = FrameSourceTypes.None;

            if (SkeletonStreamEnabled)
            {
                enabledTypes |= FrameSourceTypes.Body;
                m_SkeletonDataProcessor = new SkeletonDataProcessor(trackingConfiguration);
                m_SkeletonDataProcessor.DataAvailable += SkeletonDataProcessor_DataAvailable;
                m_SkeletonDataProcessor.Start();
            }

            if (ColorStreamEnabled)
            {
                enabledTypes |= FrameSourceTypes.Color;
                m_ColorStreamProcessor = new ColorStreamProcessor(trackingConfiguration);
                m_ColorStreamProcessor.DataAvailable += ColorStreamProcessor_DataAvailable;
                m_ColorStreamProcessor.Start();
            }

            if (DepthStreamEnabled)
            {
                enabledTypes |= FrameSourceTypes.Depth;
                m_DepthStreamProcessor = new DepthStreamProcessor(trackingConfiguration);
                m_DepthStreamProcessor.DataAvailable += DepthStreamProcessor_DataAvailable;
                m_DepthStreamProcessor.Start();
            }

            m_MultiFrameReader = Kinect.OpenMultiSourceFrameReader(enabledTypes);

            if (m_MultiFrameReader != null)
            {
                m_MultiFrameReader.MultiSourceFrameArrived += Kinect_MultiSourceFrameReady;
            }

            return true;
        }

        internal override bool Stop()
        {
            if (m_MultiFrameReader != null)
            {
                m_MultiFrameReader.MultiSourceFrameArrived -= Kinect_MultiSourceFrameReady;
                m_MultiFrameReader.Dispose();
                m_MultiFrameReader = null;
            }

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
                Kinect.Close();
            }

            return true;
        }

        #endregion

        #region Event Handlers

        private void Kinect_MultiSourceFrameReady(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var validSkeleton = false;
            var validDepth = false;
            var validColor = false;

            var frameReference = e.FrameReference;

            MultiSourceFrame multiSourceFrame;
            BodyFrame bodyFrame = null;
            DepthFrame depthFrame = null;
            ColorFrame colorFrame = null;

            try
            {
                multiSourceFrame = frameReference.AcquireFrame();

                if (multiSourceFrame != null)
                {
                    lock (RawDataLock)
                    {
                        var bodyFrameReference = multiSourceFrame.BodyFrameReference;
                        var colorFrameReference = multiSourceFrame.ColorFrameReference;
                        var depthFrameReference = multiSourceFrame.DepthFrameReference;

                        bodyFrame = bodyFrameReference.AcquireFrame();
                        colorFrame = colorFrameReference.AcquireFrame();
                        depthFrame = depthFrameReference.AcquireFrame();

                        if (depthFrame != null)
                        {
                            m_DepthStreamProcessor.SetData(depthFrame);
                            validDepth = true;
                        }

                        if (colorFrame != null)
                        {
                            m_ColorStreamProcessor.SetData(colorFrame);
                            validColor = true;
                        }

                        if (bodyFrame != null)
                        {
                            m_SkeletonDataProcessor.SetData(bodyFrame);
                            validSkeleton = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ignore if the frame is no longer available
            }
            finally
            {
                // DepthFrame, ColorFrame, BodyFrame are IDispoable
                if (bodyFrame != null)
                {
                    bodyFrame.Dispose();
                    bodyFrame = null;
                }

                if (depthFrame != null)
                {
                    depthFrame.Dispose();
                    depthFrame = null;
                }

                if (colorFrame != null)
                {
                    colorFrame.Dispose();
                    colorFrame = null;
                }

                multiSourceFrame = null;
            }

            if (validSkeleton)
            {
                m_SkeletonDataProcessor.DataReady();
            }

            if (validDepth)
            {
                m_DepthStreamProcessor.DataReady();
            }

            if (validColor)
            {
                m_ColorStreamProcessor.DataReady();
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

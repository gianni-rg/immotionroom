namespace ImmotionAR.ImmotionRoom.DataSourceSensor.Kinect2
{
    using System;
    using DataSourceService.Interfaces;
    using DataSourceService.Model;
    using Logger;
    using Microsoft.Kinect;

    // WARNING!!!! Kinect v2 IS NOT SUPPORTED TO RUN IN A Windows Service  !!!
    // See: https://social.msdn.microsoft.com/Forums/en-US/5342afba-5b18-406e-819e-ca8d6cdaef6c/access-kinect-v2-from-a-windows-service?forum=kinectv2sdk

    public class Kinect2DataSource : IDataSourceSensor
    {
        #region Events

        public event EventHandler<SensorStatusChangedEventArgs> SensorStatusChanged;
        public event EventHandler<DataSourceDataAvailableEventArgs> SkeletonDataAvailable;
        public event EventHandler<DataSourceImageDataAvailableEventArgs> ColorDataAvailable;
        public event EventHandler<DataSourceImageDataAvailableEventArgs> DepthDataAvailable;

        #endregion

        #region Private fields

        private static readonly object LockObj = new object();
        private readonly ILogger m_Logger;
        private readonly MultiStreamListener m_MultiStreamListener;

        private KinectSensor m_Kinect;
        private bool m_SourceEnabled;
        private bool m_IsActive;

        #endregion

        #region Properties

        public bool SourceEnabled
        {
            get { return m_SourceEnabled; }

            set
            {
                m_SourceEnabled = value;
                m_MultiStreamListener.SourceEnabled = value;
            }
        }

        public bool SkeletonStreamEnabled
        {
            get { return m_MultiStreamListener.SkeletonStreamEnabled; }

            set { m_MultiStreamListener.SkeletonStreamEnabled = value; }
        }

        public bool ColorStreamEnabled
        {
            get { return m_MultiStreamListener.ColorStreamEnabled; }

            set { m_MultiStreamListener.ColorStreamEnabled = value; }
        }

        public bool DepthStreamEnabled
        {
            get { return m_MultiStreamListener.DepthStreamEnabled; }

            set { m_MultiStreamListener.DepthStreamEnabled = value; }
        }

        #endregion

        #region Constructor

        public Kinect2DataSource()
        {
            m_Logger = LoggerService.GetLogger<Kinect2DataSource>();
            m_MultiStreamListener = new MultiStreamListener();
        }

        #endregion

        #region Methods

        public bool Start(TrackingSessionConfiguration trackingSessionConfiguration)
        {
            // Get the Kinect Sensor object
            m_Kinect = KinectSensor.GetDefault();

            if (m_Kinect == null)
            {
                if (m_Logger.IsErrorEnabled)
                {
                    m_Logger.Error("Kinect sensor not found");
                }

                OnSensorStatusChanged(false);
                return false;
            }

            m_Kinect.IsAvailableChanged += Sensor_IsAvailableChanged;

            if (SkeletonStreamEnabled)
            {
                m_MultiStreamListener.SkeletonDataAvailable += SkeletonStreamListener_DataAvailable;
                m_MultiStreamListener.SkeletonStreamEnabled = true;
            }

            if (ColorStreamEnabled)
            {
                m_MultiStreamListener.ColorDataAvailable += ColorStreamListener_DataAvailable;
                m_MultiStreamListener.ColorStreamEnabled = true;
            }

            if (DepthStreamEnabled)
            {
                m_MultiStreamListener.DepthDataAvailable += DepthStreamListener_DataAvailable;
                m_MultiStreamListener.DepthStreamEnabled = true;
            }

            m_MultiStreamListener.Start(trackingSessionConfiguration);

            return true;
        }

        public bool Stop()
        {
            lock (LockObj)
            {
                m_IsActive = false;
            }
            
            if (m_MultiStreamListener != null)
            {
                m_MultiStreamListener.Stop();

                m_MultiStreamListener.SkeletonDataAvailable -= SkeletonStreamListener_DataAvailable;
                m_MultiStreamListener.SkeletonStreamEnabled = false;
            
                m_MultiStreamListener.ColorDataAvailable -= ColorStreamListener_DataAvailable;
                m_MultiStreamListener.ColorStreamEnabled = false;
            
                m_MultiStreamListener.DepthDataAvailable -= DepthStreamListener_DataAvailable;
                m_MultiStreamListener.DepthStreamEnabled = false;
            }

            if (m_Kinect != null)
            {
                lock (LockObj)
                {
                    if (m_Kinect != null)
                    {
                        m_Kinect.IsAvailableChanged -= Sensor_IsAvailableChanged;
                        m_Kinect.Close();
                        m_Kinect = null;
                    }
                }
            }

            return true;
        }

        #endregion

        #region Private methods

        private void ColorStreamListener_DataAvailable(object sender, DataSourceImageDataAvailableEventArgs e)
        {
            OnColorDataAvailable(e);
        }

        private void SkeletonStreamListener_DataAvailable(object sender, DataSourceDataAvailableEventArgs e)
        {
            OnSkeletonDataAvailable(e);
        }

        private void DepthStreamListener_DataAvailable(object sender, DataSourceImageDataAvailableEventArgs e)
        {
            OnDepthDataAvailable(e);
        }

        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            OnSensorStatusChanged(e.IsAvailable);
        }

        private void OnSkeletonDataAvailable(DataSourceDataAvailableEventArgs args)
        {
            var localHandler = SkeletonDataAvailable;
            if (localHandler != null)
            {
                localHandler(this, args);
            }
        }

        private void OnColorDataAvailable(DataSourceImageDataAvailableEventArgs args)
        {
            var localHandler = ColorDataAvailable;
            if (localHandler != null)
            {
                localHandler(this, args);
            }
        }

        private void OnDepthDataAvailable(DataSourceImageDataAvailableEventArgs args)
        {
            var localHandler = DepthDataAvailable;
            if (localHandler != null)
            {
                localHandler(this, args);
            }
        }

        private void OnSensorStatusChanged(bool isActive)
        {
            if (m_IsActive == isActive)
            {
                // Do not report a status change if status has not changed
                return;
            }

            lock (LockObj)
            {
                m_IsActive = isActive;
            }

            if (isActive)
            {
                if (m_Logger.IsInfoEnabled)
                {
                    m_Logger.Info("Kinect sensor connected");
                }
            }
            else
            {
                if (m_Logger.IsErrorEnabled)
                {
                    m_Logger.Error("Kinect sensor not available");
                }
            }

            var localHandler = SensorStatusChanged;
            if (localHandler != null)
            {
                localHandler(this, new SensorStatusChangedEventArgs(isActive));
            }
        }

        #endregion
    }
}

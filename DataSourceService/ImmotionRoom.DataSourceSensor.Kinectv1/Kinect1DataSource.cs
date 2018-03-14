namespace ImmotionAR.ImmotionRoom.DataSourceSensor.Kinect1
{
    using System;
    using DataSourceService.Interfaces;
    using DataSourceService.Model;
    using Logger;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit;

    public class Kinect1DataSource : IDataSourceSensor
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

        private KinectSensorChooser m_SensorMonitor;
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

        public Kinect1DataSource()
        {
            m_Logger = LoggerService.GetLogger<Kinect1DataSource>();
            m_MultiStreamListener = new MultiStreamListener();
        }

        #endregion

        #region Methods

        public bool Start(TrackingSessionConfiguration trackingSessionConfiguration)
        {
            // Get the Kinect Sensor object
            m_SensorMonitor = new KinectSensorChooser();
            m_SensorMonitor.Start();
            
            m_Kinect = m_SensorMonitor.Kinect;

            if (m_Kinect == null || m_SensorMonitor.Status != ChooserStatus.SensorStarted)
            {
                if (m_Logger.IsErrorEnabled)
                {
                    m_Logger.Error("Kinect sensor not found or not ready");
                }

                OnSensorStatusChanged(false);
                return false;
            }

            m_SensorMonitor.PropertyChanged += Sensor_PropertyChanged;
            m_SensorMonitor.KinectChanged += Sensor_Changed;

            m_MultiStreamListener.Kinect = m_Kinect;

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

            OnSensorStatusChanged(true);

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

            if (m_SensorMonitor != null)
            {
                lock (LockObj)
                {
                    if (m_SensorMonitor != null)
                    {
                        m_SensorMonitor.KinectChanged -= Sensor_Changed;
                        m_SensorMonitor.PropertyChanged -= Sensor_PropertyChanged;
                        m_SensorMonitor.Stop();
                        m_MultiStreamListener.Kinect = null;
                        m_Kinect = null;
                        m_SensorMonitor = null;
                    }
                }
            }
            
            OnSensorStatusChanged(false);

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

        private void Sensor_Changed(object sender, KinectChangedEventArgs e)
        {
            m_Kinect = e.NewSensor;
            m_MultiStreamListener.Kinect = m_Kinect;
            OnSensorStatusChanged(m_Kinect != null && m_Kinect.Status == KinectStatus.Connected);
        }
        
        private void Sensor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Status")
            {
                OnSensorStatusChanged(m_SensorMonitor != null && m_SensorMonitor.Status == ChooserStatus.SensorStarted);
            }
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

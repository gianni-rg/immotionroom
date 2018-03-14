namespace ImmotionAR.ImmotionRoom.DataSourceSensor.Kinect1
{
    using System;
    using System.Threading;
    using DataSourceService.Model;
    using Logger;

    internal abstract class BaseStreamProcessor<T, TRaw, TEventArgs>
    {
        #region Events

        internal event EventHandler<TEventArgs> DataAvailable;

        #endregion

        #region Private fields

        protected readonly ILogger m_Logger;
        protected readonly TrackingSessionConfiguration m_TrackingConfiguration;
        private ManualResetEvent m_DataReadyEvent;
        private ManualResetEvent m_WorkerThreadStopEvent;
        private Thread m_WorkerThread;

        #endregion

        #region Properties

        internal T Data { get; set; }

        #endregion

        #region Constructor

        internal BaseStreamProcessor(ILogger logger, TrackingSessionConfiguration trackingConfiguration)
        {
            m_Logger = logger;
            m_TrackingConfiguration = trackingConfiguration;
        }

        #endregion

        #region Methods

        internal void Start()
        {
            if (m_WorkerThread != null)
            {
                return;
            }

            // Initialize events
            m_DataReadyEvent = new ManualResetEvent(false);
            m_WorkerThreadStopEvent = new ManualResetEvent(false);

            // Create worker thread and start
            m_WorkerThread = new Thread(WorkerThreadProc);
            m_WorkerThread.Start();
        }

        internal void Stop()
        {
            if (m_WorkerThread == null)
            {
                return;
            }

            // Set stop event to stop thread
            if (m_WorkerThreadStopEvent != null)
            {
                m_WorkerThreadStopEvent.Set();
            }

            // Wait for exit of thread
            m_WorkerThread.Join();

            if (m_WorkerThreadStopEvent != null)
            {
                m_WorkerThreadStopEvent.Dispose();
                m_WorkerThreadStopEvent = null;
            }

            if (m_DataReadyEvent != null)
            {
                m_DataReadyEvent.Dispose();
            }
        }

        internal void DataReady()
        {
            if (m_DataReadyEvent == null)
            {
                return;
            }

            if (m_DataReadyEvent.SafeWaitHandle.IsClosed || m_DataReadyEvent.SafeWaitHandle.IsInvalid)
            {
                return;
            }

            m_DataReadyEvent.Set();
        }

        internal abstract void SetData(TRaw data);

        #endregion

        #region Private methods

        protected abstract void ProcessData();

        /// <summary>
        ///     Worker thread in which data is processed
        /// </summary>
        private void WorkerThreadProc()
        {
            var events = new WaitHandle[] { m_WorkerThreadStopEvent, m_DataReadyEvent };

            while (true)
            {
                var index = WaitHandle.WaitAny(events);

                if (0 == index)
                {
                    // Stop event has been set. Exit thread
                    break;
                }

                // Reset data ready event
                m_DataReadyEvent.Reset();

                // Pass data to process
                ProcessData();
            }
        }

        protected void OnDataAvailable(TEventArgs args)
        {
            var localHandler = DataAvailable;
            if (localHandler != null)
            {
                localHandler(this, args);
            }
        }

        #endregion
    }
}

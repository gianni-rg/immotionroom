namespace ImmotionAR.ImmotionRoom.Recording
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Threading.Tasks;
    using Protocol;
    using Emgu.CV;
    using Emgu.CV.CvEnum;
    using Interfaces;
    using Logger;
    using Model;

    // See: http://www.codeproject.com/Articles/722569/Video-Capture-using-OpenCV-with-Csharp
    // See: http://pterneas.com/2014/02/20/kinect-for-windows-version-2-color-depth-and-infrared-streams/

    public class VideoRecorder : IVideoRecorder
    {
        private const string DefaultRecordingSessionFolder = "RecordedSessions";
        private const string SessionFileNameFormat = "{0}_{1}_{2}.avi";
        private const string SessionFileNameTimestampFormat = "yyyyMMdd_HHmmss";

        // TODO: move these settings somewhere better...
        //private const int MaxSessionFileSizeInMBytes = 1950; // AVI Limit !
        private const int DefaultCapturedVideoFps = 30;
        private const int DefaultCapturedVideoWidth = 640;
        private const int DefaultCapturedVideoHeight = 480;

        #region Private fields

        private static readonly object LockObj = new object();
        private readonly ILogger m_Logger;

        private bool m_IsRecording;        
        private int m_SessionFiles;

        private VideoWriter m_SessionFileWriter;
        private string m_SessionId;
        
        private long m_LastTick;
        private int m_LastFrameRate;
        private int m_FrameRate;
        private bool m_ShowCaptureFps;
        private int m_ShowCaptureFpsTimes;
        private int m_MeanCaptureFrameRate;

        #endregion

        #region Properties
        public bool IsRecording
        {
            get { return m_IsRecording; }
        }

        public int CapturedVideoFps { get; set; }
        public int CapturedVideoWidth { get; set; }
        public int CapturedVideoHeight { get; set; }

        public string DataRecorderSessionPath { get; set; }
        public StreamType StreamType { get; set; }
        #endregion

        #region Constructor

        public VideoRecorder()
        {
            m_Logger = LoggerService.GetLogger<VideoRecorder>();

            CapturedVideoFps = DefaultCapturedVideoFps;
            CapturedVideoWidth = DefaultCapturedVideoWidth;
            CapturedVideoHeight = DefaultCapturedVideoHeight;
            DataRecorderSessionPath = DefaultRecordingSessionFolder;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Start Video Recording Session
        /// </summary>
        /// <param name="sessionId">If Skeleton Data Recording is enabled, use the same sessionId</param>
        public Task<string> StartRecordingAsync(string sessionId = null)
        {
            if (m_IsRecording)
            {
                // Already recording. Do nothing.
                return Task.FromResult(m_SessionId);
            }

            if (string.IsNullOrEmpty(DataRecorderSessionPath))
            {
                DataRecorderSessionPath = DefaultRecordingSessionFolder;
            }

            if (!Directory.Exists(DataRecorderSessionPath))
            {
                Directory.CreateDirectory(DataRecorderSessionPath);
            }

            m_ShowCaptureFps = true;
            m_ShowCaptureFpsTimes = 0;
            m_MeanCaptureFrameRate = 0;

            // Prepare a new session file in the format "yyyyMMdd_HHmmss_<counter>.txt"
            // Each file has a max size
            m_SessionFiles = 1;
            m_SessionId = sessionId;
            if (string.IsNullOrEmpty(m_SessionId))
            {
                m_SessionId = DateTime.UtcNow.ToString(SessionFileNameTimestampFormat);
            }
            
            string sessionFileName = Path.Combine(DataRecorderSessionPath, string.Format(SessionFileNameFormat, m_SessionId, StreamType, m_SessionFiles));

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("[{0}] New Video Recording Session Started", m_SessionId);
            }

            // Create the writer for data
            int codec = VideoWriter.Fourcc('X', 'V', 'I', 'D');
            m_SessionFileWriter = new VideoWriter(sessionFileName, codec, CapturedVideoFps, new Size(CapturedVideoWidth, CapturedVideoHeight), true);

            lock (LockObj)
            {
                m_IsRecording = true;
            }

            return Task.FromResult(m_SessionId);
        }

        /// <summary>
        ///     Stop Video Recording Session
        /// </summary>
        public string StopRecording()
        {
            if (!m_IsRecording)
            {
                // Already not recording. Do nothing.
                return m_SessionId;
            }

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("[{0}] Video Recording Session Stopped", m_SessionId);
            }

            if (m_SessionFileWriter != null)
            {
                m_SessionFileWriter.Dispose();
            }

            lock (LockObj)
            {
                m_IsRecording = false;
            }

            return m_SessionId;
        }

        public void NewDataAvailableHandler(object sender, SensorVideoStreamFrame data)
        {
            if (m_IsRecording)
            {
                lock (LockObj)
                {
                    if (!m_IsRecording)
                    {
                        // Recording stopped. Do nothing.
                        return;
                    }
                }
            }

            if (m_SessionFileWriter != null)
            {
                Mat openCvMat = null;
                Mat openCvMat2 = null;

                openCvMat = new Mat(data.Height, data.Width, DepthType.Cv8U, data.Depth);
                openCvMat2 = new Mat(CapturedVideoHeight, CapturedVideoWidth, DepthType.Cv8U, data.Depth);
                openCvMat.SetTo(data.RawFrameData);
                CvInvoke.Resize(openCvMat, openCvMat2, new Size(CapturedVideoWidth, CapturedVideoHeight));
                CvInvoke.CvtColor(openCvMat2, openCvMat2, ColorConversion.Bgr2Rgb);
                CvInvoke.Flip(openCvMat2, openCvMat2, FlipType.Horizontal);
                //if (StreamType == StreamType.Color)
                //{
                //    openCvMat = new Mat(data.Height, data.Width, DepthType.Cv8U, data.Depth);
                //    openCvMat2 = new Mat(CapturedVideoHeight, CapturedVideoWidth, DepthType.Cv8U, data.Depth);
                //    openCvMat.SetTo(data.RawFrameData);
                //    CvInvoke.Resize(openCvMat, openCvMat2, new Size(CapturedVideoWidth, CapturedVideoHeight));
                //    CvInvoke.CvtColor(openCvMat2, openCvMat2, ColorConversion.Bgr2Rgb);
                //    CvInvoke.Flip(openCvMat2, openCvMat2, FlipType.Horizontal);
                //}
                //else if (StreamType == StreamType.Depth)
                //{
                //    openCvMat = new Mat(data.Height, data.Width, DepthType.Cv8U, data.Depth);
                //    openCvMat2 = new Mat(CapturedVideoHeight, CapturedVideoWidth, DepthType.Cv8U, 3);
                //    openCvMat.SetTo(data.RawFrameData);
                //    openCvMat.ConvertTo(openCvMat2, DepthType.Cv8U);
                //    //CvInvoke.Resize(openCvMat, openCvMat2, new Size(CapturedVideoWidth, CapturedVideoHeight));
                //    //CvInvoke.CvtColor(openCvMat, openCvMat2, ColorConversion.Gray2Rgb);
                //    CvInvoke.Flip(openCvMat2, openCvMat2, FlipType.Horizontal);
                //}
                
                openCvMat2.Save("Test.png");

                int fps = CalculateFrameRate();

                // DEBUG INFO
                if (m_ShowCaptureFps && m_Logger.IsDebugEnabled)
                {
                    //if (fps != 0)
                    //{
                    m_ShowCaptureFpsTimes++;
                    m_MeanCaptureFrameRate += fps;
                    //m_Logger.Debug("[{0}] Video Capture FPS: {1}", m_SessionId, fps);
                    //}

                    if (m_ShowCaptureFpsTimes >= 500)
                    {
                        m_ShowCaptureFps = false;
                        m_MeanCaptureFrameRate = m_MeanCaptureFrameRate/m_ShowCaptureFpsTimes;
                        m_Logger.Debug("[{0}] Video Capture FPS: {1}", m_SessionId, m_MeanCaptureFrameRate);
                    }
                }

                // Save frame only if running near the expected framerate
                if (fps >= (CapturedVideoFps - 5)) // GIANNI TODO: move threhold somewhere better
                {
                    m_SessionFileWriter.Write(openCvMat2);
                }

                // If real FPS is less than computed, add an additional duplicated frame
                if (fps < (CapturedVideoFps - 5)) // GIANNI TODO: move threhold somewhere better
                {
                    m_SessionFileWriter.Write(openCvMat2);
                }

                openCvMat.Dispose();
                openCvMat2.Dispose();

                //// If file reached max file size, split file. AT THE MOMENT IT IS NOT SUPPORTED!
                //// Close current file and reopen new stream.
                //if (m_SessionFileWriter.Length >= MaxSessionFileSizeInMBytes*1000000)
                //{
                //    m_SessionFileWriter.Close();

                //    m_SessionFiles++;

                //    string sessionFileName = Path.Combine(RecordingSessionFolder, string.Format(SessionFileNameFormat, m_SessionId, StreamType, m_SessionFiles));

                //    if (m_Logger.IsDebugEnabled)
                //    {
                //        m_Logger.Debug("[{0}] New Recording Session File", m_SessionId);
                //    }

                //    // Create the writer for data
                //    m_SessionFileWriter = new VideoWriter(sessionFileName, VideoWriter.Fourcc('W', 'M', 'V', '3'), CapturedVideoFPS, new System.Drawing.Size(CapturedVideoWidth, CapturedVideoHeight), true);

                //    lock (LockObj)
                //    {
                //        m_IsRecording = true;
                //    }
                //}
            }
        }

        #endregion

        #region Private methods

        private int CalculateFrameRate()
        {
            // Beware that 10000 ticks in 1ms (https://msdn.microsoft.com/it-it/library/system.datetime.ticks(v=vs.110).aspx)
            // So, here instead we need to use Stopwatch Frequency for precise calculations
            if (Stopwatch.GetTimestamp() - m_LastTick >= Stopwatch.Frequency)
            {
                m_LastFrameRate = m_FrameRate;
                m_FrameRate = 0;
                m_LastTick = Stopwatch.GetTimestamp();
            }
            m_FrameRate++;
            return m_LastFrameRate;
        }
        #endregion
    }
}

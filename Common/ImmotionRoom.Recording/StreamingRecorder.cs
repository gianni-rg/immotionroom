namespace ImmotionAR.ImmotionRoom.Recording
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Interfaces;
    using Logger;
    using PCLStorage;
    using Protocol;
    using FileAccess = PCLStorage.FileAccess;

    public class StreamingRecorder : IStreamingRecorder
    {
        private const string DefaultRecordingSessionFolder = "RecordedSessions";
        private const string SessionFileNameFormat = "{0}_{1}.ses";
        private const string SessionFileNameTimestampFormat = "yyyyMMdd_HHmmss";
        private const int MaxSessionFileSizeInMBytes = 100; // GIANNI TODO: move this setting somewhere better...

        #region Private fields

        private static readonly object LockObj = new object();
        private readonly ILogger m_Logger;

        private int m_SessionFiles;

        private BinaryWriter m_SessionFileWriter;
        private readonly SensorDataFrameSerializer m_DataBinarySerializer;
        private string m_SessionId;

        #endregion

        #region Properties

        public bool IsRecording { get; private set; }

        public string DataRecorderSessionPath { get; set; }

        #endregion

        #region Constructor

        public StreamingRecorder()
        {
            m_Logger = LoggerService.GetLogger<StreamingRecorder>();

            DataRecorderSessionPath = DefaultRecordingSessionFolder;
            m_DataBinarySerializer = new SensorDataFrameSerializer();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Start Skeleton Data Recording Session
        /// </summary>
        /// <param name="sessionId">If Video Recording is enabled, use the same sessionId</param>
        public async Task<string> StartRecordingAsync(string sessionId = null)
        {
            if (IsRecording)
            {
                // Already recording. Do nothing.
                return m_SessionId;
            }

            // Prepare a new session file in the format "yyyyMMdd_HHmmss_<counter>.txt"
            // Each file has a max size
            m_SessionFiles = 1;
            m_SessionId = sessionId;
            if (string.IsNullOrEmpty(m_SessionId))
            {
                m_SessionId = DateTime.UtcNow.ToString(SessionFileNameTimestampFormat);
            }

            await GetDataRecorderSessionFileAsync();

            lock (LockObj)
            {
                IsRecording = true;
            }

            return m_SessionId;
        }

        public string StopRecording()
        {
            if (!IsRecording)
            {
                // Already not recording. Do nothing.
                return m_SessionId;
            }

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("[{0}] SkeletonData Recording Stopped", m_SessionId);
            }

            if (m_SessionFileWriter != null)
            {
                m_SessionFileWriter.Dispose();
            }

            lock (LockObj)
            {
                IsRecording = false;
            }

            return m_SessionId;
        }

        public async void NewDataAvailableHandler(object sender, SensorDataFrame data)
        {
            if (IsRecording)
            {
                lock (LockObj)
                {
                    if (!IsRecording)
                    {
                        // Recording stopped. Do nothing.
                        return;
                    }
                }
            }

            if (m_SessionFileWriter != null)
            {
                var binData = m_DataBinarySerializer.Serialize(data);
                m_SessionFileWriter.Write(binData.Length);
                m_SessionFileWriter.Write(binData);

                // If file reached max file size, split file.
                // Close current file and reopen new stream.
                if (m_SessionFileWriter.BaseStream.Length >= MaxSessionFileSizeInMBytes*1000000)
                {
                    m_SessionFileWriter.Dispose();

                    m_SessionFiles++;

                    await GetDataRecorderSessionFileAsync();

                    lock (LockObj)
                    {
                        IsRecording = true;
                    }
                }
            }
        }

        #endregion

        #region Private methods

        private async Task<IFile> GetDataRecorderSessionFileAsync()
        {
            IFolder recordingFolder;

            if (string.IsNullOrEmpty(DataRecorderSessionPath))
            {
                var localStorage = FileSystem.Current.LocalStorage;
                recordingFolder = await localStorage.CreateFolderAsync(DefaultRecordingSessionFolder, CreationCollisionOption.OpenIfExists).ConfigureAwait(false);
            }
            else
            {
                // WARNING! The folder must be created from the platform-specific root project, otherwise
                // here we'll get a null exception. We're doing that when loading the configuration in DataSourceService app.
                recordingFolder = await FileSystem.Current.GetFolderFromPathAsync(DataRecorderSessionPath).ConfigureAwait(false);
            }

            var sessionFileName = string.Format(SessionFileNameFormat, m_SessionId, m_SessionFiles);
            var recordingFile = await recordingFolder.CreateFileAsync(sessionFileName, CreationCollisionOption.ReplaceExisting).ConfigureAwait(false);

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("[{0}] New SkeletonData Recording Session File", m_SessionId);
            }

            // Create the writer for data
            m_SessionFileWriter = new BinaryWriter(await recordingFile.OpenAsync(FileAccess.ReadAndWrite).ConfigureAwait(false));

            return recordingFile;
        }

        #endregion
    }
}

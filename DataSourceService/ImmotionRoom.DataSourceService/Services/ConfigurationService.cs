namespace ImmotionAR.ImmotionRoom.DataSourceService
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Interfaces;
    using Logger;
    using Model;
    using Newtonsoft.Json;
    using PCLStorage;

    public class ConfigurationService : IConfigurationService
    {
        private const string DataSourceSettingsFileName = "Settings.txt";
        private const string DataSourceSettingsBackupFileName = "Settings.bak";
        private const string DataSourceDataFolderName = "ImmotionAR\\ImmotionRoom\\DataSourceService";

        #region Private fields
        private readonly ILogger m_Logger;
        private bool m_Initialized;
        #endregion

        #region Properties

        public DataSourceConfiguration CurrentConfiguration { get; private set; }
        public TrackingServiceInfo TrackingService { get; set; }

        #endregion

        #region Constructor

        public ConfigurationService()
        {
            m_Logger = LoggerService.GetLogger<ConfigurationService>();
            m_Initialized = false;
        }

        #endregion

        #region Methods

        public async Task InitializeAsync()
        {
            if (m_Logger.IsInfoEnabled)
            {
                m_Logger.Info("DataSourceID: {0}", CurrentConfiguration.InstanceId);
                m_Logger.Info("Local Endpoint: {0}", CurrentConfiguration.LocalEndpoint);
            }

            if (m_Initialized)
            {
                return;
            }
           
            await LoadInternalSettingsAsync().ConfigureAwait(false);
        }

        public Task SaveCurrentConfigurationAsync()
        {
            return SaveSettingsAsync();
        }

        /// <summary>
        /// Load DataSourceService configuration from the provided <code>DataSourceConfiguration</code> instance.
        /// </summary>
        /// <param name="configuration">A DataSourceConfiguration loaded from external sources</param>
        public void LoadExternalConfiguration(DataSourceConfiguration configuration)
        {
            CurrentConfiguration = configuration;
        }

        public void LoadInternalConfiguration(TrackingServiceInfo knownTrackingService)
        {
            TrackingService = knownTrackingService;

            m_Initialized = knownTrackingService != null;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Load internal config files (known Tracking Service)
        /// </summary>
        private async Task LoadInternalSettingsAsync()
        {
            try
            {
                IFile settingsDataFile = await GetConfigurationFile(DataSourceSettingsFileName).ConfigureAwait(false);

                if (settingsDataFile == null)
                {
                    TrackingService = null;
                    m_Initialized = true;
                    return;
                }

                string serializedObject = await settingsDataFile.ReadAllTextAsync().ConfigureAwait(false);
            
                if (!string.IsNullOrEmpty(serializedObject))
                {
                    var trackingServiceInfo = JsonConvert.DeserializeObject<TrackingServiceInfo>(serializedObject);
                    TrackingService = trackingServiceInfo;

                    if (m_Logger.IsDebugEnabled)
                    {
                    	m_Logger.Debug("LoadInternalSettingsAsync: Settings loaded");
                    }
                }
                else
                {
                    TrackingService = null;

                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Warn("LoadInternalSettingsAsync: Settings file does not contain valid content");
                    }
                }
            }
            catch (Exception ex)
            {
                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Error(ex, "LoadInternalSettingsAsync: failed - {0}", ex.Message);
                }
            }

            m_Initialized = true;
        }

        private async Task SaveSettingsAsync()
        {
            try
            {
                IFile settingsDataFile = await GetConfigurationFile(DataSourceSettingsFileName).ConfigureAwait(false);

                // Save a backup copy
                if (settingsDataFile != null)
                {
                    await settingsDataFile.RenameAsync(DataSourceSettingsBackupFileName, NameCollisionOption.ReplaceExisting).ConfigureAwait(false);
                }
            
                // Remove file if no TrackingService configured
                if (TrackingService == null)
                {
                    //await settingsDataFile.DeleteAsync().ConfigureAwait(false);
                    return;
                }

                var serializedObject = JsonConvert.SerializeObject(TrackingService);

                // Create new file with new Settings data
                settingsDataFile = await GetConfigurationFile(DataSourceSettingsFileName, true).ConfigureAwait(false);
                
                await settingsDataFile.WriteAllTextAsync(serializedObject).ConfigureAwait(false);
                
                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("SaveSettingsAsync: Settings saved");
                }
            }
            catch (Exception ex)
            {
                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Error(ex, "SaveSettingsAsync: failed - {0}", ex.Message);
                }
            }
        }

        private async Task<IFile> GetConfigurationFile(string fileName, bool createIfNotExists = false)
        {
            IFolder commonAppDataPathFolder = null;
            if (string.IsNullOrEmpty(CurrentConfiguration.SettingsStorageLocation))
            {
                commonAppDataPathFolder = FileSystem.Current.LocalStorage;
            }
            else
            {
                commonAppDataPathFolder = await FileSystem.Current.GetFolderFromPathAsync(CurrentConfiguration.SettingsStorageLocation).ConfigureAwait(false);
            }
            
            IFolder dataRootFolder = await commonAppDataPathFolder.CreateFolderAsync(DataSourceDataFolderName, CreationCollisionOption.OpenIfExists).ConfigureAwait(false);

            try
            {
                return await dataRootFolder.GetFileAsync(fileName).ConfigureAwait(false);
            }
            catch (FileNotFoundException)
            {
                // Will eventually create a new file
            }

            if (createIfNotExists)
            {
                return await dataRootFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists).ConfigureAwait(false);
            }

            return null;
        }
        #endregion
    }
}

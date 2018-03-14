namespace ImmotionAR.ImmotionRoom.TrackingService
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Interfaces;
    using Logger;
    using Model;
    using Newtonsoft.Json;
    using PCLStorage;
    using TrackingEngine.Model;
    using FileNotFoundException = PCLStorage.Exceptions.FileNotFoundException;

    public class ConfigurationService : IConfigurationService
    {
        private const string TrackingServiceDataSourcesFileName = "DataSources.txt";
        private const string TrackingServiceDataSourcesBackupFileName = "DataSources.bak";
        private const string TrackingServiceCalibrationDataFileName = "Calibration.txt";
        private const string TrackingServiceCalibrationDataBackupFileName = "Calibration.bak";
        private const string TrackingServiceDataFolderName = "ImmotionAR\\ImmotionRoom\\TrackingService";
        private const string TrackingServiceSceneDescriptorFileName = "SceneDescription.txt";
        private const string TrackingServiceSceneDescriptorBackupFileName = "SceneDescription.bak";

        #region Private fields
        private readonly ILogger m_Logger;

        private static readonly object LockObj = new object();

        private ConcurrentDictionary<string, DataSourceInfo> m_KnownDataSources;
        private ConcurrentDictionary<string, byte> m_DataSourcesInternalUniqueIds;
        private int m_DataSourcesCount;
        private bool m_Initialized;

        #endregion

        #region Properties

        public TrackingServiceConfiguration CurrentConfiguration { get; private set; }
        public CalibrationSettings CalibrationData { get; private set; }

        public IReadOnlyDictionary<string, DataSourceInfo> KnownDataSources
        {
            get
            {
                if (m_KnownDataSources != null)
                {
                    return new ReadOnlyDictionary<string, DataSourceInfo>(m_KnownDataSources);
                }

                return null;
            }
        }

        public string CurrentMasterDataSource { get; private set; }

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
                m_Logger.Info("TrackingServiceID: {0}", CurrentConfiguration.InstanceId);
                m_Logger.Info("Local Endpoint: {0}", CurrentConfiguration.LocalEndpoint);
            }

            if (m_Initialized)
            {
                // Just load any existing calibration data
                await LoadCalibrationDataAsync().ConfigureAwait(false);
                return;
            }

            // Otherwise, load internal config files (datasources / calibration / scene descriptor)
            var t1 = LoadKnownDataSourcesAsync();
            var t2 = LoadCalibrationDataAsync();
            var t3 = LoadSceneDescriptorAsync();

            await Task.WhenAll(t1, t2, t3).ConfigureAwait(false);

            lock (LockObj)
            {
                m_Initialized = true;
            }
        }

        public async Task SaveCurrentConfigurationAsync()
        {
            // Save internal config files (datasources / calibration / scene descriptor)
            var t1 = SaveKnownDataSourcesAsync();
            var t2 = SaveCalibrationDataAsync();
            var t3 = SaveSceneDescriptorAsync();

            await Task.WhenAll(t1, t2, t3).ConfigureAwait(false);
        }


        public bool AddDataSource(DataSourceInfo dataSourceInfo)
        {
            bool initialized;
            lock (LockObj)
            {
                initialized = m_Initialized;
            }

            if (!initialized)
            {
                if (m_Logger.IsErrorEnabled)
                {
                    m_Logger.Error("AddDataSource: abort. ConfigurationService not initialized");
                }

                return false;
            }

            // Keep the Master DataSource info (if any)
            if (!string.IsNullOrEmpty(CurrentMasterDataSource) && dataSourceInfo.Id == CurrentMasterDataSource)
            {
                dataSourceInfo.IsMaster = true;
            }

            dataSourceInfo.UniqueId = (byte)(m_DataSourcesCount + 1);
            if (m_DataSourcesInternalUniqueIds.TryGetValue(dataSourceInfo.Id, out byte existingUniqueId))
            {
                dataSourceInfo.UniqueId = existingUniqueId;
            }
            else
            {
                if (!m_DataSourcesInternalUniqueIds.TryAdd(dataSourceInfo.Id, dataSourceInfo.UniqueId))
                {
                    if (m_Logger.IsWarnEnabled)
                    {
                        m_Logger.Warn("AddDataSource: failed UniqueId for '{0}'", dataSourceInfo.Id);
                    }
                }
                else
                {
                    Interlocked.Add(ref m_DataSourcesCount, 1);
                }
            }

            m_KnownDataSources.AddOrUpdate(dataSourceInfo.Id, dataSourceInfo, (key, existing) => dataSourceInfo);

            // Keep the Calibration Matrix (if not requested to be cleared)
            if (!CalibrationData.SlaveToMasterCalibrationMatrices.ContainsKey(dataSourceInfo.Id))
            {
                CalibrationData.SlaveToMasterCalibrationMatrices.TryAdd(dataSourceInfo.Id, Matrix4x4.Identity);
            }

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("AddDataSource: added '{0}'", dataSourceInfo.Id);
            }

            return true;
        }

        public bool RemoveDataSource(string dataSourceId)
        {
            bool initialized;
            lock (LockObj)
            {
                initialized = m_Initialized;
            }

            if (!initialized)
            {
                if (m_Logger.IsErrorEnabled)
                {
                    m_Logger.Error("RemoveDataSource: abort. ConfigurationService not initialized");
                }

                return false;
            }

            if (m_KnownDataSources.TryRemove(dataSourceId, out DataSourceInfo removedDataSource))
            {
                if (m_DataSourcesInternalUniqueIds.TryRemove(dataSourceId, out byte removedUniqueId))
                {
                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("RemoveDataSource: success for '{0}'", dataSourceId);
                    }
                }
                else
                {
                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Error("RemoveDataSource: failed for '{0}' -- UniqueId", dataSourceId);
                    }
                }
            }
            else
            {
                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Error("RemoveDataSource: failed for '{0}'", dataSourceId);
                }
            }

            return true;
        }

        public bool ClearKnownDataSources(bool clearMasterDataSource)
        {
            bool initialized;
            lock (LockObj)
            {
                initialized = m_Initialized;
            }

            if (!initialized)
            {
                if (m_Logger.IsErrorEnabled)
                {
                    m_Logger.Error("ClearKnownDataSources: abort. ConfigurationService not initialized");
                }

                return false;
            }

            foreach (var knownDataSource in m_KnownDataSources)
            {
                RemoveDataSource(knownDataSource.Key);
            }

            m_DataSourcesCount = (byte)m_KnownDataSources.Count;

            if (clearMasterDataSource)
            {
                CurrentMasterDataSource = null;

                // System Configuration Reset occured, but calibration data must be cleared separately (if needed)
                //CalibrationData.CalibrationDone = false;
                //CalibrationData.MasterToWorldCalibrationMatrix = Matrix4x4.Identity;
                //CalibrationData.SlaveToMasterCalibrationMatrices.Clear();
            }

            return true;
        }

        public bool ClearCalibrationData()
        {
            bool initialized;
            lock (LockObj)
            {
                initialized = m_Initialized;
            }

            if (!initialized)
            {
                if (m_Logger.IsErrorEnabled)
                {
                    m_Logger.Error("ClearKnownDataSources: abort. ConfigurationService not initialized");
                }

                return false;
            }

            // System Configuration Reset requested, so re-calibration is needed
            CalibrationData.CalibrationDone = false;
            CalibrationData.MasterToWorldCalibrationMatrix = Matrix4x4.Identity;
            CalibrationData.SlaveToMasterCalibrationMatrices.Clear();

            return true;
        }

        public bool SetMasterDataSource(string dataSourceId)
        {
            bool initialized;
            lock (LockObj)
            {
                initialized = m_Initialized;
            }

            if (!initialized)
            {
                if (m_Logger.IsErrorEnabled)
                {
                    m_Logger.Error("SetMasterDataSource: abort. ConfigurationService not initialized");
                }

                return false;
            }

            // Only 1 Master can exist in the Tracking System,
            // so first reset the existing ones and set the new one
            CurrentMasterDataSource = null;
            foreach (DataSourceInfo dataSource in m_KnownDataSources.Values)
            {
                dataSource.IsMaster = false;
                if (dataSource.Id == dataSourceId)
                {
                    dataSource.IsMaster = true;
                    CurrentMasterDataSource = dataSourceId;
                }
            }

            // By changing the Master DataSource, calibration data
            // become invalid. So re-calibration is required.
            CalibrationData.CalibrationDone = false;
            CalibrationData.MasterToWorldCalibrationMatrix = Matrix4x4.Identity;
            CalibrationData.SlaveToMasterCalibrationMatrices.Clear();

            if (string.IsNullOrEmpty(CurrentMasterDataSource))
            {
                if (m_Logger.IsErrorEnabled)
                {
                    m_Logger.Error("SetMasterDataSource fail: no Master DataSource configured.");
                }
                return false;
            }

            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("SetMasterDataSource success. New Master is '{0}'", dataSourceId);
            }

            return true;
        }


        public void UpdateCalibrationData(CalibrationSettings calibrationData)
        {
            if (calibrationData == null)
            {
                CalibrationData = new CalibrationSettings { CalibrationDone = false };
            }
            else
            {
                CalibrationData = calibrationData;
            }
        }

        public void SetSceneDescriptor(SceneDescriptor descriptor)
        {
            // Floor values are ignored when updating the Scene Descriptor, because
            // they can be automatically detected using Calibration (in the future)
            // or retrieved by DataSources (now).

            var existingFloorPlane = CurrentConfiguration.Scene.FloorClipPlane;

            if (descriptor == null)
            {
                CurrentConfiguration.Scene = TrackingServiceConfiguration.DefaultScene;
            }
            else
            {
                CurrentConfiguration.Scene = descriptor;
            }

            CurrentConfiguration.Scene.FloorClipPlane = existingFloorPlane;
        }

        public void LoadExternalConfiguration(TrackingServiceConfiguration configuration)
        {
            CurrentConfiguration = configuration;
        }

        public void LoadInternalConfiguration(DataSourceCollection knownDataSources)
        {
            if (knownDataSources != null)
            {
                m_KnownDataSources = new ConcurrentDictionary<string, DataSourceInfo>(knownDataSources, StringComparer.OrdinalIgnoreCase);
                m_DataSourcesInternalUniqueIds = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
                m_DataSourcesCount = (byte)m_KnownDataSources.Count;
                foreach (var dataSourceInfo in m_KnownDataSources.Values)
                {
                    if (!m_DataSourcesInternalUniqueIds.TryAdd(dataSourceInfo.Id, dataSourceInfo.UniqueId))
                    {
                        if (m_Logger.IsWarnEnabled)
                        {
                            m_Logger.Warn("LoadInternalConfiguration: Unable to add UniqueId for '{0}'", dataSourceInfo.Id);
                        }
                    }
                }

                DataSourceInfo masterDataSource = m_KnownDataSources.Values.FirstOrDefault(d => d.IsMaster);
                CurrentMasterDataSource = masterDataSource?.Id;
                m_Initialized = true;
            }
            else
            {
                m_KnownDataSources = new ConcurrentDictionary<string, DataSourceInfo>(StringComparer.OrdinalIgnoreCase);
                m_DataSourcesInternalUniqueIds = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
                m_DataSourcesCount = 0;
                CurrentMasterDataSource = null;
                m_Initialized = false;
            }
        }

        #endregion

        #region Private methods

        private async Task LoadKnownDataSourcesAsync()
        {
            try
            {
                IFile dataSourcesFile = await GetConfigurationFile(TrackingServiceDataSourcesFileName).ConfigureAwait(false);

                if (dataSourcesFile == null)
                {
                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("LoadKnownDataSourcesAsync: DataSourcesFile does not exist. First run...");
                    }

                    m_KnownDataSources = new ConcurrentDictionary<string, DataSourceInfo>(StringComparer.OrdinalIgnoreCase);
                    m_DataSourcesInternalUniqueIds = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
                    m_DataSourcesCount = 0;
                    CurrentMasterDataSource = null;
                    return;
                }

                string serializedObject = await dataSourcesFile.ReadAllTextAsync().ConfigureAwait(false);

                if (!string.IsNullOrEmpty(serializedObject))
                {
                    m_KnownDataSources = JsonConvert.DeserializeObject<ConcurrentDictionary<string, DataSourceInfo>>(serializedObject);
                    m_DataSourcesInternalUniqueIds = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
                    m_DataSourcesCount = (byte)m_KnownDataSources.Count;
                    foreach (var dataSourceInfo in m_KnownDataSources.Values)
                    {
                        if (!m_DataSourcesInternalUniqueIds.TryAdd(dataSourceInfo.Id, dataSourceInfo.UniqueId))
                        {
                            if (m_Logger.IsWarnEnabled)
                            {
                                m_Logger.Warn("LoadKnownDataSourcesAsync: Unable to add UniqueId for '{0}'", dataSourceInfo.Id);
                            }
                        }
                    }
                    DataSourceInfo masterDataSource = m_KnownDataSources.Values.FirstOrDefault(d => d.IsMaster);
                    CurrentMasterDataSource = masterDataSource?.Id;

                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("LoadKnownDataSourcesAsync: DataSources loaded");
                    }
                }
                else
                {
                    m_KnownDataSources = new ConcurrentDictionary<string, DataSourceInfo>(StringComparer.OrdinalIgnoreCase);
                    m_DataSourcesInternalUniqueIds = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
                    m_DataSourcesCount = 0;
                    CurrentMasterDataSource = null;

                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Warn("LoadKnownDataSourcesAsync: DataSourcesFile does not contain valid content");
                    }
                }
            }
            catch (Exception ex)
            {
                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Error(ex, "LoadKnownDataSourcesAsync: failed - {0}", ex.Message);
                }
            }
        }

        private async Task LoadCalibrationDataAsync()
        {
            try
            {
                IFile calibrationDataFile = await GetConfigurationFile(TrackingServiceCalibrationDataFileName).ConfigureAwait(false);

                if (calibrationDataFile == null)
                {
                    CalibrationData = new CalibrationSettings { CalibrationDone = false };
                    return;
                }

                string serializedObject = await calibrationDataFile.ReadAllTextAsync().ConfigureAwait(false);

                if (!string.IsNullOrEmpty(serializedObject))
                {
                    CalibrationData = JsonConvert.DeserializeObject<CalibrationSettings>(serializedObject);

                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("LoadCalibrationDataAsync: Calibration data loaded");
                    }
                }
                else
                {
                    CalibrationData = new CalibrationSettings { CalibrationDone = false };
                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Warn("LoadCalibrationDataAsync: Calibration file does not contain valid content");
                    }
                }
            }
            catch (Exception ex)
            {
                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Error(ex, "LoadCalibrationDataAsync: failed - {0}", ex.Message);
                }
            }
        }

        private async Task LoadSceneDescriptorAsync()
        {
            try
            {
                IFile sceneDescriptorFile = await GetConfigurationFile(TrackingServiceSceneDescriptorFileName).ConfigureAwait(false);

                if (sceneDescriptorFile == null)
                {
                    // Use built-in default settings
                    return;
                }

                string serializedObject = await sceneDescriptorFile.ReadAllTextAsync().ConfigureAwait(false);

                if (!string.IsNullOrEmpty(serializedObject))
                {
                    CurrentConfiguration.Scene = JsonConvert.DeserializeObject<SceneDescriptor>(serializedObject);

                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Debug("LoadSceneDescriptorAsync: Scene description loaded");
                    }
                }
                else
                {
                    CurrentConfiguration.Scene = new SceneDescriptor();
                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Warn("LoadSceneDescriptorAsync: Scene Description file does not contain valid content");
                    }
                }
            }
            catch (Exception ex)
            {
                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Error(ex, "LoadSceneDescriptorAsync: failed - {0}", ex.Message);
                }
            }
        }

        private async Task SaveKnownDataSourcesAsync()
        {
            try
            {
                IFile dataSourcesFile = await GetConfigurationFile(TrackingServiceDataSourcesFileName).ConfigureAwait(false);

                // Save a backup copy
                if (dataSourcesFile != null)
                {
                    await dataSourcesFile.RenameAsync(TrackingServiceDataSourcesBackupFileName, NameCollisionOption.ReplaceExisting).ConfigureAwait(false);
                }

                // Remove file if no DataSources configured
                if (m_KnownDataSources == null || m_KnownDataSources.Count == 0)
                {
                    return;
                }

                string serializedObject = JsonConvert.SerializeObject(m_KnownDataSources);

                // Create new file with new DataSources data
                dataSourcesFile = await GetConfigurationFile(TrackingServiceDataSourcesFileName, true).ConfigureAwait(false);

                await dataSourcesFile.WriteAllTextAsync(serializedObject).ConfigureAwait(false);

                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("SaveKnownDataSourcesAsync: Known Data Sources saved");
                }
            }
            catch (Exception ex)
            {
                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Error(ex, "SaveKnownDataSourcesAsync: failed - {0}", ex.Message);
                }
            }
        }

        private async Task SaveCalibrationDataAsync()
        {
            try
            {
                IFile calibrationDataFile = await GetConfigurationFile(TrackingServiceCalibrationDataFileName).ConfigureAwait(false);

                // Save a backup copy
                if (calibrationDataFile != null)
                {
                    await calibrationDataFile.RenameAsync(TrackingServiceCalibrationDataBackupFileName, NameCollisionOption.ReplaceExisting).ConfigureAwait(false);
                }

                // Remove file if no DataSources configured
                if (CalibrationData == null || CalibrationData.SlaveToMasterCalibrationMatrices.Count == 0)
                {
                    return;
                }

                string serializedObject = JsonConvert.SerializeObject(CalibrationData);

                // Create new file with new Calibration Data
                calibrationDataFile = await GetConfigurationFile(TrackingServiceCalibrationDataFileName, true).ConfigureAwait(false);

                await calibrationDataFile.WriteAllTextAsync(serializedObject).ConfigureAwait(false);

                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("SaveCalibrationDataAsync: Calibration Data saved");
                }
            }
            catch (Exception ex)
            {
                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Error(ex, "SaveCalibrationDataAsync: failed - {0}", ex.Message);
                }
            }
        }

        private async Task SaveSceneDescriptorAsync()
        {
            try
            {
                IFile sceneDescriptionFile = await GetConfigurationFile(TrackingServiceSceneDescriptorFileName).ConfigureAwait(false);

                // Save a backup copy
                if (sceneDescriptionFile != null)
                {
                    await sceneDescriptionFile.RenameAsync(TrackingServiceSceneDescriptorBackupFileName, NameCollisionOption.ReplaceExisting).ConfigureAwait(false);
                }

                // Remove file if no scene description is configured
                if (CurrentConfiguration.Scene.Equals(TrackingServiceConfiguration.Default.Scene))
                {
                    return;
                }

                string serializedObject = JsonConvert.SerializeObject(CurrentConfiguration.Scene);

                // Create new file with new Scene Descriptor Data
                sceneDescriptionFile = await GetConfigurationFile(TrackingServiceSceneDescriptorFileName, true).ConfigureAwait(false);

                await sceneDescriptionFile.WriteAllTextAsync(serializedObject).ConfigureAwait(false);

                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("SaveSceneDescriptorAsync: Scene Description saved");
                }
            }
            catch (Exception ex)
            {
                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Error(ex, "SaveSceneDescriptorAsync: failed - {0}", ex.Message);
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

            IFolder dataRootFolder = await commonAppDataPathFolder.CreateFolderAsync(TrackingServiceDataFolderName, CreationCollisionOption.OpenIfExists).ConfigureAwait(false);

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

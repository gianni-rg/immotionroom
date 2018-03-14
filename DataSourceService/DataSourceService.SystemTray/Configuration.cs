namespace ImmotionAR.ImmotionRoom.DataSourceService
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using AutoDiscovery;
    using Helpers;
    using Logger;
    using Logger.Log4Net;
    using Model;
    using Newtonsoft.Json;

    public static class Configuration
    {
        private static readonly CultureInfo EnglishCulture = new CultureInfo("en-US");

        public static DataSourceConfiguration LoadConfigurationFromAppConfig()
        {
            // Use default values if not specified in App.Config

            var configuration = DataSourceConfiguration.Default;

            configuration.InstanceId = GetStringSettingsFromAppConfig("InstanceID") ?? Guid.NewGuid().ToString("N").ToUpper();

            configuration.SettingsStorageLocation = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

            configuration.DataStreamerEndpoint = null;
            string dataStreamerEndpoint;
            if (!string.IsNullOrEmpty(dataStreamerEndpoint = GetStringSettingsFromAppConfig("DataStreamerEndpoint")))
            {
                configuration.DataStreamerEndpoint = dataStreamerEndpoint;
            }

            configuration.DataStreamerPort = GetIntSettingsFromAppConfig("DataStreamerPort") ?? DataSourceDefaultSettings.DataStreamerPort;

            configuration.ControlApiEndpoint = null;
            string controlApiEndpoint;
            if (!string.IsNullOrEmpty(controlApiEndpoint = GetStringSettingsFromAppConfig("ControlApiEndpoint")))
            {
                configuration.ControlApiEndpoint = controlApiEndpoint;
            }
            configuration.ControlApiPort = GetIntSettingsFromAppConfig("ControlApiPort") ?? DataSourceDefaultSettings.ControlApiPort;

            configuration.AutoDiscovery.MulticastAddress = GetStringSettingsFromAppConfig("AutoDiscoveryMulticastAddress") ?? AutoDiscoveryDefaultSettings.AutoDiscoveryMulticastAddress;
            configuration.AutoDiscovery.MulticastPort = GetIntSettingsFromAppConfig("AutoDiscoveryMulticastPort") ?? AutoDiscoveryDefaultSettings.AutoDiscoveryMulticastPort;
            configuration.AutoDiscovery.LocalPort = GetIntSettingsFromAppConfig("AutoDiscoveryLocalPort") ?? AutoDiscoveryDefaultSettings.DataSourceAutoDiscoveryLocalPort;
            configuration.AutoDiscovery.LoopbackLogEnabled = GetBoolSettingsFromAppConfig("AutoDiscoveryLoopbackLogEnabled") ?? AutoDiscoveryDefaultSettings.AutoDiscoveryLoopbackLogEnabled;
            configuration.AutoDiscovery.PollingIntervalInSeconds = GetIntSettingsFromAppConfig("AutoDiscoveryPollingIntervalInSeconds") ?? AutoDiscoveryDefaultSettings.AutoDiscoveryPollingIntervalInSeconds;
            configuration.AutoDiscovery.ListenerIntervalInSeconds = GetIntSettingsFromAppConfig("AutoDiscoveryListenerIntervalInSeconds") ?? AutoDiscoveryDefaultSettings.AutoDiscoveryListenerIntervalInSeconds;
            configuration.AutoDiscovery.ReachableTimeoutInSeconds = GetIntSettingsFromAppConfig("AutoDiscoveryReachableTimeoutInSeconds") ?? AutoDiscoveryDefaultSettings.AutoDiscoveryReachableTimeoutInSeconds;
            configuration.AutoDiscovery.DurationInSeconds = GetIntSettingsFromAppConfig("AutoDiscoveryDurationInSeconds") ?? AutoDiscoveryDefaultSettings.AutoDiscoveryDurationInSeconds;
            configuration.AutoDiscovery.UdpLocalClientTimeoutInSeconds = GetIntSettingsFromAppConfig("AutoDiscoveryUdpLocalClientTimeoutInSeconds") ?? AutoDiscoveryDefaultSettings.AutoDiscoveryUdpLocalClientTimeoutInSeconds;
            configuration.AutoDiscovery.RepeatIntervalInSeconds = GetIntSettingsFromAppConfig("AutoDiscoveryRepeatIntervalInSeconds") ?? DataSourceDefaultSettings.AutoDiscoveryRepeatIntervalInSeconds;

            configuration.TrackingServiceMonitorIntervalInSeconds = GetIntSettingsFromAppConfig("TrackingServiceMonitorIntervalInSeconds") ?? DataSourceDefaultSettings.TrackingServiceMonitorIntervalInSeconds;
            configuration.ClientListenerTimeoutInMilliseconds = GetIntSettingsFromAppConfig("ClientListenerTimeoutInMilliseconds") ?? DataSourceDefaultSettings.ClientListenerTimeoutInMilliseconds;

            configuration.SkeletonDataRecorderEnabled = GetBoolSettingsFromAppConfig("SkeletonDataRecorderEnabled") ?? DataSourceDefaultSettings.SkeletonDataRecorderEnabled;

            configuration.ColorStreamRecorderEnabled = GetBoolSettingsFromAppConfig("ColorStreamRecorderEnabled") ?? DataSourceDefaultSettings.ColorStreamRecorderEnabled;
            configuration.ColorStreamRecorderFps = GetIntSettingsFromAppConfig("ColorStreamRecorderFPS") ?? DataSourceDefaultSettings.ColorStreamRecorderFps;
            configuration.ColorStreamRecorderWidth = GetIntSettingsFromAppConfig("ColorStreamRecorderWidth") ?? DataSourceDefaultSettings.ColorStreamRecorderWidth;
            configuration.ColorStreamRecorderHeight = GetIntSettingsFromAppConfig("ColorStreamRecorderHeight") ?? DataSourceDefaultSettings.ColorStreamRecorderHeight;

            configuration.DepthStreamRecorderEnabled = GetBoolSettingsFromAppConfig("DepthStreamRecorderEnabled") ?? DataSourceDefaultSettings.DepthStreamRecorderEnabled;
            configuration.DepthStreamRecorderFps = GetIntSettingsFromAppConfig("DepthStreamRecorderFPS") ?? DataSourceDefaultSettings.DepthStreamRecorderFps;
            configuration.DepthStreamRecorderWidth = GetIntSettingsFromAppConfig("DepthStreamRecorderWidth") ?? DataSourceDefaultSettings.DepthStreamRecorderWidth;
            configuration.DepthStreamRecorderHeight = GetIntSettingsFromAppConfig("DepthStreamRecorderHeight") ?? DataSourceDefaultSettings.DepthStreamRecorderHeight;

            configuration.ReceivedCommandsCleanerIntervalInMinutes = GetIntSettingsFromAppConfig("ReceivedCommandsCleanerIntervalInMinutes") ?? DataSourceDefaultSettings.ReceivedCommandsCleanerIntervalInMinutes;
            configuration.MaxMessageAliveTimeInSeconds = GetIntSettingsFromAppConfig("MaxMessageAliveTimeInSeconds") ?? DataSourceDefaultSettings.MaxMessageAliveTimeInSeconds;
            configuration.ReceivedCommandsPollingIntervalInMilliseconds = GetIntSettingsFromAppConfig("ReceivedCommandsPollingIntervalInMilliseconds") ?? DataSourceDefaultSettings.ReceivedCommandsPollingIntervalInMilliseconds;

            configuration.SystemRebootDelayInMilliseconds = GetIntSettingsFromAppConfig("SystemRebootDelayInMilliseconds") ?? DataSourceDefaultSettings.SystemRebootDelayInMilliseconds;
            configuration.AutoDiscoveryDelayInMilliseconds = GetIntSettingsFromAppConfig("AutoDiscoveryDelayInMilliseconds") ?? DataSourceDefaultSettings.AutoDiscoveryDelayInMilliseconds;

            configuration.GetLocalIpRetries = GetIntSettingsFromAppConfig("GetLocalIpRetries") ?? DataSourceDefaultSettings.GetLocalIpRetries;
            configuration.GetLocalIpIntervalInSeconds = GetIntSettingsFromAppConfig("GetLocalIpIntervalInSeconds") ?? DataSourceDefaultSettings.GetLocalIpIntervalInSeconds;
            configuration.NetworkAdapterIndex = GetIntSettingsFromAppConfig("NetworkAdapterIndex") ?? DataSourceDefaultSettings.NetworkAdapterIndex;
            
            configuration.DataRecorderSessionPath = GetStringSettingsFromAppConfig("DataRecorderSessionPath");
            if (string.IsNullOrEmpty(configuration.DataRecorderSessionPath))
            {
                // TODO: place this setting in a common place (where also ServiceConfigTool can read)
                var tempFolder = Path.GetTempPath();
                configuration.DataRecorderSessionPath = Path.Combine(tempFolder, "ImmotionAR\\ImmotionRoom\\DataSourceService\\RecordedSessions");
            }

            // Creates the DataRecorderSessionPath -- required here, due to a limit in the PCLStorage library
            if (configuration.SkeletonDataRecorderEnabled && !string.IsNullOrEmpty(configuration.DataRecorderSessionPath) && !Directory.Exists(configuration.DataRecorderSessionPath))
            {
                Directory.CreateDirectory(configuration.DataRecorderSessionPath);
            }

            // Retrieve current local IPv4 address
            // It may happen that sometimes a local IPv4 address is not available when auto-starting the application.
            // So, here we have a limited number of retries in order to retrieve the IP address.
            var retries = configuration.GetLocalIpRetries;
            while (retries > 0)
            {
                configuration.LocalEndpoint = NetworkTools.GetLocalIpAddress(configuration.NetworkAdapterIndex);
                if (configuration.LocalEndpoint == null)
                {
                    retries--;
                    Thread.Sleep(configuration.GetLocalIpIntervalInSeconds*1000);
                }
                else
                {
                    break;
                }
            }

            if (configuration.LocalEndpoint == null)
            {
                throw new InvalidOperationException("No local end-point found");
            }

            // If not overridden in the app.config, use the local endpoint as DataStreamer endpoint
            if (configuration.DataStreamerEndpoint == null)
            {
                configuration.DataStreamerEndpoint = configuration.LocalEndpoint;
            }

            // If not overridden in the app.config, use the local endpoint as ControlApi endpoint
            if (configuration.ControlApiEndpoint == null)
            {
                configuration.ControlApiEndpoint = configuration.LocalEndpoint;
            }

            return configuration;
        }

        public static T LoadConfigurationItemFromFile<T>(string path)
        {
            // If an invalid file is specified, DataSource will use internal data (if any)

            if (!File.Exists(path))
            {
                return default(T);
            }

            string serializedObject;
            using (var reader = new StreamReader(path))
            {
                serializedObject = reader.ReadToEnd();
                reader.Close();
            }

            if (!string.IsNullOrEmpty(serializedObject))
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(serializedObject);
                }
                catch (JsonSerializationException)
                {
                    throw new InvalidOperationException(string.Format("Invalid data read from file: {0}", path));
                }
            }

            return default(T);
        }

        public static ILoggerConfiguration LoadLoggerConfiguration(bool diagnosticMode)
        {
            // Log4Net config

            var logRootFolder = Path.Combine(Path.GetTempPath(),"ImmotionAR\\ImmotionRoom\\DataSourceService");
            Directory.CreateDirectory(logRootFolder);

            var loggerConfig = new LoggerConfiguration
            {
                // Logging is disabled by default. If diagnostic mode is enabled, default level is Debug.
                // It can be overridden via LogLevel parameter in app.config.
                LogLevel = diagnosticMode ? GetStringSettingsFromAppConfig("LogLevel") ?? "Debug" : "Off",

                RollingFileEnabled = diagnosticMode,
                RollingFileLogFormat = "%utcdate|[%3t]|%-5p|%c|%m%n",
                RollingStyle = "Composite",
                DatePattern = "'_'yyyyMMdd'.txt'",
                AppendToFile = true,
                LogFile = Path.Combine(logRootFolder, "Logs\\DataSourceService"),
                MaxSizeRollBackups = 10,
                MaximumFileSize = "5MB",
                StaticLogFileName = false,

                ConsoleLoggerEnabled = false,
                ConsoleLogFormat = "%utcdate|%m%n",

#if DEBUG
                TraceLoggerEnabled = true,
#else
                TraceLoggerEnabled = false,
#endif
                TraceLogFormat = "%utcdate|%m%n",

                MemoryLoggerEnabled = diagnosticMode,
                MemoryLogFormat = "%utcdate|[%3t]|%-5p|%c|%m%n",
            };

            loggerConfig.LogWatcher = new LogWatcher();

            return loggerConfig;
        }

        #region Private methods

        private static int? GetIntSettingsFromAppConfig(string key)
        {
            int tempVal;
            if (int.TryParse(ConfigurationManager.AppSettings[key], out tempVal))
            {
                return tempVal;
            }

            return null;
        }

        private static bool? GetBoolSettingsFromAppConfig(string key)
        {
            bool tempVal;
            if (bool.TryParse(ConfigurationManager.AppSettings[key], out tempVal))
            {
                return tempVal;
            }

            return null;
        }

        private static string GetStringSettingsFromAppConfig(string key)
        {
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings[key]))
            {
                return ConfigurationManager.AppSettings[key];
            }

            return null;
        }

        private static float? GetFloatSettingsFromAppConfig(string key)
        {
            float tempVal;
            if (float.TryParse(ConfigurationManager.AppSettings[key], NumberStyles.Any, EnglishCulture, out tempVal))
            {
                return tempVal;
            }

            return null;
        }

        #endregion
    }
}

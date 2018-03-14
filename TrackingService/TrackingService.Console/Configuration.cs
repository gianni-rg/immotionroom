namespace ImmotionAR.ImmotionRoom.TrackingService
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
    using TrackingEngine.Model;

    public static class Configuration
    {
        private static readonly CultureInfo EnglishCulture = new CultureInfo("en-US");

        public static TrackingServiceConfiguration LoadConfigurationFromAppConfig()
        {
            // Use default values if not specified in App.Config

            var configuration = TrackingServiceConfiguration.Default;

            configuration.InstanceId = GetStringSettingsFromAppConfig("InstanceID") ?? Guid.NewGuid().ToString("N").ToUpper();

            configuration.SettingsStorageLocation = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

            configuration.AutoDiscovery.MulticastAddress = GetStringSettingsFromAppConfig("AutoDiscoveryMulticastAddress") ?? AutoDiscoveryDefaultSettings.AutoDiscoveryMulticastAddress;
            configuration.AutoDiscovery.MulticastPort = GetIntSettingsFromAppConfig("AutoDiscoveryMulticastPort") ?? AutoDiscoveryDefaultSettings.AutoDiscoveryMulticastPort;
            configuration.AutoDiscovery.LocalPort = GetIntSettingsFromAppConfig("AutoDiscoveryLocalPort") ?? AutoDiscoveryDefaultSettings.TrackingServiceAutoDiscoveryLocalPort;
            configuration.AutoDiscovery.LoopbackLogEnabled = GetBoolSettingsFromAppConfig("AutoDiscoveryLoopbackLogEnabled") ?? AutoDiscoveryDefaultSettings.AutoDiscoveryLoopbackLogEnabled;
            configuration.AutoDiscovery.PollingIntervalInSeconds = GetIntSettingsFromAppConfig("AutoDiscoveryPollingIntervalInSeconds") ?? AutoDiscoveryDefaultSettings.AutoDiscoveryPollingIntervalInSeconds;
            configuration.AutoDiscovery.ListenerIntervalInSeconds = GetIntSettingsFromAppConfig("AutoDiscoveryListenerIntervalInSeconds") ?? AutoDiscoveryDefaultSettings.AutoDiscoveryListenerIntervalInSeconds;
            configuration.AutoDiscovery.ReachableTimeoutInSeconds = GetIntSettingsFromAppConfig("AutoDiscoveryReachableTimeoutInSeconds") ?? AutoDiscoveryDefaultSettings.AutoDiscoveryReachableTimeoutInSeconds;
            configuration.AutoDiscovery.DurationInSeconds = GetIntSettingsFromAppConfig("AutoDiscoveryDurationInSeconds") ?? AutoDiscoveryDefaultSettings.AutoDiscoveryDurationInSeconds;
            configuration.AutoDiscovery.UdpLocalClientTimeoutInSeconds = GetIntSettingsFromAppConfig("AutoDiscoveryUdpLocalClientTimeoutInSeconds") ?? AutoDiscoveryDefaultSettings.AutoDiscoveryUdpLocalClientTimeoutInSeconds;
            configuration.AutoDiscovery.RepeatIntervalInSeconds = GetIntSettingsFromAppConfig("AutoDiscoveryRepeatIntervalInSeconds") ?? AutoDiscoveryDefaultSettings.AutoDiscoveryRepeatIntervalInSeconds;
            configuration.AutoDiscovery.CompletionDelayInSeconds = GetIntSettingsFromAppConfig("AutoDiscoveryCompletionDelayInSeconds") ?? AutoDiscoveryDefaultSettings.AutoDiscoveryCompletionDelayInSeconds;

            configuration.DataSourceMonitorIntervalInSeconds = GetIntSettingsFromAppConfig("DataSourceMonitorIntervalInSeconds") ?? TrackingServiceDefaultSettings.DataSourceMonitorIntervalInSeconds;
            configuration.DataSourceReachableTimeoutInSeconds = GetIntSettingsFromAppConfig("DataSourceReachableTimeoutInSeconds") ?? TrackingServiceDefaultSettings.DataSourceReachableTimeoutInSeconds;
            configuration.DataSourceUnreachableMaxRetries = GetIntSettingsFromAppConfig("DataSourceUnreachableMaxRetries") ?? TrackingServiceDefaultSettings.DataSourceUnreachableMaxRetries;
            configuration.DataSourceUnreachableRetryIntervalInMilliseconds = GetIntSettingsFromAppConfig("DataSourceUnreachableRetryIntervalInMilliseconds") ?? TrackingServiceDefaultSettings.DataSourceUnreachableRetryIntervalInMilliseconds;
            configuration.DataSourceAliveTimeInSeconds = GetIntSettingsFromAppConfig("DataSourceAliveTimeInSeconds") ?? TrackingServiceDefaultSettings.DataSourceAliveTimeInSeconds;

            configuration.DataStreamerEndpoint = null;

            string dataStreamerEndpoint;
            if (!string.IsNullOrEmpty(dataStreamerEndpoint = GetStringSettingsFromAppConfig("DataStreamerEndpoint")))
            {
                configuration.DataStreamerEndpoint = dataStreamerEndpoint;
            }

            configuration.DataStreamerPort = GetIntSettingsFromAppConfig("DataStreamerPort") ?? TrackingServiceDefaultSettings.DataStreamerPort;
            configuration.DataStreamerListenerMaxRetries = GetIntSettingsFromAppConfig("DataStreamerListenerMaxRetries") ?? TrackingServiceDefaultSettings.DataStreamerListenerMaxRetries;
            configuration.DataStreamerListenerRetryIntervalInMilliseconds = GetIntSettingsFromAppConfig("DataStreamerListenerRetryIntervalInMilliseconds") ?? TrackingServiceDefaultSettings.DataStreamerListenerRetryIntervalInMilliseconds;
            configuration.DataStreamerClientTimeoutInMilliseconds = GetIntSettingsFromAppConfig("DataStreamerClientTimeoutInMilliseconds") ?? TrackingServiceDefaultSettings.DataStreamerClientTimeoutInMilliseconds;

            configuration.ControlApiEndpoint = null;
            string controlApiEndpoint;
            if (!string.IsNullOrEmpty(controlApiEndpoint = GetStringSettingsFromAppConfig("ControlApiEndpoint")))
            {
                configuration.ControlApiEndpoint = controlApiEndpoint;
            }

            configuration.ControlApiPort = GetIntSettingsFromAppConfig("ControlApiPort") ?? TrackingServiceDefaultSettings.ControlApiPort;

            configuration.AutomaticTrackingStopTimeoutInSeconds = GetIntSettingsFromAppConfig("AutomaticTrackingStopTimeoutInSeconds") ?? TrackingServiceDefaultSettings.AutomaticTrackingStopTimeoutInSeconds;
            configuration.ActiveClientsMonitorIntervalInSeconds = GetIntSettingsFromAppConfig("ActiveClientsMonitorIntervalInSeconds") ?? TrackingServiceDefaultSettings.ActiveClientsMonitorIntervalInSeconds;

            configuration.UpdateLoopFrameRate = GetIntSettingsFromAppConfig("UpdateLoopFrameRate") ?? TrackingServiceDefaultSettings.UpdateLoopFrameRate;

            configuration.MinDataSourcesForPlay = GetIntSettingsFromAppConfig("MinDataSourcesForPlay") ?? TrackingServiceDefaultSettings.MinDataSourcesForPlay;

            configuration.SystemRebootDelayInMilliseconds = GetIntSettingsFromAppConfig("SystemRebootDelayInMilliseconds") ?? TrackingServiceDefaultSettings.SystemRebootDelayInMilliseconds;

            configuration.ReceivedCommandsCleanerIntervalInMinutes = GetIntSettingsFromAppConfig("ReceivedCommandsCleanerIntervalInMinutes") ?? TrackingServiceDefaultSettings.ReceivedCommandsCleanerIntervalInMinutes;
            configuration.MaxMessageAliveTimeInSeconds = GetIntSettingsFromAppConfig("MaxMessageAliveTimeInSeconds") ?? TrackingServiceDefaultSettings.MaxMessageAliveTimeInSeconds;
            configuration.ReceivedCommandsPollingIntervalInMilliseconds = GetIntSettingsFromAppConfig("ReceivedCommandsPollingIntervalInMilliseconds") ?? TrackingServiceDefaultSettings.ReceivedCommandsPollingIntervalInMilliseconds;
            configuration.GetLocalIpRetries = GetIntSettingsFromAppConfig("GetLocalIpRetries") ?? TrackingServiceDefaultSettings.GetLocalIpRetries;
            configuration.GetLocalIpIntervalInSeconds = GetIntSettingsFromAppConfig("GetLocalIpIntervalInSeconds") ?? TrackingServiceDefaultSettings.GetLocalIpIntervalInSeconds;
            
            // Retrieve current local IPv4 address
            // It may happen that sometimes a local IPv4 address is not available when auto-starting the application.
            // So, here we have a limited number of retries in order to retrieve the IP address.
            var retries = configuration.GetLocalIpRetries;
            while (retries > 0)
            {
                configuration.LocalEndpoint = NetworkTools.GetLocalIpAddress();
                if (configuration.LocalEndpoint == null)
                {
                    retries--;
                    Thread.Sleep(configuration.GetLocalIpIntervalInSeconds * 1000);
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
            // If an invalid file is specified, TrackingService will use internal data (if any)

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

        public static ILoggerConfiguration LoadLoggerConfiguration()
        {
            // Log4Net config .. currently we use the config in app.config
            return null;

            //var loggerConfig = new LoggerConfiguration
            //{
            //    LogLevel = GetStringSettingsFromAppConfig("LogLevel") ?? "Info",

            //    RollingFileEnabled = false,
            //    RollingFileLogFormat = "%utcdate|[%3t]|%-5p|%c|%m%n",
            //    RollingStyle = "Composite",
            //    DatePattern = "'_'yyyyMMdd'.txt'",
            //    AppendToFile = true,
            //    LogFile = "Logs\\TrackingService",
            //    MaxSizeRollBackups = 10,
            //    MaximumFileSize = "5MB",
            //    StaticLogFileName = false,

            //    ConsoleLoggerEnabled = false,
            //    ConsoleLogFormat = "%utcdate|%m%n",

            //    TraceLoggerEnabled = true,
            //    TraceLogFormat = "%utcdate|%m%n",

            //    MemoryLoggerEnabled = false,
            //    MemoryLogFormat = "%utcdate|[%3t]|%-5p|%c|%m%n",
            //};

            //return loggerConfig;
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

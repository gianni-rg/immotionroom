namespace ImmotionAR.ImmotionRoom.AutoDiscovery
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Helpers;
#if !UNITY_5
    using Helpers.CrossPlatformSupport;
#endif
    using Logger;
    using Model;
    using Networking.Interfaces;
    using ImmotionAR.ImmotionRoom.Networking;

    public class DataSourceDiscoverer
    {
        #region Constants

        private const string AutoDiscoverySettingsPath = "Config";
        private const string AutoDiscoverySettingsFile = "AutoDiscoverySettings.dat";

        #endregion

        #region Events

        public event EventHandler<DataSourcesDiscoveryCompletedEventArgs> DiscoveryCompleted;

        #endregion

        #region Private fields

        private readonly ILogger m_Logger;
        private readonly IUdpClientFactory m_UdpClientFactory;
        private readonly AutoDiscoverySettings m_Configuration;

        private AutoDiscoveryDiscoverer m_AutoDiscoveryDiscoverer;
        private DataSourceDiscoveryResult m_DiscoveryResult;
        #endregion
        
        #region Constructor

        public DataSourceDiscoverer(AutoDiscoverySettings settings)
        {
            m_Logger = LoggerService.GetLogger<TrackingServiceDiscoverer>();
#if UNITY_5
            m_UdpClientFactory = new UdpClientFactory();
#else
            m_UdpClientFactory = PlatformAdapter.Resolve<IUdpClientFactory>();
#endif
            m_Configuration = settings;
        }

        #endregion

        #region Methods

        public void StartDataSourcesDiscoveryAsync()
        {
            if (m_Logger != null && m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("StartDataSourcesDiscoveryAsync() called");
            }
            
            //if (m_Logger != null && m_Logger.IsDebugEnabled)
            //{
            //    m_Logger.Debug("StartDataSourcesDiscoveryAsync - Configuration loaded");
            //}
            
            Task.Factory.StartNew(async () =>
            {
                m_DiscoveryResult = new DataSourceDiscoveryResult();

                // Impersonates a fake Data Source
                m_AutoDiscoveryDiscoverer = new AutoDiscoveryDiscoverer(DiscovererTypes.DataSourceDiscoverer, m_UdpClientFactory);
                
                m_AutoDiscoveryDiscoverer.LocalAddress = m_Configuration.LocalAddress;
                m_AutoDiscoveryDiscoverer.AutoDiscoveryMulticastAddress = m_Configuration.MulticastAddress;
                m_AutoDiscoveryDiscoverer.AutoDiscoveryMulticastPort = m_Configuration.MulticastPort;
                m_AutoDiscoveryDiscoverer.AutoDiscoveryLocalPort = m_Configuration.LocalPort;
                m_AutoDiscoveryDiscoverer.AutoDiscoveryPollingInterval = m_Configuration.PollingIntervalInSeconds*1000;
                m_AutoDiscoveryDiscoverer.AutoDiscoveryDuration = m_Configuration.DurationInSeconds*1000;
                m_AutoDiscoveryDiscoverer.AutoDiscoveryLoopbackLogEnabled = false;
                m_AutoDiscoveryDiscoverer.AutoDiscoveryListenerPollingTime = m_Configuration.ListenerIntervalInSeconds*1000;
                m_AutoDiscoveryDiscoverer.AutoDiscoveryUdpLocalClientTimeout = m_Configuration.UdpLocalClientTimeoutInSeconds*1000;

                m_AutoDiscoveryDiscoverer.DeviceFound += AutoDiscoveryDiscoverer_OnDeviceFound;
                m_AutoDiscoveryDiscoverer.DiscoveryCompleted += AutoDiscoveryDiscoverer_OnDiscoveryCompleted;

                await m_AutoDiscoveryDiscoverer.StartAsync().ConfigureAwait(false);

                if (m_Logger != null && m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("StartDataSourcesDiscoveryAsync - AutoDiscoveryDiscoverer started");
                }
            });
        }

        public void StopDataSourcesDiscovery()
        {
            if (m_Logger != null && m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("StopDataSourcesDiscovery() called");
            }

            if (m_AutoDiscoveryDiscoverer != null)
            {
                m_AutoDiscoveryDiscoverer.DeviceFound -= AutoDiscoveryDiscoverer_OnDeviceFound;
                m_AutoDiscoveryDiscoverer.DiscoveryCompleted -= AutoDiscoveryDiscoverer_OnDiscoveryCompleted;

                m_AutoDiscoveryDiscoverer.Stop();
            }
        }

        #endregion

        #region Private methods

        private void AutoDiscoveryDiscoverer_OnDiscoveryCompleted(object sender, EventArgs eventArgs)
        {
            if (m_Logger != null && m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("AutoDiscoveryDiscoverer_OnDiscoveryCompleted event");
            }

            m_AutoDiscoveryDiscoverer.DeviceFound -= AutoDiscoveryDiscoverer_OnDeviceFound;
            m_AutoDiscoveryDiscoverer.DiscoveryCompleted -= AutoDiscoveryDiscoverer_OnDiscoveryCompleted;

            m_AutoDiscoveryDiscoverer.Stop();

            OnDiscoveryCompleted(m_DiscoveryResult);
        }

        private void AutoDiscoveryDiscoverer_OnDeviceFound(object sender, DeviceFoundEventArgs args)
        {
            if (m_Logger != null && m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("AutoDiscoveryDiscoverer_OnDeviceFound event");
            }

            // Add only once, ignore if already existing
            if (!m_DiscoveryResult.DataSources.ContainsKey(args.Info.Id))
            {
                m_DiscoveryResult.DataSources.Add(args.Info.Id, new DataSourceItem
                {
                    Id = args.Info.Id,
                    ControlApiEndpoint = args.Info.ControlApiEndpoint,
                    ControlApiPort = args.Info.ControlApiPort,
                    DataStreamerEndpoint = args.Info.DataStreamerEndpoint,
                    DataStreamerPort = args.Info.DataStreamerPort,
                });
            }
        }

        private void OnDiscoveryCompleted(DataSourceDiscoveryResult result)
        {
            EventHandler<DataSourcesDiscoveryCompletedEventArgs> localHandler = DiscoveryCompleted;
            if (localHandler != null)
            {
                localHandler(this, new DataSourcesDiscoveryCompletedEventArgs(result));
            }
        }

        //private AutoDiscoverySettings LoadAutoDiscoverySettingsFromFile()
        //{
        //    if (!Directory.Exists(AutoDiscoverySettingsPath))
        //    {
        //        Directory.CreateDirectory(AutoDiscoverySettingsPath);
        //    }

        //    string discoverySettingsFullPath = Path.Combine(AutoDiscoverySettingsPath, AutoDiscoverySettingsFile);

        //    var settings = new AutoDiscoverySettings();

        //    if (!File.Exists(discoverySettingsFullPath))
        //    {
        //        if (m_Logger != null && m_Logger.IsDebugEnabled)
        //        {
        //            m_Logger.Debug("LoadAutoDiscoverySettingsFromFile - AutoDiscoverySettingsFile does not exist. Create default.");
        //        }

        //        // Create the file with default settings
        //        using (var w = new StreamWriter(discoverySettingsFullPath))
        //        {
        //            w.WriteLine(WriteSettingField("AutoDiscoveryDurationInSeconds", settings.AutoDiscoveryDurationInSeconds));
        //            w.WriteLine(WriteSettingField("AutoDiscoveryListenerIntervalInSeconds", settings.AutoDiscoveryListenerIntervalInSeconds));
        //            w.WriteLine(WriteSettingField("AutoDiscoveryMulticastAddress", settings.AutoDiscoveryMulticastAddress));
        //            w.WriteLine(WriteSettingField("AutoDiscoveryMulticastPort", settings.AutoDiscoveryMulticastPort));
        //            w.WriteLine(WriteSettingField("AutoDiscoveryPollingIntervalInSeconds", settings.AutoDiscoveryPollingIntervalInSeconds));
        //            w.WriteLine(WriteSettingField("AutoDiscoveryUdpLocalClientTimeoutInSeconds", settings.AutoDiscoveryUdpLocalClientTimeoutInSeconds));
        //            w.WriteLine(WriteSettingField("DataSourceAutoDiscoveryLocalPort", settings.DataSourceAutoDiscoveryLocalPort));
        //            w.Flush();
        //        }
        //    }

        //    // Read settings from file
        //    using (var r = new StreamReader(discoverySettingsFullPath))
        //    {
        //        while (!r.EndOfStream)
        //        {
        //            string settingLine = r.ReadLine();
        //            if (string.IsNullOrEmpty(settingLine))
        //            {
        //                continue;
        //            }

        //            string[] settingFields = settingLine.Split(':');
        //            if (settingFields.Length != 2)
        //            {
        //                // Ignore and read next line, if any
        //                continue;
        //            }

        //            if (m_Logger != null && m_Logger.IsDebugEnabled)
        //            {
        //                m_Logger.Debug("LoadAutoDiscoverySettingsFromFile - Reading {0}", settingFields[0]);
        //            }

        //            switch (settingFields[0].Trim())
        //            {
        //                case "AutoDiscoveryDurationInSeconds":
        //                    settings.AutoDiscoveryDurationInSeconds = GetIntSettingsFromConfigFile(settingFields[1]) ?? AutoDiscoveryDefaultSettings.AutoDiscoveryDurationInSeconds;
        //                    break;

        //                case "AutoDiscoveryListenerIntervalInSeconds":
        //                    settings.AutoDiscoveryListenerIntervalInSeconds = GetIntSettingsFromConfigFile(settingFields[1]) ?? AutoDiscoveryDefaultSettings.AutoDiscoveryListenerIntervalInSeconds;
        //                    break;

        //                case "AutoDiscoveryMulticastAddress":
        //                    settings.AutoDiscoveryMulticastAddress = GetIpSettingsFromConfigFile(settingFields[1]) ?? IPAddress.Parse(AutoDiscoveryDefaultSettings.AutoDiscoveryMulticastAddress);
        //                    break;

        //                case "AutoDiscoveryMulticastPort":
        //                    settings.AutoDiscoveryMulticastPort = GetIntSettingsFromConfigFile(settingFields[1]) ?? AutoDiscoveryDefaultSettings.AutoDiscoveryMulticastPort;
        //                    break;

        //                case "AutoDiscoveryPollingIntervalInSeconds":
        //                    settings.AutoDiscoveryPollingIntervalInSeconds = GetIntSettingsFromConfigFile(settingFields[1]) ?? AutoDiscoveryDefaultSettings.AutoDiscoveryPollingIntervalInSeconds;
        //                    break;

        //                case "AutoDiscoveryUdpLocalClientTimeoutInSeconds":
        //                    settings.AutoDiscoveryUdpLocalClientTimeoutInSeconds = GetIntSettingsFromConfigFile(settingFields[1]) ?? AutoDiscoveryDefaultSettings.AutoDiscoveryUdpLocalClientTimeoutInSeconds;
        //                    break;

        //                case "DataSourceAutoDiscoveryLocalPort":
        //                    settings.DataSourceAutoDiscoveryLocalPort = GetIntSettingsFromConfigFile(settingFields[1]) ?? AutoDiscoveryDefaultSettings.DataSourceAutoDiscoveryLocalPort;
        //                    break;

        //                default:
        //                    // Ignore and read next line, if any
        //                    continue;
        //            }
        //        }
        //    }
            
        //    return settings;
        //}

        //private string WriteSettingField(string name, object value)
        //{
        //    return String.Format("{0}: {1}", name, value);
        //}

        //private int? GetIntSettingsFromConfigFile(string value)
        //{
        //    int tempVal;
        //    if (int.TryParse(value.Trim(), out tempVal))
        //    {
        //        return tempVal;
        //    }

        //    return null;
        //}

        //private string GetIpSettingsFromConfigFile(string value)
        //{
        //    //IPAddress tempVal;
        //    //if (IPAddress.TryParse(value.Trim(), out tempVal))
        //    //{
        //    //    return tempVal;
        //    //}

        //    return value;
        //}

        #endregion
    }
}

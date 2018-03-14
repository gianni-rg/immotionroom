namespace ImmotionAR.ImmotionRoom.DataSourceService
{
    using System;
    using System.IO;
    using System.Windows;
    using ControlApi;
    using ControlApi.Interfaces;
    using Hardcodet.Wpf.TaskbarNotification;
    using Helpers;
    using Helpers.Messaging;
    using Interfaces;
    using Logger;
    using Logger.Log4Net;
    using Model;
    using ViewModels;
    using System.Text;

    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon m_NotifyIcon;
        private ILogger m_Logger;
        private IMessenger m_Messenger;
        private IDataSourceService m_DataSourceService;
        private IDataSourceControlApiServer m_DataSourceServiceControlApiServer;
        private bool m_DiagnosticMode;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherUnhandledException += App_DispatcherUnhandledException;

            Directory.SetCurrentDirectory(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));

#if DEBUG
            m_DiagnosticMode = true;
#else
            if (e.Args.Length > 0)
            {
                if (e.Args[0] == "/diagnostic")
                {
                    m_DiagnosticMode = true;
                }
            }
#endif       

            LoggerService.LoggerFactory = new LoggerFactory();
            LoggerService.Configuration = Configuration.LoadLoggerConfiguration(m_DiagnosticMode);

            HelpersPlatformSetup();

            m_Logger = LoggerService.GetLogger<App>();

            if (m_Logger.IsInfoEnabled)
            {
                m_Logger.Info("ImmotionAR ImmotionRoom DataSource Service - Version: " + AppVersions.RetrieveExecutableVersion());
                m_Logger.Info("Copyright (C) 2014-2016 ImmotionAR. All rights reserved");
            }

            if (m_DiagnosticMode)
            {
                if (m_Logger.IsWarnEnabled)
                {
                    m_Logger.Warn("DIAGNOSTIC MODE ENABLED");
                }
            }

            var currentConfiguration = Configuration.LoadConfigurationFromAppConfig();
            var knownTrackingService = Configuration.LoadConfigurationItemFromFile<TrackingServiceInfo>("TrackingService.txt");
            
            var dataSourceServiceFactory = new DataSourceServiceFactory();

            m_DataSourceService = dataSourceServiceFactory.Create(currentConfiguration, knownTrackingService);
            m_DataSourceServiceControlApiServer = new DataSourceControlApiServer(currentConfiguration);

            // Create the Notify Icon (it's a resource declared in NotifyIconResources.xaml)

            m_Messenger = new Messenger();
            var iconViewModel = new NotifyIconViewModel(m_Messenger, m_DataSourceService, m_DataSourceServiceControlApiServer);

            m_NotifyIcon = FindResource("NotifyIcon") as TaskbarIcon;
            if (m_NotifyIcon != null)
            {
                m_NotifyIcon.DataContext = iconViewModel;
            }

            iconViewModel.ToolTipText = string.Format("ImmotionRoom DataSource ({0})", currentConfiguration.InstanceId);

            iconViewModel.IsDiagnosticMode = m_DiagnosticMode;

            if (!m_DataSourceServiceControlApiServer.Start())
            {
                iconViewModel.Icon = iconViewModel.ErrorIcon;
                iconViewModel.ToolTipText = "Network Configuration error. Please check your settings.";
                //m_NotifyIcon.IconSource = new BitmapImage(new Uri("pack://application:,,,/Resources/DataSourceServiceIcon_Error.ico", UriKind.Absolute));
                return;
            }

            await m_DataSourceService.Start();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (m_Logger.IsInfoEnabled)
            {
                m_Logger.Info("Stopping services...");
            }

            try
            {
                if(m_DataSourceServiceControlApiServer != null)
                {
                    m_DataSourceServiceControlApiServer.Stop();
                }
                
                if(m_DataSourceService != null)
                {
                    await m_DataSourceService.Stop();
                }
                
                if (m_Logger.IsInfoEnabled)
                {
                    m_Logger.Info("Services stopped");
                }
            }
            catch (Exception ex)
            {
                if (m_Logger.IsErrorEnabled)
                {
                    m_Logger.Error(ex, "Unable to stop services");
                }
            }

            // The icon would clean up automatically, but this is cleaner
            if(m_NotifyIcon != null)
            {
                m_NotifyIcon.Dispose();
            }

            DispatcherUnhandledException -= App_DispatcherUnhandledException;

            base.OnExit(e);
        }

        private static void HelpersPlatformSetup()
        {
            AppVersions.PlatformHelpers = new HelpersAppVersions();
            NetworkTools.PlatformHelpers = new HelpersNetworkTools();
            SystemManagement.PlatformHelpers = new HelpersSystemManagement();
            RegistryTools.PlatformHelpers = new HelpersRegistry();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (m_Logger != null && m_Logger.IsErrorEnabled)
            {
                m_Logger.Error(e.Exception, "App_DispatcherUnhandledException: {0}", e.Exception.Message);
            }
        }
    }
}

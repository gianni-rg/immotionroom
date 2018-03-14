namespace ImmotionAR.ImmotionRoom.TrackingService
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
	
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon m_NotifyIcon;
        private ILogger m_Logger;
        private IMessenger m_Messenger;
        private ITrackingService m_TrackingService;
        private ITrackingServiceControlApiServer m_TrackingServiceControlApiServer;
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
                m_Logger.Info("ImmotionAR ImmotionRoom Tracking Service - Version: " + AppVersions.RetrieveExecutableVersion());
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
            var dataSources = Configuration.LoadConfigurationItemFromFile<DataSourceCollection>("DataSources.txt");

            var trackingServiceFactory = new TrackingServiceFactory();

            m_TrackingService = trackingServiceFactory.Create(currentConfiguration, dataSources);
            m_TrackingServiceControlApiServer = new TrackingServiceControlApiServer(currentConfiguration);

            // Create the Notify Icon (it's a resource declared in NotifyIconResources.xaml)

            m_Messenger = new Messenger();
            var iconViewModel = new NotifyIconViewModel(m_Messenger, m_TrackingService, m_TrackingServiceControlApiServer);

            m_NotifyIcon = FindResource("NotifyIcon") as TaskbarIcon;
            if (m_NotifyIcon != null)
            {
                m_NotifyIcon.DataContext = iconViewModel;
            }

            iconViewModel.ToolTipText = string.Format("ImmotionRoom Tracking Service ({0})", currentConfiguration.InstanceId);

            iconViewModel.IsDiagnosticMode = m_DiagnosticMode;

            if (!m_TrackingServiceControlApiServer.Start())
            {
                iconViewModel.Icon = iconViewModel.ErrorIcon;
                iconViewModel.ToolTipText = "Network Configuration error. Please check your settings.";
                //m_NotifyIcon.IconSource = new BitmapImage(new Uri("pack://application:,,,/Resources/TrackingServiceIcon_Error.ico", UriKind.Absolute));
                return;
            }

            await m_TrackingService.Start();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (m_Logger.IsInfoEnabled)
            {
                m_Logger.Info("Stopping services...");
            }

            try
            {
                if(m_TrackingServiceControlApiServer != null)
                {
                	m_TrackingServiceControlApiServer.Stop();
                }
                
                if(m_TrackingService != null)
                {
                	await m_TrackingService.Stop();
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

namespace ImmotionAR.ImmotionRoom.TrackingService.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;
    using ControlApi.Interfaces;
    using Helpers;
    using Helpers.Messaging;
    using Interfaces;
    using Model;
    using MVVM;
    using Views;

    public sealed class StatusWindowViewModel : BaseViewModel
    {
        #region Constants

#if DEBUG
        private readonly string ServiceConfigToolApp = @"D:\ProjectsIAR\TBox_v3\Binaries_Release\TrackingService\ServiceConfigTool.exe";
#else
        private readonly string ServiceConfigToolApp = "ServiceConfigTool.exe";
#endif
        private readonly string HelpLink = "http://www.immotionar.com";
        private readonly string ServiceStatusIconOK = "/Resources/ok_icon.png";
        private readonly string ServiceStatusIconNotOK = "/Resources/no_icon.png";
        private readonly string ServiceStatusIconWarning = "/Resources/warning_icon.png";
        private readonly string ServiceStatusIconWaiting = "/Resources/hourglass_icon.png";

        #endregion

        #region Private fields

        private readonly IMessenger m_Messenger;
        private readonly ITrackingService m_TrackingService;
        private readonly ITrackingServiceControlApiServer m_TrackingServiceControlApiServer;
        private string m_ServiceStatusName;
        private TrackingServiceState m_ServiceStatus;
        private string m_ServiceStatusIcon;
        private string m_ServiceStatusDescription;
        private bool m_IsDiagnosticMode;
        private ObservableCollection<DataSourceItem> m_DataSources;

        #endregion

        #region Properties

        public ObservableCollection<DataSourceItem> DataSources
        {
            get { return m_DataSources; }
            private set { Set(ref m_DataSources, value); }
        }

        public string ServiceStatus
        {
            get { return m_ServiceStatusName; }
            private set { Set(ref m_ServiceStatusName, value); }
        }

        public string ServiceStatusIcon
        {
            get { return m_ServiceStatusIcon; }
            private set { Set(ref m_ServiceStatusIcon, value); }
        }

        public string ServiceStatusDescription
        {
            get { return m_ServiceStatusDescription; }
            private set { Set(ref m_ServiceStatusDescription, value); }
        }

        public string ServiceVersion
        {
            get { return AppVersions.RetrieveExecutableVersion(); }
        }

        public string ServiceId
        {
            get { return m_TrackingService.InstanceID; }
        }

        public bool IsDiagnosticMode
        {
            get { return m_IsDiagnosticMode; }
            set { Set(ref m_IsDiagnosticMode, value); }
        }

        #endregion

        #region Constructor

        public StatusWindowViewModel(IMessenger messenger, ITrackingService trackingService, ITrackingServiceControlApiServer trackingServiceControlApiServer, bool isDiagnosticMode)
        {
            m_Messenger = messenger;
            m_TrackingService = trackingService;
            m_TrackingServiceControlApiServer = trackingServiceControlApiServer;
            m_IsDiagnosticMode = isDiagnosticMode;

            DataSources = new ObservableCollection<DataSourceItem>();

            UpdateServiceStatusUI(trackingService.Status, TrackingServiceStateErrors.Unknown);
            UpdateDataSourceStatusUI(null, false);

            m_TrackingService.StatusChanged += TrackingService_StatusChanged;
            m_TrackingService.DataSourceStatusChanged += TrackingService_DataSourceStatusChanged;
        }

        #endregion

        #region Commands

        public ICommand ServiceSettingsCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => File.Exists(ServiceConfigToolApp),
                    CommandAction = () =>
                    {
                        try
                        {
                            Process.Start(ServiceConfigToolApp);
                        }
                        catch (Win32Exception)
                        {
                            // User didn't authorize UAC. Ignore
                        }
                    }
                };
            }
        }

        public ICommand AboutCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => true,
                    CommandAction = () =>
                    {
                        // Show About View
                        var aboutWinVM = new AboutWindowViewModel();
                        var aboutWin = new AboutWindow();
                        aboutWin.DataContext = aboutWinVM;
                        aboutWin.ShowDialog();
                    }
                };
            }
        }

        public ICommand ExitCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => true,
                    CommandAction = () => { Application.Current.Shutdown(); }
                };
            }
        }

        public ICommand HelpDocumentationCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => true,
                    CommandAction = () => { Process.Start(HelpLink); }
                };
            }
        }

        public ICommand HelpWebsiteCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => true,
                    CommandAction = () => { Process.Start(HelpLink); }
                };
            }
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            m_TrackingService.StatusChanged -= TrackingService_StatusChanged;
            m_TrackingService.DataSourceStatusChanged -= TrackingService_DataSourceStatusChanged;
        }

        #endregion

        #region Private methods

        private void TrackingService_StatusChanged(object sender, TrackingServiceStatusChangedEventArgs e)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                UpdateServiceStatusUI(e.Status, e.Error);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => { UpdateServiceStatusUI(e.Status, e.Error); }));
            }
        }

        private void TrackingService_DataSourceStatusChanged(object sender, DataSourceStatusChangedEventArgs e)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                UpdateDataSourceStatusUI(e.DataSourceId, e.IsActive);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => { UpdateDataSourceStatusUI(e.DataSourceId, e.IsActive); }));
            }
        }

        private void UpdateServiceStatusUI(TrackingServiceState status, TrackingServiceStateErrors error)
        {
            var wasInAutoDiscovery = m_ServiceStatus == TrackingServiceState.AutoDiscovery && status != TrackingServiceState.AutoDiscovery;
            var enteredAutoDiscovery = m_ServiceStatus != TrackingServiceState.AutoDiscovery && status == TrackingServiceState.AutoDiscovery;

            m_ServiceStatus = status;

            if (wasInAutoDiscovery)
            {
                // A reconfig has happened? Refresh whole collection
                DataSources = new ObservableCollection<DataSourceItem>(m_TrackingService.KnownDataSources.Values.Select(dsInfo => new DataSourceItem {Name = dsInfo.Id, StatusIcon = ServiceStatusIconWaiting, StatusDescription = "Refreshing... please wait"}));
            }
            else if (enteredAutoDiscovery)
            {
                // A reconfig may happen... prepare to refresh whole collection
                DataSources = new ObservableCollection<DataSourceItem>(new List<DataSourceItem> {new DataSourceItem {Name = "Looking for DataSources... please wait", StatusIcon = ServiceStatusIconWaiting, StatusDescription = "Looking for DataSources... please wait"}});
            }
            else if (error == TrackingServiceStateErrors.Unknown) // Initial data retrieval
            {
                DataSources = new ObservableCollection<DataSourceItem>(m_TrackingService.KnownDataSources.Values.Select(dsInfo => new DataSourceItem {Name = dsInfo.Id, StatusIcon = ServiceStatusIconWaiting, StatusDescription = "Refreshing... please wait"}));
            }

            if (status == TrackingServiceState.Error)
            {
                ServiceStatus = "Error";
                ServiceStatusIcon = ServiceStatusIconNotOK;
                ServiceStatusDescription = string.Format("Status: {0}", error);
            }
            else if (status == TrackingServiceState.Warning)
            {
                ServiceStatusIcon = ServiceStatusIconWarning;
                if (error == TrackingServiceStateErrors.NotCalibrated)
                {
                    ServiceStatus = "System is not calibrated";
                    ServiceStatusDescription = string.Format("Status: {0}", ServiceStatus);
                }
                else if (error == TrackingServiceStateErrors.NotConfigured)
                {
                    ServiceStatus = "Master DataSource is not configured";
                    ServiceStatusDescription = string.Format("Status: {0}", ServiceStatus);
                }
            }
            else if (status == TrackingServiceState.Unknown)
            {
                ServiceStatusIcon = ServiceStatusIconNotOK;
                ServiceStatus = "Check configuration";
                ServiceStatusDescription = "Status: configuration error";
            }
            else
            {
                ServiceStatus = "OK";
                ServiceStatusIcon = ServiceStatusIconOK;
                ServiceStatusDescription = "Status: OK";
            }
        }

        private void UpdateDataSourceStatusUI(string dataSourceId, bool isActive)
        {
            var currentDs = DataSources.FirstOrDefault(ds => ds.Name == dataSourceId);
            if (currentDs == null)
            {
                return;
            }

            if (isActive)
            {
                currentDs.StatusIcon = ServiceStatusIconOK;
                currentDs.StatusDescription = "Status: OK";
            }
            else
            {
                currentDs.StatusIcon = ServiceStatusIconNotOK;
                currentDs.StatusDescription = "Status: Not reachable";
            }

            // Show warning if one DataSource is not reachable ?
            //foreach (var ds in DataSources)
            //{
            //    if (!ds.StatusDescription.StartsWith("Refreshing") && !ds.StatusDescription.EndsWith("OK"))
            //    {
            //        ServiceStatus = "Check DataSources";
            //        ServiceStatusIcon = ServiceStatusIconWarning;
            //        ServiceStatusDescription = "Status: Check DataSources";
            //        break;
            //    }
            //}
        }

        #endregion
    }
}

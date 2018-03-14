namespace ImmotionAR.ImmotionRoom.DataSourceService.ViewModels
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
        private readonly string ServiceConfigToolApp = @"D:\ProjectsIAR\TBox_v3\Binaries_Release\DataSourceService\ServiceConfigTool.exe";
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
        private readonly IDataSourceService m_DataSourceService;
        private readonly IDataSourceControlApiServer m_DataSourceControlApiServer;
        private string m_ServiceStatusName;
        private DataSourceState m_ServiceStatus;
        private string m_ServiceStatusIcon;
        private string m_ServiceStatusDescription;
        private bool m_IsDiagnosticMode;
        private ObservableCollection<TrackingServiceItem> m_TrackingServices;

        #endregion

        #region Properties

        public ObservableCollection<TrackingServiceItem> TrackingServices
        {
            get { return m_TrackingServices; }
            private set { Set(ref m_TrackingServices, value);  }
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
            get { return m_DataSourceService.InstanceID; }
        }

        public bool IsDiagnosticMode
        {
            get { return m_IsDiagnosticMode; }
            set { Set(ref m_IsDiagnosticMode, value); }
        }

        #endregion

        #region Constructor

        public StatusWindowViewModel(IMessenger messenger, IDataSourceService dataSourceService, IDataSourceControlApiServer dataSourceControlApiServer, bool isDiagnosticMode)
        {
            m_Messenger = messenger;
            m_DataSourceService = dataSourceService;
            m_DataSourceControlApiServer = dataSourceControlApiServer;
            m_IsDiagnosticMode = isDiagnosticMode;
            
            TrackingServices = new ObservableCollection<TrackingServiceItem>();

            UpdateServiceStatusUI(dataSourceService.Status, DataSourceStateErrors.Unknown);
            UpdateTrackingServiceStatusUI(null, false);

            m_DataSourceService.StatusChanged += DataSourceService_StatusChanged;
            m_DataSourceService.TrackingServiceStatusChanged += DataSourceService_TrackingServiceStatusChanged;
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
            m_DataSourceService.StatusChanged -= DataSourceService_StatusChanged;
            m_DataSourceService.TrackingServiceStatusChanged -= DataSourceService_TrackingServiceStatusChanged;
        }

        #endregion

        #region Private methods

        private void DataSourceService_StatusChanged(object sender, DataSourceServiceStatusChangedEventArgs e)
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

        private void DataSourceService_TrackingServiceStatusChanged(object sender, TrackingServiceStatusChangedEventArgs e)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                UpdateTrackingServiceStatusUI(e.TrackingServiceId, e.IsActive);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => { UpdateTrackingServiceStatusUI(e.TrackingServiceId, e.IsActive); }));
            }
        }

        private void UpdateServiceStatusUI(DataSourceState status, DataSourceStateErrors error)
        {
            bool wasInAutoDiscovery = ServiceStatus == DataSourceState.AutoDiscovery.ToString() && status != DataSourceState.AutoDiscovery;
            bool enteredAutoDiscovery = ServiceStatus != DataSourceState.AutoDiscovery.ToString() && status == DataSourceState.AutoDiscovery;

            m_ServiceStatus = status;

            if (wasInAutoDiscovery)
            {
                // A reconfig has happened? Refresh whole collection
                TrackingServices = new ObservableCollection<TrackingServiceItem>(m_DataSourceService.KnownTrackingServices.Values.Select(dsInfo => new TrackingServiceItem() { Name = dsInfo.Id, StatusIcon = ServiceStatusIconWaiting, StatusDescription = "Refreshing... please wait" }));
            }
            else if (enteredAutoDiscovery)
            {
                // A reconfig may happen... prepare to refresh whole collection
                TrackingServices = new ObservableCollection<TrackingServiceItem>(new List<TrackingServiceItem> { new TrackingServiceItem { Name = "Looking for Tracking Service... please wait", StatusIcon = ServiceStatusIconWaiting, StatusDescription = "Looking for Tracking Service... please wait" } });
            }
            else if (error == DataSourceStateErrors.Unknown) // Initial data retrieval
            {
                TrackingServices = new ObservableCollection<TrackingServiceItem>(m_DataSourceService.KnownTrackingServices.Values.Select(tsInfo => new TrackingServiceItem { Name = tsInfo.Id, StatusIcon = ServiceStatusIconWaiting, StatusDescription = "Refreshing... please wait" }));
            }

            if (status == DataSourceState.Error)
            {
                ServiceStatus = "Error";
                ServiceStatusIcon = ServiceStatusIconNotOK;
                ServiceStatusDescription = string.Format("Status: {0}", error);
            }
            else if (status == DataSourceState.Warning)
            {
                ServiceStatusIcon = ServiceStatusIconWarning;
                if (error == DataSourceStateErrors.SensorError)
                {
                    ServiceStatus = "Tracking Sensor not available";
                    ServiceStatusDescription = string.Format("Status: {0}", ServiceStatus);
                }
            }
            else if (status == DataSourceState.Unknown)
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

        private void UpdateTrackingServiceStatusUI(string dataSourceId, bool isActive)
        {
            var currentTs = TrackingServices.FirstOrDefault(ts => ts.Name == dataSourceId);
            if (currentTs == null)
            {
                return;
            }

            if (isActive)
            {
                currentTs.StatusIcon = ServiceStatusIconOK;
                currentTs.StatusDescription = "Status: OK";
            }
            else
            {
                currentTs.StatusIcon = ServiceStatusIconNotOK;
                currentTs.StatusDescription = "Status: Not reachable";
            }

            // Show warning if one TrackingService is not reachable ?
            //foreach (var ts in TrackingServices)
            //{
            //    if (!ts.StatusDescription.StartsWith("Refreshing") && !ts.StatusDescription.EndsWith("OK"))
            //    {
            //        ServiceStatus = "Check Tracking Service";
            //        ServiceStatusIcon = ServiceStatusIconWarning;
            //        ServiceStatusDescription = "Status: Check Tracking Service";
            //        break;
            //    }
            //}
        }

        #endregion
    }
}

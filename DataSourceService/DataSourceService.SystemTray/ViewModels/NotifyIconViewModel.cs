namespace ImmotionAR.ImmotionRoom.DataSourceService.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;
    using ControlApi.Interfaces;
    using Helpers.Messaging;
    using Interfaces;
    using Model;
    using MVVM;
    using Views;

    public sealed class NotifyIconViewModel : BaseViewModel
    {
        #region Constants
        public readonly string ErrorIcon = "/Resources/DataSourceServiceIcon_Error.ico";
        public readonly string OkIcon = "/Resources/DataSourceServiceIcon_OK.ico";
        #endregion

        #region Private fields
        private readonly IMessenger m_Messenger;
        private readonly IDataSourceService m_DataSourceService;
        private readonly IDataSourceControlApiServer m_DataSourceServiceControlApiServer;
        private Window m_LogWindow;

        private string m_Icon;
        private string m_ToolTipText;
        private bool m_IsDiagnosticMode;
        private ObservableCollection<TrackingServiceItem> m_TrackingServices;
        private DataSourceState m_ServiceStatus;
        #endregion

        #region Properties

        public string ToolTipText
        {
            get { return m_ToolTipText; }
            set { Set(ref m_ToolTipText, value); }
        }

        public string Icon
        {
            get { return m_Icon; }
            set { Set(ref m_Icon, value); }
        }



        public bool IsDiagnosticMode
        {
            get { return m_IsDiagnosticMode; }
            set { Set(ref m_IsDiagnosticMode, value); }
        }

        public ObservableCollection<TrackingServiceItem> TrackingServices
        {
            get { return m_TrackingServices; }
            private set { Set(ref m_TrackingServices, value); }
        }
        #endregion

        #region Constructor

        public NotifyIconViewModel(IMessenger messenger, IDataSourceService dataSourceService, IDataSourceControlApiServer dataSourceServiceControlApiServer)
        {
            Icon = OkIcon;
            ToolTipText = string.Format("ImmotionRoom DataSource Service ({0})", dataSourceService.InstanceID);

            m_Messenger = messenger;
            m_DataSourceService = dataSourceService;
            m_DataSourceServiceControlApiServer = dataSourceServiceControlApiServer;

            m_Messenger.Register<ServiceStatusMessage>(this, HandleServiceStatusMessage);

            TrackingServices = new ObservableCollection<TrackingServiceItem>();
            UpdateServiceStatusUI(dataSourceService.Status, DataSourceStateErrors.Unknown);
            UpdateDataSourceStatusUI(null, false);

            m_DataSourceService.StatusChanged += TrackingService_StatusChanged;
            m_DataSourceService.TrackingServiceStatusChanged += TrackingService_DataSourceStatusChanged;
        }

        #endregion

        #region Commands

        public ICommand ShowStatusWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => true,
                    CommandAction = () =>
                    {
                        if (Application.Current.MainWindow == null || Application.Current.MainWindow == m_LogWindow)
                        {
                            Application.Current.MainWindow = new StatusWindow(m_Messenger, m_DataSourceService, m_DataSourceServiceControlApiServer, m_IsDiagnosticMode);
                        }

                        Application.Current.MainWindow.Show();

                        if (Application.Current.MainWindow.WindowState == WindowState.Minimized)
                            Application.Current.MainWindow.WindowState = WindowState.Normal;
                        else
                        {
                            Application.Current.MainWindow.Activate();
                        }
                    }
                };
            }
        }
  
  	    public ICommand ExitApplicationCommand
        {
            get { return new DelegateCommand {CommandAction = () => Application.Current.Shutdown()}; }
        }

        public ICommand ShowLogWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => m_IsDiagnosticMode,
                    CommandAction = () =>
                    {
                        if (m_LogWindow == null)
                        {
                            m_LogWindow = new LogVisualizer();
                            m_LogWindow.Closed += LogWindow_Closed;
                        }

                        m_LogWindow.Show();

                        if (m_LogWindow.WindowState == WindowState.Minimized)
                            m_LogWindow.WindowState = WindowState.Normal;
                        else
                        {
                            m_LogWindow.Activate();
                        }
                    }
                };
            }
        }
        #endregion

        #region Private methods
        private void LogWindow_Closed(object sender, EventArgs e)
        {
            if (m_LogWindow != null)
            {
                m_LogWindow.Closed -= LogWindow_Closed;
                m_LogWindow = null;
            }
        }

        private void HandleServiceStatusMessage(ServiceStatusMessage msg)
        {
            if (msg.Status != DataSourceState.Error && msg.Status != DataSourceState.Warning)
            {
                Icon = OkIcon;
            }
            else
            {
                Icon = ErrorIcon;
            }
        }

        private void TrackingService_StatusChanged(object sender, DataSourceServiceStatusChangedEventArgs e)
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

        private void TrackingService_DataSourceStatusChanged(object sender, TrackingServiceStatusChangedEventArgs e)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                UpdateDataSourceStatusUI(e.TrackingServiceId, e.IsActive);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => { UpdateDataSourceStatusUI(e.TrackingServiceId, e.IsActive); }));
            }
        }

        private void UpdateServiceStatusUI(DataSourceState status, DataSourceStateErrors error)
        {
            var wasInAutoDiscovery = m_ServiceStatus == DataSourceState.AutoDiscovery && status != DataSourceState.AutoDiscovery;
            var enteredAutoDiscovery = m_ServiceStatus != DataSourceState.AutoDiscovery && status == DataSourceState.AutoDiscovery;

            if (wasInAutoDiscovery)
            {
                // A reconfig has happened? Refresh whole collection
                TrackingServices.Clear();
            }
            else if (enteredAutoDiscovery)
            {
                // A reconfig may happen... prepare to refresh whole collection
                TrackingServices.Clear();
            }
            else if (error == DataSourceStateErrors.Unknown) // Initial data retrieval
            {
                TrackingServices = new ObservableCollection<TrackingServiceItem>(m_DataSourceService.KnownTrackingServices.Values.Select(tsInfo => new TrackingServiceItem { Name = tsInfo.Id, StatusIcon = null, StatusDescription = "Refreshing... please wait" }));
            }

            m_ServiceStatus = status;

            if (status != DataSourceState.Error && status != DataSourceState.Warning)
            {
                Icon = OkIcon;
            }
            else
            {
                Icon = ErrorIcon;
            }
        }

        private void UpdateDataSourceStatusUI(string trackingServiceId, bool isActive)
        {
            var currentTs = TrackingServices.FirstOrDefault(ts => ts.Name == trackingServiceId);
            if (currentTs == null)
            {
                return;
            }

            if (isActive)
            {
                currentTs.StatusDescription = "Status: OK";
            }
            else
            {
                currentTs.StatusDescription = "Status: Not reachable";
            }

            if (m_DataSourceService.Status != DataSourceState.Error && m_DataSourceService.Status != DataSourceState.Warning)
            {
                Icon = OkIcon;
            }
            else
            {
                Icon = ErrorIcon;
                return;
            }

            foreach (var ts in TrackingServices)
            {
                if (!ts.StatusDescription.StartsWith("Refreshing") && !ts.StatusDescription.EndsWith("OK"))
                {
                    Icon = ErrorIcon;
                    break;
                }
            }
        }
        #endregion
    }
}

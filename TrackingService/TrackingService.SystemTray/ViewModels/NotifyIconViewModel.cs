namespace ImmotionAR.ImmotionRoom.TrackingService.ViewModels
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
        public readonly string ErrorIcon = "/Resources/TrackingServiceIcon_Error.ico";
        public readonly string OkIcon = "/Resources/TrackingServiceIcon_OK.ico";
        #endregion

        #region Private fields
        private readonly IMessenger m_Messenger;
        private readonly ITrackingService m_TrackingService;
        private readonly ITrackingServiceControlApiServer m_TrackingServiceControlApiServer;
        private Window m_LogWindow;

        private string m_Icon;
        private string m_ToolTipText;
        private bool m_IsDiagnosticMode;
        private ObservableCollection<DataSourceItem> m_DataSources;
        private TrackingServiceState m_ServiceStatus;
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

        public ObservableCollection<DataSourceItem> DataSources
        {
            get { return m_DataSources; }
            private set { Set(ref m_DataSources, value); }
        }
        #endregion

        #region Constructor

        public NotifyIconViewModel(IMessenger messenger, ITrackingService trackingService, ITrackingServiceControlApiServer trackingServiceControlApiServer)
        {
            Icon = OkIcon;
            ToolTipText = string.Format("ImmotionRoom Tracking Service ({0})", trackingService.InstanceID);

            m_Messenger = messenger;
            m_TrackingService = trackingService;
            m_TrackingServiceControlApiServer = trackingServiceControlApiServer;
            
            m_Messenger.Register<ServiceStatusMessage>(this, HandleServiceStatusMessage);

            DataSources = new ObservableCollection<DataSourceItem>();
            UpdateServiceStatusUI(trackingService.Status, TrackingServiceStateErrors.Unknown);
            UpdateDataSourceStatusUI(null, false);

            m_TrackingService.StatusChanged += TrackingService_StatusChanged;
            m_TrackingService.DataSourceStatusChanged += TrackingService_DataSourceStatusChanged;
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
                            Application.Current.MainWindow = new StatusWindow(m_Messenger, m_TrackingService, m_TrackingServiceControlApiServer, m_IsDiagnosticMode);
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
            if (msg.Status != TrackingServiceState.Error && msg.Status != TrackingServiceState.Warning)
            {
                Icon = OkIcon;
            }
            else
            {
                Icon = ErrorIcon;
            }
        }

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

            if (wasInAutoDiscovery)
            {
                // A reconfig has happened? Refresh whole collection
                DataSources.Clear();
            }
            else if (enteredAutoDiscovery)
            {
                // A reconfig may happen... prepare to refresh whole collection
                DataSources.Clear();
            }
            else if (error == TrackingServiceStateErrors.Unknown) // Initial data retrieval
            {
                DataSources = new ObservableCollection<DataSourceItem>(m_TrackingService.KnownDataSources.Values.Select(dsInfo => new DataSourceItem { Name = dsInfo.Id, StatusIcon = null, StatusDescription = "Refreshing... please wait" }));
            }

            m_ServiceStatus = status;

            if (status != TrackingServiceState.Error && status != TrackingServiceState.Warning)
            {
                Icon = OkIcon;
            }
            else
            {
                Icon = ErrorIcon;
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
                currentDs.StatusDescription = "Status: OK";
            }
            else
            {
                currentDs.StatusDescription = "Status: Not reachable";
            }

            if (m_TrackingService.Status != TrackingServiceState.Error && m_TrackingService.Status != TrackingServiceState.Warning)
            {
                Icon = OkIcon;
            }
            else
            {
                Icon = ErrorIcon;
                return;
            }

            foreach (var ds in DataSources)
            {
                if (!ds.StatusDescription.StartsWith("Refreshing") && !ds.StatusDescription.EndsWith("OK"))
                {
                    Icon = ErrorIcon;
                    break;
                }
            }
        }
        #endregion
    }
}

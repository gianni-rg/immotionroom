namespace ImmotionAR.ImmotionRoom.TrackingService.Views
{
    using System.ComponentModel;
    using System.Windows;
    using ControlApi.Interfaces;
    using Helpers.Messaging;
    using Interfaces;
    using Presentation.Helpers;
    using ViewModels;

    /// <summary>
    ///     Interaction logic for StatusWindow.xaml
    /// </summary>
    public partial class StatusWindow : Window
    {
        private StatusWindowViewModel ViewModel
        {
            get { return DataContext as StatusWindowViewModel; }
        }

        public StatusWindow(IMessenger messenger, ITrackingService trackingService, ITrackingServiceControlApiServer trackingServiceControlApiServer, bool isDiagnosticMode)
        {
            InitializeComponent();

            SetPosition();

            DataContext = new StatusWindowViewModel(messenger, trackingService, trackingServiceControlApiServer, isDiagnosticMode);
        }

        private void SetPosition()
        {
            var tb = new Taskbar();

            if (tb.Position == TaskbarPosition.Bottom)
            {
                Left = SystemParameters.PrimaryScreenWidth - Width - 15*ScaleFactor.GetScalingFactor();
                if (!tb.AutoHide)
                {
                    Top = SystemParameters.PrimaryScreenHeight - Height - tb.Size.Height;
                }
                else
                {
                    Top = SystemParameters.PrimaryScreenHeight - Height - 10*ScaleFactor.GetScalingFactor();
                }
            }
            else if (tb.Position == TaskbarPosition.Top)
            {
                Left = SystemParameters.PrimaryScreenWidth - Width - 10*ScaleFactor.GetScalingFactor();
                if (!tb.AutoHide)
                {
                    Top = tb.Size.Height + 10*ScaleFactor.GetScalingFactor();
                }
                else
                {
                    Top = 15*ScaleFactor.GetScalingFactor();
                }
            }
            else if (tb.Position == TaskbarPosition.Left)
            {
                Top = SystemParameters.PrimaryScreenHeight - Height - 10*ScaleFactor.GetScalingFactor();
                if (!tb.AutoHide)
                {
                    Left = tb.Size.Width;
                }
                else
                {
                    Left = 10*ScaleFactor.GetScalingFactor();
                }
            }
            else if (tb.Position == TaskbarPosition.Right)
            {
                Top = SystemParameters.PrimaryScreenHeight - Height - 10*ScaleFactor.GetScalingFactor();

                if (!tb.AutoHide)
                {
                    Left = SystemParameters.PrimaryScreenWidth - Width - tb.Size.Width;
                }
                else
                {
                    Left = SystemParameters.PrimaryScreenWidth - Width - 10*ScaleFactor.GetScalingFactor();
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            ViewModel.Dispose();
            base.OnClosing(e);
        }
    }
}

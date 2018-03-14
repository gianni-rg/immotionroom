namespace ImmotionAR.ImmotionRoom.DataSourceService.Views
{
    using System.ComponentModel;
    using System.Windows;
    using ViewModels;

    /// <summary>
    ///     Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        private AboutWindowViewModel ViewModel
        {
            get { return DataContext as AboutWindowViewModel; }
        }

        public AboutWindow()
        {
            InitializeComponent();
        }


        protected override void OnClosing(CancelEventArgs e)
        {
            ViewModel.Dispose();
            base.OnClosing(e);
        }

        private void OKButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

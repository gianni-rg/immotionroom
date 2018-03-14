namespace ImmotionAR.ImmotionRoom.TrackingService.Views
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;
    using System.Windows.Threading;
    using ViewModels;

    /// <summary>
    ///     Interaction logic for LogVisualizer.xaml
    /// </summary>
    public partial class LogVisualizer : Window
    {
        // http://www.thepicketts.org/2012/12/colour-your-log4net-events-in-your-richtextbox/
        // http://stackoverflow.com/questions/957441/richtextbox-wpf-does-not-have-string-property-text
        private readonly Dictionary<string, SolidColorBrush> m_LevelColors = new Dictionary<string, SolidColorBrush>();
        private readonly Dictionary<string, SolidColorBrush> m_LevelBackgroundColors = new Dictionary<string, SolidColorBrush>();

        private LogVisualizerViewModel ViewModel
        {
            get { return DataContext as LogVisualizerViewModel; }
        }

        public LogVisualizer()
        {
            InitializeComponent();

            DataContext = new LogVisualizerViewModel();

            // Add colours to correspond to given log levels
            m_LevelColors.Add("FATAL", new SolidColorBrush(Colors.White));
            m_LevelBackgroundColors.Add("FATAL", new SolidColorBrush(Colors.Red));
            m_LevelColors.Add("INFO", new SolidColorBrush(Colors.White));
            m_LevelBackgroundColors.Add("INFO", new SolidColorBrush(Colors.Transparent));
            m_LevelColors.Add("WARN", new SolidColorBrush(Colors.Yellow));
            m_LevelBackgroundColors.Add("WARN", new SolidColorBrush(Colors.Transparent));
            m_LevelColors.Add("ERROR", new SolidColorBrush(Colors.Red));
            m_LevelBackgroundColors.Add("ERROR", new SolidColorBrush(Colors.Transparent));
            m_LevelColors.Add("DEBUG", new SolidColorBrush(Colors.LightGray));
            m_LevelBackgroundColors.Add("DEBUG", new SolidColorBrush(Colors.Transparent));

            LogInfoTextBox.Document.Blocks.Clear();
            ViewModel.LogEntries.CollectionChanged += LogEntries_CollectionChanged;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            ViewModel.LogEntries.CollectionChanged -= LogEntries_CollectionChanged; 
            ViewModel.Dispose();
            base.OnClosing(e);
        }

        private void LogEntries_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null)
            {
                return;
            }

            var str = (from object l in e.NewItems select l.ToString()).ToList();
            UpdateLogTextbox(str);
        }

        private void UpdateLogTextbox(List<string> logEntries)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                AddAndFormatLogLines(logEntries);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { AddAndFormatLogLines(logEntries); }));
            }
        }

        private void AddAndFormatLogLines(List<string> logEntries)
        {
            foreach (var entry in logEntries)
            {
                var splittedLog = entry.Split('|');
                if (splittedLog.Length > 3)
                {
                    var tr = new TextRange(LogInfoTextBox.Document.ContentEnd, LogInfoTextBox.Document.ContentEnd);
                    tr.Text = entry;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, m_LevelColors[splittedLog[2].Trim()]);
                    tr.ApplyPropertyValue(TextElement.BackgroundProperty, m_LevelBackgroundColors[splittedLog[2].Trim()]);
                }
                else
                {
                    var tr = new TextRange(LogInfoTextBox.Document.ContentEnd, LogInfoTextBox.Document.ContentEnd);
                    tr.Text = entry;
                }
            }
            LogInfoTextBox.ScrollToEnd();
        }
    }
}

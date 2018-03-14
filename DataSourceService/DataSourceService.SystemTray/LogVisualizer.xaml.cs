namespace ImmotionAR.ImmotionRoom.DataSourceService
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;
    using System.Windows.Threading;
    using Logger;

    /// <summary>
    ///     Interaction logic for LogVisualizer.xaml
    /// </summary>
    public partial class LogVisualizer : Window
    {
        // http://www.thepicketts.org/2012/12/colour-your-log4net-events-in-your-richtextbox/
        // http://stackoverflow.com/questions/957441/richtextbox-wpf-does-not-have-string-property-text
        private Dictionary<string, SolidColorBrush> m_LevelColors = new Dictionary<string, SolidColorBrush>();

        private readonly ILogWatcher m_LogWatcher;

        public LogVisualizer(ILogWatcher logWatcher)
        {
            InitializeComponent();

            // Add colours to correspond to given log levels
            m_LevelColors.Add("FATAL", new SolidColorBrush(Colors.Red));
            m_LevelColors.Add("INFO", new SolidColorBrush(Colors.Green));
            m_LevelColors.Add("WARN", new SolidColorBrush(Colors.Yellow));
            m_LevelColors.Add("ERROR", new SolidColorBrush(Colors.Red));
            m_LevelColors.Add("DEBUG", new SolidColorBrush(Colors.Blue));

            LogInfoTextBox.Document.Blocks.Clear();

            m_LogWatcher = logWatcher;
            m_LogWatcher.Updated += LogWatcher_Updated;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            m_LogWatcher.Updated -= LogWatcher_Updated;

            base.OnClosing(e);
        }

        private void LogWatcher_Updated(object sender, EventArgs e)
        {
            UpdateLogTextbox(m_LogWatcher.LogContent);
        }

        

        private void UpdateLogTextbox(List<string> logEntries)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                AddAndFormatLogLines(logEntries);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    AddAndFormatLogLines(logEntries);
                }));
            }
        }

        private void AddAndFormatLogLines(List<string> logEntries)
        {
            foreach (var entry in logEntries)
            {
                var splittedLog = entry.Split('|');
                if (splittedLog.Length > 3)
                {
                    TextRange tr = new TextRange(LogInfoTextBox.Document.ContentEnd, LogInfoTextBox.Document.ContentEnd);
                    tr.Text = entry;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, m_LevelColors[splittedLog[2].Trim()]);
                }
                else
                {
                    TextRange tr = new TextRange(LogInfoTextBox.Document.ContentEnd, LogInfoTextBox.Document.ContentEnd);
                    tr.Text = entry;
                }
            }
        }
    }
}

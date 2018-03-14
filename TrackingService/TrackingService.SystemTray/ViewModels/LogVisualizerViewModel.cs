namespace ImmotionAR.ImmotionRoom.TrackingService.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows.Input;
    using Logger;
    using Logger.Log4Net;
    using MVVM;

    public sealed class LogVisualizerViewModel : BaseViewModel
    {
        #region Private fields

        private readonly LoggerConfiguration m_LoggerConfiguration;

        #endregion

        #region Properties

        private ObservableCollection<string> m_LogEntries;

        public ObservableCollection<string> LogEntries
        {
            get { return m_LogEntries; }
            set { Set(ref m_LogEntries, value); }
        }

        #endregion

        #region Constructor

        public LogVisualizerViewModel()
        {
            m_LoggerConfiguration = (LoggerConfiguration) LoggerService.Configuration;
            m_LoggerConfiguration.LogWatcher.Updated += LogWatcher_Updated;
            LogEntries = new ObservableCollection<string>();
        }

        #endregion

        #region Commands

        public ICommand ShowLogFileCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => GetLastLogFile() != null,
                    CommandAction = () =>
                    {
                        var lastLogFile = GetLastLogFile();
                        if (lastLogFile != null)
                        {
                            Process.Start(lastLogFile);
                        }
                    }
                };
            }
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            m_LoggerConfiguration.LogWatcher.Updated -= LogWatcher_Updated;
        }

        #endregion

        #region Private methods

        private void LogWatcher_Updated(object sender, EventArgs e)
        {
            LogEntries.Clear();
            foreach (var logEntry in m_LoggerConfiguration.LogWatcher.LogContent)
            {
                LogEntries.Add(logEntry);
            }
        }

        private string GetLastLogFile()
        {
            var loggerConfig = (LoggerConfiguration) LoggerService.Configuration;
            var logFolder = Path.GetDirectoryName(loggerConfig.LogFile);
            if (logFolder == null || !Directory.Exists(logFolder))
            {
                return null;
            }

            var logFiles = Directory.GetFiles(logFolder).OrderByDescending(t => t).ToList();
            if (logFiles.Count > 0)
            {
                return logFiles[0];
            }

            return null;
        }

        #endregion
    }
}

#if !EXCLUDE_LOG4NET

namespace ImmotionAR.ImmotionRoom.Logger.Log4Net
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using log4net;
    using log4net.Appender;
    using log4net.Config;
    using log4net.Core;
    using log4net.Layout;
    using log4net.Repository.Hierarchy;
    using ILogger = ImmotionRoom.Logger.ILogger;

    /// <summary>
    ///     An implementation of a logger factory that creates <see cref="Logger" />s.
    /// </summary>
    public class LoggerFactory : LoggerFactoryBase
    {
        private static object LockObj = new object();

        private bool m_Configured;
        private LoggerConfiguration m_Configuration;
        private MemoryAppenderWithNotification m_AppenderWithNotification;

        /// <summary>
        ///     Creates a logger for the specified type.
        /// </summary>
        /// <param name="type">The type to create the logger for.</param>
        /// <returns>The newly-created logger.</returns>
        protected override ILogger CreateLogger(Type type)
        {
            if (!m_Configured)
            {
                lock (LockObj)
                {
                    if (!m_Configured)
                    {
                        ConfigureLogEnvironment();
                        m_Configured = true;
                    }
                }
            }

            return new Logger(type);
        }

        public override ILoggerConfiguration Configuration
        {
            get { return m_Configuration; }
            set
            {
                m_Configuration = value as LoggerConfiguration;

                if (!m_Configured)
                {
                    lock (LockObj)
                    {
                        if (!m_Configured)
                        {
                            ConfigureLogEnvironment();
                            m_Configured = true;
                        }
                    }
                }
            }
        }

        private void ConfigureLogEnvironment()
        {
            if (m_AppenderWithNotification != null)
            {
                m_AppenderWithNotification.Updated -= MemoryAppenderUpdatedHandler;
                m_AppenderWithNotification = null;
            }

            if (m_Configuration == null)
            {
                // Load from App.Config file
                XmlConfigurator.Configure();
                return;
            }

            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            // Avoid duplicated entries in Diagnostic Trace/Visual Studio
            Trace.Listeners.Clear();

            // Avoid duplicated entries in multiple log4net appenders
            hierarchy.Root.RemoveAllAppenders();
            
            if (m_Configuration.RollingFileEnabled)
            {
                RollingFileAppender.RollingMode rollingMode = RollingFileAppender.RollingMode.Size;
                switch (m_Configuration.RollingStyle)
                {
                    case "Size":
                        rollingMode = RollingFileAppender.RollingMode.Size;
                        break;
                    case "Date":
                        rollingMode = RollingFileAppender.RollingMode.Date;
                        break;
                    case "Composite":
                        rollingMode = RollingFileAppender.RollingMode.Composite;
                        break;
                    case "Once":
                        rollingMode = RollingFileAppender.RollingMode.Once;
                        break;
                }

                var appender = new RollingFileAppender();
                appender.AppendToFile = m_Configuration.AppendToFile;
                appender.File = m_Configuration.LogFile;

                var patternLayout = new PatternLayout();
                patternLayout.ConversionPattern = m_Configuration.RollingFileLogFormat;
                patternLayout.ActivateOptions();
                appender.Layout = patternLayout;

                appender.MaxSizeRollBackups = m_Configuration.MaxSizeRollBackups;
                appender.MaximumFileSize = m_Configuration.MaximumFileSize;
                appender.DatePattern = m_Configuration.DatePattern;
                appender.RollingStyle = rollingMode;
                appender.StaticLogFileName = m_Configuration.StaticLogFileName;
                appender.ActivateOptions();

                hierarchy.Root.AddAppender(appender);
            }

            if (m_Configuration.ConsoleLoggerEnabled)
            {
                var appender = new ColoredConsoleAppender();

                var patternLayout = new PatternLayout();
                patternLayout.ConversionPattern = m_Configuration.ConsoleLogFormat;
                patternLayout.ActivateOptions();
                appender.Layout = patternLayout;
                
                appender.AddMapping(new ColoredConsoleAppender.LevelColors { Level = Level.Info, ForeColor = ColoredConsoleAppender.Colors.White | ColoredConsoleAppender.Colors.HighIntensity });
                appender.AddMapping(new ColoredConsoleAppender.LevelColors { Level = Level.Debug, ForeColor = ColoredConsoleAppender.Colors.White | ColoredConsoleAppender.Colors.HighIntensity, BackColor = ColoredConsoleAppender.Colors.Blue });
                appender.AddMapping(new ColoredConsoleAppender.LevelColors { Level = Level.Warn, ForeColor = ColoredConsoleAppender.Colors.Yellow | ColoredConsoleAppender.Colors.HighIntensity });
                appender.AddMapping(new ColoredConsoleAppender.LevelColors { Level = Level.Error, ForeColor = ColoredConsoleAppender.Colors.Red| ColoredConsoleAppender.Colors.HighIntensity });
                appender.ActivateOptions();
                hierarchy.Root.AddAppender(appender);
            }

            if (m_Configuration.TraceLoggerEnabled)
            {
                var appender = new TraceAppender();

                var patternLayout = new PatternLayout();
                patternLayout.ConversionPattern = m_Configuration.TraceLogFormat;
                patternLayout.ActivateOptions();
                appender.Layout = patternLayout;

                appender.ActivateOptions();
                hierarchy.Root.AddAppender(appender);
            }

            if (m_Configuration.MemoryLoggerEnabled)
            {
                var appender = new MemoryAppenderWithNotification();

                appender.Updated += MemoryAppenderUpdatedHandler;

                var patternLayout = new PatternLayout();
                patternLayout.ConversionPattern = m_Configuration.MemoryLogFormat;
                patternLayout.ActivateOptions();
                appender.Layout = patternLayout;

                appender.ActivateOptions();

                m_AppenderWithNotification = appender;

                hierarchy.Root.AddAppender(appender);               
            }
            
            Level logLevel = Level.Off;
            switch (m_Configuration.LogLevel)
            {
                case "Debug":
                    logLevel = Level.Debug;
                    break;
                case "Info":
                    logLevel = Level.Info;
                    break;
                case "Warning":
                    logLevel = Level.Warn;
                    break;
                case "Error":
                    logLevel = Level.Error;
                    break;
                case "Fatal":
                    logLevel = Level.Fatal;
                    break;
            }
            hierarchy.Root.Level = logLevel;

            hierarchy.Configured = true;
        }

        private void MemoryAppenderUpdatedHandler(object sender, EventArgs eventArgs)
        {
            if (m_Configuration == null || m_Configuration.LogWatcher == null || m_AppenderWithNotification == null)
            {
                return;
            }

            // Get any events that may have occurred
            LoggingEvent[] events = m_AppenderWithNotification.GetEvents();
            List<string> formattedEvents = new List<string>();
            
            // Check that there are events to return
            if (events != null && events.Length > 0)
            {
                StringWriter s = new StringWriter();
                
                // If there are events, we clear them from the logger, since we're done with them  
                m_AppenderWithNotification.Clear();

                // Iterate through each event
                foreach (LoggingEvent ev in events)
                {
                    m_AppenderWithNotification.Layout.Format(s, ev);
                    formattedEvents.Add(s.ToString());
                }
            }

            // Return the constructed output
            m_Configuration.LogWatcher.LogContent = formattedEvents;
            m_Configuration.LogWatcher.NewEventsAvaialble();
        }
    }
}

#endif
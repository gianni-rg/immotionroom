#if !EXCLUDE_LOG4NET

namespace ImmotionAR.ImmotionRoom.Logger.Log4Net
{
    using System;
    using log4net;
    using log4net.Config;
    using log4net.Core;

    /// <summary>
    ///     A logger that integrates with log4net, passing all messages to an <see cref="ILog" />.
    /// </summary>
    public class Logger : LoggerBase
    {
        /// <summary>
        ///     The logger used by this instance.
        /// </summary>
        private readonly ILog m_Log4NetLogger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Log4NetLogger" /> class.
        /// </summary>
        /// <param name="type">The type to create a logger for.</param>
        public Logger(Type type)
        {
            // Configuration is now done at LoggerFactory level
            //// Load from App.Config file
            //XmlConfigurator.Configure();

            m_Log4NetLogger = LogManager.GetLogger(type);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Log4NetLogger" /> class.
        /// </summary>
        public Logger(ILog logger)
        {
            // Configuration is now done at LoggerFactory level
            //// Load from App.Config file
            //XmlConfigurator.Configure();

            m_Log4NetLogger = logger;
        }

        /// <summary>
        ///     Gets a value indicating whether messages with Debug severity should be logged.
        /// </summary>
        public override bool IsDebugEnabled
        {
            get { return m_Log4NetLogger.IsDebugEnabled; }
        }

        /// <summary>
        ///     Gets a value indicating whether messages with Info severity should be logged.
        /// </summary>
        public override bool IsInfoEnabled
        {
            get { return m_Log4NetLogger.IsInfoEnabled; }
        }

        /// <summary>
        ///     Gets a value indicating whether messages with Trace severity should be logged.
        /// </summary>
        public override bool IsTraceEnabled
        {
            get { return m_Log4NetLogger.Logger.IsEnabledFor(Level.Trace); }
        }

        /// <summary>
        ///     Gets a value indicating whether messages with Warn severity should be logged.
        /// </summary>
        public override bool IsWarnEnabled
        {
            get { return m_Log4NetLogger.IsWarnEnabled; }
        }

        /// <summary>
        ///     Gets a value indicating whether messages with Error severity should be logged.
        /// </summary>
        public override bool IsErrorEnabled
        {
            get { return m_Log4NetLogger.IsErrorEnabled; }
        }

        /// <summary>
        ///     Gets a value indicating whether messages with Fatal severity should be logged.
        /// </summary>
        public override bool IsFatalEnabled
        {
            get { return m_Log4NetLogger.IsFatalEnabled; }
        }

        /// <summary>
        ///     Logs the specified message with Debug severity.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Debug(string message)
        {
            m_Log4NetLogger.DebugFormat("{0}", message);
        }

        /// <summary>
        ///     Logs the specified message with Debug severity.
        /// </summary>
        /// <param name="format">The message or format template.</param>
        /// <param name="args">Any arguments required for the format template.</param>
        public override void Debug(string format, params object[] args)
        {
            m_Log4NetLogger.DebugFormat(format, args);
        }

        /// <summary>
        ///     Logs the specified exception with Debug severity.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="format">The message or format template.</param>
        /// <param name="args">Any arguments required for the format template.</param>
        public override void Debug(Exception exception, string format, params object[] args)
        {
            m_Log4NetLogger.Debug(string.Format(format, args), exception);
        }

        /// <summary>
        ///     Logs the specified message with Info severity.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Info(string message)
        {
            m_Log4NetLogger.InfoFormat("{0}", message);
        }

        /// <summary>
        ///     Logs the specified message with Info severity.
        /// </summary>
        /// <param name="format">The message or format template.</param>
        /// <param name="args">Any arguments required for the format template.</param>
        public override void Info(string format, params object[] args)
        {
            m_Log4NetLogger.InfoFormat(format, args);
        }

        /// <summary>
        ///     Logs the specified exception with Info severity.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="format">The message or format template.</param>
        /// <param name="args">Any arguments required for the format template.</param>
        public override void Info(Exception exception, string format, params object[] args)
        {
            m_Log4NetLogger.Info(string.Format(format, args), exception);
        }

        /// <summary>
        ///     Logs the specified message with Trace severity.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Trace(string message)
        {
            m_Log4NetLogger.Logger.Log(m_Log4NetLogger.GetType(), Level.Trace, string.Format("{0}", message), null);
        }

        /// <summary>
        ///     Logs the specified message with Trace severity.
        /// </summary>
        /// <param name="format">The message or format template.</param>
        /// <param name="args">Any arguments required for the format template.</param>
        public override void Trace(string format, params object[] args)
        {
            m_Log4NetLogger.Logger.Log(m_Log4NetLogger.GetType(), Level.Trace, string.Format(format, args), null);
        }

        /// <summary>
        ///     Logs the specified exception with Trace severity.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="format">The message or format template.</param>
        /// <param name="args">Any arguments required for the format template.</param>
        public override void Trace(Exception exception, string format, params object[] args)
        {
            m_Log4NetLogger.Logger.Log(m_Log4NetLogger.GetType(), Level.Trace, string.Format(format, args), exception);
        }

        /// <summary>
        ///     Logs the specified message with Warn severity.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Warn(string message)
        {
            m_Log4NetLogger.WarnFormat("{0}", message);
        }

        /// <summary>
        ///     Logs the specified message with Warn severity.
        /// </summary>
        /// <param name="format">The message or format template.</param>
        /// <param name="args">Any arguments required for the format template.</param>
        public override void Warn(string format, params object[] args)
        {
            m_Log4NetLogger.WarnFormat(format, args);
        }

        /// <summary>
        ///     Logs the specified exception with Warn severity.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="format">The message or format template.</param>
        /// <param name="args">Any arguments required for the format template.</param>
        public override void Warn(Exception exception, string format, params object[] args)
        {
            m_Log4NetLogger.Warn(string.Format(format, args), exception);
        }

        /// <summary>
        ///     Logs the specified message with Error severity.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Error(string message)
        {
            m_Log4NetLogger.ErrorFormat("{0}", message);
        }

        /// <summary>
        ///     Logs the specified message with Error severity.
        /// </summary>
        /// <param name="format">The message or format template.</param>
        /// <param name="args">Any arguments required for the format template.</param>
        public override void Error(string format, params object[] args)
        {
            m_Log4NetLogger.ErrorFormat(format, args);
        }

        /// <summary>
        ///     Logs the specified exception with Error severity.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="format">The message or format template.</param>
        /// <param name="args">Any arguments required for the format template.</param>
        public override void Error(Exception exception, string format, params object[] args)
        {
            m_Log4NetLogger.Error(string.Format(format, args), exception);
        }

        /// <summary>
        ///     Logs the specified message with Fatal severity.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Fatal(string message)
        {
            m_Log4NetLogger.FatalFormat("{0}", message);
        }

        /// <summary>
        ///     Logs the specified message with Fatal severity.
        /// </summary>
        /// <param name="format">The message or format template.</param>
        /// <param name="args">Any arguments required for the format template.</param>
        public override void Fatal(string format, params object[] args)
        {
            m_Log4NetLogger.FatalFormat(format, args);
        }

        /// <summary>
        ///     Logs the specified exception with Fatal severity.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="format">The message or format template.</param>
        /// <param name="args">Any arguments required for the format template.</param>
        public override void Fatal(Exception exception, string format, params object[] args)
        {
            m_Log4NetLogger.Fatal(string.Format(format, args), exception);
        }
    }
}

#endif
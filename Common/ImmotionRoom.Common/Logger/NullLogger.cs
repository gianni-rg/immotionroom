namespace ImmotionAR.ImmotionRoom.Logger
{
    using System;

    /// <summary>
    ///     A logger that does nothing (by design)
    /// </summary>
    public class NullLogger : LoggerBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NullLogger" /> class.
        /// </summary>
        /// <param name="type">The type to create a logger for.</param>
        public NullLogger(Type type)
        {
        }
        
        /// <summary>
        ///     Gets a value indicating whether messages with Debug severity should be logged.
        /// </summary>
        public override bool IsDebugEnabled
        {
            get { return false; }
        }

        /// <summary>
        ///     Gets a value indicating whether messages with Info severity should be logged.
        /// </summary>
        public override bool IsInfoEnabled
        {
            get { return false; }
        }

        /// <summary>
        ///     Gets a value indicating whether messages with Trace severity should be logged.
        /// </summary>
        public override bool IsTraceEnabled
        {
            get { return false; }
        }

        /// <summary>
        ///     Gets a value indicating whether messages with Warn severity should be logged.
        /// </summary>
        public override bool IsWarnEnabled
        {
            get { return false; }
        }

        /// <summary>
        ///     Gets a value indicating whether messages with Error severity should be logged.
        /// </summary>
        public override bool IsErrorEnabled
        {
            get { return false; }
        }

        /// <summary>
        ///     Gets a value indicating whether messages with Fatal severity should be logged.
        /// </summary>
        public override bool IsFatalEnabled
        {
            get { return false; }
        }

        /// <summary>
        ///     Logs the specified message with Debug severity.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Debug(string message)
        {
            // Do nothing by design
        }

        /// <summary>
        ///     Logs the specified message with Debug severity.
        /// </summary>
        /// <param name="format">The message or format template.</param>
        /// <param name="args">Any arguments required for the format template.</param>
        public override void Debug(string format, params object[] args)
        {
            // Do nothing by design
        }

        /// <summary>
        ///     Logs the specified exception with Debug severity.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="format">The message or format template.</param>
        /// <param name="args">Any arguments required for the format template.</param>
        public override void Debug(Exception exception, string format, params object[] args)
        {
            // Do nothing by design
        }

        /// <summary>
        ///     Logs the specified message with Info severity.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Info(string message)
        {
            // Do nothing by design
        }

        /// <summary>
        ///     Logs the specified message with Info severity.
        /// </summary>
        /// <param name="format">The message or format template.</param>
        /// <param name="args">Any arguments required for the format template.</param>
        public override void Info(string format, params object[] args)
        {
            // Do nothing by design
        }

        /// <summary>
        ///     Logs the specified exception with Info severity.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="format">The message or format template.</param>
        /// <param name="args">Any arguments required for the format template.</param>
        public override void Info(Exception exception, string format, params object[] args)
        {
            // Do nothing by design
        }

        /// <summary>
        ///     Logs the specified message with Trace severity.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Trace(string message)
        {
            // Do nothing by design
        }

        /// <summary>
        ///     Logs the specified message with Trace severity.
        /// </summary>
        /// <param name="format">The message or format template.</param>
        /// <param name="args">Any arguments required for the format template.</param>
        public override void Trace(string format, params object[] args)
        {
            // Do nothing by design
        }

        /// <summary>
        ///     Logs the specified exception with Trace severity.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="format">The message or format template.</param>
        /// <param name="args">Any arguments required for the format template.</param>
        public override void Trace(Exception exception, string format, params object[] args)
        {
            // Do nothing by design
        }

        /// <summary>
        ///     Logs the specified message with Warn severity.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Warn(string message)
        {
            // Do nothing by design
        }

        /// <summary>
        ///     Logs the specified message with Warn severity.
        /// </summary>
        /// <param name="format">The message or format template.</param>
        /// <param name="args">Any arguments required for the format template.</param>
        public override void Warn(string format, params object[] args)
        {
            // Do nothing by design
        }

        /// <summary>
        ///     Logs the specified exception with Warn severity.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="format">The message or format template.</param>
        /// <param name="args">Any arguments required for the format template.</param>
        public override void Warn(Exception exception, string format, params object[] args)
        {
            // Do nothing by design
        }

        /// <summary>
        ///     Logs the specified message with Error severity.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Error(string message)
        {
            // Do nothing by design
        }

        /// <summary>
        ///     Logs the specified message with Error severity.
        /// </summary>
        /// <param name="format">The message or format template.</param>
        /// <param name="args">Any arguments required for the format template.</param>
        public override void Error(string format, params object[] args)
        {
            // Do nothing by design
        }

        /// <summary>
        ///     Logs the specified exception with Error severity.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="format">The message or format template.</param>
        /// <param name="args">Any arguments required for the format template.</param>
        public override void Error(Exception exception, string format, params object[] args)
        {
            // Do nothing by design
        }

        /// <summary>
        ///     Logs the specified message with Fatal severity.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Fatal(string message)
        {
            // Do nothing by design
        }

        /// <summary>
        ///     Logs the specified message with Fatal severity.
        /// </summary>
        /// <param name="format">The message or format template.</param>
        /// <param name="args">Any arguments required for the format template.</param>
        public override void Fatal(string format, params object[] args)
        {
            // Do nothing by design
        }

        /// <summary>
        ///     Logs the specified exception with Fatal severity.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="format">The message or format template.</param>
        /// <param name="args">Any arguments required for the format template.</param>
        public override void Fatal(Exception exception, string format, params object[] args)
        {
            // Do nothing by design
        }
    }
}
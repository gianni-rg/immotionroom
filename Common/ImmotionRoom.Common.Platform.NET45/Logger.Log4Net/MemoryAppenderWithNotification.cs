#if !EXCLUDE_LOG4NET

namespace ImmotionAR.ImmotionRoom.Logger.Log4Net
{
    using System;
    using log4net.Appender;
    using log4net.Core;

    /// <summary>
    ///     A custom log4net appender which notifies log messages
    /// </summary>
    public class MemoryAppenderWithNotification : MemoryAppender
    {
        public event EventHandler Updated;

        protected override void Append(LoggingEvent loggingEvent)
        {
            // Append the event as usual
            base.Append(loggingEvent);

            // Then alert the Updated event that an event has occurred
            var localHandler = Updated;
            if (localHandler != null)
            {
                localHandler(this, new EventArgs());
            }
        }
    }
}

#endif 
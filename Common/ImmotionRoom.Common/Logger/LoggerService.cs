namespace ImmotionAR.ImmotionRoom.Logger
{
    using System;
    
    /// <summary>
    /// </summary>
    public static class LoggerService
    {
        private static readonly object LockObj = new object();

        private static ILoggerFactory m_LoggerFactory;

        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if (m_LoggerFactory == null)
                {
                    lock (LockObj)
                    {
                        if (m_LoggerFactory == null)
                        {
                            m_LoggerFactory = new NullLoggerFactory();
                        }
                    }
                }

                return m_LoggerFactory;
            }

            set
            {
                lock (LockObj)
                {
                    m_LoggerFactory = value;
                }
            }
        }



        public static ILoggerConfiguration Configuration
        {
            get
            {
                if (m_LoggerFactory != null)
                {
                    return m_LoggerFactory.Configuration;
                }

                return null;
            }

            set
            {
                if (m_LoggerFactory != null)
                {
                    m_LoggerFactory.Configuration = value;
                }
            }
        }
        
        /// <summary>
        ///     Gets the logger for the specified type, creating it if necessary.
        /// </summary>
        /// <returns>The newly-created logger.</returns>
        public static ILogger GetLogger<T>()
        {
            return LoggerFactory.GetLogger(typeof(T));
        }

        /// <summary>
        ///     Gets the logger for the specified type, creating it if necessary.
        /// </summary>
        /// <param name="type">The type to create the logger for.</param>
        /// <returns>The newly-created logger.</returns>
        public static ILogger GetLogger(Type type)
        {
            return LoggerFactory.GetLogger(type);
        }
    }
}
namespace ImmotionAR.ImmotionRoom.Logger
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    ///     A baseline definition of a logger factory, which tracks loggers as flyweights by type.
    ///     Custom logger factories should generally extend this type.
    /// </summary>
    public abstract class LoggerFactoryBase : ILoggerFactory
    {
        /// <summary>
        ///     Maps types to their loggers.
        /// </summary>
        private readonly Dictionary<Type, ILogger> m_Loggers = new Dictionary<Type, ILogger>();

        /// <summary>
        ///     Gets the logger for the specified type, creating it if necessary.
        /// </summary>
        /// <param name="type">The type to create the logger for.</param>
        /// <returns>The newly-created logger.</returns>
        public ILogger GetLogger(Type type)
        {
            lock (m_Loggers)
            {
                if (m_Loggers.ContainsKey(type))
                {
                    return m_Loggers[type];
                }

                var logger = CreateLogger(type);
                m_Loggers.Add(type, logger);

                return logger;
            }
        }

        /// <summary>
        ///     Gets the logger for the class calling this method.
        /// </summary>
        /// <returns>The newly-created logger.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public ILogger GetCurrentClassLogger()
        {
            //var frame = new StackFrame(1, false);
            //return GetLogger(frame.GetMethod().DeclaringType);
            return GetLogger(GetType().DeclaringType);
        }

        /// <summary>
        ///     Creates a logger for the specified type.
        /// </summary>
        /// <param name="type">The type to create the logger for.</param>
        /// <returns>The newly-created logger.</returns>
        protected abstract ILogger CreateLogger(Type type);

        public abstract ILoggerConfiguration Configuration { get; set; }
    }
}
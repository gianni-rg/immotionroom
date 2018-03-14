namespace ImmotionAR.ImmotionRoom.Logger
{
    using System;

    /// <summary>
    ///     Factory for loggers
    /// </summary>
    public interface ILoggerFactory
    {
        /// <summary>
        ///     Gets the logger for the specified type, creating it if necessary.
        /// </summary>
        /// <param name="type">The type to create the logger for.</param>
        /// <returns>The newly-created logger.</returns>
        ILogger GetLogger(Type type);

        /// <summary>
        ///     Gets the logger for the class calling this method.
        /// </summary>
        /// <returns>The newly-created logger.</returns>
        ILogger GetCurrentClassLogger();

        ILoggerConfiguration Configuration { get; set; }
    }
}
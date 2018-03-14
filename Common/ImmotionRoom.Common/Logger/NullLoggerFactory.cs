namespace ImmotionAR.ImmotionRoom.Logger
{
    using System;

    /// <summary>
    ///     An implementation of a logger factory that creates <see cref="NullLogger" />s.
    /// </summary>
    public class NullLoggerFactory : LoggerFactoryBase
    {
        /// <summary>
        ///     Creates a Null Logger for the specified type.
        /// </summary>
        /// <param name="type">The type to create the logger for.</param>
        /// <returns>The newly-created logger.</returns>
        protected override ILogger CreateLogger(Type type)
        {
            return new NullLogger(type);
        }

        public override ILoggerConfiguration Configuration { get; set; }
    }
}
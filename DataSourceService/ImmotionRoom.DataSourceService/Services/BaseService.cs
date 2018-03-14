namespace ImmotionAR.ImmotionRoom.DataSourceService.Services
{
    using System.Threading.Tasks;
    using Helpers;
    using Logger;

    public abstract class BaseService
    {
        #region Protected fields

        protected readonly ILogger m_Logger;

        #endregion

        #region Constructor

        protected BaseService(ILogger logger)
        {
            Requires.NotNull(logger, "logger");
            m_Logger = logger;
        }

        #endregion

        #region Methods

        public abstract Task StartAsync();

        public abstract void Stop();

        #endregion
    }
}

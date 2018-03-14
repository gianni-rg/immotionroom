namespace ImmotionAR.ImmotionRoom.DataSourceService.ControlApi.Controllers
{
    using System.Web.Http;
    using Logger;

    public abstract class BaseApiController : ApiController
    {
        #region Protected fields

        protected readonly ILogger m_Logger;

        #endregion

        #region Constructor

        protected BaseApiController(ILogger logger)
        {
            Helpers.Requires.NotNull(logger, "logger");
            m_Logger = logger;
        }

        #endregion
    }
}
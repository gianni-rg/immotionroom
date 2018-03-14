namespace ImmotionAR.ImmotionRoom.DataSourceService.ControlApi.Controllers
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Helpers.Messaging;
    using Logger;

    public class RequestController : BaseApiController
    {
        #region Private fields

        private readonly IMessenger m_Messenger;

        #endregion

        #region Constructor

        public RequestController() : base(LoggerService.GetLogger<RequestController>())
        {
            m_Messenger = MessengerService.Messenger;
        }

        #endregion

        #region Actions

        [HttpGet]
        public Task<HttpResponseMessage> Status(string id)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("GET /status/{0}", id);
            }

            // TaskCompletionSource should always have a generic
            // type argument so since we are not using any result,
            // we just set the argument to be of type object and will 
            // pass null to indicate the task's completion
            TaskCompletionSource<HttpResponseMessage> taskCompletionSource = new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.AttachedToParent);

            m_Messenger.Send(new Model.CommandResultRequest(id, requestResult =>
            {
                HttpResponseMessage responseMessage;
                if (requestResult == null)
                {
                    responseMessage = Request.CreateErrorResponse(HttpStatusCode.NotFound, "Invalid request id");
                }
                else
                {
                    responseMessage = Request.CreateResponse(HttpStatusCode.OK, requestResult.ToWebModel());
                }

                taskCompletionSource.SetResult(responseMessage);
            }));

            return taskCompletionSource.Task;
        }

        #endregion
    }
}
namespace ImmotionAR.ImmotionRoom.DataSourceService.ControlApi.Controllers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using DataSource.ControlClient.Model;
    using Helpers.Messaging;
    using Logger;
    using Command = Model.Command;
    using CommandRequestStatus = Model.CommandRequestStatus;
    using CommandStatus = Model.CommandStatus;
    using CommandType = Model.CommandType;
    using TrackingSessionConfiguration = DataSource.ControlClient.Model.TrackingSessionConfiguration;

    public class ServiceController : BaseApiController
    {
        #region Private fields

        private readonly IMessenger m_Messenger;

        #endregion

        #region Constructor

        public ServiceController() : base(LoggerService.GetLogger<ServiceController>())
        {
            m_Messenger = MessengerService.Messenger;
        }

        #endregion

        #region Actions

        [HttpPost]
        public HttpResponseMessage EnableAutoDiscovery()
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("POST /EnableAutoDiscovery");
            }

            var c = new Command();
            c.CommandType = CommandType.EnableAutoDiscovery;
            c.RequestId = Guid.NewGuid().ToString();
            m_Messenger.Send(c);

            var cmdResult = new Model.CommandResult<CommandStatus>();
            cmdResult.RequestId = c.RequestId;
            cmdResult.Data = new CommandStatus();
            cmdResult.Data.Status = CommandRequestStatus.Enqueued;
            return Request.CreateResponse(HttpStatusCode.OK, cmdResult);
        }

        [HttpPost]
        public HttpResponseMessage StartTracking([FromBody] TrackingSessionConfiguration trackingSessionConfiguration)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("POST /StartTracking");
            }

            var c = new Command();
            c.CommandType = CommandType.StartTracking;
            c.RequestId = Guid.NewGuid().ToString();
            c.Data.Add("TrackingSessionConfiguration", trackingSessionConfiguration.ToModel());
            m_Messenger.Send(c);

            var cmdResult = new Model.CommandResult<CommandStatus>();
            cmdResult.RequestId = c.RequestId;
            cmdResult.Data = new CommandStatus();
            cmdResult.Data.Status = CommandRequestStatus.Enqueued;
            return Request.CreateResponse(HttpStatusCode.OK, cmdResult);
        }

        [HttpPost]
        public HttpResponseMessage StopTracking()
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("POST /StopTracking");
            }

            var c = new Command();
            c.CommandType = CommandType.StopTracking;
            c.RequestId = Guid.NewGuid().ToString();
            m_Messenger.Send(c);

            var cmdResult = new Model.CommandResult<CommandStatus>();
            cmdResult.RequestId = c.RequestId;
            cmdResult.Data = new CommandStatus();
            cmdResult.Data.Status = CommandRequestStatus.Enqueued;
            return Request.CreateResponse(HttpStatusCode.OK, cmdResult);
        }

        [HttpGet]
        public HttpResponseMessage Status()
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("GET /Status");
            }

            var c = new Command();
            c.CommandType = CommandType.ServiceStatus;
            c.RequestId = Guid.NewGuid().ToString();
            m_Messenger.Send(c);

            var cmdResult = new Model.CommandResult<CommandStatus>();
            cmdResult.RequestId = c.RequestId;
            cmdResult.Data = new CommandStatus();
            cmdResult.Data.Status = CommandRequestStatus.Enqueued;
            return Request.CreateResponse(HttpStatusCode.OK, cmdResult);
        }

        //[HttpPost]
        //public HttpResponseMessage ToggleRecording()
        //{
        //    if (m_Logger.IsDebugEnabled)
        //    {
        //        m_Logger.Debug("POST /ToggleRecording");
        //    }

        //    var c = new Command();
        //    c.CommandType = CommandType.ToggleRecording;
        //    c.RequestId = Guid.NewGuid().ToString();
        //    m_CommandProcessor.EnqueueCommand(c.ToModel());

        //    var cmdResult = new CommandResult<CommandStatus>();
        //    cmdResult.RequestId = c.RequestId;
        //    cmdResult.Data = new CommandStatus();
        //    cmdResult.Data.Status = CommandRequestStatus.Enqueued;
        //    return Request.CreateResponse(HttpStatusCode.OK, cmdResult);
        //}

        [HttpPost]
        public HttpResponseMessage SystemReboot([FromBody] SystemRequest systemRequest)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("POST /SystemReboot");
            }

            if (systemRequest == null || string.IsNullOrEmpty(systemRequest.Token))
            {
                if (m_Logger.IsWarnEnabled)
                {
                    m_Logger.Warn("SystemReboot request without token");
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }

            var c = new Command();
            c.CommandType = CommandType.SystemReboot;
            c.RequestId = Guid.NewGuid().ToString();
            m_Messenger.Send(c);

            var cmdResult = new Model.CommandResult<CommandStatus>();
            cmdResult.RequestId = c.RequestId;
            cmdResult.Data = new CommandStatus();
            cmdResult.Data.Status = CommandRequestStatus.Enqueued;
            return Request.CreateResponse(HttpStatusCode.OK, cmdResult);
        }

        #endregion
    }
}
namespace ImmotionAR.ImmotionRoom.TrackingService.ControlApi.Controllers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using Helpers.Messaging;
    using Logger;
    using Model;

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
        public HttpResponseMessage EnableAutoDiscovery([FromBody] ControlClient.Model.AutoDiscoveryParameters autoDiscoveryParameters)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("POST /EnableAutoDiscovery");
            }
            
            var c = new Command();
            c.CommandType = CommandType.EnableAutoDiscovery;
            c.RequestId = Guid.NewGuid().ToString();
            c.Data.Add("AutoDiscoveryParameters", autoDiscoveryParameters.ToModel());
            m_Messenger.Send(c);
            
            var cmdResult = new CommandResult<CommandStatus>();
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

            var cmdResult = new CommandResult<CommandStatus>();
            cmdResult.RequestId = c.RequestId;
            cmdResult.Data = new CommandStatus();
            cmdResult.Data.Status = CommandRequestStatus.Enqueued;

            return Request.CreateResponse(HttpStatusCode.OK, cmdResult);
        }

        [HttpPost]
        public HttpResponseMessage StartCalibration([FromBody] ControlClient.Model.CalibrationParameters calibrationParameters)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("POST /StartCalibration");
            }

            var c = new Command();
            c.CommandType = CommandType.ExecuteCalibrationStep;
            c.RequestId = Guid.NewGuid().ToString();
            c.Data.Add("Parameters", calibrationParameters.ToModel());
            m_Messenger.Send(c);

            var cmdResult = new CommandResult<CommandStatus>();
            cmdResult.RequestId = c.RequestId;
            cmdResult.Data = new CommandStatus();
            cmdResult.Data.Status = CommandRequestStatus.Enqueued;

            return Request.CreateResponse(HttpStatusCode.OK, cmdResult);
        }

        [HttpPost]
        public HttpResponseMessage StartTracking([FromBody] ControlClient.Model.TrackingSessionConfiguration trackingSessionConfiguration)
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

            var cmdResult = new CommandResult<CommandStatus>();
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

            var cmdResult = new CommandResult<CommandStatus>();
            cmdResult.RequestId = c.RequestId;
            cmdResult.Data = new CommandStatus();
            cmdResult.Data.Status = CommandRequestStatus.Enqueued;

            return Request.CreateResponse(HttpStatusCode.OK, cmdResult);
        }

        [HttpPost]
        public HttpResponseMessage ExecuteCalibrationStep([FromBody] ControlClient.Model.CalibrationParameters calibrationParameters)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("POST /ExecuteCalibrationStep");
            }
            
            var c = new Command();
            c.CommandType = CommandType.ExecuteCalibrationStep;
            c.RequestId = Guid.NewGuid().ToString();
            c.Data.Add("Parameters", calibrationParameters.ToModel());
            m_Messenger.Send(c);

            var cmdResult = new CommandResult<CommandStatus>();
            cmdResult.RequestId = c.RequestId;
            cmdResult.Data = new CommandStatus();
            cmdResult.Data.Status = CommandRequestStatus.Enqueued;

            return Request.CreateResponse(HttpStatusCode.OK, cmdResult);
        }

        [HttpPost]
        public HttpResponseMessage SetMasterDataSource(string id)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("POST /SetMasterDataSource/{0}", id);
            }

            var c = new Command();
            c.CommandType = CommandType.SetMasterDataSource;
            c.RequestId = Guid.NewGuid().ToString();
            c.Data.Add("DataSourceId", id);
            m_Messenger.Send(c);

            var cmdResult = new CommandResult<CommandStatus>();
            cmdResult.RequestId = c.RequestId;
            cmdResult.Data = new CommandStatus();
            cmdResult.Data.Status = CommandRequestStatus.Enqueued;

            return Request.CreateResponse(HttpStatusCode.OK, cmdResult);
        }

        [HttpPost]
        public HttpResponseMessage StartDiagnosticMode()
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("POST /StartDiagnosticMode");
            }
            
            var c = new Command();
            c.CommandType = CommandType.StartDiagnosticMode;
            c.RequestId = Guid.NewGuid().ToString();
            m_Messenger.Send(c);

            var cmdResult = new CommandResult<CommandStatus>();
            cmdResult.RequestId = c.RequestId;
            cmdResult.Data = new CommandStatus();
            cmdResult.Data.Status = CommandRequestStatus.Enqueued;

            return Request.CreateResponse(HttpStatusCode.OK, cmdResult);
        }

        [HttpPost]
        public HttpResponseMessage StopDiagnosticMode()
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("POST /StopDiagnosticMode");
            }
            
            var c = new Command();
            c.CommandType = CommandType.StopDiagnosticMode;
            c.RequestId = Guid.NewGuid().ToString();
            m_Messenger.Send(c);

            var cmdResult = new CommandResult<CommandStatus>();
            cmdResult.RequestId = c.RequestId;
            cmdResult.Data = new CommandStatus();
            cmdResult.Data.Status = CommandRequestStatus.Enqueued;

            return Request.CreateResponse(HttpStatusCode.OK, cmdResult);
        }

        [HttpPost]
        public HttpResponseMessage SystemReboot([FromBody] ControlClient.Model.SystemRequest systemRequest)
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

            var cmdResult = new CommandResult<CommandStatus>();
            cmdResult.RequestId = c.RequestId;
            cmdResult.Data = new CommandStatus();
            cmdResult.Data.Status = CommandRequestStatus.Enqueued;

            return Request.CreateResponse(HttpStatusCode.OK, cmdResult);
        }

        [HttpPost]
        public HttpResponseMessage SetSceneDescriptor([FromBody] ControlClient.Model.TrackingServiceSceneDescriptor sceneDescriptor)
        {
            if (m_Logger.IsDebugEnabled)
            {
                m_Logger.Debug("POST /SetSceneDescriptor");
            }

            var c = new Command();
            c.CommandType = CommandType.SetSceneDescriptor;
            c.RequestId = Guid.NewGuid().ToString();

            if (sceneDescriptor != null)
            {
                // Mapping to Domain Model in performed in the CommandProcessor and TrackingService!
                // Floor values are retrieved to maintain an uniform mapping behaviour, bit those values
                // will be ignored when updating the Scene Descriptor.
                // Stage Area, currently, it is not used.
                c.Data["FloorClipPlaneX"] = sceneDescriptor.FloorClipPlane.X;
                c.Data["FloorClipPlaneY"] = sceneDescriptor.FloorClipPlane.Y;
                c.Data["FloorClipPlaneZ"] = sceneDescriptor.FloorClipPlane.Z;
                c.Data["FloorClipPlaneW"] = sceneDescriptor.FloorClipPlane.W;
                c.Data["StageAreaSizeX"] = sceneDescriptor.StageArea.Size.X;
                c.Data["StageAreaSizeY"] = sceneDescriptor.StageArea.Size.Y;
                c.Data["StageAreaSizeZ"] = sceneDescriptor.StageArea.Size.Z;
                c.Data["StageAreaCenterX"] = sceneDescriptor.StageArea.Center.X;
                c.Data["StageAreaCenterY"] = sceneDescriptor.StageArea.Center.Y;
                c.Data["StageAreaCenterZ"] = sceneDescriptor.StageArea.Center.Z;
                c.Data["GameAreaSizeX"] = sceneDescriptor.GameArea.Size.X;
                c.Data["GameAreaSizeY"] = sceneDescriptor.GameArea.Size.Y;
                c.Data["GameAreaSizeZ"] = sceneDescriptor.GameArea.Size.Z;
                c.Data["GameAreaCenterX"] = sceneDescriptor.GameArea.Center.X;
                c.Data["GameAreaCenterY"] = sceneDescriptor.GameArea.Center.Y;
                c.Data["GameAreaCenterZ"] = sceneDescriptor.GameArea.Center.Z;
                c.Data["GameAreaInnerLimitsX"] = sceneDescriptor.GameAreaInnerLimits.X;
                c.Data["GameAreaInnerLimitsY"] = sceneDescriptor.GameAreaInnerLimits.Y;
                c.Data["GameAreaInnerLimitsZ"] = sceneDescriptor.GameAreaInnerLimits.Z;
            }

            m_Messenger.Send(c);

            var cmdResult = new CommandResult<CommandStatus>();
            cmdResult.RequestId = c.RequestId;
            cmdResult.Data = new CommandStatus();
            cmdResult.Data.Status = CommandRequestStatus.Enqueued;

            return Request.CreateResponse(HttpStatusCode.OK, cmdResult);
        }
        #endregion
    }
}
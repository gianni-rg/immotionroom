namespace ImmotionAR.ImmotionRoom.DataSourceService.ControlApi
{
    using System;
    using Helpers;
    using Interfaces;
    using Logger;
    using Microsoft.Owin.Hosting;
    using Model;

    public class DataSourceControlApiServer : IDataSourceControlApiServer
    {
        #region Private fields

        private readonly ILogger m_Logger;
        private readonly DataSourceConfiguration m_Configuration;

        private IDisposable m_WebApiServer;

        #endregion

        #region Constructor

        public DataSourceControlApiServer(DataSourceConfiguration configuration)
        {
            m_Logger = LoggerService.GetLogger<DataSourceControlApiServer>();
            m_Configuration = configuration;
        }

        #endregion

        #region Public methods

        public bool Start()
        {
            return StartWebApiHost();
        }

        public void Stop()
        {
            StopWebApiHost();
        }

        #endregion

        #region Private methods
        private bool StartWebApiHost()
        {
            var baseAddress = string.Format("http://{0}:{1}", m_Configuration.LocalEndpoint, m_Configuration.ControlApiPort);

            try
            {
                var options = new StartOptions
                {
                    Urls = { baseAddress },
                    ServerFactory = "Nowin",
                };

                m_WebApiServer = WebApp.Start<WebApiStartup>(options);

                if (m_Configuration.LocalEndpoint == "127.0.0.1")
                {
                    if (m_Logger.IsDebugEnabled)
                    {
                        m_Logger.Warn("No network found. Reverted to localhost");
                    }
                }

                if (m_Logger.IsDebugEnabled)
                {
                    m_Logger.Debug("Control API Server is opened at {0}", baseAddress);
                }

                return true;
            }
            catch (Exception ex)
            {
                string logErrorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    logErrorMessage = ex.InnerException.Message;
                }

                if (m_Logger.IsErrorEnabled)
                {
                    m_Logger.Error("Control API Server not started: {0}", logErrorMessage);
                }
            }

            return false;
        }

        private void StopWebApiHost()
        {
            if (m_WebApiServer != null)
            {
                m_WebApiServer.Dispose();
            }
        }

        #endregion
    }
}
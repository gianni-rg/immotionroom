namespace ImmotionAR.ImmotionRoom.DataSource.ControlClient
{
    using System;
    using System.Threading.Tasks;
    using Logger;
    using Model;
    
    public class DataSourceControlClient : BaseApiClient
    {
        public delegate void RequestCompleted(OperationResponse response);

        public delegate void TrackingServiceStatusCompleted(DataSourceServiceStatusResponse response);

        private readonly int m_WebServiceCallsRetries;
        private readonly int m_WebServiceCallsRetriesIntervalInMillisecs;

        private readonly string m_Token;

        #region Constructor

        /// <summary>
        ///     Creates a new TrackingServiceWebApiClient which will connect to the specified ip/port
        /// </summary>
        /// <param name="ip">Tracking Service Listener IP Address"</param>
        /// <param name="port">Tracking Service Listener Port"</param>
        /// <param name="secure">true for using HTTPS; false otherwise</param>
        public DataSourceControlClient(string ip, int port, bool secure = false) : this(string.Format("http{0}://{1}:{2}", secure ? "s" : "", ip, port))
        {
            m_Token = null;
        }

        /// <summary>
        ///     Creates a new TrackingServiceWebApiClient which will connect to the specified endpoint
        /// </summary>
        /// <param name="endPoint">Tracking Service Endpoint in the format "http(s)://{ip}:{port}"</param>
        public DataSourceControlClient(string endPoint) : base(LoggerService.GetLogger<DataSourceControlClient>(), endPoint)
        {
            m_WebServiceCallsRetries = 5;
            m_WebServiceCallsRetriesIntervalInMillisecs = 500;
        }

        #endregion

        #region Methods

        public void EnableAutoDiscoveryAsync(RequestCompleted completedCallback)
        {
#if UNITY_5
            Task.Factory.StartNew(() =>
#else
            Task.Run(async () =>
#endif
            { 
                String requestUrl = ComposeApiUrl("Service/EnableAutoDiscovery");
                IRestClient httpClient = GetHttpClient(requestUrl, m_Token);
                CommandResult<CommandStatus> result = null;
                String errorMessage = "Service/EnableAutoDiscovery request failed";
                try
                {
#if UNITY_5
                    result = httpClient.Post<CommandResult<CommandStatus>>();
#else
                    result = await httpClient.PostAsync<CommandResult<CommandStatus>>().ConfigureAwait(false);
#endif
                }
                catch (WebApiClientException ex)
                {
                    errorMessage = ex.Message;
                }

                if (result == null || result.Data.Status != CommandRequestStatus.Enqueued)
                {
                    completedCallback(CreateErrorResponse(errorMessage));
                    return;
                }

                requestUrl = ComposeApiUrl(string.Format("request/status/{0}", result.RequestId));
                httpClient = GetHttpClient(requestUrl, m_Token);

                int retries = m_WebServiceCallsRetries;
                errorMessage = string.Format("Request/Status/{0} request failed", result.RequestId);
                while (retries > 0)
                {
                    try
                    {
#if UNITY_5
                        var status = httpClient.Get<CommandResult<object>>();
#else
                        var status = await httpClient.GetAsync<CommandResult<object>>().ConfigureAwait(false);
#endif
                        
                        if (status != null)
                        {
                            completedCallback(new OperationResponse {IsError = false});
                            return;
                        }
                    }
                    catch (WebApiClientException ex)
                    {
                        errorMessage = ex.Message;
                    }
                    retries--;
#if UNITY_5
                    Thread.Sleep(m_WebServiceCallsRetriesIntervalInMillisecs);
#else
                    await Task.Delay(m_WebServiceCallsRetriesIntervalInMillisecs).ConfigureAwait(false);
#endif
                }

                completedCallback(CreateErrorResponse(errorMessage));
            });
        }

        public void GetStatusAsync(TrackingServiceStatusCompleted completedCallback)
        {
#if UNITY_5
            Task.Factory.StartNew(() =>
#else
            Task.Run(async () =>
#endif
            {
                String requestUrl = ComposeApiUrl("Service/Status");
                IRestClient httpClient = GetHttpClient(requestUrl, m_Token);
                CommandResult<CommandStatus> result = null;
                String errorMessage = "Service/Status request failed";
                try
                {
#if UNITY_5
                    result = httpClient.Get<CommandResult<CommandStatus>>();
#else
                    result = await httpClient.GetAsync<CommandResult<CommandStatus>>().ConfigureAwait(false);
#endif
                }
                catch (WebApiClientException ex)
                {
                    errorMessage = ex.Message;
                }

                if (result == null || result.Data.Status != CommandRequestStatus.Enqueued)
                {
                    completedCallback(new DataSourceServiceStatusResponse {IsError = true, Error = errorMessage});
                    return;
                }

                requestUrl = ComposeApiUrl(string.Format("request/status/{0}", result.RequestId));
                httpClient = GetHttpClient(requestUrl, m_Token);

                int retries = m_WebServiceCallsRetries;
                errorMessage = string.Format("Request/Status/{0} request failed", result.RequestId);
                while (retries > 0)
                {
                    try
                    {
#if UNITY_5
                        var status = httpClient.Get<CommandResult<DataSourceServiceStatus>>();
#else
                        var status = await httpClient.GetAsync<CommandResult<DataSourceServiceStatus>>().ConfigureAwait(false);
#endif
                        
                        if (status != null)
                        {
                            completedCallback(new DataSourceServiceStatusResponse {IsError = false, Status = status.Data});
                            return;
                        }
                    }
                    catch (WebApiClientException ex)
                    {
                        errorMessage = ex.Message;
                    }
                    retries--;
#if UNITY_5
                    Thread.Sleep(m_WebServiceCallsRetriesIntervalInMillisecs);
#else
                    await Task.Delay(m_WebServiceCallsRetriesIntervalInMillisecs).ConfigureAwait(false);
#endif
                }

                completedCallback(new DataSourceServiceStatusResponse {IsError = true, Error = errorMessage});
            });
        }

        public void StartCalibrationAsync(RequestCompleted completedCallback)
        {
#if UNITY_5
            Task.Factory.StartNew(() =>
#else
            Task.Run(async () =>
#endif
            {
                String requestUrl = ComposeApiUrl("Service/StartCalibration");
                IRestClient httpClient = GetHttpClient(requestUrl, m_Token);
                CommandResult<CommandStatus> result = null;
                String errorMessage = "Service/StartCalibration request failed";
                try
                {
#if UNITY_5
                    result = httpClient.Post<CommandResult<CommandStatus>>();
#else
                    result = await httpClient.PostAsync<CommandResult<CommandStatus>>().ConfigureAwait(false);
#endif
                }
                catch (WebApiClientException ex)
                {
                    errorMessage = ex.Message;
                }

                if (result == null || result.Data.Status != CommandRequestStatus.Enqueued)
                {
                    completedCallback(CreateErrorResponse(errorMessage));
                    return;
                }

                requestUrl = ComposeApiUrl(string.Format("request/status/{0}", result.RequestId));
                httpClient = GetHttpClient(requestUrl, m_Token);

                int retries = m_WebServiceCallsRetries;
                errorMessage = string.Format("Request/Status/{0} request failed", result.RequestId);
                while (retries > 0)
                {
                    try
                    {
#if UNITY_5
                        var status = httpClient.Get<CommandResult<object>>();
#else
                        var status = await httpClient.GetAsync<CommandResult<object>>().ConfigureAwait(false);
#endif

                        if (status != null)
                        {
                            completedCallback(new OperationResponse {IsError = false});
                            return;
                        }
                    }
                    catch (WebApiClientException ex)
                    {
                        errorMessage = ex.Message;
                    }
                    retries--;
#if UNITY_5
                    Thread.Sleep(m_WebServiceCallsRetriesIntervalInMillisecs);
#else
                    await Task.Delay(m_WebServiceCallsRetriesIntervalInMillisecs).ConfigureAwait(false);
#endif
                }

                completedCallback(CreateErrorResponse(errorMessage));
            });
        }

        public void StartTrackingAsync(TrackingSessionConfiguration trackingSessionConfiguration, RequestCompleted completedCallback)
        {
#if UNITY_5
            Task.Factory.StartNew(() =>
#else
            Task.Run(async () =>
#endif
            {
                var requestUrl = ComposeApiUrl("Service/StartTracking");
                IRestClient httpClient = GetHttpClient(requestUrl, m_Token);
                CommandResult<CommandStatus> result = null;
                String errorMessage = "Service/StartTracking request failed";
                try
                {
#if UNITY_5
                    result = httpClient.Post<CommandResult<CommandStatus>, TrackingSessionConfiguration>(trackingSessionConfiguration);
#else
                    result = await httpClient.PostAsync<CommandResult<CommandStatus>, TrackingSessionConfiguration>(trackingSessionConfiguration).ConfigureAwait(false);
#endif
                }
                catch (WebApiClientException ex)
                {
                    errorMessage = ex.Message;
                }

                if (result == null || result.Data.Status != CommandRequestStatus.Enqueued)
                {
                    completedCallback(CreateErrorResponse(errorMessage));
                    return;
                }

                requestUrl = ComposeApiUrl(string.Format("request/status/{0}", result.RequestId));
                httpClient = GetHttpClient(requestUrl, m_Token);

                int retries = m_WebServiceCallsRetries;
                errorMessage = string.Format("Request/Status/{0} request failed", result.RequestId);
                while (retries > 0)
                {
                    try
                    {
#if UNITY_5
                        var status = httpClient.Get<CommandResult<object>>();
#else
                        var status = await httpClient.GetAsync<CommandResult<object>>().ConfigureAwait(false);
#endif

                        if (status != null)
                        {
                            completedCallback(new OperationResponse {IsError = false});
                            return;
                        }
                    }
                    catch (WebApiClientException ex)
                    {
                        errorMessage = ex.Message;
                    }
                    retries--;
#if UNITY_5
                    Thread.Sleep(m_WebServiceCallsRetriesIntervalInMillisecs);
#else
                    await Task.Delay(m_WebServiceCallsRetriesIntervalInMillisecs).ConfigureAwait(false);
#endif
                }

                completedCallback(CreateErrorResponse(errorMessage));
            });
        }

        public void StopTrackingAsync(RequestCompleted completedCallback)
        {
#if UNITY_5
            Task.Factory.StartNew(() =>
#else
            Task.Run(async () =>
#endif
            {
                String requestUrl = ComposeApiUrl("Service/StopTracking");
                IRestClient httpClient = GetHttpClient(requestUrl, m_Token);
                CommandResult<CommandStatus> result = null;
                String errorMessage = "Service/StopTracking request failed";
                try
                {
#if UNITY_5
                    result = httpClient.Post<CommandResult<CommandStatus>>();
#else
                    result = await httpClient.PostAsync<CommandResult<CommandStatus>>().ConfigureAwait(false);
#endif
                }
                catch (WebApiClientException ex)
                {
                    errorMessage = ex.Message;
                }

                if (result == null || result.Data.Status != CommandRequestStatus.Enqueued)
                {
                    completedCallback(CreateErrorResponse(errorMessage));
                    return;
                }

                requestUrl = ComposeApiUrl(string.Format("request/status/{0}", result.RequestId));
                httpClient = GetHttpClient(requestUrl, m_Token);

                int retries = m_WebServiceCallsRetries;
                errorMessage = string.Format("Request/Status/{0} request failed", result.RequestId);
                while (retries > 0)
                {
                    try
                    {
#if UNITY_5
                        var status = httpClient.Get<CommandResult<object>>();
#else
                        var status = await httpClient.GetAsync<CommandResult<object>>().ConfigureAwait(false);
#endif

                        if (status != null)
                        {
                            completedCallback(new OperationResponse {IsError = false});
                            return;
                        }
                    }
                    catch (WebApiClientException ex)
                    {
                        errorMessage = ex.Message;
                    }
                    retries--;
#if UNITY_5
                    Thread.Sleep(m_WebServiceCallsRetriesIntervalInMillisecs);
#else
                    await Task.Delay(m_WebServiceCallsRetriesIntervalInMillisecs).ConfigureAwait(false);
#endif
                }

                completedCallback(CreateErrorResponse(errorMessage));
            });
        }

        public void SystemRebootAsync(RequestCompleted completedCallback)
        {
#if UNITY_5
            Task.Factory.StartNew(() =>
#else
            Task.Run(async () =>
#endif
            {
                var requestUrl = ComposeApiUrl("Service/SystemReboot");
                var httpClient = GetHttpClient(requestUrl, m_Token);
                CommandResult<CommandStatus> result = null;
                var errorMessage = "Service/SystemReboot request failed";
                try
                {
#if UNITY_5
                    result = httpClient.Post<CommandResult<CommandStatus>, SystemRequest>(new SystemRequest { Token = Guid.NewGuid().ToString("N") });
#else
                    result = await httpClient.PostAsync<CommandResult<CommandStatus>, SystemRequest>(new SystemRequest {Token = Guid.NewGuid().ToString("N")}).ConfigureAwait(false);
#endif
                }
                catch (WebApiClientException ex)
                {
                    errorMessage = ex.Message;
                }

                if (result == null || result.Data.Status != CommandRequestStatus.Enqueued)
                {
                    completedCallback(CreateErrorResponse(errorMessage));
                    return;
                }

                requestUrl = ComposeApiUrl(string.Format("request/status/{0}", result.RequestId));
                httpClient = GetHttpClient(requestUrl, m_Token);

                var retries = m_WebServiceCallsRetries;
                errorMessage = string.Format("Request/Status/{0} request failed", result.RequestId);
                while (retries > 0)
                {
                    try
                    {
#if UNITY_5
                        var status = httpClient.Get<CommandResult<DataSourceServiceStatusResponse>>();
#else
                        var status = await httpClient.GetAsync<CommandResult<DataSourceServiceStatusResponse>>().ConfigureAwait(false);
#endif

                        if (status != null)
                        {
                            if (status.Data != null)
                            {
                                completedCallback(new OperationResponse { IsError = status.Data.IsError, ErrorCode = (int)status.Data.ErrorCode, Error = status.Data.Error });
                            }
                            else
                            {
                                completedCallback(new OperationResponse { IsError = false });
                            }
                            return;
                        }
                    }
                    catch (WebApiClientException ex)
                    {
                        errorMessage = ex.Message;
                    }
                    retries--;
#if UNITY_5
                    Thread.Sleep(m_WebServiceCallsRetriesIntervalInMillisecs);
#else
                    await Task.Delay(m_WebServiceCallsRetriesIntervalInMillisecs).ConfigureAwait(false);
#endif
                }

                completedCallback(CreateErrorResponse(errorMessage));
            });
        }

        #endregion

        #region Private methods

        private OperationResponse CreateErrorResponse(string errorDescription, int errorCode = 0)
        {
            return new OperationResponse {IsError = true, Error = errorDescription, ErrorCode = errorCode};
        }

        #endregion
    }
}

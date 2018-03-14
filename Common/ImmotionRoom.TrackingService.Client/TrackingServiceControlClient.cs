namespace ImmotionAR.ImmotionRoom.TrackingService.ControlClient
{
    using System;
    using System.Threading.Tasks;
    using Logger;
    using Model;

#if UNITY_5
    using System.Threading;
#endif

    public class TrackingServiceControlClient : BaseApiClient
    {
        public delegate void RequestCompleted(OperationResponse response);

        public delegate void TrackingServiceStatusCompleted(TrackingServiceStatusResponse response);

        private readonly int m_WebServiceCallsRetries;
        private readonly int m_WebServiceCallsRetriesIntervalInMillisecs;

        #region Constructor

        /// <summary>
        ///     Creates a new Client which will connect to the specified ip/port
        /// </summary>
        /// <param name="ip">Tracking Service Listener IP Address"</param>
        /// <param name="port">Tracking Service Listener Port"</param>
        /// <param name="secure">true for using HTTPS; false otherwise</param>
        public TrackingServiceControlClient(string ip, int port, bool secure = false) : this(string.Format("http{0}://{1}:{2}", secure ? "s" : "", ip, port))
        {
        }

        /// <summary>
        ///     Creates a new Client which will connect to the specified endpoint
        /// </summary>
        /// <param name="endPoint">Tracking Service Endpoint in the format "http(s)://{ip}:{port}"</param>
        public TrackingServiceControlClient(string endPoint) : base(LoggerService.GetLogger<TrackingServiceControlClient>(), endPoint)
        {
            m_WebServiceCallsRetries = 5;
            m_WebServiceCallsRetriesIntervalInMillisecs = 1000;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Enable AutoDiscovery Mode in the target Tracking Service
        /// </summary>
        /// <param name="completedCallback">Callback to handle AutoDiscovery Completed event</param>
        /// <param name="parameters">AutoDiscoveryParameters: ClearCalibrationData true/false, ClearMasterDataSource true/false. If not specified, default is false</param>
        public void EnableAutoDiscoveryAsync(RequestCompleted completedCallback, AutoDiscoveryParameters parameters = null)
        {
#if UNITY_5
            Task.Factory.StartNew(() =>
#else
            Task.Run(async () =>
#endif
            {
                var requestUrl = ComposeApiUrl("Service/EnableAutoDiscovery");
                var httpClient = GetHttpClient(requestUrl);
                CommandResult<CommandStatus> result = null;
                var errorMessage = "Service/EnableAutoDiscovery request failed";

                if (parameters == null)
                {
                    parameters = new AutoDiscoveryParameters { ClearCalibrationData = false, ClearMasterDataSource = false };
                }

                try
                {
#if UNITY_5
                    result = httpClient.Post<CommandResult<CommandStatus>, AutoDiscoveryParameters>(parameters);
#else
                    result = await httpClient.PostAsync<CommandResult<CommandStatus>, AutoDiscoveryParameters>(parameters).ConfigureAwait(false);
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
                httpClient = GetHttpClient(requestUrl);

                var retries = m_WebServiceCallsRetries;
                errorMessage = string.Format("Request/Status/{0} request failed", result.RequestId);
                while (retries > 0)
                {
                    try
                    {
#if UNITY_5
                        var status = httpClient.Get<CommandResult<TrackingServiceUpdateStateResult>>();
#else
                        var status = await httpClient.GetAsync<CommandResult<TrackingServiceUpdateStateResult>>().ConfigureAwait(false);
#endif
                        if (status != null)
                        {
                            if (status.Data != null)
                            {
                                completedCallback(new OperationResponse {IsError = status.Data.IsError, ErrorCode = (int) status.Data.ErrorCode, ErrorDescription = status.Data.ErrorDescription});
                            }
                            else
                            {
                                completedCallback(new OperationResponse {IsError = false});
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

        public void GetStatusAsync(TrackingServiceStatusCompleted completedCallback)
        {
#if UNITY_5
            Task.Factory.StartNew(() =>
#else
            Task.Run(async () =>
#endif
            {
                var requestUrl = ComposeApiUrl("Service/Status");
                var httpClient = GetHttpClient(requestUrl);
                CommandResult<CommandStatus> result = null;
                var errorMessage = "Service/Status request failed";
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
                    completedCallback(new TrackingServiceStatusResponse {IsError = true, ErrorDescription = errorMessage});
                    return;
                }

                requestUrl = ComposeApiUrl(string.Format("request/status/{0}", result.RequestId));
                httpClient = GetHttpClient(requestUrl);

                var retries = m_WebServiceCallsRetries;
                errorMessage = string.Format("Request/Status/{0} request failed", result.RequestId);
                while (retries > 0)
                {
                    try
                    {
#if UNITY_5
                        var status = httpClient.Get<CommandResult<TrackingServiceStatus>>();
#else
                        var status = await httpClient.GetAsync<CommandResult<TrackingServiceStatus>>().ConfigureAwait(false);
#endif

                        if (status != null)
                        {
                            completedCallback(new TrackingServiceStatusResponse {IsError = false, Status = status.Data});
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

                completedCallback(new TrackingServiceStatusResponse {IsError = true, ErrorDescription = errorMessage});
            });
        }

        public void StartCalibrationAsync(CalibrationParameters calibrationParameters, RequestCompleted completedCallback)
        {
#if UNITY_5
            Task.Factory.StartNew(() =>
#else
            Task.Run(async () =>
#endif
            {
                var requestUrl = ComposeApiUrl("Service/StartCalibration");
                var httpClient = GetHttpClient(requestUrl);
                CommandResult<CommandStatus> result = null;
                var errorMessage = "Service/StartCalibration request failed";
                try
                {
#if UNITY_5
                    result = httpClient.Post<CommandResult<CommandStatus>, CalibrationParameters>(calibrationParameters);
#else
                    result = await httpClient.PostAsync<CommandResult<CommandStatus>, CalibrationParameters>(calibrationParameters).ConfigureAwait(false);
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
                httpClient = GetHttpClient(requestUrl);

                var retries = m_WebServiceCallsRetries;
                errorMessage = string.Format("Request/Status/{0} request failed", result.RequestId);
                while (retries > 0)
                {
                    try
                    {
#if UNITY_5
                        var status = httpClient.Get<CommandResult<TrackingServiceUpdateStateResult>>();
#else
                        var status = await httpClient.GetAsync<CommandResult<TrackingServiceUpdateStateResult>>().ConfigureAwait(false);
#endif

                        if (status != null)
                        {
                            completedCallback(new OperationResponse {IsError = status.Data.IsError, ErrorCode = (int) status.Data.ErrorCode, ErrorDescription = status.Data.ErrorDescription});
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

        public void ExecuteCalibrationStepAsync(CalibrationParameters calibrationParameters, RequestCompleted completedCallback)
        {
#if UNITY_5
            Task.Factory.StartNew(() =>
#else
            Task.Run(async () =>
#endif
            {
                var requestUrl = ComposeApiUrl("Service/ExecuteCalibrationStep");
                var httpClient = GetHttpClient(requestUrl);
                CommandResult<CommandStatus> result = null;
                var errorMessage = "Service/ExecuteCalibrationStep request failed";
                try
                {
#if UNITY_5
                    result = httpClient.Post<CommandResult<CommandStatus>, CalibrationParameters>(calibrationParameters);
#else
                    result = await httpClient.PostAsync<CommandResult<CommandStatus>, CalibrationParameters>(calibrationParameters).ConfigureAwait(false);
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
                httpClient = GetHttpClient(requestUrl);

                var retries = m_WebServiceCallsRetries;
                errorMessage = string.Format("Request/Status/{0} request failed", result.RequestId);
                while (retries > 0)
                {
                    try
                    {
#if UNITY_5
                        var status = httpClient.Get<CommandResult<TrackingServiceUpdateStateResult>>();
#else
                        var status = await httpClient.GetAsync<CommandResult<TrackingServiceUpdateStateResult>>().ConfigureAwait(false);
#endif

                        if (status != null)
                        {
                            completedCallback(new OperationResponse {IsError = status.Data.IsError, ErrorCode = (int) status.Data.ErrorCode, ErrorDescription = status.Data.ErrorDescription});
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


        public void StartTrackingAsync(TrackingSessionConfiguration sessionConfiguration, RequestCompleted completedCallback)
        {
#if UNITY_5
            Task.Factory.StartNew(() =>
#else
            Task.Run(async () =>
#endif
            {
                var requestUrl = ComposeApiUrl("Service/StartTracking");
                var httpClient = GetHttpClient(requestUrl);
                CommandResult<CommandStatus> result = null;
                var errorMessage = "Service/StartTracking request failed";
                try
                {
#if UNITY_5
                    result = httpClient.Post<CommandResult<CommandStatus>, TrackingSessionConfiguration>(sessionConfiguration);
#else
                    result = await httpClient.PostAsync<CommandResult<CommandStatus>, TrackingSessionConfiguration>(sessionConfiguration).ConfigureAwait(false);
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
                httpClient = GetHttpClient(requestUrl);

                var retries = m_WebServiceCallsRetries;
                errorMessage = string.Format("Request/Status/{0} request failed", result.RequestId);
                while (retries > 0)
                {
                    try
                    {
#if UNITY_5
                        var status = httpClient.Get<CommandResult<TrackingServiceUpdateStateResult>>();
#else
                        var status = await httpClient.GetAsync<CommandResult<TrackingServiceUpdateStateResult>>().ConfigureAwait(false);
#endif

                        if (status != null)
                        {
                            completedCallback(new OperationResponse {IsError = status.Data.IsError, ErrorCode = (int) status.Data.ErrorCode, ErrorDescription = status.Data.ErrorDescription});
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
                var requestUrl = ComposeApiUrl("Service/StopTracking");
                var httpClient = GetHttpClient(requestUrl);
                CommandResult<CommandStatus> result = null;
                var errorMessage = "Service/StopTracking request failed";
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
                httpClient = GetHttpClient(requestUrl);

                var retries = m_WebServiceCallsRetries;
                errorMessage = string.Format("Request/Status/{0} request failed", result.RequestId);
                while (retries > 0)
                {
                    try
                    {
#if UNITY_5
                        var status = httpClient.Get<CommandResult<TrackingServiceUpdateStateResult>>();
#else
                        var status = await httpClient.GetAsync<CommandResult<TrackingServiceUpdateStateResult>>().ConfigureAwait(false);
#endif

                        if (status != null)
                        {
                            completedCallback(new OperationResponse {IsError = status.Data.IsError, ErrorCode = (int) status.Data.ErrorCode, ErrorDescription = status.Data.ErrorDescription});
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

        public void SetMasterDataSourceAsync(string dataSourceId, RequestCompleted completedCallback)
        {
#if UNITY_5
            Task.Factory.StartNew(() =>
#else
            Task.Run(async () =>
#endif
            {
                var requestUrl = ComposeApiUrl(string.Format("Service/SetMasterDataSource/{0}", dataSourceId));
                var httpClient = GetHttpClient(requestUrl);
                CommandResult<CommandStatus> result = null;
                var errorMessage = "Service/SetMasterDataSource/{0} request failed";
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
                httpClient = GetHttpClient(requestUrl);

                var retries = m_WebServiceCallsRetries;
                errorMessage = string.Format("Request/Status/{0} request failed", result.RequestId);
                while (retries > 0)
                {
                    try
                    {
#if UNITY_5
                        var status = httpClient.Get<CommandResult<TrackingServiceUpdateStateResult>>();
#else
                        var status = await httpClient.GetAsync<CommandResult<TrackingServiceUpdateStateResult>>().ConfigureAwait(false);
#endif

                        if (status.Data != null)
                        {
                            completedCallback(new OperationResponse {IsError = status.Data.IsError, ErrorCode = (int) status.Data.ErrorCode, ErrorDescription = status.Data.ErrorDescription});
                        }
                        else
                        {
                            completedCallback(new OperationResponse {IsError = false});
                        }
                        return;
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

        public void StartDiagnosticModeAsync(TrackingSessionConfiguration sessionConfiguration, RequestCompleted completedCallback)
        {
#if UNITY_5
            Task.Factory.StartNew(() =>
#else
            Task.Run(async () =>
#endif
            {
                var requestUrl = ComposeApiUrl("Service/StartDiagnosticMode");
                var httpClient = GetHttpClient(requestUrl);
                CommandResult<CommandStatus> result = null;
                var errorMessage = "Service/StartDiagnosticMode request failed";
                try
                {
#if UNITY_5
                    result = httpClient.Post<CommandResult<CommandStatus>, TrackingSessionConfiguration>(sessionConfiguration);
#else
                    result = await httpClient.PostAsync<CommandResult<CommandStatus>, TrackingSessionConfiguration>(sessionConfiguration).ConfigureAwait(false);
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
                httpClient = GetHttpClient(requestUrl);

                var retries = m_WebServiceCallsRetries;
                errorMessage = string.Format("Request/Status/{0} request failed", result.RequestId);
                while (retries > 0)
                {
                    try
                    {
#if UNITY_5
                        var status = httpClient.Get<CommandResult<TrackingServiceUpdateStateResult>>();
#else
                        var status = await httpClient.GetAsync<CommandResult<TrackingServiceUpdateStateResult>>().ConfigureAwait(false);
#endif

                        if (status != null)
                        {
                            completedCallback(new OperationResponse {IsError = status.Data.IsError, ErrorCode = (int) status.Data.ErrorCode, ErrorDescription = status.Data.ErrorDescription});
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

        public void StopDiagnosticModeAsync(RequestCompleted completedCallback)
        {
#if UNITY_5
            Task.Factory.StartNew(() =>
#else
            Task.Run(async () =>
#endif
            {
                var requestUrl = ComposeApiUrl("Service/StopDiagnosticMode");
                var httpClient = GetHttpClient(requestUrl);
                CommandResult<CommandStatus> result = null;
                var errorMessage = "Service/StopDiagnosticMode request failed";
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
                httpClient = GetHttpClient(requestUrl);

                var retries = m_WebServiceCallsRetries;
                errorMessage = string.Format("Request/Status/{0} request failed", result.RequestId);
                while (retries > 0)
                {
                    try
                    {
#if UNITY_5
                        var status = httpClient.Get<CommandResult<TrackingServiceUpdateStateResult>>();
#else
                        var status = await httpClient.GetAsync<CommandResult<TrackingServiceUpdateStateResult>>().ConfigureAwait(false);
#endif

                        if (status != null)
                        {
                            completedCallback(new OperationResponse {IsError = status.Data.IsError, ErrorCode = (int) status.Data.ErrorCode, ErrorDescription = status.Data.ErrorDescription});
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
                var httpClient = GetHttpClient(requestUrl);
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
                httpClient = GetHttpClient(requestUrl);

                var retries = m_WebServiceCallsRetries;
                errorMessage = string.Format("Request/Status/{0} request failed", result.RequestId);
                while (retries > 0)
                {
                    try
                    {
#if UNITY_5
                        var status = httpClient.Get<CommandResult<TrackingServiceUpdateStateResult>>();
#else
                        var status = await httpClient.GetAsync<CommandResult<TrackingServiceUpdateStateResult>>().ConfigureAwait(false);
#endif

                        if (status != null)
                        {
                            if (status.Data != null)
                            {
                                completedCallback(new OperationResponse {IsError = status.Data.IsError, ErrorCode = (int) status.Data.ErrorCode, ErrorDescription = status.Data.ErrorDescription});
                            }
                            else
                            {
                                completedCallback(new OperationResponse {IsError = false});
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

        /// <summary>
        /// Sets scene limits and sizes. From the configuration app it is possible to configure Stage Area and Game Area only.
        /// Floor is detected automatically, so even if Floor values are set, thery will be ignored when updating Tracking Service configuration.
        /// </summary>
        /// <param name="sceneDescriptor"></param>
        /// <param name="completedCallback"></param>
        public void SetSceneDescriptorAsync(TrackingServiceSceneDescriptor sceneDescriptor, RequestCompleted completedCallback)
        {
#if UNITY_5
            Task.Factory.StartNew(() =>
#else
            Task.Run(async () =>
#endif
            {
                var requestUrl = ComposeApiUrl("Service/SetSceneDescriptor");
                var httpClient = GetHttpClient(requestUrl);
                CommandResult<CommandStatus> result = null;
                var errorMessage = "Service/SetSceneDescriptor request failed";
                try
                {
#if UNITY_5
                    result = httpClient.Post<CommandResult<CommandStatus>, TrackingServiceSceneDescriptor>(sceneDescriptor);
#else
                    result = await httpClient.PostAsync<CommandResult<CommandStatus>, TrackingServiceSceneDescriptor>(sceneDescriptor).ConfigureAwait(false);
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
                httpClient = GetHttpClient(requestUrl);

                var retries = m_WebServiceCallsRetries;
                errorMessage = string.Format("Request/Status/{0} request failed", result.RequestId);
                while (retries > 0)
                {
                    try
                    {
#if UNITY_5
                        var status = httpClient.Get<CommandResult<TrackingServiceUpdateStateResult>>();
#else
                        var status = await httpClient.GetAsync<CommandResult<TrackingServiceUpdateStateResult>>().ConfigureAwait(false);
#endif

                        if (status.Data != null)
                        {
                            completedCallback(new OperationResponse { IsError = status.Data.IsError, ErrorCode = (int)status.Data.ErrorCode, ErrorDescription = status.Data.ErrorDescription });
                        }
                        else
                        {
                            completedCallback(new OperationResponse { IsError = false });
                        }
                        return;
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
            return new OperationResponse {IsError = true, ErrorDescription = errorDescription, ErrorCode = errorCode};
        }

        #endregion
    }
}

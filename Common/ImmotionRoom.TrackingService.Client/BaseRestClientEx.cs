namespace ImmotionAR.ImmotionRoom.TrackingService.ControlClient
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class BaseRestClientEx : IRestClient
    {
        #region Protected fields

        protected readonly string m_Endpoint;
        protected readonly string m_Token;
        protected readonly int m_Timeout;

        #endregion

        #region Constructor

        public BaseRestClientEx(string endpoint, string token, int timeout)
        {
            m_Endpoint = endpoint;
            m_Token = token;
            m_Timeout = timeout;
        }

        #endregion

        #region Web methods

        public async Task<T> GetAsync<T>()
        {
            using (HttpClient httpClient = NewHttpClient())
            {
                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(m_Endpoint).ConfigureAwait(false);
                    return (T) await HandleResponse<T>(response).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new WebApiClientException(HandleException(ex), ex);
                }
            }
        }

        public async Task<T> PostAsync<T>(T data)
        {
            using (HttpClient httpClient = NewHttpClient())
            {
                try
                {
                    HttpResponseMessage response = await httpClient.PostAsync(m_Endpoint, new StringContent(JsonConvert.SerializeObject(data))).ConfigureAwait(false);
                    return (T) await HandleResponse<T>(response).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new WebApiClientException(HandleException(ex), ex);
                }
            }
        }

        public async Task<TResponse> PostEmptyAsync<TResponse>()
        {
            using (HttpClient httpClient = NewHttpClient())
            {
                try
                {
                    ////Alternative version: http://stackoverflow.com/questions/10077237/httpclient-authentication-header-not-getting-sent
                    //var message = new HttpRequestMessage(HttpMethod.Post, m_Endpoint)
                    //{
                    //    Content = new StringContent(string.Empty)
                    //};
                    //message.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                    //var response = await httpClient.SendAsync(message).ConfigureAwait(false);

                    HttpResponseMessage response = await httpClient.PostAsync(m_Endpoint, new StringContent(string.Empty)).ConfigureAwait(false);
                    return (TResponse) await HandleResponse<TResponse>(response).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new WebApiClientException(HandleException(ex), ex);
                }
            }
        }

        public async Task<string> PutAsync<T>(T data)
        {
            using (HttpClient httpClient = NewHttpClient())
            {
                try
                {
                    HttpResponseMessage response = await httpClient.PutAsync(m_Endpoint, new StringContent(JsonConvert.SerializeObject(data))).ConfigureAwait(false);
                    return HandleResponse<string>(response).ToString();
                }
                catch (Exception ex)
                {
                    throw new WebApiClientException(HandleException(ex), ex);
                }
            }
        }

        public async Task<string> PutAsync<T>(int id, T data)
        {
            using (HttpClient httpClient = NewHttpClient())
            {
                try
                {
                    HttpResponseMessage response = await httpClient.PutAsync(string.Format("{0}{1}", m_Endpoint, id), new StringContent(JsonConvert.SerializeObject(data))).ConfigureAwait(false);
                    return HandleResponse<string>(response).ToString();
                }
                catch (Exception ex)
                {
                    throw new WebApiClientException(HandleException(ex), ex);
                }
            }
        }

        public async Task<string> DeleteAsync()
        {
            using (HttpClient httpClient = NewHttpClient())
            {
                try
                {
                    HttpResponseMessage response = await httpClient.DeleteAsync(m_Endpoint).ConfigureAwait(false);
                    return HandleResponse<string>(response).ToString();
                }
                catch (Exception ex)
                {
                    throw new WebApiClientException(HandleException(ex), ex);
                }
            }
        }

        #endregion

        #region Web methods (extended)

        public async Task<TResponse> PostAsync<TResponse>()
        {
            using (HttpClient httpClient = NewHttpClient())
            {
                try
                {
                    HttpResponseMessage response = await httpClient.PostAsync(m_Endpoint, new StringContent(string.Empty)).ConfigureAwait(false);
                    return (TResponse) await HandleResponse<TResponse>(response).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new WebApiClientException(HandleException(ex), ex);
                }
            }
        }

        public async Task<TResponse> PostAsync<TResponse, T>(T data)
        {
            using (HttpClient httpClient = NewHttpClient())
            {
                try
                {
                    HttpResponseMessage response = await httpClient.PostAsync(m_Endpoint, new StringContent(JsonConvert.SerializeObject(data), new UTF8Encoding(), "application/json")).ConfigureAwait(false);
                    return (TResponse) await HandleResponse<TResponse>(response).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new WebApiClientException(HandleException(ex), ex);
                }
            }
        }

        public async Task<TResponse> PutAsync<TResponse, T>(T data)
        {
            using (HttpClient httpClient = NewHttpClient())
            {
                try
                {
                    HttpResponseMessage response = await httpClient.PutAsync(m_Endpoint, new StringContent(JsonConvert.SerializeObject(data))).ConfigureAwait(false);
                    return (TResponse) await HandleResponse<TResponse>(response).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new WebApiClientException(HandleException(ex), ex);
                }
            }
        }

        public async Task<TResponse> PutAsync<TResponse, T>(int id, T data)
        {
            using (HttpClient httpClient = NewHttpClient())
            {
                try
                {
                    HttpResponseMessage response = await httpClient.PutAsync(string.Format("{0}{1}", m_Endpoint, id), new StringContent(JsonConvert.SerializeObject(data))).ConfigureAwait(false);
                    return (TResponse) await HandleResponse<TResponse>(response).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new WebApiClientException(HandleException(ex), ex);
                }
            }
        }

        public async Task<TResponse> DeleteAsync<TResponse>()
        {
            using (HttpClient httpClient = NewHttpClient())
            {
                try
                {
                    HttpResponseMessage response = await httpClient.DeleteAsync(m_Endpoint).ConfigureAwait(false);
                    return (TResponse) await HandleResponse<TResponse>(response).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new WebApiClientException(HandleException(ex), ex);
                }
            }
        }

        public async Task<TResponse> PostFormUrlEncoded<TResponse>(IDictionary<string, string> data)
        {
            using (HttpClient httpClient = NewHttpClient())
            {
                try
                {
                    HttpResponseMessage response = await httpClient.PostAsync(m_Endpoint, new FormUrlEncodedContent(data)).ConfigureAwait(false);
                    return (TResponse) await HandleResponse<TResponse>(response).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new WebApiClientException(HandleException(ex), ex);
                }
            }
        }

        #endregion

        #region Private methods

        protected HttpClient NewHttpClient()
        {
            // SSL Support
            var httpClient = new HttpClient(new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Automatic, UseProxy = true, Proxy = WebRequest.DefaultWebProxy
            });

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            // At the moment Authentication is NOT supported/implemented
            //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", m_Token);
            if (m_Timeout != 0)
            {
                httpClient.Timeout = TimeSpan.FromMinutes(m_Timeout);
            }
            return httpClient;
        }

        private string HandleException(Exception ex)
        {
            var aggregateException = ex as AggregateException;
            if (aggregateException == null)
            {
                return ex.Message;
            }

            var message = new StringBuilder();
            foreach (Exception agEx in aggregateException.InnerExceptions)
            {
                message.AppendLine(agEx.Message);
                Exception innerEx = agEx.InnerException;
                while (innerEx != null)
                {
                    message.AppendLine(String.Format("---> {0}", innerEx.Message));
                    innerEx = innerEx.InnerException;
                }
            }

            return message.ToString();
        }

        private async Task<object> HandleResponse<TResponse>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                if (typeof (TResponse) == typeof (String))
                {
                    return JsonConvert.DeserializeObject<string>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
                return JsonConvert.DeserializeObject<TResponse>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return default(TResponse);
            }
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                string text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                dynamic result = JsonConvert.DeserializeObject(text);
                throw new HttpRequestException(string.Format("500 Internal Server Error: {0}", result.Message));
            }
            throw new HttpRequestException(response.ReasonPhrase);
        }

        #endregion
    }
}
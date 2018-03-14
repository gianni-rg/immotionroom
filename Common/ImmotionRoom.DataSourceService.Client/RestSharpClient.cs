namespace ImmotionAR.ImmotionRoom.DataSource.ControlClient
{
    using System;
    using System.Net;
    using System.Text;
    using RestSharp;
    using RestSharp.Deserializers;

    public class RestSharpClient : IRestClient
    {
        private const int HttpReadWriteTimeout = 10; //seconds

        #region Protected fields

        protected readonly string m_Endpoint;
        protected readonly string m_Resource;

        #endregion

        #region Constructor

        public RestSharpClient(string endpoint, string resource)
        {
            m_Endpoint = endpoint;
            m_Resource = resource;
        }

        #endregion

        #region Web methods

        public T Get<T>() where T : new()
        {
            RestClient httpClient = NewHttpClient();

            try
            {
                var request = new RestRequest(m_Resource, Method.GET);
                request.RequestFormat = DataFormat.Json;
                IRestResponse<T> response = httpClient.Get<T>(request);
                return (T) HandleResponse(response);
            }
            catch (Exception ex)
            {
                throw new WebApiClientException(HandleException(ex), ex);
            }
        }

        public T Post<T>() where T : new()
        {
            RestClient httpClient = NewHttpClient();

            try
            {
                var request = new RestRequest(m_Resource, Method.POST);
                request.RequestFormat = DataFormat.Json;
                IRestResponse<T> response = httpClient.Post<T>(request);
                return (T) HandleResponse(response);
            }
            catch (Exception ex)
            {
                throw new WebApiClientException(HandleException(ex), ex);
            }
        }

        public T Post<T>(T data) where T : new()
        {
            RestClient httpClient = NewHttpClient();

            try
            {
                var request = new RestRequest(m_Resource, Method.POST);
                request.RequestFormat = DataFormat.Json;
                request.AddBody(data);
                IRestResponse<T> response = httpClient.Post<T>(request);
                return (T)HandleResponse(response);
            }
            catch (Exception ex)
            {
                throw new WebApiClientException(HandleException(ex), ex);
            }
        }

        public TResponse Post<TResponse, T>(T data)
            where T : new()
            where TResponse : new()
        {
            RestClient httpClient = NewHttpClient();

            try
            {
                var request = new RestRequest(m_Resource, Method.POST);
                request.RequestFormat = DataFormat.Json;
                request.AddBody(data);
                IRestResponse<TResponse> response = httpClient.Post<TResponse>(request);
                return (TResponse)HandleResponse(response);
            }
            catch (Exception ex)
            {
                throw new WebApiClientException(HandleException(ex), ex);
            }
        }

        public string Put<T>(T data) where T : new()
        {
            //var httpClient = NewHttpClient();

            //try
            //{
            //    var response = httpClient.PutAsync<T>(m_Endpoint, data, new JsonMediaTypeFormatter()).Result;
            //    return HandleResponse<string>(response).ToString();
            //}
            //catch (Exception ex)
            //{
            //    throw new WebApiClientException(HandleException(ex), ex);
            //}
            return null;
        }

        public string Put<T>(int id, T data) where T : new()
        {
            return null;
            //var httpClient = NewHttpClient();

            //try
            //{
            //    var response = httpClient.PutAsync<T>(string.Format("{0}{1}", m_Endpoint, id), data, new JsonMediaTypeFormatter()).Result;
            //    return HandleResponse<string>(response).ToString();
            //}
            //catch (Exception ex)
            //{
            //    throw new WebApiClientException(HandleException(ex), ex);
            //}
        }

        public string Delete<T>() where T : new()
        {
            //var httpClient = NewHttpClient();

            //try
            //{
            //    var response = httpClient.DeleteAsync(m_Endpoint).Result;
            //    return HandleResponse<string>(response).ToString();
            //}
            //catch (Exception ex)
            //{
            //    throw new WebApiClientException(HandleException(ex), ex);
            //}
            return null;
        }

        private object HandleResponse<TResponse>(IRestResponse<TResponse> response)
        {
            if (response.ResponseStatus == ResponseStatus.Completed)
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return response.Data;
                }

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return default(TResponse);
                }

                if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    throw new WebApiClientException(String.Format("500 Internal Server Error: {0}", response.ErrorMessage));
                }
            }

            if (response.ErrorException != null)
            {
                throw new WebApiClientException(response.ErrorMessage);
            }
            
            throw new WebApiClientException(String.Format("{0} {1}", (int)response.StatusCode, response.StatusDescription));
        }

        #endregion

        #region Private methods

        protected RestClient NewHttpClient()
        {
            var httpClient = new RestClient(m_Endpoint);

            // Load the JsonDeserializer for all types
            httpClient.ClearHandlers();
            httpClient.AddHandler("*", new JsonDeserializer());
            httpClient.ReadWriteTimeout = HttpReadWriteTimeout;

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

        #endregion
    }
}
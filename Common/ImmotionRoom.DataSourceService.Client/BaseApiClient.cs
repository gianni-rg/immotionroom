namespace ImmotionAR.ImmotionRoom.DataSource.ControlClient
{
    using System;
    using System.Globalization;
    using Logger;

    public abstract class BaseApiClient
    {
        #region Protected fields

        protected ILogger m_Logger;
        protected readonly CultureInfo m_EnglishCulture = new CultureInfo("en-US");
        protected readonly string m_Endpoint;

        #endregion

        #region Constructor

        protected BaseApiClient(ILogger logger, string endpoint)
        {
            if (!endpoint.StartsWith("http"))
            {
                throw new ArgumentException("Specified endpoint ('{0}') is not valid. Format: 'http(s)://domain:port'.", endpoint);
            }

            m_Logger = logger;
            m_Endpoint = endpoint;
        }

        #endregion

        #region Protected methods

        protected string ComposeApiUrl(string apiMethod, string apiParameters = "")
        {
            return string.Format("internal/v1/{0}{1}", apiMethod, apiParameters);
        }

        protected IRestClient GetHttpClient(string requestUrl, string token)
        {
#if UNITY_5
            return new RestSharpClient(m_Endpoint, requestUrl);
#else
            return new BaseRestClientEx(string.Format("{0}/{1}", m_Endpoint, requestUrl), token, timeout: 10);
#endif
        }

        #endregion
    }
}
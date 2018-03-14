namespace ImmotionAR.ImmotionRoom.TrackingService.ControlClient
{
    using System;

    public class WebApiClientException : Exception
    {
        public WebApiClientException(string message, Exception ex = null) : base(message, ex)
        {
        }
    }
}
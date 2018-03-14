namespace ImmotionAR.ImmotionRoom.Networking
{
    using System;

    public class NetworkException : Exception
    {
        public NetworkSocketError SocketErrorCode { get; set; }
    }
}

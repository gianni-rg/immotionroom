namespace ImmotionAR.ImmotionRoom.Networking
{
    using System.Net.Sockets;

    public static class Mappers
    {
        public static NetworkSocketError NativeSocketErrorToNetworkSocketError(SocketError nativeError)
        {
            switch (nativeError)
            {
                case SocketError.TimedOut:
                    return NetworkSocketError.TimedOut;

                case SocketError.ConnectionRefused:
                    return NetworkSocketError.ConnectionRefused;

                case SocketError.OperationAborted:
                case SocketError.ConnectionAborted:
                    return NetworkSocketError.ConnectionAborted;

                case SocketError.ConnectionReset:
                    return NetworkSocketError.ConnectionReset;
            }

            return NetworkSocketError.Unknown;
        }
    }
}

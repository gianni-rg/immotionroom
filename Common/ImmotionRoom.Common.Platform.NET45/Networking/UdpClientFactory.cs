namespace ImmotionAR.ImmotionRoom.Networking
{
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Interfaces;

    public class UdpClientFactory : IUdpClientFactory
    {
        public Task<IUdpClient> CreateMulticastClientAsync(string localIpAddress, string multicastAddress, int multicastPort)
        {
            // See: http://www.jarloo.com/c-udp-multicasting-tutorial/
            var localIp = System.Net.IPAddress.Parse(localIpAddress);
            var localEndpoint = new System.Net.IPEndPoint(localIp,  multicastPort);
            
            var nativeClient = new System.Net.Sockets.UdpClient();

            nativeClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            nativeClient.ExclusiveAddressUse = false;
            nativeClient.Client.Bind(localEndpoint);
            //client.MulticastLoopback = false;

            nativeClient.JoinMulticastGroup(System.Net.IPAddress.Parse(multicastAddress), localIp);

            IUdpClient result = new UdpClient(nativeClient);

#if !UNITY_5
            return Task.FromResult(result);
#endif

#if UNITY_5
            var taskSource = new TaskCompletionSource<IUdpClient>();
            taskSource.SetResult(result);
            return taskSource.Task;
#endif
        }

        public Task<IUdpClient> CreateLocalClientAsync(string ipAddress, int port, int timeout)
        {
            // See: http://www.jarloo.com/c-udp-multicasting-tutorial/
            var localEndpoint = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);

            var nativeClient = new System.Net.Sockets.UdpClient();

            nativeClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            nativeClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, timeout);
            nativeClient.ExclusiveAddressUse = false;
            nativeClient.Client.Bind(localEndpoint);

            IUdpClient result = new UdpClient(nativeClient);

#if !UNITY_5
            return Task.FromResult(result);
#endif

#if UNITY_5
            var taskSource = new TaskCompletionSource<IUdpClient>();
            taskSource.SetResult(result);
            return taskSource.Task;
#endif
        }
    }
}

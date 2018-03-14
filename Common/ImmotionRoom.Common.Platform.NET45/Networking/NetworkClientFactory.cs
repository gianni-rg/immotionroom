namespace ImmotionAR.ImmotionRoom.Networking
{
    using Interfaces;

    public class NetworkClientFactory : INetworkClientFactory
    {
        public INetworkClient CreateClient(ITcpClient client, string clientId)
        {
            return new SocketClient(client, clientId);
        }
    }
}

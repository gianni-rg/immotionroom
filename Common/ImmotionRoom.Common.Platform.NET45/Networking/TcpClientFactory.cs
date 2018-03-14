namespace ImmotionAR.ImmotionRoom.Networking
{
    using Interfaces;

    public class TcpClientFactory : ITcpClientFactory
    {
        public ITcpClient CreateClient()
        {
            return new TcpClient(new System.Net.Sockets.TcpClient());
        }
    }
}

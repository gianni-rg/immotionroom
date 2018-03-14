namespace ImmotionAR.ImmotionRoom.Networking
{
    using Interfaces;

    public class TcpServerFactory: ITcpServerFactory
    {
        public ITcpServer CreateServer(string ip, int port)
        {
            return new TcpServer(ip, port);
        }
    }
}

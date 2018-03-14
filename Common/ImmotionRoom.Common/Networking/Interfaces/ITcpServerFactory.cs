namespace ImmotionAR.ImmotionRoom.Networking.Interfaces
{
    public interface ITcpServerFactory
    {
        ITcpServer CreateServer(string ip, int port);
    }
}

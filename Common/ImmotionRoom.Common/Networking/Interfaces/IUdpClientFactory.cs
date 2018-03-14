namespace ImmotionAR.ImmotionRoom.Networking.Interfaces
{
    using System.Threading.Tasks;

    public interface IUdpClientFactory
    {
        Task<IUdpClient> CreateMulticastClientAsync(string localIpAddress, string multicastAddress, int multicastPort);
        Task<IUdpClient> CreateLocalClientAsync(string ipAddress, int port, int timeout);
    }
}

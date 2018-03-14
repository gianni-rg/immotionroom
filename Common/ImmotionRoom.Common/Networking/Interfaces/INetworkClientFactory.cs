namespace ImmotionAR.ImmotionRoom.Networking.Interfaces
{
    public interface INetworkClientFactory
    {
        INetworkClient CreateClient(ITcpClient client, string clientId);
    }
}

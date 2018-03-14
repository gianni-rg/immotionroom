namespace ImmotionAR.ImmotionRoom.Networking.Interfaces
{
    using System;
    using System.Threading.Tasks;

    public interface ITcpServer
    {
        event EventHandler<ClientConnectedEventArgs> ClientConnected;

        Task StartAsync();

        void Stop();
    }
}

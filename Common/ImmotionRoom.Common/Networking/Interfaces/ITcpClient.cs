namespace ImmotionAR.ImmotionRoom.Networking.Interfaces
{
    using System.IO;
    using System.Threading.Tasks;

    public interface ITcpClient
    {
        bool Connected { get; }
        IPEndPoint RemoteEndPoint { get; }

        void Close();
        Task ConnectAsync(string ip, int port);
        void ConnectWithinTimeout(string ip, int port, int timeoutInMilliseconds);
        Stream GetStream();
    }
}

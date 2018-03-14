namespace ImmotionAR.ImmotionRoom.Networking.Interfaces
{
    public interface INetworkClient
    {
        string Id { get; }
        bool IsConnected { get; }
        IPEndPoint RemoteEndPoint { get; }

        bool Send(byte[] data, int length);

        bool Send(byte[] data);

        void Close();
        int ReadClientStreamMode();
    }
}

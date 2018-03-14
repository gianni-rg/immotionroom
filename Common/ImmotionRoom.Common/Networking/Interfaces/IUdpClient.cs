namespace ImmotionAR.ImmotionRoom.Networking.Interfaces
{
    using System;
    using System.Threading.Tasks;

    public interface IUdpClient
    {
        event EventHandler<UdpMessageReceivedEventArgs> MessageReceived;
        
        void Close();

        Task SendAsync(byte[] packetBytes, int length, IPEndPoint udpMulticastGroupAddress);        
    }
}

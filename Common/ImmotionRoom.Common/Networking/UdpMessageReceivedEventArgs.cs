namespace ImmotionAR.ImmotionRoom.Networking
{
    using System;

    public class UdpMessageReceivedEventArgs : EventArgs
    {
        public IPEndPoint RemoteEndpoint { get; private set; }
        public byte[] Data { get; private set; }

        public UdpMessageReceivedEventArgs(IPEndPoint remoteEndpoint, byte[] data)
        {
            RemoteEndpoint = remoteEndpoint;
            Data = data;
        }

    }
}

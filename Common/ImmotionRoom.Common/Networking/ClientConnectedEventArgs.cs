namespace ImmotionAR.ImmotionRoom.Networking
{
    using System;
    using Interfaces;

    public class ClientConnectedEventArgs : EventArgs
    {
        public ITcpClient Client { get; private set; }

        public ClientConnectedEventArgs(ITcpClient client)
        {
            Client = client;
        }

    }
}

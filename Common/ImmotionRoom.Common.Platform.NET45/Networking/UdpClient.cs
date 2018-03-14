namespace ImmotionAR.ImmotionRoom.Networking
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Interfaces;

    public class UdpClient : IUdpClient
    {
        public event EventHandler<UdpMessageReceivedEventArgs> MessageReceived;

        private readonly System.Net.Sockets.UdpClient m_NativeUdpClient;
        private readonly CancellationTokenSource m_CancellationTokenSource;

        public UdpClient(System.Net.Sockets.UdpClient nativeUdpClient)
        {
            m_NativeUdpClient = nativeUdpClient;
            m_CancellationTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(Listener, m_CancellationTokenSource.Token, TaskCreationOptions.LongRunning);
        }

        public void Close()
        {
            // Stop Listener
            if (m_CancellationTokenSource != null)
            {
                m_CancellationTokenSource.Cancel();
            }
            
            try
            {
                m_NativeUdpClient.Close();
            }
            catch (System.Net.Sockets.SocketException e)
            {
                var ne = new NetworkException();
                ne.SocketErrorCode = (NetworkSocketError)e.SocketErrorCode;
                throw ne;
            }
        }

        public Task SendAsync(byte[] packetBytes, int length, IPEndPoint udpMulticastGroupAddress)
        {
#if !UNITY_5
            return Task.Run(() =>
#else
            return Task.Factory.StartNew(() =>
#endif
            {
                try
                {
                    var nativeEndpoint = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(udpMulticastGroupAddress.Address), udpMulticastGroupAddress.Port);
                    m_NativeUdpClient.Send(packetBytes, length, nativeEndpoint);
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    var ne = new NetworkException();
                    ne.SocketErrorCode = (NetworkSocketError) e.SocketErrorCode;
                    throw ne;
                }
            });
        }

        private void Listener(object args)
        {
            var remoteEndpoint = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0);

            while (!m_CancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    // Loop forever if not cancelled...
                    var receiveBytes = m_NativeUdpClient.Receive(ref remoteEndpoint);
                    OnMessageReceived(new IPEndPoint(remoteEndpoint.Address.ToString(), remoteEndpoint.Port), receiveBytes);
                }
                catch (System.Net.Sockets.SocketException)
                {
                    // Termination requested... IGNORE!
                }
                catch (OperationCanceledException)
                {
                    // Termination requested... IGNORE!
                }
            }
        }
        
        private void OnMessageReceived(IPEndPoint remoteEndpoint, byte[] data)
        {
            var localHandler = MessageReceived;
            if (localHandler != null)
            {
                localHandler(this, new UdpMessageReceivedEventArgs(remoteEndpoint, data));
            }
        }
    }
}

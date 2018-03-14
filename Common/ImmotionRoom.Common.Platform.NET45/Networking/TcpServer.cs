namespace ImmotionAR.ImmotionRoom.Networking
{
    using System;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Interfaces;
    using Logger;

    public class TcpServer : ITcpServer
    {
        public event EventHandler<ClientConnectedEventArgs> ClientConnected;

        private readonly ILogger m_Logger;
        private readonly System.Net.Sockets.TcpListener m_NativeTcpListener;
        private CancellationTokenSource m_CancellationTokenSource;

        public TcpServer(string ip, int port)
        {
            m_Logger = LoggerService.GetLogger<TcpServer>();
            m_NativeTcpListener = new TcpListener(System.Net.IPAddress.Parse(ip), port);
        }
        
        public Task StartAsync()
        {
            return Task.Run( () =>
            {
                try
                {
                    m_NativeTcpListener.Start();

                    m_CancellationTokenSource = new CancellationTokenSource();
                    Task.Run((Func<Task>)ClientListener, m_CancellationTokenSource.Token);
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    var ne = new NetworkException();
                    ne.SocketErrorCode = (NetworkSocketError)e.SocketErrorCode;
                    throw ne;
                }
            });
        }

        public void Stop()
        {
            // Stop Client Listener
            if(m_CancellationTokenSource != null)
            {
                m_CancellationTokenSource.Cancel();
            }
            
            try
            {
                m_NativeTcpListener.Stop();
            }
            catch (System.Net.Sockets.SocketException e)
            {
                var ne = new NetworkException();
                ne.SocketErrorCode = (NetworkSocketError) e.SocketErrorCode;
                throw ne;
            }
        }

        private async Task ClientListener()
        {
            while (true)
            {
                try
                {
                    var nativeTcpClient = await m_NativeTcpListener.AcceptTcpClientAsync().ConfigureAwait(false);
                    var client = new TcpClient(nativeTcpClient);
                    
                    if (client.Connected)
                    {
                        OnClientConnected(client);
                    }

                    // Wait for clients, if not stopped
                    if(m_CancellationTokenSource.IsCancellationRequested)
                    { 
                        break;
                    }
                }
                catch (NetworkException)
                {
                    // Ignore. Termination requested.
                    break;
                }
                catch (ObjectDisposedException)
                {
                    // Ignore. Termination requested.
                    break;
                }
                catch (OperationCanceledException)
                {
                    // Ignore. Termination requested.
                    break;
                }
            }
        }

        private void OnClientConnected(ITcpClient client)
        {
            var localHandler = ClientConnected;
            if (localHandler != null)
            {
                localHandler(this, new ClientConnectedEventArgs(client));
            }
        }
    }
}

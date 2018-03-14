namespace ImmotionAR.ImmotionRoom.Networking
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Interfaces;

    public class TcpClient : ITcpClient
    {
        private readonly System.Net.Sockets.TcpClient m_NativeTcpClient;
        private static readonly ManualResetEvent TimeoutObject = new ManualResetEvent(false);
        private static bool m_IsConnectionSuccessful;
        private static Exception m_SocketException;

        public TcpClient(System.Net.Sockets.TcpClient nativeClient)
        {
            m_NativeTcpClient = nativeClient;
        }

        public bool Connected { get { return m_NativeTcpClient.Client != null && m_NativeTcpClient.Client.Connected; } }

        public IPEndPoint RemoteEndPoint
        {
            get
            {
                try
                {
                    return m_NativeTcpClient.Client != null ? new IPEndPoint(m_NativeTcpClient.Client.RemoteEndPoint.ToString(), 0) : null;
                }
                catch (ObjectDisposedException)
                {
                    // Ignore, client disconnected
                }
                return null;
            }
        }

        public void Close()
        {
            try
            {
                m_NativeTcpClient.Close();
            }
            catch (System.Net.Sockets.SocketException e)
            {
                var ne = new NetworkException();
                ne.SocketErrorCode = Mappers.NativeSocketErrorToNetworkSocketError(e.SocketErrorCode);
                throw ne;
            }
        }

        public Task ConnectAsync(string ip, int port)
        {
            try
            {
#if !UNITY_5
                return m_NativeTcpClient.ConnectAsync(ip, port);
#endif

#if UNITY_5
                m_NativeTcpClient.Connect(ip, port);

                var taskSource = new TaskCompletionSource<object>();
                taskSource.SetResult(null);
                return taskSource.Task;
#endif
            }
            catch (System.Net.Sockets.SocketException e)
            {
                var ne = new NetworkException();
                ne.SocketErrorCode = Mappers.NativeSocketErrorToNetworkSocketError(e.SocketErrorCode);
                throw ne;
            }
            catch (System.ObjectDisposedException)
            {
                var ne = new NetworkException();
                ne.SocketErrorCode = Mappers.NativeSocketErrorToNetworkSocketError(SocketError.NotConnected);
                throw ne;
            }
        }

        public void ConnectWithinTimeout(string ip, int port, int timeoutInMilliseconds)
        {
            try
            {
                TimeoutObject.Reset();
                m_SocketException = null;

                m_NativeTcpClient.BeginConnect(ip, port, CallBackMethod, m_NativeTcpClient);

                if (TimeoutObject.WaitOne(timeoutInMilliseconds, false))
                {
                    if (m_IsConnectionSuccessful)
                    {
                        return;
                    }

                    if (m_SocketException != null)
                    {
                        throw m_SocketException;
                    }
                }

                m_NativeTcpClient.Close();

                throw new TimeoutException("Timeout Exception");
            }
            catch (System.Net.Sockets.SocketException e)
            {
                var ne = new NetworkException();
                ne.SocketErrorCode = Mappers.NativeSocketErrorToNetworkSocketError(e.SocketErrorCode);
                throw ne;
            }
        }

        public Stream GetStream()
        {
            try
            { 
                return m_NativeTcpClient.GetStream();
            }
            catch (System.Net.Sockets.SocketException e)
            {
                var ne = new NetworkException();
                ne.SocketErrorCode = Mappers.NativeSocketErrorToNetworkSocketError(e.SocketErrorCode);
                throw ne;
            }
            catch (System.InvalidOperationException)
            {
                var ne = new NetworkException();
                ne.SocketErrorCode = Mappers.NativeSocketErrorToNetworkSocketError(SocketError.NotConnected);
                throw ne;
            }
        }

        private static void CallBackMethod(IAsyncResult asyncresult)
        {
            try
            {
                m_IsConnectionSuccessful = false;
                var tcpClient = asyncresult.AsyncState as System.Net.Sockets.TcpClient;

                if (tcpClient != null)
                {
                    tcpClient.EndConnect(asyncresult);
                    m_IsConnectionSuccessful = true;
                }
            }
            catch (Exception ex)
            {
                m_IsConnectionSuccessful = false;
                m_SocketException = ex;
            }
            finally
            {
                TimeoutObject.Set();
            }
        }

       
    }
}

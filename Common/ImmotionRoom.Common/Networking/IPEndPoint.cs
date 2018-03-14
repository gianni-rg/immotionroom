namespace ImmotionAR.ImmotionRoom.Networking
{
    using System;
    using Interfaces;

    public class IPEndPoint
    {
        public IPEndPoint(string ip, int port)
        {
            Address = ip;
            Port = port;
        }

        public string Address { get; private set; }
        public int Port { get; private set; }


        public override string ToString()
        {
            if (Address.Contains(":"))
            {
                return Address;
            }
            else
            {
                return string.Format("{0}:{1}", Address, Port);
            }
        }

        public static bool TestReachability(ITcpClient client, string ip, int port, int timeout)
        {
            try
            {
                client.ConnectWithinTimeout(ip, port, timeout);
                if (client.Connected)
                {
                    client.Close();
                    return true;
                }
                return false;
            }
            catch (TimeoutException)
            {
                //if (m_Logger.IsDebugEnabled)
                //{
                //    m_Logger.Warn("DataSourceIsReachableAsync: Connect() error: timeout");
                //}
                return false;
            }
            catch (NetworkException networkException)
            {
                if (networkException.SocketErrorCode != NetworkSocketError.ConnectionRefused && networkException.SocketErrorCode != NetworkSocketError.TimedOut)
                {
                    //if (m_Logger.IsErrorEnabled)
                    //{
                    //    m_Logger.Error(networkException, "DataSourceIsReachableAsync: Connect() Socket Error: {0}", networkException.Message);
                    //}
                }
                return false;
            }
            catch (Exception)
            {
                //if (m_Logger.IsErrorEnabled)
                //{
                //    m_Logger.Error(e, "DataSourceIsReachableAsync: Connect() error: {0}", e.Message);
                //}
                return false;
            }
        }
    }
}

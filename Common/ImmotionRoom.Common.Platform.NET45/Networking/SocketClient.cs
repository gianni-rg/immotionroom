namespace ImmotionAR.ImmotionRoom.Networking
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using Interfaces;
    using Logger;

    // Inspired to SocketClient in Coding4Fun.Kinect.KinectService project
    // Copyright (C) Microsoft Corporation.
    // This source is subject to the Microsoft Public License (Ms-PL).
    // Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
    // All other rights reserved.

    public class SocketClient : INetworkClient
    {
        #region Private fields
        
        private readonly ILogger m_Logger;
        private readonly ITcpClient m_Client;

        #endregion

        #region Properties

        public string Id { get; private set; }

        public bool IsConnected
        {
            get
            {
                try
                {
                    return m_Client != null && m_Client.Connected;
                }
                catch (ObjectDisposedException)
                {
                    // Ignore. Termination requested.
                    //if (m_Logger.IsErrorEnabled)
                    //{
                    //    m_Logger.Error(ex, "SocketClient.IsConnected ObjectDisposedException");
                    //}
                    return false;
                }
                catch (NullReferenceException)
                {
                    // Ignore. Termination requested.
                    //if (m_Logger.IsErrorEnabled)
                    //{
                    //    m_Logger.Error(ex, "SocketClient.IsConnected NullReferenceException");
                    //}
                    return false;
                }
            }
        }

        public IPEndPoint RemoteEndPoint
        {
            get
            {
                try
                {
                    if (m_Client != null)
                    {
                        return m_Client.RemoteEndPoint;
                    }
                }
                catch (ObjectDisposedException)
                {
                    // Ignore. Client disconnected.
                }
                catch (NullReferenceException)
                {
                    // Ignore. Client disconnected.
                }

                return null;
            }
        }

        #endregion

        #region Constructor

        public SocketClient(ITcpClient client, string id)
        {
            m_Logger = LoggerService.GetLogger<SocketClient>();

            m_Client = client;

            Id = id;
        }

        #endregion

        #region Methods

        public bool Send(byte[] data, int length)
        {
            try
            {
                if (IsConnected)
                {
                    var stream = m_Client.GetStream();
                    stream.BeginWrite(data, 0, length, WriteCompleted, stream);
                }
                else
                {
                    return false;
                }
            }
            catch (IOException ex)
            {
                var exception = ex.InnerException as SocketException;
                if (exception != null)
                {
                    var socketException = exception;
                    var socketError = Mappers.NativeSocketErrorToNetworkSocketError(socketException.SocketErrorCode);
                    if (socketError != NetworkSocketError.ConnectionAborted && socketError != NetworkSocketError.ConnectionReset)
                    {
                        if (m_Logger.IsErrorEnabled)
                        {
                            m_Logger.Error(ex, "SocketClient.Send() IOException: {0}", socketException.SocketErrorCode);
                        }
                    }
                }
            }
            catch (ObjectDisposedException ex)
            {
                if (m_Logger.IsErrorEnabled)
                {
                    m_Logger.Error(ex, "SocketClient.Send() ObjectDisposedException");
                }
            }

            return true;
        }

        public bool Send(byte[] data)
        {
            return Send(data, data.Length);
        }

        public void Close()
        {
            if (m_Client != null && m_Client.Connected)
            {
                m_Client.Close();
            }
        }

        public int ReadClientStreamMode()
        {
            try
            {
                if (m_Client != null && m_Client.Connected)
                {
                    var ns = m_Client.GetStream();
                    var reader = new BinaryReader(ns);
                    return reader.ReadInt32();
                }
            }
            catch (EndOfStreamException)
            {
                // Ignore. Client disconnected.
            }
            catch (IOException)
            {
                // Ignore. Client disconnected.
            }
            catch (NullReferenceException)
            {
                // Ignore. Client disconneted.
            }
            catch (Exception e)
            {
                if (m_Logger != null && m_Logger.IsDebugEnabled)
                {
                    m_Logger.Error(e, "SocketClient.ReadClientStreamMode() Exception: {0}", e.Message);
                }
            }

            return 0;
        }

        #endregion

        #region Private methods

        private void WriteCompleted(IAsyncResult ar)
        {
            try
            {
                if (IsConnected)
                {
                    var ns = (Stream) ar.AsyncState;
                    ns.EndWrite(ar);
                }
            }
            catch (IOException ex)
            {
                var exception = ex.InnerException as SocketException;
                if (exception != null)
                {
                    var socketException = exception;
                    var socketError = Mappers.NativeSocketErrorToNetworkSocketError(socketException.SocketErrorCode);
                    if (socketError != NetworkSocketError.ConnectionAborted && socketError != NetworkSocketError.ConnectionReset)
                    {
                        if (m_Logger.IsErrorEnabled)
                        {
                            m_Logger.Error(ex, "SocketClient.WriteCompleted() IOException: {0}", socketException.SocketErrorCode);
                        }
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // Ignore

                //if (m_Logger.IsErrorEnabled)
                //{
                //    m_Logger.Error(ex, "SocketClient.WriteCompleted() ObjectDisposedException");
                //}
            }
        }

        #endregion
    }
}

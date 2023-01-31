using UnityEngine;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Net;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Collections.Concurrent;
namespace LitEngine.Net
{
    public class SocketNetBase<T> : NetBase<T> where T : SocketNetBase<T>
    {
        protected Socket mSocket = null;

        override public bool isConnected { get { return mState == TcpState.Connected && mSocket != null; } }

        protected SocketNetBase() : base()
        {

            for (int i = 0; i < 60; i++)
            {
                SocketAsyncEventArgs sd = new SocketAsyncEventArgs();
                sd.Completed += SendAsyncCallback;
                sd.SocketFlags = SocketFlags.None;
                cacheAsyncEvent.Enqueue(sd);
            }

            receiveAsyncEvent = new SocketAsyncEventArgs();
            receiveAsyncEvent.Completed += ReceiveAsyncCallback;
            receiveAsyncEvent.SocketFlags = SocketFlags.None;
        }

        override sealed protected void RestSocketInfo()
        {
            if (mSocket == null) return;
            mSocket.NoDelay = socketNoDelay;
            mSocket.ReceiveTimeout = mRecTimeOut;
            mSocket.SendTimeout = mSendTimeout;
            mSocket.ReceiveBufferSize = mReceiveBufferSize;
            mSocket.SendBufferSize = mSendBufferSize;
        }

        override sealed protected void KillSocket()
        {
            try
            {
                if (mSocket != null)
                {
                    if (mSocket.ProtocolType == ProtocolType.Tcp && mSocket.Connected)
                    {
                        mSocket.Shutdown(SocketShutdown.Both);
                    }
                    mSocket.Close();
                }
            }
            catch
            {
                // ignored
            }

            mSocket = null;
        }
    }
}


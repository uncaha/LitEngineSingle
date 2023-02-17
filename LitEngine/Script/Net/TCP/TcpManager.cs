using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Net;
using LitEngine.Net.KCPCommand;

namespace LitEngine.Net
{
    public abstract class TcpManager<T> : SocketNetBase<T> where T : TcpManager<T>
    {
        override public bool isConnected
        {
            get { return base.isConnected && mSocket.Connected; }
        }

        #region 构造析构

        protected TcpManager() : base()
        {
            mNetTag = GetType().Name;
        }

        #endregion


        #region 建立Socket

        private ConnectMessage connectMsg = null;
        override public void ConnectToServer(System.Action<bool> pOnDone)
        {
            if (IsCOrD())
            {
                DLog.LogError(mNetTag + string.Format("[{0}]Closing or Connecting.", mNetTag));
                
                pOnDone?.Invoke(false);
                return;
            }

            if (isConnected)
            {
                DLog.LogError(mNetTag + string.Format("[{0}] Connected now.", mNetTag));
                pOnDone?.Invoke(false);
                return;
            }

            mState = TcpState.Connecting;

            connectMsg = new ConnectMessage();
            connectMsg.OnDone = pOnDone;
            
            System.Threading.Tasks.Task.Run(ThreatConnect);
        }

        private bool TCPConnect()
        {
            bool ret = false;
            List<IPAddress> tipds = GetServerIpAddress(mHostName);
            if (tipds.Count == 0) DLog.LogError("IPAddress List.Count = 0!");

            foreach (IPAddress tip in tipds)
            {
                try
                {
                    DLog.Log(string.Format("[Start Connect]" + " HostName:{0} IpAddress:{1} AddressFamily:{2}",
                        mHostName, tip.ToString(), tip.AddressFamily.ToString()));
                    mSocket = new Socket(tip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    RestSocketInfo();
                    mSocket.Connect(tip, mPort);
                    DLog.Log("Connected!");
                    ret = true;
                    break;
                }
                catch (Exception e)
                {
                    DLog.LogError(string.Format(
                        "[Connect Error]" + " HostName:{0} IpAddress:{1} AddressFamily:{2} ErrorMessage:{3}", mHostName,
                        tip.ToString(), tip.AddressFamily.ToString(), e.ToString()));
                }
            }

            return ret;
        }

        private void ThreatConnect()
        {
            bool isOK = TCPConnect();
            string tmsg = "";
            if (isOK)
            {
                try
                {
                    mStartThread = true;

                    #region 收发;

                    CreatRec();

                    #endregion

                    DLog.Log("收发线程启动!");
                }
                catch (Exception e)
                {
                    tmsg = e.ToString();
                    CloseSRThread();
                    isOK = false;
                }
            }

            if (!isOK)
            {
                CloseSRThread();

                connectMsg.result = false;
                AddMainThreadMsgReCall(connectMsg);
                
                DLog.Log( $"{mNetTag} 建立连接失败.");
            }
            else
            {
                mState = TcpState.Connected;
                connectMsg.result = true;
                AddMainThreadMsgReCall(connectMsg);
                
                DLog.Log( $"{mNetTag} 建立连接成功.");
            }
        }

        private void CreatRec()
        {
            StartReceiveAsync();
        }

        #endregion

        #region 发送

        override public bool Send(SendData pData)
        {
            if (mSocket == null || pData == null) return false;

            bool rv = Send(pData.Data, pData.SendLen);
            DebugMsg(pData.Cmd, pData.Data, 0, pData.SendLen, "TCPSend");
            return rv;
        }

        override public bool Send(byte[] pBuffer, int pSize)
        {
            if (mSocket == null || pBuffer == null) return false;
            SocketAsyncEventArgs sd = GetSocketAsyncEvent();
            sd.SetBuffer(pBuffer, 0, pSize);
            return mSocket.SendAsync(sd);
        }

        override protected void SendAsyncCallback(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
            }
            else
            {
                DLog.Log(mNetTag + ":SendMessageThread->" + e.SocketError);
                OnNetError(MessageType.SendError, mNetTag + "-" + e.ToString());
            }

            base.SendAsyncCallback(sender, e);
        }

        #endregion

        #region　接收

        private void StartReceiveAsync()
        {
            if (mSocket == null) return;
            receiveAsyncEvent.SetBuffer(mRecbuffer, 0, mReadMaxLen);
            if (!mSocket.ReceiveAsync(receiveAsyncEvent))
            {
                ReceiveAsyncCallback(mSocket, receiveAsyncEvent);
            }
        }

        override protected void ReceiveAsyncCallback(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                if (e.BytesTransferred > 0)
                {
                    Processingdata(e.BytesTransferred, mRecbuffer);
                }

                StartReceiveAsync();
            }
            else
            {
                SocketError ttag = e.SocketError;
                DLog.Log(mNetTag + ":ReceiveMessage->" + ttag);

                OnNetError(MessageType.ReceiveError, mNetTag + "-" + ttag);
            }
        }

        override protected void Processingdata(int _len, byte[] _buffer)
        {
            base.Processingdata(_len, _buffer);
        }

        override protected void PushRecData(byte[] pBuffer, int pSize)
        {
            mBufferData.Push(pBuffer, pSize);
            while (mBufferData.IsFullData())
            {
                ReceiveData tssdata = mBufferData.GetReceiveData();
                mResultDataList.Enqueue(tssdata);
                DebugMsg(tssdata.Cmd, tssdata.Data, 0, tssdata.Len, "接收-ReceiveData");
            }
        }

        #endregion

        override protected void MainThreadUpdate()
        {
            UpdateRecMsg();
        }
    }
}
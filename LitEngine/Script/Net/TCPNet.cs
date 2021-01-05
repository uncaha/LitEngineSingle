using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Net;
namespace LitEngine.Net
{
    public sealed class TCPNet : NetBase<TCPNet>
    {
        override public bool isConnected { get { return base.isConnected && mSocket.Connected; } }
        private AsyncCallback sendCallBack;
        #region 构造析构
        private TCPNet() : base()
        {
            mNetTag = "TCP";
            sendCallBack = SendAsyncCallback;
        }

        #endregion


        #region 建立Socket


        override public void ConnectToServer()
        {
            if (IsCOrD())
            {
                DLog.LogError(mNetTag + string.Format("[{0}]Closing or Connecting.", mNetTag));
                return;
            }

            if (isConnected)
            {
                DLog.LogError(mNetTag + string.Format("[{0}] Connected now.", mNetTag));
                return;
            }
            
            TCPConnect();
        }

        private bool TCPConnect()
        {
            mState = TcpState.Connecting;
            bool ret = false;
            List<IPAddress> tipds = GetServerIpAddress(mHostName);
            if (tipds.Count == 0) DLog.LogError("IPAddress List.Count = 0!");
            try
            {
                IPAddress tip = tipds[0];
                DLog.Log(string.Format("[Start Connect]" + " HostName:{0} IpAddress:{1} AddressFamily:{2}", mHostName, tip.ToString(), tip.AddressFamily.ToString()));
                mSocket = new Socket(tip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                RestSocketInfo();
                mSocket.BeginConnect(tip,mPort,ConnectCallback,mSocket);
                ret = true;
            }
            catch (Exception e)
            {
                string terror = e.ToString();
                DLog.LogError(terror);
                mState = TcpState.Closed;
                AddMainThreadMsgReCall(GetMsgReCallData(MessageType.ConectError, mNetTag + "Connect fail. error:" + terror));
            }

            return ret;
        }

        private void ConnectCallback(IAsyncResult async)
        {
            bool isOK = false;
            string tmsg = "";
            try
            {
                Socket client = (Socket)async.AsyncState;
                client.EndConnect(async);
                if (client.Connected)
                {
                    mStartThread = true;

                    #region 接收;
                    CreatRec();
                    #endregion
                    DLog.Log("TCP Connected!");
                    isOK = true;
                }
                else
                {
                    isOK = false;
                    tmsg = "Connect fail.try again.";
                }

            }
            catch (Exception e)
            {
                tmsg = e.ToString();
            }

            if (!isOK)
            {
                CloseSRThread();
                mState = TcpState.Closed;
                AddMainThreadMsgReCall(GetMsgReCallData(MessageType.ConectError, mNetTag + "Connect fail. " + tmsg));
            }
            else
            {
                mState = TcpState.Connected;
                AddMainThreadMsgReCall(GetMsgReCallData(MessageType.Connected, mNetTag + " Connected."));
            }
        }

        private void CreatRec()
        {
            mRecThread = new Thread(ReceiveMessage);
            mRecThread.IsBackground = true;
            mRecThread.Start();
        }

        #endregion

        #region 发送

        override public bool Send(SendData _data)
        {
            if (_data == null)
            {
                DLog.LogError("试图添加一个空对象到发送队列!AddSend");
                return false;
            }
            DebugMsg(_data.Cmd, _data.Data, 0, _data.SendLen, "TCPSend");
            return Send(_data.Data, _data.SendLen);
        }

        override public bool Send(byte[] pBuffer, int pSize)
        {
            if (mSocket == null) return false;
            try
            {
                SocketError errorCode = SocketError.Success;
                var ar = mSocket.BeginSend(pBuffer, 0, pSize, SocketFlags.None, out errorCode, sendCallBack, pBuffer);
                if (errorCode != SocketError.Success)
                {
                    DLog.LogErrorFormat("TCP Send Error.{0}", errorCode);
                }
                return errorCode == SocketError.Success;
            }
            catch (System.Exception erro)
            {
                DLog.LogFormat("TCP Send Error.{0}", erro);
            }
            return false;
        }

        void SendAsyncCallback(IAsyncResult result)
        {
            int tsendLen = mSocket.EndSend(result);
            byte[] tadata = result.AsyncState as byte[];
            if (tadata != null)
            {
                DebugMsg(-1, tadata, 0, tsendLen, "TCPSend", result.IsCompleted);
            }
            if (result.IsCompleted == false)
            {
                AddMainThreadMsgReCall(new NetMessage(MessageType.SendError, mNetTag + "-" + result.IsCompleted));
            }
        }

        #endregion

        #region　接收

        private void ReceiveMessage()
        {
            DLog.Log("TCP Start ReceiveMessage");
            try
            {
                while (mStartThread)
                {

                    if (mSocket.Available != 0)
                    {
                        int receiveNumber = 0;
                        receiveNumber = mSocket.Receive(mRecbuffer, 0, mReadMaxLen, SocketFlags.None);
                        if (receiveNumber > 0)
                            Processingdata(receiveNumber, mRecbuffer);

                    }
                }

            }
            catch (Exception e)
            {
                if (mStartThread)
                {
                    DLog.LogError(mNetTag + ":ReceiveMessage->" + e.ToString());
                    CloseSRThread();
                    AddMainThreadMsgReCall(GetMsgReCallData(MessageType.ReceiveError, mNetTag + "-" + e.ToString()));
                }
            }
            DLog.Log("TCP End ReceiveMessage");
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
                ReceiveData tssdata = null;
                int tfirstLength = mBufferData.GetFirstDataLength();
                bool tissmall = tfirstLength <= cacheObjectLength;

                if (tissmall && cacheRecDatas.PopCount <= 0)
                {
                    cacheRecDatas.Switch();
                }

                if (cacheRecDatas.PopCount > 0 && tissmall)
                {
                    tssdata = cacheRecDatas.Dequeue(); 
                }
                else
                {
                    int tlen = tissmall ? cacheObjectLength : tfirstLength;
                    tssdata = new ReceiveData(tlen);
                    tssdata.useCache = tissmall;
                }
                mBufferData.SetReceiveData(tssdata);
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
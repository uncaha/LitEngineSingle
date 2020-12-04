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
                DLog.LogError(mNetTag + string.Format("[{0}]Closing或Connecting状态下不可执行.", mNetTag));
                return;
            }

            if (isConnected)
            {
                AddMainThreadMsgReCall(GetMsgReCallData(MSG_RECALL.ConectError, mNetTag + "重复建立连接"));
                return;
            }
            mState = TcpState.Connecting;
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
                    DLog.Log(string.Format("[开始连接]" + " HostName:{0} IpAddress:{1} AddressFamily:{2}", mHostName, tip.ToString(), tip.AddressFamily.ToString()));
                    mSocket = new Socket(tip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    RestSocketInfo();
                    mSocket.Connect(tip, mPort);
                    DLog.Log("连接成功!");
                    ret = true;
                    break;
                }
                catch (Exception e)
                {
                    DLog.LogError(string.Format("[网络连接异常]" + " HostName:{0} IpAddress:{1} AddressFamily:{2} ErrorMessage:{3}", mHostName, tip.ToString(), tip.AddressFamily.ToString(), e.ToString()));
                }
            }

            return ret;
        }

        private void ThreatConnect()
        {

            bool tok = TCPConnect();
            string tmsg = "";
            if (tok)
            {
                try
                {
                    mStartThread = true;

                    #region 接收;
                    CreatRec();
                    #endregion

                    DLog.Log("收发线程启动!");
                }
                catch (Exception e)
                {
                    tmsg = e.ToString();
                    CloseSRThread();
                    tok = false;
                }

            }

            if (!tok)
            {
                mState = TcpState.Closed;
                AddMainThreadMsgReCall(GetMsgReCallData(MSG_RECALL.ConectError, mNetTag + "建立连接失败. " + tmsg));
            }
            else
            {
                mState = TcpState.Connected;
                AddMainThreadMsgReCall(GetMsgReCallData(MSG_RECALL.Connected, mNetTag + "建立连接完成."));
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
                AddMainThreadMsgReCall(new MSG_RECALL_DATA(MSG_RECALL.SendError, mNetTag + "-" + result.IsCompleted));
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
                    AddMainThreadMsgReCall(GetMsgReCallData(MSG_RECALL.ReceiveError, mNetTag + "-" + e.ToString()));
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
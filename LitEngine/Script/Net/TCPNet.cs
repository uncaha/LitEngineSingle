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
        #region 构造析构
        private TCPNet() : base()
        {
            mNetTag = "TCP";
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
                    DLog.Log(string.Format("[Start Connect]" + " HostName:{0} IpAddress:{1} AddressFamily:{2}", mHostName, tip.ToString(), tip.AddressFamily.ToString()));
                    mSocket = new Socket(tip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    RestSocketInfo();
                    mSocket.Connect(tip, mPort);
                    DLog.Log("Connected!");
                    ret = true;
                    break;
                }
                catch (Exception e)
                {
                    DLog.LogError(string.Format("[Connect Error]" + " HostName:{0} IpAddress:{1} AddressFamily:{2} ErrorMessage:{3}", mHostName, tip.ToString(), tip.AddressFamily.ToString(), e.ToString()));
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
                    CreatSend();
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
            mRecThread.Priority = System.Threading.ThreadPriority.Lowest;
            mRecThread.Start();
        }

        private void CreatSend()
        {
            mSendThread = new Thread(SendMessageThread);
            mSendThread.IsBackground = true;
            mSendThread.Priority = System.Threading.ThreadPriority.Lowest;
            mSendThread.Start();
        }

        #endregion

        #region 发送

        private void SendMessageThread()
        {
            while (mStartThread)
            {
                try
                {

                    if (mSendDataList.PushCount == 0) continue;

                    mSendDataList.Switch();
                    for (int i = 0, length = mSendDataList.PopCount; i < length; i++)
                    {
                        var tdata = mSendDataList.Dequeue();
                        int tsendlen = ThreadSend(tdata.Data, tdata.SendLen);
                        DebugMsg(tdata.Cmd, tdata.Data, 0, tsendlen, "TCPSend");
                    }
                    Thread.Sleep(10);
                }
                catch (Exception e)
                {
                    if (mStartThread)
                    {
                        DLog.LogError(mNetTag + ":SendMessageThread->" + e.ToString());
                        CloseSRThread();
                        AddMainThreadMsgReCall(new NetMessage(MessageType.SendError, mNetTag + "-" + e.ToString()));
                        return;
                    }
                }

            }

        }

        private int ThreadSend(byte[] pBuffer, int pSize)
        {
            if (mSocket == null) return 0;
            return mSocket.Send(pBuffer, pSize, SocketFlags.None);
        }

        override public bool Send(SendData pData)
        {
            if (mSocket == null || pData == null) return false;
            mSendDataList.Enqueue(pData);
            return true;
        }

        override public bool Send(byte[] pBuffer, int pSize)
        {
            if (mSocket == null || pBuffer == null) return false;
            SendData tdata = new SendData(-1, pBuffer, pSize);
            mSendDataList.Enqueue(tdata);
            return true;
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
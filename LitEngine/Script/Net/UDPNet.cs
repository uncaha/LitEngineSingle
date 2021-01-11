using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Threading;
namespace LitEngine.Net
{
    public sealed class UDPNet : NetBase<UDPNet>
    {
        #region socket属性
        private IPEndPoint mTargetPoint;//目标地址
        private EndPoint mRecPoint;
        private IPAddress mServerIP;
        private int mLocalPort = 30379;
        #endregion

        #region 构造析构

        private UDPNet() : base()
        {
            mNetTag = "UDP";
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
            System.Threading.Tasks.Task.Run(UDPConnect);
        }

        private void UDPConnect()
        {
            try
            {
                var ips = GetServerIpAddress(mHostName);
                mServerIP = ips[0];
                mSocket = new Socket(mServerIP.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                mTargetPoint = new IPEndPoint(mServerIP, mPort);
                mRecPoint = new IPEndPoint(mServerIP, mPort);

                int tempport = mLocalPort;
                while (true)
                {
                    try
                    {
                        IPEndPoint tMypoint = new IPEndPoint(IPAddress.Any, tempport);
                        mSocket.Bind(tMypoint);
                        mLocalPort = tempport;

                        break;
                    }
                    catch
                    {
                        tempport++;
                    }
                }
                mStartThread = true;
                CreatSendAndRecThread();
                mState = TcpState.Connected;
                AddMainThreadMsgReCall(GetMsgReCallData(MessageType.Connected, mNetTag + "建立连接完成."));
            }
            catch (Exception ex)
            {
                DLog.LogError(ex);
                CloseSRThread();
                mState = TcpState.Closed;
                AddMainThreadMsgReCall(GetMsgReCallData(MessageType.ConectError, mNetTag + "建立连接失败. " + ex.Message));
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

        private void CreatSendAndRecThread()
        {
            CreatRec();
            CreatSend();
            DLog.Log(mNetTag + "建立连接完成");
        }

        #endregion


        #region 属性设置方法
        public void ChangetTargetPoint(IPEndPoint _tar)
        {
            mTargetPoint = _tar;
        }
        #endregion

        #region 收发
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
                        DebugMsg(tdata.Cmd, tdata.Data, 0, tsendlen, "UDPSend");
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
            return mSocket.SendTo(pBuffer, 0, pSize, SocketFlags.None, mTargetPoint);
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

        #region thread rec
        private void ReceiveMessage()
        {
            while (mStartThread)
            {
                try
                {
                    if (mSocket.Available != 0)
                    {
                        int receiveNumber = mSocket.ReceiveFrom(mRecbuffer, SocketFlags.None, ref mRecPoint);
                        IPEndPoint tremot = (IPEndPoint)mRecPoint;

                        if (receiveNumber > 0 && tremot.Address.Equals(mServerIP))
                            Processingdata(receiveNumber, mRecbuffer);
                    }

                }
                catch (Exception e)
                {
                    DLog.LogError("UDP接收线程异常:" + e + "|" + "|" + mServerIP);
                }
                Thread.Sleep(1);
            }

        }
        #endregion

        override protected void PushRecData(byte[] pBuffer, int pSize)
        {
            try
            {
                ReceiveData tssdata = null;
                bool tissmall = pSize <= cacheObjectLength;

                if(tissmall && cacheRecDatas.PopCount <= 0)
                {
                    cacheRecDatas.Switch();
                }

                if (cacheRecDatas.PopCount > 0 && tissmall)
                {
                    tssdata = cacheRecDatas.Dequeue();
                }
                else
                {
                    int tlen = tissmall ? cacheObjectLength : pSize;
                    tssdata = new ReceiveData(tlen);
                    tssdata.useCache = tissmall;
                }
                tssdata.CopyBuffer(pBuffer, 0);
                mResultDataList.Enqueue(tssdata);
                DebugMsg(tssdata.Cmd, tssdata.Data, 0, tssdata.Len, "接收-ReceiveData");
            }
            catch (Exception e)
            {
                DLog.LogError(mNetTag + "-" + e.ToString());
            }

        }
        #endregion

        override protected void MainThreadUpdate()
        {
            UpdateRecMsg();
        }

        #endregion
    }
}
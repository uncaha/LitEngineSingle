using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Collections;
using System.Threading;
using System.Runtime.InteropServices;

using LitEngine.Net.KCPCommand;
namespace LitEngine.Net
{
    public sealed class KCPNet : NetBase<KCPNet>
    {
        public class CacheByteObject
        {
            public byte[] bytes;
            public int length;
            public void Initialize()
            {
                if(bytes != null)
                {
                    bytes.Initialize();
                }
                length = 0;
            }
            override public string ToString()
            {
                System.Text.StringBuilder bufferstr = new System.Text.StringBuilder();
                bufferstr.AppendFormat("length = {0},bytes = ", length);
                bufferstr.Append("{");
                for (int i = 0; i < length; i++)
                {
                    if (i != 0)
                        bufferstr.Append(",");
                    bufferstr.Append(bytes[i]);
                }
                bufferstr.Append("}");

                return bufferstr.ToString();
            }
        }
        #region socket属性
        private IPEndPoint mTargetPoint;//目标地址
        private EndPoint mRecPoint;
        private IPAddress mServerIP;
        private int mLocalPort = 30379;
        private KCP kcpObject;

        private byte[] kcpRecvBuffer = new byte[4096];
        private int cacheByteLen = 2048;
        #endregion
        #region 初始化
        private KCPNet() : base()
        {
            mNetTag = "KCP";
            kcpObject = new KCP(1, HandleKcpSend);
            kcpObject.NoDelay(1, 10, 2, 1);
            kcpObject.WndSize(128, 128);
        }
        #endregion

        static public void NoDelay(int nodelay_, int interval_, int resend_, int nc_)
        {
            Instance.kcpObject.NoDelay(nodelay_,interval_,resend_,nc_);
        }

        static public void WndSize(int sndwnd, int rcvwnd)
        {
            Instance.kcpObject.WndSize(sndwnd,rcvwnd);
        }

        override public void Dispose()
        {
            if (kcpObject != null)
            {
                kcpObject.Dispose();
                kcpObject = null;
            }
            base.Dispose();
        }

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
            System.Threading.Tasks.Task.Run(KCPConnect);
        }

        private void KCPConnect()
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

        override protected void CloseSocket()
        {
            base.CloseSocket();
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
                        DebugMsg(tdata.Cmd, tdata.Data, 0, tsendlen, "KCPSend");
                    }
                    UpdateKCP();
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
            return kcpObject.Send(pBuffer, pSize);
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

        private void HandleKcpSend(byte[] buff, int size)
        {
            if (mSocket == null) return;
            mSocket.SendTo(buff, 0, size, SocketFlags.None, mTargetPoint);
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
                        int receiveNumber = mSocket.ReceiveFrom(mRecbuffer, cacheByteLen, SocketFlags.None, ref mRecPoint);
                        IPEndPoint tremot = (IPEndPoint)mRecPoint;

                        if (receiveNumber > 0 && tremot.Address.Equals(mServerIP))
                            Processingdata(receiveNumber, mRecbuffer);
                    }

                }
                catch (Exception e)
                {
                    DLog.LogError("KCP接收线程异常:" + e + "|" + "|" + mServerIP);
                }
                Thread.Sleep(1);
            }

        }
        #endregion

        override protected void Processingdata(int _len, byte[] _buffer)
        {
            try
            {
                PushKCPData(_buffer, _len);
            }
            catch (Exception e)
            {
                DLog.LogError(mNetTag + "-" + e.ToString());
            }

        }

        private void PushKCPData(byte[] pBuffer,int pLength)
        {
            var recvobject = pBuffer;
            int ret = kcpObject.Input(pBuffer,pLength);

            if (ret < 0)
            {
                //收到的不是一个正确的KCP包
                if (IsShowDebugLog)
                {
                    DLog.Log("Error kcp package.");
                }
                return;
            }

            needKcpUpdateFlag = true;

            for (int size = kcpObject.PeekSize(); size > 0; size = kcpObject.PeekSize())
            {
                if (size > 1048576)
                {
                    DLog.LogErrorFormat("The size is too long.size = {0}", size);
                }
                if (kcpRecvBuffer.Length < size)
                {
                    int tnewlen = size + kcpRecvBuffer.Length;
                    kcpRecvBuffer = new byte[tnewlen];
                }
                else
                {
                    kcpRecvBuffer.Initialize();
                }

                int treclen = kcpObject.Recv(kcpRecvBuffer, size);
                if (treclen > 0)
                {
                    PopRecData(kcpRecvBuffer, treclen);
                }
            }
        }

        private void PopRecData(byte[] pRecbuf, int pSize)
        {
            DebugMsg(-1, pRecbuf, 0, pSize, "接收-bytes");
            if (receiveOutput == null)
            {
                PushRecData(pRecbuf, pSize);
            }
            else
            {
                OutputToDelgate(pRecbuf, pSize);
            }
        }

        override protected void PushRecData(byte[] pBuffer, int pSize)
        {
            try
            {
                ReceiveData tssdata = new ReceiveData();
                tssdata.CopyBuffer(pBuffer, 0);
                mResultDataList.Enqueue(tssdata);
                DebugMsg(tssdata.Cmd, tssdata.Data, 0, tssdata.Len, "接收-ReceiveData");
            }
            catch (System.Exception error)
            {
                DLog.LogError(error);
            }
        }
        #endregion

        private static readonly DateTime UTCTimeBegin = new DateTime(1970, 1, 1);
        public static UInt32 GetClockMS()
        {
            return (UInt32)(Convert.ToInt64(DateTime.UtcNow.Subtract(UTCTimeBegin).TotalMilliseconds) & 0xffffffff);
        }

        private bool needKcpUpdateFlag = false;
        private uint nextKcpUpdateTime = 0;

        private void UpdateKCP()
        {
            uint currentTimeMS = GetClockMS();
            if (needKcpUpdateFlag || currentTimeMS >= nextKcpUpdateTime)
            {
                kcpObject.Update(currentTimeMS);
                nextKcpUpdateTime = kcpObject.Check(currentTimeMS);
                needKcpUpdateFlag = false;
            }
        }

        override protected void MainThreadUpdate()
        {
            UpdateRecMsg();
        }

        #endregion

    }
}

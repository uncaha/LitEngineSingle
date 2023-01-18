using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Collections;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using LitEngine.Net.KCPCommand;
namespace LitEngine.Net
{
    public abstract class KCPManager<T> : NetBase<T> where T : KCPManager<T>
    {
        private static T typeInstance => Instance;
        public class CacheByteObject
        {
            public byte[] bytes;
            public int length;
            public void Initialize()
            {
                if (bytes != null)
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
        private int mLocalPort = 10824;

        private KCP kcpObject;
        private ConcurrentQueue<CacheByteObject> recvQueue = new ConcurrentQueue<CacheByteObject>();
        private byte[] kcpRecvBuffer = new byte[4096];

        private int cacheByteLen = 2048;
        private ConcurrentQueue<CacheByteObject> cacheBytesQue = new ConcurrentQueue<CacheByteObject>();

        #endregion
        #region 初始化
        protected KCPManager()
        {
            mNetTag = GetType().Name;
            kcpObject = new KCP(1, HandleKcpSend);
            kcpObject.NoDelay(1, 10, 2, 1);
            kcpObject.WndSize(128, 128);

            for (int i = 0; i < 60; i++)
            {
                cacheBytesQue.Enqueue(new CacheByteObject() { bytes = new byte[cacheByteLen] });
            }
        }
        #endregion

        static public void NoDelay(int nodelay_, int interval_, int resend_, int nc_)
        {
            Instance.kcpObject.NoDelay(nodelay_, interval_, resend_, nc_);
        }

        static public void WndSize(int sndwnd, int rcvwnd)
        {
            Instance.kcpObject.WndSize(sndwnd, rcvwnd);
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
            StartReceiveAsync();
        }

        private void CreatSendAndRecThread()
        {
            CreatRec();
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
        override public bool Send(SendData _data)
        {
            if (mSocket == null) return false;
            if (_data == null)
            {
                DLog.LogError("试图添加一个空对象到发送队列!AddSend");
                return false;
            }
            DebugMsg(_data.Cmd, _data.Data, 0, _data.SendLen, "KCPSend");
            return Send(_data.Data, _data.SendLen);
        }

        override public bool Send(byte[] pBuffer, int pSize)
        {
            return kcpObject.Send(pBuffer, pSize) >= 0;
        }

        private void HandleKcpSend(byte[] buff, int size)
        {
            if (mSocket == null) return;
            try
            {
                SocketAsyncEventArgs sd = GetSocketAsyncEvent();
                sd.SetBuffer(buff, 0, size);
                sd.RemoteEndPoint = mTargetPoint;
                mSocket.SendToAsync(sd);
            }
            catch (System.Exception erro)
            {
                DLog.LogFormat("KCP Send Error.{0}", erro);
            }
        }

        override protected void SendAsyncCallback(object sender, SocketAsyncEventArgs e)
        {
            base.SendAsyncCallback(sender,e);
        }

        #endregion


        #region　接收

        #region thread rec

        private void StartReceiveAsync()
        {
            if (mSocket == null) return;
            receiveAsyncEvent.RemoteEndPoint = mRecPoint;
            receiveAsyncEvent.SetBuffer(mRecbuffer, 0, mReadMaxLen);
            if (!mSocket.ReceiveFromAsync(receiveAsyncEvent))
            {
                ReceiveAsyncCallback(mSocket, receiveAsyncEvent);
            }
        }

        override protected void ReceiveAsyncCallback(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                IPEndPoint tremot = receiveAsyncEvent.RemoteEndPoint as IPEndPoint;
                if (e.BytesTransferred > 0 && tremot.Address.Equals(mServerIP))
                {
                    Processingdata(e.BytesTransferred, mRecbuffer);
                }
                StartReceiveAsync();
            }
            else
            {
                DLog.Log(mNetTag + ":ReceiveMessage->" + e.SocketError);
                StartReceiveAsync();
            }
        }

        #endregion

        override protected void Processingdata(int _len, byte[] _buffer)
        {
            try
            {
                CacheByteObject dst = null;
                if (cacheBytesQue.TryDequeue(out dst))
                {
                    dst.Initialize();
                }
                else
                {
                    dst = new CacheByteObject() { bytes = new byte[cacheByteLen] };
                }
                dst.length = _len;
                Buffer.BlockCopy(_buffer, 0, dst.bytes, 0, _len);
                recvQueue.Enqueue(dst);
            }
            catch (Exception e)
            {
                DLog.LogError(mNetTag + "-" + e.ToString());
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
                ReceiveData tssdata = new ReceiveData(mBufferData.headInfo);
                tssdata.CopyBuffer(pBuffer, 0);

                Call(tssdata.Cmd, tssdata);

                DebugMsg(tssdata.Cmd, tssdata.Data, 0, tssdata.Len, "接收-ReceiveData");
            }
            catch (System.Exception error)
            {
                DLog.LogError(error);
            }
        }
        #endregion

        private void HandleRecvQueue()
        {
            CacheByteObject recvobject = null;
            while (recvQueue.TryDequeue(out recvobject))
            {
                int ret = kcpObject.Input(recvobject.bytes, recvobject.length);

                cacheBytesQue.Enqueue(recvobject);

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
        }

        private static readonly DateTime UTCTimeBegin = new DateTime(1970, 1, 1);
        public static UInt32 GetClockMS()
        {
            return (UInt32)(Convert.ToInt64(DateTime.UtcNow.Subtract(UTCTimeBegin).TotalMilliseconds) & 0xffffffff);
        }

        private bool needKcpUpdateFlag = false;
        private uint nextKcpUpdateTime = 0;

        override protected void MainThreadUpdate()
        {
            uint currentTimeMS = GetClockMS();
            HandleRecvQueue();
            if (needKcpUpdateFlag || currentTimeMS >= nextKcpUpdateTime)
            {
                kcpObject.Update(currentTimeMS);
                nextKcpUpdateTime = kcpObject.Check(currentTimeMS);
                needKcpUpdateFlag = false;
            }
        }

        #endregion

    }
}
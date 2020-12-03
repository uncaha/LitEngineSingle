using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Collections;
using System.Threading;
using LitEngine.UpdateSpace;
using LitEngine.Net.KCPCommand;
namespace LitEngine.Net
{
    public class KCPNet : NetBase<KCPNet>
    {
        #region socket属性
        static public bool IsPushPackage = false;
        protected IPEndPoint mTargetPoint;//目标地址
        protected EndPoint mRecPoint;
        protected IPAddress mServerIP;
        protected int mLocalPort = 30379;
        protected AsyncCallback sendCallBack;
        private KCP kcpObject;
        private SwitchQueue<byte[]> recvQueue = new SwitchQueue<byte[]>(128);
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
                AddMainThreadMsgReCall(GetMsgReCallData(MSG_RECALL.Connected, mNetTag + "建立连接完成."));
            }
            catch (Exception ex)
            {
                DLog.LogError(ex);
                mState = TcpState.Closed;
                AddMainThreadMsgReCall(GetMsgReCallData(MSG_RECALL.ConectError, mNetTag + "建立连接失败. " + ex.Message));
            }

        }

        protected void CreatRec()
        {
            mRecThread = new Thread(ReceiveMessage);
            mRecThread.IsBackground = true;
            mRecThread.Start();
        }

        virtual protected void CreatSendAndRecThread()
        {
            CreatRec();
            DLog.Log(mNetTag + "建立连接完成");
        }

        #endregion


        #region 属性设置方法
        virtual public void ChangetTargetPoint(IPEndPoint _tar)
        {
            mTargetPoint = _tar;
        }
        #endregion

        #region 收发
        #region 发送  
        override public void AddSend(SendData _data)
        {
            if (mSocket == null) return;
            if (_data == null)
            {
                DLog.LogError("试图添加一个空对象到发送队列!AddSend");
                return;
            }
            AddSend(_data.Data, _data.SendLen);
        }

        public bool AddSend(byte[] buff, int size)
        {
            return kcpObject.Send(buff, size) >= 0;
        }

        private void HandleKcpSend(byte[] buff, int size)
        {
            if (mSocket == null) return;
            try
            {
                var ar = mSocket.BeginSendTo(buff, 0, size, SocketFlags.None, mTargetPoint, sendCallBack, buff);
            }
            catch (System.Exception erro)
            {
                DLog.LogFormat("KCP Send Error.{0}", erro);
            }
        }

        #region thread send
        void SendAsyncCallback(IAsyncResult result)
        {
            mSocket.EndSendTo(result);
            byte[] tbuff = result.AsyncState as byte[];
            if (result.IsCompleted)
            {
            }
            if (tbuff != null)
            {
                DebugMsg(-1, tbuff, 0, tbuff.Length, "KCPSend", result.IsCompleted);
            }
        }

        #endregion
        #endregion


        #region　接收

        #region thread rec
        virtual protected void ReceiveMessage()
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
                DebugMsg(-1, _buffer, 0, _len, "接收-bytes");
                byte[] dst = new byte[_len];
                Buffer.BlockCopy(_buffer, 0, dst, 0, _len);
                recvQueue.Push(dst);
                //recvQueue
                // if (!IsPushPackage)
                // {
                //     ReceiveData tssdata = new ReceiveData(_buffer, 0);
                //     mResultDataList.Enqueue(tssdata);
                //     DebugMsg(tssdata.Cmd, tssdata.Data, 0, tssdata.Len, "接收-ReceiveData");
                // }
                // else
                // {
                //     mBufferData.Push(_buffer, _len);
                //     while (mBufferData.IsFullData())
                //     {
                //         ReceiveData tssdata = mBufferData.GetReceiveData();
                //         mResultDataList.Enqueue(tssdata);
                //         DebugMsg(tssdata.Cmd, tssdata.Data, 0, tssdata.Len, "接收-ReceiveData");
                //     }
                // }
            }
            catch (Exception e)
            {
                DLog.LogError(mNetTag + "-" + e.ToString());
            }

        }
        #endregion

        override protected void MainThreadUpdate()
        {
            UpdateReCalledMsg();
            UpdateRecMsg();
        }

        #endregion

    }
}

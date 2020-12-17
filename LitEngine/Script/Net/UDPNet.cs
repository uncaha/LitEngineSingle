using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Collections;
using System.Threading;
using LitEngine.UpdateSpace;
namespace LitEngine.Net
{
    public sealed class UDPNet : NetBase<UDPNet>
    {
        #region socket属性
        private IPEndPoint mTargetPoint;//目标地址
        private EndPoint mRecPoint;
        private IPAddress mServerIP;
        private int mLocalPort = 30379;
        private AsyncCallback sendCallBack;
        #endregion

        #region 构造析构

        private UDPNet() : base()
        {
            mNetTag = "UDP";
            sendCallBack = SendAsyncCallback;
        }
        #endregion

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

        private void CreatRec()
        {
            mRecThread = new Thread(ReceiveMessage);
            mRecThread.IsBackground = true;
            mRecThread.Start();
        }

        private void CreatSendAndRecThread()
        {
            CreatRec();
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
        override public bool Send(SendData _data)
        {
            if (_data == null)
            {
                DLog.LogError("试图添加一个空对象到发送队列!AddSend");
                return false;
            }
            DebugMsg(_data.Cmd, _data.Data, 0, _data.SendLen, "UDPSend");
            return Send(_data.Data, _data.SendLen);
        }
        override public bool Send(byte[] pBuffer, int pSize)
        {
            try
            {
                var ar = mSocket.BeginSendTo(pBuffer, 0, pSize, SocketFlags.None, mTargetPoint, sendCallBack, pBuffer);
                return true;
            }
            catch (System.Exception erro)
            {
                DLog.LogFormat("UDP Send Error.{0}", erro);
            }
            return false;
        }
        #region thread send
        void SendAsyncCallback(IAsyncResult result)
        {
            int tlen = mSocket.EndSendTo(result);
            byte[] tadata = result.AsyncState as byte[];
            if (result.IsCompleted)
            {
            }
            if (tadata != null)
            {
                DebugMsg(-1, tadata, 0, tlen, "UdpSend", result.IsCompleted);
            }
        }

        #endregion
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
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Collections;
using System.Threading;
using LitEngine.UpdateSpace;
namespace LitEngine.Net
{
    public class UDPNet : NetBase<UDPNet>
    {
        #region socket属性
        static public bool IsPushPackage = false;
        protected IPEndPoint mTargetPoint;//目标地址
        protected EndPoint mRecPoint;
        protected string mServerIP;
        protected int mLocalPort = 10186;
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
            try
            {
                var ips = GetServerIpAddress(mHostName);
                mServerIP = ips[0].ToString();
                mSocket = new Socket(IPAddress.Parse(mServerIP).AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                mTargetPoint = new IPEndPoint(IPAddress.Parse(mServerIP), mPort);

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

        protected void CreatSend()
        {
            mSendThread = new Thread(SendMessageThread);
            mSendThread.IsBackground = true;
            mSendThread.Start();
        }

        protected void CreatRec()
        {
            mRecThread = new Thread(ReceiveMessage);
            mRecThread.IsBackground = true;
            mRecThread.Start();
        }

        virtual protected void CreatSendAndRecThread()
        {
            //CreatSend();
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
            if (_data == null)
            {
                DLog.LogError("试图添加一个空对象到发送队列!AddSend");
                return;
            }
            var ar = mSocket.BeginSendTo(_data.Data, 0, _data.SendLen, SocketFlags.None, mTargetPoint,SendAsyncCallback, _data);

            //if (!mStartThread) return;
            //mSendDataList.Enqueue(_data);
        }

        #region thread send
        void SendAsyncCallback(IAsyncResult result)
        {
            if(result.IsCompleted)
            {
                SendData tadata = result.AsyncState as SendData;
                DebugMsg(tadata.Cmd, tadata.Data, 0, tadata.SendLen, "UdpSend");
            }
            else
            {

            }
        }
        virtual protected void SendMessageThread()
        {
            while (mStartThread)
            {
                try
                {
                    if (mSendDataList.Count == 0)
                        continue;
                    SendThread((SendData)mSendDataList.Dequeue());
                }
                catch (Exception e)
                {
                    DLog.LogError(mNetTag + e.ToString());
                }
            }
        }

        virtual protected void SendThread(SendData _data)
        {
            if (_data == null) return;
            int tsend = mSocket.SendTo(_data.Data, _data.SendLen, SocketFlags.None, mTargetPoint);
            DebugMsg(_data.Cmd, _data.Data, 0, _data.SendLen, "UdpSend");

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
                    DLog.LogError("UDP接收线程异常:" + e + "|" + "|" + mServerIP);
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
                if (!IsPushPackage)
                {
                    ReceiveData tssdata = new ReceiveData(_buffer, 0);
                    mResultDataList.Enqueue(tssdata);
                }
                else
                {
                    mBufferData.Push(_buffer, _len);
                    while (mBufferData.IsFullData())
                    {
                        ReceiveData tssdata = mBufferData.GetReceiveData();
                        mResultDataList.Enqueue(tssdata);
                        DebugMsg(tssdata.Cmd, tssdata.Data, 0, tssdata.Len, "接收-ReceiveData");
                    }
                }
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
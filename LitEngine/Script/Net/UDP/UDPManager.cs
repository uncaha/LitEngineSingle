using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Threading;

namespace LitEngine.Net
{

    public class UDPManager<T> : NetBase<T> where T : UDPManager<T>
    {
        #region socket属性
        private IPEndPoint mTargetPoint;//目标地址
        private EndPoint mRecPoint;
        private IPAddress mServerIP;
        private int mLocalPort = 30379;
        #endregion

        #region 构造析构

        protected UDPManager()
        {
            mNetTag = GetType().Name;
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
            //mRecThread = new Thread(ReceiveMessage);
            //mRecThread.IsBackground = true;
            //mRecThread.Priority = System.Threading.ThreadPriority.Lowest;
            //mRecThread.Start();
            StartReceiveAsync();
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

        override public bool Send(SendData pData)
        {
            if (mSocket == null || pData == null) return false;

            bool rv = Send(pData.Data, pData.SendLen);
            DebugMsg(pData.Cmd, pData.Data, 0, pData.SendLen, "UDPSend");
            return rv;
        }

        override public bool Send(byte[] pBuffer, int pSize)
        {
            if (mSocket == null || pBuffer == null) return false;
            SocketAsyncEventArgs sd = GetSocketAsyncEvent();
            sd.SetBuffer(pBuffer, 0, pSize);
            sd.RemoteEndPoint = mTargetPoint;
            return mSocket.SendToAsync(sd);
        }

        override protected void SendAsyncCallback(object sender, SocketAsyncEventArgs e)
        {
            base.SendAsyncCallback(sender, e);
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

        override protected void PushRecData(byte[] pBuffer, int pSize)
        {
            try
            {
                ReceiveData tssdata = new ReceiveData();
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
﻿using UnityEngine;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Net;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Collections.Concurrent;
namespace LitEngine.Net
{
    #region 回调消息
    public enum MessageType
    {
        Created = 1,//建立socket
        Connected,//连接并建立发送接收逻辑完成
        ConectError,//连接出现错误
        ReceiveError,//接收出现错误
        SendError,//发送出现错误
        DisConnected,//断开连接完成
        Destoryed,//删除对象
    }
    #endregion

    #region 回调对象
    public class NetMessage
    {
        public MessageType mCmd;
        public string mMsg;
        public object data;
        public NetMessage(MessageType _cmd, string _msg, object pdata = null)
        {
            mCmd = _cmd;
            mMsg = _msg;
            data = pdata;
        }
    }

    #endregion

    #region Tcp状态
    public enum TcpState
    {
        None = 0,
        Connected,
        Connecting,
        Closed,
        Closing,
        Disposed,
    }
    #endregion
    #region Net基类

    public abstract class NetBase<T> : MonoBehaviour where T : NetBase<T>
    {
        #region socket属性
        public enum IPTYPE
        {
            IPVNONE = 0,
            IPV4ONLY,
            IPV6ONLY,
            IPVALL,
        }

        protected Socket mSocket = null;
        protected string mHostName;//服务器地址
        protected int mPort;
        protected int mRecTimeOut = 0;
        protected int mSendTimeout = 0;
        protected int mReceiveBufferSize = 1024 * 8;
        protected int mSendBufferSize = 1024 * 8;
        protected bool socketNoDelay = true;

        protected string mNetTag = "";
        public bool StopUpdateRecMsg { get; set; }

        private ConcurrentQueue<SocketAsyncEventArgs> cacheAsyncEvent = new ConcurrentQueue<SocketAsyncEventArgs>();
        protected SocketAsyncEventArgs receiveAsyncEvent = null;
        #endregion

        #region 数据
        protected const int mReadMaxLen = 2048 * 20;
        protected byte[] mRecbuffer = new byte[mReadMaxLen];

        protected BufferBase mBufferData = new BufferBase(1024 * 400);
        protected ConcurrentQueue<ReceiveData> mResultDataList = new ConcurrentQueue<ReceiveData>();
        protected ConcurrentQueue<SendData> mSendDataList = new ConcurrentQueue<SendData>();
        #endregion
        #region 分发
        protected SafeMap<int, SafeList<System.Action<object>>> mMsgHandlerList = new SafeMap<int, SafeList<System.Action<object>>>();//消息注册列表
        protected ConcurrentQueue<NetMessage> mToMainThreadMsgList = new ConcurrentQueue<NetMessage>();//给主线程发送通知
        #endregion

        #region 输出到外部
        public delegate void OutputEvent(byte[] pBuffer,int pSize);
        protected OutputEvent receiveOutput = null;
        #endregion

        #region 日志
        protected bool IsShowDebugLog = false;
        #endregion

        #region 回调
        protected System.Action<NetMessage> MessageDelgate = null;

        static public System.Action<NetMessage> NetMessageDelage
        {
            set { Instance.MessageDelgate = value; }
        }
        #endregion

        #region 控制
        virtual public bool isConnected { get { return mState == TcpState.Connected && mSocket != null; } }
        protected TcpState mState = TcpState.None;
        protected bool mStartThread = false; //线程开关
        protected bool mDisposed = false;
        #endregion

        #region static
        static protected T sInstance = null;
        static protected T Instance
        {
            get
            {
                if (sInstance == null)
                {
                    GameObject tobj = new GameObject();
                    DontDestroyOnLoad(tobj);
                    sInstance = tobj.AddComponent<T>();
                    sInstance.InitNet();
                    tobj.name = sInstance.mNetTag + "-Object";
                }
                return sInstance;
            }
        }
        static public void DisposeNet()
        {
            if (Instance == null) return;
            Instance.Dispose();
        }

        static public void DisConnect()
        {
            if (Instance == null) return;
            Instance._DisConnect();
        }

        static public void ClearNetBuffer()
        {
            if (Instance == null) return;
            Instance.ClearBuffer();
        }

        static public void ClearMsgHandler()
        {
            Instance.mMsgHandlerList.Clear();
        }
        static public void Init(string _hostname, int _port, System.Action<NetMessage> pMsgDelgate = null)
        {
            Instance.InitSocket(_hostname, _port);
            Instance.MessageDelgate = pMsgDelgate;
        }

        static public void Connect()
        {
            Instance.ConnectToServer();
        }

        static public bool IsConnect()
        {
            return Instance.isConnected;
        }

        static public void SetSocketTime(int _rec, int _send, int _recsize, int _sendsize, bool pNoDelay)
        {
            Instance.SetTimerOutAndBuffSize(_rec, _send, _recsize, _sendsize, pNoDelay);
        }

        static public void SetOutputDelgate(OutputEvent pEvent)
        {
            Instance.receiveOutput = pEvent;
        }

        static public void SetHeadInfo(DataHead pInfo)
        {
            if (pInfo == null) return;
            BufferBase.headInfo = pInfo;
        }

        static public void ShowMsgLog(bool pShow)
        {
            Instance.IsShowDebugLog = pShow;
        }

        static public bool SendObject(SendData pData)
        {
            if (!Instance.isConnected) return false;
            return Instance.Send(pData);
        }

        static public bool SendBytes(byte[] pBuffer,int pSize)
        {
            if (!Instance.isConnected) return false;
            return Instance.Send(pBuffer, pSize);
        }
        static public void Reg(int msgid, System.Action<object> func)
        {
            Instance._Reg(msgid, func);
        }

        static public void UnReg(int msgid, System.Action<object> func)
        {
            Instance._UnReg(msgid, func);
        }
        #endregion

        protected NetBase()
        {
            StopUpdateRecMsg = false;

            for (int i = 0; i < 60; i++)
            {
                SocketAsyncEventArgs sd = new SocketAsyncEventArgs();
                sd.Completed += SendAsyncCallback;
                sd.SocketFlags = SocketFlags.None;
                cacheAsyncEvent.Enqueue(sd);
            }

            receiveAsyncEvent = new SocketAsyncEventArgs();
            receiveAsyncEvent.Completed += ReceiveAsyncCallback;
            receiveAsyncEvent.SocketFlags = SocketFlags.None;
        }

        virtual protected void InitNet()
        {

        }

        virtual protected void OnDestroy()
        {
            sInstance = null;
            Dispose(true);
            if (MessageDelgate != null)
                MessageDelgate(GetMsgReCallData(MessageType.Destoryed, mNetTag + "- 删除Net对象完成."));
        }

        virtual public void Dispose()
        {
            DestroyImmediate(this.gameObject);
        }

        protected virtual void Dispose(bool _disposing)
        {
            if (mDisposed)
                return;
            mDisposed = true;
            if (IsCOrD())
                return;
            MessageDelgate = null;
            mMsgHandlerList.Clear();
            _DisConnect();
            mState = TcpState.Disposed;
        }

        virtual public void InitSocket(string _hostname, int _port)
        {
            mHostName = _hostname;
            mPort = _port;
            gameObject.name = mNetTag + "-Server:" + mHostName;
        }

        protected List<IPAddress> GetServerIpAddress(string _hostname)
        {
            List<IPAddress> ret = new List<IPAddress>();
            try
            {
                IPAddress[] tips = Dns.GetHostAddresses(mHostName);
                DLog.Log("HostName: " + mHostName + " Length:" + tips.Length);
                for (int i = 0; i < tips.Length; i++)
                {
                    // DLog.Log( "IpAddress: " + tips[i].ToString() + " AddressFamily:" + tips[i].AddressFamily.ToString());

                    if (tips[i].AddressFamily == AddressFamily.InterNetwork)
                        ret.Insert(0, tips[i]);
                    else
                        ret.Add(tips[i]);
                }
            }
            catch (Exception e)
            {
                DLog.LogError(string.Format("[Get IPAddress error]" + " HostName:{0} IP:{1} ErrorMessage:{2}", mHostName, ret.Count, e.ToString()));
            }
            return ret;
        }
        #region 建立Socket
        virtual public void ConnectToServer()
        {

        }
        virtual public void SetTimerOutAndBuffSize(int _rec, int _send, int _recsize, int _sendsize, bool pNoDelay = true)
        {
            mRecTimeOut = _rec;
            mSendTimeout = _send;
            mReceiveBufferSize = _recsize;
            mSendBufferSize = _sendsize;
            socketNoDelay = pNoDelay;
            RestSocketInfo();
        }

        virtual protected void RestSocketInfo()
        {
            if (mSocket == null) return;
            mSocket.NoDelay = socketNoDelay;
            mSocket.ReceiveTimeout = mRecTimeOut;
            mSocket.SendTimeout = mSendTimeout;
            mSocket.ReceiveBufferSize = mReceiveBufferSize;
            mSocket.SendBufferSize = mSendBufferSize;
        }

        #endregion

        #region 断开管理
        virtual protected bool IsCOrD()
        {
            if (mState == TcpState.Disposed)
            {
                DLog.LogError(mNetTag + string.Format("[{0}]Disposed状态下对象已经被释放,请重新建立对象.", mNetTag));
                return true;
            }
            if (mState != TcpState.Closing && mState != TcpState.Connecting) return false;
            return true;
        }

        virtual public bool IsCanConnect()
        {
            if (IsCOrD() || isConnected) return false;
            return true;
        }


        virtual protected void CloseSRThread()
        {
            mStartThread = false;
            mState = TcpState.Closed;
            KillSocket();
        }
        virtual protected void ClearQueue()
        {
            mBufferData.Clear();

            mSendDataList = new ConcurrentQueue<SendData>();

            mResultDataList = new ConcurrentQueue<ReceiveData>();
        }
        virtual protected void WaitThreadJoin(Thread _thread)
        {
            if (_thread == null) return;
            _thread.Join();
        }

        void KillSocket()
        {
            try
            {
                if (mSocket != null)
                {
                    if (mSocket.ProtocolType == ProtocolType.Tcp && mSocket.Connected)
                    {
                        mSocket.Shutdown(SocketShutdown.Both);
                    }
                    mSocket.Close();
                }
            }
            catch (Exception err)
            {
                // DLog.LogError(mNetTag + "socket的关闭时出现异常:" + err);
            }
            
            mSocket = null;
        }
        virtual protected void CloseSocket()
        {
            mStartThread = false;
            ClearBuffer();
            KillSocket();
        }
        virtual protected void CloseSocketStart()
        {
            //需要注意重复调用
            mState = TcpState.Closing;
            try
            {
                CloseSocket();
                DLog.Log(mNetTag + ":socket is closed!");
            }
            catch (Exception err)
            {
                DLog.LogError(mNetTag + ":Disconnect - " + err);
            }
            mState = TcpState.Closed;

            if (!mDisposed)
            {
                AddMainThreadMsgReCall(GetMsgReCallData(MessageType.DisConnected, mNetTag + "- 断开连接完成."));
            }

        }

        virtual public void ClearBuffer()
        {
            ClearQueue();
        }
        virtual public void _DisConnect()
        {
            if (IsCOrD())
                return;
            CloseSocketStart();
        }

        #endregion

        #region 通知类

        virtual protected NetMessage GetMsgReCallData(MessageType _cmd, string _msg = "")
        {
            return new NetMessage(_cmd, _msg);
        }

        virtual protected void AddMainThreadMsgReCall(NetMessage _recall)
        {
            if (MessageDelgate == null) return;
            mToMainThreadMsgList.Enqueue(_recall);

        }

        #endregion

        #region 消息注册与分发

        virtual public void _Reg(int msgid, System.Action<object> func)
        {
            SafeList<System.Action<object>> tlist = null;
            if (mMsgHandlerList.ContainsKey(msgid))
            {
                tlist = mMsgHandlerList[msgid];
            }
            else
            {
                tlist = new SafeList<System.Action<object>>();
                mMsgHandlerList.Add(msgid, tlist);
            }
            if (!tlist.Contains(func))
                tlist.Add(func);
        }
        virtual public void _UnReg(int msgid, System.Action<object> func)
        {
            if (!mMsgHandlerList.ContainsKey(msgid)) return;
            SafeList<System.Action<object>> tlist = mMsgHandlerList[msgid];
            if (tlist.Contains(func))
                tlist.Remove(func);
            if (tlist.Count == 0)
                mMsgHandlerList.Remove(msgid);
        }

        virtual public void Call(int _msgid, object _msg)
        {
            try
            {
                if (mMsgHandlerList.ContainsKey(_msgid))
                {
                    SafeList<System.Action<object>> tlist = mMsgHandlerList[_msgid];
                    int tlen = tlist.Count;
                    for (int i = tlen - 1; i >= 0; i--)
                        tlist[i](_msg);
                }
            }
            catch (Exception _error)
            {
                DLog.LogError(_error.ToString());
            }
        }

        #endregion

        #region 主线程逻辑

        virtual protected void UpdateReCalledMsg()
        {
            try
            {
                if (!mToMainThreadMsgList.IsEmpty || MessageDelgate == null) return;
                NetMessage tmsg = null;
                if(mToMainThreadMsgList.TryDequeue(out tmsg))
                {
                    MessageDelgate(tmsg);
                }
               
            }
            catch (Exception _error)
            {
                DLog.LogError(_error.ToString());
            }
        }

        virtual public void UpdateRecMsg()
        {
            if (StopUpdateRecMsg) return;
            if (receiveOutput != null) return;

            if(!mResultDataList.IsEmpty)
            {
                ReceiveData trecdata = null;
                while (mResultDataList.TryDequeue(out trecdata))
                {
                    try
                    {
                        Call(trecdata.Cmd, trecdata);
                    }
                    catch (Exception _error)
                    {
                        DLog.LogError(_error.ToString());
                    }

                }
            }

            

        }
        #endregion

        #region 处理接收到的数据

        virtual protected void Processingdata(int _len, byte[] _buffer)
        {
            DebugMsg(-1, _buffer, 0, _len, "接收-bytes");
            if (receiveOutput == null)
            {
                PushRecData(_buffer, _len);
            }
            else
            {
                OutputToDelgate(_buffer, _len);
            }
        }
        virtual protected void PushRecData(byte[] pBuffer,int pSize)
        {

        }
        virtual protected void OutputToDelgate(byte[] pBuffer, int pSize)
        {
            try
            {
                receiveOutput(pBuffer, pSize);
            }
            catch (Exception ex)
            {
                DLog.LogError(ex);
            }  
        }
        #endregion

        #region 发送和接收
        virtual protected void DebugMsg(int _cmd, byte[] _buffer, int offset, int _len, string _title, bool pIsComplete = true)
        {
            if (IsShowDebugLog)
            {
                System.Text.StringBuilder bufferstr = new System.Text.StringBuilder();
                bufferstr.Append("{");
                for (int i = offset; i < _len; i++)
                {
                    if (i != offset)
                        bufferstr.Append(",");
                    bufferstr.Append(_buffer[i]);
                }
                bufferstr.Append("}");
                string tmsg = string.Format("{0}-cmd:{1} title:{2}  长度:{3}  内容:{4}", mNetTag, _cmd, _title, _len, bufferstr);
                if (pIsComplete)
                {
                    DLog.Log(tmsg);
                }
                else
                {
                    DLog.LOGColor(DLogType.Log, tmsg, LogColor.YELLO);
                }
            }
        }
        #region 发送
        virtual public bool Send(SendData _data)
        {
            return false;
        }
        virtual public bool Send(byte[] pBuffer,int pSize)
        {
            return false;
        }

        virtual protected void SendAsyncCallback(object sender, SocketAsyncEventArgs e)
        {
            cacheAsyncEvent.Enqueue(e);
        }

        virtual protected void ReceiveAsyncCallback(object sender, SocketAsyncEventArgs e)
        {

        }

        protected SocketAsyncEventArgs GetSocketAsyncEvent()
        {
            SocketAsyncEventArgs ret = null;

            if(cacheAsyncEvent.TryDequeue(out ret))
            {
                return ret;
            }
            else
            {
                ret = new SocketAsyncEventArgs();
                ret.Completed += SendAsyncCallback;
                ret.SocketFlags = SocketFlags.None;
            }
            return ret;
        }
        #endregion

        #endregion

        private void Update()
        {
            UpdateReCalledMsg();
            if (isConnected)
            {
                MainThreadUpdate();
            }
        }

        virtual protected void MainThreadUpdate()
        {

        }

    }
    #endregion
}
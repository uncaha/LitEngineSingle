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

    public class ReciveMsgDataList : List<ReceiveMessageEvent>
    {
    }
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

        public NetMessage()
        {
        }

        public NetMessage(MessageType _cmd, string _msg, object pdata = null)
        {
            mCmd = _cmd;
            mMsg = _msg;
            data = pdata;
        }

        virtual public void CallEvent()
        {

        }
    }

    public class ConnectMessage : NetMessage
    {
        public System.Action<bool> OnDone;
        public bool result = false;

        public ConnectMessage()
        {
            mCmd = MessageType.Connected;
        }

        override public void CallEvent()
        {
            try
            {
                if (result)
                {
                    mCmd = MessageType.Connected;
                }
                else
                {
                    mCmd = MessageType.ConectError;
                }

                var tevent = OnDone;
                OnDone = null;
                tevent?.Invoke(result);
            }
            catch (Exception e)
            {
                DLog.LogError(e);
            }
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
   
    public delegate void ReceiveMessageEvent(ReceiveData pData);
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

        //protected Socket mSocket = null;
        protected string mHostName;//服务器地址
        protected int mPort;
        protected int mRecTimeOut = 0;
        protected int mSendTimeout = 0;
        protected int mReceiveBufferSize = 1024 * 8;
        protected int mSendBufferSize = 1024 * 8;
        protected bool socketNoDelay = true;

        protected string mNetTag = "";
        public bool StopUpdateRecMsg { get; set; }

        protected ConcurrentQueue<SocketAsyncEventArgs> cacheAsyncEvent = new ConcurrentQueue<SocketAsyncEventArgs>();
        protected SocketAsyncEventArgs receiveAsyncEvent = null;
        #endregion

        #region 数据
        protected const int mReadMaxLen = 1024 * 8;
        protected byte[] mRecbuffer = new byte[mReadMaxLen];
        
        protected BufferBase mBufferData = new BufferBase(1024 * 400);
        protected ConcurrentQueue<ReceiveData> mResultDataList = new ConcurrentQueue<ReceiveData>();
        protected ConcurrentQueue<SendData> mSendDataList = new ConcurrentQueue<SendData>();
        #endregion
        #region 分发
        protected Dictionary<int, ReciveMsgDataList> mMsgHandlerList = new Dictionary<int, ReciveMsgDataList>();//消息注册列表
        protected ConcurrentQueue<NetMessage> mToMainThreadMsgList = new ConcurrentQueue<NetMessage>();//给主线程发送通知
        #endregion

        #region 日志
        protected bool IsShowDebugLog = false;
        #endregion

        #region 回调

        public event ReceiveMessageEvent OnReciveMessage_;
        public static event ReceiveMessageEvent OnReciveMessage
        {
            add
            {
                Instance.OnReciveMessage_ += value;
            }

            remove
            {
                Instance.OnReciveMessage_ -= value;
            }
        }
        public event System.Action<NetMessage> OnError_;
        
        public static event System.Action<NetMessage> OnError
        {
            add
            {
                Instance.OnError_ += value;
            }

            remove
            {
                Instance.OnError_ -= value;
            }
        }
        #endregion

        #region 控制
        virtual public bool isConnected { get { return mState == TcpState.Connected; } }
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
                    tobj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                    sInstance = tobj.AddComponent<T>();
                    sInstance.Oninit();
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
        static public void Init(string _hostname, int _port)
        {
            Instance.InitSocket(_hostname, _port);
        }

        
        static public void Connect(System.Action<bool> pOnDone = null)
        {
            Instance.ConnectToServer(pOnDone);
        }

        static public bool IsConnect()
        {
            return Instance.isConnected;
        }

        static public void SetHost(string pHost, int pPort)
        {
            Instance.mHostName = pHost;
            Instance.mPort = pPort;
            Instance.gameObject.name = Instance.mNetTag + "-Server:" + pHost;
        }

        static public void SetSocketTime(int _rec, int _send, int _recsize, int _sendsize, bool pNoDelay)
        {
            Instance.SetTimerOutAndBuffSize(_rec, _send, _recsize, _sendsize, pNoDelay);
        }

        public static DataFormat Format
        {
            get { return Instance.mBufferData.headInfo; }

            set
            {
                if (value == null) return;
                Instance.mBufferData.headInfo = value;
            }
        }

        static public void ShowMsgLog(bool pShow)
        {
            Instance.IsShowDebugLog = pShow;
        }

        static public SendData CreatSendData(int cmd)
        {
            return new SendData(Instance.mBufferData.headInfo, cmd);
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
        static public void Reg(int msgid, ReceiveMessageEvent func)
        {
            Instance._Reg(msgid, func);
        }

        static public void UnReg(int msgid, ReceiveMessageEvent func)
        {
            Instance._UnReg(msgid, func);
        }
        #endregion

        protected NetBase()
        {
            StopUpdateRecMsg = false;
            mNetTag = typeof(T).Name;
        }

        virtual protected void Oninit()
        {

        }

        virtual protected void OnDestroy()
        {
            sInstance = null;
            Dispose(true);

            mMsgHandlerList.Clear();
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
        virtual public void ConnectToServer(System.Action<bool> pOnDone)
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

        virtual protected void KillSocket()
        {

        }
        virtual protected void CloseSocket()
        {
            mStartThread = false;
            mState = TcpState.Closed;
            KillSocket();
            ClearBuffer();
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
        }

        virtual public void ClearBuffer()
        {
            ClearQueue();
        }
        virtual public void _DisConnect()
        {
            if (IsCOrD())
                return;
            if (mState == TcpState.Closed
                || mState == TcpState.Disposed)
            {
                return;
            }
            CloseSocketStart();
        }

        #endregion

        #region 通知类

        protected void OnNetError(MessageType pType,string pMsg)
        {
            if (!mStartThread) return;
            DLog.Log($"{pType}-> {pMsg}");
            CloseSRThread();
            AddMainThreadMsgReCall(new NetMessage(pType, pMsg));
        }

        virtual protected void AddMainThreadMsgReCall(NetMessage _recall)
        {
            mToMainThreadMsgList.Enqueue(_recall);
        }

        #endregion

        #region 消息注册与分发

        virtual public void _Reg(int msgid, ReceiveMessageEvent func)
        {
            ReciveMsgDataList tlist = null;
            if (mMsgHandlerList.ContainsKey(msgid))
            {
                tlist = mMsgHandlerList[msgid];
            }
            else
            {
                tlist = new ReciveMsgDataList();
                mMsgHandlerList.Add(msgid, tlist);
            }
            if (!tlist.Contains(func))
                tlist.Add(func);
        }
        virtual public void _UnReg(int msgid, ReceiveMessageEvent func)
        {
            if (!mMsgHandlerList.ContainsKey(msgid)) return;
            ReciveMsgDataList tlist = mMsgHandlerList[msgid];
            if (tlist.Contains(func))
                tlist.Remove(func);
            if (tlist.Count == 0)
                mMsgHandlerList.Remove(msgid);
        }

        virtual public void Call(int _msgid, ReceiveData _msg)
        {
            try
            {
                if (mMsgHandlerList.ContainsKey(_msgid))
                {
                    ReciveMsgDataList tlist = mMsgHandlerList[_msgid];
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
                if (mToMainThreadMsgList.IsEmpty) return;
                //OnConnectDone
                NetMessage tmsg = null;
                if(mToMainThreadMsgList.TryDequeue(out tmsg))
                {
                    tmsg.CallEvent();
                    switch (tmsg.mCmd)
                    {
                        case MessageType.ReceiveError:
                        case MessageType.SendError:
                        {
                            OnError_?.Invoke(tmsg);
                        }
                            break;
                        default:
                            break;
                    }
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

            if(!mResultDataList.IsEmpty)
            {
                ReceiveData trecdata = null;
                while (mResultDataList.TryDequeue(out trecdata))
                {
                    try
                    {
                        Call(trecdata.Cmd, trecdata);
                        OnReciveMessage_?.Invoke(trecdata);
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
            PushRecData(_buffer, _len);
        }
        virtual protected void PushRecData(byte[] pBuffer,int pSize)
        {

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
                    DLog.Log(tmsg);
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

            try
            {
                if (isConnected)
                {
                    MainThreadUpdate();
                }
            }
            catch (Exception ex)
            {
                DLog.LogError($"{mNetTag} -> {ex}");
            }

        }

        virtual protected void MainThreadUpdate()
        {

        }

    }
    #endregion
}
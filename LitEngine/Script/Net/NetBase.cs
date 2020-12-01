﻿using UnityEngine;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Net;
using System.Net.NetworkInformation;
using System.Collections.Generic;
namespace LitEngine
{
    namespace NetTool
    {

        #region 回调消息
        public enum MSG_RECALL
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
        public class MSG_RECALL_DATA
        {
            public MSG_RECALL mCmd;
            public string mMsg;
            public object data;
            public MSG_RECALL_DATA(MSG_RECALL _cmd, string _msg,object pdata = null)
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
        public abstract class NetBase : MonoBehaviour
        {
            #region socket属性
            public enum IPTYPE
            {
                IPVNONE = 0,
                IPV4ONLY,
                IPV6ONLY,
                IPVALL,
            }

            protected Thread mSendThread;
            protected Thread mRecThread;
           // protected ManualResetEvent mWaitObject = new ManualResetEvent(false);

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
            #endregion

            #region 数据
            protected const int mReadMaxLen = 1024 * 20;
            protected byte[] mRecbuffer = new byte[mReadMaxLen];

            protected BufferBase mBufferData = new BufferBase(1024 * 400);
            protected System.Collections.Queue mSendDataList = System.Collections.Queue.Synchronized(new System.Collections.Queue());//发送数据队列
            protected System.Collections.Queue mResultDataList = System.Collections.Queue.Synchronized(new System.Collections.Queue());//已接收的消息队列             
            #endregion
            #region 分发
            static public int OneFixedUpdateChoseCount = 60;
            protected SafeMap<int, SafeList<System.Action<ReceiveData>>> mMsgHandlerList = new SafeMap<int, SafeList<System.Action<ReceiveData>>>();//消息注册列表
            protected SafeQueue<MSG_RECALL_DATA> mToMainThreadMsgList = new SafeQueue<MSG_RECALL_DATA>();//给主线程发送通知
            #endregion
            #region 日志
            public bool IsShowDebugLog = false;
            #endregion

            #region 回调
            public event System.Action<MSG_RECALL_DATA> MessageDelgate = null;
            #endregion

            #region 控制
            public bool isConnected { get { return mState == TcpState.Connected && mSocket != null && mSocket.Connected; } }
            protected TcpState mState = TcpState.None;
            protected bool mStartThread = false; //线程开关
            protected bool mDisposed = false;
            #endregion

            public NetBase()
            {
                StopUpdateRecMsg = false;
            }

            virtual protected void OnDestroy()
            {
                Dispose(true);
                if (MessageDelgate != null)
                    MessageDelgate(GetMsgReCallData(MSG_RECALL.Destoryed, mNetTag + "- 删除Net对象完成."));
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
                try {
                    IPAddress[] tips = Dns.GetHostAddresses(mHostName);
                    DLog.Log( "HostName: " + mHostName + " Length:" + tips.Length);
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
                    DLog.LogError(string.Format("[获取IPAddress失败]" + " HostName:{0} IP:{1} ErrorMessage:{2}", mHostName, ret.Count, e.ToString()));
                }
                return ret;
            }
            #region 建立Socket
            virtual public void ConnectToServer()
            {

            }
            virtual public void SetTimerOutAndBuffSize(int _rec, int _send, int _recsize, int _sendsize,bool pNoDelay = true)
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
                if(mState == TcpState.Disposed)
                {
                    DLog.LogError( mNetTag + string.Format("[{0}]Disposed状态下对象已经被释放,请重新建立对象.", mNetTag));
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
                mSendDataList.Clear();
                mBufferData.Clear();
                mResultDataList.Clear();

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
                        mSocket = null;
                    }
                }
                catch (Exception err)
                {
                   // DLog.LogError(mNetTag + "socket的关闭时出现异常:" + err);
                }
            }
            virtual protected void CloseSocket()
            {
                mStartThread = false;
                ClearBuffer();
                KillSocket();
                WaitThreadJoin(mSendThread);
                WaitThreadJoin(mRecThread);
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
                    AddMainThreadMsgReCall(GetMsgReCallData(MSG_RECALL.DisConnected, mNetTag + "- 断开连接完成."));
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

            virtual protected MSG_RECALL_DATA GetMsgReCallData(MSG_RECALL _cmd, string _msg = "")
            {
                return new MSG_RECALL_DATA(_cmd, _msg);
            }

            virtual protected void AddMainThreadMsgReCall(MSG_RECALL_DATA _recall)
            {
                if (MessageDelgate == null) return;
                mToMainThreadMsgList.Enqueue(_recall);

            }

            #endregion

            #region 消息注册与分发

            virtual public void _Reg(int msgid, System.Action<ReceiveData> func)
            {
                SafeList<System.Action<ReceiveData>> tlist = null;
                if (mMsgHandlerList.ContainsKey(msgid))
                {
                    tlist = mMsgHandlerList[msgid];
                }
                else
                {
                    tlist = new SafeList<System.Action<ReceiveData>>();
                    mMsgHandlerList.Add(msgid, tlist);
                }
                if (!tlist.Contains(func))
                    tlist.Add(func);
            }
            virtual public void _UnReg(int msgid, System.Action<ReceiveData> func)
            {
                if (!mMsgHandlerList.ContainsKey(msgid)) return;
                SafeList<System.Action<ReceiveData>> tlist = mMsgHandlerList[msgid];
                if (tlist.Contains(func))
                    tlist.Remove(func);
                if (tlist.Count == 0)
                    mMsgHandlerList.Remove(msgid);
            }

            virtual public void Call(int _msgid, ReceiveData _msg)
            {
#if LITDEBUG
                try
                {
#endif
                    if (mMsgHandlerList.ContainsKey(_msgid))
                    {
                        SafeList<System.Action<ReceiveData>> tlist = mMsgHandlerList[_msgid];
                        int tlen = tlist.Count;
                        for (int i = tlen - 1; i >= 0; i--)
                            tlist[i](_msg);
                    }
#if LITDEBUG
                }
                catch (Exception _error)
                {
                    DLog.LogError(_error.ToString());
                }
#endif
            }

            #endregion

            #region 主线程逻辑

            virtual protected void UpdateReCalledMsg()
            {
                try {
                    if (mToMainThreadMsgList.Count == 0 || MessageDelgate == null) return;
                    MessageDelgate(mToMainThreadMsgList.Dequeue());
                }
                catch (Exception _error)
                {
                    DLog.LogError( _error.ToString());
                } 
            }

            virtual public void UpdateRecMsg()
            {
                if (StopUpdateRecMsg) return;

                int i = mResultDataList.Count > OneFixedUpdateChoseCount ? OneFixedUpdateChoseCount: mResultDataList.Count;

                while (i > 0)
                {
                    try
                    {
                        ReceiveData trecdata = (ReceiveData)mResultDataList.Dequeue();
                        Call(trecdata.Cmd, trecdata);
                        i--;
                    }
                    catch (Exception _error)
                    {
                        DLog.LogError( _error.ToString());
                    }
                    
                }

            }
            #endregion

            #region 处理接收到的数据
            virtual protected void Processingdata(int _len, byte[] _buffer)
            {
                DebugMsg(0, _buffer, 0, _len, "接收");
            }
            #endregion

            #region 发送和接收
            virtual protected void DebugMsg(int _cmd, byte[] _buffer, int offset, int _len, string _title)
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
                    DLog.Log(tmsg);
                }
            }
            #region 发送
            virtual public void AddSend(SendData _data)
            {

            }
            #endregion

            #endregion

        }
        #endregion
    }
}



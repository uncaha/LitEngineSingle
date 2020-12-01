using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Net;
namespace LitEngine
{
    namespace NetTool
    {

        public class TCPNet : NetBase
        {
            static private TCPNet sInstance = null;
            static private TCPNet Instance
            {
                get
                {
                    if (sInstance == null)
                    {
                        GameObject tobj = new GameObject();
                        DontDestroyOnLoad(tobj);
                        sInstance = tobj.AddComponent<TCPNet>();
                        tobj.name = sInstance.mNetTag + "-Object";
                    }
                    return sInstance;
                }
            }

            #region 构造析构
            private TCPNet():base()
            {
                mNetTag = "TCP";
            }

            #endregion

            override protected void OnDestroy()
            {
                sInstance = null;
                base.OnDestroy();
            }
            #region static
            static public void DisposeNet()
            {
                if(Instance == null) return;
                Instance.Dispose();
            }
            
            static public void DisConnect()
            {
                if(Instance == null) return;
                Instance._DisConnect();
            }

            static public void ClearNetBuffer()
            {
                if(Instance == null) return;
                Instance.ClearBuffer();
            }

            static public void ClearMsgHandler()
            {
                Instance.mMsgHandlerList.Clear();
            }
            static public void Init(string _hostname, int _port)
            {
                Instance.InitSocket(_hostname,_port);
            }

            static public void Connect()
            {
                Instance.ConnectToServer();
            }

            static public void SetSocketTime(int _rec, int _send, int _recsize, int _sendsize,bool pNoDelay)
            {
                Instance.SetTimerOutAndBuffSize(_rec,_send,_recsize,_sendsize,pNoDelay);
            }

            static public void Add(SendData _data)
            {
                Instance.AddSend(_data);
            }

            static public void Reg(int msgid, System.Action<ReceiveData> func)
            {
                Instance._Reg(msgid,func);
            }

            static public void UnReg(int msgid, System.Action<ReceiveData> func)
            {
                Instance._UnReg(msgid,func);
            }
            #endregion

            #region 建立Socket


            override public void ConnectToServer()
            {
                if (IsCOrD())
                {
                    DLog.LogError(mNetTag + string.Format("[{0}]Closing或Connecting状态下不可执行.", mNetTag));
                    return;
                }
                    
                if (isConnected)
                {
                    AddMainThreadMsgReCall(GetMsgReCallData(MSG_RECALL.ConectError, mNetTag + "重复建立连接"));
                    return;
                }
                mState = TcpState.Connecting;
                System.Threading.Tasks.Task.Run(ThreatConnect);
            }

            virtual protected bool TCPConnect()
            {
                bool ret = false;
                List<IPAddress> tipds = GetServerIpAddress(mHostName);
                if (tipds.Count == 0) DLog.LogError( "IPAddress List.Count = 0!");

                foreach (IPAddress tip in tipds)
                {
                    try
                    {
                        DLog.Log( string.Format("[开始连接]" + " HostName:{0} IpAddress:{1} AddressFamily:{2}", mHostName, tip.ToString(), tip.AddressFamily.ToString()));
                        mSocket = new Socket(tip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        RestSocketInfo();
                        mSocket.Connect(tip, mPort);
                        DLog.Log( "连接成功!");
                        ret = true;
                        break;
                    }
                    catch (Exception e)
                    {
                        DLog.LogError(string.Format("[网络连接异常]" + " HostName:{0} IpAddress:{1} AddressFamily:{2} ErrorMessage:{3}", mHostName, tip.ToString(), tip.AddressFamily.ToString(), e.ToString()));
                    }
                }

                return ret;
            }

            protected void ThreatConnect()
            {

                bool tok = TCPConnect();
                string tmsg= "";
                if (tok)
                {
                    try
                    {
                        mStartThread = true;

                        #region 接收;
                        CreatRec();
                        #endregion
                        #region 发送;
                        CreatSend();
                        #endregion

                        DLog.Log( "收发线程启动!");
                    }
                    catch (Exception e)
                    {
                        tmsg = e.ToString();
                        CloseSRThread();
                        tok = false;
                    }

                }
                
                if (!tok)
                {
                    mState = TcpState.Closed;
                    AddMainThreadMsgReCall(GetMsgReCallData(MSG_RECALL.ConectError, mNetTag + "建立连接失败. " + tmsg));
                }
                else
                {
                    mState = TcpState.Connected;
                    AddMainThreadMsgReCall(GetMsgReCallData(MSG_RECALL.Connected, mNetTag + "建立连接完成."));
                }
                    
            }

            protected void CreatSend()
            {
                mSendThread = new Thread(SendMessageThread);
                mSendThread.Start();
            }

            protected void CreatRec()
            {
                mRecThread = new Thread(ReceiveMessage);
                mRecThread.Start();
            }

            #endregion

            #region 发送

            override public void AddSend(SendData _data)
            {
                if(_data == null)
                {
                    DLog.LogError( "试图添加一个空对象到发送队列!AddSend");
                    return;
                }
                if (!mStartThread) return;
                mSendDataList.Enqueue(_data);

            }

            #region 线程发送模式
            protected void SendMessageThread()
            {
                try
                {
                    while (mStartThread)
                    {
                        if (mSendDataList.Count == 0)
                            continue;
                        SendThread((SendData)mSendDataList.Dequeue());
                    }
                    
                }
                catch (Exception e)
                {
                    if(mStartThread)
                    {
                        DLog.LogError(mNetTag + ":SendMessageThread->" + e.ToString());
                        CloseSRThread();
                        AddMainThreadMsgReCall(new MSG_RECALL_DATA(MSG_RECALL.SendError, mNetTag + "-" + e.ToString()));
                    }
                }
            }
            virtual protected void SendThread(SendData _data)
            {
                if (_data == null) return;
                int sendlen = mSocket.Send(_data.Data, _data.SendLen, SocketFlags.None);
                DebugMsg(_data.Cmd, _data.Data, 0, _data.SendLen, "Send-SendThread-"+ sendlen);
            }
            #endregion
            #endregion

            #region　接收

            protected void ReceiveMessage()
            {
                if (IsShowDebugLog)
                {
                    DLog.Log("Start ReceiveMessage");
                }
                try
                {
                    while (mStartThread)
                    {
                        
                        if (mSocket.Available != 0)
                        {
                            int receiveNumber = 0;
                            receiveNumber = mSocket.Receive(mRecbuffer, 0, mReadMaxLen, SocketFlags.None);
                            if (receiveNumber > 0)
                                Processingdata(receiveNumber, mRecbuffer);
                            
                        }
                    }

                }
                catch (Exception e)
                {
                    if(mStartThread)
                    {
                        DLog.LogError(mNetTag + ":ReceiveMessage->" + e.ToString());
                        CloseSRThread();
                        AddMainThreadMsgReCall(GetMsgReCallData(MSG_RECALL.ReceiveError, mNetTag + "-" + e.ToString()));
                    }  
                }
                if (IsShowDebugLog)
                {
                    DLog.Log("End ReceiveMessage");
                }
            }

            override protected void Processingdata(int _len, byte[] _buffer)
            {
                try {
                    DebugMsg(-1, _buffer, 0, _len, "接收-bytes");
                    mBufferData.Push(_buffer, _len);
                    while (mBufferData.IsFullData())
                    {
                        ReceiveData tssdata = mBufferData.GetReceiveData();
                        mResultDataList.Enqueue(tssdata);
                        //mBufferData.Pop();
                        DebugMsg(tssdata.Cmd, tssdata.Data, 0, tssdata.Len, "接收-ReceiveData");
                    }
                }
                catch (Exception e)
                {
                    DLog.LogError( mNetTag + ":Processingdata->" + e.ToString());
                }
                
            }

            #endregion

            protected void Update()
            {
                UpdateReCalledMsg();
                UpdateRecMsg();
            }
        }
    }
}
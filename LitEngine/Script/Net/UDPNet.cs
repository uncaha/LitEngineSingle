using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Collections;
using System.Threading;
namespace LitEngine
{
    namespace NetTool
    {
        public class UDPNet : NetBase
        {
            #region socket属性
            static public bool IsPushPackage = false;
            static private UDPNet sInstance = null;
            protected IPEndPoint mTargetPoint;//目标地址
            protected EndPoint mRecPoint;
            protected string mServerIP;

            protected SendData mNowSendData;
            protected bool mSendFinished = true;
            protected bool mSended = false;
            protected bool mStartRec = false;
            #endregion
            protected int mLocalPort = 10186;
            #region 构造析构

            static private UDPNet Instance
            {
                get
                {
                    if (sInstance == null)
                    {
                        GameObject tobj = new GameObject();
                        DontDestroyOnLoad(tobj);
                        sInstance = tobj.AddComponent<UDPNet>();
                        tobj.name = sInstance.mNetTag + "-Object";
                    }
                    return sInstance;
                }
            }

            private UDPNet():base()
            {
                mNetTag = "UDPAsync";
            }
            override protected void OnDestroy()
            {
                sInstance = null;
                base.OnDestroy();
            }
            #endregion

            #region 属性


            public IPEndPoint TargetPoint
            {
                get
                {
                    return mTargetPoint;
                }

                set
                {
                    mTargetPoint = value;
                }
            }
            #endregion

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
                // GameUpdateManager.Instance().RegUpdate(mUpdateDelgate);
                IPAddress[] ips = Dns.GetHostAddresses(mHostName);
                mServerIP = ips[0].ToString();
                mSocket = new Socket(IPAddress.Parse(mServerIP).AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                mTargetPoint = new IPEndPoint(IPAddress.Parse(mServerIP),mPort);
                mStartThread = true;

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

                CreatSendAndRecThread();

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

            virtual protected void CreatSendAndRecThread()
            {
                CreatSend();
                CreatRec();
               DLog.Log( mNetTag + "建立连接完成");
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
                if(_data == null)
                {
                    DLog.LogError( "试图添加一个空对象到发送队列!AddSend");
                    return;
                }
                if (!mStartThread) return;
                mSendDataList.Enqueue(_data);
            }

            #region thread send
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
                        DLog.LogError( "UDP接收线程异常:" + e + "|" + "|" + mServerIP);
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

            protected void Update()
            {
                UpdateReCalledMsg();
                UpdateRecMsg();
            }
            #endregion
        }
    }
}
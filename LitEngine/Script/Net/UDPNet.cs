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

            static public UDPNet Instance
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

            public UDPNet():base()
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
                if (!mStartThread) return;
                mSendDataList.Enqueue(_data);
            }

            #region thread send
            virtual protected void SendMessageThread()
            {
                while (mStartThread)
                {
                    Thread.Sleep(2);
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
                    base.Processingdata(_len, _buffer);
                    if (_len < SocketDataBase.mPackageTopLen) return;
                    ReceiveData tssdata = new ReceiveData(_buffer, 0);
                    Call(tssdata.Cmd, tssdata);
                }
                catch (Exception e)
                {
                    DLog.LogError(mNetTag + "-" + e.ToString());
                }

            }
            #endregion

            #endregion
        }
    }
}
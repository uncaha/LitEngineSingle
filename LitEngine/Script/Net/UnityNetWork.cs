//using UnityEngine;
//using System.Collections;
//namespace LitEngine
//{
//    namespace NetTool
//    {
//        public class UnityNetWork : MonoBehaviour
//        {
//            static private UnityNetWork sInstance = null;

//            protected string mNetTag = "";
//            protected GameObject mNetObject;
//            protected bool mConnecting = false;

//            protected string mServerIP;
//            protected int mPort;
//            protected System.Action<MSG_RECALL_DATA> mRecCall = null;

//            protected Queue mToMainThreadMsgList = Queue.Synchronized(new Queue());//给主线程发送通知
//            protected Hashtable mMsgHandlerList = Hashtable.Synchronized(new Hashtable());//消息注册列表
//            static public UnityNetWork Instance
//            {
//                get
//                {
//                    if (sInstance == null)
//                    {
//                        GameObject tobj = new GameObject();
//                        UnityEngine.Object.DontDestroyOnLoad(tobj);
//                        sInstance = tobj.AddComponent<UnityNetWork>();
//                        sInstance.mNetObject = tobj;
//                        tobj.name = sInstance.mNetTag + "-Object";
//                    }
//                    return sInstance;
//                }
//            }

//            public UnityNetWork()
//            {
//                mNetTag = "UnityNetWork";
//            }

//            void OnDestroy()
//            {
//                sInstance = null;
//            }

//            public void InitSocket(string _serverip, int _port, System.Action<MSG_RECALL_DATA> _ReCallDelegate = null)
//            {
//                mServerIP = _serverip;
//                mPort = _port;
//                mRecCall = _ReCallDelegate;
//            }

//            public void ConnectToServer()
//            {
//                if (Network.peerType == NetworkPeerType.Disconnected)
//                {
//                    NetworkConnectionError error = Network.Connect(mServerIP, mPort);
//                    //连接状态  
//                    switch (error)
//                    {
//                        case NetworkConnectionError.NoError:
//                            AddMainThreadMsgReCall(GetMsgReCallData(MSG_RECALL.Connected, mNetTag + "建立连接完成"));
//                            break;
//                        default:
//                            AddMainThreadMsgReCall(GetMsgReCallData(MSG_RECALL.ConectError, mNetTag + error.ToString()));
//                            break;
//                    }
//                }
//                else
//                {
//                   DLog.Log("network已连接!");
//                }

//            }

//            protected MSG_RECALL_DATA GetMsgReCallData(MSG_RECALL _cmd, string _msg = "")
//            {
//                return new MSG_RECALL_DATA(_cmd, _msg);
//            }

//            protected void AddMainThreadMsgReCall(MSG_RECALL_DATA _recall)
//            {
//                if (mRecCall != null)
//                    mToMainThreadMsgList.Enqueue(_recall);
//            }

//            virtual public void Reg(int msgid, System.Action<object> func)
//            {
//                System.Action<object> action = null;

//                if (mMsgHandlerList.Contains(msgid))
//                {
//                    action = (System.Action<object>)mMsgHandlerList[msgid];
//                    action += func;
//                    mMsgHandlerList[msgid] = action;
//                }
//                else
//                {
//                    mMsgHandlerList.Add(msgid, func);
//                }
//            }
//            virtual public void UnReg(int msgid, System.Action<object> func)
//            {
//                System.Action<object> action = null;

//                if (mMsgHandlerList.Contains(msgid))
//                {
//                    action = (System.Action<object>)mMsgHandlerList[msgid];
//                    action -= func;
//                    mMsgHandlerList[msgid] = action;
//                }
//            }

//            void Update()
//            {
//                UpdateReCalledMsg();
//            }

//            protected void UpdateReCalledMsg()
//            {
//                if (mRecCall == null || mToMainThreadMsgList.Count == 0) return;
//                mRecCall((MSG_RECALL_DATA)mToMainThreadMsgList.Dequeue());
//            }

//        }
//    }
//}

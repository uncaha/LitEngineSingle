using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System;
using System.Threading.Tasks;
using LitEngine.Net;
using LitEngine;
namespace LitEngine.Net.TestServer
{

    public class ServerTestTCP : MonoBehaviour
    {
        public class UserObject
        {
            private static int countIndex = 1;
            public int Key;
            public Socket socket;

            AsyncCallback recCallback;
            AsyncCallback sendCallback;

            byte[] recBuffer = new byte[1024 * 100];
            public UserObject(Socket psoc)
            {
                socket = psoc;
                Key = countIndex++;

                recCallback = RecAsyncCallback;
                sendCallback = SendAsyncCallback;
            }

            public void BeginReceive()
            {
                try
                {
                    SocketError errorCode = SocketError.Success;
                    var ar = socket.BeginReceive(recBuffer, 0, recBuffer.Length, SocketFlags.None, out errorCode, recCallback, null);
                }
                catch (System.Exception erro)
                {
                    DLog.LogFormat("TCP Rec Error.{0}", erro);
                }
            }

            public void BeginSend(byte[] buffer, int offset, int size)
            {
                try
                {
                    SocketError errorCode = SocketError.Success;
                    var ar = socket.BeginSend(buffer, offset, size, SocketFlags.None, out errorCode, sendCallback, buffer);
                    if (errorCode != SocketError.Success)
                    {
                        DLog.LogErrorFormat("TCP Send Error.{0}", errorCode);
                    }
                }
                catch (System.Exception erro)
                {
                    DLog.LogFormat("TCP Send Error.{0}", erro);
                }
            }

            void SendAsyncCallback(IAsyncResult result)
            {
                int tsendLen = socket.EndSend(result);
                if (result.IsCompleted == false)
                {

                }
            }

            void RecAsyncCallback(IAsyncResult result)
            {
                int tsendLen = socket.EndReceive(result);
                if (tsendLen > 0)
                {
                    SendData tetstdata = new SendData(11);
                    tetstdata.AddInt(3);
                    BeginSend(tetstdata.Data, 0, tetstdata.SendLen);

                    System.Text.StringBuilder bufferstr = new System.Text.StringBuilder();
                    bufferstr.Append("{");
                    for (int i = 0; i < tsendLen; i++)
                    {
                        if (i != 0)
                            bufferstr.Append(",");
                        bufferstr.Append(recBuffer[i]);
                    }
                    bufferstr.Append("}");
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    var localIP = endPoint.Address.ToString();
                    string tmsg = string.Format("{0}{1}", localIP, bufferstr);
                    Debug.Log(tmsg);
                }

                BeginReceive();
            }
        }
        // Start is called before the first frame update
        Socket testServer;
        Task recTask;
        bool taskStart = false;

        SafeMap<int, UserObject> userMap = new SafeMap<int, UserObject>();
        void Start()
        {
            gameObject.name = "TCP-" + 20240;
            //var mTargetPoint = new IPEndPoint(tip, 200236);
            IPAddress tip = IPAddress.Parse("127.0.0.1");
            testServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint tMypoint = new IPEndPoint(IPAddress.Any, 20240);
            testServer.Bind(tMypoint);
            testServer.Listen(100);

            taskStart = true;
            recTask = Task.Run(AccaptThread);

            Application.runInBackground = true;

            Debug.Log("测试TCP服务器启动");
        }

        private void OnDestroy()
        {
            taskStart = false;
            testServer.Close();
        }

        void AccaptThread()
        {
            while (taskStart)
            {
                Debug.Log("等待一个新玩家");
                Socket tuser = testServer.Accept();
                UserObject tobj = new UserObject(tuser);

                userMap.Add(tobj.Key, tobj);

                Debug.Log("新玩家." + tobj.Key);
                tobj.BeginReceive();
            }
        }


        // Update is called once per frame
        void Update()
        {

        }
    }

}

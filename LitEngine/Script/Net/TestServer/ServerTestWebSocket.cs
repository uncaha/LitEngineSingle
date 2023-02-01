
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using LitEngine.Net;
using LitEngine;
namespace LitEngine.Net.TestServer
{
    public class ServerTestWebSocket : MonoBehaviour
    {
        public class UserObject
        {
            private static int countIndex = 1;
            public int Key;
            public Socket socket;

            AsyncCallback recCallback;
            AsyncCallback sendCallback;

            byte[] recBuffer = new byte[1024 * 100];
            
            DataFormat headinfo = new SocketDataFormat(4, 4, DataFormat.CmdPosType.lenFirst, DataFormat.ByteLenType.allbytes);

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
                    recBuffer.Initialize();
                    var ar = socket.BeginReceive(recBuffer, 0, recBuffer.Length, SocketFlags.None, out errorCode, recCallback, null);
                }
                catch (System.Exception erro)
                {
                    DLog.LogFormat("websocket Rec Error.{0}", erro);
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
                        DLog.LogErrorFormat("websocket Send Error.{0}", errorCode);
                    }
                }
                catch (System.Exception erro)
                {
                    DLog.LogFormat("websocket Send Error.{0}", erro);
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
                    String tdata = Encoding.UTF8.GetString(recBuffer);
                    if (!IsAccept(tdata))
                    {
                        SendData tetstdata = new SendData(headinfo,11);
                        tetstdata.AddInt(3);
                        BeginSend(tetstdata.Data, 0, tetstdata.SendLen);
                    }
                    
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    var localIP = endPoint.Address.ToString();
                    string tmsg = $"[WebSocket]{localIP}->{tdata}";
                    Debug.Log(tmsg);
                }

                BeginReceive();
            }

            bool IsAccept(string pdata)
            {
                if (new System.Text.RegularExpressions.Regex("^GET").IsMatch(pdata))
                {
                    const string eol = "\r\n"; // HTTP/1.1 defines the sequence CR LF as the end-of-line marker

                    Byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + eol
                        + "Connection: Upgrade" + eol
                        + "Upgrade: websocket" + eol
                        + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                            System.Security.Cryptography.SHA1.Create().ComputeHash(
                                Encoding.UTF8.GetBytes(
                                    new System.Text.RegularExpressions.Regex("Sec-WebSocket-Key: (.*)").Match(pdata).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                                )
                            )
                        ) + eol
                        + eol);
                    BeginSend(response, 0, response.Length);

                    return true;
                }

                return false;
            }
        }
        // Start is called before the first frame update
        Socket testServer;
        Task recTask;
        bool taskStart = false;

        SafeMap<int, UserObject> userMap = new SafeMap<int, UserObject>();

        private bool initServer = false;
        public void InitServer()
        {
            if (initServer) return;
            initServer = true;
            
            gameObject.name = "websocket-" + 20260;
            //var mTargetPoint = new IPEndPoint(tip, 200236);
            IPAddress tip = IPAddress.Parse("127.0.0.1");
            testServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint tMypoint = new IPEndPoint(IPAddress.Any, 20260);
            testServer.Bind(tMypoint);
            testServer.Listen(100);

            taskStart = true;
            recTask = Task.Run(AccaptThread);

            Application.runInBackground = true;

            Debug.Log("测试TCP服务器启动");
        }

        private void Awake()
        {
            InitServer();
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

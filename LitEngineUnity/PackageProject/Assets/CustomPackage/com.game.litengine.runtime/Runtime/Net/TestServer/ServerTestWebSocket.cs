
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System;
using System.IO;
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
                        DecodeData(recBuffer);
                    }
                    

                }

                BeginReceive();
            }

            bool IsAccept(string pdata)
            {
                string tmsg = $"[WebSocket]->{pdata}";
                if (new System.Text.RegularExpressions.Regex("^GET").IsMatch(pdata))
                {
                    string swk = System.Text.RegularExpressions.Regex.Match(pdata, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
                    string swka = swk + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
                    byte[] swkaSha1 = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(swka));
                    string swkaSha1Base64 = Convert.ToBase64String(swkaSha1);

                    // HTTP/1.1 defines the sequence CR LF as the end-of-line marker
                    byte[] response = Encoding.UTF8.GetBytes(
                        "HTTP/1.1 101 Switching Protocols\r\n" +
                        "Connection: Upgrade\r\n" +
                        "Upgrade: websocket\r\n" +
                        "Sec-WebSocket-Accept: " + swkaSha1Base64 + "\r\n\r\n");
                    
                    BeginSend(response, 0, response.Length);

                    return true;
                }

                return false;
            }

            void DecodeData(byte[] bytes)
            {
                bool fin = (bytes[0] & 0b10000000) != 0;
                bool mask = (bytes[1] & 0b10000000) != 0; // must be true, "All messages from the client to the server have this bit set"
                ulong opcode = (ulong) bytes[0] & 0b00001111; // expecting 1 - text message
                ulong offset = 2;
                ulong msglen = (ulong) bytes[1] & 0b01111111;

                if (msglen == 126) {
                    // bytes are reversed because websocket will print them in Big-Endian, whereas
                    // BitConverter will want them arranged in little-endian on windows
                    msglen = BitConverter.ToUInt16(new byte[] { bytes[3], bytes[2] }, 0);
                    offset = 4;
                } else if (msglen == 127) {
                    // To test the below code, we need to manually buffer larger messages — since the NIC's autobuffering
                    // may be too latency-friendly for this code to run (that is, we may have only some of the bytes in this
                    // websocket frame available through client.Available).
                    msglen = BitConverter.ToUInt64(new byte[] { bytes[9], bytes[8], bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2] },0);
                    offset = 10;
                }

                if (msglen == 0) {
                    Console.WriteLine("msglen == 0");
                } else if (mask) {
                    byte[] decoded = new byte[msglen];
                    byte[] masks = new byte[4] { bytes[offset], bytes[offset + 1], bytes[offset + 2], bytes[offset + 3] };
                    offset += 4;

                    for (ulong i = 0; i < msglen; ++i)
                    {
                        decoded[i] = (byte)(bytes[offset + i] ^ masks[i % 4]);
                    }

                    System.Text.StringBuilder bufferstr = new System.Text.StringBuilder();
                    bufferstr.Append("{");
                    for (int i = 0; i < decoded.Length; i++)
                    {
                        if (i != 0)
                            bufferstr.Append(",");
                        bufferstr.Append(decoded[i]);
                    }
                    bufferstr.Append("}");

                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    var localIP = endPoint.Address.ToString();
                    string tmsg = $"[WebSocket]{localIP}->len={msglen} , bytes = {bufferstr}";
                    Debug.Log(tmsg);
                    
                                        
                    SendData tetstdata = new SendData(headinfo,11);
                    tetstdata.AddInt(3);

                    EnCodeByteSend(opcode,tetstdata.Data,tetstdata.SendLen);

                }
                else
                {
                    DLog.Log("mask bit not set");
                }
            }

            void EnCodeByteSend(ulong pOpCode, byte[] pBuffer, int pSize)
            {
                MemoryStream memoryStream = new MemoryStream();
                var writer = new BinaryWriter(memoryStream);
                
                try
                {
                    byte finBitSetAsByte = (byte) 0x80;
                    byte byte1 = (byte) (finBitSetAsByte | (byte) pOpCode);
                    
                    writer.Write(byte1);
                    byte mask = (byte) 0x00; //client 0x80

                    if (pSize < 126)
                    {
                        byte byte2 = (byte) (mask | (byte) pSize);
                        writer.Write(byte2);
                    }
                    else if (pSize <= ushort.MaxValue)
                    {
                        byte byte2 = (byte) (mask | 126);
                        writer.Write(byte2);
                        writer.Write( (ushort)pSize);
                    }
                    else
                    {
                        byte byte2 = (byte) (mask | 127);
                        writer.Write(byte2);
                        writer.Write((ulong)pSize);
                    }
                    
                    writer.Write(pBuffer,0,pSize);

                    var tsendBuffer = memoryStream.ToArray();
                    
                    BeginSend(tsendBuffer, 0, tsendBuffer.Length);
                }
                catch (Exception e)
                {
                    DLog.LogError(e);
                }
                
                writer.Close();
                writer.Dispose();
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

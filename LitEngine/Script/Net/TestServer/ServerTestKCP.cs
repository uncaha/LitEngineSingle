using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System;
using System.Threading.Tasks;
using LitEngine.Net;
using LitEngine.Net.KCPCommand;
namespace LitEngine.Net.TestServer
{
    public class ServerTestKCP : MonoBehaviour
    {
        // Start is called before the first frame update
        Socket testServer;
        int mLocalPort = 20250;
        byte[] recbuffer = new byte[1024 * 20];
        protected EndPoint mRecPoint;
        Task recTask;
        bool taskStart = false;
        DataFormat headinfo = new SocketDataFormat(4, 4, DataFormat.CmdPosType.lenFirst, DataFormat.ByteLenType.allbytes);
        SendData tetstdata = null;

        private KCP kcpObject;

        IPEndPoint serverPoint = null;
        IPEndPoint tarpoint = null;
        private SwitchQueue<byte[]> recvQueue = new SwitchQueue<byte[]>(128);

        private void Awake()
        {
            
        }

        void Start()
        {
            gameObject.name = "KCP-" + mLocalPort;
            kcpObject = new KCP(1, HandleKcpSend);
            kcpObject.NoDelay(1, 10, 2, 1);
            kcpObject.WndSize(128, 128);

            IPAddress tip = IPAddress.Parse("127.0.0.1");
            //var mTargetPoint = new IPEndPoint(tip, 200236);
            testServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            mRecPoint = new IPEndPoint(IPAddress.Any, 10824);
            serverPoint = new IPEndPoint(IPAddress.Any, mLocalPort);
            testServer.Bind(serverPoint);

            taskStart = true;
            recTask = Task.Run(RecThread);

            Application.runInBackground = true;

            tetstdata = new SendData(headinfo, 10);
            tetstdata.AddInt(3);
            Debug.Log("测试KCP服务器启动");
        }

        private void OnDestroy()
        {
            taskStart = false;
            testServer.Close();
        }

        void RecThread()
        {
            while (taskStart)
            {
                try
                {
                    if (testServer.Available != 0)
                    {
                        int receiveNumber = testServer.ReceiveFrom(recbuffer, SocketFlags.None, ref mRecPoint);
                        IPEndPoint tremot = (IPEndPoint)mRecPoint;
                        if (receiveNumber > 0 && !tremot.Address.Equals(serverPoint))
                        {
                            DLog.Log(tremot);
                            tarpoint = tremot;

                            HandleRecvQueue(recbuffer, receiveNumber);
                            ServerUpdate();
                        }
                    }

                }
                catch (Exception e)
                {
                    Debug.LogError("UDP接收线程异常:" + e);
                }
                Thread.Sleep(1);
            }

        }

        string ShowBuffer(byte[] pBuffer, int pSize)
        {
            System.Text.StringBuilder bufferstr = new System.Text.StringBuilder();
            bufferstr.Append("{");
            for (int i = 0; i < pSize; i++)
            {
                if (i != 0)
                    bufferstr.Append(",");
                bufferstr.Append(pBuffer[i]);
            }
            bufferstr.Append("}");
            return bufferstr.ToString();
        }

        public bool AddSend(byte[] buff, int size)
        {
            return kcpObject.Send(buff, size) >= 0;
        }

        private void HandleKcpSend(byte[] buff, int size)
        {
            if (testServer == null || tarpoint == null) return;
            try
            {
                var ar = testServer.BeginSendTo(buff, 0, size, SocketFlags.None, tarpoint, SendAsyncCallback, buff);
            }
            catch (System.Exception erro)
            {
                DLog.LogFormat("KCP Send Error.{0}", erro);
            }
        }

        void SendAsyncCallback(IAsyncResult result)
        {
            testServer.EndSendTo(result);
            byte[] tbuff = result.AsyncState as byte[];
            if (result.IsCompleted)
            {
            }
            if (tbuff != null)
            {

            }
        }

        // Update is called once per frame
        void ServerUpdate()
        {
            uint currentTimeMS = GetClockMS();
            if (needKcpUpdateFlag || currentTimeMS >= nextKcpUpdateTime)
            {
                kcpObject.Update(currentTimeMS);
                nextKcpUpdateTime = kcpObject.Check(currentTimeMS);
                needKcpUpdateFlag = false;
            }
        }

        private void HandleRecvQueue(byte[] pBuffer,int pLen)
        {
            int ret = kcpObject.Input(pBuffer, pLen);

            //收到的不是一个正确的KCP包
            if (ret < 0)
            {
                string tstr = ShowBuffer(pBuffer, pLen);
                DLog.LogFormat("收到了错误的kcp包: {0}", tstr);
                return;
            }

            needKcpUpdateFlag = true;

            for (int size = kcpObject.PeekSize(); size > 0; size = kcpObject.PeekSize())
            {
                DLog.Log("size =" + size);
                var recvBuffer = new byte[size];
                int treclen = kcpObject.Recv(recvBuffer, recvBuffer.Length);
                if (treclen > 0)
                {
                    PushRecData(recvBuffer, treclen);
                }
            }
        }

        protected void PushRecData(byte[] pRecbuf, int pSize)
        {
            DLog.Log( ShowBuffer(pRecbuf, pSize));
            AddSend(tetstdata.Data, tetstdata.SendLen);

            // ReceiveData tssdata = new ReceiveData(pRecbuf, 0);

        }

        private static readonly DateTime UTCTimeBegin = new DateTime(1970, 1, 1);
        public static UInt32 GetClockMS()
        {
            return (UInt32)(Convert.ToInt64(DateTime.UtcNow.Subtract(UTCTimeBegin).TotalMilliseconds) & 0xffffffff);
        }

        private bool needKcpUpdateFlag = false;
        private uint nextKcpUpdateTime = 0;
    }

}

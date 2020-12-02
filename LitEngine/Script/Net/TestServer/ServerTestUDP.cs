using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System;
using System.Threading.Tasks;
using LitEngine.Net;
namespace LitEngine.Net.TestServer
{

    public class ServerTest : MonoBehaviour
    {
        // Start is called before the first frame update
        Socket testServer;
        int mLocalPort = 20236;
        byte[] recbuffer = new byte[1024 * 20];
        protected EndPoint mRecPoint;
        Task recTask;
        bool taskStart = false;
        SendData tetstdata = new SendData(11);
        void Start()
        {
            gameObject.name = "UDP-" + mLocalPort;
            // string hostName = Dns.GetHostName();
            // IPHostEntry iPHostEntry = Dns.GetHostEntry(hostName);
            // Debug.Log(iPHostEntry.AddressList[0].ToString());
            IPAddress tip = IPAddress.Parse("127.0.0.1");
            //var mTargetPoint = new IPEndPoint(tip, 200236);
            testServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            mRecPoint = new IPEndPoint(IPAddress.Any, mLocalPort);
            IPEndPoint tMypoint = new IPEndPoint(IPAddress.Any, mLocalPort);
            testServer.Bind(tMypoint);

            taskStart = true;
            recTask = Task.Run(RecThread);

            Application.runInBackground = true;

            tetstdata.AddInt(3);
            Debug.Log("测试UDP服务器启动");
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
                        if (receiveNumber > 0)
                        {
                            testServer.SendTo(tetstdata.Data, 0, tetstdata.SendLen, SocketFlags.None, tremot);
                            System.Text.StringBuilder bufferstr = new System.Text.StringBuilder();
                            bufferstr.Append("{");
                            for (int i = 0; i < receiveNumber; i++)
                            {
                                if (i != 0)
                                    bufferstr.Append(",");
                                bufferstr.Append(recbuffer[i]);
                            }
                            bufferstr.Append("}");
                            string tmsg = string.Format("{0}{1}", tremot.Address.ToString(), bufferstr);
                            Debug.Log(tmsg);
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

        // Update is called once per frame
        void Update()
        {

        }
    }

}

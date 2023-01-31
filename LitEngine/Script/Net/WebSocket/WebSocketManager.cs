using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


namespace LitEngine.Net
{

    public abstract class WebSocketManager<T> : NetBase<T> where T : WebSocketManager<T>
    {

        private ClientWebSocket webSocket;
        private CancellationToken cancellation = new CancellationToken();

        private bool connecting = false;

        #region 构造析构

        protected WebSocketManager()
        {
            mNetTag = GetType().Name;
        }

        #endregion

        #region staticfun

        static protected T sInstance = null;

        static protected T Instance
        {
            get
            {
                if (sInstance == null)
                {
                    GameObject tobj = new GameObject();
                    DontDestroyOnLoad(tobj);
                    tobj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                    sInstance = tobj.AddComponent<T>();
                    sInstance.Oninit();
                    tobj.name = sInstance.mNetTag + "-Object";
                }

                return sInstance;
            }
        }

        #endregion

        #region 连接

        protected void Oninit()
        {
        }

        override sealed protected void RestSocketInfo()
        {

        }

        override sealed protected void KillSocket()
        {
            try
            {
                if (webSocket != null)
                {
                    webSocket.Abort();
                }
            }
            catch
            {
                // ignored
            }

            webSocket = null;
        }


        public async void ConnectAsync()
        {
            if (webSocket == null)
            {
                webSocket = new ClientWebSocket();
            }

            switch (webSocket.State)
            {
                case WebSocketState.Connecting:
                    DLog.LogError($"[{mNetTag}] is Connected.");
                    return;
                case WebSocketState.Open:
                    DLog.LogError($"[{mNetTag}] is Connecting.");
                    return;
                case WebSocketState.Aborted:
                    DLog.LogError($"[{mNetTag}] is Dispose.");
                    return;
                case WebSocketState.Closed:
                    DLog.LogError($"[{mNetTag}] is Dispose.");
                    return;
            }

            if (connecting)
            {
                DLog.LogError($"[{mNetTag}] is Connecting.");
                return;
            }

            connecting = true;

            try
            {
                var tconnectTask = webSocket.ConnectAsync(new Uri(mHostName), cancellation);
                await tconnectTask;

                if (webSocket.State == WebSocketState.Open)
                {
                    Task.Run(async () => { ReceiveAsync(); }, new CancellationToken());
                }
            }
            catch (Exception e)
            {
                DLog.LogError($"[{mNetTag}]: ConnectAsync-> {e}");
            }

            connecting = false;
        }

        async void ReceiveAsync()
        {
            try
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(mRecbuffer), new CancellationToken());

                while (!result.CloseStatus.HasValue)
                {
                    PushRecData(mRecbuffer, result.Count);

                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(mRecbuffer), new CancellationToken());
                }
            }
            catch (Exception e)
            {
                DLog.LogError($"[{mNetTag}]: ReceiveAsync-> {e}");
            }
        }

        private void PushRecData(byte[] pBuffer, int pSize)
        {
            mBufferData.Push(pBuffer, pSize);
            while (mBufferData.IsFullData())
            {
                ReceiveData tssdata = mBufferData.GetReceiveData();
                mResultDataList.Enqueue(tssdata);
            }
        }

        #endregion

        public bool isConnected
        {
            get { return webSocket != null && webSocket.State == WebSocketState.Open; }
        }

        static public bool SendBytes(byte[] pBuffer, int pSize)
        {
            if (pBuffer == null || !Instance.isConnected) return false;
            return Instance.Send(pBuffer, pSize);
        }

        static public bool SendBytes(byte[] pBuffer)
        {
            if (pBuffer == null || !Instance.isConnected) return false;
            SendBytes(pBuffer, pBuffer.Length);
            return true;
        }

        bool Send(byte[] pBytes, int pSize)
        {
            if (webSocket.State != WebSocketState.Open)
            {
                return false;
            }

            DoSend(pBytes, pSize);
            return true;
        }

        async Task<bool> DoSend(byte[] pBytes, int pSize)
        {
            //发送消息
            var ttask = webSocket.SendAsync(new ArraySegment<byte>(pBytes, 0, pSize), WebSocketMessageType.Text, true,
                CancellationToken.None);
            await ttask;

            return true;
        }


        override protected void MainThreadUpdate()
        {
            UpdateRecMsg();
        }

    }
}
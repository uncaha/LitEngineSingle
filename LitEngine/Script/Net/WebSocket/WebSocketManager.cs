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


        #region 构造析构

        protected WebSocketManager()
        {
            mNetTag = GetType().Name;
        }

        #endregion

        #region 连接

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

        override public void ConnectToServer()
        {
            ConnectAsync();
        }

        async void ConnectAsync()
        {
            if (IsCOrD())
            {
                DLog.LogError(mNetTag + string.Format("[{0}]Closing or Connecting.", mNetTag));
                return;
            }

            if (isConnected)
            {
                DLog.LogError($"[{mNetTag}] is Connected.");
                return;
            }

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



            mState = TcpState.Connecting;

            try
            {
                var tconnectTask = webSocket.ConnectAsync(new Uri(mHostName), cancellation);
                await tconnectTask;

                if (webSocket.State == WebSocketState.Open)
                {
                    mState = TcpState.Connected;
                    Task.Run(async () => { ReceiveAsync(); }, new CancellationToken());
                    AddMainThreadMsgReCall(GetMsgReCallData(MessageType.Connected, mNetTag + " Connected."));
                }
                else
                {
                    mState = TcpState.Closed;
                    AddMainThreadMsgReCall(GetMsgReCallData(MessageType.ConectError, mNetTag + "Connect fail. " + webSocket.State));
                }
            }
            catch (Exception e)
            {
                mState = TcpState.Closed;
                DLog.LogError($"[{mNetTag}]: ConnectAsync-> {e}");
                AddMainThreadMsgReCall(GetMsgReCallData(MessageType.ConectError, mNetTag + "Connect fail. " + e.Message));
            }

        }

        async void ReceiveAsync()
        {
            try
            {
                var ttask = webSocket.ReceiveAsync(new ArraySegment<byte>(mRecbuffer), new CancellationToken());
                var result = await ttask;

                if (ttask.Exception != null || result.CloseStatus.HasValue)
                {
                    throw new NullReferenceException($"{mNetTag} : error = {ttask?.Exception}");
                }

                while (!result.CloseStatus.HasValue)
                {
                    PushRecData(mRecbuffer, result.Count);
                    ttask = webSocket.ReceiveAsync(new ArraySegment<byte>(mRecbuffer), new CancellationToken());
                    result = await ttask;

                    if(ttask.Exception != null || result.CloseStatus.HasValue)
                    {
                        throw new NullReferenceException($"{mNetTag} : error = {ttask?.Exception}");
                    }

                }

                
            }
            catch (Exception e)
            {
                DLog.LogError($"[{mNetTag}]: ReceiveAsync-> {e}");
                CloseSRThread();
                AddMainThreadMsgReCall(GetMsgReCallData(MessageType.ReceiveError, $"{mNetTag} : {e.Message}"));
            }
        }

        private void PushRecData(byte[] pBuffer, int pSize)
        {
            mBufferData.Push(pBuffer, pSize);
            while (mBufferData.IsFullData())
            {
                ReceiveData tssdata = mBufferData.GetReceiveData();
                mResultDataList.Enqueue(tssdata);

                DebugMsg(tssdata.Cmd, tssdata.Data, 0, tssdata.Len, $"{mNetTag}:接收-ReceiveData");
            }
        }

        #endregion

        public bool isConnected
        {
            get { return webSocket != null && webSocket.State == WebSocketState.Open; }
        }
        
        override public bool Send(SendData pData)
        {
            if (!isConnected) return false;
            return Send(pData.Data, pData.SendLen);
        }
        
        override public bool Send(byte[] pBuffer, int pSize)
        {
            if (pBuffer == null || !Instance.isConnected) return false;
            return StartSend(pBuffer, pSize);
        }

        bool StartSend(byte[] pBytes, int pSize)
        {
            if (webSocket.State != WebSocketState.Open)
            {
                return false;
            }

            DebugMsg(0, pBytes, 0, pSize, $"{mNetTag}:Send");

            DoSend(pBytes, pSize);
            return true;
        }

        async Task<bool> DoSend(byte[] pBytes, int pSize)
        {
            //发送消息
            var ttask = webSocket.SendAsync(new ArraySegment<byte>(pBytes, 0, pSize), WebSocketMessageType.Text, true,
                CancellationToken.None);
            await ttask;

            if (ttask.Exception != null)
            {
                CloseSRThread();
                AddMainThreadMsgReCall(new NetMessage(MessageType.SendError, mNetTag + "-" + ttask.Exception.Message));

                return false;
            }

            return true;
        }


        override protected void MainThreadUpdate()
        {
            UpdateRecMsg();
        }

    }
}
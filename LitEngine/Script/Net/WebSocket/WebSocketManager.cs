﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
        
        override public bool isConnected
        {
            get { return base.isConnected && webSocket != null && webSocket.State == WebSocketState.Open; }
        }

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

        private ConnectMessage connectMsg = null;
        override public void ConnectToServer(System.Action<bool> pOnDone)
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
            
            mState = TcpState.Connecting;
            
            connectMsg = new ConnectMessage();
            connectMsg.OnDone = pOnDone;
            
            DLog.Log($"[{mNetTag}] start Connect.");
            
            Task.Run(ConnectAsync, new CancellationToken());
        }

        async void ConnectAsync()
        {
            
            if (webSocket == null)
            {
                webSocket = new ClientWebSocket();
            }

            DLog.Log($"[{mNetTag}] webSocket ConnectAsync.");
            try
            {
                var tconnectTask = webSocket.ConnectAsync(new Uri(mHostName), cancellation);
                await tconnectTask;
                
                

                if (webSocket.State == WebSocketState.Open)
                {
                    mStartThread = true;
                    
                    mState = TcpState.Connected;

                    connectMsg.result = true;
                    AddMainThreadMsgReCall(connectMsg);
                    DLog.Log( $"{mNetTag} Connected.");
                    
                    var trectask = Task.Run(ReceiveAsync, new CancellationToken());
                }
                else
                {
                    CloseSRThread();

                    connectMsg.result = false;
                    AddMainThreadMsgReCall(connectMsg);
                    DLog.Log( $"{mNetTag} Connect fail.  state = {webSocket.State}");
                }
                
                DLog.Log($"[{mNetTag}] webSocket ConnectAsync end.");
            }
            catch (Exception e)
            {
                mState = TcpState.Closed;
                
                connectMsg.result = false;
                AddMainThreadMsgReCall(connectMsg);
                DLog.Log( $"[{mNetTag}]: ConnectAsync-> {e}");
            }

        }

        virtual protected async void ReceiveAsync()
        {
            try
            {
                while (true)
                {
                    var tmem = new MemoryStream();
                    try
                    {
                        while (true)
                        {
                            var tbuffer = WebSocket.CreateClientBuffer(2048,0);
                            var ttask = webSocket.ReceiveAsync(tbuffer, CancellationToken.None);
                            var result = await ttask;
                            
                            if (ttask.Exception != null || result.CloseStatus.HasValue)
                            {
                                throw new NullReferenceException($"{mNetTag} : error = {ttask?.Exception}");
                            }

                            if (tbuffer.Array != null)
                            {
                                tmem.Write(tbuffer.Array, tbuffer.Offset, result.Count);
                            }
                            
                            if (result.EndOfMessage)
                            {
                                var tbytes = tmem.ToArray();
                                PushRecData(tbytes, tbytes.Length);
                                break;
                            }
                            
                        }
                        
                        tmem?.Close();
                        tmem?.Dispose();
                    }
                    catch (Exception e)
                    {
                        tmem?.Close();
                        tmem?.Dispose();
                        throw new NullReferenceException($"{mNetTag} : error = {e.Message}");
                    }

                }
            }
            catch (Exception e)
            {
                OnNetError(MessageType.SendError, $"{mNetTag} : {e.Message}");
            }
        }

        override protected void PushRecData(byte[] pBuffer, int pSize)
        {
            ReceiveData tssdata = new ReceiveData(0,pBuffer);
            mResultDataList.Enqueue(tssdata);
            DebugMsg(tssdata.Cmd, tssdata.Data, 0, tssdata.Len, $"{mNetTag}:接收-ReceiveData");
        }

        #endregion

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

        virtual protected async void DoSend(byte[] pBytes, int pSize)
        {
            //发送消息
            var ttask = webSocket.SendAsync(new ArraySegment<byte>(pBytes, 0, pSize), WebSocketMessageType.Text, true,
                CancellationToken.None);
            await ttask;

            if (ttask.Exception != null)
            {
                OnNetError(MessageType.SendError, mNetTag + "-" + ttask.Exception.Message);
            }
        }


        override protected void MainThreadUpdate()
        {
            UpdateRecMsg();
        }

    }
}
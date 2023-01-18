using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LitEngine.Net
{
    public abstract class WebSocketManager<T> : MonoBehaviour where T : WebSocketManager<T>
    {
        public string NetTag { get; protected set; } = "";
        public TcpState State { get; protected set; } = TcpState.None;
        public string HostName { get; protected set; }
        public int Port { get; protected set; }
        public event System.Action<NetMessage> MessageDelgate = null;
        
        private ClientWebSocket webSocket;
        private CancellationToken cancellation = new CancellationToken();

        private bool connecting = false;
        
        private const int readMaxLen = 2048 * 4;
        private byte[] recbuffer = new byte[readMaxLen];
        
        private BufferBase bufferData = new BufferBase(1024 * 400);
        #region 构造析构

        protected WebSocketManager()
        {
            NetTag = GetType().Name;
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
                    tobj.name = sInstance.NetTag + "-Object";
                }
                return sInstance;
            }
        }
        
        static public void Init(string pHostName, int pPort, System.Action<NetMessage> pMsgDelgate = null)
        {
            Instance.InitSocket(pHostName, pPort,pMsgDelgate);
        }

        #endregion

        #region 连接

        protected void Oninit()
        {

        }
        
        virtual protected void InitSocket(string pHostName, int pPort, System.Action<NetMessage> pMsgDelgate)
        {
            HostName = pHostName;
            Port = pPort;
            gameObject.name = $"{NetTag}-Server:{pHostName}";
            MessageDelgate = pMsgDelgate;
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
                    DLog.LogError(NetTag + $"[{NetTag}] is Connected.");
                    return;
                case WebSocketState.Open:
                    DLog.LogError(NetTag + $"[{NetTag}] is Connecting.");
                    return;
                case WebSocketState.Aborted:
                    DLog.LogError(NetTag + $"[{NetTag}] is Dispose.");
                    return;
                case WebSocketState.Closed:
                    DLog.LogError(NetTag + $"[{NetTag}] is Dispose.");
                    return;

            }

            if (connecting)
            {
                DLog.LogError(NetTag + $"[{NetTag}] is Connecting.");
                return;
            }

            connecting = true;

            try
            {
                var tconnectTask = webSocket.ConnectAsync(new Uri(HostName),cancellation);
                await tconnectTask;

                if (webSocket.State == WebSocketState.Open)
                {
                    Task.Run(async () =>
                    {
                        ReceiveAsync();
                    }, new CancellationToken());
                }
            }
            catch (Exception e)
            {
                DLog.LogError($"[{NetTag}]: ConnectAsync-> {e}");
            }

            connecting = false;
        }

        async void ReceiveAsync()
        {
            try
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(recbuffer), new CancellationToken());

                while (!result.CloseStatus.HasValue)
                {
                    PushRecData(recbuffer,result.Count);
                
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(recbuffer), new CancellationToken());
                }
            }
            catch (Exception e)
            {
                DLog.LogError($"[{NetTag}]: ReceiveAsync-> {e}");
            }

        }
        
        private void PushRecData(byte[] pBuffer, int pSize)
        {
            bufferData.Push(pBuffer, pSize);
            while (bufferData.IsFullData())
            {
                ReceiveData tssdata = bufferData.GetReceiveData();

                //DebugMsg(tssdata.Cmd, tssdata.Data, 0, tssdata.Len, "接收-ReceiveData");
            }
        }

        #endregion
        public bool isConnected { get { return webSocket!= null && webSocket.State == WebSocketState.Open; } }
        static public bool SendBytes(byte[] pBuffer,int pSize)
        {
            if (!Instance.isConnected) return false;
            Instance.Send(pBuffer);
            return true;
        }
        public async Task<bool> Send(byte[] pBytes)
        {
            if (webSocket.State != WebSocketState.Open)
                return false;
            //发送消息
            var ttask = webSocket.SendAsync(new ArraySegment<byte>(pBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            await ttask;
 
            return true;
        }
    }
}
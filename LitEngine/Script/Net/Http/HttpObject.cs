using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LitEngine.Net
{
    public delegate void HttpErorEvent(int statuCode, string error, string url);
    public delegate void HttpFinishEvent<TResponse>(TResponse response);
    public enum HttpState
    {
        none = 0,
        waitSend,
        sending,
        finish,
        timeTooLong,
        done,
    }

    public class HttpObject : IDisposable
    {
        public static int httpObjectMax = 1;
        public IHttpManager httpManager;
        public int Key { get; protected set; } = 1;

        public string Url { get; protected set; }
        public string requestData { get; protected set; }
        public string responseString { get; protected set; }
        public HttpState state { get; protected set; } = HttpState.none;
        public int statusCode { get; protected set; } = -1;
        public string ErrorMsg { get; protected set; } = null;

        public bool IsCallStartEvent { get; private set; } = false;

        private long _slowTimeBoundaries = -1;
        public long slowTimeBoundaries
        {
            get
            {
                return _slowTimeBoundaries;
            }
            set
            {
                if (IsLockProperty) return;
                _slowTimeBoundaries = value;
            }
        }

        private bool _isMask = false;
        public bool isMask
        {
            get
            {
                return _isMask;
            }
            set
            {
                if (IsLockProperty) return;
                _isMask = value;
            }
        }

        public bool IsLockProperty
        {
            get
            {
                return !(state == HttpState.none || state == HttpState.waitSend);
            }
        }


        protected long ticks;
        public HttpObject()
        {
            Key = httpObjectMax++;
        }
        ~HttpObject()
        {
            Dispose(false);
        }

        protected bool disposed { get; private set; } = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            try
            {
                if (disposing)
                    DisposeNoGcCode();
            }
            catch (System.Exception err)
            {
                DLog.LogFormat("[HttpObject]:{0}", err);
            }


            disposed = true;
        }

        virtual protected void DisposeNoGcCode()
        {

        }

        virtual public void SetHeader(string pKey, string pValue)
        {

        }

        virtual public void StartSend()
        {
            if (disposed) return;
            if (state != HttpState.none) return;
            state = HttpState.waitSend;
            httpManager.Add(this);
            //GuildDLog.LogFormat("[HttpRequest+Add]: URL = {0}", Url);
        }

        virtual protected void OnFinshed()
        {
            SendEndEvent();
            httpManager.Remove(Key);
        }

        public void Update()
        {
            try
            {
                switch (state)
                {
                    case HttpState.sending:
                        UpdateSending();
                        break;
                    case HttpState.waitSend:
                        WaitSendUpdate();
                        break;
                    case HttpState.finish:
                        UpdateFinish();
                        break;
                    default:
                        break;
                }
            }
            catch (System.Exception err)
            {
                DLog.LogError(err);
            }
        }

        virtual protected void WaitSendUpdate()
        {

        }

        virtual protected void UpdateFinish()
        {

        }

        void UpdateSending()
        {
            if (slowTimeBoundaries < 0) return;
            long tdalyTime = (System.DateTime.Now.Ticks - ticks) / 10000;

            if (tdalyTime > slowTimeBoundaries)
            {
                state = HttpState.timeTooLong;
                httpManager.AddSlowHttpObject(this);
            }
        }

        protected void SendStartEvent()
        {
            IsCallStartEvent = true;
            httpManager.OnHttpStartSend(this);
        }

        protected void SendEndEvent()
        {
            if (!IsCallStartEvent) return;
            httpManager.OnHttpFinished(this);
            IsCallStartEvent = false;
        }
    }

}

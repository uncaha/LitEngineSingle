using System;
using System.Collections;
using System.Collections.Generic;
using LitEngine.Tool;

namespace LitEngine.Net.Http
{
    public delegate void HttpErorEvent(int statuCode, string error, string url, HttpObject pSender);
    public delegate void HttpFinishEvent<TResponse>(TResponse response, HttpObject pSender);
    public enum HttpState
    {
        none = 0,
        waitSend,
        sending,
        finish,
        timeTooLong,
        done,
        pamarsInvalid,
    }
    
    public class HttpObject : IDisposable
    {
        public static int httpObjectMax = 1;
        public IHttpManager httpManager;
        public int Key { get; protected set; } = 1;
        public HttpErrorObject Error { get; protected set; }
        public string ErrorMsg { get; protected set; }
        public HTTPMethodType methodType { get; protected set; }
        public string Url { get; protected set; }
        public object requestData { get; protected set; }
        public bool useCrypto { get; protected set; } = false;
        public HttpState state { get; protected set; } = HttpState.none;
        public int statusCode { get; protected set; } = -1;
        public int requestCode { get; protected set; } = -1;
        public int exceptionStatus { get; protected set; } = 0;
        public string responseString { get; protected set; } = null;
        public object responseObject { get; protected set; } = null;
        public bool IsCallStartEvent { get; private set; } = false;
        
        public Dictionary<string, string> headers { get; protected set; } = new Dictionary<string, string>();
        
        public int timeOut { get; protected set; } = 60;

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

        public bool IsReTry { get; set; } = true;

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
                DLog.LogFormat(httpManager.Tag,"[HttpObject]:{0}", err);
            }


            disposed = true;
        }

        virtual protected void DisposeNoGcCode()
        {

        }

        virtual public string GetServerDataTime()
        {
            return GetResponseHeader("Date");
        }

        public void SetHeader(string pKey, string pValue)
        {
            if (!headers.ContainsKey(pKey))
            {
                headers.Add(pKey, pValue);
            }
            else
            {
                headers[pKey] = pValue;
            }
        }

        virtual public string GetResponseHeader(string pKey)
        {
            return null;
        }
        
        virtual public void StartSend()
        {
            if (disposed) return;
            if (state != HttpState.none) return;
            state = HttpState.waitSend;
            httpManager.Add(this);
            //GuildDLog.LogFormat("[HttpRequest+Add]: URL = {0}", Url);
        }
        
        virtual public void ThrowError(string pError)
        {
            if (disposed) return;
            if (state != HttpState.none) return;
            state = HttpState.pamarsInvalid;
            ErrorMsg = pError;
            httpManager.Add(this);
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
                        UpdateSlow();
                        UpdateSending();
                        break;
                    case HttpState.waitSend:
                        WaitSendUpdate();
                        break;
                    case HttpState.finish:
                        UpdateFinish();
                        break;
                    case HttpState.pamarsInvalid:
                        UpdatePamarsInvailid();
                        break;
                    default:
                        break;
                }
            }
            catch (System.Exception err)
            {
                DLog.LogError(httpManager.Tag,err);
            }
        }

        virtual protected void WaitSendUpdate()
        {

        }

        virtual protected void UpdateFinish()
        {
            
        }
        
        virtual protected void UpdatePamarsInvailid()
        {
            state = HttpState.done;
            httpManager.Remove(Key);
        }

        virtual protected void UpdateSending()
        {
            
        }

        void UpdateSlow()
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

    public class HttpBaseResponse
    {
        public int code = -1;
        public string message;
    }

    
    public class HttpErrorObject
    {
        public int errorStatus;
        public int requestCode;
        public int statusCode;
        public int responseCode;
        public int exceptionStatus;
        public string errorMessage;
        public string responseJson;
        override public string ToString()
        {
            return DataConvert.ToJson(this);
        }
    }
}

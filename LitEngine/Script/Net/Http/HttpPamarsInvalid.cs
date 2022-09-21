using System;

namespace LitEngine.Net.Http
{
    public class HttpPamarsInvalid<TResponse> : HttpObject
    {
        public event HttpErorEvent onError;

        public HttpPamarsInvalid(string pUrl) : base()
        {
            Url = pUrl;
            ErrorMsg = "Params is invaild";
        }
        public HttpPamarsInvalid(string pUrl,string pErrorMsg) : base()
        {
            Url = pUrl;
            ErrorMsg = pErrorMsg;
        }

        override public void StartSend()
        {
            if (disposed) return;
            if (state != HttpState.none) return;
            state = HttpState.waitSend;
            httpManager.Add(this);
        }

        override protected void WaitSendUpdate()
        {
            httpManager.Remove(Key);
            state = HttpState.done;
            try
            {
                var terror = new HttpErrorObject();
                terror.errorMessage = ErrorMsg;

                DLog.LogFormat(httpManager.Tag, "[StartRequest+PamarsInvalid]: {0}. URL:{1}",ErrorMsg, Url);
                onError?.Invoke((int)HttpCodeState.error,terror.ToString(), Url, this);
            }
            catch (Exception ex)
            {
                DLog.LogErrorFormat(httpManager.Tag, "Type:{0}, error:{1}", this.GetType().Name, ex);
            }
        }
    }
}
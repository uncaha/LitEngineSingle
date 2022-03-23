using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace LitEngine.Net
{
    public class HttpCSharpBase : HttpObject
    {
        public static readonly HttpClient httpClient;

        static HttpCSharpBase()
        {
            httpClient = new HttpClient();
        }
    }

    public class HttpObjectSharp : HttpCSharpBase
    {
        public enum HttpCodeState
        {
            none = 0,
            timeOUt = 5,
            error = 4,
        }

        public event HttpErorEvent onError;
        public event HttpFinishEvent<string> onFinish;
        public HTTPMethodType methodType { get; private set; }

        public Task task { get; private set; } = null;


        private int timeOut = 60;
        Dictionary<string, string> headers = new Dictionary<string, string>();

        private HttpRequestMessage requestMsg;


        public HttpObjectSharp(string pUrl, string pData, HTTPMethodType pType, int pTimeOut) : base()
        {
            Url = pUrl;
            requestData = pData;

            methodType = pType;
            timeOut = pTimeOut;
        }

        override protected void DisposeNoGcCode()
        {
            onFinish = null;
            onError = null;
            Abort();
        }

        override public void SetHeader(string pKey, string pValue)
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

        public void Abort()
        {
            try
            {
                task?.Dispose();
                task = null;
                CloseHttpClient();
            }
            catch (System.Exception erro)
            {
                DLog.LogErrorFormat("[URL] = {0},[Error] = {1}", Url, erro.Message);
            }
        }

        private void CloseHttpClient()
        {

        }

        override protected void UpdateFinish()
        {
            OnFinshed();
            state = HttpState.done;


            long tdalyTime = (System.DateTime.Now.Ticks - ticks) / 10000;
            DLog.LogFormat("[HTTPResponse]:Url = {0},delayTime = {1},StatusCode = {2} Data = {3}",
                Url, tdalyTime, statusCode, responseString);
            try
            {
                if (statusCode == 200 || statusCode == 304)
                {
                    OnHttpFinish(responseString);
                }
                else
                {
                    OnHttpError(ErrorMsg, null, statusCode);
                }
            }
            catch (Exception e)
            {
                DLog.LogError(e);
                ErrorMsg = e.Message;
                OnHttpError(ErrorMsg, null, statusCode);
            }
        }

        override protected void WaitSendUpdate()
        {
            bool isSuccess = false;
            try
            {
                state = HttpState.sending;

                ticks = System.DateTime.Now.Ticks;
                SendAsync();

                DLog.LogFormat("[HttpRequest]: URL = {0},body = {1}", Url, requestData);
                isSuccess = true;
            }
            catch (System.Exception erro)
            {
                statusCode = (int)HttpCodeState.error;
                OnHttpError(erro.ToString(), null, statusCode);
            }

            if (isSuccess)
            {
                SendStartEvent();
            }
            else
            {
                Dispose();
            }
        }

        void SendAsync()
        {
            if (task != null) return;
            //task = System.Threading.Tasks.Task.Run((System.Action)ReadNetBytes);
            ReadNetBytes();
        }

        void ReadNetBytes()
        {
            try
            {
                SendRequest();
            }
            catch (System.Exception ex)
            {
                statusCode = (int)HttpCodeState.error;
                ErrorMsg = ex.ToString();
                OnTaskDone();
            }
        }

        async void SendRequest()
        {
            statusCode = -1;

            var tmethod = new HttpMethod(methodType.ToString());
            requestMsg = new HttpRequestMessage(tmethod, Url);

            var tResponse = await httpClient.SendAsync(requestMsg);

            CheckRequest();
            CheckHeader();

            await CheckRespose(tResponse);

            OnTaskDone();
        }

        void CheckRequest()
        {
            if (methodType == HTTPMethodType.GET)
            {
                var tcache = HttpCacheManager.Instance.GetCache(Url);
                if (tcache != null)
                {
                    SetHeader("If-None-Match", tcache.ETag);
                    SetHeader("If-Modified-Since", tcache.LastModified);
                }

                if (!string.IsNullOrEmpty(requestData))
                {
                    SetHeader("Data", requestData);
                }
            }
            else
            {
                requestMsg.Content = new StringContent(requestData, Encoding.UTF8, "application/json");
            }
        }


        void CheckHeader()
        {
            foreach (var item in headers)
            {
                if (string.IsNullOrEmpty(item.Key) || string.IsNullOrEmpty(item.Value)) continue;
                switch (item.Key)
                {
                    case "Content-Type":
                        break;
                    default:
                        requestMsg.Headers.TryAddWithoutValidation(item.Key, item.Value);
                        break;
                }
            }
        }

        async Task CheckRespose(HttpResponseMessage response)
        {
            if (response == null)
            {
                statusCode = (int)HttpCodeState.error;
                ErrorMsg = "response = null";
                return;
            }

            statusCode = (int)response.StatusCode;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                responseString = await response.Content.ReadAsStringAsync();
            }
            else if (response.StatusCode == HttpStatusCode.NotModified)
            {
                var tcache = HttpCacheManager.Instance.GetCache(Url);
                responseString = tcache?.responseData;
            }

            CheckCache(response);
        }

        void CheckCache(HttpResponseMessage response)
        {
            if (methodType != HTTPMethodType.GET) return;
            string tEtag = null;
            {
                if (response.Headers.TryGetValues("ETag", out IEnumerable<string> values))
                {
                    List<string> tlist = new List<string>(values);
                    tEtag = tlist[0];
                }
            }


            string tlasttime = null;
            {
                if (response.Headers.TryGetValues("Last-Modified", out IEnumerable<string> values))
                {
                    List<string> tlist = new List<string>(values);
                    tlasttime = tlist[0];
                }
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                AddCache(tEtag, tlasttime);
            }
            else if (response.StatusCode == HttpStatusCode.NotModified)
            {
                var tcache = HttpCacheManager.Instance.GetCache(Url);

                if (tcache != null)
                {
                    responseString = tcache.responseData;
                    tcache.LastModified = tlasttime;

                    HttpCacheManager.Instance.AddSave(tcache);
                }
            }
        }


        void AddCache(string pETag, string pTime)
        {
            if (!string.IsNullOrEmpty(pETag))
            {
                var tcache = new HttpCacheObject(Url, true);
                tcache.responseData = responseString;
                tcache.Url = Url;
                tcache.LastModified = pTime;
                tcache.ETag = pETag;

                HttpCacheManager.Instance.AddCache(tcache);
            }
        }

        void OnTaskDone()
        {
            state = HttpState.finish;
        }

        void OnHttpError(string pMsg, HttpWebResponse response, int pState)
        {
            try
            {
                int tstatecode = response != null ? (int)response.StatusCode : 100000;
                int statuCode = tstatecode + pState;
                onError?.Invoke(statuCode, pMsg, Url);
            }
            catch (System.Exception erro)
            {
                DLog.LogErrorFormat("onError called error : {0}, {1}", Url, erro);
            }
        }

        void OnHttpFinish(string pData)
        {
            try
            {
                onFinish(pData);
            }
            catch (System.Exception erro)
            {
                DLog.LogErrorFormat("response called error : {0}, {1}", Url, erro);
            }
        }
    }
}
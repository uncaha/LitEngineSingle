using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace LitEngine.Net
{
    public class HttpCSharpBase : HttpObject
    {
        public static readonly HttpClient httpClient;

        static HttpCSharpBase()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Connection","Keep-Alive");
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            
            var cache = HttpCacheManager.Instance;
        }
    }

    public class HttpObjectSharp : HttpCSharpBase
    {
        public enum HttpCodeState
        {
            none = 0,
            
            error = 4,
            timeOUt = 5,
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
                if (task != null)
                {
                    task.Dispose();
                    task = null;
                }
                
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
                if (statusCode == (int)HttpStatusCode.OK || statusCode == (int)HttpStatusCode.NotModified)
                {
                    OnHttpFinish(responseString);
                }
                else
                {
                    OnHttpError(ErrorMsg, responseString, statusCode);
                }
            }
            catch (Exception e)
            {
                DLog.LogError(e);
                ErrorMsg = e.Message;
                OnHttpError(ErrorMsg, null, (int)HttpCodeState.error);
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
            task = System.Threading.Tasks.Task.Run((System.Action)ReadNetBytes);
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

            task = null;
        }

        void SendRequest()
        {
            statusCode = (int)HttpCodeState.error;
            try
            {
                int treTryCount = 3;
                while (treTryCount-- > 0)
                {
                    try
                    {
                        SendProcess();
                        break;
                    }
                    catch (Exception e)
                    {
                        if(treTryCount <= 0)
                        {
                            throw e;
                        }
                    }
                    Thread.Sleep(20);
                }
            }
            catch (Exception e)
            {
                ErrorMsg = e.ToString();
                DLog.LogError(e);
            }

            OnTaskDone();
        }

        void SendProcess()
        {
            statusCode = (int)HttpCodeState.error;

            var tmethod = new HttpMethod(methodType.ToString());
            requestMsg = new HttpRequestMessage(tmethod, Url);
            requestMsg.Version = httpManager.defaultHttpVersion;

            CheckRequest();
            CheckHeader();

            var tsendTask = httpClient.SendAsync(requestMsg);
            var response = tsendTask.Result;

            statusCode = (int)response.StatusCode;

            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                var tcache = HttpCacheManager.Instance.GetCache(Url);
                responseString = tcache?.responseData;
            }
            else
            {
                var treadTask = response.Content.ReadAsStringAsync();
                responseString = treadTask.Result;
            }

            CheckCache(response);
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

        void OnHttpError(string pMsg, string response, int pState)
        {
            try
            {
                string tmsg = "{" + $"\"response\":\"{response}\",\"Error\":\"{pMsg}\"" + "}";
                onError?.Invoke(pState, tmsg, Url);
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
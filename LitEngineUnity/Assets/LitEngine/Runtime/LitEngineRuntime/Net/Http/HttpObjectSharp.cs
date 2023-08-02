using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.ConstrainedExecution;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LitEngine.Tool;
using UnityEngine;

namespace LitEngine.Net.Http
{
    public class HttpCSharpBase : HttpObject
    {

    }
    public class HttpClientObject : HttpCSharpBase
    {
        public static HttpClient httpClient { get; private set; }

        static void InitHttpClient()
        {
            if (httpClient != null) return;
            
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            var cache = HttpCacheManager.Instance;
            
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = false,
            };
            
            
            httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Add("Connection","Keep-Alive");
            //httpClient.DefaultRequestHeaders.Add("Keep-Alive","3600");
            
            //var tcache = new CacheControlHeaderValue();
            //tcache.NoCache = false;
            
            //httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue();
        }

        public HttpClientObject()
        {
            if (httpClient == null)
            {
                InitHttpClient();
            }
        }

        public static void ClearHttpRequest()
        {
            httpClient?.CancelPendingRequests();
            httpClient?.Dispose();
            httpClient = null;
        }
    }
    
    public enum HttpCodeState
    {
        jsonError = 2,
        Finished = 3,
        error = 4,
        TimedOut = 7,
    }

    public class HttpObjectSharp<TResponse> : HttpClientObject
    {
        public event HttpErorEvent onError;
        public event HttpFinishEvent<TResponse> onFinish;
        public Task task { get; private set; } = null;
        

        private HttpRequestMessage requestMsg;
        private HttpResponseMessage httpResponse;
        private string reqJson = null;

        public HttpObjectSharp(string pUrl, object data, HTTPMethodType pType, int pTimeOut, bool needCrypto) : base()
        {
            Url = pUrl;
            requestData = data;
            useCrypto = needCrypto;

            methodType = pType;
            timeOut = pTimeOut;
        }

        override protected void DisposeNoGcCode()
        {
            onFinish = null;
            onError = null;
            Abort();
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
                DLog.LogErrorFormat(httpManager.Tag, "[URL] = {0},[Error] = {1}", Url, erro.Message);
            }
        }

        private void CloseHttpClient()
        {
            
        }

        protected override void UpdateSending()
        {
   
        }

        override protected void UpdateFinish()
        {
            OnFinshed();
            state = HttpState.done;

            long tdalyTime = (System.DateTime.Now.Ticks - ticks) / 10000;
            DLog.Log(httpManager.Tag,
                $"[HTTPResponse]:Url = {Url},delayTime = {tdalyTime},requestCode = {requestCode}, StatusCode = {statusCode} Json = {responseString}");
            
            try
            {
                if (statusCode == (int)HttpStatusCode.OK || statusCode == (int)HttpStatusCode.NotModified)
                {
                    string tresData = null;
                    if (!useCrypto)
                    {
                        tresData = responseString;
                    }
                    else
                    {
                        var tbytes = httpManager.DeCryptData(Encoding.UTF8.GetBytes(responseString));
                        tresData = Encoding.UTF8.GetString(tbytes);
                    }

                    TResponse ret = DataConvert.FromJson<TResponse>(tresData);
                    responseObject = ret;
                    if (ret != null)
                    {
                        OnHttpFinish(ret);
                    }
                    else
                    {
                        OnHttpError(
                            string.Format("DeserializeObject fail.response is null.Type={0}, Data={1}",
                                typeof(TResponse), responseString), (int) HttpCodeState.jsonError,null);
                    }
                }
                else
                {
                    OnHttpError(ErrorMsg, statusCode, responseString);
                }
            }
            catch (Exception e)
            {
                DLog.LogError(httpManager.Tag, e);
                ErrorMsg = e.Message;
                OnHttpError(ErrorMsg, (int)HttpCodeState.error,null);
            }
        }

        override protected void WaitSendUpdate()
        {
            bool isSuccess = false;
            try
            {
                state = HttpState.sending;

                if (requestData != null)
                {
                    if (!useCrypto)
                    {
                        reqJson = DataConvert.ToJson(requestData);
                    }
                    else
                    {
                        reqJson = DataConvert.ToJson(requestData);
                        var tbytes = httpManager.EnCryptData(Encoding.UTF8.GetBytes(reqJson));
                        reqJson = Encoding.UTF8.GetString(tbytes);
                    }
                }

                ticks = System.DateTime.Now.Ticks;
                SendAsync();

                DLog.Log(httpManager.Tag, $"[HTTPRequest]: URL = {Url}, Method = {methodType}, body = {reqJson}");
                isSuccess = true;
            }
            catch (System.Exception erro)
            {
                OnHttpError(erro.ToString(), (int) HttpCodeState.error, null);
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
            task = System.Threading.Tasks.Task.Run((System.Action) ReadNetBytes);
        }

        void ReadNetBytes()
        {
            try
            {
               SendRequest();
            }
            catch (System.Exception ex)
            {
                statusCode = (int) HttpCodeState.error;
                ErrorMsg = ex.ToString();
                OnTaskDone();
            }

            task = null;
        }

        void SendRequest()
        {
            
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
                        if (!IsReTry || treTryCount <= 0)
                        {
                            throw e;
                        }
                    }
                    Thread.Sleep(200);
                }

            }
            catch (Exception e)
            {
                ErrorMsg = e.ToString();
                DLog.LogError(httpManager.Tag,"SendRequest Error = " + e);
            }
            
            OnTaskDone();
        }
        
        void SendProcess()
        {
            statusCode = (int) HttpCodeState.error;
            requestCode = (int) HttpCodeState.error;
            exceptionStatus = (int)WebExceptionStatus.Success;

            var tmethod = new HttpMethod(methodType.ToString());
            requestMsg = new HttpRequestMessage(tmethod, Url);
            requestMsg.Version = httpManager.defaultHttpVersion;

            CheckRequest();
            CheckHeader();

            HttpResponseMessage response = null;
            string texception = null;
            try
            {
                var tsendTask = httpClient.SendAsync(requestMsg);
                response = tsendTask.Result;
            }
            catch (Exception e)
            {
                if (e.InnerException is HttpRequestException thttpReq
                    && thttpReq.InnerException is WebException twebexp)
                {
                    exceptionStatus = (int)twebexp.Status;
                    switch (twebexp.Status)
                    {
                        case WebExceptionStatus.Timeout:
                            requestCode = (int) HttpCodeState.TimedOut;
                            break;
                    }
                }
                texception = e.ToString();
            }

            if (response == null)
            {
                throw new NullReferenceException($"Get Response error = {texception}");
            }
            
            requestCode = (int) HttpCodeState.Finished;
            statusCode = (int) response.StatusCode;

            httpResponse = response;
                
            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                var tcache = HttpCacheManager.Instance.GetCache(Url);
                responseString = tcache?.responseData;
            }
            else
            {
                var tresponsestr = response.Content.ReadAsStringAsync().Result;

                responseString = tresponsestr;
            }

            CheckCache(response);
        }

        void CheckRequest()
        {
            if (methodType == HTTPMethodType.GET)
            {
                var tcache = HttpCacheManager.Instance.GetCache(Url);
                if (tcache != null && !string.IsNullOrEmpty(tcache.responseData))
                {
                    if (!string.IsNullOrEmpty(tcache.ETag))
                        SetHeader("If-None-Match", tcache.ETag);
                    if (!string.IsNullOrEmpty(tcache.LastModified))
                        SetHeader("If-Modified-Since", tcache.LastModified);
                }
                
                if (!string.IsNullOrEmpty(reqJson))
                {
                    SetHeader("DataJson", reqJson);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(reqJson))
                {
                    if (!useCrypto)
                    {
                        requestMsg.Content = new StringContent(reqJson, Encoding.UTF8, "application/json");
                    }
                    else
                    {
                        requestMsg.Content = new StringContent(reqJson, Encoding.UTF8, "application/octet-stream");
                    }
                }
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
                    tcache.ETag = tEtag != null? tEtag : tcache.ETag;

                    HttpCacheManager.Instance.AddSave(tcache);
                }
            }
        }


        void AddCache(string pETag, string pTime)
        {
            if (!string.IsNullOrEmpty(pETag))
            {
                var tcache = new HttpCacheObject(Url,true);
                tcache.responseData = responseString;
                tcache.LastModified = pTime;
                tcache.ETag = pETag;

                HttpCacheManager.Instance.AddCache(tcache);
            }
        }

        void OnTaskDone()
        {
            state = HttpState.finish;
        }
        
        override public string GetResponseHeader(string pKey)
        {
            if (httpResponse == null) return null;
            if (httpResponse.Headers.TryGetValues(pKey, out IEnumerable<string> values))
            {
                if (values == null) return null;
                List<string> tlist = new List<string>(values);
                if (tlist.Count == 0) return null;
                return tlist[0];
            }
            return null;
        }

        void OnHttpError(string pMsg, int pState, string jsonString)
        {
            try
            {
                Error = new HttpErrorObject()
                {
                    errorStatus = pState,
                    requestCode = requestCode,
                    statusCode = statusCode,
                    errorMessage = pMsg,
                    responseJson = jsonString,
                    exceptionStatus = exceptionStatus,
                };

                ErrorMsg = Error.ToString();
                onError?.Invoke(pState, ErrorMsg, Url, this);
            }
            catch (System.Exception erro)
            {
                DLog.LogErrorFormat(httpManager.Tag, "onError called error : {0}, {1}", Url, erro);
            }
        }

        void OnHttpFinish(TResponse responseData)
        {
            try
            {
                onFinish?.Invoke(responseData, this);
            }
            catch (System.Exception erro)
            {
                DLog.LogErrorFormat(httpManager.Tag, "response called error : {0}, {1}", Url, erro);
            }
        }

        override protected void UpdatePamarsInvailid()
        {
            base.UpdatePamarsInvailid();

            try
            {
                Error = new HttpErrorObject()
                {
                    errorStatus = (int) HttpCodeState.error,
                    errorMessage = ErrorMsg,
                    requestCode = (int) HttpCodeState.error,
                    responseCode = (int) HttpCodeState.error,
                    statusCode = (int) HttpCodeState.error,
                };
                ErrorMsg = Error.ToString();
                DLog.Log(httpManager.Tag, $"[HTTPResponse]:URL = {Url}, error = {ErrorMsg}");
                onError?.Invoke((int) HttpCodeState.error, ErrorMsg, Url, this);
            }
            catch (Exception ex)
            {
                DLog.LogErrorFormat(httpManager.Tag, "Type:{0}, error:{1}", this.GetType().Name, ex);
            }
        }
    }
}
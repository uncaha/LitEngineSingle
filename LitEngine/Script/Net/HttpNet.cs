using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System;

namespace LitEngine.Net
{
    public class HttpNet : MonoBehaviour
    {
        public enum HttpMethod
        {
            none = 0,
            POST,
            GET,
            PUT,
            DELETE,
        }
        public delegate void HttpObjectCompleteEvent(int code, string responseData, string error);
        public class HttpObject : IDisposable
        {
            public enum State
            {
                normal = 0,
                sending,
                disposed,
            }

            public event HttpObjectCompleteEvent OnComplete = null;

            public string Url { get; private set; }
            public int timeOut { get; private set; } = 60;
            public HttpMethod methodType { get; private set; } = HttpMethod.POST;
            public byte[] sendData { get; private set; } = null;

            public bool IsDone { get; private set; } = false;
            public string ResponseData { get; private set; } = null;
            public string Error { get; private set; } = null;
            public int statusCode { get; private set; } = -1;
            public bool IsStart { get; private set; } = false;

            public Task task { get; private set; } = null;
            public State state { get; private set; } = State.normal;

            private HttpWebRequest httpRequest;
            private HttpWebResponse webResponse;
            private Stream httpResponseStream;
            public HttpObject(string pUrl, HttpMethod pMethod, byte[] pData, int pTimeOut)
            {
                Url = pUrl;
                sendData = pData;
                methodType = pMethod;
                timeOut = pTimeOut;

                headers.Add("Content-Type", "application/json");
            }

            bool mDisposed = false;
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected void Dispose(bool _disposing)
            {
                if (mDisposed)
                    return;
                CloseHttpClient();
                task = null;
                state = State.disposed;
                mDisposed = true;
            }

            ~HttpObject()
            {
                Dispose(false);
            }

            private void CloseHttpClient()
            {
                try
                {
                    if (httpResponseStream != null)
                    {
                        httpResponseStream.Close();
                        httpResponseStream = null;
                    }

                    if (webResponse != null)
                    {
                        webResponse.Close();
                        webResponse = null;
                    }

                    if (httpRequest != null)
                    {
                        httpRequest.Abort();
                        httpRequest = null;
                    }
                }
                catch (System.Exception erro)
                {
                    Debug.LogError(string.Format("[URL] = {0},[Error] = {1}", Url, erro.Message));
                }
            }

            Dictionary<string, string> headers = new Dictionary<string, string>();
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

            public void SetHeader(Dictionary<string, string> pValues)
            {
                if (pValues == null) return;
                headers = pValues;
            }

            public void Abort()
            {
                CloseHttpClient();
            }

            public void SendAsync()
            {
                if (state != State.normal) return;
                state = State.sending;
                task = Task.Run(ReadNetBytes);

                Instance.httpObjects.Add(this);
            }

            public void Send()
            {
                if (state != State.normal) return;
                state = State.sending;
                ReadNetBytes();
                CloseHttpClient();
            }


            void ReadNetBytes()
            {
                try
                {
                    statusCode = -1;
                    httpRequest = HttpWebRequest.CreateHttp(Url);
                    httpRequest.Timeout = timeOut;
                    httpRequest.KeepAlive = true;

                    CheckHeader();
                    CheckRequestData();
                    CheckRespose();
                }
                catch (System.Exception ex)
                {
                    Error = ex.ToString();
                }

                IsDone = true;
            }

            void CheckRequestData()
            {
                if (sendData != null)
                {
                    httpRequest.ContentLength = sendData.Length;

                    Stream stream = httpRequest.GetRequestStream();
                    stream.Write(sendData, 0, sendData.Length);
                    stream.Close();
                }
            }

            void CheckHeader()
            {
                httpRequest.Method = methodType.ToString();
                httpRequest.Headers.Clear();
                foreach (var item in headers)
                {
                    switch (item.Key)
                    {
                        case "Content-Type":
                            httpRequest.ContentType = item.Value;
                            break;
                        default:
                            httpRequest.Headers.Add(item.Key, item.Value);
                            break;
                    }
                }

            }

            void CheckRespose()
            {
                try
                {
                    try
                    {
                        webResponse = (HttpWebResponse)httpRequest.GetResponse();
                    }
                    catch (System.Exception _error)
                    {
                        Error = _error.Message;
                    }

                    if (webResponse == null || Error != null)
                    {
                        string terro = string.Format("ReadNetByte Get Response fail.[Error] = {0}", Error);
                        throw new System.NullReferenceException(terro);
                    }


                    if (webResponse.StatusCode == HttpStatusCode.OK)
                    {
                        httpResponseStream = webResponse.GetResponseStream();

                        using (StreamReader reader = new StreamReader(httpResponseStream))
                        {
                            ResponseData = reader.ReadToEnd();
                        }
                        statusCode = 0;
                    }
                    else
                    {
                        statusCode = (int)webResponse.StatusCode;
                        Error = string.Format("Http statusCode = {0}", webResponse.StatusCode);
                    }
                }
                catch (System.Exception ex)
                {
                    Error = ex.Message;
                }

            }

            public void CallEvent()
            {
                try
                {
                    OnComplete?.Invoke(statusCode, ResponseData, Error);
                }
                catch (Exception ex)
                {
                    DLog.LogErrorFormat("URL = {0},error = {1}", Url, ex);
                }
            }

        }
        static private HttpNet sInstance = null;
        static private HttpNet Instance
        {
            get
            {
                if (sInstance == null)
                {
                    GameObject tobj = new GameObject();
                    DontDestroyOnLoad(tobj);
                    sInstance = tobj.AddComponent<HttpNet>();
                    tobj.name = "Http" + "-Object";
                }
                return sInstance;
            }
        }

        List<HttpObject> httpObjects = new List<HttpObject>();

        public HttpNet()
        {
            //headers.Add("Content-Type", "application/json");
        }

        private void Update()
        {
            if (httpObjects.Count == 0) return;

            for (int i = httpObjects.Count - 1; i >= 0; i--)
            {
                var item = httpObjects[i];
                if (item.IsDone)
                {
                    httpObjects.RemoveAt(i);
                    item.CallEvent();
                    item.Dispose();
                }
            }
        }

        public static HttpObject SendDelete(string pUrl, HttpObjectCompleteEvent pOnComplete, int pTimeOut = 60, Dictionary<string, string> pHeaders = null)
        {
            HttpObject ret = GetHttpObject(pUrl, HttpMethod.DELETE, null, pOnComplete, pTimeOut, pHeaders);

            ret.SendAsync();

            return ret;
        }

        public static HttpObject SendPut(string pUrl, byte[] pPostData, HttpObjectCompleteEvent pOnComplete, int pTimeOut = 60, Dictionary<string, string> pHeaders = null)
        {
            HttpObject ret = GetHttpObject(pUrl, HttpMethod.PUT, pPostData, pOnComplete, pTimeOut, pHeaders);

            ret.SendAsync();

            return ret;
        }

        public static HttpObject SendPost(string pUrl, byte[] pPostData, HttpObjectCompleteEvent pOnComplete, int pTimeOut = 60, Dictionary<string, string> pHeaders = null)
        {
            HttpObject ret = GetHttpObject(pUrl, HttpMethod.POST, pPostData, pOnComplete, pTimeOut, pHeaders);

            ret.SendAsync();

            return ret;
        }

        public static HttpObject SendGet(string pUrl, HttpObjectCompleteEvent pOnComplete, int pTimeOut = 60, Dictionary<string, string> pHeaders = null)
        {
            HttpObject ret = GetHttpObject(pUrl, HttpMethod.GET, null, pOnComplete, pTimeOut, pHeaders);

            ret.SendAsync();

            return ret;
        }

        public static HttpObject GetHttpObject(string pUrl, HttpMethod httpType, byte[] pPostData, HttpObjectCompleteEvent pOnComplete, int pTimeOut = 60, Dictionary<string, string> pHeaders = null)
        {
            HttpObject obj = new HttpObject(pUrl, httpType, pPostData, pTimeOut);

            if (pOnComplete != null)
            {
                obj.OnComplete += pOnComplete;
            }

            if (pHeaders != null)
            {
                obj.SetHeader(pHeaders);
            }

            return obj;
        }

    }
}

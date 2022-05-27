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
    public delegate void HttpResponseEvent<TResponse>(TResponse response, string errorMsg, int errorCode = 0);

    public interface IHttpManager
    {
        void AddSlowHttpObject(HttpObject pObj);
        void OnHttpStartSend(HttpObject pObj);


        void OnHttpFinished(HttpObject pObj);

        void Add(HttpObject pObj);

        void Remove(int pKey);

    }


    public enum HTTPMethodType : byte
    {
        none = 0,
        GET,
        POST,
        PUT,
        DELETE,
        HEAD,
        PATCH,
        OPTIONS,
    }


    public class HttpNet : MonoBehaviour, IHttpManager
    {
        public long slowTimeBoundaries = -1;
        public event System.Action<int> OnHttpTimeTooLong;

        public event System.Action<HttpObject> OnHttpStartEvent;
        public event System.Action<HttpObject> OnHttpFinishedEvent;

        public Dictionary<string, string> publicHeaders = new Dictionary<string, string>();

        private static HttpNet sInstance = null;
        public static HttpNet Instance
        {
            get
            {
                if (sInstance == null)
                {
                    GameObject tobj = new GameObject(typeof(HttpNet).Name);
                    GameObject.DontDestroyOnLoad(tobj);
                    sInstance = tobj.AddComponent<HttpNet>();
                }
                return sInstance;
            }
        }

        public static void UnInit()
        {
            if (sInstance == null) return;
            Destroy(sInstance);
            sInstance = null;
        }

        public static string EscapeURL(string pUrl)
        {
            return UnityEngine.Networking.UnityWebRequest.EscapeURL(pUrl);
        }

        public static string UnEscapeURL(string pUrl)
        {
            return UnityEngine.Networking.UnityWebRequest.UnEscapeURL(pUrl);
        }

        protected Dictionary<int, HttpObject> httpObjectMap = new Dictionary<int, HttpObject>();
        protected Dictionary<int, HttpObject> slowHttpObjectMap = new Dictionary<int, HttpObject>();
        protected List<HttpObject> updateList = new List<HttpObject>(100);

        protected void OnDestroy()
        {
            sInstance = null;
        }

        protected void Update()
        {
            try
            {
                if (httpObjectMap.Count == 0) return;

                updateList.Clear();
                updateList.AddRange(httpObjectMap.Values);

                for (int i = 0, length = updateList.Count; i < length; i++)
                {
                    updateList[i].Update();
                }
            }
            catch (Exception e)
            {
                DLog.LogError(e);
            }

        }

        void OnSlowHttpChanged()
        {
            try
            {
                OnHttpTimeTooLong?.Invoke(slowHttpObjectMap.Count);
            }
            catch (System.Exception err)
            {
                DLog.LogError(err);
            }
        }

        public void ClearAllHttpRequest()
        {
            if (httpObjectMap.Count == 0) return;

            var tmap = httpObjectMap;
            List<HttpObject> tlist = new List<HttpObject>(tmap.Values);
            tmap.Clear();
            for (int i = 0, length = tlist.Count; i < length; i++)
            {
                var item = tlist[i];
                item.Dispose();
            }

            HttpCSharpBase.httpClient?.CancelPendingRequests();
        }


        #region send
        public HttpObject GetRequestObject(string url, HTTPMethodType pType, string requestData, HttpFinishEvent<string> onFinish, HttpErorEvent onError = null, int pTimeOut = 60)
        {

            var thttpobj = new HttpObjectSharp(url, requestData, pType, pTimeOut);

            thttpobj.httpManager = this;
            thttpobj.slowTimeBoundaries = slowTimeBoundaries;
            thttpobj.onFinish += onFinish;
            thttpobj.onError += onError;
            return thttpobj;
        }

        protected void CallOnComplete(string pKey, HttpResponseEvent<string> pOnComplete, string pUrl, object requestData, string response)
        {
            try
            {
                pOnComplete?.Invoke(response, null, 0);
            }
            catch (Exception ex)
            {
                DLog.LogErrorFormat("CallOnComplete:Fun = {0}, Url = {1},requestData = {2}, response = {3}", pKey, pUrl, requestData, response);
                DLog.LogErrorFormat("CallOnComplete:{0}", ex);
            }
        }

        protected void CallError(string pKey, HttpResponseEvent<string> pOnComplete, string pUrl, int pCode, string pErrMsg)
        {
            try
            {
                DLog.LogErrorFormat("[{0}]:URL={1},statuCode = {2}, error={3},", pKey, pUrl, pCode, pErrMsg);
                pOnComplete?.Invoke(null, pErrMsg, pCode);
            }
            catch (Exception ex)
            {
                DLog.LogErrorFormat("[CallError]:Fun = {0} Url = {1},Code = {2}, ErrorMsg = {3}, error = {4}", pKey, pUrl, pCode, pErrMsg, ex.ToString());
            }
        }

        protected HttpObject StartRequest(string pUrl, HTTPMethodType pType, string requestData,
            HttpResponseEvent<string> pOnComplete)
        {
            var httpObject = GetRequestObject(pUrl, pType, requestData,
                (response) => { CallOnComplete("StartRequestSend+" + pType, pOnComplete, pUrl, requestData, response); },
                (statucode, msg, url) =>
                {
                    CallError("StartRequestSend+" + pType, pOnComplete, pUrl, 10000000 + statucode, msg);
                }, 60);

            InitHeaderData(httpObject);
            httpObject.StartSend();

            return httpObject;
        }

        public HttpObject StartPost(string pUrl, string requestData, HttpResponseEvent<string> pOnComplete)
        {
            return StartRequest(pUrl, HTTPMethodType.POST, requestData, pOnComplete);
        }

        public HttpObject StartPutSend(string pUrl, string requestData, HttpResponseEvent<string> pOnComplete)
        {
            return StartRequest(pUrl, HTTPMethodType.PUT, requestData, pOnComplete);
        }

        public HttpObject StartDeleteSend(string pUrl, string requestData, HttpResponseEvent<string> pOnComplete)
        {
            return StartRequest(pUrl, HTTPMethodType.DELETE, requestData, pOnComplete);
        }

        public HttpObject StartGetResponse(string pUrl, string requestData, HttpResponseEvent<string> pOnComplete)
        {
            return StartRequest(pUrl, HTTPMethodType.GET, requestData, pOnComplete);
        }

        public HttpObject StartPatchSend(string pUrl, string requestData, HttpResponseEvent<string> pOnComplete)
        {
            return StartRequest(pUrl, HTTPMethodType.PATCH, requestData, pOnComplete);
        }

        protected void InitHeaderData(HttpObject httpObject)
        {
            foreach (var item in publicHeaders)
            {
                httpObject.SetHeader(item.Key, item.Value);
            }
        }

        public void SetPublicHeader(string key, string v)
        {
            if (publicHeaders.ContainsKey(key))
            {
                publicHeaders[key] = v;
            }
            else
            {
                publicHeaders.Add(key, v);
            }
        }
        #endregion


        #region interface
        void IHttpManager.AddSlowHttpObject(HttpObject pObj)
        {
            if (slowHttpObjectMap.ContainsKey(pObj.Key)) return;
            slowHttpObjectMap.Add(pObj.Key, pObj);
            OnSlowHttpChanged();
        }

        void IHttpManager.OnHttpStartSend(HttpObject pObj)
        {
            try
            {
                OnHttpStartEvent?.Invoke(pObj);
            }
            catch (System.Exception err)
            {
                DLog.LogError(err);
            }
        }

        void IHttpManager.OnHttpFinished(HttpObject pObj)
        {
            try
            {
                OnHttpFinishedEvent?.Invoke(pObj);
            }
            catch (System.Exception err)
            {
                DLog.LogError(err);
            }
        }

        void IHttpManager.Add(HttpObject pObj)
        {
            if (!httpObjectMap.ContainsKey(pObj.Key))
            {
                httpObjectMap.Add(pObj.Key, pObj);
            }
            else
            {
                DLog.LogError(pObj.Key);
            }
        }

        void IHttpManager.Remove(int pKey)
        {
            if (httpObjectMap.ContainsKey(pKey))
            {
                httpObjectMap.Remove(pKey);
            }

            if (slowHttpObjectMap.ContainsKey(pKey))
            {
                slowHttpObjectMap.Remove(pKey);
                OnSlowHttpChanged();
            }
        }
        #endregion


    }

}

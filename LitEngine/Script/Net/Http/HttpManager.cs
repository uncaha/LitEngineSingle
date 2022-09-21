using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

using System.Reflection.Emit;
using System.Security.Policy;
using LitEngine.Event;
using LitEngine.Tool;
using Object = System.Object;


namespace LitEngine.Net.Http
{
    public delegate void HttpResponseEvent<TResponse>(TResponse response, string errorMsg, int errorCode);

    public interface IHttpManager
    {
        string Tag { get; }
        Version defaultHttpVersion { get; }
        byte[] EnCryptData(byte[] sor);
        byte[] DeCryptData(byte[] sor);
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

    public abstract class HttpManagerBase : MonoBehaviour
    {
        #region Specifies

        /// <summary>
        /// Specifies httpManager encryption,if set
        /// </summary>
        public Func<byte[], byte[]> EncryptContentDelgate;

        /// <summary>
        /// Specifies httpManager decrypts, if set
        /// </summary>
        public Func<byte[], byte[]> DecryptContentDelgate;

        #endregion

        static protected ConcurrentDictionary<string, HttpManagerBase> httpManagerMap =
            new ConcurrentDictionary<string, HttpManagerBase>();

        static HttpManagerBase()
        {
        }

        public static string EscapeURL(string pUrl)
        {
            return UnityEngine.Networking.UnityWebRequest.EscapeURL(pUrl);
        }

        public static string UnEscapeURL(string pUrl)
        {
            return UnityEngine.Networking.UnityWebRequest.UnEscapeURL(pUrl);
        }

        public static bool IsParamsValid(params string[] p)
        {
            if (p == null) return false;
            for (int i = 0, max = p.Length; i < max; i++)
            {
                if (string.IsNullOrEmpty(p[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsTrackRequest = false;

        virtual public void Init()
        {
        }
    }


    public class HttpManager<T> : HttpManagerBase, IHttpManager where T : HttpManagerBase
    {
        public string Tag { get; protected set; } = "Http";

        public bool UseCrypto = false;
        public long slowTimeBoundaries = -1;
        public event System.Action<int> OnHttpTimeTooLong;

        public event System.Action<HttpObject> OnHttpStartEvent;
        public event System.Action<HttpObject> OnHttpFinishedEvent;

        public Version defaultHttpVersion { get; set; } = new Version(1, 1);
        public Dictionary<string, string> publicHeaders { get; private set; } = new Dictionary<string, string>();

        public Dictionary<string, object> customHeaders { get; private set; } = new Dictionary<string, object>();

        private static T sInstance = null;

        public static T Instance
        {
            get
            {
                if (sInstance == null)
                {
                    GameObject tobj = new GameObject(typeof(T).Name);
                    GameObject.DontDestroyOnLoad(tobj);
                    tobj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                    sInstance = tobj.AddComponent<T>();
                    sInstance.Init();
                }

                return sInstance;
            }
        }


        protected Dictionary<int, HttpObject> httpObjectMap = new Dictionary<int, HttpObject>();
        protected Dictionary<int, HttpObject> slowHttpObjectMap = new Dictionary<int, HttpObject>();
        protected List<HttpObject> updateList = new List<HttpObject>(100);

        private bool isInited = false;

        override public void Init()
        {
            if (isInited) return;
            isInited = true;

            InitClientData();
            AddCustomHeader("ClientData", customClientData);

            DLog.Log(Tag, $"Type = {typeof(T).Name} Inited.");
        }

        #region clientdata

        void InitClientData()
        {
            var tfields = new Dictionary<string, object>()
            {
                {"deviceId", SystemInfo.deviceUniqueIdentifier},
                {"appVersion", Application.version},
                {"osVersion", SystemInfo.operatingSystem},
                {"systemLanguage", Application.systemLanguage.ToString()},
                {"appBundle", Application.identifier},
                {"deviceModel", SystemInfo.deviceModel},
                {"systemMemorySize", SystemInfo.systemMemorySize},
            };

            SetClientFields(tfields);
        }

        Dictionary<string, object> customClientData = new Dictionary<string, object>();

        public void SetClientFields(Dictionary<string, object> pFields)
        {
            if (pFields == null) return;
            foreach (var item in pFields)
            {
                if (customClientData.ContainsKey(item.Key))
                {
                    customClientData[item.Key] = item.Value;
                }
                else
                {
                    customClientData.Add(item.Key, item.Value);
                }
            }
        }

        public void SetClientField(string pkey, object pField)
        {
            if (pField == null) return;
            if (customClientData.ContainsKey(pkey))
            {
                customClientData[pkey] = pField;
            }
            else
            {
                customClientData.Add(pkey, pField);
            }
        }

        #endregion

        private void Update()
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
                DLog.LogError(Tag, e);
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
                DLog.LogError(Tag, err);
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
        }


        #region send

        public HttpObject GetRequestObject<TResponse>(string url, HTTPMethodType pType, object requestData,
            HttpFinishEvent<TResponse> onFinish, HttpErorEvent onError, int pTimeOut, bool isPamarsVaild)
        {
            if (isPamarsVaild)
            {
                var tparmhttp = new HttpPamarsInvalid<TResponse>(url);
                tparmhttp.onError += onError;
                tparmhttp.httpManager = this;
                return tparmhttp;
            }


#if USE_BESTHTTP
            var thttpobj = new BestHttpObject<TResponse>(url, requestData, pType, pTimeOut, UseCrypto);
#else
            var thttpobj = new HttpObjectSharp<TResponse>(url, requestData, pType, pTimeOut, UseCrypto);
#endif


            thttpobj.httpManager = this;
            thttpobj.slowTimeBoundaries = slowTimeBoundaries;
            thttpobj.onFinish += onFinish;
            thttpobj.onError += onError;
            return thttpobj;
        }

        virtual protected void CallOnComplete<TResponse>(string pKey, HttpResponseEvent<TResponse> pOnComplete,
            string pUrl,
            object requestData, TResponse response, HttpObject pHttpObject)
        {
            try
            {
                pOnComplete?.Invoke(response, pHttpObject.ErrorMsg, 0);
            }
            catch (Exception ex)
            {
                DLog.LogErrorFormat(Tag, "CallOnComplete:Fun = {0}, Url = {1},requestData = {2}, response = {3}",
                    pKey, pUrl, DataConvert.ToJson(requestData), DataConvert.ToJson(response));
                DLog.LogErrorFormat(Tag, "CallOnComplete:{0}", ex);
            }
        }

        virtual protected void CallError<TResponse>(string pKey, HttpResponseEvent<TResponse> pOnComplete, string pUrl,
            int pCode, string pErrMsg, HttpObject pHttpObject)
        {
            TResponse response = default(TResponse);
            try
            {
                response = (TResponse) pHttpObject.responseObject;
            }
            catch
            {
                // ignored
            }

            try
            {
                pOnComplete?.Invoke(response, pErrMsg, pCode);
            }
            catch (Exception ex)
            {
                DLog.LogErrorFormat(Tag, "[CallError]:Fun = {0} Url = {1},Code = {2}, ErrorMsg = {3}, error = {4}",
                    pKey, pUrl, pCode, pErrMsg, ex.ToString());
            }
        }

        virtual protected HttpObject CreatRequest<TResponse>(string pUrl, HTTPMethodType pType, object requestData,
            HttpResponseEvent<TResponse> pOnComplete, int pTimeOut, bool isPamarsVaild)
        {
            var httpObject = GetRequestObject<TResponse>(pUrl, pType, requestData,
                (response, sender) =>
                {
                    CallOnComplete("StartRequestSend+" + pType, pOnComplete, pUrl, requestData, response, sender);
                },
                (statucode, msg, url, sender) =>
                {
                    CallError("StartRequestSend+" + pType, pOnComplete, pUrl, statucode, msg, sender);
                },
                pTimeOut, isPamarsVaild);


            return httpObject;
        }

        virtual protected HttpObject StartRequest<TResponse>(string pUrl, HTTPMethodType pType, object requestData,
            HttpResponseEvent<TResponse> pOnComplete, int pTimeOut, bool isPamarsVaild)
        {
            var httpObject = CreatRequest(pUrl, pType, requestData, pOnComplete, pTimeOut, isPamarsVaild);
            InitHeaderData(httpObject);
            httpObject.StartSend();
            return httpObject;
        }

        virtual public HttpObject StartPost<TResponse>(string pUrl, object requestData,
            HttpResponseEvent<TResponse> pOnComplete, int pTimeOut = 60, bool isPamarsVaild = false)
        {
            return StartRequest(pUrl, HTTPMethodType.POST, requestData, pOnComplete, pTimeOut, isPamarsVaild);
        }

        virtual public HttpObject StartPutSend<TResponse>(string pUrl, object requestData,
            HttpResponseEvent<TResponse> pOnComplete, int pTimeOut = 60, bool isPamarsVaild = false)
        {
            return StartRequest(pUrl, HTTPMethodType.PUT, requestData, pOnComplete, pTimeOut, isPamarsVaild);
        }

        virtual public HttpObject StartDeleteSend<TResponse>(string pUrl, object requestData,
            HttpResponseEvent<TResponse> pOnComplete, int pTimeOut = 60, bool isPamarsVaild = false)
        {
            return StartRequest(pUrl, HTTPMethodType.DELETE, requestData, pOnComplete, pTimeOut, isPamarsVaild);
        }

        virtual public HttpObject StartGetResponse<TResponse>(string pUrl, object requestData,
            HttpResponseEvent<TResponse> pOnComplete, int pTimeOut = 60, bool isPamarsVaild = false)
        {
            return StartRequest(pUrl, HTTPMethodType.GET, requestData, pOnComplete, pTimeOut, isPamarsVaild);
        }

        virtual public HttpObject StartPatchSend<TResponse>(string pUrl, object requestData,
            HttpResponseEvent<TResponse> pOnComplete, int pTimeOut = 60, bool isPamarsVaild = false)
        {
            return StartRequest(pUrl, HTTPMethodType.PATCH, requestData, pOnComplete, pTimeOut, isPamarsVaild);
        }

        protected void InitHeaderData(HttpObject httpObject)
        {
            foreach (var item in publicHeaders)
            {
                httpObject.SetHeader(item.Key, item.Value);
            }

            foreach (var item in customHeaders)
            {
                httpObject.SetHeader(item.Key, DataConvert.ToJson(item.Value));
            }
        }

        public void AddCustomHeader(string key, object pObj)
        {
            if (pObj == null || string.IsNullOrEmpty(key)) return;
            if (customHeaders.ContainsKey(key))
            {
                customHeaders[key] = pObj;
            }
            else
            {
                customHeaders.Add(key, pObj);
            }
        }

        public void SetPublicHeader(string key, string v)
        {
            if (string.IsNullOrEmpty(v) || string.IsNullOrEmpty(key)) return;
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

        byte[] IHttpManager.EnCryptData(byte[] sor)
        {
            try
            {
                if (EncryptContentDelgate != null)
                {
                    return EncryptContentDelgate(sor);
                }
            }
            catch (Exception err)
            {
                DLog.LogError(Tag, err);
            }

            return sor;
        }

        byte[] IHttpManager.DeCryptData(byte[] sor)
        {
            try
            {
                if (DecryptContentDelgate != null)
                {
                    return DecryptContentDelgate(sor);
                }
            }
            catch (Exception err)
            {
                DLog.LogError(Tag, err);
            }

            return sor;
        }

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
                DLog.LogError(Tag, err);
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
                DLog.LogError(Tag, err);
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
                DLog.LogError(Tag, pObj.Key);
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
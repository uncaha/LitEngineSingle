using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
namespace LitEngine.Net
{
    public sealed class UnityHttp : MonoBehaviour
    {
        
        static private UnityHttp sInstance = null;
        static private UnityHttp Instance
        {
            get
            {
                if (sInstance == null)
                {
                    GameObject tobj = new GameObject();
                    DontDestroyOnLoad(tobj);
                    sInstance = tobj.AddComponent<UnityHttp>();
                    tobj.name = "UnityHttp" + "-Object";
                }
                return sInstance;
            }
        }

        public UnityHttp()
        {
            headers.Add("Content-Type", "application/json");
        }

        Dictionary<string, string> headers = new Dictionary<string, string>();
        static public void SetHeader(string pKey, string pValue)
        {
            if (!Instance.headers.ContainsKey(pKey))
            {
                Instance.headers.Add(pKey, pValue);
            }
            else
            {
                Instance.headers[pKey] = pValue;
            }
        }
        static public IHttpRequest Get(string pUrl, System.Action<IHttpRequest> pOnComplete = null,int timeout = 10, bool bypassCertificate = true)
        {
            HttpRequestObject ret = new HttpRequestObject(pUrl, "GET", null, bypassCertificate);
            ret.SetHeaders(Instance.headers);
            ret.webReq.timeout = timeout;
            ret.OnComplete += pOnComplete;
            Get(ret);
            return ret;
        }

        static public IHttpRequest Post(string pUrl, string pSendText, System.Action<IHttpRequest> pOnComplete = null, int timeout = 10, bool bypassCertificate = true)
        {
            HttpRequestObject ret = new HttpRequestObject(pUrl, "POST",string.IsNullOrEmpty(pSendText) ? null : Encoding.UTF8.GetBytes(pSendText), bypassCertificate);
            ret.SetHeaders(Instance.headers);
            ret.webReq.timeout = timeout;
            ret.OnComplete += pOnComplete;
            Post(ret);
            return ret;
        }

        static public IHttpRequest Post(string pUrl, byte[] pBytes, System.Action<IHttpRequest> pOnComplete = null, int timeout = 10, bool bypassCertificate = true)
        {
            HttpRequestObject ret = new HttpRequestObject(pUrl, "POST", pBytes, bypassCertificate);
            ret.SetHeaders(Instance.headers);
            ret.webReq.timeout = timeout;
            ret.OnComplete += pOnComplete;
            Post(ret);
            return ret;
        }

        static public void Post(IHttpRequest pRequest)
        {
            Instance.StartCoroutine(pRequest.Post());
        }
        static public void Get(IHttpRequest pRequest)
        {
            Instance.StartCoroutine(pRequest.Get());
        }
    }
}

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
namespace LitEngine.Net
{
    public class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            //Simply return true no matter what
            return true;
        }
    }

    public interface IHttpRequest
    {
        string Url { get; }
        bool IsDone { get; }
        string Error { get; }
        string text { get; }
        byte[] bytes { get; }
        event System.Action<IHttpRequest> OnComplete;

        IEnumerator Post();
        IEnumerator Get();
    }

    public class HttpErroObject : IHttpRequest
    {
        public string Url { get; private set; } = "";
        public bool IsDone { get; private set; } = false;
        public string Error { get; private set; } = null;
        public string text { get; private set; } = null;
        public byte[] bytes { get; private set; } = null;

        public event System.Action<IHttpRequest> OnComplete;
        public HttpErroObject(string pUrl,string pErro)
        {
            Url = pUrl;
            Error = pErro;
            IsDone = true;
        }

        public IEnumerator Post()
        {
            yield return null;
            try
            {
                IsDone = true;
                OnComplete?.Invoke(this);
            }
            catch (System.Exception err)
            {
                DLog.LogError(err);
            }
        }
        public IEnumerator Get()
        {
            yield return null;
        }
    }

    public class HttpRequestObject : IHttpRequest
    {
        public string Url { get; private set; } = "";
        public bool IsDone { get; private set; } = false;
        public string Error { get; private set; } = null;
        public string text { get; private set; } = null;
        public byte[] bytes { get; private set; } = null;
        public UnityWebRequest webReq { get; private set; } = null;

        public event System.Action<IHttpRequest> OnComplete;

        byte[] sendbytes = null;

        public HttpRequestObject(string pUrl, string pType, byte[] pbytes, bool pBypass = true)
        {
            Url = pUrl;
            sendbytes = pbytes;
            webReq = new UnityWebRequest(Url, pType);
            webReq.timeout = 10;
            if (pBypass)
            {
                webReq.certificateHandler = new BypassCertificate();
            }
        }

        public void SetHeader(string pKey, string pValue)
        {
            webReq.SetRequestHeader(pKey, pValue);
        }

        public void SetHeaders(Dictionary<string, string> headers)
        {
            foreach (var item in headers)
            {
                webReq.SetRequestHeader(item.Key, item.Value);
            }
        }

        public IEnumerator Get()
        {
            yield return null;
            webReq.downloadHandler = new DownloadHandlerBuffer();
            yield return webReq.SendWebRequest();
            if (webReq.isHttpError || webReq.isNetworkError)
            {
                Error = webReq.error;
                DLog.LogFormat("UnityHttp Get error:{0}", Error);
            }
            else
            {
                OnGetComplete();
            }
            CompleteReq();
        }

        public IEnumerator Post()
        {
            yield return null;
            if (sendbytes != null)
            {
                webReq.uploadHandler = new UploadHandlerRaw(sendbytes);
            }
            webReq.downloadHandler = new DownloadHandlerBuffer();
            yield return webReq.SendWebRequest();
            if (webReq.isHttpError || webReq.isNetworkError)
            {
                Error = webReq.error;
                DLog.LogFormat("UnityHttp Get error:{0}", Error);
            }
            else
            {
                OnGetComplete();
            }
            CompleteReq();
        }

        void OnGetComplete()
        {
            bytes = webReq.downloadHandler.data;
            text = webReq.downloadHandler.text;
        }
        void CompleteReq()
        {
            try
            {
                IsDone = true;
                OnComplete?.Invoke(this);
            }
            catch (System.Exception err)
            {
                DLog.LogError(err);
            }
            webReq.Dispose();
            webReq = null;
        }

    }
}

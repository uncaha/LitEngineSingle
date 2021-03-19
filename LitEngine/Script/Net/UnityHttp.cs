using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
namespace LitEngine.Net
{
    public sealed class UnityHttp : MonoBehaviour
    {
        public class HttpRequestObject
        {
            public string Url { get; private set; } = "";
            public bool IsDone { get; private set; } = false;
            public UnityWebRequest webReq { get; private set; }

            public event System.Action<HttpRequestObject> OnComplete;

            public HttpRequestObject(string pUrl)
            {
                Url = pUrl;
                webReq = new UnityWebRequest(Url);
            }
     
            public IEnumerator Send()
            {
                yield return null;
                webReq.SendWebRequest();
                yield return webReq;
                CompleteReq();
            }
            void CompleteReq()
            {
                try
                {
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
                    tobj.name = "Http" + "-Object";
                }
                return sInstance;
            }
        }

        static public HttpRequestObject Send(string pUrl,System.Action<HttpRequestObject> pOnComplete)
        {
            HttpRequestObject ret = new HttpRequestObject(pUrl);
            ret.OnComplete += pOnComplete;
            Instance.StartCoroutine(ret.Send());
            return ret;
        }
    }
}
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System.Text;
using System.Security.Cryptography;

namespace LitEngine.Net
{
    public class HttpCacheManager
    {
        private static HttpCacheManager _instance = null;

        public static HttpCacheManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HttpCacheManager();
                }

                return _instance;
            }
        }

        private string _cachePath;
        public string cachePath
        {
            get
            {
                if (_cachePath == null)
                {

                    _cachePath = $"{Application.persistentDataPath}/habby/httpCache";
                }

                return _cachePath;
            }
        }

        public static string GetMD5(string pStr)
        {
            byte[] data = Encoding.UTF8.GetBytes(pStr);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] OutBytes = md5.ComputeHash(data);
            StringBuilder tstr = new StringBuilder();
            for (int i = 0; i < OutBytes.Length; i++)
            {
                tstr.Append(OutBytes[i].ToString("x2"));
            }
            return tstr.ToString();
        }

        private Dictionary<string, HttpCacheObject> cacheMap = new Dictionary<string, HttpCacheObject>(20);
        public HttpCacheObject GetCache(string pKey)
        {
            if (cacheMap.ContainsKey(pKey))
            {
                return cacheMap[pKey];
            }

            var tobj = LoadCache(pKey);
            if (tobj != null)
            {
                cacheMap.Add(tobj.Url, tobj);
            }

            return tobj;
        }

        public void AddCache(HttpCacheObject pObj)
        {
            if (pObj == null || pObj.Url == null) return;
            if (cacheMap.ContainsKey(pObj.Url)) return;

            cacheMap.Add(pObj.Url, pObj);

            AddSave(pObj);
        }

        internal void AddSave(HttpCacheObject pObj)
        {
            if (pObj == null) return;
            SaveCache(pObj);
        }

        private void SaveCache(HttpCacheObject pObj)
        {
            if (pObj == null) return;

            try
            {
                pObj.file = GetFIlePathByKey(pObj.Url);

                //var tjson = DataConvert.ToJson(pObj);

                //if (tjson != null)
                {
                    // File.WriteAllText(pObj.file, tjson);
                }
            }
            catch (System.Exception e)
            {
                // HabbyLog.LogError("CacheSave", e.Message);
            }

        }

        private HttpCacheObject LoadCache(string pUrl)
        {
            try
            {
                string tpath = GetFIlePathByKey(pUrl);
                if (File.Exists(tpath))
                {
                    var tstr = File.ReadAllText(tpath);

                    // var tobj = DataConvert.FromJson<HttpCacheObject>(tstr);

                    //if (tobj != null)
                    {
                        // cacheMap.Add(tobj.Url, tobj);
                    }

                    //return tobj;
                }
            }
            catch (System.Exception e)
            {
                //HabbyLog.LogError("CacheLoad", e.Message);
            }


            return null;
        }

        private string GetFIlePathByKey(string pKey)
        {
            string tfile = GetMD5(pKey);
            string tpath = $"{cachePath}/{tfile}.cache";
            return tpath;
        }
    }
}
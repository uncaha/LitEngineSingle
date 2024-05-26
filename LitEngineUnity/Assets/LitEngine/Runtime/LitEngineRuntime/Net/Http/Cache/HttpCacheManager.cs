using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Concurrent;

namespace LitEngine.Net.Http
{
   public class HttpCacheManager
    {
        private static HttpCacheManager _instance = null;
        private static object lockObj = new object();
        public static HttpCacheManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (lockObj)
                    {
                        if (_instance == null)
                        {
                            _instance = new HttpCacheManager();
                        }
                    }
                }

                return _instance;
            }
        }

        public string CachePath { get; private set; }

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

        private HttpCacheManager()
        {
            try
            {
                CachePath = $"{Application.persistentDataPath}/HabbyHttpCache";
                if (!Directory.Exists(CachePath))
                {
                    Directory.CreateDirectory(CachePath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"HttpCacheManager init error = {e}");
            }

        }

        private ConcurrentDictionary<string, HttpCacheObject> cacheMap = new ConcurrentDictionary<string, HttpCacheObject>();
        public HttpCacheObject GetCache(string pKey)
        {
            HttpCacheObject ret = null;
            if (cacheMap.ContainsKey(pKey))
            {
                cacheMap.TryGetValue(pKey, out ret);
            }
            else
            {
                ret = new HttpCacheObject(pKey);
                ret.LoadCache();
                cacheMap.TryAdd(ret.Url, ret);
            }

            return ret != null && ret.cached ? ret : null;
        }

        public void AddCache(HttpCacheObject pObj)
        {
            if (pObj == null || string.IsNullOrEmpty(pObj.Url)) return;
            if (cacheMap.ContainsKey(pObj.Url))
            {
                cacheMap[pObj.Url] = pObj;
                AddSave(pObj);
            }
            else
            {
                if (cacheMap.TryAdd(pObj.Url, pObj))
                {
                    AddSave(pObj);
                }
            }
        }

        internal void AddSave(HttpCacheObject pObj)
        {
            if (pObj == null) return;
            SaveCache(pObj);
        }

        private void SaveCache(HttpCacheObject pObj)
        {
            if (pObj == null) return;
            pObj.SaveCache();
        }

        internal string GetFIlePathByKey(string pKey)
        {
            string tfile = GetMD5(pKey);
            string tpath = $"{CachePath}/{tfile}.cache";
            return tpath;
        }
    }
}
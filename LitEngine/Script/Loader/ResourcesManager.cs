using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using LitEngine.LoadAsset;
using UnityEngine;
namespace LitEngine
{
    public sealed class ResourcesManager : MonoManagerBase
    {

        #region static
        private static object lockobj = new object();
        private static ResourcesManager sInstance = null;
        private static ResourcesManager Instance
        {
            get
            {
                if (sInstance == null)
                {
                    lock (lockobj)
                    {

                        if (sInstance == null)
                        {
                            GameObject tobj = new GameObject("ResourcesManager");
                            GameObject.DontDestroyOnLoad(tobj);
                            sInstance = tobj.AddComponent<ResourcesManager>();
                            sInstance.Init();
                        }
                    }
                }
                return sInstance;
            }
        }

        static public string resourcesMapPath = "ResourcesMap";
        static public Func<string, System.Type, UnityEngine.Object> assetLoaderDelgate = null;

        static public void SetResourcesMap(string pMapPath, Func<string, System.Type, UnityEngine.Object> pLoader = null)
        {
            resourcesMapPath = pMapPath;
            assetLoaderDelgate = pLoader;
            Instance.InitAssetMap();
        }

        static public string GetRealPath(string pPath)
        {
            return GameCore.CombinePath(GameCore.ExportPath, pPath);
        }

        static public bool ReleaseAsset(string path)
        {
            if(Instance.resCacheDic.ContainsKey(path))
            {
                var titem = Instance.resCacheDic[path];
                titem.Release();
                if(titem.disposed)
                {
                    Instance.resCacheDic.Remove(path);
                    return true;
                }
            }
            return false;
        }

        static public bool RemoveAsset(string path)
        {
            if (Instance.resCacheDic.ContainsKey(path))
            {
                var titem = Instance.resCacheDic[path];
                titem.Dispose();
                Instance.resCacheDic.Remove(path);
                return true;
            }
            return false;
        }

        static public void RemoveAllAsset()
        {
            var tdic = Instance.resCacheDic;

            var tlistkeys = new List<string>(tdic.Keys);
            for (int i = 0,max = tlistkeys.Count; i < max; i++)
            {
                var tkey = tlistkeys[i];
                var item = tdic[tkey];

                item.Dispose();
                tdic.Remove(tkey);
            }
            
        }

        public static T Load<T>(string path) where T : UnityEngine.Object
        {
            T ret = null;
            #region 已缓存
            bool tisCached = Instance.GetCachedRes(path, out ret);
            if (tisCached)
            {
                return ret;
            }
            #endregion

            #region load
            var assetobj = Instance.assetMap.GetAsset(path);
            IResourcesObject tResObject = null;
            if (!assetobj.isInSide)
            {
                string trealPath = GetRealPath(path + assetobj.sufixx);

                if (assetLoaderDelgate != null)
                {
                    var tobj = assetLoaderDelgate(trealPath, typeof(T));
                    tResObject = new ResourcesObject(trealPath, tobj);
                }
                else
                {
                    var tbundle = LoaderManager.GetBundleFromAsset(trealPath);
                    tResObject = new ResourcesAssetObject(trealPath, tbundle);
                }
            }
            else
            {
                var tobj = Resources.Load<T>(path);
                tResObject = new ResourcesObject(path, tobj);
            }

            if (tResObject.resObject != null)
            {
                Instance.resCacheDic.Add(path, tResObject);
                ret = tResObject.Retain() as T;
            }

            #endregion

            return ret;
        }

        public static IResourcesLoader LoadAnsyc<T>(string path, Action<UnityEngine.Object> onLoadComplete) where T : UnityEngine.Object
        {
            IResourcesLoader tloader = null;

            #region 已缓存
            bool tisCached = Instance.GetCachedRes(path,out T tcachedRes);
            if(tisCached)
            {
                try
                {
                    if (onLoadComplete != null)
                    {
                        onLoadComplete(tcachedRes);
                    }
                }
                catch (Exception ex)
                {
                    DLog.LogErrorFormat("ResourcesManager.LoadAnsyc:{0}", ex.ToString()); ;
                }

                tloader = new ResourcesLoaded<T>(path, tcachedRes, null);
                return tloader;
            }
            #endregion

            #region 加载中
            if (Instance.asyncLoaderList.ContainsKey(path))
            {
                tloader = Instance.asyncLoaderList[path];

                if (onLoadComplete != null)
                {
                    tloader.resourcesObject.Retain();
                    tloader.onComplete += onLoadComplete;
                }

                return tloader;
            }
            #endregion

            #region load
            var assetobj = Instance.assetMap.GetAsset(path);
            
            if (!assetobj.isInSide)
            {
                string realPath = GetRealPath(path + assetobj.sufixx);
                if (assetLoaderDelgate != null)
                {
                    tloader = new EditorAssetLoader<T>(path, realPath, assetLoaderDelgate, onLoadComplete);
                }
                else
                {
                    tloader = new ResourcesAssetLoader<T>(path, realPath, onLoadComplete);
                }
            }
            else
            {
                tloader = new ResourcesLoader<T>(path, onLoadComplete);
            }

            bool tisStart = tloader.StartLoad();
            if (tisStart)
            {
                Instance.asyncLoaderList.Add(path, tloader);
            }

            return tloader;
            #endregion

        }
        #endregion

        #region member
        private bool mInited = false;
        private Dictionary<string,IResourcesLoader> asyncLoaderList = new Dictionary<string,IResourcesLoader>(500);
        private Dictionary<string, IResourcesObject> resCacheDic = new Dictionary<string, IResourcesObject>();
        private AssetMap assetMap;

        private void Init()
        {
            if (mInited) return;
            mInited = true;

            InitAssetMap();
        }

        private void InitAssetMap()
        {
            assetMap = Resources.Load<AssetMap>(resourcesMapPath);
            if (assetMap == null)
            {
                assetMap = new AssetMap();
            }
            assetMap.Init();
        }

        bool GetCachedRes<T>(string path,out T resObj) where T:UnityEngine.Object
        {
            resObj = null;
            if (resCacheDic.TryGetValue(path, out IResourcesObject outobj))
            {
                if (!outobj.disposed)
                {
                    resObj = (T)outobj.Retain();
                    return true;
                }
                else
                {
                    resCacheDic.Remove(path);
                }
            }
            return false;
        }

        private void UpdateLoader(string pKey,IResourcesLoader pLoader)
        {
            pLoader.Update();
            if (pLoader.IsDone)
            {
                if (pLoader.res != null)
                {
                    if (!resCacheDic.ContainsKey(pLoader.resPath))
                    {
                        resCacheDic.Add(pLoader.resPath, pLoader.resourcesObject);
                    }
                }

                asyncLoaderList.Remove(pKey);
            }
        }

        List<string> updateKeyList = new List<string>(100);
        private void Update()
        {
            if (asyncLoaderList.Count > 0)
            {
                updateKeyList.Clear();
                updateKeyList.AddRange(asyncLoaderList.Keys);

                int tcount = updateKeyList.Count;
                for (int i = 0; i < tcount; i++)
                {
                    var itemkey = updateKeyList[i];
                    UpdateLoader(itemkey, asyncLoaderList[itemkey]);
                }
            }
        }

        #endregion
    }
}

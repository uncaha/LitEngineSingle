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
        static private long refIndex = 1;

        static public void SetResourcesMap(string pMapPath, Func<string, System.Type, UnityEngine.Object> editorLoader)
        {
            resourcesMapPath = pMapPath;
            assetLoaderDelgate = editorLoader;
            Instance.InitAssetMap();
        }

        static public string GetRealPath(string pPath)
        {
            return GameCore.CombinePath(GameCore.ExportPath, pPath);
        }

        public static T Load<T>(string path) where T : UnityEngine.Object
        {

            IResourcesObject outobj = null;
            if (Instance.resCacheDic.TryGetValue(path, out outobj))
            {
                return outobj.Retain() as T;
            }

            T ret = null;
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

            return ret;
        }

        public static IResourcesLoader LoadAnsyc<T>(string path, Action<UnityEngine.Object> onLoadComplete) where T : UnityEngine.Object
        {
            IResourcesLoader tloader = null;

            #region 已缓存
            IResourcesObject tresobj = null;
            if (Instance.resCacheDic.TryGetValue(path, out tresobj))
            {
                tloader = new ResourcesLoaded<T>(path, tresobj.Retain(), onLoadComplete);
                if (tloader.StartLoad())
                {
                    Instance.asyncLoaderList.Add(path + refIndex, tloader);
                    refIndex++;
                }
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

        public void Update()
        {
            int tcount = asyncLoaderList.Count;
            
            if (tcount > 0)
            {
                var tlistkeys = new List<string>(asyncLoaderList.Keys);
                for (int i = tcount - 1; i >= 0; i--)
                {
                    var tkey = tlistkeys[i];
                    var item = asyncLoaderList[tkey];
                    item.Update();
                    if (item.IsDone)
                    {
                        if (item.res != null)
                        {
                            if (!resCacheDic.ContainsKey(item.resPath))
                            {
                                resCacheDic.Add(item.resPath, item.resourcesObject);
                            }
                        }

                        asyncLoaderList.Remove(tkey);
                    }
                }
            }
        }

        #endregion
    }
}

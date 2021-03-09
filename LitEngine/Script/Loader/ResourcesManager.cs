using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using LitEngine.LoadAsset;
using UnityEngine;
namespace LitEngine
{
    public class ResourcesManager : MonoManagerBase
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
            T ret = null;
            var assetobj = Instance.assetMap.GetAsset(path);
            if (!assetobj.isInSide)
            {
                string trealPath = GetRealPath(path + assetobj.sufixx);
                if (assetLoaderDelgate != null)
                {
                    ret = (T)assetLoaderDelgate(trealPath, typeof(T));
                }
                else
                {
                    ret = (T)LoaderManager.LoadAsset(trealPath);
                }
            }
            else
            {
                ret = Resources.Load<T>(path);
            }
            return ret;
        }

        public static IResourcesLoader LoadAnsyc<T>(string path, Action<T> onLoadComplete) where T : UnityEngine.Object
        {
            var assetobj = Instance.assetMap.GetAsset(path);
            if (!assetobj.isInSide)
            {
                string realPath = GetRealPath(path + assetobj.sufixx);
                IResourcesLoader tloader = null;
                if (assetLoaderDelgate != null)
                {
                    tloader = new EditorAssetLoader<T>(realPath, assetLoaderDelgate, onLoadComplete);
                }
                else
                {
                    tloader = new ResourcesAssetLoader<T>(path, realPath, onLoadComplete);
                }

                bool tisStart = tloader.StartLoad();
                if (tisStart)
                {
                    Instance.asyncLoaderList.Add(tloader);
                }
                return tloader;
            }
            else
            {
                IResourcesLoader tloader = new ResourcesLoader<T>(path, onLoadComplete);
                bool tisStart = tloader.StartLoad();
                if (tisStart)
                {
                    Instance.asyncLoaderList.Add(tloader);
                }
                return tloader;
            }
        }
        #endregion

        #region member
        private bool mInited = false;
        List<IResourcesLoader> asyncLoaderList = new List<IResourcesLoader>(500);
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
                for (int i = tcount - 1; i >= 0; i--)
                {
                    var item = asyncLoaderList[i];
                    item.Update();
                    if (item.IsDone)
                    {
                        //if (!string.IsNullOrEmpty(item.resPath) && item.res != null)
                        //{
                        //    if (Instance.resMap.ContainsKey(item.resPath))
                        //    {
                        //        Instance.resMap[item.resPath] = item.res;
                        //    }
                        //    else
                        //    {
                        //        Instance.resMap.Add(item.resPath, item.res);
                        //    }
                        //}

                        asyncLoaderList.RemoveAt(i);
                    }
                }
            }
        }

        #endregion
    }
}

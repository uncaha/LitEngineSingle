﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using LitEngine.LoadAsset;
namespace LitEngine.LoadAsset
{
    public class LoaderManager
    {
        public const string ManifestName = "AppManifest";
        public const string byteFileInfoFileName = "bytefileinfo.txt";
        private static object lockobj = new object();
        private static LoaderManager sInstance = null;
        public static LoaderManager Instance
        {
            get
            {
                if (sInstance == null)
                {
                    sInstance = new LoaderManager();
                    sInstance.Init();
                }
                return sInstance;
            }
        }

        #region 属性
        private Dictionary<string,AssetGroup> groupMap = new Dictionary<string, AssetGroup>();
        private BundleVector mBundleList = null;
        private LoadTaskVector mBundleTaskList = null;
        private WaitingList mWaitLoadBundleList = null;
        public AssetBundleManifest Manifest { get; private set; }

        private ByteFileInfoList _ByteInfoData;
        public static ByteFileInfoList ByteInfoData { get { return Instance._ByteInfoData; } }

        private bool mInited = false;
        #endregion
        #region 路径获取

        static public string GetFullPath(string _filename)
        {
            _filename = BaseBundle.CombineSuffixName(_filename);
            string tfullpathname = $"{GameCore.PersistentResDataPath}/{_filename}";
            if (!File.Exists(tfullpathname))
                tfullpathname = $"{GameCore.StreamingAssetsResDataPath}/{_filename}";
            return tfullpathname;
        }
        #endregion

        #region 初始化,销毁,设置
        private LoaderManager()
        {
            
        }
        static public void ReLoadResInfo()
        {
            if(sInstance != null)
            {
                sInstance.LoadResInfo();
            }
        }
        private void Init()
        {
            if (mInited) return;
            mInited = true;

            mWaitLoadBundleList = new WaitingList();
            mBundleList = new BundleVector();
            mBundleTaskList = new LoadTaskVector();

            LoadResInfo();
            
            GameUpdateManager.RegUpdate(Update, "LoaderManager");
        }

        private void LoadResInfo()
        {
            LoadMainfest();
            LoadByteFileInfoList();
        }

        private void LoadMainfest()
        {
            AssetBundle tbundle = AssetBundle.LoadFromFile(GetFullPath(ManifestName));
            if (tbundle != null)
            {
                Manifest = tbundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                tbundle.Unload(false);
            }
        }

        public void LoadByteFileInfoList()
        {
            _ByteInfoData = new ByteFileInfoList();
            AssetBundle tinfobundle = AssetBundle.LoadFromFile(GetFullPath(byteFileInfoFileName));
            if (tinfobundle != null)
            {
                TextAsset tass = tinfobundle.LoadAsset<TextAsset>(byteFileInfoFileName);
                if (tass != null)
                {
                    _ByteInfoData.Load(tass.bytes);
                }
                tinfobundle.Unload(false);
            }
        }

        #endregion

        #region update
        void Update()
        {
            if (mWaitLoadBundleList.Count == 0 && mBundleTaskList.Count == 0) return;
            if (mWaitLoadBundleList.Count > 0)
            {
                for (int i = mWaitLoadBundleList.Count - 1; i >= 0; i--)
                {
                    BaseBundle tbundle = mWaitLoadBundleList[i];
                    if (tbundle.IsDone())
                        mWaitLoadBundleList.RemoveAt(i);
                }
            }

            if (mBundleTaskList.Count > 0)
            {
                for (int i = mBundleTaskList.Count - 1; i >= 0; i--)
                {
                    mBundleTaskList[i].IsDone();
                }
            }
        }
        #endregion

        #region 资源管理
        static public string GetRealAssetName(string pAssetName)
        {
            return $"{GameCore.ExportPath}/{pAssetName}";
        }

        static public string[] GetAllDependencies(string _assetBundleName)
        {
            if (Instance.Manifest == null) return null;
            return Instance.Manifest.GetAllDependencies(BaseBundle.CombineSuffixName(_assetBundleName));
        }
        static public string[] GetDirectDependencies(string _assetBundleName)
        {
            if (Instance.Manifest == null) return null;
            return Instance.Manifest.GetDirectDependencies(BaseBundle.CombineSuffixName(_assetBundleName));
        }
        static public string[] GetAllAssetBundles()
        {
            if (Instance.Manifest == null) return null;
            return Instance.Manifest.GetAllAssetBundles();
        }

        private void AddmWaitLoadList(BaseBundle _bundle)
        {
            mWaitLoadBundleList.Add(_bundle);
        }

        private void AddCache(BaseBundle _bundle)
        {
            mBundleList.Add(_bundle);
        }
        static public void AddToGroup(string pGroupKey, string assetName)
        {
            if(string.IsNullOrEmpty(pGroupKey) || string.IsNullOrEmpty(assetName)) return;
            var tmap = Instance.groupMap;
            if (!tmap.ContainsKey(pGroupKey))
            {
                tmap.Add(pGroupKey, new AssetGroup(pGroupKey));
            }
            tmap[pGroupKey].AddAsset(assetName);
        }
        static public void ReleaseGroup(string _key)
        {
            var tmap = Instance.groupMap;
            if(tmap.ContainsKey(_key))
            {
                tmap[_key].ReleaseAssets();
            }
        }

        static public void ReleaseAsset(string _key)
        {
            Instance.mBundleList.ReleaseBundle(BaseBundle.DeleteSuffixName(_key.ToLowerInvariant()));
        }

        private void RemoveAllAsset()
        {
            mBundleList.Clear();
        }

        public void ReleaseUnUsedAssets()
        {
            List<BaseBundle> tlist = new List<BaseBundle>(mBundleList.values);
            for (int i = tlist.Count - 1; i >= 0; i--)
            {
                BaseBundle tbundle = tlist[i];
                if (tbundle.WillBeReleased && tbundle.Loaded)
                {
                    tbundle.Release();
                }

            }
        }

        static public void RemoveAsset(string _AssetsName)
        {
            Instance.mBundleList.Remove(BaseBundle.DeleteSuffixName(_AssetsName.ToLowerInvariant()));
        }

        #endregion
        private LoadTask CreatTaskAndStart(string _key, BaseBundle _bundle, System.Action<string, object> _callback, bool _retain)
        {
            LoadTask ret = new LoadTask(_key, _bundle, _callback, _retain);
            mBundleTaskList.Add(ret);
            return ret;
        }

        #region 资源载入
        #region static fun

        #region Scene
        private static bool IsSceneLoading = false;
        static private System.Action mLoadSceneCall = null;
        static private string mNowLoadingScene = null;

        static public bool LoadScene(string _scenename,string pGroupKey = null)
        {
            if (IsSceneLoading)
            {
                DLog.LogError("The Scene is Loading.");
                return false;
            }
            _scenename = BaseBundle.DeleteSuffixName(_scenename);
            string tusname = _scenename.EndsWith(".unity") ? _scenename.Replace(".unity", "") : _scenename;
            if (SceneManager.GetActiveScene().name.Equals(tusname))
                return false;
            LoaderManager.LoadAsset(_scenename,pGroupKey);
            SceneManager.LoadScene(tusname, LoadSceneMode.Single);
            return true;
        }

        static public bool LoadSceneAsync(string _scenename, System.Action _FinishdCall,string pGroupKey = null)
        {
            if (IsSceneLoading)
            {
                DLog.LogError("The Scene is Loading.");
                return false;
            }
            _scenename = BaseBundle.DeleteSuffixName(_scenename);
            mNowLoadingScene = _scenename.EndsWith(".unity") ? _scenename.Replace(".unity", "") : _scenename;
            if (SceneManager.GetActiveScene().name.Equals(mNowLoadingScene))
                return false;

            IsSceneLoading = true;
            mLoadSceneCall = _FinishdCall;

            LoaderManager.LoadAssetAsync(_scenename, _scenename, Instance.LoadedStartScene,pGroupKey);

            return true;
        }


        private void LoadedStartScene(string _key, object _object)
        {
            SceneManager.sceneLoaded += LoadSceneCall;
            AsyncOperation topert = SceneManager.LoadSceneAsync(mNowLoadingScene);

        }
        private void LoadSceneCall(Scene _scene, LoadSceneMode _mode)
        {
            if (mLoadSceneCall != null)
                mLoadSceneCall();
            mLoadSceneCall = null;
            SceneManager.sceneLoaded -= LoadSceneCall;
            mNowLoadingScene = null;
            IsSceneLoading = false;
        }

        #endregion

        #region 同步
        static public UnityEngine.Object LoadAsset(string pAssetName,string pGroupKey = null)
        {
            var tbundle = Instance.LoadAssetRetain(pAssetName.ToLowerInvariant());
            if (tbundle == null) return null;
            AddToGroup(pGroupKey, pAssetName);
            return (UnityEngine.Object)tbundle.Retain();
        }

        static public BaseBundle GetBundleFromAsset(string pAssetName, string pGroupKey = null)
        {
            var tbundle = Instance.LoadAssetRetain(pAssetName.ToLowerInvariant());
            if (tbundle == null) return null;
            AddToGroup(pGroupKey, pAssetName);
            tbundle.Retain();
            return tbundle;
        }
        #endregion

        #region 异步
        static public BaseBundle LoadAssetAsync(string pKey, string pAssetName, System.Action<string, object> _callback, string pGroupKey = null)
        {
            AddToGroup(pGroupKey, pAssetName);
            return Instance.LoadAssetAsyncRetain(pKey, pAssetName.ToLowerInvariant(), _callback, true);
        }

        #endregion


        #endregion

        #region 同步载入
        private BaseBundle LoadAssetRetain(string _AssetsName)
        {
            if (string.IsNullOrEmpty(_AssetsName)) return null;
            BaseBundle ret = null;
            bool tiscached = mBundleList.Contains(_AssetsName);
            if (!tiscached)
            {
                ret = new AssetsBundleHaveDependencie(_AssetsName, LoadAssetRetain);
                AddCache(ret);
                ret.Load();
            }
            else
            {
                ret = mBundleList[_AssetsName];
                if(!ret.Loaded)
                {
                    DLog.LogErrorFormat("{0} 已存在一个异步操作,并且还未完成加载.", _AssetsName);
                    return null;
                }
            }
            return ret;
        }
        #endregion
        #region 异步载入

        protected void LoadBundleAsync(BaseBundle _bundle, string _key, System.Action<string, object> _callback, bool _retain)
        {
            AddCache(_bundle);
            _bundle.Load();
            AddmWaitLoadList(_bundle);
            CreatTaskAndStart(_key, _bundle, _callback, _retain);
        }

        private BaseBundle LoadAssetAsyncRetain(string _key, string _AssetsName, System.Action<string, object> _callback, bool _retain)
        {

            if (_AssetsName.Length == 0)
            {
                DLog.LogError("LoadAssetAsyncRetain -- _AssetsName 的长度不能为空");
                if (_callback != null)
                    _callback(_key, null);
                return null;
            }
            if (_callback == null)
            {
                DLog.LogError("LoadAssetAsyncRetain -- CallBack Fun can not be null");
                return null;
            }
            if (mBundleList.Contains(_AssetsName))
            {
                if (mBundleList[_AssetsName].Loaded)
                {
                    if (mBundleList[_AssetsName].Asset == null)
                        DLog.LogError("LoadAssetAsyncRetain-erro in vector。文件载入失败,请检查文件名:" + _AssetsName);
                    if (_retain)
                        mBundleList[_AssetsName].Retain();
                    _callback(_key, mBundleList[_AssetsName].Asset);

                }
                else
                {
                    CreatTaskAndStart(_key, mBundleList[_AssetsName], _callback, _retain);
                }

            }
            else
            {

                LoadBundleAsync(new AssetsBundleHaveDependencieAsync(_AssetsName, LoadAssetAsyncRetain), _key, _callback, _retain);
            }
            return mBundleList[_AssetsName];
        }
        #endregion

        #endregion

        #region 文本读取

        static byte[] LoadBytes(string pFileName)
        {
            var tfullname = $"{GameCore.PersistentResDataPath}/{GameCore.ExportPath}/{GameCore.ConfigDataPath}/{pFileName}{BaseBundle.sSuffixName}";

            if (!File.Exists(tfullname))
            {
                return LoadBytesFromFile(pFileName);
            }

            return LoadBytesFromBundle(pFileName, tfullname);
        }

        static byte[] LoadBytesFromFile(string pFileName)
        {
            pFileName = BaseBundle.DeleteSuffixName(pFileName);
            var tfullname = $"{GameCore.ConfigDataPath}/{pFileName}";
            var ttext = Resources.Load<TextAsset>(tfullname);
            if (ttext == null)
            {
                DLog.LogError("read config fail. name = " + tfullname);
                return null;
            }

            return ttext.bytes;
        }

        static byte[] LoadBytesFromBundle(string pName, string pFilePath)
        {
            try
            {
                byte[] ret = null;

                AssetBundle tinfobundle = AssetBundle.LoadFromFile(pFilePath);
                if (tinfobundle != null)
                {
                    TextAsset tass = tinfobundle.LoadAsset<TextAsset>(pName);
                    if (tass != null)
                    {
                        ret = tass.bytes;
                    }
                    else
                    {
                        DLog.LogErrorFormat("AssetBundle.LoadAsset<TextAsset>() read failed. name = {0}", pFilePath);
                    }

                    tinfobundle.Unload(false);
                }
                else
                {
                    DLog.LogError($"read config failed. name = {pFilePath}" );
                }

                return ret;
            }
            catch (Exception e)
            {
                DLog.LogError(e);
            }

            return null;
        }


        static public byte[] LoadConfigFile(string pFileName)
        {
            byte[] ret = LoadBytes(pFileName);
            if (ret == null)
                DLog.LogError("文件读取失败 name = " + pFileName);
            return ret;
        }
        #endregion
    }
}


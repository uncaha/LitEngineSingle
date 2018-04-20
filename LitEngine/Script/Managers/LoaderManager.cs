using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
namespace LitEngine
{
    using UpdateSpace;
    using Loader;
    public class LoaderManager : MonoManagerBase
    {
        static public string ManifestName = "AppManifest";
        private static LoaderManager sInstance = null;
        private static LoaderManager Instance
        {
            get
            {
                if (sInstance == null)
                {
                    GameObject tobj = new GameObject("LoaderManager");
                    GameObject.DontDestroyOnLoad(tobj);
                    sInstance = tobj.AddComponent<LoaderManager>();
                    sInstance.Init();
                }
                return sInstance;
            }
        }

        #region 属性
        private BundleVector mBundleList = null;
        private LoadTaskVector mBundleTaskList = null;
        private WaitingList mWaitLoadBundleList = null;
        public AssetBundleManifest Manifest { get; private set; }
        private bool mInited = false;

        #endregion
        #region 路径获取

        static public string GetResourcesDataPath(string _filename)
        {
            return Path.Combine(GameCore.ResourcesResDataPath, _filename);
        }

        static public string GetFullPath(string _filename)
        {
            _filename = BaseBundle.CombineSuffixName(_filename);
            string tfullpathname = GameCore.CombineFilePath(GameCore.PersistentResDataPath, _filename);
            if (!File.Exists(tfullpathname))
                tfullpathname = GameCore.CombineFilePath(GameCore.StreamingAssetsResDataPath, _filename);
            return tfullpathname;
        }
        #endregion

        #region 初始化,销毁,设置
        private void Init()
        {
            if (mInited) return;
            mInited = true;
            AssetBundle tbundle = AssetBundle.LoadFromFile(GetFullPath(ManifestName));
            if (tbundle != null)
            {
                Manifest = tbundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                tbundle.Unload(false);
            }
            else
                DLog.LogErrorFormat("未能加载App资源列表 filename = {0}", ManifestName);

            mWaitLoadBundleList = new WaitingList();
            mBundleList = new BundleVector();
            mBundleTaskList = new LoadTaskVector();
        }
        #region 释放
        override public void DestroyManager()
        {
            base.DestroyManager();
        }
        override protected void OnDestroy()
        {
            DisposeNoGcCode();
            base.OnDestroy();
        }
        protected void DisposeNoGcCode()
        {
            if (Manifest != null)
                Object.DestroyImmediate(Manifest, true);
            mBundleTaskList.Clear();
            mWaitLoadBundleList.Clear();
            RemoveAllAsset();
        }
        #endregion

        #endregion

        #region update
        void Update()
        {
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


            if (mWaitLoadBundleList.Count == 0 && mBundleTaskList.Count == 0)
                ActiveLoader(false);
        }
        #endregion

        #region fun
        void ActiveLoader(bool _active)
        {
            if (gameObject.activeSelf == _active) return;
            gameObject.SetActive(_active);
        }
        #endregion

        #region 资源管理

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

        static public void ReleaseAsset(string _key)
        {
            Instance.mBundleList.ReleaseBundle(_key);
        }

        private void RemoveAllAsset()
        {
            mBundleList.Clear();
        }

        static public void RemoveAsset(string _AssetsName)
        {
            Instance.mBundleList.Remove(_AssetsName);
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

        static public void LoadScene(string _scenename)
        {
            if (IsSceneLoading)
            {
                DLog.LogError("The Scene is Loading.");
                return;
            }
            LoaderManager.LoadAsset(_scenename);
            _scenename = BaseBundle.DeleteSuffixName(_scenename);
            SceneManager.LoadScene(_scenename.EndsWith(".unity") ? _scenename.Replace(".unity", "") : _scenename, LoadSceneMode.Single);
        }
        
        static public void LoadSceneAsync(string _scenename, System.Action _FinishdCall)
        {
            if (IsSceneLoading)
            {
                DLog.LogError("The Scene is Loading.");
                return;
            }
            IsSceneLoading = true;
            mLoadSceneCall = _FinishdCall;
            _scenename = BaseBundle.DeleteSuffixName(_scenename);
            mNowLoadingScene = _scenename.Replace(".unity", "");
            LoaderManager.LoadAssetAsync(_scenename, _scenename, LoadedStartScene);
        }

        
        static private void LoadedStartScene(string _key, object _object)
        {
            SceneManager.sceneLoaded += LoadSceneCall;
            AsyncOperation topert = SceneManager.LoadSceneAsync(mNowLoadingScene);

        }
        static private void LoadSceneCall(Scene _scene, LoadSceneMode _mode)
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
        static public Object sLoadResources(string _AssetsName)
        {
            return Instance.LoadResources(_AssetsName.ToLowerInvariant());
        }

        static public UnityEngine.Object LoadAsset(string _AssetsName)
        {
            return (UnityEngine.Object)Instance.LoadAssetRetain(_AssetsName.ToLowerInvariant()).Retain();
        }
        #endregion

        #region 异步
        
        static public void LoadResourcesAsync(string _key, string _AssetsName, System.Action<string, object> _callback)
        {
            Instance.LoadResourcesAsync_(_key.ToLowerInvariant(), _AssetsName.ToLowerInvariant(), _callback);
        }

        static public void LoadAssetAsync(string _key, string _AssetsName, System.Action<string, object> _callback)
        {
            Instance.LoadAssetAsyncRetain(_key.ToLowerInvariant(), _AssetsName.ToLowerInvariant(), _callback, true);
        }

        static public BaseBundle WWWLoadAsync(string _key, string _FullName, System.Action<string, object> _callback)
        {
            return Instance.WWWLoad(_key.ToLowerInvariant(), _FullName.ToLowerInvariant(), _callback);
        }
        #endregion


        #endregion

        #region 同步载入
        #region Res资源
        /// <summary>
        /// 载入Resources资源
        /// </summary>
        /// <param name="_AssetsName">_curPathname 是相对于path/Date/下的路径 例如目录结构Assets/Resources/Date/ +_curPathname</param>
        /// <returns></returns>
        private Object LoadResources(string _AssetsName)
        {
            if (_AssetsName == null || _AssetsName.Equals("")) return null;
            if (mBundleList.Contains(_AssetsName))
            {
                return (Object)mBundleList[_AssetsName].Retain();
            }

            ResourcesBundle tbundle = new ResourcesBundle(_AssetsName);
            AddCache(tbundle);
            tbundle.Load();
            return (Object)tbundle.Retain();
        }
        #endregion
        private BaseBundle LoadAssetRetain(string _AssetsName)
        {
            if (string.IsNullOrEmpty(_AssetsName)) return null;
            if (!mBundleList.Contains(_AssetsName))
            {
                AssetsBundleHaveDependencie tbundle = new AssetsBundleHaveDependencie(_AssetsName, LoadAssetRetain);
                AddCache(tbundle);
                tbundle.Load();
            }
            return mBundleList[_AssetsName];
        }
        #endregion
        #region 异步载入

        protected void LoadBundleAsync(BaseBundle _bundle, string _key, System.Action<string, object> _callback, bool _retain)
        {
            AddCache(_bundle);
            _bundle.Load();
            AddmWaitLoadList(_bundle);
            CreatTaskAndStart(_key, _bundle, _callback, _retain);
            ActiveLoader(true);
        }

        protected void LoadResourcesAsync_(string _key, string _AssetsName, System.Action<string, object> _callback)
        {
            if (_AssetsName.Length == 0)
            {
                DLog.LogError("LoadResourcesBundleByRelativePathNameAsync -- _AssetsName 的长度不能为空");
            }
            if (_callback == null)
            {
                DLog.LogError("LoadResourcesBundleByRelativePathNameAsync -- CallBack Fun can not be null");
                return;
            }
            if (mBundleList.Contains(_AssetsName))
            {
                if (mBundleList[_AssetsName].Loaded)
                {
                    if (mBundleList[_AssetsName].Asset == null)
                        DLog.LogError("ResourcesBundleAsync-erro in vector。文件载入失败,请检查文件名:" + _AssetsName);

                    mBundleList[_AssetsName].Retain();
                    _callback(_key, mBundleList[_AssetsName].Asset);
                }
                else
                {
                    CreatTaskAndStart(_key, mBundleList[_AssetsName], _callback, true);
                    ActiveLoader(true);
                }

            }
            else
            {
                LoadBundleAsync(new ResourcesBundleAsync(_AssetsName), _key, _callback, true);
            }
        }

        private BaseBundle LoadAssetAsyncRetain(string _key, string _AssetsName, System.Action<string, object> _callback, bool _retain)
        {

            if (_AssetsName.Length == 0)
            {
                DLog.LogError("LoadAssetsBundleByFullNameAsync -- _AssetsName 的长度不能为空");
            }
            if (_callback == null)
            {
                DLog.LogError("LoadAssetsBundleByFullNameAsync -- CallBack Fun can not be null");
                return null;
            }
            if (mBundleList.Contains(_AssetsName))
            {
                if (mBundleList[_AssetsName].Loaded)
                {
                    if (mBundleList[_AssetsName].Asset == null)
                        DLog.LogError("AssetsBundleAsyncFromFile-erro in vector。文件载入失败,请检查文件名:" + _AssetsName);
                    if (_retain)
                        mBundleList[_AssetsName].Retain();
                    _callback(_key, mBundleList[_AssetsName].Asset);

                }
                else
                {
                    CreatTaskAndStart(_key, mBundleList[_AssetsName], _callback, _retain);
                    ActiveLoader(true);
                }

            }
            else
            {

                LoadBundleAsync(new AssetsBundleHaveDependencieAsync(_AssetsName, LoadAssetAsyncRetain), _key, _callback, _retain);
            }
            return mBundleList[_AssetsName];
        }
        #endregion
        #region WWW载入
        protected BaseBundle WWWLoad(string _key, string _FullName, System.Action<string, object> _callback)
        {
            if (_callback == null)
            {
                DLog.LogError("assetsbundle -- CallBack Fun can not be null");
                return null;
            }

            if (mBundleList.Contains(_FullName))
            {
                if (mBundleList[_FullName].Loaded)
                {
                    if (mBundleList[_FullName].Asset == null)
                        DLog.LogError("WWWLoad-erro in vector。文件载入失败,请检查文件名:" + _FullName);
                    mBundleList[_FullName].Retain();
                    _callback(_key, mBundleList[_FullName].Asset);
                }
                else
                {
                    CreatTaskAndStart(_key, mBundleList[_FullName], _callback, true);
                    ActiveLoader(true);
                }

            }
            else
            {
                LoadBundleAsync(new WWWBundle(_FullName), _key, _callback, true);
            }
            return mBundleList[_FullName];
        }
        #endregion
        #endregion

        #region 文本读取
        static public byte[] LoadScriptFile(string _filename)
        {
            string tfullname = GameCore.PersistentScriptDataPath + _filename;
            byte[] ret = null;
            if (System.IO.File.Exists(tfullname))
            {
                ret = System.IO.File.ReadAllBytes(tfullname);
            }
            else
            {
                tfullname = GameCore.ResourcesScriptDataPath + _filename;
                TextAsset tasset = (TextAsset)Resources.Load(BaseBundle.DeleteSuffixName(tfullname));
                ret = tasset.bytes;
            }

            return ret;
        }

        static public byte[] LoadConfigFile(string _filename)
        {
            string tfullname = GameCore.PersistentConfigDataPath + _filename;
            byte[] ret = null;
            if (System.IO.File.Exists(tfullname))
            {
                ret = System.IO.File.ReadAllBytes(tfullname);
            }
            else
            {
                tfullname = GameCore.ResourcesConfigDataPath + _filename;
                TextAsset tasset = (TextAsset)Resources.Load(BaseBundle.DeleteSuffixName(tfullname));
                if(tasset != null)
                    ret = tasset.bytes;
            }

            return ret;
        }
        #endregion
    }
}


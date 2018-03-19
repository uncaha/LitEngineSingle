using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
namespace LitEngine
{
    using UpdateSpace;
    namespace Loader
    {
        public class LoaderManager : MonoManagerBase
        {
            
            #region 属性
            private string mAppName = "";//App名字 App数据目录\
            private BundleVector mBundleList = null;
            private LoadTaskVector mBundleTaskList = null;
            private WaitingList mWaitLoadBundleList = null;
            public AssetBundleManifest Manifest { get; private set; }
            private bool mInited = false;
            #region PATH_LOADER
            private  string mStreamingDataPath = null;
            private  string mResourcesPath = null;
            private  string mPersistentDataPath = null;
            #endregion
            #endregion
            #region 路径获取

            public string GetResourcesDataPath(string _filename)
            {
                return Path.Combine(mResourcesPath, _filename);
            }

            public string GetFullPath(string _filename)
            {
                _filename = BaseBundle.CombineSuffixName(_filename);
                string tfullpathname = Path.Combine(mPersistentDataPath, _filename);
                if (!File.Exists(tfullpathname))
                    tfullpathname = Path.Combine(mStreamingDataPath, _filename);
                return tfullpathname;
            }
            #endregion

            #region 初始化,销毁,设置
            public void Init(string _appname, string _persistenpath, string _streamingpath, string _resources)
            {
                if (mInited) return;
                mInited = true;
                mPersistentDataPath = _persistenpath;
                mStreamingDataPath = _streamingpath;
                mResourcesPath = string.Format("{0}{1}", _resources, GameCore.ResDataPath).Replace("//", "/");
                SetAppName(_appname);

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
                    Object.DestroyImmediate(Manifest,true);
                mBundleTaskList.Clear();
                if (mWaitLoadBundleList.Count != 0)
                {
                    DLog.LogError(mAppName + ":删除LoaderManager时,发现仍然有未完成的加载动作.可能会对后续资源加载造成影响.");
                    for (int i = mWaitLoadBundleList.Count - 1; i >= 0; i--)
                    {
                        BaseBundle tbundle = mWaitLoadBundleList[i];
                        mBundleList.Remove(tbundle, false);
                        PublicUpdateManager.AddWaitLoadBundle(tbundle);
                    }
                }
                    
                mWaitLoadBundleList.Clear();
                RemoveAllAsset();
            }
            #endregion

            private void SetAppName(string _appname)
            {
                mAppName = _appname;
                AssetBundle tbundle = AssetBundle.LoadFromFile(GetFullPath(mAppName));
                if (tbundle != null)
                {
                    Manifest = tbundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                    tbundle.Unload(false);
                }
                else
                    DLog.LogErrorFormat("未能加载App资源列表 AppName = {0}" , mAppName);
                
            }
            #endregion

            #region update
            void Update()
            {
                if (mWaitLoadBundleList.Count > 0)
                {
                    for(int i = mWaitLoadBundleList.Count -1; i >= 0 ; i--)
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

            public string[] GetAllDependencies(string _assetBundleName)
            {
                if (Manifest == null) return null;
                return Manifest.GetAllDependencies(BaseBundle.CombineSuffixName(_assetBundleName));
            }
            public string[] GetDirectDependencies(string _assetBundleName)
            {
                if (Manifest == null) return null;
                return Manifest.GetDirectDependencies(BaseBundle.CombineSuffixName(_assetBundleName));
            }
            public string[] GetAllAssetBundles()
            {
                if (Manifest == null) return null;
                return Manifest.GetAllAssetBundles();
            }

            private void AddmWaitLoadList(BaseBundle _bundle)
            {
                mWaitLoadBundleList.Add(_bundle);
            }

            private void AddCache(BaseBundle _bundle)
            {
                mBundleList.Add(_bundle);
            }

            public void ReleaseAsset(string _key)
            {
                mBundleList.ReleaseBundle(_key);
            }

            private void RemoveAllAsset()
            {
                mBundleList.Clear();
            }

            public void RemoveAsset(string _AssetsName)
            {
                mBundleList.Remove(_AssetsName);
            }

            #endregion
            private LoadTask CreatTaskAndStart(string _key, BaseBundle _bundle, System.Action<string, object> _callback,bool _retain)
            {
                LoadTask ret = new LoadTask(_key, _bundle, _callback, _retain);
                mBundleTaskList.Add(ret);
                return ret;
            }

            #region 资源载入

            #region 同步载入
            #region Res资源
            /// <summary>
            /// 载入Resources资源
            /// </summary>
            /// <param name="_AssetsName">_curPathname 是相对于path/Date/下的路径 例如目录结构Assets/Resources/Date/ +_curPathname</param>
            /// <returns></returns>
            public Object LoadResources(string _AssetsName)
            {
                if (_AssetsName == null || _AssetsName.Equals("")) return null;
                _AssetsName = BaseBundle.DeleteSuffixName(_AssetsName).ToLower();
                if (mBundleList.Contains(_AssetsName))
                {
                    return (Object)mBundleList[_AssetsName].Retain();
                }

                ResourcesBundle tbundle = new ResourcesBundle(_AssetsName);
                AddCache(tbundle);
                tbundle.Load(this);
                return (Object)tbundle.Retain();
            }
            #endregion
            //使用前需要设置datapath 默认为 Data _assetname 
            public UnityEngine.Object LoadAsset(string _AssetsName)
            {
                return (UnityEngine.Object)LoadAssetRetain(_AssetsName).Retain();
            }

            private BaseBundle LoadAssetRetain(string _AssetsName)
            {
                if (_AssetsName == null || _AssetsName.Equals("")) return null;
                _AssetsName = BaseBundle.DeleteSuffixName(_AssetsName).ToLower();
                if (!mBundleList.Contains(_AssetsName))
                {
                    AssetsBundleHaveDependencie tbundle = new AssetsBundleHaveDependencie(_AssetsName, LoadAssetRetain);
                    AddCache(tbundle);
                    tbundle.Load(this); 
                }
                return mBundleList[_AssetsName];
            }
            #endregion
            #region 异步载入

            protected void LoadBundleAsync(BaseBundle _bundle,string _key, System.Action<string, object> _callback,bool _retain)
            {
                AddCache(_bundle);
                _bundle.Load(this);
                AddmWaitLoadList(_bundle);
                CreatTaskAndStart(_key, _bundle, _callback, _retain);
                ActiveLoader(true);
            }

            public void LoadResourcesAsync(string _key, string _AssetsName, System.Action<string, object> _callback)
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
                _AssetsName = BaseBundle.DeleteSuffixName(_AssetsName).ToLower();
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
                        CreatTaskAndStart(_key, mBundleList[_AssetsName], _callback,true);
                        ActiveLoader(true);
                    }

                }
                else
                {
                    LoadBundleAsync(new ResourcesBundleAsync(_AssetsName), _key, _callback,true);
                }
            }
            
            public void LoadAssetAsync(string _key, string _AssetsName, System.Action<string, object> _callback)
            {
                 LoadAssetAsyncRetain(_key, _AssetsName, _callback,true);
            }

            private BaseBundle LoadAssetAsyncRetain(string _key, string _AssetsName, System.Action<string, object> _callback, bool _retain )
            {
               
                if (_AssetsName.Length == 0)
                {
                    DLog.LogError( "LoadAssetsBundleByFullNameAsync -- _AssetsName 的长度不能为空");
                }
                if (_callback == null)
                {
                    DLog.LogError( "LoadAssetsBundleByFullNameAsync -- CallBack Fun can not be null");
                    return null;
                }
                _AssetsName = BaseBundle.DeleteSuffixName(_AssetsName).ToLower();
                if (mBundleList.Contains(_AssetsName))
                {
                    if (mBundleList[_AssetsName].Loaded)
                    {
                        if (mBundleList[_AssetsName].Asset == null)
                            DLog.LogError( "AssetsBundleAsyncFromFile-erro in vector。文件载入失败,请检查文件名:" + _AssetsName);
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
            public BaseBundle WWWLoad(string _key, string _FullName, System.Action<string, object> _callback)
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
                        CreatTaskAndStart(_key, mBundleList[_FullName], _callback,true);
                        ActiveLoader(true);
                    }

                }
                else
                {
                    LoadBundleAsync(new WWWBundle(_FullName), _key, _callback,true);
                }
                return mBundleList[_FullName];
            }
            #endregion
            #endregion
        }

    }
}


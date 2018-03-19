using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
namespace LitEngine
{
    using Loader;
    public class GameCore : CoreBase
    {
        public const string DataPath = "/Data/";//App总数据目录
        public const string ResDataPath = "/ResData/";//App资源目录
        public const string ConfigDataPath = "/ConfigData/";//App配置文件目录
        public const string ScriptDataPath = "/LogicDll/";//App配置文件目录
        #region static tool
        static public string GetDelegateAppName(System.Delegate _delgate)
        {
            if (_delgate == null) return "";
            if (_delgate.Method.DeclaringType.IsSubclassOf(typeof(ILRuntime.Runtime.Intepreter.DelegateAdapter)))
            {
                ILRuntime.Runtime.Intepreter.DelegateAdapter ttypeinstance = (ILRuntime.Runtime.Intepreter.DelegateAdapter)_delgate.Target;
                if (ttypeinstance != null)
                    return ttypeinstance.AppName;
            }
            return null;
        }
        #endregion
        #region static path获取
        static public string GetPersistentAppPath(string _appname)
        {
            return CombinePath(AppCore.persistentDataPath, DataPath, _appname);
        }
        static public string GetStreamingAssetsAppPath(string _appname)
        {
            return CombinePath(AppCore.streamingAssetsPath, DataPath, _appname);
        }
        static public string CombinePath(params object[] _params)
        {
            System.Text.StringBuilder tformatbuilder = new System.Text.StringBuilder();
            for (int i = 0; i < _params.Length; i++)
            {
                string tobjstr = _params[i].ToString();
                if(i!=0)
                    tobjstr = RemoveStartWithString(tobjstr, "/");
                tobjstr = RemoveEndWithString(tobjstr, "/");
                tformatbuilder.Append(tobjstr);
                tformatbuilder.Append("/");
            }
            return tformatbuilder.ToString();
        }

        static public string CombineFilePath(params object[] _params)
        {
            System.Text.StringBuilder tformatbuilder = new System.Text.StringBuilder();
            for (int i = 0; i < _params.Length; i++)
            {
                string tobjstr = _params[i].ToString();
                if (i != 0)
                    tobjstr = RemoveStartWithString(tobjstr, "/");
                tobjstr = RemoveEndWithString(tobjstr, "/");
                tformatbuilder.Append(tobjstr);
                if (i < _params.Length - 1)
                    tformatbuilder.Append("/");
            }
            return tformatbuilder.ToString();
        }

        static public string RemoveEndWithString(string _source,string _des)
        {
            if (string.IsNullOrEmpty(_des)) return _source;
            return _source.TrimEnd(_des.ToCharArray());
        }

        static public string RemoveStartWithString(string _source, string _des)
        {
            if (string.IsNullOrEmpty(_des)) return _source;
            return _source.TrimStart(_des.ToCharArray());
        }

        #endregion
        #region 类变量
        private AppCore mParentCore;
        protected bool mIsInited = false;
        private bool IsSceneLoading = false;
        public string AppName
        {
            get;
            private set;
        }
        public string AppPersistentDataPath { get; private set; }
        public string AppStreamingAssetsDataPath { get; private set; }
        public string AppResourcesDataPath { get; private set; }

        public string AppPersistentResDataPath { get; private set; }
        public string AppStreamingAssetsResDataPath { get; private set; }

        public string AppPersistentConfigDataPath { get; private set; }
        public string AppStreamingAssetsConfigDataPath { get; private set; }

        public string AppPersistentScriptDataPath { get; private set; }
        public string AppStreamingAssetsScriptDataPath { get; private set; }

        private List<UnityEngine.GameObject> mDontDestroyList = new List<UnityEngine.GameObject>();
        private List<ScriptInterface.BehaviourInterfaceBase> mScriptInterfaces = new List<ScriptInterface.BehaviourInterfaceBase>();
        #endregion

        #region 管理器
        public ScriptManager SManager
        {
            get;
            private set;
        }
        public LoaderManager LManager
        {
            get;
            private set;
        }
        public GameUpdateManager GManager
        {
            get;
            private set;
        }
        public CorotineManager CManager
        {
            get;
            private set;
        }
        public PlayAudioManager AudioManager
        {
            get;
            private set;
        }
        #endregion
        #region 初始化
        protected GameCore()
        {

        }
        protected GameCore(AppCore _core,string _appname)
        {
            AppName = _appname;
            mParentCore = _core;
        }
        private void InitGameCore(UseScriptType _scripttype)
        {
            if(!CheckInited(false))
            {
                DLog.LogError( "不允许重复初始化GameCore,请检查代码");
                return;
            }
            SetPath();

            GameObject tobj = new GameObject("GameUpdateManager-" + AppName);
            GameObject.DontDestroyOnLoad(tobj);
            GManager = tobj.AddComponent<GameUpdateManager>();

            tobj = new GameObject("PlayAudioManager-" + AppName);
            GameObject.DontDestroyOnLoad(tobj);
            AudioManager = tobj.AddComponent<PlayAudioManager>();

            tobj = new GameObject("LoaderManager-" + AppName);
            GameObject.DontDestroyOnLoad(tobj);
            LManager = tobj.AddComponent<LoaderManager>();
            LManager.Init(AppName, AppPersistentResDataPath, AppStreamingAssetsResDataPath, AppResourcesDataPath);

            CManager = new CorotineManager(AppName,this);
            SManager = new ScriptManager(AppName,_scripttype);

            mIsInited = true;
        }

        public bool CheckInited(bool _need)
        {
            if (mIsInited != _need)
            {
                DLog.LogError( string.Format("GameCore的初始化状态不正确:Inited = {0} needstate = {1}", mIsInited, _need));
                return false;
            }
            return true ;
        }


        #endregion
        #region 释放

        override protected void DisposeNoGcCode()
        {
            if (IsSceneLoading)
                DLog.LogError("异步加载场景未完成时,此时进行卸载GameCore操作,可能引起场景错乱.");
            //公用
            PublicUpdateManager.ClearByKey(AppName);
            NetTool.HttpNet.ClearByKey(AppName);
            NetTool.TCPNet.Instance.ClearAppDelgate(AppName);

            for (int i = mScriptInterfaces.Count - 1; i >= 0; i--)
            {
                ScriptInterface.BehaviourInterfaceBase tscript = mScriptInterfaces[i];
                if (tscript == null) continue;
                if (!tscript.mAppName.Equals(AppName)) continue;
                tscript.ClearScriptObject();
            }
            mScriptInterfaces.Clear();

            for (int i = mDontDestroyList.Count - 1;i >= 0;i--)
            {
                DestroyImmediate(mDontDestroyList[i]);
            }
            mDontDestroyList.Clear();

            GManager.DestroyManager();
            AudioManager.DestroyManager();
            LManager.DestroyManager();
            CManager.Dispose();
            SManager.Dispose();

            GManager = null;
            LManager = null;
            SManager = null;
            AudioManager = null;
            CManager = null;
        }
        #endregion
        #region 方法
        private void SetPath()
        {
            AppResourcesDataPath = CombinePath(DataPath, AppName);
            AppPersistentDataPath = GetPersistentAppPath(AppName);
            AppStreamingAssetsDataPath = GetStreamingAssetsAppPath(AppName);

            AppPersistentResDataPath = CombinePath(AppPersistentDataPath, ResDataPath);
            AppStreamingAssetsResDataPath = CombinePath(AppStreamingAssetsDataPath, ResDataPath);

            AppPersistentConfigDataPath = CombinePath(AppPersistentDataPath, ConfigDataPath);
            AppStreamingAssetsConfigDataPath = CombinePath(AppStreamingAssetsDataPath, ConfigDataPath);

            AppPersistentScriptDataPath = CombinePath(AppPersistentDataPath, ScriptDataPath);
            AppStreamingAssetsScriptDataPath = CombinePath(AppStreamingAssetsDataPath, ScriptDataPath);
        }
        #region interface
        public void AddScriptInterface(ScriptInterface.BehaviourInterfaceBase _scriptinterface)
        {
            if (mScriptInterfaces.Contains(_scriptinterface)) return;
            mScriptInterfaces.Add(_scriptinterface);
        }

        public void RemveScriptInterface(ScriptInterface.BehaviourInterfaceBase _scriptinterface)
        {
            if (!mScriptInterfaces.Contains(_scriptinterface)) return;
            mScriptInterfaces.Remove(_scriptinterface);
        }
        #endregion

        #region Object
        public void DontDestroyOnLoad(UnityEngine.GameObject _obj)
        {
            UnityEngine.Object.DontDestroyOnLoad(_obj);
            if (!mDontDestroyList.Contains(_obj))
                mDontDestroyList.Add(_obj);
        }
        public void DestroyObject(UnityEngine.GameObject _obj, float _t)
        {
            if (mDontDestroyList.Contains(_obj))
                mDontDestroyList.Remove(_obj);
            UnityEngine.GameObject.DestroyObject(_obj, _t);
        }
        public void DestroyImmediate(UnityEngine.GameObject _obj)
        {
            if (mDontDestroyList.Contains(_obj))
                mDontDestroyList.Remove(_obj);
            UnityEngine.GameObject.DestroyImmediate(_obj);
        }

        public void Destroy(UnityEngine.GameObject _obj)
        {
            if (mDontDestroyList.Contains(_obj))
                mDontDestroyList.Remove(_obj);
            UnityEngine.GameObject.Destroy(_obj);
        }
        #endregion

        #region Scene
        public void LoadScene(string _scenename)
        {
            if (IsSceneLoading)
            {
                DLog.LogError("The Scene is Loading.");
                return;
            }
            LManager.LoadAsset(_scenename);
            _scenename = _scenename.Replace(".unity", "");
            UnityEngine.SceneManagement.SceneManager.LoadScene(_scenename);
        }
        System.Action mLoadSceneCall = null;
        public void LoadSceneAsync(string _scenename,System.Action _FinishdCall)
        {
            if (IsSceneLoading)
            {
                DLog.LogError("The Scene is Loading.");
                return;
            }
            IsSceneLoading = true;
            mLoadSceneCall = _FinishdCall;
            LManager.LoadAssetAsync(_scenename, _scenename, LoadedStartScene);
        }

        private string mNowLoadingScene = null;
        private void LoadedStartScene(string _key, object _object)
        {
            mNowLoadingScene = _key.Replace(".unity", "");
            SceneManager.sceneLoaded += LoadSceneCall;
            AsyncOperation topert = SceneManager.LoadSceneAsync(mNowLoadingScene);

        }
        private void LoadSceneCall(Scene _scene, LoadSceneMode _mode)
        {
            if (!_scene.name.Equals(mNowLoadingScene)) return;
            if (mLoadSceneCall == null)
                mLoadSceneCall();
            mLoadSceneCall = null;
            SceneManager.sceneLoaded -= LoadSceneCall;
            mNowLoadingScene = null;
            IsSceneLoading = false;
        }

        #endregion

        #endregion
        #region Tool方法
        public void DownLoadFileAsync(string _sourceurl, string _destination, bool _IsClear, System.Action<string, string> _finished, System.Action<long, long, float> _progress)
        {
            PublicUpdateManager.DownLoadFileAsync(AppName, _sourceurl, _destination, _IsClear, _finished, _progress);
        }

        public void UnZipFileAsync(string _source, string _destination,System.Action<string> _finished, System.Action<float> _progress)
        {
            PublicUpdateManager.UnZipFileAsync(AppName, _source, _destination, _finished, _progress);
        }

        public void HttpSend(string _url, string _key, System.Action<string,string, byte[]> _delegate)
        {
            NetTool.HttpNet.Send(AppName, _url, _key, _delegate);
        }

        public void HttpSendHaveHeader(string _url, string _key,Dictionary<string,string> _headers, System.Action<string,string, byte[]> _delegate)
        {
            NetTool.HttpData tdata = new NetTool.HttpData(AppName, _key, _url, _delegate);

            if (tdata.Request.Headers == null) tdata.Request.Headers = new System.Net.WebHeaderCollection();
            foreach (KeyValuePair<string, string> tkey in _headers)
            {
                tdata.Request.Headers.Add(tkey.Key, tkey.Value);
            }

            NetTool.HttpNet.SendData(tdata);
        }
        #endregion
    }
}

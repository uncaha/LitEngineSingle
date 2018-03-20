using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
namespace LitEngine
{
    using Loader;
    public class GameCore
    {
        public const string DataPath = "/Data/";//App总数据目录
        public const string ResDataPath = "/ResData/";//App资源目录
        public const string ConfigDataPath = "/ConfigData/";//App配置文件目录
        public const string ScriptDataPath = "/LogicDll/";//App脚本

        private static GameCore sInstance = null;
        public static GameCore Core
        {
            get
            {
                if (sInstance == null)
                    sInstance = new GameCore();
                return sInstance;
            }
        }
        #region static path

        static private string sAppPersistentAssetsPath = null;
        static public string AppPersistentAssetsPath
        {
            get
            {
                if (sAppPersistentAssetsPath == null)
                {
                    if (UnityEngine.Application.platform != UnityEngine.RuntimePlatform.WindowsEditor && UnityEngine.Application.platform != UnityEngine.RuntimePlatform.WindowsPlayer)
                        sAppPersistentAssetsPath = string.Format("{0}/", UnityEngine.Application.persistentDataPath).Replace("//", "/");
                    else
                        sAppPersistentAssetsPath = string.Format("{0}", UnityEngine.Application.dataPath + "/../").Replace("//", "/");
                }
                return sAppPersistentAssetsPath;
            }
        }
        static public string AppStreamingAssetsPath
        {
            get
            {
                return UnityEngine.Application.streamingAssetsPath;
            }
        }

        static public string PersistentDataPath { get; private set; }
        static public string StreamingAssetsDataPath { get; private set; }
        static public string ResourcesDataPath { get; private set; }

        static public string PersistentResDataPath { get; private set; }
        static public string StreamingAssetsResDataPath { get; private set; }
        static public string ResourcesResDataPath { get; private set; }

        static public string PersistentConfigDataPath { get; private set; }
        static public string StreamingAssetsConfigDataPath { get; private set; }
        static public string ResourcesConfigDataPath { get; private set; }

        static public string PersistentScriptDataPath { get; private set; }
        static public string StreamingAssetsScriptDataPath { get; private set; }
        static public string ResourcesScriptDataPath { get; private set; }

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
        protected bool mIsInited = false;
        public ScriptManager SManager
        {
            get;
            private set;
        }
        #endregion



        #region 初始化
        protected GameCore()
        {
            SetPath();
        }
        static public void InitGameCore(UseScriptType _scripttype)
        {
            if(Core.mIsInited)
            {
                DLog.LogError( "不允许重复初始化GameCore,请检查代码");
                return;
            }

            Core.SManager = new ScriptManager(_scripttype);
            Core.mIsInited = true;
        }

        #endregion
        #region 方法
        private void SetPath()
        {
            ResourcesDataPath = DataPath;
            PersistentDataPath = CombinePath(AppPersistentAssetsPath, DataPath);
            StreamingAssetsDataPath = CombinePath(AppStreamingAssetsPath, DataPath);

            PersistentResDataPath = CombinePath(PersistentDataPath, ResDataPath);
            StreamingAssetsResDataPath = CombinePath(StreamingAssetsDataPath, ResDataPath);
            ResourcesResDataPath = CombinePath(ResourcesDataPath, ResDataPath);

            PersistentConfigDataPath = CombinePath(PersistentDataPath, ConfigDataPath);
            StreamingAssetsConfigDataPath = CombinePath(StreamingAssetsDataPath, ConfigDataPath);
            ResourcesConfigDataPath = CombinePath(ResourcesDataPath, ConfigDataPath);

            PersistentScriptDataPath = CombinePath(PersistentDataPath, ScriptDataPath);
            StreamingAssetsScriptDataPath = CombinePath(StreamingAssetsDataPath, ScriptDataPath);
            ResourcesScriptDataPath = CombinePath(ResourcesDataPath, ScriptDataPath);
        }

        #region Scene
        private static bool IsSceneLoading = false;
        public void LoadScene(string _scenename)
        {
            if (IsSceneLoading)
            {
                DLog.LogError("The Scene is Loading.");
                return;
            }
            LoaderManager.LoadAsset(_scenename);
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
            LoaderManager.LoadAssetAsync(_scenename, _scenename, LoadedStartScene);
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
    }
}

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
namespace LitEngine
{
    using Loader;
    using IO;
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

        static public string PersistentDataPath { get { return Core.mPersistentDataPath; } }
        static public string StreamingAssetsDataPath { get { return Core.mStreamingAssetsDataPath; } }
        static public string ResourcesDataPath { get { return Core.mResourcesDataPath; } }

        static public string PersistentResDataPath { get { return Core.mPersistentResDataPath; } }
        static public string StreamingAssetsResDataPath { get { return Core.mStreamingAssetsResDataPath; } }
        static public string ResourcesResDataPath { get { return Core.mResourcesResDataPath; } }

        static public string PersistentConfigDataPath { get { return Core.mPersistentConfigDataPath; } }
        static public string StreamingAssetsConfigDataPath { get { return Core.mStreamingAssetsConfigDataPath; } }
        static public string ResourcesConfigDataPath { get { return Core.mResourcesConfigDataPath; } }

        static public string PersistentScriptDataPath { get { return Core.mPersistentScriptDataPath; } }
        static public string StreamingAssetsScriptDataPath { get { return Core.mStreamingAssetsScriptDataPath; } }
        static public string ResourcesScriptDataPath { get { return Core.mResourcesScriptDataPath; } }

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

        protected string mPersistentDataPath;
        protected string mStreamingAssetsDataPath;
        protected string mResourcesDataPath;

        protected string mPersistentResDataPath;
        protected string mStreamingAssetsResDataPath;
        protected string mResourcesResDataPath;

        protected string mPersistentConfigDataPath;
        protected string mStreamingAssetsConfigDataPath;
        protected string mResourcesConfigDataPath;

        protected string mPersistentScriptDataPath;
        protected string mStreamingAssetsScriptDataPath;
        protected string mResourcesScriptDataPath;
        protected ScriptManager mScriptManager;
        #endregion

        static public CodeToolBase CodeTool { get { return Core.mScriptManager.CodeTool; } }

        #region 初始化
        protected GameCore()
        {
            SetPath();
        }
        static public void InitGameCore(UseScriptType _scripttype,string _filename)
        {
            if(Core.mIsInited)
            {
                DLog.LogError( "不允许重复初始化GameCore,请检查代码");
                return;
            }

            Core.mScriptManager = new ScriptManager(_scripttype);
            Core.mScriptManager.LoadScriptFile(_filename);
            Core.mIsInited = true;
        }

        #endregion
        #region 方法
        private void SetPath()
        {
            mResourcesDataPath = DataPath.Replace("/","");
            mPersistentDataPath = CombinePath(AppPersistentAssetsPath, DataPath);
            mStreamingAssetsDataPath = CombinePath(AppStreamingAssetsPath, DataPath);

            mPersistentResDataPath = CombinePath(mPersistentDataPath, ResDataPath);
            mStreamingAssetsResDataPath = CombinePath(mStreamingAssetsDataPath, ResDataPath);
            mResourcesResDataPath = CombinePath(mResourcesDataPath, ResDataPath);

            mPersistentConfigDataPath = CombinePath(mPersistentDataPath, ConfigDataPath);
            mStreamingAssetsConfigDataPath = CombinePath(mStreamingAssetsDataPath, ConfigDataPath);
            mResourcesConfigDataPath = CombinePath(mResourcesDataPath, ConfigDataPath);

            mPersistentScriptDataPath = CombinePath(mPersistentDataPath, ScriptDataPath);
            mStreamingAssetsScriptDataPath = CombinePath(mStreamingAssetsDataPath, ScriptDataPath);
            mResourcesScriptDataPath = CombinePath(mResourcesDataPath, ScriptDataPath);
        }
        static public object GetScriptObject(string _classname, params object[] _parmas)
        {
            return GameCore.CodeTool.GetCSLEObjectParmas(_classname);
        }

        static public object CallMethodByName(string _name, object _this, params object[] _params)
        {
            return GameCore.CodeTool.CallMethodByName(_name, _this, _params);
        }

        static public object GetCSLEObjectParmasByType(ILRuntime.CLR.TypeSystem.IType _type,object _object, params object[] _parmas)
        {
            return GameCore.CodeTool.GetCSLEObjectParmasByType(_type, _object);
        }
        #endregion
    }
}

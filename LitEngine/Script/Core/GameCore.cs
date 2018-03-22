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
        static public void InitGameCore(UseScriptType _scripttype,string _filename)
        {
            if(Core.mIsInited)
            {
                DLog.LogError( "不允许重复初始化GameCore,请检查代码");
                return;
            }

            Core.SManager = new ScriptManager(_scripttype);
            Core.InitScriptFile(_filename);
            Core.mIsInited = true;
        }

        private void InitScriptFile(string _filename)
        {
            if (string.IsNullOrEmpty(_filename)) return;
            byte[] dllbytes = null;
            byte[] pdbbytes = null;
            string tdllPath = PersistentScriptDataPath + _filename;
            if (System.IO.File.Exists(tdllPath + ".dll"))
            {
                dllbytes = System.IO.File.ReadAllBytes(tdllPath + ".dll");
                pdbbytes = System.IO.File.ReadAllBytes(tdllPath + ".pdb");
            }

            if (dllbytes == null || pdbbytes == null)
            {
                DLog.LogErrorFormat("LoadScriptFormFile{dllbytes = {0},pdbbytes = {1}}", dllbytes, pdbbytes);
                return;
            }

            AESReader tdllreader = new AESReader(dllbytes);
            AESReader tpdbreader = new AESReader(pdbbytes);

            dllbytes = tdllreader.ReadAllBytes();
            pdbbytes = tpdbreader.ReadAllBytes();

            tdllreader.Dispose();
            tpdbreader.Dispose();

            SManager.LoadProjectByBytes(dllbytes, pdbbytes);
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
        static public object GetScriptObject(string _classname, params object[] _parmas)
        {
            return Core.SManager.CodeTool.GetCSLEObjectParmas(_classname);
        }

        static public object CallMethodByName(string _name, object _this, params object[] _params)
        {
            return Core.SManager.CodeTool.CallMethodByName(_name, _this, _params);
        }
        #endregion
    }
}

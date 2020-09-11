using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using LitEngine.CodeTool;
namespace LitEngine
{
    using IO;
    public sealed class GameCore
    {
        public const string ExportPath = "Assets/ExportResources";
        public const string DataPath = "Data";//App总数据目录
        public const string ResDataPath = "ResData";//App资源目录
        public const string ConfigDataPath = "Config";//配置文件目录


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
        static public bool IsEditor
        {
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.LinuxEditor:
                    case RuntimePlatform.OSXEditor:
                    case RuntimePlatform.WindowsEditor:
                        return true;
                    default:
                        return false;
                }
            }
        }

        static private string sAppPersistentAssetsPath = null;
        static public string AppPersistentAssetsPath
        {
            get
            {
                if (sAppPersistentAssetsPath == null)
                {
                    switch (Application.platform)
                    {
                        case RuntimePlatform.OSXEditor:
                        case RuntimePlatform.WindowsEditor:
                        case RuntimePlatform.LinuxEditor:
                            sAppPersistentAssetsPath = string.Format("{0}/../", UnityEngine.Application.dataPath).Replace("//", "/");
                            break;
                        case RuntimePlatform.WindowsPlayer:
                            sAppPersistentAssetsPath = string.Format("{0}/", UnityEngine.Application.dataPath).Replace("//", "/");
                            break;
                        default:
                            sAppPersistentAssetsPath = string.Format("{0}/", UnityEngine.Application.persistentDataPath).Replace("//", "/");
                            break;
                    }


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
        static public string PersistentResDataPath { get { return Core.mPersistentResDataPath; } }

        static public string StreamingAssetsDataPath { get { return Core.mStreamingAssetsDataPath; } }
        static public string StreamingAssetsResDataPath { get { return Core.mStreamingAssetsResDataPath; } }

        #endregion
        #region 类变量
        private bool mIsInited = false;

        private string mPersistentDataPath;
        private string mPersistentResDataPath;

        private string mStreamingAssetsDataPath;
        private string mStreamingAssetsResDataPath;


        private ScriptManager mScriptManager;
        #endregion

        static public CodeToolBase CodeTool { get { return Core.mScriptManager.CodeTool; } }

        #region 初始化
        private GameCore()
        {
            SetPath();
        }
        static public void InitGameCore(CodeToolBase _codeTool)
        {
            if (Core.mIsInited)
            {
                DLog.LogError("不允许重复初始化GameCore,请检查代码");
                return;
            }

            Core.SetPath();

            Core.mScriptManager = new ScriptManager(_codeTool);
            Core.mIsInited = true;
        }

        #endregion
        #region 方法
        private void SetPath()
        {
            mPersistentDataPath = string.Format("{0}/{1}/", AppPersistentAssetsPath, DataPath).Replace("//", "/");
            mStreamingAssetsDataPath = string.Format("{0}/{1}/", AppStreamingAssetsPath, DataPath).Replace("//", "/");

            mPersistentResDataPath = string.Format("{0}/{1}/", mPersistentDataPath, ResDataPath).Replace("//", "/");
            mStreamingAssetsResDataPath = string.Format("{0}/{1}/", mStreamingAssetsDataPath, ResDataPath).Replace("//", "/");
        }
        static public object GetScriptObject(string _classname, params object[] _parmas)
        {
            return GameCore.CodeTool.GetObject(_classname, _parmas);
        }

        #endregion
    }
}

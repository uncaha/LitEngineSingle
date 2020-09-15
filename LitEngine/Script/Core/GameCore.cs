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
                            sAppPersistentAssetsPath = CombinePath(UnityEngine.Application.dataPath, "../");
                            break;
                        case RuntimePlatform.WindowsPlayer:
                            sAppPersistentAssetsPath = CombinePath(UnityEngine.Application.dataPath, "");
                            break;
                        default:
                            sAppPersistentAssetsPath = CombinePath(UnityEngine.Application.persistentDataPath, "");
                            break;
                    }


                }
                return sAppPersistentAssetsPath;
            }
        }

        static private string sAppStreamingAssetsPath = null;
        static public string AppStreamingAssetsPath
        {
            get
            {
                if(sAppStreamingAssetsPath == null)
                {
                    sAppStreamingAssetsPath = CombinePath(UnityEngine.Application.streamingAssetsPath,"");
                }
                return sAppStreamingAssetsPath;
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
            mPersistentDataPath = CombinePath(AppPersistentAssetsPath,DataPath,"");
            mStreamingAssetsDataPath = CombinePath(AppStreamingAssetsPath,DataPath,"");

            mPersistentResDataPath = CombinePath(mPersistentDataPath,ResDataPath,"");
            mStreamingAssetsResDataPath = CombinePath(mStreamingAssetsDataPath,ResDataPath,"");
        }
        static public object GetScriptObject(string _classname, params object[] _parmas)
        {
            return GameCore.CodeTool.GetObject(_classname, _parmas);
        }

        static public string FormatPath(string pPath)
        {
            return string.Join("/", pPath.Replace("\\", "/").Split('/'));
        }

        static System.Text.StringBuilder cCombineBuilder = new System.Text.StringBuilder();
        static public string CombinePath(params string[] paths)
        {
            if(paths == null || paths.Length == 0) return null;
            cCombineBuilder.Clear();
            for (int i = 0, length = paths.Length; i < length; i++)
            {
                bool thavenext = i + 1 < length;
                string item = paths[i];
                string next = thavenext ? paths[i + 1] : "";
                bool thv = item.EndsWith("/");
                bool tnexthv = !thavenext || next.StartsWith("/");

                cCombineBuilder.Append(item);

                if (!thv && !tnexthv)
                {
                    cCombineBuilder.Append("/");
                }
            }

            return cCombineBuilder.ToString();
        }

        #endregion
    }
}

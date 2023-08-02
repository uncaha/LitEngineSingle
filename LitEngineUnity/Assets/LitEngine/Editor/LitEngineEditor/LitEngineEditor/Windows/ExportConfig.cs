using UnityEngine;
using UnityEditor;
using System.Text;
using System.Security;
using System.Collections;
using System.IO;
namespace LitEngineEditor
{
    public class ExportConfig 
    {
        #region datepath
        public readonly string sDefaultFolder = "Assets/BundlesResources/Data/"; // 默认导出路径,统一不可更改
        public readonly string sResourcesPath = "Assets/ExportResources/"; //需要导出的资源文件夹于GameCore路径对应
        public readonly string sEditorBundleFolder = "Assets/../Data/"; //编辑器工程外部资源路径
        public readonly string sStreamingBundleFolder = "Assets/StreamingAssets/Data/";//内部资源路径
        public const string sResDataPath = "/ResData/";//app资源路径,固定分级
        #endregion

        
        public static string GetTartFolder(BuildTarget _target)
        {
            return string.Format("/{0}/", _target.ToString());
        }

        public ExportConfig()
        {
            
        }


        public void OnGUI()
        {
            GUILayout.Label("Config:", EditorStyles.boldLabel);
            StringBuilder tbuilder = new StringBuilder();
            tbuilder.AppendLine(string.Format("[Platm]:{0}", ExportObject.sPlatformList[ExportSetting.Instance.sSelectedPlatm]));
            tbuilder.AppendLine(string.Format("[Compressed]:{0}", ExportSetting.Instance.sCompressed == 0 ? true : false));
            tbuilder.AppendLine(string.Format("[ResourcesPath]:{0}", sResourcesPath));
            tbuilder.AppendLine(string.Format("[ExportPath]:{0}/{1}", sDefaultFolder, GetTartFolder(ExportObject.sBuildTarget[ExportSetting.Instance.sSelectedPlatm])).Replace("//", "/"));
            tbuilder.AppendLine(string.Format("[SidePath]:{0}/{1}", sEditorBundleFolder, sResDataPath).Replace("//", "/"));
            tbuilder.AppendLine(string.Format("[StreamingPath]:{0}/{1}", sStreamingBundleFolder,sResDataPath).Replace("//","/"));

            GUILayout.Box(tbuilder.ToString(), EditorStyles.textField);
        }
    }
}

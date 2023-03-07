using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using LitEngine.ScriptInterface;
using LitEngine;
using System.Text;
using ExportTool;
namespace LitEngineEditor
{
    public class ExportExcelWiindow : ExportBase
    {
        private Vector2 mScrollPosition = new Vector2(0,40);
        private StringBuilder mContext = new StringBuilder();
        protected string filestag = "*.*";
        public ExportExcelWiindow() : base()
        {
            ExWType = ExportWType.ExcelWindow;
            RestFileList();
        }
        override public void OnGUI()
        {
            GUILayout.Label("Tips:");
            GUILayout.Label("line 1 = server or client, line 2 = context, line 3 = type, line 4 = fieldname");

            mScrollPosition = PublicGUI.DrawScrollview("Files", mContext.ToString(), mScrollPosition, mWindow.position.size.x, 160);
            
            //excel目录
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Ex:", GUILayout.Width(35));
            EditorGUILayout.TextField("", ExportSetting.Instance.sExcelPath, EditorStyles.textField);
            if (GUILayout.Button("...", GUILayout.Width(25)))
            {
                string toldstr = ExportSetting.Instance.sExcelPath;
                toldstr = EditorUtility.OpenFolderPanel("file Path", toldstr, "");
                if (!string.IsNullOrEmpty(toldstr) && !toldstr.Equals(ExportSetting.Instance.sExcelPath))
                {
                    ExportSetting.Instance.sExcelPath = toldstr;
                    NeedSaveSetting();

                    RestFileList();
                }

            }
            EditorGUILayout.EndHorizontal();

            //导出byte目录
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("DB:", GUILayout.Width(35));
            EditorGUILayout.TextField("", ExportSetting.Instance.sExcelBytesPath, EditorStyles.textField);
            if (GUILayout.Button("...", GUILayout.Width(25)))
            {
                string toldstr = ExportSetting.Instance.sExcelBytesPath;
                toldstr = EditorUtility.OpenFolderPanel("file Path", toldstr, "");
                if (!string.IsNullOrEmpty(toldstr) && !toldstr.Equals(ExportSetting.Instance.sExcelBytesPath))
                {
                    ExportSetting.Instance.sExcelBytesPath = toldstr;
                    NeedSaveSetting();
                }

            }
            EditorGUILayout.EndHorizontal();

            //导出c#目录
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("C#:", GUILayout.Width(35));
            EditorGUILayout.TextField("", ExportSetting.Instance.sExcelSharpPath, EditorStyles.textField);
            if (GUILayout.Button("...", GUILayout.Width(25)))
            {
                string toldstr = ExportSetting.Instance.sExcelSharpPath;
                toldstr = EditorUtility.OpenFolderPanel("file Path", toldstr, "");
                if (!string.IsNullOrEmpty(toldstr) && !toldstr.Equals(ExportSetting.Instance.sExcelSharpPath))
                {
                    ExportSetting.Instance.sExcelSharpPath = toldstr;
                    NeedSaveSetting();
                }

            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Export Bytes"))
            {
                WriteBytesFile();
            }

            if (GUILayout.Button("Export C#"))
            {
                WriteShapFile();
            }
            
            if (GUILayout.Button("Export Json"))
            {
                WriteJsonFile();
            }
            
            if (GUILayout.Button("Move To Resources"))
            {
                MoveToResources();
            }
            
            if (GUILayout.Button("Move To ExportFolder"))
            {
                MoveToExportFolder();
            }
        }
        
        string[] GetExcelFiles()
        {
           
            string[] tfiles = Directory.GetFiles(ExportSetting.Instance.sExcelPath, "*.*", SearchOption.AllDirectories);

            List<string> tlist = new List<string>();

            foreach (var cur in tfiles)
            {
                if (cur.EndsWith(".xls") || cur.EndsWith(".xlsx"))
                {
                    tlist.Add(cur);
                }
            }

            
            return tlist.ToArray();
        }

        public void RestFileList()
        {
            if (!string.IsNullOrEmpty(ExportSetting.Instance.sExcelPath) && Directory.Exists(ExportSetting.Instance.sExcelPath))
            {
                mContext.Remove(0, mContext.Length);
                string[] files = Directory.GetFiles(ExportSetting.Instance.sExcelPath,filestag, SearchOption.AllDirectories);
                foreach (string filename in files)
                {
                    AddContext(filename);
                }
            }
        }

        public void AddContext(string _text)
        {
            lock (mContext)
            {
                mContext.AppendLine(_text);
            }

        }

        private void WriteBytesFile()
        {
            if (string.IsNullOrEmpty(ExportSetting.Instance.sExcelBytesPath)) return;
            if (EditorUtility.DisplayDialog("Export To Bytes", " Start Export?", "ok", "cancel"))
            {
                string[] files = GetExcelFiles();
                foreach (string filename in files)
                {
                    ExcelClass texcel = new ExcelClass(filename, ExportSetting.Instance.sExcelBytesPath);
                    texcel.SaveFile();
                    texcel.Close();
                }
                DLog.LogFormat("Complete  Export Data .filecount = {0}", files.Length);
                UnityEditor.AssetDatabase.Refresh();
            }
        }
        
        private void WriteShapFile()
        {
            if (string.IsNullOrEmpty(ExportSetting.Instance.sExcelSharpPath)) return;
            if (EditorUtility.DisplayDialog("Export C#", " Start Export C#?", "ok", "cancel"))
            {
                string[] files = GetExcelFiles();
                List<string> tcfgs = new List<string>();
                foreach (string filename in files)
                {
                    ExcelClass texcel = new ExcelClass(filename, ExportSetting.Instance.sExcelSharpPath);
                    List<string> tsnames = texcel.ExportReadClass();
                    texcel.Close();
                    tcfgs.AddRange(tsnames);
                }

                ExportConfigManager tem = new ExportConfigManager(ExportSetting.Instance.sExcelSharpPath + "/ConfigManager.cs", tcfgs);
                tem.StartExport();

                DLog.LogFormat("Complete  Export C# .filecount = {0}", files.Length);
                //UnityEditor.AssetDatabase.Refresh();
            }
        }
        
        private void WriteJsonFile()
        {
            if (string.IsNullOrEmpty(ExportSetting.Instance.sExcelBytesPath)) return;
            if (EditorUtility.DisplayDialog("Export To Bytes", " Start Export?", "ok", "cancel"))
            {
                string[] files = GetExcelFiles();
                foreach (string filename in files)
                {
                    ExcelClass texcel = new ExcelClass(filename, ExportSetting.Instance.sExcelBytesPath);
                    texcel.SaveToJson();
                    texcel.Close();
                }
                DLog.LogFormat("Complete  Export Data .filecount = {0}", files.Length);
                UnityEditor.AssetDatabase.Refresh();
            }
        }

        private void MoveToResources()
        {
            var tpath = ExportSetting.Instance.sExcelBytesPath;
            var tdespath = ExportObject.GetFormatPath($"{System.IO.Directory.GetCurrentDirectory()}/Assets/Resources/{GameCore.ConfigDataPath}");

            ExportObject.MoveToPath(tpath, tdespath, "");
            AssetDatabase.Refresh();
        }
        
        private void MoveToExportFolder()
        {
            var tpath = ExportSetting.Instance.sExcelBytesPath;
            var tdespath =
                $"{System.IO.Directory.GetCurrentDirectory()}/{GameCore.ExportPath}/{GameCore.ConfigDataPath}";

            ExportObject.MoveToPath(tpath, tdespath, "");
            AssetDatabase.Refresh();
        }
    }

}

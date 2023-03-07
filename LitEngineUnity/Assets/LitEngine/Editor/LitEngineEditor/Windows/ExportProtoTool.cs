// using UnityEngine;
// using UnityEditor;
// using System.Text;
// using ProtoBuf.Reflection;
// using Google.Protobuf.Reflection;
// using System.IO;
// namespace LitEngineEditor
// {
//     public class ExportProtoTool : ExportBase
//     {
//         private Vector2 mScrollPosition = Vector2.zero;
//         private StringBuilder mContext = new StringBuilder();
//
//         public ExportProtoTool():base()
//         {
//             ExWType = ExportWType.PrptoWindow;
//         }
//         override public void OnGUI()
//         {
//             mScrollPosition = PublicGUI.DrawScrollview("Console",mContext.ToString(), mScrollPosition, mWindow.position.size.x, 130);
//
//             EditorGUILayout.BeginHorizontal();
//             GUILayout.Label("SuperClass:", GUILayout.Width(80));
//             ExportSetting.Instance.sProtoClassString = EditorGUILayout.TextField("", ExportSetting.Instance.sProtoClassString, EditorStyles.textField);
//             EditorGUILayout.EndHorizontal();
//
//             EditorGUILayout.BeginHorizontal();
//             GUILayout.Label("ProtoFilePath:", GUILayout.Width(80));
//             EditorGUILayout.TextField("", ExportSetting.Instance.sProtoFilePath, EditorStyles.textField);
//             if (GUILayout.Button("...", GUILayout.Width(25)))
//             {
//                 string toldpath = ExportSetting.Instance.sProtoFilePath;
//                 toldpath = EditorUtility.OpenFolderPanel("Proto file Path", toldpath, "");
//                 if (!string.IsNullOrEmpty(toldpath) && !toldpath.Equals(ExportSetting.Instance.sProtoFilePath))
//                 {
//                     ExportSetting.Instance.sProtoFilePath = toldpath;
//                     NeedSaveSetting();
//                 }
//                     
//             }
//             EditorGUILayout.EndHorizontal();
//
//             
//             EditorGUILayout.BeginHorizontal();
//             GUILayout.Label("ExportPath:", GUILayout.Width(80));
//             EditorGUILayout.TextField("", ExportSetting.Instance.sCSFilePath, EditorStyles.textField);
//             if (GUILayout.Button("...", GUILayout.Width(25)))
//             {
//                 string toldpath = ExportSetting.Instance.sCSFilePath;
//                 toldpath = EditorUtility.OpenFolderPanel("CS file Path", toldpath, "");
//                 if (!string.IsNullOrEmpty(toldpath) && !toldpath.Equals(ExportSetting.Instance.sCSFilePath))
//                 {
//                     ExportSetting.Instance.sCSFilePath = toldpath;
//                     NeedSaveSetting();
//                 }
//                    
//             }
//             EditorGUILayout.EndHorizontal();
//             EditorGUILayout.Space();
//             if (GUILayout.Button("To cs file"))
//             {
//                 if (EditorUtility.DisplayDialog("Export", " Start export cs file?", "ok","cancel"))
//                 {
//                     ExportCSFile();
//                     NeedSaveSetting();
//                     UnityEditor.AssetDatabase.Refresh();
//                 }
//             }
//         }
//
//         public void AddContext(string _text)
//         {
//             lock(mContext)
//             {
//                 mContext.AppendLine(string.Format("[{0}]{1}", System.DateTime.Now, _text));
//             }
//            
//         }
//         public void AddSpace()
//         {
//             lock (mContext)
//             {
//                 mContext.AppendLine();
//             }
//             
//         }
//
//         private void ExportCSFile()
//         {
//             AddContext("Start Export:");
//
//             int exitCode = 0;
//             var codegen = ILCodeGenerator.Default;
//             codegen.superClassString = ExportSetting.Instance.sProtoClassString;
//
//             var set = new FileDescriptorSet();
//             set.AddImportPath(ExportSetting.Instance.sProtoFilePath);
//             string[] tpaths = Directory.GetDirectories(ExportSetting.Instance.sProtoFilePath);
//             foreach (string tp in tpaths)
//             {
//                 set.AddImportPath(tp);
//             }
//
//             DirectoryInfo tdirfolder = new DirectoryInfo(ExportSetting.Instance.sProtoFilePath);
//             FileInfo[] tfileinfos = tdirfolder.GetFiles("*.proto", System.IO.SearchOption.AllDirectories);
//             foreach (var input in tfileinfos)
//             {
//                 if (!set.Add(input.Name, true))
//                 {
//                     AddContext($"File not found: {input}");
//                     exitCode = 1;
//                 }
//             }
//             set.Process();
//             var errors = set.GetErrors();
//             foreach (var err in errors)
//             {
//                 if (err.IsError) exitCode++;
//                 AddContext(err.ToString());
//             }
//             if (exitCode != 0) return;
//
//             var files = codegen.Generate(set);
//             foreach (var file in files)
//             {
//                 var path = Path.Combine(ExportSetting.Instance.sCSFilePath, file.Name);
//                 File.WriteAllText(path, file.Text);
//             }
//
//             AddContext("Export End.");
//             AddSpace();
//         }
//     }
// }

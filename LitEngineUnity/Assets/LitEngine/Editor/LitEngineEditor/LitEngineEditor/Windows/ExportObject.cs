using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using LitEngine.ScriptInterface;
using LitEngine;
using System.Text;
using LitEngine.LoadAsset;
using LitEngine.Tool;

namespace LitEngineEditor
{

    public class ExportObject : ExportBase
    {
        const string sByteFileInfo = "bytefileInfo.txt";
        public static readonly string[] sPlatformList = new string[] { "Android", "iOS", "Windows64" };
        public static readonly BuildTarget[] sBuildTarget = { BuildTarget.Android, BuildTarget.iOS, BuildTarget.StandaloneWindows64 };

        public static string[] sCompressed = new string[2] { "Compressed", "UnCompressed" };
        public static string[] sBuildType = new string[4] { "every", "depInOne", "allInOne", "folder" };
        public static string[] sPathType = new string[2] { "onlyName", "FullPath" };
        private static readonly BuildAssetBundleOptions[] sBuildOption = { BuildAssetBundleOptions.ChunkBasedCompression, BuildAssetBundleOptions.UncompressedAssetBundle };
        public ExportObject() : base()
        {
            ExWType = ExportWType.AssetsWindow;
        }

        override public void OnGUI()
        {
            GUILayout.Label("Platform", EditorStyles.boldLabel);
            int oldSelectedPlatm = ExportSetting.Instance.sSelectedPlatm;
            int oldcompressed = ExportSetting.Instance.sCompressed;
            int oldsBuildType = ExportSetting.Instance.sBuildType;
            int oldssPathType = ExportSetting.Instance.sPathType;

            ExportSetting.Instance.sSelectedPlatm = GUILayout.SelectionGrid(ExportSetting.Instance.sSelectedPlatm, sPlatformList, 3);
            ExportSetting.Instance.sCompressed = GUILayout.SelectionGrid(ExportSetting.Instance.sCompressed, sCompressed, 2);
            ExportSetting.Instance.sBuildType = GUILayout.SelectionGrid(ExportSetting.Instance.sBuildType, sBuildType, 4);
            //ExportSetting.Instance.sPathType = GUILayout.SelectionGrid(ExportSetting.Instance.sPathType, sPathType, 2);

            if (oldSelectedPlatm != ExportSetting.Instance.sSelectedPlatm
                || oldcompressed != ExportSetting.Instance.sCompressed
                || oldsBuildType != ExportSetting.Instance.sBuildType
                || oldssPathType != ExportSetting.Instance.sPathType
                )
                NeedSaveSetting();

            Config.OnGUI();

            if (GUILayout.Button("Export Assets"))
            {
                if (ExportSetting.Instance.sPathType == 0)
                {
                    ExportAllBundle(sBuildTarget[ExportSetting.Instance.sSelectedPlatm]);
                }
                else
                {
                    ExportAllBundleFullPath(sBuildTarget[ExportSetting.Instance.sSelectedPlatm]);
                }

            }

            if (GUILayout.Button("Creat Infos"))
            {
                var ttar = sBuildTarget[ExportSetting.Instance.sSelectedPlatm];
                string tpath = GetExportPath(ttar);
                BuildByteFileInfoFile(tpath, tpath, ttar);
                AssetDatabase.Refresh();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Move Assets to SidePath"))
            {
                MoveBundleToSideDate(sBuildTarget[ExportSetting.Instance.sSelectedPlatm]);
            }
            if (GUILayout.Button("Move Assets to StreamPath"))
            {
                MoveBUndleToStreamingPath(sBuildTarget[ExportSetting.Instance.sSelectedPlatm]);
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Move to"))
            {
                string toldpath = ExportSetting.Instance.sMoveAssetsFilePath;
                toldpath = EditorUtility.OpenFolderPanel("Move to path", toldpath, "");
                if (!string.IsNullOrEmpty(toldpath) && !toldpath.Equals(ExportSetting.Instance.sMoveAssetsFilePath))
                {
                    ExportSetting.Instance.sMoveAssetsFilePath = toldpath;
                    NeedSaveSetting();
                }
                if (string.IsNullOrEmpty(ExportSetting.Instance.sMoveAssetsFilePath)) return;

                BuildTarget _target = sBuildTarget[ExportSetting.Instance.sSelectedPlatm];
                string tpath = GetExportPath(_target);
                MoveToPath(tpath, ExportSetting.Instance.sMoveAssetsFilePath, ExportConfig.GetTartFolder(_target));
            }
        }

        #region export

        public static void ExportAllBundle(BuildTarget _target)
        {
            string tpath = Config.sDefaultFolder + ExportConfig.GetTartFolder(_target);

            List<AssetBundleBuild> builds = new List<AssetBundleBuild>();

            DirectoryInfo tdirfolder = new DirectoryInfo(Config.sResourcesPath);
            FileInfo[] tfileinfos = tdirfolder.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

            bool tisCanExport = false;
            //  Dictionary<string, ExObject> tfiledic = new Dictionary<string, ExObject>();
            Dictionary<string, List<string>> tHavedfiledic = new Dictionary<string, List<string>>();
            Dictionary<string, string> tfiledic = new Dictionary<string, string>();
            foreach (FileInfo tfile in tfileinfos)
            {
                if (tfile.Name.EndsWith(".meta")) continue;
                if (tfiledic.ContainsKey(tfile.Name))
                {
                    if (!tHavedfiledic.ContainsKey(tfile.Name))
                        tHavedfiledic.Add(tfile.Name, new List<string>());

                    if (!tHavedfiledic[tfile.Name].Contains(tfiledic[tfile.Name]))
                        tHavedfiledic[tfile.Name].Add(tfiledic[tfile.Name]);

                    if (!tHavedfiledic[tfile.Name].Contains(tfile.FullName))
                        tHavedfiledic[tfile.Name].Add(tfile.FullName);

                    tisCanExport = true;
                    continue;
                }
                else
                {
                    tfiledic.Add(tfile.Name, tfile.FullName);
                }

                AssetBundleBuild tbuild = new AssetBundleBuild();
                tbuild.assetBundleName = tfile.Name + LitEngine.LoadAsset.BaseBundle.sSuffixName;
                string tRelativePath = tfile.FullName;
                int tindex = tRelativePath.IndexOf("Assets");
                tRelativePath = tRelativePath.Substring(tindex, tRelativePath.Length - tindex);
                tRelativePath = tRelativePath.Replace("\\", "/");
                tbuild.assetNames = new string[] { tRelativePath };
                builds.Add(tbuild);
            }
            if (!tisCanExport)
            {
                GoExport(tpath, builds.ToArray(), _target);
            }
            else
            {
                DLog.LogError("存在重名文件.导出失败.");

                List<string> tkeys = new List<string>(tHavedfiledic.Keys);
                foreach (var key in tkeys)
                {
                    System.Text.StringBuilder tstrbuilder = new System.Text.StringBuilder();
                    tstrbuilder.AppendLine("重名文件:" + key);
                    tstrbuilder.AppendLine("{");
                    List<string> tfiles = tHavedfiledic[key];
                    foreach (var item in tfiles)
                    {
                        tstrbuilder.AppendLine("    " + item);
                    }

                    tstrbuilder.AppendLine("}");
                    DLog.LogError(tstrbuilder.ToString());
                }

            }

        }

        static Dictionary<string, UnityEditor.AssetBundleBuild> waitExportFiles = new Dictionary<string, UnityEditor.AssetBundleBuild>();

        public static void ExportAllBundleFullPath(BuildTarget _target)
        {
            waitExportFiles.Clear();
            string tpath = GetExportPath(_target);

            List<UnityEditor.AssetBundleBuild> builds = null;
            switch (ExportSetting.Instance.sBuildType)
            {
                case 0:
                case 1:
                    builds = GetBunldeBuildsEvery(tpath, ExportSetting.Instance.sBuildType == 0);
                    break;
                case 2:
                    builds = GetBunldeBuildsAllInOne(tpath);
                    break;
                case 3:
                    builds = GetBunldeBuildsFolder(tpath);
                    break;
            }
            GoExport(tpath, builds.ToArray(), _target);
        }

        static public string GetExportPath(BuildTarget target)
        {
            return Config.sDefaultFolder + ExportConfig.GetTartFolder(target);
        }

        static public List<UnityEditor.AssetBundleBuild> GetBunldeBuildsFolder(string path)
        {
            waitExportFiles.Clear();
            string tpath = path;
            List<UnityEditor.AssetBundleBuild> builds = new List<UnityEditor.AssetBundleBuild>();
            DirectoryInfo tdirfolder = new DirectoryInfo(Config.sResourcesPath);
            DirectoryInfo[] dirs = tdirfolder.GetDirectories("*.*", System.IO.SearchOption.TopDirectoryOnly);

            foreach (var curDirectory in dirs)
            {
                FileInfo[] tfileinfos = curDirectory.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

                UnityEditor.AssetBundleBuild tbuild = new UnityEditor.AssetBundleBuild();
                tbuild.assetBundleName = curDirectory.Name + LitEngine.LoadAsset.BaseBundle.sSuffixName;

                List<string> tnamelist = new List<string>();

                for (int i = 0, tmax = tfileinfos.Length; i < tmax; i++)
                {
                    FileInfo tfile = tfileinfos[i];
                    if (!IsResFile(tfile.Name)) continue;
                    string tresPath = GetResPath(tfile.FullName);
                    tnamelist.Add(tresPath);

                    EditorUtility.DisplayProgressBar("FolderBuild", "Build " + curDirectory.Name, (float)i / tmax);
                }

                tbuild.assetNames = tnamelist.ToArray();
                builds.Add(tbuild);
            }
            EditorUtility.ClearProgressBar();
            return builds;
        }

        static public List<UnityEditor.AssetBundleBuild> GetBunldeBuildsAllInOne(string path)
        {
            waitExportFiles.Clear();
            string tpath = path;
            List<UnityEditor.AssetBundleBuild> builds = new List<UnityEditor.AssetBundleBuild>();

            DirectoryInfo tdirfolder = new DirectoryInfo(Config.sResourcesPath);
            FileInfo[] tfileinfos = tdirfolder.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

            UnityEditor.AssetBundleBuild tbuild = new UnityEditor.AssetBundleBuild();
            tbuild.assetBundleName = "allinone" + LitEngine.LoadAsset.BaseBundle.sSuffixName;

            List<string> tnamelist = new List<string>();

            for (int i = 0, tmax = tfileinfos.Length; i < tmax; i++)
            {
                FileInfo tfile = tfileinfos[i];
                if (!IsResFile(tfile.Name)) continue;
                string tresPath = GetResPath(tfile.FullName);
                tnamelist.Add(tresPath);

                EditorUtility.DisplayProgressBar("AllInOne", "Build " + tfile.Name, (float)i / tmax);
            }

            tbuild.assetNames = tnamelist.ToArray();
            builds.Add(tbuild);
            EditorUtility.ClearProgressBar();
            return builds;
        }

        static public List<UnityEditor.AssetBundleBuild> GetBunldeBuildsEvery(string path , bool isHaveDep)
        {
            waitExportFiles.Clear();
            string tpath = path;

            List<UnityEditor.AssetBundleBuild> builds = new List<UnityEditor.AssetBundleBuild>();

            DirectoryInfo tdirfolder = new DirectoryInfo(Config.sResourcesPath);
            FileInfo[] tfileinfos = tdirfolder.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

            for (int i = 0, tmax = tfileinfos.Length; i < tmax; i++)
            {
                FileInfo tfile = tfileinfos[i];
                if (!IsResFile(tfile.Name)) continue;
                string tresPath = GetResPath(tfile.FullName);
                if (waitExportFiles.ContainsKey(tresPath))
                {
                    continue;
                }

                UnityEditor.AssetBundleBuild tbuild = GetAssetBundleBuild(tfile.FullName);
                builds.Add(tbuild);

                waitExportFiles.Add(tresPath, tbuild);
                if (isHaveDep)
                {
                    AddAssetFromDependencles(tresPath, ref builds);
                }
                EditorUtility.DisplayProgressBar("单文件Build", tresPath, (float)i / tmax);
            }

            EditorUtility.ClearProgressBar();
            return builds;
        }

        static public string GetResPath(string pFullPath)
        {
            string tresPath = pFullPath;
            int tindex = tresPath.IndexOf("Assets");
            tresPath = tresPath.Substring(tindex, tresPath.Length - tindex);
            tresPath = tresPath.Replace("\\", "/");
            return tresPath;
        }

        static public void AddAssetFromDependencles(string presPath, ref List<UnityEditor.AssetBundleBuild> buildList)
        {
            string[] tresdepends = AssetDatabase.GetDependencies(presPath, true);
            foreach (var item in tresdepends)
            {
                if (!IsResFile(item)) continue;

                if (waitExportFiles.ContainsKey(item)) continue;

                UnityEditor.AssetBundleBuild tbuild = GetAssetBundleBuild(item);
                buildList.Add(tbuild);

                waitExportFiles.Add(item, tbuild);
                AddAssetFromDependencles(item, ref buildList);
            }
        }
        static public UnityEditor.AssetBundleBuild GetAssetBundleBuild(string pFileName)
        {
            string tresPath = GetResPath(pFileName);
            UnityEditor.AssetBundleBuild tbuild = new UnityEditor.AssetBundleBuild();
            tbuild.assetBundleName = tresPath + LitEngine.LoadAsset.BaseBundle.sSuffixName;
            tbuild.assetNames = new string[] { tresPath };
            return tbuild;
        }

        public static void GoExport(string _path, AssetBundleBuild[] _builds, BuildTarget _target)
        {
            if (_builds == null) return;
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
            if (_builds.Length == 0) return;

            BuildPipeline.BuildAssetBundles(_path, _builds, sBuildOption[ExportSetting.Instance.sCompressed] | BuildAssetBundleOptions.DeterministicAssetBundle, _target);

            string tmanifestname = ExportConfig.GetTartFolder(_target).Replace("/", "");
            string tdespathname = _path + "/" + LoaderManager.ManifestName + LitEngine.LoadAsset.BaseBundle.sSuffixName;
            if (File.Exists(tdespathname))
                File.Delete(tdespathname);
            File.Copy(_path + tmanifestname, tdespathname);

            BuildByteFileInfoFile(_path, _path, _target);
            AssetDatabase.Refresh();
            Debug.Log("导出完成!");
        }
        #endregion

        #region move

        static public void MoveBUndleToStreamingPath(BuildTarget _target)
        {

            string tpath = GetExportPath(_target);
            string tfullpath = System.IO.Directory.GetCurrentDirectory() + "\\" + Config.sStreamingBundleFolder + ExportConfig.sResDataPath;
            tfullpath = tfullpath.Replace("\\", "/");
            MoveToPath(tpath, tfullpath, ExportConfig.GetTartFolder(_target));
            AssetDatabase.Refresh();
        }

        static public void MoveBundleToSideDate(BuildTarget _target)
        {

            string tpath = GetExportPath(_target);
            string tfullpath = System.IO.Directory.GetCurrentDirectory() + "\\" + Config.sEditorBundleFolder + ExportConfig.sResDataPath;
            tfullpath = tfullpath.Replace("\\", "/");
            MoveToPath(tpath, tfullpath, ExportConfig.GetTartFolder(_target));
        }

        public static void MoveToPath(string _socPath, string _desPath, string _targetname)
        {
            if (!Directory.Exists(_desPath))
            {
                Directory.CreateDirectory(_desPath);
            }
                
            DeleteAllFile(_desPath);
            _desPath = GetFormatPath(_desPath);
            
            DirectoryInfo tdirfolder = new DirectoryInfo(_socPath);
            _socPath = GetFormatPath(tdirfolder.FullName);

            FileInfo[] tfileinfos = tdirfolder.GetFiles("*" + BaseBundle.sSuffixName, System.IO.SearchOption.AllDirectories);
            for (int i = 0, tmax = tfileinfos.Length; i < tmax; i++)
            {
                FileInfo tfile = tfileinfos[i];
                string tresPath = GetFormatPath(tfile.FullName).Replace(_socPath, "");
                string dicPath = (_desPath + "/" + tresPath.Replace(tfile.Name, ""));

                if (!Directory.Exists(dicPath))
                    Directory.CreateDirectory(dicPath);

                File.Copy(tfile.FullName, _desPath + "/" + tresPath, true);

                EditorUtility.DisplayProgressBar("Copy文件", "Copy " + tresPath, (float)i / tmax);
            }
            Debug.Log($"移动完成.Count = {tfileinfos.Length}, path = {_desPath}");
            EditorUtility.ClearProgressBar();
        }

        static void DeleteAllFile(string _path, string searchPatter = "*.*")
        {
            if (!Directory.Exists(_path)) return;

            string[] tfiles = Directory.GetFiles(_path, searchPatter, System.IO.SearchOption.AllDirectories);
            int tmax = tfiles.Length;
            for (int i = 0; i < tfiles.Length; i++)
            {
                string tfilename = tfiles[i];
                if (File.Exists(tfilename))
                {
                    FileInfo fi = new FileInfo(tfilename);
                    fi.Attributes = FileAttributes.Normal;
                    fi.Delete();
                }

                i++;
                EditorUtility.DisplayProgressBar("清除目录文件", "Delete " + tfilename, (float)i / tmax);
            }

            string[] tdics = Directory.GetDirectories(_path, searchPatter, System.IO.SearchOption.AllDirectories);
            foreach (var item in tdics)
            {
                if (Directory.Exists(item))
                {
                    Directory.Delete(item, true);
                }
            }
            EditorUtility.ClearProgressBar();
            Debug.Log("清除结束.");
        }

        public static bool IsResFile(string pName)
        {
            pName = pName.ToLowerInvariant();
            if (pName.EndsWith(".cs")
            || pName.EndsWith(".dll")
            || pName.EndsWith(".meta")
            || pName.EndsWith(".ds_store"))
                return false;
            return true;
        }
        #endregion

        #region fileinfo

        static public void BuildByteFileInfoFile(string pSocPath, string pDesPath, BuildTarget _target)
        {
            string txtbytefile = pSocPath + sByteFileInfo + BaseBundle.sSuffixName;
            if (File.Exists(txtbytefile))
            {
                File.Delete(txtbytefile);
            }
            string tmainfdest = txtbytefile + ".manifest";
            if (File.Exists(tmainfdest))
            {
                File.Delete(tmainfdest);
            }

            DirectoryInfo tdirfolder = new DirectoryInfo(pSocPath);
            pSocPath = GetFormatPath(tdirfolder.FullName);

            FileInfo[] tfileinfos = tdirfolder.GetFiles("*" + LitEngine.LoadAsset.BaseBundle.sSuffixName, System.IO.SearchOption.AllDirectories);

            List<ByteFileInfo> byteFileInfoList = new List<ByteFileInfo>();
            string appmainfest = "AppManifest" + LitEngine.LoadAsset.BaseBundle.sSuffixName;
            for (int i = 0, tmax = tfileinfos.Length; i < tmax; i++)
            {
                FileInfo tfile = tfileinfos[i];
                string tresPath = GetFormatPath(tfile.FullName).Replace(pSocPath, "");
                ByteFileInfo tbyteinfo = CreatByteFileInfo(tfile, tresPath);
                byteFileInfoList.Add(tbyteinfo);
                if (tfile.FullName.EndsWith(appmainfest))
                {
                    tbyteinfo.priority = 999;
                }

                EditorUtility.DisplayProgressBar("计算Build文件信息", tresPath, (float)i / tmax);
            }

            EditorUtility.ClearProgressBar();
            CreatTxtInfo(byteFileInfoList, pDesPath);

            AssetDatabase.Refresh();

            string txtfile = pDesPath + sByteFileInfo;
            UnityEditor.AssetBundleBuild[] tbuilds = new UnityEditor.AssetBundleBuild[1];
            tbuilds[0] = new UnityEditor.AssetBundleBuild();
            tbuilds[0].assetBundleName = sByteFileInfo + LitEngine.LoadAsset.BaseBundle.sSuffixName;
            tbuilds[0].assetNames = new string[] { txtfile };
            BuildPipeline.BuildAssetBundles(pSocPath, tbuilds, sBuildOption[ExportSetting.Instance.sCompressed] | BuildAssetBundleOptions.DeterministicAssetBundle, _target);

        }
        static void CreatTxtInfo(List<ByteFileInfo> pList, string pDesPath)
        {
            try
            {
                string tfilePath = pDesPath + sByteFileInfo;
                if (File.Exists(tfilePath))
                {
                    File.Delete(tfilePath);
                }
                StringBuilder tstrbd = new StringBuilder();

                for (int i = 0, tcount = pList.Count; i < tcount; i++)
                {
                    var item = pList[i];
                    item.fileMD5 = GetMD5File(item.fileFullPath);
                    string tline = DataConvert.ToJson(item);
                    tstrbd.AppendLine(tline);

                    EditorUtility.DisplayProgressBar("建立数据表 ", "Creat " + item.resName, (float)i / tcount);
                }

                EditorUtility.ClearProgressBar();
                File.AppendAllText(tfilePath, tstrbd.ToString());
            }
            catch (System.Exception ex)
            {
                Debug.LogError("生成文件信息出现错误, error:" + ex.Message);
            }

            Debug.Log("生成文件信息完成.");
        }
        static ByteFileInfo CreatByteFileInfo(FileInfo pInfo, string pResName)
        {
            ByteFileInfo ret = new ByteFileInfo();
            ret.fileFullPath = pInfo.FullName;
            ret.resName = pResName;
            ret.fileSize = pInfo.Length;
            return ret;
        }

        public static string GetMD5File(string file)
        {
            try
            {
                FileStream fs = new FileStream(file, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(fs);
                fs.Close();

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (System.Exception ex)
            {
                Debug.LogError("md5file() fail, error:" + ex.Message);
            }
            return null;
        }
        public static string GetFormatPath(string pPath)
        {
            return string.Join("/", pPath.Replace("\\", "/").Split('/'));
        }

        #endregion

    }
}

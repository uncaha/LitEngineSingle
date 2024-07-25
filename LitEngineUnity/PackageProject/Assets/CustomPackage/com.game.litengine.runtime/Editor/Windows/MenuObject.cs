using UnityEditor;
using UnityEngine;
using LitEngine;

using LitEngineEditor;
using System.IO;
using System.Collections.Generic;
using LitEngine.LoadAsset;
public class MenuObject
{

    [UnityEditor.MenuItem("LitEngine/CreatDirectory For App")]
    static internal void CreatDirectoryForApp()
    {
        if (!Directory.Exists(ExportBase.Config.sResourcesPath))
            Directory.CreateDirectory(ExportBase.Config.sResourcesPath);
        if (!Directory.Exists(ExportBase.Config.sDefaultFolder))
            Directory.CreateDirectory(ExportBase.Config.sDefaultFolder);

        CreatLitEngineFolders(ExportBase.Config.sStreamingBundleFolder);
        CreatLitEngineFolders(ExportBase.Config.sEditorBundleFolder);
    }

    static void CreatLitEngineFolders(string rootPath)
    {
        string tresfolder = "ResData/";
        string tconfigfolder = $"{tresfolder}{GameCore.ConfigDataPath}/";

        if (!Directory.Exists(rootPath))
            Directory.CreateDirectory(rootPath);

        CreatDirectory(rootPath, tresfolder);
        CreatDirectory(rootPath, tconfigfolder);
    }

    static void CreatDirectory(string rootPath,string forlder)
    {
        string tpath = rootPath + forlder;
        if (!Directory.Exists(tpath))
            Directory.CreateDirectory(tpath);
    }

    [UnityEditor.MenuItem("LitEngine/生成Resources预留资源表")]
    static public void CreatModelAsset()
    {
        string tgamepath = "Assets/Resources";

        List<AssetMap.AssetObject> tfiles = GetFileList(tgamepath, true);
        tfiles.AddRange(GetFileList(ExportBase.Config.sResourcesPath, false));

        AssetMap asset = ScriptableObject.CreateInstance<AssetMap>();
        asset.assets = tfiles.ToArray();
        AssetDatabase.CreateAsset(asset, "Assets/Resources/ResourcesMap.asset");
        AssetDatabase.Refresh();
    }

    static List<AssetMap.AssetObject> GetFileList(string pFullPath, bool isInSide)
    {
        List<AssetMap.AssetObject> tassetNames = new List<AssetMap.AssetObject>();
        if (!Directory.Exists(pFullPath))
        {
            return tassetNames;
        }
        string trepPath = Application.dataPath;
        DirectoryInfo tdirfolder = new DirectoryInfo(pFullPath);
        FileInfo[] tfileinfos = tdirfolder.GetFiles("*.*", System.IO.SearchOption.AllDirectories);
        for (int i = 0, tmax = tfileinfos.Length; i < tmax; i++)
        {
            FileInfo tfile = tfileinfos[i];
            if (!ExportObject.IsResFile(tfile.Name)) continue;
            string tresPath = ExportObject.GetFormatPath(tfile.FullName).Replace(trepPath, "").ToLowerInvariant();
            string tfindstr = "Resources/".ToLowerInvariant();
            int tindex = tresPath.IndexOf(tfindstr) + tfindstr.Length;
            tresPath = tresPath.Substring(tindex, tresPath.Length - tindex);
            AssetMap.AssetObject tobj = new AssetMap.AssetObject(tresPath);
            tobj.isInSide = isInSide;
            tassetNames.Add(tobj);
            EditorUtility.DisplayProgressBar(pFullPath + "文件夹", tresPath, (float)i / tmax);
        }
        EditorUtility.ClearProgressBar();
        return tassetNames;
    }
}
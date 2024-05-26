using UnityEditor;
using UnityEngine;
using LitEngine;
using LitEngine.ScriptInterface;
using LitEngineEditor;
using System.IO;
using System.Collections.Generic;
using LitEngine.LoadAsset;
public class MenuObject
{
    #region 脚本接口
    static T AddScript<T>(GameObject _object) where T : BehaviourInterfaceBase
    {
        if (_object == null) return null;

        T tscript = _object.AddComponent<T>();

        UnityEngine.SceneManagement.Scene tscene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(tscene);
        return tscript;

    }

    [UnityEditor.MenuItem("GameObject/ScriptInterface/UIInterface", priority = 0)]
    static void AddUIInterface()
    {
        AddScript<UIInterface>(UnityEditor.Selection.activeGameObject);
    }

    [UnityEditor.MenuItem("GameObject/ScriptInterface/BaseInterface", priority = 0)]
    static void AddBaseInterface()
    {
        AddScript<BehaviourInterfaceBase>(UnityEditor.Selection.activeGameObject);
    }

    [UnityEditor.MenuItem("GameObject/ScriptInterface/CET/OnEnableInterface", priority = 0)]
    static void AddOnEnableInterface()
    {
        AddScript<ScriptInterfaceOnEnable>(UnityEditor.Selection.activeGameObject);
    }
    [UnityEditor.MenuItem("GameObject/ScriptInterface/CET/OnTriggerInterface", priority = 0)]
    static void AddOnTriggerInterface()
    {
        AddScript<ScriptInterfaceTrigger>(UnityEditor.Selection.activeGameObject);
    }
    [UnityEditor.MenuItem("GameObject/ScriptInterface/CET/OnCollisionInterface", priority = 0)]
    static void AddOnCollisionInterface()
    {
        AddScript<ScriptInterfaceCollision>(UnityEditor.Selection.activeGameObject);
    }
    [UnityEditor.MenuItem("GameObject/ScriptInterface/CET/CETInterface", priority = 0)]
    static void AddCETInterface()
    {
        AddScript<ScriptInterfaceCET>(UnityEditor.Selection.activeGameObject);
    }
    [UnityEditor.MenuItem("GameObject/ScriptInterface/Other/MouseInterface", priority = 0)]
    static void AddMouseInterface()
    {
        AddScript<ScriptInterfaceMouse>(UnityEditor.Selection.activeGameObject);
    }
    [UnityEditor.MenuItem("GameObject/ScriptInterface/Other/OnApplicationInterface", priority = 0)]
    static void OnApplicationInterface()
    {
        AddScript<ScriptInterfaceApplication>(UnityEditor.Selection.activeGameObject);
    }
    [UnityEditor.MenuItem("GameObject/ScriptInterface/Other/OnBecameInterface", priority = 0)]
    static void OnBecameInterface()
    {
        AddScript<ScriptInterfaceBecame>(UnityEditor.Selection.activeGameObject);
    }
    #endregion

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
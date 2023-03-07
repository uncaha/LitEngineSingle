using UnityEngine;
using System.Collections;
using LitEngine.UpdateTool;
public class UpdateCheckTool : MonoBehaviour
{
    public delegate void UpdateAction();
    private static UpdateCheckTool sInstance = null;
    private static UpdateCheckTool Ins
    {
        get
        {
            if (sInstance == null)
            {
                GameObject tobj = new GameObject("UpdateCheckTool");
                GameObject.DontDestroyOnLoad(tobj);
                sInstance = tobj.AddComponent<UpdateCheckTool>();
                sInstance.Init();
            }
            return sInstance;
        }
    }

    private bool mInited = false;

    private void Init()
    {
        if (mInited) return;
        mInited = true;
    }

    #region 更新
    /// <summary>
    /// 更新检测示例
    /// </summary>
    /// <returns></returns>
    static public bool CheckUpdate()
    {
        if (UpdateAssetManager.Ins.checkType == UpdateAssetManager.CheckType.AllGood)
        {
            return false;
        }
        else
        {
            Ins.StartCoroutine(Ins.Check());
            return true;
        }
    }
    IEnumerator Check()
    {
        yield return null;
        Debug.Log(UpdateAssetManager.Ins.checkType);
        if (UpdateAssetManager.Ins.checkType == UpdateAssetManager.CheckType.AllGood)
        {
            //不需要更新
        }
        else
        {
            if (UpdateAssetManager.Ins.checkType != UpdateAssetManager.CheckType.checking)
            {
                UpdateAssetManager.Ins.CheckUpdate();
            }

            while (UpdateAssetManager.Ins.checkType == UpdateAssetManager.CheckType.checking)
            {
                yield return null;
            }
            Debug.Log(UpdateAssetManager.Ins.checkType);
            switch (UpdateAssetManager.Ins.checkType)
            {
                case UpdateAssetManager.CheckType.needUpdate:
                    {
                        StartCoroutine(UpdateAsset());
                    }
                    break;
                case UpdateAssetManager.CheckType.fail:
                    {
                        //检测失败
                    }
                    break;
                case UpdateAssetManager.CheckType.AllGood:
                    {
                        //无需更新
                    }
                    break;
                default:
                    break;
            }

        }

    }

    IEnumerator UpdateAsset()
    {
        yield return null;
        if (UpdateAssetManager.Ins.checkType != UpdateAssetManager.CheckType.needUpdate)
        {
            //不需要更新
        }
        else
        {
            Debug.Log(UpdateAssetManager.Ins.updateType);
            if (UpdateAssetManager.Ins.updateType != UpdateAssetManager.UpdateType.updateing)
            {
                UpdateAssetManager.Ins.UpdateAssets();
            }

            Debug.Log(UpdateAssetManager.Ins.updateType);
            while (UpdateAssetManager.Ins.updateType == UpdateAssetManager.UpdateType.updateing)
            {
                Debug.Log(UpdateAssetManager.Ins.DownLoadLength + "/" + UpdateAssetManager.Ins.ContentLength + "|" + UpdateAssetManager.Ins.UpdateProcess);
                yield return null;
            }
            Debug.Log(UpdateAssetManager.Ins.updateType);
            switch (UpdateAssetManager.Ins.updateType)
            {
                case UpdateAssetManager.UpdateType.fail:
                    {
                        //更新失败
                    }
                    break;
                case UpdateAssetManager.UpdateType.finished:
                    {
                        //更新完成
                    }
                    break;
                default:
                    break;
            }

        }

    }
    #endregion
}

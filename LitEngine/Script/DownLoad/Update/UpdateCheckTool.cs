using UnityEngine;
using System.Collections;

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
    static public bool CheckUpdate(UpdateAction onComplete)
    {
        if (UpdateAssetManager.Ins.checkType == UpdateAssetManager.CheckType.AllGood)
        {
            onComplete?.Invoke();
            return false;
        }
        else
        {
            Ins.StartCoroutine(Ins.Check(onComplete));
            return true;
        }

    }
    IEnumerator Check(UpdateAction onComplete)
    {
        yield return null;
        Debug.Log(UpdateAssetManager.Ins.checkType);
        if (UpdateAssetManager.Ins.checkType == UpdateAssetManager.CheckType.AllGood)
        {
            //不需要更新
            onComplete?.Invoke();
        }
        else
        {
            if(UpdateAssetManager.Ins.checkType != UpdateAssetManager.CheckType.checking)
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
                        StartCoroutine(UpdateAsset(onComplete));
                    }
                    break;
                case UpdateAssetManager.CheckType.fail:
                    {
                        //检测失败
                        onComplete?.Invoke();
                    }
                    break;
                case UpdateAssetManager.CheckType.AllGood:
                    {
                        //无需更新
                        onComplete?.Invoke();
                    }
                    break;
                default:
                    break;
            }

        }

    }

    IEnumerator UpdateAsset(UpdateAction onComplete)
    {
        yield return null;
        if (UpdateAssetManager.Ins.checkType != UpdateAssetManager.CheckType.needUpdate)
        {
            //不需要更新
            onComplete?.Invoke();
        }
        else
        {
            Debug.Log(UpdateAssetManager.Ins.updateType);
            if(UpdateAssetManager.Ins.updateType != UpdateAssetManager.UpdateType.updateing)
            {
                UpdateAssetManager.Ins.UpdateAssets();
            }
            
            Debug.Log(UpdateAssetManager.Ins.updateType);
            while (UpdateAssetManager.Ins.updateType == UpdateAssetManager.UpdateType.updateing)
            {
                Debug.Log(UpdateAssetManager.Ins.DownLoadLength + "/" + UpdateAssetManager.Ins.ContentLength + "|" + UpdateAssetManager.Ins.UpdateProcess);
                yield return null;
            }

            switch (UpdateAssetManager.Ins.updateType)
            {
                case UpdateAssetManager.UpdateType.fail:
                    {
                        //更新失败
                        onComplete?.Invoke();
                    }
                    break;
                case UpdateAssetManager.UpdateType.finished:
                    {
                        //更新完成
                        onComplete?.Invoke();
                    }
                    break;
                default:
                    break;
            }

        }

    }
    #endregion
}

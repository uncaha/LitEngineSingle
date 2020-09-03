using UnityEngine;
using LitEngine.UpdateTool;
using LitEngine.LoadAsset;
namespace LitEngine.UpdateTool
{
    public class UpdateAssetManager
    {
        public enum CheckType
        {
            none = 0,
            checking,
            fail,
            needUpdate,
            AllGood,
        }

        public enum UpdateType
        {
            none = 0,
            updateing,
            fail,
            finished,
        }

        #region static object
        private static object lockobj = new object();
        private static UpdateAssetManager sInstance = null;
        public static UpdateAssetManager Ins
        {
            get
            {
                if (sInstance == null)
                {
                    lock (lockobj)
                    {

                        if (sInstance == null)
                        {
                            sInstance = new UpdateAssetManager();
                        }
                    }
                }
                return sInstance;
            }
        }

        #endregion

        #region prop
        public CheckType checkType { get; private set; }
        public UpdateType updateType { get; private set; }
        public ByteFileInfoList updateList { get; private set; }

        private float _CheckProcess = 0;
        public float CheckProcess
        {
            get
            {
                if (UpdateManager.checkDL == null) return _CheckProcess;
                _CheckProcess = UpdateManager.checkDL.Progress;
                return _CheckProcess;
            }
        }

        public float UpdateProcess
        {
            get
            {
                return UpdateManager.DownloadProcess;
            }
        }

        public long DownLoadLength
        {
            get
            {
                return UpdateManager.DownLoadLength;
            }
        }

        public long ContentLength
        {
            get
            {
                return UpdateManager.ContentLength;
            }
        }
        #endregion
        private UpdateAssetManager()
        {
            checkType = CheckType.none;
            updateType = UpdateType.none;
        }
        #region update
        public void UpdateAssets()
        {
            if (updateType == UpdateType.updateing || checkType != CheckType.needUpdate) return;
            switch (updateType)
            {
                case UpdateType.none:
                case UpdateType.fail:
                    CaseUpdateFail();
                    break;
                case UpdateType.finished:
                    CaseUpdateFinished();
                    break;
                case UpdateType.updateing:
                    CaseUpdateUpdateing();
                    break;
                default:
                    break;
            }
        }
        void CaseUpdateFail()
        {
            if (updateList != null)
            {
                updateType = UpdateType.updateing;
                UpdateManager.UpdateRes(updateList, OnUpdateComplete, true);
            }
            else
            {
                Debug.LogError("更新列表不能为空.");
            }

        }
        void CaseUpdateFinished()
        {

        }
        void CaseUpdateUpdateing()
        {

        }

        void OnUpdateComplete(ByteFileInfoList info, string error)
        {
            if (string.IsNullOrEmpty(error))
            {
                updateType = UpdateType.finished;
                checkType = CheckType.AllGood;
                updateList = null;
            }
            else
            {
                updateType = UpdateType.fail;
                updateList = info;
            }
        }
        #endregion
        #region check
        public bool IsNeedUpdate()
        {
            bool needCheck = checkType != CheckType.AllGood;
            bool needUpdate = updateType != UpdateType.finished;
            return needCheck || needUpdate;
        }
        public void CheckUpdate()
        {
            if (checkType == CheckType.checking) return;
            switch (checkType)
            {
                case CheckType.none:
                case CheckType.fail:
                    CaseFail();
                    break;
                case CheckType.needUpdate:
                    CaseNeedUpdate();
                    break;
                case CheckType.AllGood:
                    CaseAllGood();
                    break;
                case CheckType.checking:
                    CaseChecking();
                    break;
                default:
                    break;
            }
        }
        void CaseChecking()
        {

        }
        void CaseAllGood()
        {

        }
        void CaseFail()
        {
            checkType = CheckType.checking;
            UpdateManager.CheckUpdate(OnCheckComplete, false, true);
        }
        void CaseNeedUpdate()
        {

        }

        void OnCheckComplete(ByteFileInfoList info, string error)
        {
            if (error == null)
            {
                if (info != null && info.fileMap.Count > 0)
                {
                    checkType = CheckType.needUpdate;
                    updateList = info;
                }
                else
                {
                    checkType = CheckType.AllGood;
                }
            }
            else
            {
                checkType = CheckType.fail;
            }
        }

        #endregion

    }
}

using System.Collections.Generic;
namespace LitEngine.LoadAsset
{
    public class AssetGroup
    {
        public string Key { get; private set; }
        public List<string> assetList = new List<string>();
        public AssetGroup(string pkey)
        {
            Key = pkey;
        }

        public void AddAsset(string pAsset)
        {
            assetList.Add(pAsset);
        }

        public void ReleaseAssets()
        {
            for (int i = 0, length = assetList.Count; i < length; i++)
            {
                LoaderManager.ReleaseAsset(assetList[i]);
            }
            assetList.Clear();
        }
    }

    public class LoadGroup
    {
        public class LoadAssetObject
        {
            public string assetName { get; private set; }
            public string groupName { get; private set; }
            public bool isLoaded { get; private set; }
            public object resObject { get; private set; }
            System.Action<string, object> onComplete;
            System.Action<LoadAssetObject> onCompleteParent;

            private bool isStart = false;
            private bool isCalled = false;
            public LoadAssetObject(string pName, System.Action<string, object> pOnComplete, System.Action<LoadAssetObject> pParentCall, string pGroup)
            {
                assetName = pName;
                onComplete = pOnComplete;
                onCompleteParent = pParentCall;
                groupName = pGroup;
            }
            public void Start()
            {
                if (isStart) return;
                isStart = true;
                LoaderManager.LoadAssetAsync(assetName, assetName, LoadComplete, groupName);
            }

            void LoadComplete(string pKey, object pRes)
            {
                isLoaded = true;
                resObject = pRes;
                onCompleteParent?.Invoke(this);
            }

            public void CallComplete()
            {
                if (!isLoaded)
                {
                    DLog.LogErrorFormat("{0} 还未载入完成.错误的调用 CallComplete",assetName);
                    return;
                }
                if (isCalled)
                {
                    DLog.LogWarningFormat("{0} 重复调用 CallComplete",assetName);
                    return;
                }
                try
                {
                    onComplete?.Invoke(assetName, resObject);
                }
                catch (System.Exception error)
                {
                    DLog.LogErrorFormat("asset = {0},error = {1}", assetName, error.Message);
                }
                isCalled = true;
            }
        }
        public string Key { get; private set; }
        public bool IsStart { get; private set; }
        public int CompleteCount { get; private set; }
        public int Count { get { return waitLoadList.Count; } }
        public event System.Action<LoadGroup> onComplete;
        private List<LoadAssetObject> waitLoadList = new List<LoadAssetObject>();
        private bool isCalledComplete = false;
        public LoadGroup(string pkey)
        {
            Key = pkey;
            IsStart = false;
        }

        public void Add(string pAssetName, System.Action<string, object> pComplete)
        {
            if (IsStart)
            {
                DLog.LogErrorFormat("Group已经开始加载.添加失败.name = {0}", pAssetName);
                return;
            }
            var tobj = new LoadAssetObject(pAssetName, pComplete, OnAssetLoaded, Key);
            waitLoadList.Add(tobj);
        }

        private void OnAssetLoaded(LoadAssetObject pSender)
        {
            CompleteCount++;
            CallComplete();
        }

        void CallComplete()
        {
            if (isCalledComplete) return;
            if (CompleteCount == waitLoadList.Count)
            {
                isCalledComplete = true;
                try
                {
                    onComplete?.Invoke(this);
                }
                catch (System.Exception error)
                {
                    DLog.LogErrorFormat("group = {0},error = {1}", Key, error.Message);
                }
            }
        }

        public void StartLoad()
        {
            if (IsStart) return;
            IsStart = true;
            isCalledComplete = false;
            CompleteCount = 0;
            for (int i = waitLoadList.Count - 1; i >= 0; i--)
            {
                var item = waitLoadList[i];
                if (!item.isLoaded)
                {
                    item.Start();
                }
                else
                {
                    CompleteCount++;
                }

            }
        }
    }
}
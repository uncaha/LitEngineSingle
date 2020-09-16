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
            public string assetName;
            public System.Action<string, object> onComplete;
        }
        public string Key { get; private set; }

        private List<LoadAssetObject> assetList = new List<LoadAssetObject>();
        public LoadGroup(string pkey)
        {
            Key = pkey;
        }

        public void Add(string pAssetName, System.Action<string, object> pComplete)
        {
            var tobj = new LoadAssetObject(){assetName = pAssetName,onComplete = pComplete};
            assetList.Add(tobj);
        }
    }
}
using System.Collections.Generic;
namespace LitEngine.LoadAsset
{
    public class LoadGroup
    {
        public string Key { get; private set; }
        public List<string> assetList = new List<string>();
        public LoadGroup(string pkey)
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
}
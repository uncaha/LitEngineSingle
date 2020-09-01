using UnityEngine;
using System.IO;
using System.Collections.Generic;
namespace LitEngine.LoadAsset
{
    [CreateAssetMenu(fileName = "AssetMap", menuName = "AssetTool/AssetMap", order =1)]
    public class AssetMap : ScriptableObject
    {
        [System.Serializable]
        public class AssetObject
        {
            public string assetName;
            public string sufixx;
            public bool isInSide;
            public AssetObject(string pFileName)
            {
                int tindex = pFileName.LastIndexOf('.');
                if(tindex > -1)
                {
                    sufixx = pFileName.Substring(tindex,pFileName.Length - tindex);
                    assetName = pFileName.Replace(sufixx,"");
                }
                else
                {
                    sufixx = "";
                    assetName = pFileName;
                }
                    
                
            }
        }
        
        public AssetObject[] assets;

        private static AssetObject sNullObject;
        private Dictionary<string,AssetObject> assetMap;
        
       [System.NonSerialized] bool inited = false;
        public void Init()
        {
            if(inited) return;
            inited = true;
            if(assets == null)
            {
                assets = new AssetObject[0];
            }
            sNullObject = new AssetObject("Null");
            assetMap  = new Dictionary<string,AssetObject>(assets.Length);
            foreach(var item in assets)
            {
                try
                {
                    assetMap.Add(item.assetName,item);
                }
                catch (System.Exception e)
                {
                    Debug.LogErrorFormat("asset = {0} error = {1}" ,item.assetName, e.Message);
                }
            }
        }

        public AssetObject GetAsset(string pAsset)
        {
            if (assetMap == null)
            {
                Init();
            }
            string pkey = pAsset.ToLowerInvariant();
            AssetObject ret;
            if (!assetMap.TryGetValue(pkey, out ret))
            {
                ret = sNullObject;
                ret.assetName = pAsset;
                ret.isInSide = true;
            }
            return ret;
        }
    }
    
}
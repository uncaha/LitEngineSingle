using UnityEngine;
namespace LitEngine
{
    namespace Loader
    {
        public class AssetsBundleFromFile : BaseBundle
        {
            //只支持bundle 类型，其他种类的读取请使用异步或者直接stream读取
            public AssetsBundleFromFile(string _assetsname) : base(_assetsname)
            {

            }
            public override void Load(LoaderManager _loader)
            {
                mPathName = _loader.GetFullPath(mAssetName);

                mAssetsBundle = AssetBundle.LoadFromFile(mPathName);
                if (mAssetsBundle != null)
                {
                    if (((AssetBundle)mAssetsBundle).isStreamedSceneAssetBundle)
                        mAsset = ((AssetBundle)mAssetsBundle).mainAsset;
                    else
                        mAsset = ((AssetBundle)mAssetsBundle).LoadAsset(DeleteSuffixName(mAssetName).ToLower());
                }
                else
                    DLog.LogError("AssetsBundleFromFile打开文件失败,请检查资源是否存在-" + mPathName);
                LoadEnd();
            }
        }
    }
}


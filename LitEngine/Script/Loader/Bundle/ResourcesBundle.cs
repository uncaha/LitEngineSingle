using UnityEngine;
namespace LitEngine
{
    namespace Loader {
        public class ResourcesBundle : BaseBundle
        {
            public ResourcesBundle(string _assetsname) : base(_assetsname)
            {

            }

            public override void Load(LoaderManager _loader)
            {
                mPathName = _loader.GetResourcesDataPath(mAssetName);
                mAssetsBundle = Resources.Load(mPathName);
                if (mAssetsBundle == null)
                    DLog.LogError("ResourcesBundle打开文件失败,请检查资源是否存在-" + mPathName);
                mAsset = mAssetsBundle;
                LoadEnd();
            }

            public override void Destory()
            {
                if ( mAssetsBundle != null)
                    Resources.UnloadAsset(mAssetsBundle);
                base.Destory();
            }
        }
    }  
}

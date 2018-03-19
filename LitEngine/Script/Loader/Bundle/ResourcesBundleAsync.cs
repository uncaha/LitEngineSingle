using UnityEngine;
namespace LitEngine
{
    namespace Loader
    {
        public class ResourcesBundleAsync : BaseBundle
        {
            private ResourceRequest mReq = null;
            private bool mLoadFinished = false;
            public ResourcesBundleAsync(string _assetsname) : base(_assetsname)
            {

            }

            public override void Load(LoaderManager _loader)
            {
                mPathName = _loader.GetResourcesDataPath(mAssetName);
                mReq = Resources.LoadAsync(mPathName);
                base.Load(_loader);
            }
            override public bool IsDone()
            {
                if (!base.IsDone()) return false;
                if (mLoadFinished) return true;
                if (mReq == null)
                {
                    DLog.LogError( "erro Resources->loadasync.载入过程中，错误的调用了清除函数。AssetName = "+ mAssetName);
                    mLoadFinished = true;
                    return false;
                }

                if (!mReq.isDone)
                {
                    mProgress = mReq.progress;
                    return false;
                }
                mProgress = mReq.progress;
                mAssetsBundle = mReq.asset;
                mAsset = mReq.asset;
                mReq = null;
                if (mAsset == null)
                    DLog.LogError( "erro Resources->loadasync.载入失败! mPathName = "+mPathName);
                LoadEnd();
                return true;
            }

            public override void LoadEnd()
            {
                mLoadFinished = true;
                base.LoadEnd();
            }
            public override void Destory()
            {
                if ( mAssetsBundle != null)
                    Resources.UnloadAsset(mAssetsBundle);
                mReq = null;
                base.Destory();
            }
        }
    }
}
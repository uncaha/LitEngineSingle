using UnityEngine;
namespace LitEngine
{
    namespace Loader
    {
        public class AssetsBundleAsyncFromFile : BaseBundle
        {
            public enum StepState
            {
                None = 0,
                BundleLoad,
                AssetsLoad,
                LoadEnd,
            }

            private AssetBundleCreateRequest mCreat = null;
            private AssetBundleRequest mLoadObjReq = null;
            private StepState mStep = StepState.None;
            public AssetsBundleAsyncFromFile()
            {
            }

            public AssetsBundleAsyncFromFile(string _assetsname) : base(_assetsname)
            {
            }

            public override void LoadEnd()
            {
                mStep = StepState.LoadEnd;
                base.LoadEnd();
            }
            void CreatBundleReq()
            {
                AssetBundle tasbd = mAssetsBundle as AssetBundle;
                if (tasbd == null)
                {
                    DLog.LogError("AssetBundle 转换失败.mAssetName = " + mAssetName);
                    return;
                }
                string tname = DeleteSuffixName(mAssetName).ToLower();
                mLoadObjReq = ((AssetBundle)mAssetsBundle).LoadAssetAsync(tname);

            }
            override public bool IsDone()
            {
                switch(mStep)
                {
                    case StepState.None:
                        return base.IsDone();
                    case StepState.LoadEnd:
                        return true;
                    case StepState.BundleLoad:
                        return BundleLoad();
                    case StepState.AssetsLoad:
                        return AssetsLoad();
                }
                return true;
            }

            private bool AssetsLoad()
            {

                if (!mLoadObjReq.isDone) return false;
                mAsset = mLoadObjReq.asset;
                if (mAsset == null)
                {
                    mAsset = ((AssetBundle)mAssetsBundle).mainAsset;
                    DLog.LogError("在资源包 " + mPathName + " 中找不到文件名:" + DeleteSuffixName(mAssetName).ToLower() + " 的资源。或者因为资源的命名不规范导致unity加载模块找不到该资源. ");
                }

                mCreat = null;
                mLoadObjReq = null;
                LoadEnd();
                return true;
            }

            private bool BundleLoad()
            {
                if (mCreat == null)
                {
                    DLog.LogError("erro loadasync.载入过程中，错误的调用了清除函数。mAssetName = " + mAssetName);
                    LoadEnd();
                    return false;
                }
                if (!mCreat.isDone)
                {
                    mProgress = mCreat.progress;
                    return false;
                }

                mProgress = mCreat.progress;
                mAssetsBundle = mCreat.assetBundle;
                if (mAssetsBundle == null)
                {
                    DLog.LogError("AssetsBundleAsyncFromFile-erro created。文件载入失败,请检查文件名:" + mPathName);
                    LoadEnd();
                    return true;
                }

                if (((AssetBundle)mAssetsBundle).isStreamedSceneAssetBundle)
                {
                    mAsset = ((AssetBundle)mAssetsBundle).mainAsset;
                    mCreat = null;
                    mLoadObjReq = null;
                    LoadEnd();
                    return true;
                }
                else
                {
                    CreatBundleReq();
                    mStep = StepState.AssetsLoad;
                    return false;
                }
                 
            }

            public override void Load(LoaderManager _loader)
            {
                mPathName = _loader.GetFullPath(mAssetName);
                mCreat = AssetBundle.LoadFromFileAsync(mPathName);
                mStep = StepState.BundleLoad;
                base.Load(_loader);
            }

            public override void Destory()
            {
                mCreat = null;
                mLoadObjReq = null;
                base.Destory();
            }
        }
    }
}


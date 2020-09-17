using UnityEngine;
namespace LitEngine.LoadAsset
{
    public class AssetsBundleAsyncFromFile : BaseBundle
    {

        private AssetBundleCreateRequest mCreat = null;
        private AssetBundleRequest mLoadObjReq = null;
        private bool WaitCallStartLoadAsset = false;
        private bool isStreamedSceneAssetBundle = false;
        public AssetsBundleAsyncFromFile()
        {
        }

        public AssetsBundleAsyncFromFile(string _assetsname, bool _waitloadcall = false) : base(_assetsname)
        {
            WaitCallStartLoadAsset = _waitloadcall;
        }

        public override void LoadEnd()
        {
            base.LoadEnd();
        }
        void CreatBundleReq()
        {
            AssetBundle tasbd = mAssetsBundle as AssetBundle;
            string tname = DeleteSuffixName(mAssetName);
            mLoadObjReq = tasbd.LoadAssetAsync(tname);
        }

        void CreatStreamedSceneReq()
        {
            AssetBundle tasbd = mAssetsBundle as AssetBundle;
            mLoadObjReq = tasbd.LoadAllAssetsAsync();
        }
        override public bool IsDone()
        {
            switch (Step)
            {
                case StepState.None:
                    return base.IsDone();
                case StepState.LoadEnd:
                    return true;
                case StepState.BundleLoad:
                    return BundleLoad();
                case StepState.AssetsLoad:
                    return AssetsLoad();
                case StepState.WaitingLoadAsset:
                    return Waiting();
            }
            return true;
        }

        private bool Waiting()
        {
            return false;
        }

        private bool AssetsLoad()
        {
            if (!mLoadObjReq.isDone) return false;
            if (isStreamedSceneAssetBundle)
            {
                mAsset = mLoadObjReq.allAssets;
            }
            else
            {
                mAsset = mLoadObjReq.asset;
            }

            if (mAsset == null)
            {
                UnityEngine.Debug.LogError("在资源包 " + mPathName + " 中找不到文件名:" + DeleteSuffixName(mAssetName).ToLowerInvariant() + " 的资源。或者因为资源的命名不规范导致unity加载模块找不到该资源. ");
            }

            OptAssetShow();

            mCreat = null;
            mLoadObjReq = null;
            LoadEnd();
            return true;
        }

        private bool BundleLoad()
        {
            if (mCreat == null)
            {
                UnityEngine.Debug.LogError("erro loadasync.载入过程中，错误的调用了清除函数。mAssetName = " + mAssetName);
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
                UnityEngine.Debug.LogError("AssetsBundleAsyncFromFile-erro created。文件载入失败,请检查文件名:" + mPathName);
                LoadEnd();
                return true;
            }
            Step = StepState.WaitingLoadAsset;
            if (!WaitCallStartLoadAsset)
            {
                StartLoadAssets();
            } 
            return false;
        }

        public void StartLoadAssets()
        {
            if (mAssetsBundle == null || Step != StepState.WaitingLoadAsset) return;
            isStreamedSceneAssetBundle = ((AssetBundle)mAssetsBundle).isStreamedSceneAssetBundle;
            if (isStreamedSceneAssetBundle)
            {
                CreatStreamedSceneReq();
            }
            else
            {
                CreatBundleReq();
            }
            Step = StepState.AssetsLoad;
        }

        public override void Load()
        {
            mPathName = LoaderManager.GetFullPath(mAssetName);
            mCreat = AssetBundle.LoadFromFileAsync(mPathName);
            base.Load();
        }

        public override void Destory()
        {
            mCreat = null;
            mLoadObjReq = null;
            base.Destory();
        }
    }
}


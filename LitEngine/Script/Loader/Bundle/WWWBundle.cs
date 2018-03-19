using UnityEngine;
namespace LitEngine
{
    namespace Loader
    {
        class WWWBundle : BaseBundle
        {
            private WWW mCreat = null;
            private bool mLoadFinished = false;
            public WWWBundle()
            {
            }

            public WWWBundle(string _assetsname) : base(_assetsname)
            {
            }

            public override void Load(LoaderManager _loader)
            {
                mPathName = mAssetName;
                if (!mPathName.Contains("file://"))
                    mPathName = "file://" + mPathName;
                mCreat = new WWW(mPathName);
                base.Load(_loader);
            }
            public override void LoadEnd()
            {
                base.LoadEnd();
                mLoadFinished = true;
            }
            public override bool IsDone()
            {
                if (!base.IsDone()) return false;
                if (mLoadFinished) return true;
                if (!mCreat.isDone)
                {
                    mProgress = mCreat.progress;
                    return false;
                }
                mProgress = mCreat.progress;
                if (mCreat.error == null)
                    mAsset = mCreat;
                LoadEnd();
                return true;
            }



            public override void Destory()
            {
                if (mCreat.assetBundle != null)
                    mCreat.assetBundle.Unload(true);
                mCreat.Dispose();
                mCreat = null;
                base.Destory();
            }
        }
    }
}


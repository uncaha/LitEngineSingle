using System;
using System.Collections.Generic;

namespace LitEngine
{
    namespace Loader
    {
        public class AssetsBundleHaveDependencie : BaseBundle
        {
            public delegate BaseBundle LoadAssetRetain(string _AssetsName);
            protected BaseBundle mMainBundle = null;
            protected List<BaseBundle> mDepList = new List<BaseBundle>();
            private LoadAssetRetain mLoadCall = null;
            public AssetsBundleHaveDependencie(string _assetsname) : base(_assetsname)
            {
            }
            public AssetsBundleHaveDependencie(string _assetsname, LoadAssetRetain _loadcall) : base(_assetsname)
            {
                mLoadCall = _loadcall;
            }

            public override void Load(LoaderManager _loader)
            {
                string[] tdeps = _loader.GetDirectDependencies(mAssetName);
                if (tdeps != null)
                {
                    for (int i = 0; i < tdeps.Length; i++)
                    {
                        string tdepassetname = tdeps[i];
                        BaseBundle tchile = mLoadCall(tdepassetname);
                        mDepList.Add(tchile);
                    }
                }

                mMainBundle = new AssetsBundleFromFile(mAssetName);
                mMainBundle.Load(_loader);

                LoadEnd();
            }

            public override void LoadEnd()
            {
                mAssetsBundle = mMainBundle.AssetsBundleObject;
                mAsset = mMainBundle.Asset;
                mMainBundle = null;
                base.LoadEnd();
            }

            public override object Retain()
            {
                for (int i = 0; i < mDepList.Count; i++)
                {
                    mDepList[i].Retain();
                }
                return base.Retain();
            }

            public override void Release(int _count)
            {
                for (int i = 0; i < mDepList.Count; i++)
                {
                    mDepList[i].Release(_count);
                }
                base.Release(_count);
            }

            public override void Destory()
            {
                for (int i = 0; i < mDepList.Count; i++)
                {
                    mDepList[i].Release(mPCount);
                }
                mDepList.Clear();
                base.Destory();
            }

        }
    }  
}

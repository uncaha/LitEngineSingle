using System.Collections.Generic;
namespace LitEngine
{
    namespace Loader
    {
        public class AssetsBundleHaveDependencieAsync : AssetsBundleHaveDependencie
        {
            public delegate BaseBundle LoadAssetAsyncRetain(string _key, string _AssetsName, System.Action<string, object> _callback, bool _retain);
            private int mNeedLoadCount = 0;
            private int mLoadedCount = 0;
            private LoadAssetAsyncRetain mLoadCall = null;
            public AssetsBundleHaveDependencieAsync(string _assetsname, LoadAssetAsyncRetain _loadcall) : base(_assetsname)
            {
                mLoadCall = _loadcall;
            }

            public override void Load(LoaderManager _loader)
            {
                string[] tdeps = _loader.GetDirectDependencies(mAssetName);
                mNeedLoadCount = tdeps != null ? tdeps.Length : 0;
                if (tdeps != null)
                {
                    for (int i = 0; i < tdeps.Length; i++)
                    {
                        string tdepassetname = tdeps[i];
                        BaseBundle tchile = mLoadCall(tdepassetname, tdepassetname, DependencieCallBack,false);
                        mDepList.Add(tchile);
                    }
                }
                mMainBundle = new AssetsBundleAsyncFromFile(mAssetName);
                mMainBundle.Load(_loader);
                mStartLoad = true;
            }

            protected void DependencieCallBack(string _key,object _res)
            {
                LoadedOne();
            }

            private void LoadedOne()
            {
                mLoadedCount++;
            }

            override public bool IsDone()
            {
                if (!base.IsDone()) return false;
                if (Loaded) return true;
                if (!mMainBundle.IsDone()) return false;
                if (mLoadedCount < mNeedLoadCount) return false;
                LoadEnd();
                return true;
            }
        }
    } 
}

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

            public override void Load()
            {
                string[] tdeps = LoaderManager.GetDirectDependencies(mAssetName);
                mNeedLoadCount = tdeps != null ? tdeps.Length : 0;
                if (tdeps != null)
                {
                    for (int i = 0; i < tdeps.Length; i++)
                    {
                        string tdepassetname = DeleteSuffixName(tdeps[i]);
                        BaseBundle tchile = mLoadCall(tdepassetname, tdepassetname, DependencieCallBack,false);
                        if(tchile != null)
                            mDepList.Add(tchile);
                    }
                }
                mMainBundle = new AssetsBundleAsyncFromFile(mAssetName,true);
                mMainBundle.Load();

                mStartLoad = true;
                Step = StepState.BundleLoad;
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
                    case StepState.ChildenAssetLoad:
                        return ChildenAssetsLoad();
                    default:
                        return false;
                }
            }

            private bool BundleLoad()
            {
                mMainBundle.IsDone();
                if (mMainBundle.Step == StepState.BundleLoad) return false;
                for (int i = 0; i < mDepList.Count; i++)
                {
                    
                    if (mDepList[i].Step == StepState.BundleLoad)
                        return false;
                }
                StartLoadAssets();
                return false;
            }

            private bool AssetsLoad()
            {
                if (!mMainBundle.IsDone()) return false;
                if (mLoadedCount < mNeedLoadCount) return false;
                LoadEnd();
                return false;
            }

            private bool ChildenAssetsLoad()
            {
                for (int i = 0; i < mDepList.Count; i++)
                {
                    if (!mDepList[i].IsDone())
                        return false;
                }
                ((AssetsBundleAsyncFromFile)mMainBundle).StartLoadAssets();
                Step = StepState.AssetsLoad;
                return false;
            }

            public void StartLoadAssets()
            {
                if(mDepList.Count > 0 )
                {
                    for (int i = 0; i < mDepList.Count; i++)
                    {
                        if (mDepList[i].Loaded) continue;
                        AssetsBundleHaveDependencieAsync tbd = mDepList[i] as AssetsBundleHaveDependencieAsync;
                        if (tbd == null)
                        {
                            UnityEngine.Debug.LogErrorFormat("异步加载强制转换出现错误.类型 ={0},所属资源 = {1}", mDepList[i].GetType(), mAssetName);
                            continue;
                        }
                        if (tbd.Step == StepState.WaitingLoadAsset)
                            tbd.StartLoadAssets();
                    }
                    Step = StepState.ChildenAssetLoad;
                }
                else
                {
                    ((AssetsBundleAsyncFromFile)mMainBundle).StartLoadAssets();
                    Step = StepState.AssetsLoad;
                }

            }

            public override void LoadEnd()
            {
                base.LoadEnd();
                ChoseRelease();
            }
        }
    } 
}

using UnityEngine;
namespace LitEngine
{
    namespace Loader
    {
        public class BaseBundle 
        {
            #region 类属性
            static public string sSuffixName = ".bytes";
            protected string mAssetName = "";
            protected string mPathName = "";
            protected UnityEngine.Object mAssetsBundle;
            protected object mAsset;
            protected bool mIsLoaded = false;
            protected bool mStartLoad = false;
            protected int mPCount = 0;
            protected float mProgress = 0;
            protected BundleVector mParent;
            #endregion
            public BaseBundle()
            {

            }

            public BaseBundle(string _assetname)
            {
                mAssetName = DeleteSuffixName(_assetname).ToLower();
            }
            #region 静态方法
            public static string CombineSuffixName(string _assetsname)
            {
                string ret = _assetsname;
                if (!_assetsname.EndsWith(sSuffixName))
                    return _assetsname + sSuffixName;
                return _assetsname;
            }
            public static string DeleteSuffixName(string _assetsname)
            {
                if (_assetsname.EndsWith(sSuffixName))
                    return _assetsname.Replace(sSuffixName,"");
                return _assetsname;
            }
            #endregion

            #region 属性
            public float Progress
            {
                get
                {
                    return mProgress;
                }
            }
            public object Asset
            {
                get
                {
                    return mAsset;
                }
            }

            public BundleVector Parent
            {
                get
                {
                    return mParent;
                }

                set
                {
                    mParent = value;
                }
            }

            public bool Loaded
            {
                get
                {
                    return mIsLoaded;
                }
            }

            public virtual UnityEngine.Object AssetsBundleObject
            {
                get
                {
                    return mAssetsBundle;
                }
            }

            public string AssetName
            {
                get
                {
                    return mAssetName;
                }
            }
            #endregion

            public virtual bool IsDone()
            {
                if (!mStartLoad) return false;
                return true;
            }
            public virtual void Load(LoaderManager _loader)
            {
                mStartLoad = true;
            }
            public virtual void LoadEnd()
            {
                mIsLoaded = true;
            }

            #region 资源计数以及删除
            public virtual void Release(int _count)
            {
                if (!Loaded)
                {
                    DLog.LogError( "错误的释放时机，资源还没有载入完成");
                    return;
                }

                mPCount-= _count;
                if (mPCount < 0)
                    DLog.LogError("资源计数释放异常 pcount = " + mPCount);
                if (mPCount <= 0)
                    DestoryFormParent();
            }
            public virtual void Release()
            {
                Release(1);
            }

            public virtual object Retain()
            {
                mPCount++;
                return Asset;
            }

            //删除自身并且从父类列表删除
            protected virtual void DestoryFormParent()
            {
                if (Parent != null)
                    Parent.Remove(this);
            }

            public virtual void Destory()
            {
                mAsset = null;
                if (mAssetsBundle != null && mAssetsBundle.GetType() == typeof(AssetBundle))
                    ((AssetBundle)mAssetsBundle).Unload(true);
                mAssetsBundle = null;
            }
            #endregion
        }
    }
}




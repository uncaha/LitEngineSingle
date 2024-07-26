using UnityEngine;
namespace LitEngine.LoadAsset
{
    public enum StepState
    {
        None = 0,
        BundleLoad,
        AssetsLoad,
        ChildenAssetLoad,
        WaitingLoadAsset,
        LoadEnd,
    }
    public class BaseBundle
    {

        #region 类属性
        public const string sSuffixName = ".bytes";
        protected string mAssetName = "";
        protected string mPathName = "";
        protected UnityEngine.Object mAssetsBundle;
        protected object mAsset;
        protected bool mIsLoaded = false;
        protected bool mStartLoad = false;
        public bool WillBeReleased { get { return mPCount <= 0; } }
        protected int mPCount = 0;
        protected float mProgress = 0;
        protected BundleVector mParent;
        public StepState Step { get; protected set; }
        #endregion
        public BaseBundle()
        {

        }

        public BaseBundle(string _assetname)
        {
            mAssetName = DeleteSuffixName(_assetname);
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
                return _assetsname.Replace(sSuffixName, "");
            return _assetsname;
        }
        #endregion

        #region 属性
        virtual public float Progress
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
        protected void OptAssetShow()
        {
            if (mAsset != null
                && (Application.platform == RuntimePlatform.WindowsEditor
                || Application.platform == RuntimePlatform.OSXEditor
                || Application.platform == RuntimePlatform.LinuxEditor
                    )
                )
            {

                if (mAsset.GetType().Equals(typeof(UnityEngine.Material)))
                    RestShader((UnityEngine.Material)mAsset);
            }
        }

        protected void RestShader(UnityEngine.Material targetMat)
        {
            if (targetMat == null) return;
            Shader tshader = Shader.Find(targetMat.shader.name);
            if (tshader != null)
                targetMat.shader = tshader;
            else
                Debug.LogError("未能找到对应的shader.name = " + targetMat.shader.name);
        }

        public virtual bool IsDone()
        {
            if (!mStartLoad) return false;
            return true;
        }
        public virtual void Load()
        {
            mStartLoad = true;
            Step = StepState.BundleLoad;
        }
        public virtual void LoadEnd()
        {
            mIsLoaded = true;
            Step = StepState.LoadEnd;
        }

        #region 资源计数以及删除
        public virtual void Release(int _count)
        {
            mPCount -= _count;
            if (mPCount < 0)
            {
                Debug.LogWarning(AssetName + " 资源计数释放异常 pcount = " + mPCount);
                mPCount = 0;
            }

            if (!Loaded) return;
            ChoseRelease();
        }

        protected void ChoseRelease()
        {
            if (mPCount <= 0)
            {
                DestoryFormParent();
            }
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




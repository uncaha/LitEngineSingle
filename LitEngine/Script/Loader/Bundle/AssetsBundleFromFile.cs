using UnityEngine;
namespace LitEngine
{
    namespace Loader
    {
        public class AssetsBundleFromFile : BaseBundle
        {
            //只支持bundle 类型，其他种类的读取请使用异步或者直接stream读取
            public AssetsBundleFromFile(string _assetsname) : base(_assetsname)
            {

            }
            public override void Load()
            {
                base.Load();
                mPathName = LoaderManager.GetFullPath(mAssetName);

                mAssetsBundle = AssetBundle.LoadFromFile(mPathName);
                if (mAssetsBundle != null)
                {
                    if (((AssetBundle)mAssetsBundle).isStreamedSceneAssetBundle)
                        mAsset = ((AssetBundle)mAssetsBundle).mainAsset;
                    else
                        mAsset = ((AssetBundle)mAssetsBundle).LoadAsset(mAssetName);

                    if(mAsset != null && mAsset.GetType().Equals(typeof(UnityEngine.Material))
                        &&(Application.platform == RuntimePlatform.WindowsEditor
                           || Application.platform == RuntimePlatform.OSXEditor
                           || Application.platform == RuntimePlatform.LinuxEditor)
                        )
                    {
                        RestShader((UnityEngine.Material)mAsset);
                    }
                }
                else
                    DLog.LogError("AssetsBundleFromFile打开文件失败,请检查资源是否存在-" + mPathName);
                LoadEnd();
            }
        }
    }
}


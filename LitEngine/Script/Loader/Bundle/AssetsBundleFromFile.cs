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
                mPathName = LoaderManager.GetFullPath(mAssetName);

                mAssetsBundle = AssetBundle.LoadFromFile(mPathName);
                if (mAssetsBundle != null)
                {
                    if (((AssetBundle)mAssetsBundle).isStreamedSceneAssetBundle)
                        mAsset = ((AssetBundle)mAssetsBundle).mainAsset;
                    else
                        mAsset = ((AssetBundle)mAssetsBundle).LoadAsset(mAssetName);

                    if(mAsset != null && mAsset.GetType() == typeof(UnityEngine.Material)
                        &&(Application.platform == RuntimePlatform.WindowsEditor
                           || Application.platform == RuntimePlatform.OSXEditor
                           || Application.platform == RuntimePlatform.LinuxEditor)
                        )
                    {
                        
                        UnityEngine.Material tmat = (UnityEngine.Material)mAsset;
                        Shader tshader = Shader.Find(tmat.shader.name);
                        if (tshader != null)
                            tmat.shader = tshader;
                        else
                            DLog.LogError("未能找到对应的shader.name = "+ tmat.shader.name);
                        
                    }
                }
                else
                    DLog.LogError("AssetsBundleFromFile打开文件失败,请检查资源是否存在-" + mPathName);
                LoadEnd();
            }
        }
    }
}


using UnityEngine;
namespace LitEngine.LoadAsset
{
    public interface IResourcesObject
    {
        string resPath { get; }
        Object resObject { get; }
        bool disposed { get; }
        int retinCount { get; }
        void Release();
        Object Retain();
        void Dispose();
    }
    public class ResourcesAssetObject : IResourcesObject
    {
        public string resPath { get; private set; }
        public Object resObject { get { return assetbundle != null ? assetbundle.Asset as Object : null; } }
        public bool disposed { get; private set; }
        public int retinCount { get; private set; }

        private BaseBundle assetbundle;
        public ResourcesAssetObject(string path, BaseBundle pBundle)
        {
            resPath = path;
            assetbundle = pBundle;
            retinCount = 0;
        }

        public void Release()
        {
            if (disposed) return;
            retinCount--;
            if(retinCount <= 0)
            {
                Dispose();
            }
        }

        public Object Retain()
        {
            if (disposed) return null;
            retinCount++;
            return resObject;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            retinCount = 0;
            assetbundle.Release();
        }
    }

    public class ResourcesObject : IResourcesObject
    {
        public string resPath { get; private set; }
        public Object resObject { get; set; }
        public bool disposed { get; private set; }
        public int retinCount { get; private set; }

        public ResourcesObject(string pPath, Object pRes)
        {
            resPath = pPath;
            resObject = pRes;
        }

        public void Release()
        {
            if (disposed) return;
            retinCount--;
            if (retinCount <= 0)
            {
                Dispose();
            }
        }

        public Object Retain()
        {
            if (disposed) return null;
            retinCount++;
            return resObject;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            retinCount = 0;
            if(resObject != null)
            {
                Resources.UnloadAsset(resObject);
                resObject = null;
            }
        }
    }
}
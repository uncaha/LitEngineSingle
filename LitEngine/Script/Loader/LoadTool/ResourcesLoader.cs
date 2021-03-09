using System;
using UnityEngine;
namespace LitEngine.LoadAsset
{
    public interface IResourcesLoader
    {
        string resPath { get; }
        bool IsDone { get; }
        bool IsStart { get; }
        IResourcesObject resourcesObject{ get; }
        UnityEngine.Object res{ get; }
        event Action<UnityEngine.Object> onComplete;
        void Update();
        bool StartLoad();
    }

    public class ResourcesLoaded<T> : IResourcesLoader where T : UnityEngine.Object
    {
        public string resPath { get; private set; }
        public IResourcesObject resourcesObject { get; private set; }
        public UnityEngine.Object res { get{ if (resourcesObject == null) return null; return resourcesObject.resObject; } }
        public bool IsStart { get; private set; }
        public bool IsDone { get; private set; }

        public event Action<UnityEngine.Object> onComplete;
        public ResourcesLoaded(string path, UnityEngine.Object obj, Action<UnityEngine.Object> delegateOnComplete)
        {
            resPath = path;
            resourcesObject = new ResourcesObject(path, obj);
            onComplete = delegateOnComplete;
        }
        public bool StartLoad()
        {
            if (IsStart) return true;
            IsStart = true;
            return true;
        }
        public void Update()
        {
            if (IsDone) return;
            LoadEnd();
            CallComplete();
        }
        void LoadEnd()
        {
            IsDone = true;
        }

        void CallComplete()
        {
            try
            {
                if (onComplete != null)
                    onComplete(res);
            }
            catch (System.Exception error)
            {
                Debug.LogErrorFormat("ResourcesObject CallComplet失败:res = {0},path = {1},msg = {2}", res, resPath, error.ToString());
            }
        }
    }

    public class EditorAssetLoader<T> : IResourcesLoader where T : UnityEngine.Object
    {
        public string resPath { get; private set; }
        public string realPath { get; private set; }
        public IResourcesObject resourcesObject { get; private set; }
        public UnityEngine.Object res { get { if (resourcesObject == null) return null; return resourcesObject.resObject; } }
        public bool IsStart { get; private set; }
        public bool IsDone { get; private set; }
        public event Action<UnityEngine.Object> onComplete;
        private Func<string, System.Type, UnityEngine.Object> assetLoader;
        public EditorAssetLoader(string path,string pRealPath, Func<string, System.Type, UnityEngine.Object> loadFun, Action<UnityEngine.Object> delegateOnComplete)
        {
            resPath = path;
            realPath = pRealPath;
            assetLoader = loadFun;
            onComplete = delegateOnComplete;
        }

        public bool StartLoad()
        {
            if (IsStart) return true;
            resourcesObject = new ResourcesObject(realPath, assetLoader(realPath, typeof(T)));
            IsStart = true;
            return true;
        }

        public void Update()
        {
            if (IsDone) return;
            LoadEnd();
            CallComplete();

        }

        void LoadEnd()
        {
            IsDone = true;
        }

        public void CallComplete()
        {
            try
            {
                if (onComplete != null)
                    onComplete(res);
            }
            catch (System.Exception error)
            {
                Debug.LogErrorFormat("CallComplet失败:res = {0},path = {1},msg = {2}", res, resPath, error.ToString());
            }
        }
    }

    public class ResourcesLoader<T> : IResourcesLoader where T : UnityEngine.Object
    {
        public string resPath { get; private set; }
        public event Action<UnityEngine.Object> onComplete;

        public IResourcesObject resourcesObject { get; private set; }
        public UnityEngine.Object res { get { if (resourcesObject == null) return null; return resourcesObject.resObject; } }
        ResourceRequest request;
        public bool IsStart { get; private set; }
        public bool IsDone { get; private set; }

        public ResourcesLoader(string path, Action<UnityEngine.Object> delegateOnComplete)
        {
            resPath = path;
            onComplete = delegateOnComplete;
        }

        public bool StartLoad()
        {
            if (IsStart) return true;
            if (resPath == null)
            {
                LoadEnd();
                CallComplete();
                return false;
            }
            request = Resources.LoadAsync<T>(resPath);
            request.priority = 255;

            resourcesObject = new ResourcesObject(resPath, null);
            if (request == null)
            {
                LoadEnd();
                CallComplete();
                return false;
            }
            IsStart = true;
            return true;
        }

        public void Update()
        {
            if (IsDone) return;
            if (request != null && !request.isDone) return;
            LoadEnd();
            CallComplete();

        }

        void LoadEnd()
        {
            IsDone = true;
        }

        public void CallComplete()
        {
            try
            {
                if (request != null)
                {
                    ((ResourcesObject)resourcesObject).resObject = request.asset;
                }

                if (onComplete != null)
                    onComplete(res);
            }
            catch (System.Exception error)
            {
                Debug.LogErrorFormat("CallComplet失败:res = {0},path = {1},msg = {2}", res, resPath, error.ToString());
            }
        }
    }

    public class ResourcesAssetLoader<T> : IResourcesLoader where T : UnityEngine.Object
    {
        public string resPath { get; private set; }
        public string realPath { get; private set; }
        
        public IResourcesObject resourcesObject { get; private set; }
        public UnityEngine.Object res { get { if (resourcesObject == null) return null; return resourcesObject.resObject; } }

        public bool IsStart { get; private set; }
        public bool IsDone { get; private set; }
        public bool IsLoaded { get; private set; }

        public event Action<UnityEngine.Object> onComplete;
        public ResourcesAssetLoader(string path, string pRealPath, Action<UnityEngine.Object> delegateOnComplete)
        {
            resPath = path;
            realPath = pRealPath;
            onComplete = delegateOnComplete;
            IsLoaded = false;
        }

        public bool StartLoad()
        {
            if (IsStart) return true;
            if (realPath == null)
            {
                LoadEnd();
                CallComplete();
                return false;
            }
            var tbundle = LoaderManager.LoadAssetAsync(resPath, realPath, LoadCallBack);
            resourcesObject = new ResourcesAssetObject(realPath, tbundle);
            IsStart = true;
            return true;
        }

        void LoadCallBack(string pKey, object pRes)
        {
            IsLoaded = true;
        }

        public void Update()
        {
            if (IsDone) return;
            if (!IsLoaded) return;
            LoadEnd();
            CallComplete();
        }

        void LoadEnd()
        {
            IsDone = true;
        }

        public void CallComplete()
        {
            try
            {
                if (onComplete != null)
                    onComplete(resourcesObject.Retain());
            }
            catch (System.Exception error)
            {
                Debug.LogErrorFormat("CallComplet失败:res = {0},path = {1},msg = {2}", res, resPath, error.ToString());
            }
        }
    }
}

using System;
using UnityEngine;
namespace LitEngine.LoadAsset
{
    public interface IResourcesLoader
    {
        string resPath { get; }
        bool IsDone { get; }
        bool IsStart { get; }
        UnityEngine.Object res { get; }
        void Update();
        bool StartLoad();
    }

    public class ResourcesObject<T> : IResourcesLoader where T : UnityEngine.Object
    {
        public string resPath { get; private set; }
        public UnityEngine.Object res { get; private set; }
        public bool IsStart { get; private set; }
        public bool IsDone { get; private set; }

        private Action<T> onComplete;
        public ResourcesObject(string path, UnityEngine.Object obj, Action<T> delegateOnComplete)
        {
            resPath = path;
            res = obj;
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
                    onComplete((T)res);
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
        public UnityEngine.Object res { get; private set; }
        public bool IsStart { get; private set; }
        public bool IsDone { get; private set; }
        private Action<T> onComplete;
        private Func<string, System.Type, UnityEngine.Object> assetLoader;
        public EditorAssetLoader(string path, Func<string, System.Type, UnityEngine.Object> loadFun, Action<T> delegateOnComplete)
        {
            resPath = path;
            assetLoader = loadFun;
            onComplete = delegateOnComplete;
        }

        public bool StartLoad()
        {
            if (IsStart) return true;
            res = assetLoader(resPath, typeof(T));
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
                    onComplete((T)res);
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
        Action<T> onComplete;
        T resObejct;
        public UnityEngine.Object res { get { return resObejct; } }
        ResourceRequest request;
        public bool IsStart { get; private set; }
        public bool IsDone { get; private set; }
        public ResourcesLoader(string path, Action<T> delegateOnComplete)
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
            if (!request.isDone) return;
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
                    resObejct = request.asset as T;
                }

                if (onComplete != null)
                    onComplete(resObejct);
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
        Action<T> onComplete;
        T resObejct;
        public UnityEngine.Object res { get { return resObejct; } }

        public bool IsStart { get; private set; }
        public bool IsDone { get; private set; }
        public bool IsLoaded { get; private set; }
        public ResourcesAssetLoader(string path, string pRealPath, Action<T> delegateOnComplete)
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
            LoaderManager.LoadAssetAsync(resPath, realPath, LoadCallBack);
            IsStart = true;
            return true;
        }

        void LoadCallBack(string pKey, object pRes)
        {
            resObejct = pRes as T;
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
                    onComplete(resObejct);
            }
            catch (System.Exception error)
            {
                Debug.LogErrorFormat("CallComplet失败:res = {0},path = {1},msg = {2}", res, resPath, error.ToString());
            }
        }
    }
}

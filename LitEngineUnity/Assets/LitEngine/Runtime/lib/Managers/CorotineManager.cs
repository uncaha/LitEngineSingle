using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
namespace LitEngine
{
    public class CorotineObject : MonoBehaviour
    {
        public object Target;
    }
    public class CorotineManager : ManagerInterface
    {
        private static bool IsDispose = false;
        private static CorotineManager sInstance = null;
        private static CorotineManager Instance
        {
            get
            {
                if (sInstance == null)
                {
                    IsDispose = false;
                    sInstance = new CorotineManager();
                }
                return sInstance;
            }
        }
        private CorotineObject RootCorotineObject;
        private GameObject SelfObject;
        private Dictionary<object, CorotineObject> CorotineDic = new Dictionary<object, CorotineObject>();
        public CorotineManager()
        {
            SelfObject = new GameObject("CorotineManager");
            UnityEngine.Object.DontDestroyOnLoad(SelfObject);
            RootCorotineObject = SelfObject.AddComponent<CorotineObject>();
            RootCorotineObject.Target = this;
        }
        #region 释放
        bool mDisposed = false;
        ~CorotineManager()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool _disposing)
        {
            if (mDisposed)
                return;

            if (_disposing)
                DisposeNoGcCode();
            CorotineDic.Clear();
            mDisposed = true;

            IsDispose = true;
            sInstance = null;
        }

        virtual protected void DisposeNoGcCode()
        {
            UnityEngine.Object.DestroyImmediate(SelfObject);
        }
        #endregion

        #region 方法
        //获取的对象不会自动释放.必须手动调用Remove.
        static public CorotineObject GetCorotineObjectForTarget(object target)
        {
            if (target == null) return null;
            if (Instance.CorotineDic.ContainsKey(target)) return Instance.CorotineDic[target];
            GameObject tobj = new GameObject(target.ToString());
            tobj.transform.SetParent(Instance.SelfObject.transform,true);
            CorotineObject ret = tobj.AddComponent<CorotineObject>();
            ret.Target = target;
            Instance.CorotineDic.Add(target, ret);
            return ret;
        }

        static public void RemoveCorotineObjectByTarget(object target)
        {
            if (target == null || IsDispose) return;
            if (!Instance.CorotineDic.ContainsKey(target)) return;

            CorotineObject tobj = Instance.CorotineDic[target];

            Instance.CorotineDic.Remove(target);
            UnityEngine.Object.DestroyImmediate(tobj.gameObject);
        }

        static public void Start(IEnumerator _enumerator)
        {
            Instance.RootCorotineObject.StartCoroutine(_enumerator);
        }

        static public void Stop(IEnumerator _enumerator)
        {
            if (IsDispose) return;
            Instance.RootCorotineObject.StopCoroutine(_enumerator);
        }

        static public void StopAll()
        {
            if (IsDispose) return;
            Instance.RootCorotineObject.StopAllCoroutines();
        }
        #endregion
    }
}

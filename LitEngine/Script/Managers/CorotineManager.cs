using UnityEngine;
using System.Collections;
namespace LitEngine
{
    class CorotineMonoS : MonoBehaviour
    {

    }
    public class CorotineManager : ManagerInterface
    {
        private GameObject mCorotineobject = null;
        private CorotineMonoS mScriptMono = null;
        public string AppName { get; private set; }
        #region 构造
        public CorotineManager(string _appname,GameCore _core)
        {
            AppName = _appname;
            mCorotineobject = new GameObject("CorotineManager-" + AppName);
            mScriptMono = mCorotineobject.AddComponent<CorotineMonoS>();
            _core.DontDestroyOnLoad(mCorotineobject);
        }
        #endregion

        #region 释放
        bool mDisposed = false;
        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool _disposing)
        {
            if (mDisposed)
                return;
            mDisposed = true;

            if (_disposing)
                DisposeNoGcCode();
            
            mCorotineobject = null;
            mScriptMono = null;
        }

        virtual protected void DisposeNoGcCode()
        {
        }

        ~CorotineManager()
        {
            Dispose(false);
        }
        #endregion

        #region 方法
        public void StartCoroutine(IEnumerator _enumerator)
        {
            mScriptMono.StartCoroutine(_enumerator);
        }
        public void StopCoroutine(IEnumerator _enumerator)
        {
            mScriptMono.StopCoroutine(_enumerator);
        }

        public void StopAllCoroutines()
        {
            mScriptMono.StopAllCoroutines();
        }
        #endregion
    }
}

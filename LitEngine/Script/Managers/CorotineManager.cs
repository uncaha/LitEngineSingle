using UnityEngine;
using System.Collections;
namespace LitEngine
{

    public class CorotineManager : MonoManagerBase
    {
        private static CorotineManager sInstance = null;
        private static CorotineManager Instance
        {
            get
            {
                if (sInstance == null)
                {
                    GameObject tobj = new GameObject("CorotineManager");
                    GameObject.DontDestroyOnLoad(tobj);
                    sInstance = tobj.AddComponent<CorotineManager>();
                }
                return sInstance;
            }
        }
        #region 释放
        override protected void OnDestroy()
        {
            base.OnDestroy();
        }
        #endregion

        #region 方法
        static public void Start(IEnumerator _enumerator)
        {
            Instance.StartCoroutine(_enumerator);
        }
        static public void Stop(IEnumerator _enumerator)
        {
            Instance.StopCoroutine(_enumerator);
        }

        static public void StopAll()
        {
            Instance.StopAllCoroutines();
        }
        #endregion
    }
}

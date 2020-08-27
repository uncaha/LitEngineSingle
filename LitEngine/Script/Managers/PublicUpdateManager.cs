using System;
using System.Collections.Generic;
using UnityEngine;
namespace LitEngine
{
    using UpdateSpace;
    using DownLoad;
    using UnZip;
    public class PublicUpdateManager : MonoManagerBase
    {
        private UpdateObjectVector mUpdateList = new UpdateObjectVector(UpdateType.Update);
        private static PublicUpdateManager sInstance = null;
        private static PublicUpdateManager Instance
        {
            get
            {
                if (sInstance == null)
                {
                    GameObject tobj = new GameObject("PublicUpdateManager");
                    GameObject.DontDestroyOnLoad(tobj);
                    sInstance = tobj.AddComponent<PublicUpdateManager>();
                }
                return sInstance;
            }
        }

        public static UpdateObjectVector UpdateList
        {
            get
            {
                return Instance.mUpdateList;
            }
        }

        override protected void OnDestroy()
        {
            sInstance = null;
            mUpdateList.Clear();
            base.OnDestroy();
        }

        public PublicUpdateManager()
        {

        }

        static public void SetActive(bool _active)
        {
            if (Instance.gameObject.activeSelf != _active)
                Instance.gameObject.SetActive(_active);
        }

        static public void AddUpdate(UpdateBase _updateobj)
        {
            UpdateList.Add(_updateobj);
            SetActive(true);
        }

        static public void RemoveUpdate(UpdateBase _updateobj)
        {
            UpdateList.Remove(_updateobj);
        }

        void Update()
        {
            mUpdateList.Update();
            if (mUpdateList.Count == 0)
                gameObject.SetActive(false);
        }
    }
}

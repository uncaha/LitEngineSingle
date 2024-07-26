using System;
using System.Collections.Generic;
using UnityEngine;
namespace LitEngine
{
    using UpdateSpace;
    using DownLoad;
    using UnZip;
    public class PublicUpdateManager : MonoManagerGeneric<PublicUpdateManager>
    {
        private UpdateObjectVector mUpdateList = new UpdateObjectVector(UpdateType.Update);

        public static UpdateObjectVector UpdateList
        {
            get
            {
                return Instance.mUpdateList;
            }
        }
        
        protected override void Init()
        {

        }

        protected void OnDestroy()
        {
            sInstance = null;
            mUpdateList.Clear();
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

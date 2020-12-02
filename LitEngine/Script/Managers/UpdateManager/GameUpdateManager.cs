using UnityEngine;
using System.Collections.Generic;
using LitEngine.UpdateSpace;
namespace LitEngine
{
    public class GameUpdateManager : MonoManagerBase
    {
        private static object lockobj = new object();
        private static GameUpdateManager sInstance = null;
        public static GameUpdateManager Instance
        {
            get
            {
                if (sInstance == null)
                {
                    lock (lockobj)
                    {
                        if (sInstance == null)
                        {
                            GameObject tobj = new GameObject("GameUpdateManager");
                            GameObject.DontDestroyOnLoad(tobj);
                            sInstance = tobj.AddComponent<GameUpdateManager>();
                        }
                    }
                }

                return sInstance;
            }
        }
        override protected void OnDestroy()
        {
            Clear();
            base.OnDestroy();
        }
        public bool mIsFixedUpdate = true;
        public bool mIsUpdate = true;
        public bool mIsLateUpdate = true;
        public bool mIsGUIUpdate = true;

        public readonly UpdateObjectVector UpdateList = new UpdateObjectVector(UpdateType.Update);
        public readonly UpdateObjectVector FixedUpdateList = new UpdateObjectVector(UpdateType.FixedUpdate);
        public readonly UpdateObjectVector LateUpdateList = new UpdateObjectVector(UpdateType.LateUpdate);
        public readonly UpdateObjectVector OnGUIList = new UpdateObjectVector(UpdateType.OnGUI);

        public GameUpdateManager()
        {

        }
        #region 注册
        static internal void InsertUpdate(int pIndex,UpdateBase pSor)
        {
            Instance.UpdateList.Insert(pIndex,pSor);
        }
        static public void RegUpdate(UpdateBase _act)
        {
            Instance.UpdateList.Add(_act);
        }
        static public void RegLateUpdate(UpdateBase _act)
        {
            Instance.LateUpdateList.Add(_act);
        }
        static public void RegFixedUpdate(UpdateBase _act)
        {
            Instance.FixedUpdateList.Add(_act);
        }
        static public void RegGUIUpdate(UpdateBase _act)
        {
            Instance.OnGUIList.Add(_act);
        }
        #endregion

        #region 销毁
        protected void Clear()
        {
            UpdateList.Clear();
            FixedUpdateList.Clear();
            LateUpdateList.Clear();
            OnGUIList.Clear();
        }
        static public void UnRegUpdate(UpdateBase _act)
        {
            Instance.UpdateList.Remove(_act);

        }
        static public void UnRegLateUpdate(UpdateBase _act)
        {
            Instance.LateUpdateList.Remove(_act);
        }
        static public void UnRegFixedUpdate(UpdateBase _act)
        {
            Instance.FixedUpdateList.Remove(_act);
        }
        static public void UnGUIUpdate(UpdateBase _act)
        {
            Instance.OnGUIList.Remove(_act);
        }
        #endregion

        #region Updates
        void Update()
        {
            if (!mIsUpdate) return;
            UpdateList.Update();
        }

        void LateUpdate()
        {
            if (!mIsLateUpdate) return;
            LateUpdateList.Update();
        }

        void FixedUpdate()
        {
            if (!mIsFixedUpdate) return;
            FixedUpdateList.Update();
        }

        #endregion

        #region OnRender
        void OnGUI()
        {
            if (!mIsGUIUpdate) return;
            OnGUIList.Update();
        }
        #endregion

        override public void DestroyManager()
        {
            base.DestroyManager();
        }
    }
}


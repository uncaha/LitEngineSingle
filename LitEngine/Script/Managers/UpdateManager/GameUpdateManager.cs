using UnityEngine;
using System.Collections.Generic;
namespace LitEngine
{
    using UpdateSpace;
    public class GameUpdateManager : MonoManagerBase
    {

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
        public void RegUpdate(UpdateBase _act)
        {
            UpdateList.Add(_act);
        }
        public void RegLateUpdate(UpdateBase _act)
        {
            LateUpdateList.Add(_act);
        }
        public void RegFixedUpdate(UpdateBase _act)
        {
            FixedUpdateList.Add(_act);
        }
        public void RegGUIUpdate(UpdateBase _act)
        {
            OnGUIList.Add(_act);
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
        public void UnRegUpdate(UpdateBase _act)
        {
            UpdateList.Remove(_act);

        }
        public void UnRegLateUpdate(UpdateBase _act)
        {
            LateUpdateList.Remove(_act);
        }
        public void UnRegFixedUpdate(UpdateBase _act)
        {
            FixedUpdateList.Remove(_act);
        }
        public void UnGUIUpdate(UpdateBase _act)
        {
            OnGUIList.Remove(_act);
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


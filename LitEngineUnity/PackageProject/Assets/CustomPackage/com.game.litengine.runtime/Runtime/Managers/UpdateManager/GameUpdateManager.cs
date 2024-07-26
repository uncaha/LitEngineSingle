using UnityEngine;
using System.Collections.Generic;
using LitEngine.UpdateSpace;
using LitEngine.Method;
namespace LitEngine
{
    public class GameUpdateManager : MonoManagerGeneric<GameUpdateManager>
    {
        protected override void Init()
        {

        }
        protected void OnDestroy()
        {
            Clear();
        }
        public bool mIsFixedUpdate = true;
        public bool mIsUpdate = true;
        public bool mIsLateUpdate = true;
        public bool mIsGUIUpdate = true;

        private readonly UpdateObjectVector UpdateList = new UpdateObjectVector(UpdateType.Update);
        private readonly UpdateObjectVector FixedUpdateList = new UpdateObjectVector(UpdateType.FixedUpdate);
        private readonly UpdateObjectVector LateUpdateList = new UpdateObjectVector(UpdateType.LateUpdate);
        private readonly UpdateObjectVector OnGUIList = new UpdateObjectVector(UpdateType.OnGUI);

        private GameUpdateManager()
        {

        }

        #region 设置父列表
        static public void SetUpdateOwner(UpdateBase pDelgate)
        {
            if(pDelgate.IsRegToOwner)
            {
                pDelgate.UnRegToOwner();
            }
            pDelgate.Owner = Instance.UpdateList;
        }
        static public void SetLateUpdateOwner(UpdateBase pDelgate)
        {
            if (pDelgate.IsRegToOwner)
            {
                pDelgate.UnRegToOwner();
            }
            pDelgate.Owner = Instance.LateUpdateList;
        }
        static public void SetFixedUpdateOwner(UpdateBase pDelgate)
        {
            if (pDelgate.IsRegToOwner)
            {
                pDelgate.UnRegToOwner();
            }
            pDelgate.Owner = Instance.FixedUpdateList;
        }
        static public void SetGUIUpdateOwner(UpdateBase pDelgate)
        {
            if (pDelgate.IsRegToOwner)
            {
                pDelgate.UnRegToOwner();
            }
            pDelgate.Owner = Instance.OnGUIList;
        }
        #endregion
        #region 注册
        static public void RegUpdate(UpdateBase pDelgate)
        {
            Instance.UpdateList.Add(pDelgate);
        }
        static public void RegLateUpdate(UpdateBase pDelgate)
        {
            Instance.LateUpdateList.Add(pDelgate);
        }
        static public void RegFixedUpdate(UpdateBase pDelgate)
        {
            Instance.FixedUpdateList.Add(pDelgate);
        }
        static public void RegGUIUpdate(UpdateBase pDelgate)
        {
            Instance.OnGUIList.Add(pDelgate);
        }

        static public UpdateBase RegUpdate(System.Action pDelgate,string pKey)
        {
            var ret = new UpdateObject(pKey, new MethodActionSharp(pDelgate),pDelgate.Target);
            Instance.UpdateList.Add(ret);
            return ret;
        }
        static public UpdateBase RegLateUpdate(System.Action pDelgate, string pKey = null)
        {
            var ret = new UpdateObject(pKey, new MethodActionSharp(pDelgate), pDelgate.Target);
            Instance.LateUpdateList.Add(ret);
            return ret;
        }
        static public UpdateBase RegFixedUpdate(System.Action pDelgate, string pKey)
        {
            var ret = new UpdateObject(pKey, new MethodActionSharp(pDelgate), pDelgate.Target);
            Instance.FixedUpdateList.Add(ret);
            return ret;
        }
        static public UpdateBase RegGUIUpdate(System.Action pDelgate, string pKey)
        {
            var ret = new UpdateObject(pKey, new MethodActionSharp(pDelgate), pDelgate.Target);
            Instance.OnGUIList.Add(ret);
            return ret;
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
        static public void UnRegUpdate(UpdateBase pDelgate)
        {
            Instance.UpdateList.Remove(pDelgate);

        }
        static public void UnRegLateUpdate(UpdateBase pDelgate)
        {
            Instance.LateUpdateList.Remove(pDelgate);
        }
        static public void UnRegFixedUpdate(UpdateBase pDelgate)
        {
            Instance.FixedUpdateList.Remove(pDelgate);
        }
        static public void UnGUIUpdate(UpdateBase pDelgate)
        {
            Instance.OnGUIList.Remove(pDelgate);
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
    }
}


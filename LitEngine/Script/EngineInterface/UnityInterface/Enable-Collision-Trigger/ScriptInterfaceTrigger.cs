using UnityEngine;
using System;

namespace LitEngine
{
    namespace ScriptInterface
    {
        public class ScriptInterfaceTrigger : ScriptInterfaceCETBase
        {
            private bool _CanEnter = true;
            public bool CanEnter
            {
                get { return _CanEnter; }
                set
                {
                    _CanEnter = value;
                    if (SelfCollider != null)
                        SelfCollider.enabled = value;
                    enabled = value;
                }
            }

            protected Collider SelfCollider = null;
            #region mymethod
            protected Action<Collider> mOnTriggerEnter;
            protected Action<Collider> mOnTriggerExit;
            #endregion
            #region 脚本初始化以及析构
            public ScriptInterfaceTrigger()
            {

            }

            override protected void Awake()
            {
                base.Awake();
                SelfCollider = GetComponent<Collider>();
            }

            override public void ClearScriptObject()
            {
                mOnTriggerEnter = null;
                mOnTriggerExit = null;
                base.ClearScriptObject();
            }
            override protected void InitParamList()
            {
                base.InitParamList();
                mOnTriggerEnter = mCodeTool.GetCSLEDelegate<Action<Collider>, Collider>("OnTriggerEnter", mScriptType, ScriptObject);
                mOnTriggerExit = mCodeTool.GetCSLEDelegate<Action<Collider>, Collider>("OnTriggerExit", mScriptType, ScriptObject);
            }
            #endregion
            #region Unity 
            virtual protected void OnTriggerEnter(Collider _other)
            {
                if (!CanEnter) return;
                if (mOnTriggerEnter == null) return;
                if (mTriggerTarget != null && !mTriggerTarget.Equals(_other.transform)) return;
                if (!string.IsNullOrEmpty(TriggerTargetName) && !_other.name.Equals(TriggerTargetName)) return;

                if (mTriggerEnterTimer > Time.realtimeSinceStartup) return;
                mTriggerEnterTimer = Time.realtimeSinceStartup + mTriggerEnterInterval;
                CallAction(mOnTriggerEnter, _other);
            }
            virtual protected void OnTriggerExit(Collider _other)
            {
                if (mOnTriggerExit == null) return;
                if (mTriggerTarget != null && mTriggerTarget != _other.transform) return;
                if (!string.IsNullOrEmpty(TriggerTargetName) && !_other.name.Equals(TriggerTargetName)) return;
                CallAction(mOnTriggerExit, _other);
            }

            override protected void OnDestroy()
            {
                base.OnDestroy();
            }
            
            #endregion
        }
    }
    
}

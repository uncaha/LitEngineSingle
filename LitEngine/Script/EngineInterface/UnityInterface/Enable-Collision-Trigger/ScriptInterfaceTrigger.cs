using UnityEngine;
using System;

namespace LitEngine
{
    namespace ScriptInterface
    {
        public class ScriptInterfaceTrigger : ScriptInterfaceCETBase
        {
            #region mymethod
            protected Action<Collider> mOnTriggerEnter;
            protected Action<Collider> mOnTriggerExit;
            #endregion
            #region 脚本初始化以及析构
            public ScriptInterfaceTrigger()
            {

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
            protected void OnTriggerEnter(Collider _other)
            {
                if (mOnTriggerEnter == null) return;
                if (mTriggerTarget != null && mTriggerTarget != _other.transform) return;
                if (!_other.name.Equals(TriggerTargetName)) return;

                if (mTriggerEnterTimer > Time.realtimeSinceStartup) return;
                mTriggerEnterTimer = Time.realtimeSinceStartup + mTriggerEnterInterval;
                CallAction(mOnTriggerEnter, _other);
            }
            protected void OnTriggerExit(Collider _other)
            {
                if (mOnTriggerExit == null) return;
                if (mTriggerTarget != null && mTriggerTarget != _other.transform) return;
                if (!_other.name.Equals(TriggerTargetName)) return;
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

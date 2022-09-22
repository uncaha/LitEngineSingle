using UnityEngine;
using System;

namespace LitEngine
{
    namespace ScriptInterface
    {
        public class ScriptInterfaceCET : ScriptInterfaceCETBase
        {
            #region mymethod
            protected LitEngine.Method.MethodAction<Collision> mOnCollisionEnter;
            protected LitEngine.Method.MethodAction<Collision> mOnCollisionExit;
            protected LitEngine.Method.MethodAction<Collider> mOnTriggerEnter;
            protected LitEngine.Method.MethodAction<Collider> mOnTriggerExit;
            #endregion

            #region 构造
            public ScriptInterfaceCET()
            {

            }
            override public void ClearScriptObject()
            {
                mOnCollisionEnter = null;
                mOnCollisionExit = null;
                mOnTriggerEnter = null;
                mOnTriggerExit = null;
                base.ClearScriptObject();
            }
            override protected void InitParamList()
            {
                base.InitParamList();
                mOnCollisionEnter = mCodeTool.GetMethodAction<Collision>("OnCollisionEnter", mScriptClass, ScriptObject);
                mOnCollisionExit = mCodeTool.GetMethodAction<Collision>("OnCollisionExit", mScriptClass, ScriptObject);
                mOnTriggerEnter = mCodeTool.GetMethodAction<Collider>("OnTriggerEnter", mScriptClass, ScriptObject);
                mOnTriggerExit = mCodeTool.GetMethodAction<Collider>("OnTriggerExit", mScriptClass, ScriptObject);

            }
            #endregion
            #region Unity 

            protected void OnCollisionEnter(Collision _collision)
            {
                if (mOnCollisionEnter == null) return;
                if (!IsInTagList(_collision.gameObject)) return;
                if (mCollEnterTimer > Time.realtimeSinceStartup) return;
                mCollEnterTimer = Time.realtimeSinceStartup + mCollEnterInterval;
                mOnCollisionEnter.Call(_collision);
            }

            protected void OnCollisionExit(Collision _collision)
            {
                if (mOnCollisionExit == null) return;
                if (!IsInTagList(_collision.gameObject)) return;
                mOnCollisionExit.Call(_collision);
            }

            protected void OnTriggerEnter(Collider _other)
            {
                if (mOnTriggerEnter == null) return;
                if (mTriggerTarget != null && mTriggerTarget != _other.transform) return;
                if (!_other.name.Equals(TriggerTargetName)) return;

                if (mTriggerEnterTimer > Time.realtimeSinceStartup) return;
                mTriggerEnterTimer = Time.realtimeSinceStartup + mTriggerEnterInterval;
                mOnTriggerEnter.Call(_other);
            }
            protected void OnTriggerExit(Collider _other)
            {
                if (mOnTriggerExit == null) return;
                if (mTriggerTarget != null && mTriggerTarget != _other.transform) return;
                if (!_other.name.Equals(TriggerTargetName)) return;

                mOnTriggerExit.Call(_other);
            }

            override protected void OnDestroy()
            {
                base.OnDestroy();
            }
            
            #endregion

        }
    }
   
}


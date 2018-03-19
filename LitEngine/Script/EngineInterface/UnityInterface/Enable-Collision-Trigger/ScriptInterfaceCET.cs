using UnityEngine;
using System;
namespace LitEngine
{
    namespace ScriptInterface
    {
        public class ScriptInterfaceCET : ScriptInterfaceCETBase
        {
            #region mymethod
            protected Action<Collision> mOnCollisionEnter;
            protected Action<Collision> mOnCollisionExit;
            protected Action<Collider> mOnTriggerEnter;
            protected Action<Collider> mOnTriggerExit;
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
                mOnCollisionEnter = mCodeTool.GetCSLEDelegate<Action<Collision>, Collision>("OnCollisionEnter", mScriptType, ScriptObject);
                mOnCollisionExit = mCodeTool.GetCSLEDelegate<Action<Collision>, Collision>("OnCollisionExit", mScriptType, ScriptObject);
                mOnTriggerEnter = mCodeTool.GetCSLEDelegate<Action<Collider>, Collider>("OnTriggerEnter", mScriptType, ScriptObject);
                mOnTriggerExit = mCodeTool.GetCSLEDelegate<Action<Collider>, Collider>("OnTriggerExit", mScriptType, ScriptObject);

            }
            #endregion
            #region Unity 

            protected void OnCollisionEnter(Collision _collision)
            {
                if (mOnCollisionEnter == null) return;
                if (!IsInTagList(_collision.gameObject)) return;
                if (mCollEnterTimer > Time.realtimeSinceStartup) return;
                mCollEnterTimer = Time.realtimeSinceStartup + mCollEnterInterval;
                CallAction(mOnCollisionEnter, _collision);
            }

            protected void OnCollisionExit(Collision _collision)
            {
                if (mOnCollisionExit == null) return;
                if (!IsInTagList(_collision.gameObject)) return;
                CallAction(mOnCollisionExit, _collision);
            }

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


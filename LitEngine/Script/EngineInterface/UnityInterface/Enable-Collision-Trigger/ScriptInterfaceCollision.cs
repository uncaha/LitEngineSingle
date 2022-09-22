using UnityEngine;
using System;
namespace LitEngine
{
    namespace ScriptInterface
    {
        public class ScriptInterfaceCollision : ScriptInterfaceCETBase
        {
            #region mymethod
            protected LitEngine.Method.MethodAction<Collision> mOnCollisionEnter;
            protected LitEngine.Method.MethodAction<Collision> mOnCollisionExit;

            public Collider AptCol { get; set; }
            #endregion
            #region 脚本初始化以及析构
            public ScriptInterfaceCollision()
            {

            }
            override public void ClearScriptObject()
            {
                mOnCollisionEnter = null;
                mOnCollisionExit = null;
                base.ClearScriptObject();
            }
            override protected void InitParamList()
            {
                base.InitParamList();
                mOnCollisionEnter = mCodeTool.GetMethodAction< Collision>("OnCollisionEnter", mScriptClass, ScriptObject);
                mOnCollisionExit = mCodeTool.GetMethodAction<Collision>("OnCollisionExit", mScriptClass, ScriptObject);
            }
            #endregion
            #region Unity 
            protected void OnCollisionEnter(Collision _collision)
            {
                if (mOnCollisionEnter == null) return;
                if (AptCol != null && !AptCol.Equals(_collision.contacts[0].thisCollider)) return;
                if (!IsInTagList(_collision.gameObject)) return;
                if (mCollEnterTimer > Time.realtimeSinceStartup) return;
                mCollEnterTimer = Time.realtimeSinceStartup + mCollEnterInterval;

                mOnCollisionEnter.Call(_collision);
            }

            protected void OnCollisionExit(Collision _collision)
            {
                if (mOnCollisionExit == null) return;
               // if (AptCol != null && !AptCol.Equals(_collision.contacts[0].thisCollider)) return;
                if (!IsInTagList(_collision.gameObject)) return;
                mOnCollisionExit.Call(_collision);
            }

            override protected void OnDestroy()
            {
                base.OnDestroy();
            }
            
            #endregion
        }
    }
    
}

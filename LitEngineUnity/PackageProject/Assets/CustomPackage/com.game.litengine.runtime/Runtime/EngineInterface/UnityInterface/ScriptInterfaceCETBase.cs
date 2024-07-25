using System;
using UnityEngine;
using System.Collections.Generic;
namespace LitEngine
{
    namespace ScriptInterface
    {
        public class ScriptInterfaceCETBase :BehaviourInterfaceBase
        {
            #region 碰撞和trigger间隔
            protected List<string> mCollIsionObjectTagList = new List<string>();
            protected int mCollListTagCount = 0;
            protected float mCollEnterTimer = 0;
            protected float mCollEnterInterval = 1;
            protected float mTriggerEnterTimer = 0;
            protected float mTriggerEnterInterval = 1;
            protected Transform mTriggerTarget = null;
            protected string mTriggerTargetName = "";
            public string TriggerTargetName
            {
                get { return mTriggerTargetName; }
                set { mTriggerTargetName = value; }
            }
            public Transform TriggerTarget
            {
                get { return mTriggerTarget; }
                set { mTriggerTarget = value; }
            }
            public float CollEnterInterval
            {
                get { return mCollEnterInterval; }
                set { mCollEnterInterval = value; }
            }
            public float TriggerEnterInterval
            {
                get { return mTriggerEnterInterval; }
                set { mTriggerEnterInterval = value; }
            }
            #endregion
            #region 脚本初始化以及析构
            public ScriptInterfaceCETBase()
            {

            }

            override public void ClearScriptObject()
            {
                mCollIsionObjectTagList.Clear();
                mTriggerTarget = null;
                base.ClearScriptObject();
            }

            override protected void OnDestroy()
            {
                base.OnDestroy();
            }
            #endregion

            #region 检测方法
            virtual protected bool IsInTagList(GameObject _obj)
            {
                if (mCollListTagCount == 0) return true;
                for (int i = 0; i < mCollListTagCount; i++)
                {
                    if (_obj.CompareTag(mCollIsionObjectTagList[i]))
                        return true;
                }
                return false;
            }
            virtual public void AddCollIsionTag(string _obj)
            {
                mCollIsionObjectTagList.Add(_obj);
                mCollListTagCount++;
            }
            #endregion
        }
    }
    
}

using System;
using LitEngine.Method;

namespace LitEngine
{
    namespace ScriptInterface
    {
        public class ScriptInterfaceMouse : BehaviourInterfaceBase
        {
            #region mymethod
            protected Method_Action mOnMouseDown;
            protected Method_Action mOnMouseDrag;
            protected Method_Action mOnMouseEnter;
            protected Method_Action mOnMouseExit;
            protected Method_Action mOnMouseOver;
            #endregion
            #region 脚本初始化以及析构
            public ScriptInterfaceMouse()
            {

            }

            override public void ClearScriptObject()
            {
                mOnMouseDown = null;
                mOnMouseDrag = null;
                mOnMouseEnter = null;
                mOnMouseExit = null;
                mOnMouseOver = null;
                base.ClearScriptObject();
            }
            override protected void InitParamList()
            {
                base.InitParamList();
                mOnMouseDown = mCodeTool.GetMethodAction("OnMouseDown", mScriptClass, ScriptObject);
                mOnMouseDrag = mCodeTool.GetMethodAction("OnMouseDrag", mScriptClass, ScriptObject);
                mOnMouseEnter = mCodeTool.GetMethodAction("OnMouseEnter", mScriptClass, ScriptObject);
                mOnMouseExit = mCodeTool.GetMethodAction("OnMouseExit", mScriptClass, ScriptObject);
                mOnMouseOver = mCodeTool.GetMethodAction("OnMouseOver", mScriptClass, ScriptObject);
            }
            #endregion
            #region Unity 
            
            protected void OnMouseDown()
            {
                mOnMouseDown.Call();
            }
            protected void OnMouseDrag()
            {
                mOnMouseDrag.Call();
            }
            protected void OnMouseEnter()
            {
                mOnMouseEnter.Call();
            }
            protected void OnMouseExit()
            {
                mOnMouseExit.Call();
            }
            protected void OnMouseOver()
            {
                mOnMouseOver.Call();
            }

            override protected void OnDestroy()
            {
                base.OnDestroy();
            }
            #endregion
        }
    }
    
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LitEngine
{
    namespace ScriptInterface
    {
        public class ScriptInterfaceBecame : BehaviourInterfaceBase
        {
            #region 脚本初始化以及析构
            public ScriptInterfaceBecame()
            {

            }

            override protected void InitParamList()
            {
                base.InitParamList();
            }

            override public void ClearScriptObject()
            {
                base.ClearScriptObject();
            }
            #endregion
            #region Unity 
            protected void OnBecameInvisible()
            {
                CallScriptFunctionByName("OnBecameInvisible");
            }
            protected void OnBecameVisible()
            {
                CallScriptFunctionByName("OnBecameVisible");
            }

            override protected void OnDestroy()
            {
                base.OnDestroy();
            }
            #endregion
        }
    }
    
}

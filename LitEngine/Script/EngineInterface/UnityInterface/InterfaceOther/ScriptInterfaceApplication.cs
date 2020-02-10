using System;
namespace LitEngine
{
    namespace ScriptInterface
    {
        public class ScriptInterfaceApplication : BehaviourInterfaceBase
        {
            #region 脚本初始化以及析构
            public ScriptInterfaceApplication()
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
            protected void OnApplicationFocus(bool _hasFocus)
            {
                CallScriptFunctionByNameParams("OnApplicationFocus", _hasFocus);
            }
            protected void OnApplicationPause(bool _pauseStatus)
            {
                CallScriptFunctionByNameParams("OnApplicationPause", _pauseStatus);
            }
            protected void OnApplicationQuit()
            {
                CallFunctionVoid("OnApplicationQuit");
            }
            override protected void OnDestroy()
            {
                base.OnDestroy();
            }
            #endregion
        }
    }
    
}

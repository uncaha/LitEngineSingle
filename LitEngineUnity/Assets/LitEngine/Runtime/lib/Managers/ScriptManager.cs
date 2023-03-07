
using System;
using LitEngine.CodeTool;
namespace LitEngine
{
    public class ScriptManager: ManagerInterface
    {
        public CodeToolBase CodeTool
        {
            get;
            private set;
        }
        public ScriptManager(CodeToolBase _delgateCodeTool)
        {
            CodeTool = _delgateCodeTool;
        }
#region 释放
        bool mDisposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool _disposing)
        {
            if (mDisposed)
                return;

            if (_disposing)
                DisposeNoGcCode();

            mDisposed = true;
        }

        virtual protected void DisposeNoGcCode()
        {
            if(CodeTool != null)
                CodeTool.Dispose();
        }

        ~ScriptManager()
        {
            Dispose(false);
        }
#endregion

    }
}


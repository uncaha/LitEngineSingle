#if ILRuntime
using System;
using LitEngine.Method;
using ILRuntime.CLR.Method;

namespace LitEngine.Method
{
    public class Method_LS : MethodBase
    {
        private ILRuntime.Runtime.Enviorment.AppDomain mApp;
        public IMethod mMethod;
    
        public Method_LS(ILRuntime.Runtime.Enviorment.AppDomain _app, object pTar, IMethod _method) : base(pTar)
        {
            mApp = _app;
            mMethod = _method;
        }
    
        override public object Invoke(params object[] parameters)
        {
            try
            {
                return mApp.Invoke(mMethod, target, parameters);
            }
            catch (Exception e)
            {
                DLog.LogError(e);
            }
    
            return null;
        }
    
        override public void Call()
        {
            try
            {
                mApp.Invoke(mMethod, target);
            }
            catch (Exception e)
            {
                DLog.LogError(e);
            }
        }
    }
}


#endif
#if ILRuntime
using System;
using LitEngine.Method;

namespace LitEngine.Method
{
    public class MethodActionILR : Method_Action
    {
        protected Method_LS method;
        public MethodActionILR(Method_LS pAct)
        {
            method = pAct;
        }
            
        override public void Call()
        {
            if (method == null) return;
            method.Call();
        }
    }
        
    public class MethodActionILR<T> : MethodAction<T>
    {
        protected Method_LS method;
        public MethodActionILR(Method_LS pMethod)
        {
            method = pMethod;
        }
    
        public override void Call(T pData)
        {
            if (method == null) return;
            try
            {
                method.Invoke(pData);
            }
            catch (Exception e)
            {
                DLog.LogError(e);
            }
        }
    }
}


#endif
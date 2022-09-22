using System;
namespace LitEngine.Method
{
    public class MethodActionBase : MethodBase
    {
        
    }
    public class Method_Action : MethodActionBase
    {
        protected Action action;
        public Method_Action(Action pAct)
        {
            action = pAct;
        }
        
        override public void Call()
        {
            if (action == null) return;
            action();
        }
    }

    public abstract class MethodAction<T> : MethodBase
    {
        abstract public void Call(T pData);
    }
    
    public class MethodActionCSharp<T> : MethodAction<T>
    {
        protected Action<T> action;
        public MethodActionCSharp(Action<T> pAct)
        {
            action = pAct;
        }

        public override void Call(T pData)
        {
            if (action == null) return;
            try
            {
                action.Invoke(pData);
            }
            catch (Exception e)
            {
                DLog.LogError(e);
            }
        }
    }
}

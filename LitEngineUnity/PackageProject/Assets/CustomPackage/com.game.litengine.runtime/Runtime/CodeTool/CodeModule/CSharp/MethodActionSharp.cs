using System;

namespace LitEngine.Method
{
    public class MethodActionSharp : Method_Action
    {
        protected Action action;
        public MethodActionSharp(Action pAct)
        {
            action = pAct;
        }
        
        override public void Call()
        {
            if (action == null) return;
            action();
        }
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


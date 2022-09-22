using System;
namespace LitEngine.Method
{
    public class Method_Action : MethodBase
    {
        protected Action action;
        public Method_Action(Action pAct) : base(pAct.Target)
        {
            action = pAct;
        }
        override public object Invoke(params object[] parameters)
        {
            if (action == null) return null;
            action();
            return null;
        }
        override public void Call()
        {
            if (action == null) return;
            action();
        }
    }
}

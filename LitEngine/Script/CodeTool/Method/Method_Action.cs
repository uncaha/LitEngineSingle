using System;
namespace LitEngine.Method
{
    public class Method_Action : MethodBase
    {
        protected Action action;
        public Method_Action(Action _act)
        {
            action = _act;
        }
        override public object Invoke(object obj, params object[] parameters)
        {
            if (action == null) return null;
            action();
            return null;
        }
    }
}

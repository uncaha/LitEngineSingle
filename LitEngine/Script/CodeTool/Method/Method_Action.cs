using System;

namespace LitEngine.Method
{
    public class MethodActionBase : MethodBase
    {
    }

    public abstract class Method_Action : MethodActionBase
    {
        abstract public void Call();
    }

    public abstract class MethodAction<T> : MethodBase
    {
        abstract public void Call(T pData);
    }
}
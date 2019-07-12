using System.Reflection;
namespace LitEngine.Method
{
    public class Method_CS : MethodBase
    {
        public MethodInfo SMethod { get; private set; }
        public Method_CS(MethodInfo _method)
        {
            SMethod = _method;
        }
        override public object Invoke(object obj, params object[] parameters)
        {
            return SMethod.Invoke(obj, parameters);
        }
    }
}

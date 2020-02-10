using System.Reflection;
namespace LitEngine.Method
{
    public class Method_CS : MethodBase
    {
        public MethodInfo SMethod { get; private set; }
        public Method_CS(object pTar,MethodInfo pMethod):base(pTar)
        {
            SMethod = pMethod;
        }
        override public object Invoke(params object[] parameters)
        {
            return SMethod.Invoke(target, parameters);
        }
        override public void Call()
        {
            SMethod.Invoke(target, null);
        }
    }
}


namespace LitEngine.Method
{
    public abstract class MethodBase
    {
        public MethodBase()
        {

        }

        public abstract object Invoke(object obj, params object[] parameters);

    }
}


namespace LitEngine.Method
{
    public abstract class MethodBase
    {
        protected object target;

        protected MethodBase()
        {
            
        }

        protected MethodBase(object pTar)
        {
            target = pTar;
        }

        virtual public object Invoke(params object[] parameters)
        {
            return null;
        }

        virtual public void Call()
        {
            
        }

    }
}

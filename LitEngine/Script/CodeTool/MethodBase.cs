
namespace LitEngine.Method
{
    public abstract class MethodBase
    {
        protected object target;
        public MethodBase(object pTar)
        {
            target = pTar;
        }

        public abstract object Invoke(params object[] parameters);
        public abstract void Call();

    }
}

//using System.Reflection;
//using LuaInterface;
//namespace LitEngine.Method
//{
//    public class Method_Lua : MethodBase
//    {
//        public LuaFunction LuaMethod { get; private set; }
//        public Method_Lua(object pTar, LuaFunction pMethod) : base(pTar)
//        {
//            LuaMethod = pMethod;
//        }
//        override public void Call()
//        {
//            LuaMethod.Call();//无返回值无参数
//        }
//        override public object Invoke(params object[] parameters)
//        {
//            //有gc調用
//            LuaMethod.BeginPCall();
//            LuaMethod.PushArgs(parameters);
//            LuaMethod.PCall();
//            LuaMethod.EndPCall();
//            return null;
//        }
//    }
//}

//#if USEILRUNTIME
//using ILRuntime.CLR.Method;
//namespace ILRuntime.Runtime.Enviorment
//{
//    public partial class DelegateManager
//    {
//        public bool IsRegToMethodDelegate(ILMethod method)
//        {
//            if (method.ParameterCount == 0) return true;
//            foreach (var i in methods)
//            {
//                if (i.ParameterTypes.Length == method.ParameterCount)
//                {
//                    bool match = true;
//                    for (int j = 0; j < method.ParameterCount; j++)
//                    {
//                        if (i.ParameterTypes[j] != method.Parameters[j].TypeForCLR)
//                        {
//                            match = false;
//                            break;
//                        }
//                    }
//                    if (match)
//                        return true;
//                }
//            }
//            return false;
//        }

//        public void Clear()
//        {
//            methods.Clear();
//            functions.Clear();
//            clrDelegates.Clear();
//        }
//    }

    
//}
//#endif
using ILRuntime.CLR.Method;
namespace ILRuntime.Runtime.Enviorment
{
    public partial class AppDomain
    {
        public string AppName { get; set; }
        public bool IsCleared { get; private set; }
        public void Clear()
        {
            IsCleared = true;
            DelegateManager.Clear();

            freeIntepreters.Clear();
            intepreters.Clear();
            crossAdaptors.Clear();
            valueTypeBinders.Clear();
            mapType.Clear();
            clrTypeMapping.Clear();
            mapTypeToken.Clear();
            mapMethod.Clear();
            mapString.Clear();
            redirectMap.Clear();
            fieldGetterMap.Clear();
            fieldSetterMap.Clear();
            memberwiseCloneMap.Clear();
            createDefaultInstanceMap.Clear();
            createArrayInstanceMap.Clear();
            loadedAssemblies = null;
            references.Clear();

            LoadedTypes.Clear();
            RedirectMap.Clear();
            FieldGetterMap.Clear();
            FieldSetterMap.Clear();
            MemberwiseCloneMap.Clear();
            CreateDefaultInstanceMap.Clear();
            CreateArrayInstanceMap.Clear();
            CrossBindingAdaptors.Clear();
            ValueTypeBinders.Clear();
            Intepreters.Clear();
            FreeIntepreters.Clear();
        }
    }

    public partial class DelegateManager
    {
        public bool IsRegToMethodDelegate(ILMethod method)
        {
            if (method.ParameterCount == 0) return true;
            foreach (var i in methods)
            {
                if (i.ParameterTypes.Length == method.ParameterCount)
                {
                    bool match = true;
                    for (int j = 0; j < method.ParameterCount; j++)
                    {
                        if (i.ParameterTypes[j] != method.Parameters[j].TypeForCLR)
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                        return true;
                }
            }
            return false;
        }

        public void Clear()
        {
            methods.Clear();
            functions.Clear();
            clrDelegates.Clear();
        }
    }

    
}
namespace ILRuntime.Runtime.Intepreter
{
    abstract partial class DelegateAdapter : ILTypeInstance, IDelegateAdapter
    {
        public string AppName { get; protected set; }
        public string MethodName { get; protected set; }
        public string ClassName { get; protected set; }
        protected object[] mParams;
        virtual protected void InitDelegate(int _paramcount)
        {
            if(mParams == null && _paramcount > 0)
                mParams = new object[_paramcount];
            AppName = appdomain != null ? appdomain.AppName : "";
            ClassName = method != null ? method.DeclearingType.FullName : "";
            MethodName = method != null ? method.Name : "";
        }

        virtual protected object InvokeILMethodByDomain()
        {
            if(appdomain == null || appdomain.IsCleared)
            {
                DLog.LogErrorFormat("App已被清除.请检查是否有未清除的委托注册.AppName = {0},Class = {1},Method = {2}", AppName,ClassName,MethodName);
                appdomain = null;
                return null;
            }
            try
            {
                if (method.HasThis)
                    return appdomain.Invoke(method, instance, mParams);
                else
                    return appdomain.Invoke(method, null, mParams);
            }
            catch(System.Exception _error)
            {
                DLog.LogErrorFormat("委托调用异常.AppName = {0},Class = {1},Method = {2},error = {3}", AppName, ClassName, MethodName, _error);
            }
            return null;
        }
    }
}

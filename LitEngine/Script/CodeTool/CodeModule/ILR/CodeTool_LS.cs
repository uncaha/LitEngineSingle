#if ILRuntime

using System;
using System.Collections;
using System.Collections.Generic;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.CLR.Method;
using ILRuntime.Mono.Cecil.Pdb;
using ILRuntime.Runtime.Enviorment;
using LitEngine.CodeTool;
using LitEngine.Method;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using LitEngine.UpdateSpace;

namespace LitEngine.CodeTool
{
    public class CodeTool_LS : CodeToolBase
    {
        #region 初始化

        public static System.Action<ILRuntime.Runtime.Enviorment.AppDomain> CLRBindingDelegate = null;
        public static System.Action<ILRuntime.Runtime.Enviorment.AppDomain> RegisterDelegate = null;
        public static System.Action<ILRuntime.Runtime.Enviorment.AppDomain> TypeBindingDelegate = null;

        public ILRuntime.Runtime.Enviorment.AppDomain mApp { get; private set; }

        private Dictionary<string, IBaseType> typesMap = new Dictionary<string, IBaseType>();

        public CodeTool_LS() : base()
        {
            mApp = new ILRuntime.Runtime.Enviorment.AppDomain();
            RegDelegate();
            BindingCLR();
        }

        protected void BindingCLR()
        {
            if (CLRBindingDelegate != null)
                CLRBindingDelegate(mApp);
            if (TypeBindingDelegate != null)
                TypeBindingDelegate(mApp);
        }

        #region 委托设置

        protected void RegDelegate()
        {
            RegMethodDelegate();
            RegFunctionDelegate();
            RegDelegateConvertor();
            RegBindingAdaptor();
            if (RegisterDelegate != null)
                RegisterDelegate(mApp);
        }

        protected void RegMethodDelegate()
        {
            mApp.DelegateManager.RegisterMethodDelegate<float>();
            mApp.DelegateManager.RegisterMethodDelegate<int>();
            mApp.DelegateManager.RegisterMethodDelegate<bool>();
            mApp.DelegateManager.RegisterMethodDelegate<short>();
            mApp.DelegateManager.RegisterMethodDelegate<string>();
            mApp.DelegateManager.RegisterMethodDelegate<object>();

            // mApp.DelegateManager.RegisterMethodDelegate<NetTool.ReceiveData>();
            // mApp.DelegateManager.RegisterMethodDelegate<NetTool.MSG_RECALL_DATA>();
            mApp.DelegateManager.RegisterMethodDelegate<UnityEngine.Object>();
            mApp.DelegateManager.RegisterMethodDelegate<WWW>();
            mApp.DelegateManager.RegisterMethodDelegate<Component>();
            mApp.DelegateManager.RegisterMethodDelegate<GameObject>();
            mApp.DelegateManager.RegisterMethodDelegate<Transform>();
            mApp.DelegateManager.RegisterMethodDelegate<Scene>();

            mApp.DelegateManager.RegisterMethodDelegate<Scene, Scene>();
            mApp.DelegateManager.RegisterMethodDelegate<Scene, LoadSceneMode>();
            mApp.DelegateManager.RegisterMethodDelegate<string, byte[]>();
            mApp.DelegateManager.RegisterMethodDelegate<object, object>();
            mApp.DelegateManager.RegisterMethodDelegate<int, int>();
            mApp.DelegateManager.RegisterMethodDelegate<int, string>();
            mApp.DelegateManager.RegisterMethodDelegate<string, float>();
            mApp.DelegateManager.RegisterMethodDelegate<string, string>();
            mApp.DelegateManager.RegisterMethodDelegate<string, object>();

            mApp.DelegateManager.RegisterMethodDelegate<string, string, byte[]>();
            mApp.DelegateManager.RegisterMethodDelegate<long, long, float>();
            mApp.DelegateManager.RegisterMethodDelegate<int, int, float>();
            mApp.DelegateManager.RegisterMethodDelegate<string, int, int>();
            mApp.DelegateManager.RegisterMethodDelegate<int, string, string>();
            mApp.DelegateManager.RegisterMethodDelegate<string, object, object>();
            mApp.DelegateManager.RegisterMethodDelegate<object, object, object>();
        }

        protected void RegFunctionDelegate()
        {
            mApp.DelegateManager.RegisterFunctionDelegate<float>();
            mApp.DelegateManager.RegisterFunctionDelegate<int>();
            mApp.DelegateManager.RegisterFunctionDelegate<bool>();
            mApp.DelegateManager.RegisterFunctionDelegate<short>();
            mApp.DelegateManager.RegisterFunctionDelegate<string>();
            mApp.DelegateManager.RegisterFunctionDelegate<GameObject>();
            mApp.DelegateManager.RegisterFunctionDelegate<UnityEngine.Object>();
            mApp.DelegateManager.RegisterFunctionDelegate<UnityEngine.Transform>();
            mApp.DelegateManager.RegisterFunctionDelegate<object>();

            mApp.DelegateManager.RegisterFunctionDelegate<int, bool>();
            mApp.DelegateManager.RegisterFunctionDelegate<float, float>();
            mApp.DelegateManager.RegisterFunctionDelegate<int, int>();
            mApp.DelegateManager.RegisterFunctionDelegate<bool, bool>();
            mApp.DelegateManager.RegisterFunctionDelegate<short, short>();
            mApp.DelegateManager.RegisterFunctionDelegate<string, string>();
            mApp.DelegateManager.RegisterFunctionDelegate<object, object>();

            mApp.DelegateManager.RegisterFunctionDelegate<float, float, int>();
            mApp.DelegateManager.RegisterFunctionDelegate<string, string, int>();
            mApp.DelegateManager.RegisterFunctionDelegate<int, int, int>();
            mApp.DelegateManager.RegisterFunctionDelegate<object, object, object>();
            mApp.DelegateManager
                .RegisterFunctionDelegate<ILRuntime.Runtime.Intepreter.ILTypeInstance,
                    ILRuntime.Runtime.Intepreter.ILTypeInstance, int>();
        }

        protected void RegDelegateConvertor()
        {
            mApp.DelegateManager.RegisterDelegateConvertor<Predicate<int>>((act) =>
            {
                return new System.Predicate<int>((obj) => { return ((Func<int, bool>) act)(obj); });
            });

            mApp.DelegateManager.RegisterDelegateConvertor<Predicate<ILTypeInstance>>((act) =>
            {
                return new System.Predicate<ILRuntime.Runtime.Intepreter.ILTypeInstance>((obj) =>
                {
                    return ((Func<ILTypeInstance, bool>) act)(obj);
                });
            });

            mApp.DelegateManager.RegisterDelegateConvertor<System.Comparison<ILTypeInstance>>((act) =>
            {
                return new System.Comparison<ILTypeInstance>((x, y) =>
                {
                    return ((Func<ILTypeInstance, ILTypeInstance, int>) act)(x, y);
                });
            });

            mApp.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction>((act) =>
            {
                return new UnityEngine.Events.UnityAction(() => { ((Action) act)(); });
            });

            mApp.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<int>>((act) =>
            {
                return new UnityEngine.Events.UnityAction<int>((arg0) => { ((Action<int>) act)(arg0); });
            });

            mApp.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<float>>((act) =>
            {
                return new UnityEngine.Events.UnityAction<float>((arg0) => { ((Action<float>) act)(arg0); });
            });

            mApp.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<string>>((act) =>
            {
                return new UnityEngine.Events.UnityAction<string>((arg0) => { ((Action<string>) act)(arg0); });
            });

            mApp.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<UnityEngine.Object>>((act) =>
            {
                return new UnityEngine.Events.UnityAction<UnityEngine.Object>((arg0) =>
                {
                    ((Action<UnityEngine.Object>) act)(arg0);
                });
            });

            mApp.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<object>>((act) =>
            {
                return new UnityEngine.Events.UnityAction<object>((arg0) => { ((Action<object>) act)(arg0); });
            });

            #region 多参

            mApp.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<object, object>>((act) =>
            {
                return new UnityEngine.Events.UnityAction<object, object>((arg0, arg1) =>
                {
                    ((Action<object, object>) act)(arg0, arg1);
                });
            });

            mApp.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<object, object, object>>(
                (act) =>
                {
                    return new UnityEngine.Events.UnityAction<object, object, object>((arg0, arg1, arg2) =>
                    {
                        ((Action<object, object, object>) act)(arg0, arg1, arg2);
                    });
                });

            mApp.DelegateManager
                .RegisterDelegateConvertor<UnityEngine.Events.UnityAction<object, object, object, object>>((act) =>
                {
                    return new UnityEngine.Events.UnityAction<object, object, object, object>(
                        (arg0, arg1, arg2, arg3) =>
                        {
                            ((Action<object, object, object, object>) act)(arg0, arg1, arg2, arg3);
                        });
                });

            #endregion
        }

        protected void RegBindingAdaptor()
        {
            mApp.RegisterCrossBindingAdaptor(new IEnumeratorAdaptor());
        }

        #endregion

        override protected void DisposeNoGcCode()
        {
            mApp = null;
        }

        #endregion

        #region 写入类型

        public void InitByBytes(byte[] _dll, byte[] _pdb)
        {
            if (_dll == null) throw new System.NullReferenceException("AddAssemblyType bytes 不可为null");
            System.IO.MemoryStream msDll = new System.IO.MemoryStream(_dll);
            System.IO.MemoryStream msPdb = new System.IO.MemoryStream(_pdb);
            mApp.LoadAssembly(msDll, msPdb, new PdbReaderProvider());
        }

        #endregion

        #region 类型判断

        override public IBaseType GetLType(string pName)
        {
            if (typesMap.ContainsKey(pName)) return typesMap[pName];
            if (mApp.GetType(pName) is not ILType ttype) return null;
            var ret = new ILRType(pName, ttype);
            typesMap.Add(pName, ret);
            return ret;
        }

        override public IBaseType GetObjectType(object _obj)
        {
            if (_obj == null) throw new NullReferenceException("LS GetObjectType _obj = null");
            if (_obj is ILTypeInstance)
            {
                var ttype = ((ILTypeInstance) _obj).Type;

                return GetLType(ttype.FullName);
            }

            else
                DLog.LogError("GetObjectType 只可用于ILTypeInstance");

            return null;
        }

        #endregion

        #region 方法

        override public MethodBase GetLMethod(IBaseType _type, object pTar, string _funname, int _pamcount)
        {
            IMethod ret = null;
            ret = ((ILRType) _type).Type.GetMethod(_funname, _pamcount);
            return ret != null ? new Method_LS(mApp, pTar, ret) : null;
        }

        #endregion

        #region 属性

        #region 获取

        override public object GetTargetMemberByKey(string _key, object _target)
        {
            if (_target == null) return null;
            IBaseType ttype = GetObjectType(_target);
            return GetMemberByKey(ttype, _key, _target);
        }

        override public object GetTargetMemberByIndex(int _index, object _target)
        {
            if (_target == null) return null;
            var ttype = GetObjectType(_target);
            return GetMemberByIndex(ttype, _index, _target);
        }

        override public object GetMemberByKey(IBaseType _type, string _key, object _target)
        {
            if (_type == null)
                throw new NullReferenceException("Base GetMemberByIndex _type = null");
            if (_target == null)
                throw new NullReferenceException("Base GetMemberByIndex _object = null");
            if (!(_target is ILTypeInstance))
                throw new NullReferenceException("Base GetMemberByIndex _object is not ILTypeInstance");
            ILTypeInstance tilobj = (ILTypeInstance) _target;
            IType ttype = tilobj.Type.GetField(_key, out var tindex);
            return tilobj[tindex];
        }

        override public object GetMemberByIndex(IBaseType _type, int _index, object _target)
        {
            if (_type == null)
                throw new NullReferenceException("Base GetMemberByIndex _type = null");
            if (_target == null)
                throw new NullReferenceException("Base GetMemberByIndex _object = null");
            if (!(_target is ILTypeInstance))
                throw new NullReferenceException("Base GetMemberByIndex _object is not ILTypeInstance");
            ILTypeInstance tilobj = (ILTypeInstance) _target;
            return tilobj[_index];
        }

        #endregion

        #region 设置

        override public void SetMember(IBaseType _type, int _index, object _object, object _target)
        {
            if (_type == null)
                throw new NullReferenceException("Base GetMemberByIndex _type = null");
            if (_target == null)
                throw new NullReferenceException("Base GetMemberByIndex _object = null");
            if (!(_target is ILTypeInstance))
                throw new NullReferenceException("Base GetMemberByIndex _object is not ILTypeInstance");
            ILTypeInstance tilobj = (ILTypeInstance) _target;
            tilobj[_index] = _object;
        }

        override public void SetMember(IBaseType _type, string _key, object _object, object _target)
        {
            if (_type == null)
                throw new NullReferenceException("Base GetMemberByIndex _type = null");
            if (_target == null)
                throw new NullReferenceException("Base GetMemberByIndex _object = null");
            if (!(_target is ILTypeInstance))
                throw new NullReferenceException("Base GetMemberByIndex _object is not ILTypeInstance");
            ILTypeInstance tilobj = _target as ILTypeInstance;
            int tindex = 0;
            IType ttype = tilobj.Type.GetField(_key, out tindex);
            tilobj[tindex] = _object;
        }

        #endregion

        #endregion

        #region 对象获取

        override public object GetObject(IBaseType pType, params object[] _parmas)
        {
            if (pType == null) throw new NullReferenceException("LS GetCSLEObjectParmasByType _type = null");
            if (pType is ILRType tilrtype)
            {
                bool hasConstructor = _parmas != null && _parmas.Length != 0;
                ILTypeInstance res = tilrtype.Type.Instantiate(!hasConstructor);
                if (hasConstructor)
                {
                    var ilm = tilrtype.Type.GetConstructor(_parmas.Length);
                    mApp.Invoke(ilm, res, _parmas);
                }

                return res;
            }


            return null;
        }

        #endregion

        #region 委托

        override public UpdateBase GetUpdateObjectAction(string _Function, string _classname, object _target)
        {
            if (GetLMethod(GetLType(_classname), _target, _Function, 0) is Method_LS tmethod)
                return new UpdateILObject($"{_classname}->{_Function}", tmethod, _target);
            return null;
        }

        public override Method_Action GetMethodAction(string _Function, string _classname, object _target)
        {
            if (GetLMethod(GetLType(_classname), _target, _Function, 0) is Method_LS tmethod)
            {
                return new MethodActionILR(tmethod);
            }

            return null;
        }

        public override MethodAction<T> GetMethodAction<T>(string _Function, string _classname, object _target)
        {
            if (GetLMethod(GetLType(_classname), _target, _Function, 1) is Method_LS tmethod)
            {
                return new MethodActionILR<T>(tmethod);
            }

            return null;
        }

        #endregion
    }
}


#endif
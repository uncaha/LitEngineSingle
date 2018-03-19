using System;
using System.Reflection;
using ILRuntime.CLR.TypeSystem;
namespace LitEngine
{
    using UpdateSpace;
    public class CodeTool_SYS : CodeToolBase
    {
        private SafeMap<string, Type> mAssembType = new SafeMap<string, Type>();
        private SafeMap<string, IType> mMapType = new SafeMap<string, IType>();
        private System.Reflection.Assembly mAssembly;
        private AppDomain mApp = null;
        public CodeTool_SYS(string _appname) : base(_appname)
        {
            DLog.LogWarning("反射模式对多结构的支持目前有问题.暂时不能使用.单app结构,不需要卸载gamecore的不存在问题.");
        }

        override protected void DisposeNoGcCode()
        {
            mAssembType.Clear();
            mMapType.Clear();
            mAssembly = null;
            if (mApp != null)
                AppDomain.Unload(mApp);
            mApp = null;
           // DLog.LogError( "Assembly 无法直接卸载.如有卸载需求请使用IL模式.");
        }
        #region Sys类型缓存
        public void AddAssemblyType(byte[] _dll,byte[] _pdb)
        {
            if (_dll == null) throw new System.NullReferenceException("AddAssemblyType bytes 不可为null");

            mAssembly = System.AppDomain.CurrentDomain.Load(_dll, _pdb);
            if (mAssembType == null)
                mAssembType = new SafeMap<string, Type>();
            Type[] ttypes = mAssembly.GetTypes();
            foreach (Type ttype in ttypes)
            {
                if (mAssembType.ContainsKey(ttype.FullName)) continue;
                mAssembType.Add(ttype.FullName, ttype);
            }
        }

        public Type GetAssType(string _name)
        {
            if (mAssembType.ContainsKey(_name)) return mAssembType[_name];
            return null;
        }
        public IType GetICLRTypeAss(Type _type)
        {
            if (mMapType.ContainsKey(_type.FullName)) return mMapType[_type.FullName];
            mMapType.Add(_type.FullName, new SystemType(_type));
            return mMapType[_type.FullName];
        }
        #endregion
        #region 类型判断
        override public IType GetLType(string _name)
        {
            Type ttype = GetAssType(_name);

            if (ttype != null)
                return GetICLRTypeAss(ttype);

            DLog.LogError("GetLType 没找到类型:" + _name + "|" + ttype);
            return null;
        }

        override public IType GetObjectType(object _obj)
        {
            if (_obj == null) throw new NullReferenceException("SYS GetObjectType _obj = null");
            return GetICLRTypeAss(_obj.GetType());
        }
        override public bool IsLSType(Type _type)
        {
            if (GetAssType(_type.FullName) != null)
                return true;
            return false;
        }
        override public IType GetListChildType(IType _type)
        {
            IType ret = null;
            if (_type.TypeForCLR.IsGenericType)
            {
                Type[] genericArgTypes = _type.TypeForCLR.GetGenericArguments();
                if (genericArgTypes != null && genericArgTypes.Length > 0)
                {

                    ret = GetICLRTypeAss(genericArgTypes[0]);
                    if (ret == null)
                        DLog.LogError( genericArgTypes[0].Name);
                }
                else
                {
                    DLog.LogError(genericArgTypes);
                }
            }
            return ret;
        }
        #endregion
        #region 方法
        override public object GetLMethod(IType _type, string _funname, int _pamcount)
        {
            return _type.TypeForCLR.GetMethod(_funname, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        }

        override public object CallMethodNoTry(object method, object _this, params object[] _params)
        {
            return ((MethodInfo)method).Invoke(_this, _params);
        }

        override public object CallMethod(object method, object _this, params object[] _params)
        {
            try
            {
                return ((MethodInfo)method).Invoke(_this, _params);
            }
            catch (Exception _erro)
            {
                DLog.LogError(_erro);
            }
            return null;
        }

        override public object CallMethodByName(string _name, object _this, params object[] _params)
        {
            if (_name == null || _name.Equals("")) return null;
            if (_this == null || !IsLSType(_this.GetType())) return null;
            int tpramcount = _params != null ? _params.Length : 0;

            IType ttype = GetICLRTypeAss(_this.GetType());
            object tmethod = GetLMethod(ttype, _name, tpramcount);
            return CallMethod(tmethod, _this, _params);
        }
        #endregion
        #region 属性
        #region 获取
        override public object GetTargetMemberByKey(string _key, object _target)
        {
            if (_target == null) return null;
            IType ttype = GetObjectType(_target);
            return GetMemberByKey(ttype, _key, _target);
        }

        override public object GetTargetMemberByIndex(int _index, object _target)
        {
            if (_target == null) return null;
            IType ttype = GetObjectType(_target);
            return GetMemberByIndex(ttype, _index, _target);
        }

        override public object GetMemberByKey(IType _type, string _key, object _object)
        {
            if (_type == null)
                throw new NullReferenceException("Base GetMember _type = null");
            FieldInfo pi = _type.TypeForCLR.GetField(_key, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (pi == null)
                throw new NullReferenceException("Base GetMember FieldInfo pi = null");
            return pi.GetValue(_object);
        }

        override public object GetMemberByIndex(IType _type, int _index, object _object)
        {
            if (_type == null)
                throw new NullReferenceException("Base GetMember _type = null");
            FieldInfo[] infos = _type.TypeForCLR.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (_index >= infos.Length)
                throw new System.ArgumentNullException(string.Format("Base GetMember _index 越界 index = {0} Fieldinfo数组长度 = ", _index, infos.Length));
            FieldInfo pi = infos[_index];
            if (pi == null)
                throw new NullReferenceException("Base GetMember FieldInfo pi = null");
            return pi.GetValue(_object);
        }
        #endregion
        #region 设置
        override public void SetMember(IType _type, int _index, object _object, object _target)
        {
            if (_type == null) return;
            FieldInfo[] infos = _type.TypeForCLR.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (_index >= infos.Length)
                throw new System.ArgumentNullException(string.Format("Base SetMember _index 越界 index = {0} Fieldinfo数组长度 = {1}", _index, infos.Length));
            FieldInfo pi = infos[_index];
            if (pi == null)
                throw new NullReferenceException("SYS SetMember pi = null ,_index = " + _index);
            pi.SetValue(_target, _object);
        }
        override public void SetMember(IType _type, string _key, object _object, object _target)
        {
            if (_type == null) return;
            FieldInfo pi = _type.TypeForCLR.GetField(_key, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (pi == null)
                throw new NullReferenceException("SYS SetMember pi = null ,_key = " + _key);
            pi.SetValue(_target, _object);
        }
        #endregion
        #endregion
        #region 对象获取
        override public object GetCSLEObjectParmasByType(IType _type, params object[] _parmas)
        {
            if (_type == null) throw new NullReferenceException("SYS GetCSLEObjectParmasByType _type = null");
            return Activator.CreateInstance(_type.TypeForCLR, _parmas); 
        }

        #endregion
        #region 委托
        override public UpdateBase GetUpdateObjectAction(string _Function, string _classname, object _target)
        {
            IType ttype = GetLType(_classname);
            if (ttype == null) return null;
            Action tact = GetCSLEDelegate<Action>(_Function, ttype, _target);
            if (tact == null) return null;
            return new UpdateObject(string.Format("{0}:{1}->{2}", AppName, _classname, _Function), tact);
        }
        override public K GetCSLEDelegate<K>(string _Function, IType _classtype, object _target)
        {
            if (_classtype == null || _target == null) return default(K);
            object ret = null;
            MethodInfo methodctor = (MethodInfo)GetLMethod(_classtype, _Function, 0);
            if (methodctor == null) return default(K);

            try
            {
                ret = Delegate.CreateDelegate(typeof(K), _target, methodctor);
                return (K)ret;
            }
            catch (Exception _error)
            {
                DLog.LogErrorFormat("_classtype = {0},_Function = {1},error = {2}", _classtype.Name, _Function, _error);
                return default(K);
            }

            
        }

        override public K GetCSLEDelegate<K, T1>(string _Function, IType _classtype, object _target)
        {
            return GetCSLEDelegate<K>(_Function, _classtype, _target);
        }

        override public K GetCSLEDelegate<K, T1, T2>(string _Function, IType _classtype, object _target)
        {
            return GetCSLEDelegate<K>(_Function, _classtype, _target);
        }

        override public K GetCSLEDelegate<K, T1, T2, T3>(string _Function, IType _classtype, object _target)
        {
            return GetCSLEDelegate<K>(_Function, _classtype, _target);
        }
        override public K GetCSLEDelegate<K, T1, T2, T3, T4>(string _Function, IType _classtype, object _target)
        {
            return GetCSLEDelegate<K>(_Function, _classtype, _target);
        }
        #endregion
    }
}



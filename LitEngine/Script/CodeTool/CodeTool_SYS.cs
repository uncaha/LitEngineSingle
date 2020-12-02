using System;
using System.Reflection;
using LitEngine.Method;
using LitEngine.UpdateSpace;
namespace LitEngine.CodeTool
{
    public class CodeTool_SYS : CodeToolBase
    {
        private SafeMap<string, Type> mAssembType = new SafeMap<string, Type>();
        private SafeMap<string, IBaseType> mMapType = new SafeMap<string, IBaseType>();
        private System.Reflection.Assembly mAssembly;
        private AppDomain mApp = null;
        public CodeTool_SYS() : base()
        {
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
        public void InitByBytes(byte[] _dll, byte[] _pdb)
        {
            if (_dll == null) throw new System.NullReferenceException("AddAssemblyType bytes 不可为null");
            InitByAssembly(System.AppDomain.CurrentDomain.Load(_dll, _pdb));
        }

        public void InitByAssembly(Assembly _assembly)
        {
            if (_assembly == null) return;
            mAssembly = _assembly;
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
        public IBaseType GetICLRTypeAss(Type _type)
        {
            if (mMapType.ContainsKey(_type.FullName)) return mMapType[_type.FullName];
            mMapType.Add(_type.FullName, new SystemType(_type));
            return mMapType[_type.FullName];
        }
        #endregion
        #region 类型判断
        override public IBaseType GetLType(string _name)
        {
            Type ttype = GetAssType(_name);

            if (ttype != null)
                return GetICLRTypeAss(ttype);

            DLog.LogError("GetLType 没找到类型:" + _name + "|" + ttype);
            return null;
        }

        override public IBaseType GetObjectType(object _obj)
        {
            if (_obj == null) throw new NullReferenceException("SYS GetObjectType _obj = null");
            return GetICLRTypeAss(_obj.GetType());
        }

        #endregion
        #region 方法
        override public LitEngine.Method.MethodBase GetLMethod(IBaseType _type,object pTar, string _funname, int _pamcount)
        {
            MethodInfo tmethod = _type.TypeForCLR.GetMethod(_funname, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            return tmethod != null ? new Method_CS(pTar,tmethod) : null;
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
            IBaseType ttype = GetObjectType(_target);
            return GetMemberByIndex(ttype, _index, _target);
        }

        override public object GetMemberByKey(IBaseType _type, string _key, object _object)
        {
            if (_type == null)
                throw new NullReferenceException("Base GetMember _type = null");
            FieldInfo pi = _type.TypeForCLR.GetField(_key, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (pi == null)
                throw new NullReferenceException("Base GetMember FieldInfo pi = null");
            return pi.GetValue(_object);
        }

        override public object GetMemberByIndex(IBaseType _type, int _index, object _object)
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
        override public void SetMember(IBaseType _type, int _index, object _object, object _target)
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
        override public void SetMember(IBaseType _type, string _key, object _object, object _target)
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
        override public object GetObject(IBaseType _type, params object[] _parmas)
        {
            if (_type == null) throw new NullReferenceException("SYS GetCSLEObjectParmasByType _type = null");
            return Activator.CreateInstance(_type.TypeForCLR, _parmas);
        }

        #endregion
        #region 委托
        override public UpdateBase GetUpdateObjectAction(string _Function, string _classname, object _target)
        {
            IBaseType ttype = GetLType(_classname);
            if (ttype == null) return null;
            Action tact = GetCSLEDelegate<Action>(_Function, ttype, _target);
            if (tact == null) return null;
            return new UpdateObject(string.Format("{0}->{1}", _classname, _Function), new Method_Action(tact), _target);
        }
        override public K GetCSLEDelegate<K>(string _Function, IBaseType _classtype, object _target)
        {
            if (_classtype == null || _target == null) return default(K);
            object ret = null;
            Method_CS methodctor = (Method_CS)GetLMethod(_classtype, _target, _Function, 0);
            if (methodctor == null) return default(K);

            try
            {
                ret = Delegate.CreateDelegate(typeof(K), _target, methodctor.SMethod);
                return (K)ret;
            }
            catch (Exception _error)
            {
                DLog.LogErrorFormat("_classtype = {0},_Function = {1},error = {2}", _classtype.Name, _Function, _error);
                return default(K);
            }


        }

        override public K GetCSLEDelegate<K, T1>(string _Function, IBaseType _classtype, object _target)
        {
            return GetCSLEDelegate<K>(_Function, _classtype, _target);
        }

        override public K GetCSLEDelegate<K, T1, T2>(string _Function, IBaseType _classtype, object _target)
        {
            return GetCSLEDelegate<K>(_Function, _classtype, _target);
        }

        override public K GetCSLEDelegate<K, T1, T2, T3>(string _Function, IBaseType _classtype, object _target)
        {
            return GetCSLEDelegate<K>(_Function, _classtype, _target);
        }
        override public K GetCSLEDelegate<K, T1, T2, T3, T4>(string _Function, IBaseType _classtype, object _target)
        {
            return GetCSLEDelegate<K>(_Function, _classtype, _target);
        }
        #endregion
    }

}




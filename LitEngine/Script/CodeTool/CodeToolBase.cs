using System;
using ILRuntime.CLR.TypeSystem;
namespace LitEngine
{
    using UpdateSpace;
    public abstract class CodeToolBase :IDisposable
    {
        public string AppName { get; private set; }

        public CodeToolBase(string _appname)
        {
            AppName = _appname;
        }

        bool mDisposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool _disposing)
        {
            if (mDisposed)
                return;

            if (_disposing)
                DisposeNoGcCode();

            mDisposed = true;
        }

        virtual protected void DisposeNoGcCode()
        {

        }

        ~CodeToolBase()
        {
            Dispose(false);
        }
        #region 类型判断
        virtual public IType GetLType(string _name)
        {
            return null;
        }
        virtual public IType GetObjectType(object _obj)
        {
            if (_obj == null)
            {
                throw new NullReferenceException("GetObjectType _obj = null");
            }
            return null;
        }
        virtual public bool IsLSType(Type _type)
        {
            return false;
        }
        virtual public IType GetListChildType(IType _type)
        {
            return null;
        }

        virtual public IType[] GetFieldTypes(IType _type)
        {
            if (_type == null) throw new NullReferenceException("Base GetFieldType _type =" + _type);
            if (typeof(ILType) == _type.GetType())
                return ((ILType)_type).FieldTypes;
            else if (typeof(CLRType) == _type.GetType())
                return ((CLRType)_type).OrderedFieldTypes;
            else if (typeof(SystemType) == _type.GetType())
                return ((SystemType)_type).FieldTypes;
            else
                return null;
        }
        #endregion
        #region 方法
        virtual public object GetLMethod(IType _type, string _funname, int _pamcount)
        {
            return null;
        }
        virtual public object CallMethodNoTry(object method, object _this, params object[] _params)
        {
            return null;
        }
        virtual public object CallMethod(object method, object _this, params object[] _params)
        {
            if (method == null) throw new NullReferenceException("CallMethod method = null");
            return null;
        }

        virtual public object CallMethodByName(string _name, object _this, params object[] _params)
        {
            if (_name == null || _name.Equals("")) throw new NullReferenceException("CallMethodByName _name = null");

            return null;
        }
        #endregion
        #region 属性
        virtual public object GetTargetMemberByKey(string _key, object _target)
        {
            return null;
        }

        virtual public object GetTargetMemberByIndex(int _index, object _target)
        {
            return null;
        }
        virtual public object GetMemberByKey(IType _type, string _key, object _object)
        {
            return null;
        }

        virtual public object GetMemberByIndex(IType _type, int _index, object _object)
        {
            return null;
        }

        virtual public void SetTargetMember(object _target, int _index, object _object)
        {
            IType ttype = GetObjectType(_target);
            SetMember(ttype, _index, _object, _target);
        }

        virtual public void SetTargetMember(object _target, string _key, object _object)
        {
            IType ttype = GetObjectType(_target);
            SetMember(ttype, _key, _object, _target);
        }
        virtual public void SetMember(IType _type, int _index, object _object, object _target)
        {
           
        }
        virtual public void SetMember(IType _type, string _key, object _object, object _target)
        {

        }
        #endregion
        #region 对象获取
        virtual public object GetCSLEObjectParmasByType(IType _type, params object[] _parmas)
        {
            return null;
        }
        virtual public object GetCSLEObjectParmas(string _classname, params object[] _parmas)
        {
            if (_classname == null || _classname.Length == 0) return null;
            return GetCSLEObjectParmasByType(GetLType(_classname), _parmas);
        }
        #endregion

        #region 委托
        virtual public UpdateBase GetUpdateObjectAction(string _Function, string _classname, object _target)
        {

            return null;
        }

        virtual public K GetCSLEDelegate<K>(string _Function, IType _classtype, object _target)
        {
            return default(K);
        }

        virtual public K GetCSLEDelegate<K, T1>(string _Function, IType _classtype, object _target)
        {
            return default(K);
        }

        virtual public K GetCSLEDelegate<K, T1, T2>(string _Function, IType _classtype, object _target)
        {
            return default(K);
        }

        virtual public K GetCSLEDelegate<K, T1, T2, T3>(string _Function, IType _classtype, object _target)
        {
            return default(K);
        }
        virtual public K GetCSLEDelegate<K, T1, T2, T3, T4>(string _Function, IType _classtype, object _target)
        {
            return default(K);
        }
        #endregion
    }
}



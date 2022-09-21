using System;
using LitEngine.Method;
using LitEngine.UpdateSpace;
namespace LitEngine.CodeTool
{
    public abstract class CodeToolBase : IDisposable
    {
        public CodeToolBase()
        {
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
        #region 写入类型
        #endregion
        #region 类型判断

        abstract public IBaseType GetLType(string _name);
        abstract public IBaseType GetObjectType(object _obj);
        #endregion
        #region 方法

        abstract public MethodBase GetLMethod(IBaseType _type, object pTar, string _funname, int _pamcount);
        virtual public object CallMethod(MethodBase pMethod)
        {
            try
            {
                pMethod.Call();
            }
            catch (Exception _erro)
            {
                DLog.LogError(_erro);
            }
            return null;
        }
        virtual public object CallMethod(MethodBase pMethod, params object[] _params)
        {
            try
            {
                return pMethod.Invoke(_params);
            }
            catch (Exception _erro)
            {
                DLog.LogError(_erro);
            }
            return null;
        }

        #endregion
        #region 属性
        abstract public object GetTargetMemberByKey(string _key, object _target);
        abstract public object GetTargetMemberByIndex(int _index, object _target);
        abstract public object GetMemberByKey(IBaseType _type, string _key, object _object);
        abstract public object GetMemberByIndex(IBaseType _type, int _index, object _object);
        virtual public void SetTargetMember(object _target, int _index, object _object)
        {
            IBaseType ttype = GetObjectType(_target);
            SetMember(ttype, _index, _object, _target);
        }
        virtual public void SetTargetMember(object _target, string _key, object _object)
        {
            IBaseType ttype = GetObjectType(_target);
            SetMember(ttype, _key, _object, _target);
        }
        abstract public void SetMember(IBaseType _type, int _index, object _object, object _target);
        abstract public void SetMember(IBaseType _type, string _key, object _object, object _target);
        #endregion
        #region 对象获取

        abstract public object GetObject(IBaseType _type, params object[] _parmas);

        virtual public object GetObject(string pClassName, params object[] _parmas)
        {
            if (string.IsNullOrEmpty(pClassName)) return null;
            return GetObject(GetLType(pClassName), _parmas);
        }
        #endregion

        #region 委托

        public abstract UpdateBase GetUpdateObjectAction(string _Function, string _classname, object _target);

        abstract public K GetCSLEDelegate<K>(string _Function, IBaseType _classtype, object _target);
        
        abstract public K GetCSLEDelegate<K, T1>(string _Function, IBaseType _classtype, object _target);

        abstract public K GetCSLEDelegate<K, T1, T2>(string _Function, IBaseType _classtype, object _target);

        abstract public K GetCSLEDelegate<K, T1, T2, T3>(string _Function, IBaseType _classtype, object _target);

        abstract public K GetCSLEDelegate<K, T1, T2, T3, T4>(string _Function, IBaseType _classtype, object _target);
        
        #endregion
    }
}



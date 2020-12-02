using System;
using System.Collections.Generic;
using UnityEngine;
using LitEngine.Method;
namespace LitEngine.UpdateSpace
{
    public class UpdateBase : IDisposable
    {
        public string Key { get; protected set; }
        public bool Dead { get; set; }
        public UpdateObjectVector Owner { get; set; }
        public bool IsRegToOwner { get; protected set; }
        public float deleteTime { get; protected set; }

        protected MethodBase method = null;
        protected object target = null;
        protected float mMaxTime = 0.0f;
        protected bool mIsUseTimer = false;

        protected float timer = 0;
        public float MaxTime
        {
            get
            {
                return mMaxTime;
            }
            set
            {
                mMaxTime = value;
                if (mMaxTime > 0)
                    mIsUseTimer = true;
                else
                    mIsUseTimer = false;
            }
        }
        #region 构造.释放

        public UpdateBase(string _key, MethodBase _method, object _target)
        {
            Key = _key != null ? _key : "";
            method = _method;
            target = _target;
            IsRegToOwner = false;
            Dead = false;
        }

        public UpdateBase()
        {

        }

        ~UpdateBase()
        {
            Dispose(false);
        }

        protected bool mDisposed = false;
        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        protected void Dispose(bool _disposing)
        {
            if (mDisposed)
                return;
            DisposeObj();
            mDisposed = true;
        }

        virtual protected void DisposeObj()
        {
            UnRegToOwner();
            Owner = null;
            method = null;
            Dead = true;
        }
        #endregion
        virtual public void RegToOwner()
        {
            if (IsRegToOwner) return;
            if (Owner != null)
                Owner.AddNoSetOwner(this);
            IsRegToOwner = true;
        }

        virtual public void UnRegToOwner()
        {
            if (!IsRegToOwner) return;
            if (Owner != null)
                Owner.Remove(this);
            IsRegToOwner = false;
        }

        virtual public void RunDelgete()
        {
            if (Dead) return;
            if (!IsTimeOut()) return;
            CallMethod();
        }

        virtual public void CallMethod()
        {
            if (method != null)
                method.Call();
        }

        virtual public bool IsTimeOut()
        {
            timer += Time.deltaTime;
            if (!mIsUseTimer) return true;
            if (deleteTime < mMaxTime) return false;
            deleteTime = timer;
            timer = 0;
            return true;
        }
    }

    public class UpdateObject : UpdateBase
    {
        public UpdateObject(string _key, MethodBase _method, object _target) : base(_key, _method, _target)
        {
        }
    }

    public class UpdateILObject : UpdateBase
    {
        public UpdateILObject(string _key, MethodBase _method, object _target) : base(_key, _method, _target)
        {
        }
    }

    public class UpdateNeedDisObject : UpdateBase
    {
        private Action mDisposable = null;
        public UpdateNeedDisObject(string _key, Action _delegate, Action _disposable) : base(_key, null, null)
        {
            if (_disposable == null) throw new NullReferenceException("释放委托不可为NULL. _disposable = null");
            mDisposable = _disposable;
            method = new Method_Action(_delegate);
        }

        override protected void DisposeObj()
        {
            mDisposable();
            base.DisposeObj();
        }
    }
}



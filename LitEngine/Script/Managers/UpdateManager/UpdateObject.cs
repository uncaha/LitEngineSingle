using System;
using System.Collections.Generic;
using UnityEngine;
using ILRuntime.CLR.Method;

namespace LitEngine
{
    namespace UpdateSpace
    {
        public class UpdateBase:IDisposable
        {
            public string Key { get; protected set; }
            public bool Dead { get; set; }
            public UpdateObjectVector Owner { get;set; }
            public bool IsRegToOwner { get; protected set; }
            protected Action mZeroDelegate = null;
            protected float mMaxTime = 0.1f;
            protected float mTimer = 0;
            protected bool mIsUseTimer = true;
            public float UpdateTimer
            {
                get { return mTimer; }
                set { mTimer = value; }
            }

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

            public UpdateBase(string _key, Action _delegate)
            {
                Key = _key != null ? _key:"";
                mZeroDelegate = _delegate;
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
                IsRegToOwner = false;
                Owner = null;
                mZeroDelegate = null;
            }
            #endregion


            virtual public void RegToOwner()
            {
                if (Owner == null || IsRegToOwner) return;
                Owner.Add(this);
                IsRegToOwner = true;
            }

            virtual public void UnRegToOwner()
            {
                if (Owner == null || !IsRegToOwner) return;
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
                if (mZeroDelegate != null)
                    mZeroDelegate();
            }

            virtual public bool IsTimeOut()
            {
                if (!mIsUseTimer) return true;
                if (Time.realtimeSinceStartup < mTimer) return false;
                mTimer = Time.realtimeSinceStartup + mMaxTime;
                return true;
            }
        }

        public class UpdateObject : UpdateBase
        {
            public UpdateObject(string _key, Action _delegate) : base(_key, _delegate)
            {
            }

        }

        public class UpdateILObject : UpdateBase
        {
            private IMethod mMethod;
            private ILRuntime.Runtime.Enviorment.AppDomain mApp;
            private object mTarget;
            public UpdateILObject(string _key, ILRuntime.Runtime.Enviorment.AppDomain _app, IMethod _method, object _target) : base(_key, null)
            {
                mApp = _app;
                mMethod = _method;
                mTarget = _target;
                if (mMethod.ParameterCount > 0) throw new System.IndexOutOfRangeException(_method.Name + "-Method.ParamterCount > 0");
            }

            override public void CallMethod()
            {
                if (mApp == null || mMethod == null) return;
                mApp.Invoke(mMethod, mTarget, null);
            }
        }

        public class UpdateNeedDisObject : UpdateBase
        {
            private Action mDisposable = null;
            public UpdateNeedDisObject(string _key, Action _delegate, Action _disposable) : base(_key, _delegate)
            {
                if (_disposable == null) throw new NullReferenceException("释放委托不可为NULL. _disposable = null");
                mDisposable = _disposable;
            }

            override protected void DisposeObj()
            {
                mDisposable();
                base.DisposeObj();
            }
        }

        public class UpdateGroup : UpdateBase
        {
            private List<UpdateObject> GroupList;

            private int mUpdateCount = 0;
            private int mNowUpdateCount = 0;
            private int mUpdateCountEveryFrame = 0;
            public int UpdateCountEveryFrame
            {
                get { return mUpdateCountEveryFrame; }
                set { mUpdateCountEveryFrame = value < 0 ? 0 : value; }
            }
            public int Count
            {
                get;
                private set;
            }
            public UpdateGroup()
            {
                GroupList = new List<UpdateObject>();
                Count = 0;
                UpdateCountEveryFrame = 0;
            }
            public void AddObject(UpdateObject _obj)
            {
                GroupList.Add(_obj);
                Count++;
            }
            public void Remove(UpdateObject _obj)
            {
                if (GroupList.Contains(_obj))
                {
                    GroupList.Remove(_obj);
                    Count--;
                }
            }


            override public void RunDelgete()
            {
                if (!IsTimeOut()) return;
                if (UpdateCountEveryFrame == 0)
                    UpdateAll();
                else
                    UpdatePerCount();
            }

            private void UpdateAll()
            {
                short i = 0;
                for (; i < Count; i++)
                {
                    GroupList[i].RunDelgete();
                }
            }
            private void UpdatePerCount()
            {
                int i = mUpdateCount;
                for (; i < Count; i++)
                {
                    GroupList[i].RunDelgete();
                    mUpdateCount++;
                    mNowUpdateCount++;
                    if (mNowUpdateCount == UpdateCountEveryFrame)
                    {
                        mNowUpdateCount = 0;
                        break;
                    }

                }
                if (mUpdateCount >= Count)
                {
                    mUpdateCount = 0;
                    mNowUpdateCount = 0;
                }
            }
        }
    }
   
}



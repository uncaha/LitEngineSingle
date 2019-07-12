using System;
using System.Collections.Generic;
using UnityEngine;
using LitEngine.Method;
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
            protected MethodBase method = null;
            protected object target = null;
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

            public UpdateBase(string _key, MethodBase _method,object _target)
            {
                Key = _key != null ? _key:"";
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
                    method.Invoke(target);
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
            public UpdateObject(string _key, MethodBase _method, object _target):base(_key, _method, _target)
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
            public UpdateNeedDisObject(string _key, Action _delegate, Action _disposable) : base(_key, null,null)
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



//#if USEILRUNTIME
//using UnityEngine;
//using System.Collections.Generic;
//using ILRuntime.Other;
//using System;
//using System.Collections;
//using ILRuntime.Runtime.Enviorment;
//using ILRuntime.Runtime.Intepreter;
//using ILRuntime.CLR.Method;
//namespace LitEngine
//{
//    //ILRuntime Demo
//    public class IEnumeratorAdaptor : CrossBindingAdaptor
//    {
//        public override Type BaseCLRType
//        {
//            get
//            {
//                return null;
//            }
//        }

//        public override Type[] BaseCLRTypes
//        {
//            get
//            {
//                return new Type[] { typeof(IEnumerator<object>), typeof(IEnumerator), typeof(IDisposable) };
//            }
//        }

//        public override Type AdaptorType
//        {
//            get
//            {
//                return typeof(Adaptor);
//            }
//        }

//        public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
//        {
//            return new Adaptor(appdomain, instance);
//        }
//        internal class Adaptor : IEnumerator<System.Object>, IEnumerator, IDisposable, CrossBindingAdaptorType
//        {
//            ILTypeInstance instance;
//            ILRuntime.Runtime.Enviorment.AppDomain appdomain;

//            public Adaptor()
//            {

//            }

//            public Adaptor(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
//            {
//                this.appdomain = appdomain;
//                this.instance = instance;
//            }

//            public ILTypeInstance ILInstance { get { return instance; } }

//            IMethod mCurrentMethod;
//            bool mCurrentMethodGot;
//            public object Current
//            {
//                get
//                {
//                    if (!mCurrentMethodGot)
//                    {
//                        mCurrentMethod = instance.Type.GetMethod("get_Current", 0);
//                        if (mCurrentMethod == null)
//                        {
//                            mCurrentMethod = instance.Type.GetMethod("System.Collections.IEnumerator.get_Current", 0);
//                        }
//                        mCurrentMethodGot = true;
//                    }

//                    if (mCurrentMethod != null)
//                    {
//                        var res = appdomain.Invoke(mCurrentMethod, instance, null);
//                        return res;
//                    }
//                    else
//                    {
//                        return null;
//                    }
//                }
//            }

//            IMethod mDisposeMethod;
//            bool mDisposeMethodGot;
//            public void Dispose()
//            {
//                if (!mDisposeMethodGot)
//                {
//                    mDisposeMethod = instance.Type.GetMethod("Dispose", 0);
//                    if (mDisposeMethod == null)
//                    {
//                        mDisposeMethod = instance.Type.GetMethod("System.IDisposable.Dispose", 0);
//                    }
//                    mDisposeMethodGot = true;
//                }

//                if (mDisposeMethod != null)
//                {
//                    appdomain.Invoke(mDisposeMethod, instance, null);
//                }
//            }

//            IMethod mMoveNextMethod;
//            bool mMoveNextMethodGot;
//            public bool MoveNext()
//            {
//                if (!mMoveNextMethodGot)
//                {
//                    mMoveNextMethod = instance.Type.GetMethod("MoveNext", 0);
//                    mMoveNextMethodGot = true;
//                }

//                if (mMoveNextMethod != null)
//                {
//                    return (bool)appdomain.Invoke(mMoveNextMethod, instance, null);
//                }
//                else
//                {
//                    return false;
//                }
//            }

//            IMethod mResetMethod;
//            bool mResetMethodGot;
//            public void Reset()
//            {
//                if (!mResetMethodGot)
//                {
//                    mResetMethod = instance.Type.GetMethod("Reset", 0);
//                    mResetMethodGot = true;
//                }

//                if (mResetMethod != null)
//                {
//                    appdomain.Invoke(mResetMethod, instance, null);
//                }
//            }

//            public override string ToString()
//            {
//                IMethod m = appdomain.ObjectType.GetMethod("ToString", 0);
//                m = instance.Type.GetVirtualMethod(m);
//                if (m == null || m is ILMethod)
//                {
//                    return instance.ToString();
//                }
//                else
//                    return instance.Type.FullName;
//            }
//        }
//    }
//}
//#endif
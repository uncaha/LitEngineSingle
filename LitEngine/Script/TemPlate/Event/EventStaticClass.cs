using UnityEngine;
using System;
namespace LitEngine.TemPlate.Event
{
    public static class EventStaticClass
    {
        public static void RegEvent(this object pTarget, Type pType, Action<object> pRreceive)
        {
            EventDispatch.Reg(pTarget, pType, pRreceive);
        }
        public static void RegEvent<T>(this object pTarget, Action<object> pRreceive)
        {
            EventDispatch.Reg(pTarget, typeof(T), pRreceive);
        }

        public static void UnRegEvent(this object pTarget, Type pType)
        {
            EventDispatch.UnReg(pTarget, pType);
        }
        public static void UnRegEvent<T>(this object pTarget)
        {
            EventDispatch.UnReg(pTarget, typeof(T));
        }

        public static void UnRegAllEvent(this object pTarget)
        {
            EventDispatch.UnRegAllEvent(pTarget);
        }

        static public void SendEvent(this object target, object pObject)
        {
            if (pObject == null) return;
            Type tkey = pObject.GetType();
            EventDispatch.Send(tkey, pObject);
        }
        static public void Send<T>(this object target, T pObject = null) where T : class
        {
            EventDispatch.Send(typeof(T), pObject);
        }
    }
}

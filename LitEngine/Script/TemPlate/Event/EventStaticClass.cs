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

        public static void UnRegEvent(this object pTarget, Type pType)
        {
            EventDispatch.UnReg(pTarget, pType);
        }

        public static void UnRegAllEvent(this object pTarget)
        {
            EventDispatch.UnRegAllEvent(pTarget);
        }

        static public void SendEvent(this object target,object pObject)
        {
            EventDispatch.Send(pObject);
        }
    }
}

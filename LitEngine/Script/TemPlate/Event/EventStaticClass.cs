using UnityEngine;
using System;
namespace LitEngine.TemPlate.Event
{
    public static class EventStaticClass
    {
        public static void RegEvent(this object pTarget, Enum _def, Action<object> pRreceive)
        {
            EventDispatch.Reg(pTarget,_def, pRreceive);
        }

        public static void UnRegEvent(this object pTarget, Enum _def)
        {
            EventDispatch.UnReg(pTarget,_def);
        }

        public static void UnRegAllEvent(this object pTarget)
        {
            EventDispatch.UnRegByTarget(pTarget);
        }

        static public void SendEvent(this object target, Enum _def, object _sendObject = null)
        {
            EventDispatch.Send(_def, _sendObject);
        }
    }
}

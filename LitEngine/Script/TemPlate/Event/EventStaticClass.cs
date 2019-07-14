using UnityEngine;
using System;
namespace LitEngine.TemPlate.Event
{
    public static class EventStaticClass
    {
        public static void RegEvent(this object target, Enum _def, Action<object> _receiver)
        {
            EventDispatch.Reg(_def, _receiver);
        }

        public static void UnRegEvent(this object target , Enum _def, Action<object> _receiver)
        {
            EventDispatch.UnReg(_def, _receiver);
        }

        static public void SendEvent(this object target, Enum _def, object _sendObject = null)
        {
            EventDispatch.Send(_def, _sendObject);
        }
    }
}

using UnityEngine;
using System.Collections.Generic;
namespace LitEngine.Event
{
    internal class EventObject
    {
        public int HashCode { get; private set; }
        public object Target { get; private set; }
        public System.Action<object> EventDelgate { get; private set; }
        public EventObject(object pTar, System.Action<object> pDel)
        {
            Target = pTar;
            HashCode = Target.GetHashCode();
            EventDelgate = pDel;
        }

        public bool IsLife
        {
            get
            {
                return Target != null;
            }
        }

        public void Call(object pObject)
        {
            try
            {
                EventDelgate(pObject);
            }
            catch (System.Exception _e)
            {
                DLog.LogError(_e.ToString());
            }
        }

    }
}

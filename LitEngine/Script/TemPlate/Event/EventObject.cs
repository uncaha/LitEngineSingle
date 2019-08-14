using UnityEngine;
using System.Collections.Generic;
namespace LitEngine.TemPlate.Event
{
    public class EventObject
    {
        public object Target { get; private set; }
        public System.Action<object> EventDelgate { get; private set;}
        public EventObject(object pTar, System.Action<object> pDel)
        {
            Target = pTar;
            EventDelgate = pDel;
        }

        public void Call(object pObject)
        {
#if LITDEBUG
            try
            {
                EventDelgate(pObject);
            }
            catch (System.Exception _e)
            {
                DLog.LogError(_e.ToString());
            }
#else
            _action(_obj);
#endif
        }

    }
}

using UnityEngine;
using System.Collections.Generic;
namespace LitEngine.TemPlate.Event
{
    public class EventObject
    {
        public object Target { get; private set; }
        public string methodName;
        public Dictionary<string,System.Action<object>> Delgates{ get; private set; }
        public EventObject(object _tar)
        {
            Target = _tar;
        }

        public System.Action<object> GetDelgate()
        {
            return null;
        }

    }
}

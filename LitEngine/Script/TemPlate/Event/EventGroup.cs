
using System.Collections.Generic;
namespace LitEngine.TemPlate.Event
{
    internal class EventGroup
    {
        public System.Type Key { get; private set; }
        public Dictionary<object, EventObject> Delgates { get; private set; }
        public EventGroup(System.Type _key)
        {
            Key = _key;
            Delgates = new Dictionary<object, EventObject>();
        }

        public void Add(object target, System.Action<object> _delgate)
        {
            if(!Delgates.ContainsKey(_delgate))
            {
                EventObject titem = new EventObject(target, _delgate);
                Delgates.Add(target, titem);
            }
        }

        public void Remove(object target)
        {
            if (Delgates.ContainsKey(target))
                Delgates.Remove(target);
        }

        public void Call(object pObj)
        {
            List<object> tkeyslist = new List<object>(Delgates.Keys);
            for (int i = tkeyslist.Count - 1; i >= 0; i--)
            {
                object tkey = tkeyslist[i];
                var tact = Delgates[tkey];

                if (!tact.IsLife)
                {
                    Delgates.Remove(tkey);
                    continue;
                }
                tact.Call(pObj);
            }
        }
    }

}

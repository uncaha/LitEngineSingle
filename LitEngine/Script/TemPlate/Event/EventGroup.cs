
using System.Collections.Generic;
namespace LitEngine.Event
{
    internal class EventGroup
    {
        public System.Type Key { get; private set; }
        public LinkedList<EventObject> Delgates { get; private set; }

        Dictionary<int, LinkedListNode<EventObject>> map = new Dictionary<int, LinkedListNode<EventObject>>();
        public EventGroup(System.Type _key)
        {
            Key = _key;
            Delgates = new LinkedList<EventObject>();
        }

        public void Add(object target, System.Action<object> _delgate)
        {
            var thash = target.GetHashCode();
            if (!map.ContainsKey(thash))
            {
                var tobj = Delgates.AddLast(new EventObject(target, _delgate));
                map.Add(thash, tobj);
            }

        }

        public void Remove(object target)
        {
            Remove(target.GetHashCode());
        }

        public void Remove(int hash)
        {
            if (map.ContainsKey(hash))
            {
                var tobj = map[hash];
                map.Remove(hash);

                Delgates.Remove(tobj);
            }
        }

        public void Call(object pObj)
        {
            var tcur = Delgates.First;
            while (tcur != null)
            {
                var tact = tcur.Value;

                tcur = tcur.Next;

                if (!tact.IsLife)
                {
                    Remove(tact.HashCode);
                    continue;
                }
                tact.Call(pObj);
            }
        }
    }

}

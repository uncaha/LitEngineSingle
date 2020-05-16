using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LitEngine.TemPlate.Event
{
    internal class ObjectGroupList
    {
        private Dictionary<System.Type, EventGroup> groups = new Dictionary<System.Type, EventGroup>();
        private object target;
        public int Count
        {
            get
            {
                return groups.Count;
            }
        }
        public ObjectGroupList(object pTarget)
        {
            target = pTarget;
        }

        public void Add(EventGroup pGroup)
        {
            if(!groups.ContainsKey(pGroup.Key))
                groups.Add(pGroup.Key, pGroup);
        }

        public int Remove(System.Type pKey)
        {
            if (groups.ContainsKey(pKey))
            {
                groups[pKey].Remove(target);
                groups.Remove(pKey);
            }
               
            return groups.Count;
        }
        public void Clear()
        {
            foreach (var item in groups)
            {
                item.Value.Remove(target);
            }
            groups.Clear();
        }
    }
}

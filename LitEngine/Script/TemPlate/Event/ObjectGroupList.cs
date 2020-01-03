using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LitEngine.TemPlate.Event
{
    internal class ObjectGroupList
    {
        private Dictionary<System.Enum, EventGroup> groups = new Dictionary<System.Enum, EventGroup>();
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

        public int Remove(System.Enum pEnum)
        {
            if (groups.ContainsKey(pEnum))
            {
                groups[pEnum].Remove(target);
                groups.Remove(pEnum);
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

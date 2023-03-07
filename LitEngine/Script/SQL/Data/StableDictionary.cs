using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Habby.SQL
{
    public class StableDictionary<TKey,TValue>
    {
        public int Count => map.Count;
        
        private Dictionary<TKey, LinkedListNode<TValue>> map = new Dictionary<TKey, LinkedListNode<TValue>>();
        private LinkedList<TValue> linkedList = new LinkedList<TValue>();

        public StableDictionary() 
        {

        }
        
        public TValue this[TKey pKey]
        {
            get
            {
                var tretnode = map[pKey];
                return tretnode != null ? tretnode.Value : default(TValue);
            }
            set
            {
                if (map.ContainsKey(pKey))
                {
                    map[pKey].Value = value;
                }
                else
                {
                    Add(pKey,value);
                }
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return map.Keys;
            }
        }

        public List<TValue> Values
        {
            get
            {
                return linkedList.ToList();
            }
        }

        public void Add(TKey pKey,TValue pValue)
        {
            if (map.ContainsKey(pKey))
            {
                Debug.LogError($"Add faild. {GetType().Name} -> The same key = {pKey}");
                return;
            }
            
            var tnode = linkedList.AddLast(pValue);
            map.Add(pKey,tnode);
        }

        public void Clear()
        {
            map.Clear();
            linkedList.Clear();
        }

        public TValue Remove(TKey pKey)
        {
            if (!map.ContainsKey(pKey)) return default(TValue);
            var rm = map[pKey];
            map.Remove(pKey);
            
            linkedList.Remove(rm);
            return rm.Value;
            
        }

        public bool ContainsKey(TKey pKey)
        {
            return map.ContainsKey(pKey);
        }
    }
}
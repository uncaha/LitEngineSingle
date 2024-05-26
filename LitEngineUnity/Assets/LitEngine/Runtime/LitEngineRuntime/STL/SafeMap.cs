using System.Threading;
using System.Collections.Generic;
using UnityEngine;
namespace LitEngine
{
    public class SafeMap<K, T>
    {
        private Dictionary<K, T> mDMap = new Dictionary<K, T>();

        public T this[K key]
        {
            get
            {
                lock(mDMap)
                    return mDMap[key];
            }
            set
            {
                lock (mDMap)
                    mDMap[key] = value;
            }
        }

        public Dictionary<K, T>.ValueCollection values
        {
            get
            {
                lock (mDMap)
                    return mDMap.Values;
            }
        }

        public Dictionary<K, T>.KeyCollection Keys
        {
            get
            {
                lock (mDMap)
                    return mDMap.Keys;
            }
        }

        public bool ContainsKey(K _key)
        {
            lock (mDMap)
                return mDMap.ContainsKey(_key);
        }

        public bool ContainsValue(T _value)
        {
            lock (mDMap)
                return mDMap.ContainsValue(_value);
        }

        public void Add(K _key, T _value)
        {
            lock (mDMap)
            {
                if (!mDMap.ContainsKey(_key))
                    mDMap.Add(_key, _value);
                else
                    DLog.LogError(string.Format("SafeMap关键字={0}:不能重复添加.", _key));
            }
        }

        public void Remove(K _key)
        {
            lock (mDMap)
            {
                if (mDMap.ContainsKey(_key))
                    mDMap.Remove(_key);
                else
                    DLog.LogError(string.Format("SafeMap关键字={0}:找不到要删除的对象.", _key));
            }
        }

        public void Clear()
        {
            lock (mDMap)
                mDMap.Clear();
        }

        public int Count
        {
            get {
                lock (mDMap)
                    return mDMap.Count;
            }
            
        }
    }
}


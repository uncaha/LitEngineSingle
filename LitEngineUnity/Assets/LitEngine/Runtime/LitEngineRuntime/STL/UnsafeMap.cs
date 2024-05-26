
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
namespace LitEngine
{
    public class UnsafeMap<K, T>
    {
        private Dictionary<K, T> mDMap = new Dictionary<K, T>();
        public T this[K key]
        {
            get
            {
                return mDMap[key];
            }
            set
            {
                mDMap[key] = value;
            }
        }

        public Dictionary<K, T>.ValueCollection values
        {
            get
            {
                return mDMap.Values;
            }
        }

        public Dictionary<K, T>.KeyCollection Keys
        {
            get
            {
                return mDMap.Keys;
            }
        }

        public bool ContainsKey(K _key)
        {
            return mDMap.ContainsKey(_key);
        }

        public bool ContainsValue(T _value)
        {
            return mDMap.ContainsValue(_value);
        }

        public void Add(K _key, T _value)
        {
            if (!mDMap.ContainsKey(_key))
                mDMap.Add(_key, _value);
            else
                DLog.LogError( string.Format("UnsafeMap关键字={0}:不能重复添加.", _key));
        }

        public void Remove(K _key)
        {
            if (mDMap.ContainsKey(_key))
                mDMap.Remove(_key);
            else
                DLog.LogError( string.Format("UnsafeMap关键字={0}:找不到要删除的对象.", _key));
        }

        public void Clear()
        {
            mDMap.Clear();
        }

        public int Count
        {
            get{
                return mDMap.Count;
            } 
        }
    }
}


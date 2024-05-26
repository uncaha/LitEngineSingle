using System.Collections.Generic;
namespace LitEngine
{
    public class SafeList<T>
    {
        List<T> mList = new List<T>();

        public T this[int _index]
        {
            get
            {
                lock (mList)
                  return mList[_index];
            }
            set
            {
                lock (mList)
                    mList[_index] = value;
            }
        }

        public int IndexOf(T _item)
        {
            lock (mList)
                return mList.IndexOf(_item);
        }
        public void Insert(int _index, T _item)
        {
            lock (mList)
                mList.Insert(_index, _item);
        }
        public void RemoveAt(int _index)
        {
            lock (mList)
                mList.RemoveAt(_index);
        }

        public int Count
        {
            get
            {
                lock (mList)
                    return mList.Count;
            }
        }
        public bool IsReadOnly { get; }

        public void Add(T _item)
        {
            lock (mList)
                mList.Add(_item);
        }
        public void Clear()
        {
            lock (mList)
                mList.Clear();
        }
        public bool Contains(T _item)
        {
            lock (mList)
               return mList.Contains(_item);
        }
        public void CopyTo(T[] _array, int _arrayIndex)
        {
            lock (mList)
                mList.CopyTo(_array, _arrayIndex);
        }
        public bool Remove(T _item)
        {
            lock (mList)
                return mList.Remove(_item);
        }
    }
}

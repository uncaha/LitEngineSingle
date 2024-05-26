using System.Threading;
using System.Collections.Generic;
namespace LitEngine
{
    public class SafeQueue<T>
    {
        private Queue<T> mQueue = new Queue<T>();
        public void Enqueue(T _obj)
        {
            lock (mQueue)
            {
                if (_obj == null)
                {
                    DLog.LogError("Enqueue 不能添加空的对象到队列!");
                    return;
                }
                mQueue.Enqueue(_obj);
            }
        }
        public T Dequeue()
        {
            lock (mQueue)
            {
                if (mQueue.Count == 0)
                {
                    DLog.LogError("Dequeue 已经是空队列了!");
                    return default(T);
                }
                return mQueue.Dequeue();
            }
            
        }
        public T Peek()
        {
            lock (mQueue)
            {
                if (mQueue.Count == 0)
                {
                    DLog.LogError("Peek 已经是空队列了!");
                    return default(T);
                }
                return mQueue.Peek();
            }
                
        }
        public int Count
        {
            get
            {
                lock (mQueue)
                    return mQueue.Count;
            }
        }
        public void Clear()
        {
            lock (mQueue)
                mQueue.Clear();
        }
    }
}

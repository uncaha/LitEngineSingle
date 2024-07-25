﻿using System;
using System.Collections;
using System.Collections.Generic;
namespace LitEngine.Net
{
    public class SafeSwitchQueue<T> where T : class
    {
        public static void Swap<QT>(ref QT t1, ref QT t2)
        {

            QT temp = t1;
            t1 = t2;
            t2 = temp;
        }

        private Queue<T> mPopQueue;
        private Queue<T> mPushQueue;

        public int PopCount
        {
            get
            {
                return mPopQueue.Count;
            }
        }

        public int PushCount
        {
            get
            {
                return mPushQueue.Count;
            }
        }

        public SafeSwitchQueue()
        {
            mPopQueue = new Queue<T>(16);
            mPushQueue = new Queue<T>(16);
        }

        public SafeSwitchQueue(int capcity)
        {
            mPopQueue = new Queue<T>(capcity);
            mPushQueue = new Queue<T>(capcity);
        }

        public void Enqueue(T obj)
        {
            lock (mPushQueue)
            {
                mPushQueue.Enqueue(obj);
            }
        }

        public T Dequeue()
        {
            return mPopQueue.Dequeue();
        }

        public bool Empty()
        {
            return 0 == mPopQueue.Count;
        }

        public void Switch()
        {
            lock (mPushQueue)
            {
                Swap(ref mPopQueue, ref mPushQueue);
            }
        }

        public void Clear()
        {
            lock (mPushQueue)
            {
                mPopQueue.Clear();
                mPushQueue.Clear();
            }
        }

        public List<T> DequeueAll()
        {

            lock (mPushQueue)
            {
                List<T> ret = new List<T>(mPushQueue.Count + mPopQueue.Count);
                for (int i = 0, max = mPushQueue.Count; i < max; i++)
                {
                    T trecdata = mPushQueue.Dequeue();
                    ret.Add(trecdata);
                }

                for (int i = 0, max = mPopQueue.Count; i < max; i++)
                {
                    T trecdata = mPopQueue.Dequeue();
                    ret.Add(trecdata);
                }

                return ret;
            }
        }
    }
}

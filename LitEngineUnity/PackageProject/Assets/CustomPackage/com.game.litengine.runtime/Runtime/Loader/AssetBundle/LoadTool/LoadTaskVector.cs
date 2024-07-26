using System.Collections.Generic;
namespace LitEngine.LoadAsset
{
    public class LoadTaskVector
    {
        private List<LoadTask> mList = new List<LoadTask>(10);

        public int Count
        {
            get
            {
                return mList.Count;
            }
        }

        public LoadTask this[int _index]
        {
            get
            {
                return mList[_index];
            }
            set
            {
                mList[_index] = value;
            }
        }

        public void Add(LoadTask _task)
        {
            if (!mList.Contains(_task))
            {
                _task.Parent = this;
                mList.Add(_task);
            }
            else
                DLog.LogError("LoadTaskVector 重复添加. _task = " + _task.TaskKey);
        }

        public void Clear()
        {
            mList.Clear();
        }

        public void Remove(LoadTask _task)
        {
            mList.Remove(_task);
        }
    }

}

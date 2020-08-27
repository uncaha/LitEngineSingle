using System.Collections.Generic;
namespace LitEngine.LoadAsset
{
    public class WaitingList
    {
        private List<BaseBundle> mList = new List<BaseBundle>();

        public int Count
        {
            get
            {
                return mList.Count;
            }
        }

        public BaseBundle this[int _index]
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

        public void Add(BaseBundle _bundle)
        {
            if (!mList.Contains(_bundle))
            {
                mList.Add(_bundle);
            }
            else
                DLog.LogError("LoadTaskVector 重复添加. _bundle.GetHashCode() = " + _bundle.GetHashCode() + " AssetName = " + _bundle.AssetName);
        }

        public void Clear()
        {
            mList.Clear();
        }

        public void Remove(BaseBundle _bundle)
        {
            mList.Remove(_bundle);
        }

        public void RemoveAt(int _index)
        {
            mList.RemoveAt(_index);
        }
    }
}

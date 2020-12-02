using System.Collections.Generic;
namespace LitEngine.UpdateSpace
{
    public enum UpdateType
    {
        Update = 1,
        FixedUpdate,
        LateUpdate,
        OnGUI,
    }
    public class UpdateObjectVector
    {

        private UpdateType mUpdateType = UpdateType.Update;
        private List<UpdateBase> mList = new List<UpdateBase>();

        public int Count { get { return mList.Count; } }

        public UpdateObjectVector(UpdateType _type)
        {
            mUpdateType = _type;
        }

        public void Add(UpdateBase _obj)
        {
            if (_obj == null || mList.Contains(_obj))
                return;
            _obj.Owner = this;
            _obj.RegToOwner();
        }

        public void Insert(int pIndex, UpdateBase pSor)
        {
            if (pSor == null || mList.Contains(pSor))
                return;
            pSor.Owner = this;
            mList.Insert(pIndex, pSor);
        }

        public void AddNoSetOwner(UpdateBase _obj)
        {
            if (_obj == null || mList.Contains(_obj))
                return;
            mList.Add(_obj);
        }

        public void Remove(UpdateBase _obj)
        {
            if (_obj == null || !mList.Contains(_obj))
                return;
            mList.Remove(_obj);
        }

        public void Clear()
        {
            for (int i = mList.Count - 1; i >= 0; i--)
            {
                UpdateBase tobj = mList[i];
                mList.RemoveAt(i);
                tobj.Dispose();
            }
            mList.Clear();
        }

        private void RunUpdate(UpdateBase _runobj)
        {
#if LITDEBUG
            try
            {
#endif
                _runobj.RunDelgete();
#if LITDEBUG
            }
            catch (System.Exception _erro)
            {
                DLog.LogError(string.Format("[{0}] [{1}]{2}", mUpdateType.ToString(), _runobj.Key, _erro.ToString()));
                _runobj.UnRegToOwner();
            }
#endif

        }

        public void Update()
        {
            if (mList.Count == 0) return;
            for (int i = mList.Count - 1; i >= 0; i--)
            {
                if (!mList[i].Dead)
                    RunUpdate(mList[i]);
                else
                    mList[i].Dispose();
            }
        }
    }

}

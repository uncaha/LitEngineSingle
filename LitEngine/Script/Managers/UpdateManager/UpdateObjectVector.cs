using System.Collections.Generic;
namespace LitEngine
{
    namespace UpdateSpace
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

            public void ClearByKey(string _key)
            {
                for (int i = mList.Count - 1; i >= 0; i--)
                {
                    UpdateBase tobj = mList[i];
                    if (!tobj.Key.Equals(_key)) continue;
                    tobj.Dead = true;
                }
            }

            private void RunUpdate(UpdateBase _runobj)
            {
#if LITDEBUG
                try
                {
                    _runobj.RunDelgete();
                }
                catch (System.Exception _erro)
                {
                    DLog.LogError( string.Format("[{0}] [{1}]{2}", mUpdateType.ToString(), _runobj.Key, _erro.ToString()));
                    _runobj.UnRegToOwner();
                }
#else
                _runobj.RunDelgete();
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
    
}

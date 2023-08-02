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
    public sealed class UpdateObjectVector
    {
        private UpdateType mUpdateType = UpdateType.Update;
        private LinkedList<UpdateBase> updaterList = new LinkedList<UpdateBase>();

        public int Count { get { return updaterList.Count; } }

        public UpdateObjectVector(UpdateType _type)
        {
            mUpdateType = _type;
        }

        internal void Add(UpdateBase pUpdater)
        {
            if (pUpdater == null) return;
            if (pUpdater.IsRegToOwner)
            {
                pUpdater.UnRegToOwner();
            }

            pUpdater.Owner = this;
            pUpdater.RegToOwner();
        }

        internal void AddNoSetOwner(UpdateBase pUpdater)
        {
            if (pUpdater == null) return;
            if (pUpdater.IsRegToOwner)
            {
                pUpdater.UnRegToOwner();
            }
            pUpdater.node = updaterList.AddLast(pUpdater);
        }

        internal void Remove(UpdateBase pUpdater)
        {
            if (pUpdater == null) return;
            if (pUpdater.node != null)
            {
                updaterList.Remove(pUpdater.node);
                pUpdater.node = null;
            }
            
        }

        internal void Clear()
        {
            foreach (var item in updaterList)
            {
                item.Dispose();
            }
            updaterList.Clear();
        }

        private void RunUpdate(UpdateBase _runobj)
        {
            try
            {
                _runobj.RunDelgete();
            }
            catch (System.Exception _erro)
            {
                DLog.LogError(string.Format("[{0}] [{1}]{2}", mUpdateType.ToString(), _runobj.Key, _erro.ToString()));
                _runobj.UnRegToOwner();
            }
        }

        internal void Update()
        {
            if (updaterList.Count == 0) return;

            var itor = updaterList.First;

            while (itor != null)
            {
                var item = itor.Value;
                RunUpdate(item);
                itor = itor.Next;
            }
        }
    }
}

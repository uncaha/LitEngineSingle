using System;
using System.Collections.Generic;
using LitEngine.UpdateSpace;
namespace LitEngine.DownLoad
{

    public class DownLoadGroup : System.Collections.IEnumerator, IDisposable
    {
        public long ContentLength { get; private set; }
        public long DownLoadedLength { get; private set; }
        public float Progress
        {
            get
            {
                return ContentLength > 0 ? (float)DownLoadedLength / ContentLength : 0;
            }
        }

        public bool IsDone { get; private set; }
        public bool IsStart { get; private set; }
        public string Key { get; private set; }

        private bool autoDispose = false;
        private Action<string[], string> FinishedDelegate;
        private Action<long, long, float> ProgressDelegate;
        private List<DownLoadObject> groupList = new List<DownLoadObject>();
        private UpdateNeedDisObject mUpdateObject;
        public DownLoadGroup(string newKey ,bool newAutoDispose = true)
        {
            Key = newKey;
            autoDispose = newAutoDispose;
            mUpdateObject = new UpdateNeedDisObject(Key, Update, Dispose);
        }
        ~DownLoadGroup()
        {
            Dispose(false);
        }

        #region dispose
        protected bool mDisposed = false;
        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        protected void Dispose(bool _disposing)
        {
            if (mDisposed)
                return;
            mDisposed = true;
            DisposeGC();
        }

        protected void DisposeGC()
        {
            for (int i = 0; i < groupList.Count; i++)
            {
                groupList[i].Dispose();
            }
            groupList.Clear();
            mUpdateObject.UnRegToOwner();
            mUpdateObject = null;
            FinishedDelegate = null;
            ProgressDelegate = null;
        }
        #endregion

        #region ito
        public object Current { get; }

        public bool MoveNext()
        {
            return !IsDone;
        }
        public void Reset()
        {

        }
        #endregion

        public void AddByUrl(string _sourceurl, string _destination, bool _clear)
        {
            Add(new DownLoadObject(_sourceurl, _destination, _clear));
        }

        public void Add(DownLoadObject newObject)
        {
            if(IsStart)
            {
                DLog.LogError("已经开始的任务不可插入新内容." );
                return;
            }
            if (groupList.Contains(newObject))
            {
                DLog.LogError("重复下载,url = "+newObject.SourceURL);
                return;
            }
            groupList.Add(newObject);
        }

        public void StartAsync()
        {
            if (IsStart) return;
            IsStart = true;

            for (int i = 0; i < groupList.Count; i++)
            {
                groupList[i].StartDownLoadAsync();
            }

            PublicUpdateManager.AddUpdate(mUpdateObject);
        }

        bool isAllDone = false;
        protected void Update()
        {
            if (IsDone) return;

            ContentLength = 0;
            DownLoadedLength = 0;
            isAllDone = true;
            for (int i = 0; i < groupList.Count; i++)
            {
                ContentLength += groupList[i].ContentLength;
                DownLoadedLength += groupList[i].DownLoadedLength;
                if (!groupList[i].IsDone)
                    isAllDone = false;
            }
            IsDone = isAllDone;

            if (ProgressDelegate != null)
                ProgressDelegate(DownLoadedLength,ContentLength, Progress);

            if (IsDone)
            {
                string[] tcmpfile = new string[groupList.Count];
                string terror = "";
                for (int i = 0; i < groupList.Count; i++)
                {
                    tcmpfile[i] = groupList[i].CompleteFile;
                    terror += groupList[i].Error == null ? "" : groupList[i].Error + "|";
                }

                Action<string[], string> tfinished = FinishedDelegate;

                if (autoDispose)
                    Dispose();
                else
                    mUpdateObject.UnRegToOwner();

                try
                {
                    if (tfinished != null)
                        tfinished(tcmpfile, terror);
                }
                catch (System.Exception _error)
                {
                    DLog.LogError(_error);
                }
            }
        }
    }
}

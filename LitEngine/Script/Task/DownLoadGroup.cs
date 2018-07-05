using System;
using System.Collections.Generic;
using LitEngine.UpdateSpace;
namespace LitEngine.DownLoad
{
    public enum DownloadState
    {
        normal = 1,
        downloading,
        finished,
    }

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

        public bool IsDone { get { return State == DownloadState.finished; } }
        public DownloadState State { get; private set; }
        public string Key { get; private set; }
        public string Error { get; private set; }

        private bool autoDispose = false;
        private Action<string[], string> FinishedDelegate;
        private Action<long, long, float> ProgressDelegate;
        private List<DownLoadObject> groupList = new List<DownLoadObject>();
        private UpdateNeedDisObject updateObject;
        public DownLoadGroup(string newKey ,bool newAutoDispose = true)
        {
            Key = newKey;
            autoDispose = newAutoDispose;
            updateObject = new UpdateNeedDisObject(Key, Update, Dispose);
            State = DownloadState.normal;
            Error = null;
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
            updateObject.UnRegToOwner();
            updateObject = null;
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
            if(State != DownloadState.normal)
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
            if (State != DownloadState.normal) return;
            State = DownloadState.downloading;

            for (int i = 0; i < groupList.Count; i++)
            {
                groupList[i].StartDownLoadAsync();
            }

            PublicUpdateManager.AddUpdate(updateObject);
        }

        public void ReTryDownload()
        {
            if (State != DownloadState.finished || Error == null) return;
            State = DownloadState.normal;
            for (int i = groupList.Count - 1; i >= 0; i--)
            {
                if (groupList[i].IsDone && groupList[i].Error == null)
                    groupList.RemoveAt(i);
                else
                    groupList[i].RestState();
            }
            if (groupList.Count > 0)
                StartAsync();
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

            if (ProgressDelegate != null)
                ProgressDelegate(DownLoadedLength,ContentLength, Progress);

            if (isAllDone)
            {
                State = DownloadState.finished;

                string[] tcmpfile = new string[groupList.Count];
                string terror = null;
                for (int i = 0; i < groupList.Count; i++)
                {
                    tcmpfile[i] = groupList[i].CompleteFile;
                    if (groupList[i].Error != null)
                        terror += groupList[i].Error + "|";
                }

                Error = terror;

                Action<string[], string> tfinished = FinishedDelegate;

                if (autoDispose)
                    Dispose();
                else
                    updateObject.UnRegToOwner();

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

using System;
using System.Collections.Generic;
using LitEngine.UpdateSpace;
using System.Threading.Tasks;
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
        public event Action<string[], string> FinishedDelegate;
        public event Action<long, long, float> ProgressDelegate;
        private List<DownLoader> groupList = new List<DownLoader>();
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
            if (State != DownloadState.normal)
            {
                DLog.LogError("已经开始的任务不可插入新内容.");
                return;
            }
            if (IsHaveURL(_sourceurl)) return;
            Add(new DownLoader(_sourceurl, _destination, _clear));
        }

        private void Add(DownLoader newObject)
        {
            groupList.Add(newObject);
        }

        private bool IsHaveURL(string pSourceurl)
        {
            int tindex = groupList.FindIndex((a) => a.SourceURL.Contains(pSourceurl));
            if (tindex != -1)
            {
                DLog.LogError("重复添加下载,url = " + pSourceurl);
                return true;
            }

            return false;
        }

        public void Start()
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
                Start();
        }


        bool isAllChildDone {
            get
            {
                bool ret = true;
                for (int i = 0; i < groupList.Count; i++)
                {
                    ContentLength += groupList[i].ContentLength;
                    DownLoadedLength += groupList[i].DownLoadedLength;
                    if (!groupList[i].IsDone)
                    {
                        ret = false;
                        break;
                    }   
                }
                return ret;
            }
        }
        protected void Update()
        {
            if (IsDone) return;

            ContentLength = 0;
            DownLoadedLength = 0;

            if (ProgressDelegate != null)
                ProgressDelegate(DownLoadedLength,ContentLength, Progress);

            if (isAllChildDone)
            {
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

                State = DownloadState.finished;

            }
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Text;
namespace LitEngine.DownLoad
{
    public enum DownloadState
    {
        normal = 1,
        downloading,
        finished,
    }

    public class DownLoadGroup : IDownLoad
    {
        public event Action<DownLoadGroup> onComplete;
        public event Action<DownLoadGroup> OnProgress;
        public long ContentLength { get; private set; }
        public long DownLoadedLength { get; private set; }
        public float Progress { get; private set; }

        public bool IsDone { get; private set; }
        public bool IsCompleteDownLoad { get { return IsDone && Error == null; } } //成功下载
        public DownloadState State { get; private set; }
        public string Key { get; private set; }
        public string Error { get; private set; }
        private List<DownLoader> groupList = new List<DownLoader>();

        public DownLoadGroup(string newKey)
        {
            Key = newKey;
            State = DownloadState.normal;
            Error = null;
            ContentLength = 0;
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

            IsDone = true;

            onComplete = null;
            OnProgress = null;
        }
        #endregion

        public DownLoader AddByUrl(string pSourceurl, string pDestination, string pFileName,string pMD5, long pLength, bool pClear)
        {
            if (State != DownloadState.normal)
            {
                Debug.LogError("已经开始的任务不可插入新内容.");
                return null;
            }
            if (IsHaveURL(pSourceurl)) return null;
            DownLoader ret = new DownLoader(pSourceurl, pDestination,pFileName, pMD5, pLength, pClear);
            Add(ret);
            return ret;
        }

        private void Add(DownLoader newObject)
        {
            groupList.Add(newObject);
        }

        private bool IsHaveURL(string pSourceurl)
        {
            int tindex = groupList.FindIndex((a) => a.Key.Equals(pSourceurl));
            if (tindex != -1)
            {
                Debug.LogError("重复添加下载,url = " + pSourceurl);
                return true;
            }

            return false;
        }

        public void StartAsync()
        {
            if (State != DownloadState.normal) return;
            State = DownloadState.downloading;
            ContentLength = 0;
            for (int i = 0; i < groupList.Count; i++)
            {
                if (!groupList[i].IsCompleteDownLoad)
                {
                    DownLoadManager.DownLoadFileAsync(groupList[i],null,null);
                }
                ContentLength += groupList[i].InitContentLength;
            }
            Error = null;
            IsDone = false;
            DownLoadManager.AddGroup(this);
        }

        public void ReTryAsync()
        {
            if (State != DownloadState.finished || IsCompleteDownLoad) return;
            State = DownloadState.normal;
            IsDone = false;
            for (int i = groupList.Count - 1; i >= 0; i--)
            {
                if (!groupList[i].IsCompleteDownLoad)
                {
                    groupList[i].RestState();
                }
                    
            }
            if (groupList.Count > 0)
                StartAsync();
        }

        public Dictionary<string,string> GetNotCompletFileNameTable()
        {
            Dictionary<string,string> ret = new Dictionary<string,string>();
            for (int i = groupList.Count - 1; i >= 0; i--)
            {
                if (!groupList[i].IsCompleteDownLoad)
                {
                    ret.Add(groupList[i].FileName,groupList[i].SourceURL);
                }     
            }

            return ret;
        }


        bool UpdateChild()
        {
            if(groupList.Count == 0) return true;
            bool isAllDone = true;
            DownLoadedLength = 0;
            Progress = 0;
            for (int i = 0; i < groupList.Count; i++)
            {
                var item = groupList[i];
                DownLoadedLength += item.DownLoadedLength;
                Progress += item.Progress;
                if (!groupList[i].IsCompleteDownLoad)
                {
                    item.Update();
                    if (!item.IsDone)
                    {
                        isAllDone = false;
                    }
                }
            }
            Progress = Progress / groupList.Count;
            return isAllDone;
        }

        void DownLoadComplete()
        {
            if (IsDone) return;
            IsDone = true;

            StringBuilder terrostr = new StringBuilder();
            for (int i = 0; i < groupList.Count; i++)
            {
                if (groupList[i].Error != null)
                {
                    terrostr.AppendLine(groupList[i].Error);
                }
            }

            Error = terrostr.ToString();

            var tfinished = onComplete;

            try
            {
                if (tfinished != null)
                    tfinished(this);
            }
            catch (System.Exception _error)
            {
                Debug.LogError(_error);
            }
        }
        public void Update()
        {
            if (IsDone) return;

            switch (State)
            {
                case DownloadState.downloading:
                    {
                        bool tisAllDone = UpdateChild();
                        if (tisAllDone)
                        {
                            State = DownloadState.finished;
                        }
                        if (OnProgress != null)
                        {
                            OnProgress(this);
                        }
                    }
                    break;
                case DownloadState.finished:
                    {
                        DownLoadComplete();
                    }
                    break;
                default:
                    break;
            }
        }
    }
}

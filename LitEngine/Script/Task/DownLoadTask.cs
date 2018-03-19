using System;
namespace LitEngine
{
    namespace DownLoad
    {
        using LitTask;
        using UpdateSpace;
        public class DownLoadTask : TaskBase
        {
            public static bool DownLoadFileAsync(UpdateObjectVector _owner, string _appname, string _sourceurl, string _destination, bool _IsClear, Action<string, string> _finished, Action<long, long, float> _progress)
            {
                if (sDownLoadMap.ContainsKey(_sourceurl))
                {
                    DLog.LogError("有相同URL文件正在下载当中.URL = " + _sourceurl);
                    return false;
                }
                DownLoadTask ttaskdown = new DownLoadTask(_owner, _appname, _sourceurl, _destination, _IsClear, _finished, _progress);
                bool isstart = ttaskdown.StartDownloadAsync();
                if (!isstart)
                    ttaskdown.Dispose();
                return isstart;
            }

            public static bool DownLoadFileAsync(UpdateObjectVector _owner, string _sourceurl, string _destination, bool _IsClear, Action<string, string> _finished, Action<long, long, float> _progress)
            {
                if (sDownLoadMap.ContainsKey(_sourceurl))
                {
                    DLog.LogError("有相同URL文件正在下载当中.URL = " + _sourceurl);
                    return false;
                }
                DownLoadTask ttaskdown = new DownLoadTask(_owner, _sourceurl, _sourceurl, _destination, _IsClear, _finished, _progress);
                bool isstart = ttaskdown.StartDownloadAsync();
                if (!isstart)
                    ttaskdown.Dispose();
                return isstart;
            }

            private static SafeMap<string, DownLoadTask> sDownLoadMap = new SafeMap<string, DownLoadTask>();
            #region 属性
            private Action<string, string> mFinished;
            private Action<long, long, float> mProgress;
            private DownLoadObject mObject;
            private UpdateNeedDisObject mUpdateObject;
            private string mURL;
            private bool mIsDone = false;
            private bool mIsStart = false;

            public string AppName { get; protected set; }
            #endregion

            #region 构造析构
            private DownLoadTask()
            {

            }
            public DownLoadTask(UpdateObjectVector _owner,string _key,string _sourceurl, string _destination, bool _IsClear, Action<string,string> _finished, Action<long, long, float> _progress)
            {
                try
                {
                    AppName = _key;
                    mURL = _sourceurl;
                    mObject = new DownLoadObject(_sourceurl, _destination, _IsClear);

                    mFinished = _finished;
                    mProgress = _progress;

                    mUpdateObject = new UpdateNeedDisObject(AppName, Update, Dispose);
                    mUpdateObject.Owner = _owner;

                    sDownLoadMap.Add(mURL, this);
                }
                catch (Exception _error)
                {
                    DLog.LogError(_error);
                }
            }

            ~DownLoadTask()
            {
                Dispose(false);
            }
            override protected void DisposeGC()
            {
                sDownLoadMap.Remove(mURL);
                if (mObject != null) mObject.Dispose();
                if (mUpdateObject != null)
                    mUpdateObject.UnRegToOwner();
                mUpdateObject = null;
                mFinished = null;
                mProgress = null;
            }
            #endregion

            public bool StartDownloadAsync()
            {
                if (mUpdateObject == null)
                {
                    DLog.LogError("UpdateObject == null");
                    return false;
                }
                if (mIsStart) return true;
                mIsStart = true;
                mObject.StartDownLoadAsync();
                mUpdateObject.RegToOwner();
                return true;
            }

            override public bool IsDone
            {
                get
                {
                    if (mIsDone) return true;
                    if (mObject == null) return false;
                    if (mProgress != null)
                        mProgress((int)mObject.DownLoadedLength,(int)mObject.ContentLength, mObject.Progress);
                    if (!mObject.IsDone) return false;
                    mIsDone = true;

                    Action<string, string> tfinished = mFinished;
                    string tcompfile = mObject.CompleteFile;
                    string terror = mObject.Error;
                    Dispose();
                    try {
                        if (tfinished != null)
                            tfinished(tcompfile, terror);
                    }
                    catch (System.Exception _error)
                    {
                        DLog.LogError(_error);
                    }
                    
                    return true;
                }
            }
        }

    }

}

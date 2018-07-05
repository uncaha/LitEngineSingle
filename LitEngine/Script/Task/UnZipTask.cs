using System;
using System.IO;

namespace LitEngine
{
    namespace UnZip
    {
        using LitTask;
        using UpdateSpace;
        public class UnZipTask : TaskBase
        {
            public static bool UnZipFileAsync( string _source, string _destination, Action<string> _finished, Action<float> _progress)
            {
                if (sUnZipMap.ContainsKey(_source))
                {
                    DLog.LogError("有相同文件正在解压当中.source = " + _source);
                    return false;
                }
                UnZipTask ttaskunzip = new UnZipTask(_source, _destination, _finished, _progress);
                bool tstart = ttaskunzip.StartUnZipAsync();
                if (!tstart)
                    ttaskunzip.Dispose();
                return tstart;
            }

            private static SafeMap<string, UnZipTask> sUnZipMap = new SafeMap<string, UnZipTask>();
            #region 属性
            private Action<float> mProgress;
            private Action<string> mFinished;
            private UpdateNeedDisObject mUpdateObject;
            private UnZipObject mUnZipObject;
            private string mKey;
            private bool mIsDone = false;
            private bool mIsStart = false;

            #endregion
            #region 构造析构
            private UnZipTask()
            {

            }
            public UnZipTask(string _source, string _destination, Action<string> _finished, Action<float> _progress)
            {
                try {
                    mKey = _source;
                    mUnZipObject = new UnZipObject(_source, _destination);
                    mFinished = _finished;
                    mProgress = _progress;
                    mUpdateObject = new UpdateNeedDisObject(mKey, Update, Dispose);
                }
                catch (Exception _error)
                {
                    DLog.LogError(_error);
                }
            }

            ~UnZipTask()
            {
                Dispose(false);
            }

            override protected void DisposeGC()
            {
                sUnZipMap.Remove(mKey);
                if (mUnZipObject != null) mUnZipObject.Dispose();
                if (mUpdateObject != null)
                    mUpdateObject.UnRegToOwner();
                mUpdateObject = null;
                mFinished = null;
                mProgress = null;
            }
            #endregion

            public bool StartUnZipAsync()
            {
                if (mUpdateObject == null)
                {
                    DLog.LogError("UpdateObject == null");
                    return false;
                }
                if (mIsStart) return true;
                mIsStart = true;
                sUnZipMap.Add(mKey, this);
                mUnZipObject.StartUnZipAsync();
                PublicUpdateManager.AddUpdate(mUpdateObject);
                return true;
            }

            override public bool IsDone
            {
                get
                {
                    if (mIsDone) return true;
                    if (mUnZipObject == null) return false;
                    if (mProgress != null)
                        mProgress(mUnZipObject.Progress);
                    if (!mUnZipObject.IsDone) return false;
                    mIsDone = true;

                    Action<string> tfinished = mFinished;
                    string terror = mUnZipObject.Error;
                    Dispose();
                    try
                    {
                        if (tfinished != null)
                            tfinished(terror);
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

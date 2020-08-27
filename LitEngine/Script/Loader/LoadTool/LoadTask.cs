namespace LitEngine.LoadAsset
{
    public class LoadTask
    {
        #region 属性
        public string TaskKey { get; private set; }
        protected System.Action<string, object> mCallBack;
        protected BaseBundle mBundle;
        protected LoadTaskVector mParent;
        protected bool mRetain = false;
        #endregion
        public LoadTask(string _key, BaseBundle _bundle, System.Action<string, object> _callThreeParmater, bool _retain)
        {
            TaskKey = _key;
            mBundle = _bundle;
            mCallBack = _callThreeParmater;
            mRetain = _retain;
            if (mRetain)
                mBundle.Retain();
        }

        public LoadTaskVector Parent
        {
            get
            {
                return mParent;
            }

            set
            {
                mParent = value;
            }
        }

        #region 执行任务

        public bool IsDone()
        {
            if (!mBundle.Loaded)
            {
                return false;
            }
            if (mCallBack != null)
            {
                try
                {
                    if (!mBundle.WillBeReleased)
                        mCallBack(TaskKey, mBundle.Asset);
                }
                catch (System.Exception _error)
                {
                    DLog.LogError(_error);
                }
            }
            DestoryTaskFormParent();
            return true;

        }


        #endregion
        public virtual void DestoryTaskFormParent()
        {
            if (Parent != null)
                Parent.Remove(this);
        }
    }

}


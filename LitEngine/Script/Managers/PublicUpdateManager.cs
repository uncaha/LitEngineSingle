using System;
using System.Collections.Generic;
namespace LitEngine
{
    using UpdateSpace;
    using DownLoad;
    using UnZip;
    using Loader;
    public class PublicUpdateManager : MonoManagerBase
    {
        private static PublicUpdateManager sInstance = null;
        private UpdateObjectVector UpdateList = new UpdateObjectVector(UpdateType.Update);
        private List<BaseBundle> mWaitLoadBundle = new List<BaseBundle>();
        static private void CreatInstance()
        {
            if (sInstance == null)
            {
                UnityEngine.GameObject tobj = new UnityEngine.GameObject("PublicUpdateManager");
                UnityEngine.GameObject.DontDestroyOnLoad(tobj);
                sInstance = tobj.AddComponent<PublicUpdateManager>();
                AppCore.AddPublicMono("PublicUpdateManager", sInstance);
                tobj.SetActive(false);
            }
        }

        static public void AddWaitLoadBundle(BaseBundle _bundle)
        {
            if (_bundle == null) return;
            if (sInstance == null) CreatInstance();
            if(sInstance.mWaitLoadBundle.Contains(_bundle))
            {
                sInstance.mWaitLoadBundle.Add(_bundle);
                sInstance.SetActive(true);
            }
        }

        static public void ClearByKey(string _appkey)
        {
            if(sInstance != null)
                sInstance.UpdateList.ClearByKey(_appkey);
        }

        static public void DownLoadFileAsync(string _AppName, string _sourceurl, string _destination, bool _IsClear, Action<string, string> _finished, Action<long, long, float> _progress)
        {
            if (sInstance == null) CreatInstance();
            string tdeleappname = GameCore.GetDelegateAppName(_finished);
            if (!string.IsNullOrEmpty(tdeleappname))
                _AppName = tdeleappname;
            if (DownLoadTask.DownLoadFileAsync(sInstance.UpdateList, _AppName, _sourceurl, _destination, _IsClear, _finished, _progress))
                sInstance.SetActive(true);
        }

        static public void UnZipFileAsync(string _appname, string _source, string _destination, Action<string> _finished, Action<float> _progress)
        {
            if (sInstance == null) CreatInstance();
            string tdeleappname = GameCore.GetDelegateAppName(_finished);
            if (!string.IsNullOrEmpty(tdeleappname))
                _appname = tdeleappname;
            if (UnZipTask.UnZipFileAsync(sInstance.UpdateList, _appname, _source, _destination, _finished, _progress))
                sInstance.SetActive(true);
        }

        override protected void OnDestroy()
        {
            sInstance = null;
            UpdateList.Clear();
            base.OnDestroy();
        }

        public PublicUpdateManager()
        {

        }

        protected void SetActive(bool _active)
        {
            if (gameObject.activeSelf != _active)
                gameObject.SetActive(_active);
        }

        void UpdateWaitLoad()
        {
            if (mWaitLoadBundle.Count == 0) return;
            for(int i = mWaitLoadBundle.Count -1;i>=0;i--)
            {
                BaseBundle tbundle = mWaitLoadBundle[i];
                if(tbundle.IsDone())
                {
                    mWaitLoadBundle.RemoveAt(i);
                    tbundle.Destory();
                }
            }
        }

        void Update()
        {
            UpdateList.Update();
            UpdateWaitLoad();
            if (UpdateList.Count == 0 && mWaitLoadBundle.Count == 0)
                gameObject.SetActive(false);
        }
    }
}

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using LitEngine.LoadAsset;
using LitEngine.DownLoad;
using System.IO;
using System.Collections.Generic;
using System.Text;
namespace LitEngine.UpdateTool
{
    public class UpdateManager : MonoBehaviour
    {
        public const string checkfile = "checkInfoData.txt";
        public const string downloadedfile = "downloadedData.txt";
        public const string upateMgrData = "updateData";
        private static object lockobj = new object();
        private static UpdateManager sInstance = null;
        private static UpdateManager Instance
        {
            get
            {
                if (sInstance == null)
                {
                    lock (lockobj)
                    {

                        if (sInstance == null)
                        {
                            GameObject tobj = new GameObject("UpdateManager");
                            GameObject.DontDestroyOnLoad(tobj);
                            sInstance = tobj.AddComponent<UpdateManager>();
                            sInstance.Init();
                        }
                    }
                }
                return sInstance;
            }
        }

        public static string platformPath = null;

        public UpdateData updateData { get; private set; }

        private bool mInited = false;
        private UpdateManager()
        {

        }
        private void Init()
        {
            if (mInited) return;
            mInited = true;

            updateData = new UpdateData();
            TextAsset datatxt = Resources.Load<TextAsset>(upateMgrData);
            if (datatxt != null)
            {
                UnityEngine.JsonUtility.FromJsonOverwrite(datatxt.text, updateData);
            }
            else
            {
                StringBuilder tstrbd = new StringBuilder();
                tstrbd.AppendLine(string.Format("加载版本文件失败.请检查Resources目录下是否有 {0}.txt 文件", upateMgrData));
                tstrbd.AppendLine("格式如下:");
                tstrbd.AppendLine("{");
                tstrbd.AppendLine("\"version\":\"1\",");
                tstrbd.AppendLine("\"server\":\"http://localhost/Resources/\"");
                tstrbd.AppendLine("}");

                DLog.LogError(tstrbd.ToString());
                
            }
        }

        private void OnDestroy()
        {

        }
        private void OnDisable()
        {
            Stop();
            ReleaseGroupLoader();
            ReleaseCheckLoader();
        }


        #region prop
        static public float DownloadProcess { get; private set; }
        static public long DownLoadLength { get; private set; }
        static public long ContentLength { get; private set; }
        static public DownLoadGroup updateGroup
        {
            get
            {
                return Instance.downLoadGroup;
            }
        }
        static public DownLoader checkDL
        {
            get
            {
                return Instance.checkLoader;
            }
        }
        static public bool IsRuningUpdate
        {
            get
            {
                if (isUpdateing || isChecking) return true;
                return false;
            }
        }
        static bool isUpdateing = false;
        static bool isChecking = false;

        int ReTryMaxCount = 20;
        int ReTryCount = 0;
        int ReTryCheckCount = 0;
        DownLoadGroup downLoadGroup;
        DownLoader checkLoader;

        ByteFileInfoList curInfo;
        UpdateComplete curOnComplete;
        bool curAutoRetry;
        #endregion

        #region update
        public delegate void UpdateComplete(ByteFileInfoList info, string error);
        static public void StopAll()
        {
            Instance.Stop();
        }

        static public bool ReStart()
        {
           return Instance.ReTryGroupDownload();
        }

        private void Stop()
        {
            StopAllCoroutines();
            ReTryCount = 0;
            ReTryCheckCount = 0;
            isUpdateing = false;
            isChecking = false;
            StopGroupLoader();
        }

        void ReleaseGroupLoader()
        {
            if (downLoadGroup == null) return;
            downLoadGroup.Dispose();
            downLoadGroup = null;
        }

        void StopGroupLoader()
        {
            if (downLoadGroup == null) return;
            downLoadGroup.Stop();
        }

        bool ReTryGroupDownload()
        {
            if (isUpdateing)
            {
                Debug.LogError("Updateing.");
                return false;
            }
            if (downLoadGroup == null) return false;
            isUpdateing = true;
            downLoadGroup.ReTryAsync();
            StartCoroutine(WaitUpdateDone());

            return true;
        }

        static public void UpdateRes(ByteFileInfoList pInfo, UpdateComplete onComplete, bool autoRetry)
        {
            if (isUpdateing)
            {
                Debug.LogError("Updateing.");
                return;
            }
            isUpdateing = true;
            Instance.StartCoroutine(Instance.WaitStarUpdate(0.1f, pInfo, onComplete, autoRetry));
        }

        IEnumerator WaitStarUpdate(float delayTime, ByteFileInfoList pInfo, UpdateComplete onComplete, bool autoRetry)
        {
            yield return new WaitForSeconds(delayTime);
            Instance.FileUpdateing(pInfo, onComplete, autoRetry);
        }

        void FileUpdateing(ByteFileInfoList pInfo, UpdateComplete onComplete, bool autoRetry)
        {
            curInfo = pInfo;
            curOnComplete = onComplete;
            curAutoRetry = autoRetry;
            ReleaseGroupLoader();
            downLoadGroup = new DownLoadGroup("updateGroup");
            foreach (var item in pInfo.fileInfoList)
            {
                string turl = GetServerUrl(item.resName);
                var tloader = downLoadGroup.AddByUrl(turl, GameCore.PersistentResDataPath, item.resName, item.fileMD5, item.fileSize, false);
                tloader.priority = item.priority;
                tloader.OnComplete += (a) =>
                {
                    if (a.IsCompleteDownLoad)
                    {
                        OnUpdateOneComplete(pInfo[a.FileName]);
                    }
                };
            }
            downLoadGroup.StartAsync();
            UpdateProcess();

            StartCoroutine(WaitUpdateDone());

        }

        IEnumerator WaitUpdateDone()
        {
            while (!downLoadGroup.IsDone)
            {
                UpdateProcess();
                yield return null;
            }
            UpdateProcess();
            if (downLoadGroup.IsCompleteDownLoad)
            {
                UpdateFileFinished();
            }
            else
            {
                UpdateFileFail();
            }
        }

        IEnumerator WaitReTryUpdate()
        {
            yield return new WaitForSeconds(5f);
            downLoadGroup.ReTryAsync();
            StartCoroutine(WaitUpdateDone());
        }

        void UpdateProcess()
        {
            if (downLoadGroup == null) return;
            DownloadProcess = downLoadGroup.Progress;
            ContentLength = downLoadGroup.ContentLength;
            DownLoadLength = downLoadGroup.DownLoadedLength;
        }

        void UpdateFileFinished()
        {
            isUpdateing = false;
            UpdateLocalList();
            CallUpdateOnComplete(curOnComplete, null, null);
        }

        void UpdateFileFail()
        {
            isUpdateing = false;
            ByteFileInfoList erroListInfo = GetErroListInfo(downLoadGroup, curInfo);
            if (ReTryCount >= ReTryMaxCount)
            {
                curAutoRetry = false;
            }
            if (!curAutoRetry)
            {
                CallUpdateOnComplete(curOnComplete, erroListInfo, downLoadGroup.Error);
            }
            else
            {
                Debug.Log(downLoadGroup.Error);
                ReTryCount++;
                StartCoroutine(WaitReTryUpdate());
            }
        }

        void CallUpdateOnComplete(UpdateComplete onComplete, ByteFileInfoList pInfo, string pError)
        {
            try
            {
                ReleaseGroupLoader();
                ReTryCount = 0;
                onComplete?.Invoke(pInfo, pError);
            }
            catch (System.Exception erro)
            {
                Debug.LogError("CheckUpdate->" + erro.ToString());
            }
        }

        ByteFileInfoList GetErroListInfo(DownLoadGroup pGroup, ByteFileInfoList pInfo)
        {
            var tlist = pGroup.GetNotCompletFileNameTable();
            pInfo.RemoveRangeWithOutList(tlist);
            return pInfo;
        }
        void OnUpdateOneComplete(ByteFileInfo pInfo)
        {
            if (pInfo == null) return;
            try
            {
                string tline = UnityEngine.JsonUtility.ToJson(pInfo);
                List<string> tlines = new List<string>();
                tlines.Add(tline);
                string tdedfile = GameCore.CombinePath(GameCore.PersistentResDataPath, downloadedfile);
                File.AppendAllLines(tdedfile, tlines);
            }
            catch (System.Exception erro)
            {
                Debug.LogError(erro.Message);
            }
        }
        #endregion

        #region check
        public delegate void CheckComplete(ByteFileInfoList info, string error);
        public static bool autoUseCacheCheck = false;
        void ReleaseCheckLoader()
        {
            if (checkLoader == null) return;
            checkLoader.Dispose();
            checkLoader = null;
        }

        static public void CheckUpdate(CheckComplete onComplete, bool useCache , bool needRetry)
        {
            if (isChecking || isUpdateing)
            {
                Debug.LogError("Checking or Updateing.");
                return;
            }
            isChecking = true;
            Instance.StartCoroutine(sInstance.WaitStartCheck(0.1f, onComplete, useCache, needRetry));
        }

        IEnumerator WaitStartCheck(float delayTime, CheckComplete onComplete, bool useCache, bool needRetry)
        {
            yield return new WaitForSeconds(delayTime);
            Instance.StartCoroutine(sInstance.CheckingUpdate(onComplete, useCache,needRetry));
        }

        IEnumerator CheckingUpdate(CheckComplete onComplete, bool useCache, bool needRetry)
        {
            ReleaseCheckLoader();

            string tuf = GetServerUrl(LoaderManager.byteFileInfoFileName + BaseBundle.sSuffixName);
            string tcheckfile = GetCheckFileName();
            string tfilePath = GameCore.CombinePath(GameCore.PersistentResDataPath, tcheckfile);
            
            if (!useCache || !File.Exists(tfilePath))
            {
                checkLoader = DownLoadManager.DownLoadFileAsync(tuf, GameCore.PersistentResDataPath, tcheckfile, null, 0, null);
                while (!checkLoader.IsDone)
                {
                    yield return null;
                }
                DownLoadCheckFileEnd(checkLoader, onComplete, useCache, needRetry);
            }
            else
            {
                DownLoadCheckFileFinished(onComplete);
            }

        }

        IEnumerator WaitRetryCheck(float dt,CheckComplete onComplete, bool useCache, bool needRetry)
        {
            yield return new WaitForSeconds(dt);
            CheckUpdate(onComplete, useCache, needRetry);
        }

        void DownLoadCheckFileFinished(CheckComplete onComplete)
        {
            isChecking = false;
            ByteFileInfoList ret = GetNeedDownloadFiles(GetUpdateList());
            CallCheckOnComplete(onComplete, ret);
        }

        void DownLoadCheckFileFail(DownLoader dloader,CheckComplete onComplete, bool useCache, bool needRetry)
        {
            isChecking = false;
            string tfilePath = GameCore.CombinePath(GameCore.PersistentResDataPath, GetCheckFileName());
            bool isfileExit = File.Exists(tfilePath);
            if (dloader.IsCompleteDownLoad && isfileExit)
            {
                DownLoadCheckFileFinished(onComplete);
            }
            else
            {
                if (ReTryCheckCount >= ReTryMaxCount)
                {
                    needRetry = false;
                }

                if (needRetry)
                {
                    ReTryCheckCount++;
                    if (autoUseCacheCheck && isfileExit)
                    {
                        StartCoroutine(WaitRetryCheck(0.1f, onComplete, true, needRetry));
                    }
                    else
                    {
                        StartCoroutine(WaitRetryCheck(3, onComplete, useCache, needRetry));
                    }

                }
                else
                {
                    CallCheckOnComplete(onComplete, null);
                }
            }
        }

        void DownLoadCheckFileEnd(DownLoader dloader, CheckComplete onComplete,bool useCache, bool needRetry)
        {
            if (dloader.IsCompleteDownLoad)
            {
                DownLoadCheckFileFinished(onComplete);
            }
            else
            {
                Debug.Log(dloader.Error);
                DownLoadCheckFileFail(dloader,onComplete, useCache, needRetry);
            }
        }

        void CallCheckOnComplete(CheckComplete onComplete, ByteFileInfoList pObj)
        {
            try
            {
                string error = checkLoader != null ? checkLoader.Error : null;

                ReleaseCheckLoader();
                ReTryCheckCount = 0;
               
                onComplete?.Invoke(pObj, error);
            }
            catch (System.Exception erro)
            {
                Debug.LogError("CheckUpdate->" + erro.ToString());
            }
        }

        ByteFileInfoList GetNeedDownloadFiles(List<ByteFileInfo> pList)
        {
            if (pList == null || pList.Count == 0) return null;


            var tcmp = new ByteFileInfoList();
            tcmp.AddRange(pList);

            string tdedfile = GameCore.CombinePath(GameCore.PersistentResDataPath, downloadedfile);
            var tdedinfo = new ByteFileInfoList(tdedfile);
            var tneedList = tdedinfo.Comparison(tcmp);

            if (tneedList.Count == 0)
            {
                UpdateLocalList();
                return null;
            }
            else
            {
                ByteFileInfoList ret = new ByteFileInfoList();
                ret.AddRange(tneedList);
                return ret;
            }

        }

        List<ByteFileInfo> GetUpdateList()
        {
            List<ByteFileInfo> ret = null;
            var tinfo = new ByteFileInfoList();
            string tfilePath = GameCore.CombinePath(GameCore.PersistentResDataPath, GetCheckFileName());

            if (File.Exists(tfilePath))
            {
                AssetBundle tinfobundle = AssetBundle.LoadFromFile(tfilePath);
                if (tinfobundle != null)
                {
                    TextAsset tass = tinfobundle.LoadAsset<TextAsset>(LoaderManager.byteFileInfoFileName);
                    if (tass != null)
                    {
                        tinfo.Load(tass.bytes);
                    }
                    tinfobundle.Unload(false);
                }
            }

            ret = LoaderManager.ByteInfoData.Comparison(tinfo);
            return ret;
        }

        string GetCheckFileName()
        {
            return string.Format("{0}_{1}", updateData.version, checkfile);
        }

        void UpdateLocalList()
        {
            string tfilePath = GameCore.CombinePath(GameCore.PersistentResDataPath, GetCheckFileName());
            string tsavefile = GameCore.CombinePath(GameCore.PersistentResDataPath, LoaderManager.byteFileInfoFileName + BaseBundle.sSuffixName);

            if (File.Exists(tfilePath))
            {
                if (File.Exists(tsavefile))
                {
                    File.Delete(tsavefile);
                }
                File.Copy(tfilePath, tsavefile);

                LoaderManager.ReLoadResInfo();
            }

            string tdedfile = GameCore.CombinePath(GameCore.PersistentResDataPath, downloadedfile);
            if (File.Exists(tdedfile))
            {
                File.Delete(tdedfile);
            }
        }
        #endregion

        public string GetPlatformPath()
        {
            if (string.IsNullOrWhiteSpace(platformPath))
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.tvOS:
                    case RuntimePlatform.OSXPlayer:
                        platformPath = "ios";
                        break;
                    case RuntimePlatform.Android:
                        platformPath = "android";
                        break;
                    case RuntimePlatform.LinuxEditor:
                    case RuntimePlatform.OSXEditor:
                    case RuntimePlatform.WindowsEditor:
                        platformPath = "editor";
                        break;
                    default:
                        platformPath = Application.platform.ToString();
                        break;
                }
                DLog.LogError("UpdateManager未设置 platform,启用默认设置. platformPath = " + platformPath);
            }
            return platformPath;
        }
        public string GetServerUrl(string pFile)
        {
            string tkey = GetPlatformPath();
            return string.Format("{0}/{1}/{2}/{3}", updateData.server, tkey, updateData.version, pFile);
        }
    }
}
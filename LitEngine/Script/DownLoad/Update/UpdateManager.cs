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

        public UpdateData updateData;

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
        }

        private void Stop()
        {
            StopAllCoroutines();
            ReTryCount = 0;
            ReTryCheckCount = 0;
            isUpdateing = false;
            isChecking = false;
            ReleaseGroupLoader();
            ReleaseCheckLoader();
        }

        #region prop
        static public DownLoadGroup updateGroup
        {
            get
            {
                return Instance.downLoadGroup;
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

        static int ReTryMaxCount = 5;
        static int ReTryCount = 0;
        static int ReTryCheckCount = 0;
        DownLoadGroup downLoadGroup;
        DownLoader checkLoader;
        #endregion

        #region update
        static public void StopAll()
        {
            Instance.Stop();
        }

        void ReleaseGroupLoader()
        {
            if (downLoadGroup == null) return;
            downLoadGroup.Dispose();
            downLoadGroup = null;
        }

        static public void UpdateRes(ByteFileInfoList pInfo, System.Action<ByteFileInfoList, string> onComplete, bool autoRetry = false)
        {
            if (isUpdateing)
            {
                Debug.LogError("更新中,请勿重复调用.");
                return;
            }
            isUpdateing = true;
            Instance.StartCoroutine(Instance.WaitStarUpdate(0.1f, pInfo, onComplete, autoRetry));
        }

        IEnumerator WaitStarUpdate(float delayTime, ByteFileInfoList pInfo, System.Action<ByteFileInfoList, string> onComplete, bool autoRetry)
        {
            yield return new WaitForSeconds(delayTime);
            Instance.StartCoroutine(Instance.FileUpdateing(pInfo, onComplete, autoRetry));
        }

        IEnumerator FileUpdateing(ByteFileInfoList pInfo, System.Action<ByteFileInfoList, string> onComplete, bool autoRetry)
        {
            string tdicpath = string.Format("{0}/{1}/", updateData.server, updateData.version);
            ReleaseGroupLoader();
            downLoadGroup = new DownLoadGroup("updateGroup");
            foreach (var item in pInfo.fileInfoList)
            {
                string turl = tdicpath + item.resName;
                var tloader = downLoadGroup.AddByUrl(turl, GameCore.PersistentResDataPath, item.resName, item.fileMD5, item.fileSize, false);
                tloader.OnComplete += (a) =>
                {
                    if (string.IsNullOrEmpty(a.Error))
                    {
                        OnUpdateOneComplete(pInfo[a.FileName]);
                    }
                };
            }
            downLoadGroup.StartAsync();

            while (!downLoadGroup.IsDone)
            {
                yield return null;
            }

            if (string.IsNullOrEmpty(downLoadGroup.Error))
            {
                UpdateFileFinished(onComplete);
            }
            else
            {
                UpdateFileFail(pInfo, onComplete, autoRetry);
            }

        }

        void UpdateFileFinished(System.Action<ByteFileInfoList, string> onComplete)
        {
            isUpdateing = false;
            UpdateLocalList();
            CallUpdateOnComplete(onComplete, null, null);
        }

        void UpdateFileFail(ByteFileInfoList pInfo, System.Action<ByteFileInfoList, string> onComplete, bool autoRetry)
        {
            isUpdateing = false;
            ByteFileInfoList erroListInfo = GetErroListInfo(downLoadGroup, pInfo);
            if (ReTryCount >= ReTryMaxCount)
            {
                autoRetry = false;
            }
            if (!autoRetry)
            {
                CallUpdateOnComplete(onComplete, erroListInfo, downLoadGroup.Error);
            }
            else
            {
                Debug.Log(downLoadGroup.Error);
                ReTryCount++;
                UpdateRes(erroListInfo, onComplete, autoRetry);
            }
        }

        void CallUpdateOnComplete(System.Action<ByteFileInfoList, string> onComplete, ByteFileInfoList pInfo, string pError)
        {
            try
            {
                if (downLoadGroup != null)
                {
                    downLoadGroup.Dispose();
                }
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
                string tdedfile = Path.Combine(GameCore.PersistentResDataPath, downloadedfile);
                File.AppendAllLines(tdedfile, tlines);
            }
            catch (System.Exception erro)
            {
                Debug.LogError(erro.Message);
            }
        }
        #endregion

        #region check
        void ReleaseCheckLoader()
        {
            if (checkLoader == null) return;
            checkLoader.Dispose();
            checkLoader = null;
        }

        static public void CheckUpdate(System.Action<ByteFileInfoList> onComplete, bool needRetry = false)
        {
            if (isChecking || isUpdateing)
            {
                Debug.LogError("更新流程进行中,请勿重复调用.");
                return;
            }
            isChecking = true;
            Instance.StartCoroutine(sInstance.WaitStartCheck(0.1f, onComplete, needRetry));
        }

        IEnumerator WaitStartCheck(float delayTime, System.Action<ByteFileInfoList> onComplete, bool needRetry)
        {
            yield return new WaitForSeconds(delayTime);
            Instance.StartCoroutine(sInstance.CheckingUpdate(onComplete, needRetry));
        }

        IEnumerator CheckingUpdate(System.Action<ByteFileInfoList> onComplete, bool needRetry)
        {
            string tdicpath = string.Format("{0}/{1}/", updateData.server, updateData.version);
            string tuf = tdicpath + LoaderManager.byteFileInfoFileName;
            string tcheckfile = GetCheckFileName();
            string tfilePath = Path.Combine(GameCore.PersistentResDataPath, tcheckfile);
            if (!File.Exists(tfilePath))
            {
                ReleaseCheckLoader();
                checkLoader = DownLoadManager.DownLoadFileAsync(tuf, GameCore.PersistentResDataPath, tcheckfile, null, 0, null);
                while (!checkLoader.IsDone)
                {
                    yield return null;
                }
                DownLoadCheckFileEnd(checkLoader, onComplete, needRetry);
            }
            else
            {
                DownLoadCheckFileFinished(onComplete);
            }

        }

        void DownLoadCheckFileFinished(System.Action<ByteFileInfoList> onComplete)
        {
            isChecking = false;
            ByteFileInfoList ret = GetNeedDownloadFiles(GetUpdateList());
            CallCheckOnComplete(onComplete, ret);
        }

        void DownLoadCheckFileFail(System.Action<ByteFileInfoList> onComplete, bool needRetry)
        {
            isChecking = false;
            if (ReTryCheckCount >= ReTryMaxCount)
            {
                needRetry = false;
            }

            if (needRetry)
            {
                ReTryCheckCount++;
                CheckUpdate(onComplete, needRetry);
            }
            else
            {
                CallCheckOnComplete(onComplete, null);
            }
        }

        void DownLoadCheckFileEnd(DownLoader dloader, System.Action<ByteFileInfoList> onComplete, bool needRetry)
        {
            if (dloader.Error == null)
            {
                DownLoadCheckFileFinished(onComplete);
            }
            else
            {
                Debug.Log(dloader.Error);
                DownLoadCheckFileFail(onComplete, needRetry);
            }
        }

        static void CallCheckOnComplete(System.Action<ByteFileInfoList> onComplete, ByteFileInfoList pObj)
        {
            try
            {
                ReTryCheckCount = 0;
                onComplete?.Invoke(pObj);
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

            string tdedfile = Path.Combine(GameCore.PersistentResDataPath, downloadedfile);
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
            string tfilePath = Path.Combine(GameCore.PersistentResDataPath, GetCheckFileName());

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
            string tfilePath = Path.Combine(GameCore.PersistentResDataPath, GetCheckFileName());
            string tsavefile = Path.Combine(GameCore.PersistentResDataPath, LoaderManager.byteFileInfoFileName + BaseBundle.sSuffixName);

            if (File.Exists(tfilePath))
            {
                if (File.Exists(tsavefile))
                {
                    File.Delete(tsavefile);
                }
                File.Copy(tfilePath, tsavefile);

                LoaderManager.ReLoadResInfo();
            }

            string tdedfile = Path.Combine(GameCore.PersistentResDataPath, downloadedfile);
            if (File.Exists(tdedfile))
            {
                File.Delete(tdedfile);
            }
        }
        #endregion
    }
}
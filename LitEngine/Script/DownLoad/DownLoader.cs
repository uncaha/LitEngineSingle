using UnityEngine.Networking;
using System.IO;
using UnityEngine;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace LitEngine.DownLoad
{
    public class DownLoader : IDownLoad
    {
        #region event
        public event System.Action<DownLoader> OnStart = null;
        public event System.Action<DownLoader> OnComplete = null;
        public event System.Action<DownLoader> OnProgress = null;
        #endregion
        #region 属性
        public string Key { get; private set; }
        public string DestinationPath { get; private set; }//目标路径
        public string SourceURL { get; private set; }//url
        public string TempFile { get; private set; }//下载临时文件名字F ullPath
        public string TempPath{ get; private set; }//下载临时文件名路径
        public string CompleteFile { get; private set; }//下载结束文件 FullPath
        public string FileName { get; private set; }//文件名
        public string Error { get; private set; }//error message
        public float Progress
        {
            get
            {
                return Mathf.Clamp01(ContentLength > 0 ? (float)DownLoadedLength / ContentLength : 0);
            }
        }
        
        public DownloadState State { get; private set; }
        public bool IsDone { get;private set;}//下载线程完成
        public bool IsCompleteDownLoad { get { return IsDone && Error == null; } } //成功下载

        public string MD5String { get; private set; }
        public long ContentLength { get; private set; }//需要下载的长度
        public long DownLoadedLength { get; private set; }//已下载长度
        public long InitContentLength { get; private set; }//预下载长度


        private bool mIsClear = false;//是否清除断点续传

        private bool mThreadRuning = false;

        private HttpWebRequest mReqest;
        private WebResponse mResponse;
        private Stream mHttpStream;


        #endregion
        #region 构造析构
        public DownLoader(string pSourceurl, string pDestination,string pFileName = null,string pMD5 = null,long pLength = 0, bool pClear = true)
        {
            Key = pSourceurl;
            SourceURL = pSourceurl;
            DestinationPath = pDestination;
            FileName = pFileName;
            MD5String = pMD5;
            mIsClear = pClear;
            InitContentLength = pLength;


            string[] turlstrs = SourceURL.Split('/');
            string tfileName = turlstrs[turlstrs.Length - 1];
            if (string.IsNullOrEmpty(FileName))
            {
                FileName = tfileName;
            }
            TempPath = string.Format("{0}/tempDownLoad", DestinationPath);
            TempFile = string.Format("{0}/{1}.temp", TempPath, string.IsNullOrEmpty(MD5String) ? tfileName : MD5String);
            CompleteFile = DestinationPath + "/" + FileName;

            State = DownloadState.normal;
            Error = null;
        }

        ~DownLoader()
        {
            Dispose(false);
        }

        private bool mDisposed = false;
        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        private void Dispose(bool pDisposing)
        {
            if (mDisposed)
                return;
            mDisposed = true;

            IsDone = true;
            mThreadRuning = false;
            CloseHttpClient();
            OnComplete = null;
            OnProgress = null;
            OnStart = null;
        }
        #endregion

        public void RestState()
        {
            if (State != DownloadState.finished || IsCompleteDownLoad) return;
            State = DownloadState.normal;
            IsDone = false;
            Error = null;
        }

        public void StartAsync()
        {
            if (State != DownloadState.normal) return;
            State = DownloadState.downloading;
            mThreadRuning = true;
            IsDone = false;
            Error = null;
            OnStart?.Invoke(this);
            Task.Run((System.Action)ReadNetByte);
        }

        private void ReadNetByte()
        {
            FileStream ttempfile = null;
            try
            {

                if (!Directory.Exists(TempPath))
                {
                    Directory.CreateDirectory(TempPath);
                }
                if(!Directory.Exists(DestinationPath))
                {
                    Directory.CreateDirectory(DestinationPath);
                }

                long thaveindex = 0;
                if (File.Exists(TempFile))
                {

                    if (!mIsClear)
                    {
                        ttempfile = System.IO.File.OpenWrite(TempFile);
                        thaveindex = ttempfile.Length;
                        ttempfile.Seek(thaveindex, SeekOrigin.Current);
                    }
                    else
                    {
                        File.Delete(TempFile);
                        thaveindex = 0;
                    }

                }

                mReqest = (HttpWebRequest)HttpWebRequest.Create(SourceURL);
                mReqest.Timeout = 20000;

                if (SourceURL.Contains("https://"))
                {
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                }

                if (thaveindex > 0)
                {
                    mReqest.AddRange((int)thaveindex);
                }

                string rspError = null;
                try
                {
                    mResponse = mReqest.GetResponse();
                }
                catch (System.Exception _error)
                {
                    rspError = _error.Message;
                }

                if (mResponse == null || rspError != null)
                {
                    string terro = string.Format("ReadNetByte 获取Response失败.[Error] = {0}", rspError);
                    throw new System.NullReferenceException(terro);
                }

                mHttpStream = mResponse.GetResponseStream();
                ContentLength = mResponse.ContentLength;
                InitContentLength = ContentLength;//重置为实际大小

                if (ttempfile == null)
                {
                    ttempfile = System.IO.File.Create(TempFile);
                }
                    
                int tcount = 0;
                int tlen = 1024;
                byte[] tbuffer = new byte[tlen];
                int tReadSize = 0;
                tReadSize = mHttpStream.Read(tbuffer, 0, tlen);
                while (tReadSize > 0 && mThreadRuning)
                {
                    DownLoadedLength += tReadSize;
                    ttempfile.Write(tbuffer, 0, tReadSize);
                    tReadSize = mHttpStream.Read(tbuffer, 0, tlen);

                    if (++tcount >= 512)
                    {
                        ttempfile.Flush();
                        tcount = 0;
                    }

                }
            }
            catch (System.Exception _error)
            {
                Error = string.Format("[URL] = {0},[Error] = {1}", SourceURL, _error.Message);
            }

            if (ttempfile != null)
            {
                ttempfile.Close();
            }

            try
            {
                if (DownLoadedLength == ContentLength)
                {
                    if (File.Exists(TempFile))
                    {
                        if (File.Exists(CompleteFile))
                        {
                            File.Delete(CompleteFile);
                        }

                        int tindex = CompleteFile.LastIndexOf('/');
                        string tcompletePagth = CompleteFile.Substring(0, tindex);

                        if (!Directory.Exists(tcompletePagth))
                        {
                            Directory.CreateDirectory(tcompletePagth);
                        }
                        File.Move(TempFile, CompleteFile);
                    }
                }
                else
                {
                    if (Error == null)
                    {
                        Error = string.Format("[URL] = {0},[Error] = 文件未能完成下载.Stream 被中断.", SourceURL);
                    }
                        
                }
            }
            catch (System.Exception erro)
            {
                Error = string.Format("[URL] = {0},[Error] = {1}", SourceURL, erro.Message);
            }

            FinishedThread();  
        }

        private void FinishedThread()
        {
            CloseHttpClient();
            mThreadRuning = false;
            State = DownloadState.finished;
        }
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
        private void CloseHttpClient()
        {
            if (mHttpStream != null)
            {
                mHttpStream.Close();
                mHttpStream = null;
            }

            if (mResponse != null)
            {
                mResponse.Close();
                mResponse = null;
            }

            if (mReqest != null)
            {
                mReqest.Abort();
                mReqest = null;
            }
        }
    

        void CallComplete()
        {
            if(IsDone) return;
            IsDone = true;
            try
            {
                 OnComplete?.Invoke(this);
            }
            catch (System.Exception e)
            {
                Debug.LogErrorFormat("DownLoader->CallComplete 出现错误.Url = {0},erro = {1}",SourceURL,e.ToString());
            }
           
        }
        public void Update()
        {
            if (IsDone) return;
            switch (State)
            {
                case DownloadState.downloading:
                    {
                        OnProgress?.Invoke(this);
                    }
                    break;
                case DownloadState.finished:
                    {
                        CallComplete();
                    }
                    break;
                default:
                    break;
            }
        }
    }
}

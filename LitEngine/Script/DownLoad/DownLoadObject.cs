using UnityEngine.Networking;
using System.IO;
using UnityEngine;
using System.Net;
using System.Threading;

using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace LitEngine
{
    namespace DownLoad
    {
        public class DownLoadObject: YieldInstruction, System.IDisposable
        {
            #region 属性
            public string DestinationPath { get; private set; }
            public string SourceURL { get; private set; }
            public string TempFile { get; private set; }
            public string CompleteFile { get; private set; }
            public string FileName { get; private set; }
            public string Error { get; private set; }
            public float Progress
            {
                get
                {
                    return ContentLength > 0 ?(float)DownLoadedLength / ContentLength : 0;
                }
            }

            public bool IsDone
            {
                get {
                    if (mIsDone) return true;
                    return false;
                }
            }

            public long ContentLength { get; private set; }
            public long DownLoadedLength { get; private set; }
            //public long DownLoadedPart { get; private set; }

            private bool mIsClear = false;
            private bool mIsDone = false;

            private bool mIsStart = false;
            private bool mThreadRuning = false;

            private HttpWebRequest mReqest;
            private WebResponse mResponse;
            private Stream mHttpStream;

            private Thread mDownLoadThread = null;

            #endregion
            #region 构造析构
            public DownLoadObject(string _sourceurl, string _destination, bool _clear)
            {
                SourceURL = _sourceurl;
                DestinationPath = _destination;
                mIsClear = _clear;

                Error = null;

                string[] turlstrs = SourceURL.Split('/');
                FileName = turlstrs[turlstrs.Length - 1];

                TempFile = DestinationPath + "/" + FileName + ".temp";
                CompleteFile = DestinationPath + "/" + FileName;
            }

            ~DownLoadObject()
            {
                Dispose(false);
            }

            private bool mDisposed = false;
            public void Dispose()
            {
                Dispose(true);
                System.GC.SuppressFinalize(this);
            }

            private void Dispose(bool _disposing)
            {
                if (mDisposed)
                    return;
                mDisposed = true;

                mThreadRuning = false;

                if (mDownLoadThread != null)
                    mDownLoadThread.Join();

                CloseHttpClient();
            }
            #endregion
            public void StartDownLoadAsync()
            {
                if (mIsStart) return;
                mIsStart = true;

                mThreadRuning = true;
                mDownLoadThread = new Thread(ReadNetByte);
                mDownLoadThread.IsBackground = true;
                mDownLoadThread.Start();
            }

            public void StartDownLoad()
            {
                if (mIsStart) return;
                mIsStart = true;

                mThreadRuning = true;
                ReadNetByte();
            }

            private void ReadNetByte()
            {
                FileStream ttempfile = null;
                try
                {
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
                    mReqest.Timeout = 5000;

                    if (SourceURL.Contains("https://"))
                        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

                    if (thaveindex > 0)
                        mReqest.AddRange((int)thaveindex);

                    int tindex = 0;
                    while(tindex++ < 4 && mThreadRuning)
                    {
                        try
                        {
                            mResponse = mReqest.GetResponse();
                            break;
                        }
                        catch (System.Exception _error)
                        {
                            DLog.LogFormat("ReadNetByte 获取Response失败,尝试次数{0},error = {1}", tindex, _error);
                        }
                    }

                    if (mResponse == null)
                        throw new System.NullReferenceException("ReadNetByte 获取Response失败.");
                    
                    mHttpStream = mResponse.GetResponseStream();
                    ContentLength = mResponse.ContentLength;

                    if (ttempfile == null)
                        ttempfile = System.IO.File.Create(TempFile);

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

                        if (++tcount>= 512)
                        {
                            ttempfile.Flush();
                            tcount = 0;
                        }
                        
                    }
                }
                catch(System.Exception _error)
                {
                    Error = _error.ToString();
                }

                if (ttempfile != null)
                    ttempfile.Close();

                if (DownLoadedLength == ContentLength)
                {
                    if (File.Exists(TempFile))
                    {
                        if (File.Exists(CompleteFile))
                        {
                            File.Delete(CompleteFile);
                        }
                        File.Move(TempFile, CompleteFile);
                    }
                }
                else
                {
                    if(Error == null)
                        Error = "文件未能完成下载.Stream 被中断.";
                }

                CloseHttpClient();
                mIsDone = true;
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
                    mHttpStream.Dispose();
                    mHttpStream = null;
                }

                if(mResponse != null)
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
        }
    }
}

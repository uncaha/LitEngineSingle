using UnityEngine;
using System.Threading;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Text;
using System;
namespace LitEngine
{
    namespace UnZip
    {
        public class UnZipObject : System.Collections.IEnumerator, System.IDisposable
        {
            public static int CodePage = Encoding.Default.CodePage;
            public float Progress { get; private set; }
            public string CurFile { get; private set; }
            public bool IsDone { get; private set; }
            public bool IsStart { get; private set; }
            public string Error { get; private set; }

            private string mDestinationFilePath = null;
            private Stream mStream = null;
            private Thread mAsyncThread = null;
            private bool mThreadRuning = false;

            #region 构造.释放
            public UnZipObject(string _source, string _destination)
            {
                Init(File.OpenRead(_source), _destination);
            }

            public UnZipObject(Stream _stream, string _destination)
            {
                Init(_stream, _destination);
            }

            private void Init(Stream _stream, string _destination)
            {
                mStream = _stream;
                mDestinationFilePath = _destination;
                IsDone = false;
                Error = null;
                ZipConstants.DefaultCodePage = CodePage;
            }

            ~UnZipObject()
            {
                Dispose(false);
            }

            bool mDisposed = false;
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
                if (mAsyncThread != null)
                {
                    mAsyncThread.Join();
                    mAsyncThread = null;
                }

                if (mStream != null)
                {
                    mStream.Close();
                    mStream.Dispose();
                    IsDone = true;
                } 
            }

            #endregion

            public object Current { get; }

            public bool MoveNext()
            {
                return !IsDone;
            }
            public void Reset()
            {

            }

            public void StartUnZip()
            {
                if (IsStart) return;
                IsStart = true;
                mThreadRuning = true;
                StartUnZipFileByStream();
            }

            public void StartUnZipAsync()
            {
                if (IsStart) return;
                IsStart = true;
                mThreadRuning = true;
                mAsyncThread = new Thread(StartUnZipFileByStream);
                mAsyncThread.IsBackground = true;
                mAsyncThread.Start();
            }

            private void StartUnZipFileByStream()
            {
                if (mStream == null)
                {
                    Error = "Stream = null";
                    IsDone = true;
                    return;
                }
                
                try
                {
                    using (ZipInputStream f = new ZipInputStream(mStream))
                    {
                        long tunziplen = 0;
                        long tFileLen = mStream.Length;
                        string un_dir = mDestinationFilePath;

                        ZipEntry zp = f.GetNextEntry();

                        int tcachelen = 2048;

                        byte[] tcachebuffer = new byte[tcachelen];  //每次缓冲 2048 字节
                        while (zp != null && mThreadRuning)
                        {
                            CurFile = zp.Name;
                            tunziplen += zp.CompressedSize;
                            Progress = (float)tunziplen / tFileLen;

                            string un_tmp2;
                            if (zp.Name.IndexOf("/") >= 0)
                            {
                                int tmp1 = zp.Name.LastIndexOf("/");
                                un_tmp2 = zp.Name.Substring(0, tmp1);
                                if (!Directory.Exists(un_dir + un_tmp2))
                                {
                                    Directory.CreateDirectory(un_dir + un_tmp2);
                                }
                            }
                            if (!zp.IsDirectory && zp.Crc != 00000000L) //此“ZipEntry”不是“标记文件”
                            {
                                string tnewfile = un_dir + "/" + zp.Name;

                                if (File.Exists(tnewfile))
                                {
                                    File.Delete(tnewfile);
                                }
                                using (FileStream ts = File.Create(tnewfile))
                                {
                                    int treadlen = 0;
                                    while (true && mThreadRuning) //持续读取字节，直到一个“ZipEntry”字节读完
                                    {
                                        treadlen = f.Read(tcachebuffer, 0, tcachebuffer.Length); //读取“ZipEntry”中的字节
                                        if (treadlen > 0)
                                        {
                                            ts.Write(tcachebuffer, 0, treadlen); //将字节写入新建的文件流

                                        }
                                        else
                                        {
                                            break; //读取的字节为 0 ，跳出循环
                                        }
                                    }

                                    ts.Flush();
                                    ts.Close();
                                }

                            }
                            zp = f.GetNextEntry();
                        }
                        f.Close();
                    }
                }
                catch (System.Exception _error)
                {
                    Error = _error.ToString();
                }

                Progress = 1;
                mStream.Close();
                mStream.Dispose();
                mStream = null;
                IsDone = true;

            }
        }
    }
    
}




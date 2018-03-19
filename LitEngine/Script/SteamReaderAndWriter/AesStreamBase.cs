using System.Security.Cryptography;
using System.Text;
using System.IO;
namespace LitEngine
{
    namespace IO
    {
        public class AesStreamBase : System.IDisposable
        {
            public static string AESKey = "fjeicl458c81k53mc7ckd823ng5bcr32";
            public const string AesTag = "LitEngineAes";
            protected const int SafeByteLen = 100;
            protected bool mClosed = false;         
            protected RijndaelManaged mRijindael = null;
            protected Stream mStream = null;
            protected CryptoStream mCrypto = null;
            public AesStreamBase()
            {
            }
            ~AesStreamBase()
            {
                Dispose(false);
            }

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
                DisposeStream();
            }

            static protected RijndaelManaged GetRijndael()
            {
                if (AESKey.Length != 32)
                    DLog.Log("AESKey's Length != 32");
                RijndaelManaged ret = new RijndaelManaged();
                byte[] keyArray = Encoding.UTF8.GetBytes(AESKey);
                ret.Key = keyArray;
                ret.Mode = CipherMode.ECB;
                ret.Padding = PaddingMode.PKCS7;
                return ret;
            }

            virtual protected void DisposeStream()
            {
                Close();
                if (mCrypto != null)
                    mCrypto.Dispose();
                if (mStream != null)
                    mStream.Dispose();
                if (mRijindael != null)
                    mRijindael.Clear();
            }
            virtual public void Close()
            {
                if(mCrypto != null)
                    mCrypto.Close();
                if (mStream != null)
                    mStream.Close();
            }

            virtual public void Flush()
            {
                if (mCrypto != null)
                    mCrypto.Flush();
                if (mStream != null)
                    mStream.Flush();
            }

            #region AES
            public static byte[] Encrypt(byte[] _encryptbytes)
            {
                RijndaelManaged tdel = GetRijndael();
                ICryptoTransform tcryptoTransform = tdel.CreateEncryptor();
                byte[] ret =  tcryptoTransform.TransformFinalBlock(_encryptbytes, 0, _encryptbytes.Length);
                tcryptoTransform.Dispose();
                tdel.Clear();
                return ret;
            }

            public static byte[] Decrypt(byte[] _decryptbytes)
            {
                RijndaelManaged tdel = GetRijndael();
                ICryptoTransform tcryptoTransform = tdel.CreateDecryptor();
                byte[] ret = tcryptoTransform.TransformFinalBlock(_decryptbytes, 0, _decryptbytes.Length);
                tcryptoTransform.Dispose();
                tdel.Clear();
                return ret;
            }

            static int sOnelen = 102400;
            public static void EnCryptFile(string sourceFileName)
            {
                string tempfilename = sourceFileName + ".temp";

                AESReader tsourcefile = new AESReader(sourceFileName);
                if (tsourcefile.IsEncrypt)
                {
                    DLog.LogError("尝试对已经加密的文件进行2次加密.sourceFileName = "+ sourceFileName);
                    tsourcefile.Dispose();
                    return;
                }
                AESWriter twriter = new AESWriter(tempfilename);

                bool tisfinised = false;
                try
                {
                    long tlength = tsourcefile.Length;
                    byte[] tbuffer = new byte[sOnelen];
                    long readedsize = 0;
                    int treadsize = tsourcefile.Read(tbuffer, 0, sOnelen);
                    while (treadsize > 0)
                    {
                        readedsize += treadsize;
                        twriter.WriteBytesWhere(tbuffer, 0, treadsize);
                        twriter.Flush();

                        treadsize = tsourcefile.Read(tbuffer, 0, sOnelen);
                    }
                    tisfinised = true;
                }
                catch (System.Exception _error)
                {
                    DLog.LogError(_error);
                    File.Delete(tempfilename);
                }

                tsourcefile.Close();
                tsourcefile.Dispose();
                twriter.Close();
                twriter.Dispose();

                if(tisfinised)
                {
                    File.Delete(sourceFileName);
                    File.Move(tempfilename, sourceFileName);
                }
               

            }

            public static void DeCryptFile(string sourceFileName)
            {
                string tempfilename = sourceFileName + ".temp";

                AESReader treader = new AESReader(sourceFileName);
                if(!treader.IsEncrypt)
                {
                    treader.Dispose();
                    DLog.LogError("试图解密一个非加密文件或文件无法解密.");
                    return;
                }
                FileStream tfilesource = File.Create(tempfilename);

                bool tisfinished = false;
                try
                {
                    byte[] tbuffer = new byte[sOnelen];
                    long tlength = treader.Length;
                    long readedsize = 0;
                    int treadsize = treader.Read(tbuffer, 0, sOnelen);
                    while (treadsize > 0)
                    {
                        readedsize += treadsize;

                        int twritesize = treadsize;
                        if (readedsize + SafeByteLen >= tlength)
                            twritesize -= SafeByteLen;

                        tfilesource.Write(tbuffer, 0, twritesize);
                        tfilesource.Flush();

                        treadsize = treader.Read(tbuffer, 0, sOnelen);
                    }
                    tisfinished = true;
                }
                catch(System.Exception _error)
                {
                    DLog.LogError(_error);
                    File.Delete(tempfilename);
                }
                treader.Close();
                treader.Dispose();

                tfilesource.Flush();
                tfilesource.Close();
                tfilesource.Dispose();

                if(tisfinished)
                {
                    File.Delete(sourceFileName);
                    File.Move(tempfilename, sourceFileName);
                }
            }
            #endregion

        }
    }
   
}

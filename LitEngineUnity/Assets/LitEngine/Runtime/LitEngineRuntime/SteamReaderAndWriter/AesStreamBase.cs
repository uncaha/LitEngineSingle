using System.Security.Cryptography;
using System.Text;
using System.IO;
namespace LitEngine.IO
{
    public class AesStreamBase : System.IDisposable
    {
        protected static string AESKey = "fjeicl458c81k53mc7ckd823ng5bKHKchkhdkaa*(&*%$%fdf&(KHHJKHDDdfdsaf4534&*^*(^cr32";
        protected const string AesTag = "LitEngineAes";
        protected const int SafeByteLen = 100;
        protected bool mClosed = false;
        protected string mFileName = null;
        protected byte[] mBuffer = null;
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


        virtual protected void DisposeStream()
        {
            Close();
        }
        virtual public void Close()
        {
        }

        virtual public void Flush()
        {
        }

        static public void EncryptAndUncrypt(byte[] _value, int _offset, long _size)
        {
            byte[] tSecret = Encoding.UTF8.GetBytes(AESKey);
            int j = 0;
            for (int i = _offset; i < _size; i++)
            {
                _value[i] = (byte)(_value[i] ^ tSecret[j]);
                j++;
                if (j >= (tSecret.Length - 1))
                    j = 0;
            }
        }

        #region AES
        public static void EnCryptFile(string sourceFileName)
        {
            string tempfilename = sourceFileName + ".temp";

            AESReader tsourcefile = new AESReader(sourceFileName);
            if (tsourcefile.IsEncrypt)
            {
                DLog.LogError("尝试对已经加密的文件进行2次加密.sourceFileName = " + sourceFileName);
                tsourcefile.Dispose();
                return;
            }
            byte[] tallbytes = tsourcefile.ReadAllBytes();
            tsourcefile.Dispose();

            AESWriter twriter = new AESWriter(tempfilename);
            twriter.WriteBytes(tallbytes);
            twriter.Close();


            File.Delete(sourceFileName);
            File.Move(tempfilename, sourceFileName);
        }

        public static void DeCryptFile(string sourceFileName)
        {
            string tempfilename = sourceFileName + ".temp";

            AESReader treader = new AESReader(sourceFileName);
            if (!treader.IsEncrypt)
            {
                treader.Dispose();
                DLog.LogError("试图解密一个非加密文件或文件无法解密.");
                return;
            }
            byte[] tallbytes = treader.ReadAllBytes();
            treader.Dispose();
            File.WriteAllBytes(tempfilename, tallbytes);

            File.Delete(sourceFileName);
            File.Move(tempfilename, sourceFileName);
        }
        #endregion

    }

}

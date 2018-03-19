using System.Security.Cryptography;
using System.IO;
namespace LitEngine
{
    namespace IO
    {
        public class AESReader : AesStreamBase
        {
            public bool IsEncrypt { get; private set; }
            public long Length { get; private set; }
            private BinaryReader mReaderStream = null;
            private string mFileName = null;
            private byte[] mBuffer = null;
            public AESReader(string _filename)
            {
                if (!File.Exists(_filename)) throw new System.NullReferenceException(_filename + "Can not found.");
                mFileName = _filename;
                mStream = File.OpenRead(mFileName);
                Init();
            }

            public AESReader(byte[] _bytes)
            {
                if (_bytes.Length >= int.MaxValue) throw new System.IndexOutOfRangeException("_bytes长度大于 2147483647.");
                mBuffer = _bytes;
                mStream = new MemoryStream(mBuffer);
                Init();
            }

            protected void Init()
            {
                mRijindael = GetRijndael();
                ICryptoTransform cTransform = mRijindael.CreateDecryptor();
                mCrypto = new CryptoStream(mStream, cTransform, CryptoStreamMode.Read);
                mReaderStream = new BinaryReader(mCrypto);

                byte[] tbytes = null;
                try
                {
                    tbytes = ReadBytes(AesTag.Length);
                }
                catch (System.Exception _error)
                {
                    IsEncrypt = false;
                }
                if (tbytes != null)
                {
                    string ttag = System.Text.Encoding.UTF8.GetString(tbytes);
                    IsEncrypt = AesTag.Equals(ttag);
                }
                if (!IsEncrypt)
                {
                    #region rest stream
                    mReaderStream.Close();
                    mCrypto.Close();
                    mStream.Close();

                    mCrypto.Dispose();
                    mStream.Dispose();
                    mRijindael.Clear();
                    mReaderStream = null;
                    mRijindael = null;
                    mCrypto = null;
                    mStream = null;

                    if (!string.IsNullOrEmpty(mFileName))
                        mStream = File.OpenRead(mFileName);
                    else
                        mStream = new MemoryStream(mBuffer);
                    mReaderStream = new BinaryReader(mStream);
                    #endregion

                    Length = mStream.Length;
                }
                else
                    Length = mStream.Length - AesTag.Length;
            }

            public override void Close()
            {
                if (mClosed) return;
                mClosed = true;
                if(mReaderStream != null)
                    mReaderStream.Close();
                base.Close();
            }

            #region 读取
            public virtual byte[] ReadAllBytes()
            {
                if (IsEncrypt)
                {
                    long pos = mStream.Position;
                    byte[] tbytes = ReadBytes((int)Length);
                    mStream.Seek(pos, SeekOrigin.Current);

                    byte[] ret = new byte[tbytes.Length - SafeByteLen];
                    System.Array.Copy(tbytes, 0, ret, 0, tbytes.Length - SafeByteLen);
                    return ret;
                }
                else
                {
                    if (mBuffer != null) return mBuffer;
                    byte[] ret = new byte[mStream.Length];

                    DLog.Log(mStream.Length);
                    long pos = mStream.Position;
                    mStream.Seek(0, SeekOrigin.Begin);
                    mStream.Read(ret, 0, (int)mStream.Length);
                    mStream.Seek(pos, SeekOrigin.Current);
                    return ret;
                }
                
            }

            public virtual bool ReadBoolean()
            {
                return mReaderStream.ReadBoolean();
            }
            public virtual byte ReadByte()
            {
                return mReaderStream.ReadByte();
            }

            public virtual int Read(byte[] buffer, int index, int count)
            {
                return mReaderStream.Read(buffer, index, count);
            }

            public virtual byte[] ReadBytes(int count)
            {
                return mReaderStream.ReadBytes(count);
            }

            public virtual char ReadChar()
            {
                return mReaderStream.ReadChar();
            }

            public virtual char[] ReadChars(int count)
            {
                return mReaderStream.ReadChars(count);
            }

            public virtual decimal ReadDecimal()
            {
                return mReaderStream.ReadDecimal();
            }

            public virtual double ReadDouble()
            {
                return mReaderStream.ReadDouble();
            }

            public virtual short ReadInt16()
            {
                return mReaderStream.ReadInt16();
            }

            public virtual int ReadInt32()
            {
                return mReaderStream.ReadInt32();
            }

            public virtual long ReadInt64()
            {
                return mReaderStream.ReadInt64();
            }

            public virtual sbyte ReadSByte()
            {
                return mReaderStream.ReadSByte();
            }

            public virtual float ReadSingle()
            {
                return mReaderStream.ReadSingle();
            }

            public virtual string ReadString()
            {
                return mReaderStream.ReadString();
            }

            public virtual ushort ReadUInt16()
            {
                return mReaderStream.ReadUInt16();
            }

            public virtual uint ReadUInt32()
            {
                return mReaderStream.ReadUInt32();
            }

            public virtual ulong ReadUInt64()
            {
                return mReaderStream.ReadUInt64();
            }
            #endregion
        }
    }  
}

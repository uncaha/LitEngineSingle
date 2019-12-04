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
            
            public AESReader(string _filename)
            {
                if (!File.Exists(_filename)) throw new System.NullReferenceException(_filename + "Can not found.");
                mFileName = _filename;
                mBuffer = File.ReadAllBytes(mFileName);
                Init();
            }

            public AESReader(byte[] _bytes)
            {
                if (_bytes.Length >= int.MaxValue) throw new System.IndexOutOfRangeException("_bytes长度大于 2147483647.");
                mBuffer = _bytes;
                Init();
            }

            protected void Init()
            {
                if (mBuffer.Length >= AesTag.Length)
                {
                    byte[] tbytes = System.Text.Encoding.UTF8.GetBytes(AesTag);

                    IsEncrypt = true;
                    int tcount = tbytes.Length;
                    for (int i = 0;i< tcount;i++)
                    {
                        if(tbytes[i] != mBuffer[i])
                        {
                            IsEncrypt = false;
                            break;
                        }
                    }
                }
                
                if (!IsEncrypt)
                {
                    mReaderStream = new BinaryReader(new MemoryStream(mBuffer));
                    Length = mBuffer.Length;
                }
                else
                {
                    Length = mBuffer.Length - AesTag.Length - SafeByteLen;
                    EncryptAndUncrypt(mBuffer, AesTag.Length, Length + SafeByteLen);
                    mReaderStream = new BinaryReader(new MemoryStream(mBuffer));
                    mReaderStream.BaseStream.Seek(AesTag.Length, SeekOrigin.Current);
                }
                   
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
                    byte[] ret = new byte[Length];
                    System.Array.Copy(mBuffer, AesTag.Length, ret, 0, Length);
                    return ret;
                }
                else
                    return mBuffer;
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
                int tlen = ReadInt32();
                if (tlen == 0)
                {
                    return null;
                }
                else
                {
                    byte[] tbytes = ReadBytes(tlen);
                    return System.Text.UTF8Encoding.UTF8.GetString(tbytes);
                }
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

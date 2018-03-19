using System.Security.Cryptography;
using System.IO;
namespace LitEngine
{
    namespace IO
    {
        public class AESWriter : AesStreamBase
        {
            protected BinaryWriter mWriterStream = null;

            public AESWriter(string _filename)
            {
                mStream = File.Create(_filename);
                Init();
            }

            public AESWriter(Stream _stream)
            {
                mStream = _stream;
                Init();
            }

            protected void Init()
            {
                mRijindael = GetRijndael();
                ICryptoTransform cTransform = mRijindael.CreateEncryptor();
                mCrypto = new CryptoStream(mStream, cTransform, CryptoStreamMode.Write);
                mWriterStream = new BinaryWriter(mCrypto);
                WriteBytes(System.Text.Encoding.UTF8.GetBytes(AesTag));
            }

            override public void Close()
            {
                if (mClosed) return;
                mClosed = true;

                //加密字串尾端偶尔会解析错误.尾端填入100 btys 可防止
                if(SafeByteLen > 0)
                    WriteBytes(new byte[SafeByteLen]);

                Flush();
                if (mWriterStream != null)
                    mWriterStream.Close();
                base.Close();
            }
            override public void Flush()
            {
                if (mWriterStream != null)
                    mWriterStream.Flush();
                base.Flush();
            }

            #region Write
            public virtual long Seek(int offset, SeekOrigin origin)
            {
                return mWriterStream.Seek(offset, origin);
            }

            public virtual void WriteDouble(double value)
            {
                mWriterStream.Write(value);
            }
            public virtual void WriteString(string value)
            {
                mWriterStream.Write(value);
            }
            public virtual void WriteFloat(float value)
            {
                mWriterStream.Write(value);
            }
            public virtual void WriteULong(ulong value)
            {
                mWriterStream.Write(value);
            }
            public virtual void WriteLong(long value)
            {
                mWriterStream.Write(value);
            }
            public virtual void WriteUInt(uint value)
            {
                mWriterStream.Write(value);
            }
            public virtual void WriteInt(int value)
            {
                mWriterStream.Write(value);
            }
            public virtual void WriteUShort(ushort value)
            {
                mWriterStream.Write(value);
            }
            public virtual void WriteShort(short value)
            {
                mWriterStream.Write(value);
            }
            public virtual void WriteChar(char ch)
            {
                mWriterStream.Write(ch);
            }
            public virtual void WriteCharsWhere(char[] chars, int index, int count)
            {
                mWriterStream.Write(chars, index, count);
            }
            public virtual void WriteChars(char[] chars)
            {
                mWriterStream.Write(chars);
            }
            public virtual void WriteBytesWhere(byte[] buffer, int index, int count)
            {
                mWriterStream.Write(buffer, index, count);
            }
            public virtual void WriteBytes(byte[] buffer)
            {
                mWriterStream.Write(buffer);
            }
            public virtual void WriteSByte(sbyte value)
            {
                mWriterStream.Write(value);
            }
            public virtual void WriteByte(byte value)
            {
                mWriterStream.Write(value);
            }
            public virtual void WriteBool(bool value)
            {
                mWriterStream.Write(value);
            }
            public virtual void WriteDecimal(decimal value)
            {
                mWriterStream.Write(value);
            }
            #endregion
        }
    }
}
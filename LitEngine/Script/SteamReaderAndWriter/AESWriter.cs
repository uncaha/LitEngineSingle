using System.Security.Cryptography;
using System.IO;
namespace LitEngine
{
    namespace IO
    {
        public class AESWriter : AesStreamBase
        {
            protected BinaryWriter mWriterStream = null;
            protected MemoryStream mMemorystream = null;
            public AESWriter(string _filename)
            {
                mFileName = _filename;
                Init();
            }
            protected void Init()
            {
                mMemorystream = new MemoryStream();
                mWriterStream = new BinaryWriter(mMemorystream);
            }

            override public void Close()
            {
                if (mClosed) return;
                mClosed = true;
                Flush();
                if (mWriterStream != null)
                    mWriterStream.Close();

                BinaryWriter twriter = new BinaryWriter(File.OpenWrite(mFileName));
                twriter.Write(System.Text.Encoding.UTF8.GetBytes(AesTag));
                twriter.Write(mBuffer);
                twriter.Flush();
                twriter.Close();
                base.Close();
            }
            override public void Flush()
            {
                long tlen = mMemorystream.Length;
                mBuffer = new byte[tlen + SafeByteLen];
                System.Array.Copy(mMemorystream.GetBuffer(), 0, mBuffer,0, tlen);
                EncryptAndUncrypt(mBuffer, 0, mBuffer.Length);
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
                if (string.IsNullOrEmpty(value))
                {
                    WriteInt(0);
                }
                else
                {
                    byte[] tbytes = System.Text.UTF8Encoding.UTF8.GetBytes(value);
                    WriteInt(tbytes.Length);
                    WriteBytes(tbytes);
                }

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
using System;
using System.Text;
using System.Runtime.InteropServices;
namespace LitEngine.Net
{
    public class BufferBase
    {
        public const int maxLen = 1024 * 1024 * 100;
        static public bool IsHDate = false;
        static public DataHead headInfo = new SocketDataHead<int, int>(false);

        private byte[] mBuffer = null;
        private int mIndex = 0;
        private int mPos = 0;
        private int mSize = 0;

        public BufferBase(int _bufferlen)
        {
            mSize = _bufferlen;
            mBuffer = new byte[mSize];
        }

        #region 缓存处理
        public void Clear()
        {
            mPos = 0;
            mIndex = 0;
        }
        void CalculationPop(int pLen)
        {
            if (mPos > 0 && (mSize - mIndex) < pLen)
            {
                int tlen = mIndex - mPos;
                if (tlen > 0)
                {
                    Pop();
                }
            }
        }
        void ExpansionBuffer(int pLen)
        {
            if ((mSize - mIndex) < pLen)
            {
                mSize = mSize + pLen + 1024 * 10;
                byte[] tbuffer = new byte[mSize];
                Buffer.BlockCopy(mBuffer, 0, tbuffer, 0, mIndex);
                mBuffer = tbuffer;
            }
        }
        public void Push(byte[] pbuffer, int pLen)
        {
            CalculationPop(pLen);
            ExpansionBuffer(pLen);

            Buffer.BlockCopy(pbuffer, 0, mBuffer, mIndex, pLen);
            mIndex += pLen;
        }
        public void Pop()
        {
            if (mPos == 0) return;
            int tlen = mIndex - mPos;
            if (tlen > 0)
            {
                IntPtr tcopyptr = Marshal.AllocHGlobal(tlen);
                Marshal.Copy(mBuffer, mPos, tcopyptr, tlen);
                Marshal.Copy(tcopyptr, mBuffer, 0, tlen);
                Marshal.FreeHGlobal(tcopyptr);
            }
            mPos = 0;
            mIndex = tlen;
        }
        public bool IsFullData()
        {
            if (mIndex - mPos < headInfo.packageHeadLen) return false;
            int tlen = headInfo.ReadHeadLen(mBuffer, mPos);
            if (tlen > maxLen || tlen < 0) throw new System.ArgumentOutOfRangeException("数据长度超出了限制 len = " + tlen);
            if (mIndex - mPos < tlen) return false;
            return true;
        }

        public int GetFirstDataLength()
        {
            return SReadInt(mBuffer, mPos);
        }

        public ReceiveData GetReceiveData()
        {
            ReceiveData ret = new ReceiveData();
            SetReceiveData(ret);
            return ret;
        }

        public void SetReceiveData(ReceiveData _data)
        {
            int tlen = SReadInt(mBuffer, mPos);//外部读取,保证独立性
            _data.CopyBuffer(mBuffer, mPos);
            mPos += tlen;
        }
        #endregion


        #region 读取工具
        unsafe public static void GetNetValue(byte* pdata, byte[] _buffer, int _startindex, int _length)
        {
            if (_startindex + _length - 1 >= _buffer.Length)
            {
                DLog.LogError("GetNetValue数组越界");
                return;
            }
            if (IsHDate)
            {
                int i = _startindex + _length - 1;
                for (; i >= _startindex; i--)
                    *pdata++ = _buffer[i];
            }
            else
            {
                int imax = _startindex + _length;
                for (int i = _startindex; i < imax; i++)
                    *pdata++ = _buffer[i];
            }

        }
        public static byte SReadByte(byte[] _buffer, int _startindex)
        {
            return _buffer[_startindex];
        }
        public static byte[] SReadBytes(byte[] _buffer, int _startindex, int _count)
        {
            byte[] ret = new byte[_count];
            Array.Copy(_buffer, _startindex, ret, 0, _count);
            return ret;
        }

        unsafe public static short SReadShort(byte[] _buffer, int _startindex)
        {
            short u = 0;
            GetNetValue((byte*)&u, _buffer, _startindex, sizeof(short));
            return u;
        }

        unsafe public static ushort SReadUShort(byte[] _buffer, int _startindex)
        {
            ushort u = 0;
            GetNetValue((byte*)&u, _buffer, _startindex, sizeof(ushort));
            return u;
        }

        unsafe public static int SReadInt(byte[] _buffer, int _startindex)
        {
            int u = 0;
            GetNetValue((byte*)&u, _buffer, _startindex, sizeof(int));
            return u;
        }

        unsafe public static uint SReadUInt(byte[] _buffer, int _startindex)
        {
            uint u = 0;
            GetNetValue((byte*)&u, _buffer, _startindex, sizeof(uint));
            return u;
        }

        unsafe public static long SReadLong(byte[] _buffer, int _startindex)
        {

            long u = 0;
            GetNetValue((byte*)&u, _buffer, _startindex, sizeof(long));
            return u;
        }

        unsafe public static float SReadFloat(byte[] _buffer, int _startindex)
        {
            float u = 0;
            GetNetValue((byte*)&u, _buffer, _startindex, sizeof(float));
            return u;
        }

        unsafe public static bool SReadBool(byte[] _buffer, int _startindex)
        {
            bool u = false;
            byte* pdata = (byte*)&u;
            *pdata = _buffer[_startindex];
            return u;
        }

        public static string SReadString(byte[] _buffer, int _startindex)
        {
            ushort len = SReadUShort(_buffer, _startindex);
            _startindex += sizeof(ushort);
            byte[] tarry = SReadBytes(_buffer, _startindex, len);
            return Encoding.UTF8.GetString(tarry);
        }
        #endregion

        #region 写入工具

        #region 写入buffer

        unsafe public static void WriteValue(byte* pdata, int plength,byte[] pDst,int pOffset)
        {
            if (IsHDate)
            {
                int i = pOffset +  plength - 1;
                for (; i >= pOffset; i--)
                    pDst[i] = *pdata++;
            }
            else
            {
                for (int i = 0; i < plength; i++)
                    pDst[i] = *pdata++;
            }
        }

        unsafe public static int WriteToBuffer(byte pSrc, byte[] pDst, int pOffset)
        {
            pDst[pOffset] = pSrc;
            return sizeof(byte);
        }

        unsafe public static int WriteToBuffer(byte[] pSrc, byte[] pDst, int pOffset)
        {
            Buffer.BlockCopy(pSrc, 0, pDst, pOffset, pSrc.Length);
            return pSrc.Length;
        }

        unsafe public static int WriteToBuffer(bool pSrc, byte[] pDst, int pOffset)
        {
            return WriteToBuffer((byte*)&pSrc,sizeof(bool),pDst, pOffset);
        }

        unsafe public static int WriteToBuffer(float pSrc, byte[] pDst, int pOffset)
        {
            return WriteToBuffer((byte*)&pSrc,sizeof(float),pDst, pOffset);
        }

        unsafe public static int WriteToBuffer(int pSrc, byte[] pDst, int pOffset)
        {
            return WriteToBuffer((byte*)&pSrc,sizeof(int),pDst, pOffset);
        }

        unsafe public static int WriteToBuffer(uint pSrc, byte[] pDst, int pOffset)
        {
            return WriteToBuffer((byte*)&pSrc, sizeof(uint), pDst, pOffset);
        }

        unsafe public static int WriteToBuffer(short pSrc, byte[] pDst, int pOffset)
        {
            return WriteToBuffer((byte*)&pSrc,sizeof(short),pDst, pOffset);
        }

        unsafe public static int WriteToBuffer(ushort pSrc, byte[] pDst, int pOffset)
        {
            return WriteToBuffer((byte*)&pSrc, sizeof(ushort), pDst, pOffset);
        }

        unsafe public static int WriteToBuffer(long pSrc, byte[] pDst, int pOffset)
        {
            return WriteToBuffer((byte*)&pSrc, sizeof(long), pDst, pOffset);
        }

        unsafe public static int WriteToBuffer(ulong pSrc, byte[] pDst, int pOffset)
        {
            return WriteToBuffer((byte*)&pSrc, sizeof(ulong), pDst, pOffset);
        }

        unsafe public static int WriteToBuffer(byte* pSrc,int pLen, byte[] pDst, int pOffset)
        {
            WriteValue(pSrc, pLen, pDst, pOffset);
            return pLen;
        }

        unsafe public static int WriteToBuffer(string pSrc, byte[] pDst, int pOffset)
        {
            byte[] strbyte = Encoding.UTF8.GetBytes(pSrc);
            int ttypelen = sizeof(ushort);
            int retlen = strbyte.Length + ttypelen;

            int twritePos = pOffset;
            twritePos += WriteToBuffer((ushort)strbyte.Length, pDst, twritePos);
            twritePos += WriteToBuffer(strbyte, pDst, twritePos);

            return retlen;
        }

        //写入buffer
        #endregion

        #region getbuffer
        unsafe public static byte[] SetNetValue(byte* pdata, int _length)
        {
            byte[] retbuffer = new byte[_length];

            if (IsHDate)
            {
                int i = _length - 1;
                for (; i >= 0; i--)
                    retbuffer[i] = *pdata++;
            }
            else
            {
                for (int i = 0; i < _length; i++)
                    retbuffer[i] = *pdata++;
            }
            return retbuffer;
        }

        unsafe public static byte[] GetBuffer(int _src)
        {
            return SetNetValue((byte*)&_src, sizeof(int));
        }
        unsafe public static byte[] GetBuffer(short _src)
        {
            return SetNetValue((byte*)&_src, sizeof(short));
        }
        unsafe public static byte[] GetBuffer(ushort _src)
        {
            return SetNetValue((byte*)&_src, sizeof(ushort));
        }
        unsafe public static byte[] GetBuffer(long _src)
        {
            return SetNetValue((byte*)&_src, sizeof(long));
        }

        unsafe public static byte[] GetBuffer(float _src)
        {
            return SetNetValue((byte*)&_src, sizeof(float));
        }
        unsafe public static byte GetBuffer(bool _src)
        {
            byte* pdata = (byte*)&_src;
            byte tBuffer = *pdata;
            return tBuffer;
        }

        public static byte[] GetBuffer(string _src)
        {
            byte[] strbyte = Encoding.UTF8.GetBytes(_src);
            byte[] lenbyte = BufferBase.GetBuffer((ushort)strbyte.Length);
            byte[] ret = new byte[strbyte.Length + lenbyte.Length];

            lenbyte.CopyTo(ret, 0);
            if (strbyte.Length > 0)
                strbyte.CopyTo(ret, lenbyte.Length);

            return ret;
        }
        #endregion
        
        //写入
        #endregion
    }
}

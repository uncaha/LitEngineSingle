using System;
using System.Text;
using System.Runtime.InteropServices;
namespace LitEngine
{
    namespace NetTool
    {
        public class BufferBase
        {
            static public bool IsHDate = false;
            static public CodeTool_CS sCodeCS = new CodeTool_CS();//C#类工具
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
            public void Push(byte[] _buffer,int _len)
            {
                if ((mSize - mIndex) < _len)
                {
                    mSize = mSize + _len + 1024;
                    byte[] tbuffer = new byte[mSize];
                    Array.Copy(mBuffer, 0, tbuffer, 0, mIndex);
                    mBuffer = tbuffer;
                }
                Array.Copy(_buffer, 0, mBuffer, mIndex, _len);
                mIndex += _len;
            }
            public void Pop()
            {
                int tlen = mIndex - mPos;
                if(tlen > 0)
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
                if (mIndex - mPos < SocketDataBase.mPackageTopLen) return false;
                short tlen = SReadShort(mBuffer, mPos);
                if (mIndex - mPos - SocketDataBase.mPackageTopLen < tlen) return false;
                return true;    
            }

            public ReceiveData GetReceiveData()
            {
                ReceiveData ret  = new ReceiveData();
                SetReceiveData(ret);
                return ret;
            }

            public void SetReceiveData(ReceiveData _data)
            {
                short tlen = SReadShort(mBuffer, mPos);
                _data.SetBuffer(mBuffer, mPos);
                mPos = tlen + SocketDataBase.mPackageTopLen;
            }
            #endregion

            #region 读取工具
            unsafe public static void GetNetValue(byte* pdata, byte[] _buffer, int _startindex, int _length)
            {
                if (_startindex + _length - 1 >= _buffer.Length)
                {
                    DLog.LogError( "GetNetValue数组越界");
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
                    int imax = _startindex + _length ;
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

            unsafe public static int SReadInt(byte[] _buffer, int _startindex)
            {
                int u = 0;
                GetNetValue((byte*)&u, _buffer, _startindex, sizeof(int));
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
                short len = SReadShort(_buffer, _startindex);
                _startindex += sizeof(short);
                byte[] tarry = SReadBytes(_buffer, _startindex, len);
                return Encoding.UTF8.GetString(tarry);
            }
            #endregion

            #region 写入工具
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
                byte[] lenbyte = BufferBase.GetBuffer((short)strbyte.Length);
                byte[] ret = new byte[strbyte.Length + lenbyte.Length];

                lenbyte.CopyTo(ret, 0);
                if (strbyte.Length > 0)
                    strbyte.CopyTo(ret, lenbyte.Length);

                return ret;
            }
            #endregion
        }
    }
   
}

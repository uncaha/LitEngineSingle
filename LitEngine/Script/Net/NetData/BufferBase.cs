using System;
using System.Text;
using System.Runtime.InteropServices;
namespace LitEngine.Net
{
    public class BufferBase
    {
        public const int maxLen = 1024 * 1024 * 100;
        
        public static bool IsHDate = false;
        public DataFormat headInfo = new SocketDataFormat(4,4,DataFormat.CmdPosType.lenFirst, DataFormat.ByteLenType.allbytes);

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
            int tlen = GetFullDataLen();
            if (tlen > maxLen || tlen < 0) throw new System.ArgumentOutOfRangeException("数据长度超出了限制 len = " + tlen);
            if (mIndex - mPos < tlen) return false;
            return true;
        }

        public int GetFullDataLen()
        {
            int ret = headInfo.ReadHeadLen(mBuffer, mPos);
            ret = headInfo.GetFullDataLen(ret);
            return ret;
        }

        public int GetFirstDataLength()
        {
            int ret = headInfo.ReadHeadLen(mBuffer, mPos);
            return ret;
        }

        public ReceiveData GetReceiveData()
        {
            ReceiveData ret = new ReceiveData(headInfo);
            SetReceiveData(ret);
            return ret;
        }

        public byte[] GetEndSuccessBytes()
        {
            int tindex = mPos;
            var recLen = headInfo.ReadHeadLen(mBuffer, tindex);
            if (recLen > maxLen || recLen < 0) throw new System.ArgumentOutOfRangeException("数据长度超出了限制 len = " + recLen);

            var tdata = new byte[recLen];
            Buffer.BlockCopy(mBuffer, tindex, tdata, 0, recLen);
            return tdata;
        }

        public void SetReceiveData(ReceiveData _data)
        {
            int tlen = GetFullDataLen();
            _data.CopyBuffer(mBuffer, mPos);
            mPos += tlen;
        }
        #endregion
        
    }
}

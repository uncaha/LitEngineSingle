using System;
using UnityEngine;

namespace LitEngine.Net
{
    public class SendData
    {
        byte[] mData;
        int mIndex;
        bool mIsEnd = false;
        #region 属性
        public byte[] Data { get { return GetData(); } }
        public int Len{get;private set;}
        public int SendLen { get { return mIndex; } }
        public int Cmd {get;private set;}
        #endregion

        private DataHead headInfo;
        public SendData(DataHead pInfo,int pCmd,int pSize = 128)
        {
            headInfo = pInfo;
            mData = new byte[pSize];
            Cmd = pCmd;
            Len = 0;
            mIndex = 0;
            mIsEnd = false;

            headInfo.WriteHead(Len,mData, 0);
            headInfo.WriteCmd(Cmd,mData, 0);
            mIndex = headInfo.packageHeadLen;
        }
        public SendData(int pCmd,byte[] pData,int pSize)
        {
            Cmd = pCmd;
            mData = pData;
            mIndex = pSize;
        }
        public void Rest()
        {
            Len = 0;
            mIndex = headInfo.packageHeadLen;
            mIsEnd = false;
        }
        private byte[] GetData()
        {
            lock (this)
            { 
                if (mIsEnd) return mData;
                Len = headInfo.GetContectLenByIndex(mIndex);
                headInfo.WriteHead(Len, mData, 0);
                mIsEnd = true;
                return mData;
            }
        }

        #region　添加数据

        private void ChoseDataLen(int pLen)
        {
            if ((pLen + mIndex) < mData.Length) return;
            int tlen = mData.Length;
            byte[] tdata = new byte[pLen + tlen * 2];
            Buffer.BlockCopy(mData, 0, tdata, 0, tlen);
            mData = tdata;
        }
        public void AddByte(byte pSrc)
        {
            ChoseDataLen(1);
            mData[mIndex] = pSrc;
            mIndex++;
        }

        public void AddBytes(byte[] pSrc)
        {
            if (pSrc == null) return;
            ChoseDataLen(pSrc.Length);
            mIndex += BufferBase.WriteToBuffer(pSrc,mData,mIndex);
        }

        public void AddShort(short pSrc)
        {
            ChoseDataLen(sizeof(short));
            mIndex += BufferBase.WriteToBuffer(pSrc,mData,mIndex);
        }

        public void AddUShort(ushort pSrc)
        {
            ChoseDataLen(sizeof(ushort));
            mIndex += BufferBase.WriteToBuffer(pSrc,mData,mIndex);
        }

        public void AddInt(int pSrc)
        {
            ChoseDataLen(sizeof(int));
            mIndex += BufferBase.WriteToBuffer(pSrc,mData,mIndex);
        }

        public void AddUInt(uint pSrc)
        {
            ChoseDataLen(sizeof(uint));
            mIndex += BufferBase.WriteToBuffer(pSrc,mData,mIndex);
        }

        public void AddULong(ulong pSrc)
        {
            ChoseDataLen(sizeof(ulong));
            mIndex += BufferBase.WriteToBuffer(pSrc,mData,mIndex);
        }

        public void AddLong(long pSrc)
        {
            ChoseDataLen(sizeof(long));
            mIndex += BufferBase.WriteToBuffer(pSrc,mData,mIndex);
        }

        public void AddFloat(float pSrc)
        {
            ChoseDataLen(sizeof(float));
            mIndex += BufferBase.WriteToBuffer(pSrc,mData,mIndex);
        }

        public void AddBool(bool pSrc)
        {
            ChoseDataLen(sizeof(bool));
            mIndex += BufferBase.WriteToBuffer(pSrc,mData,mIndex);
        }

        public void AddString(string pSrc)
        {
            byte[] strbyte = System.Text.Encoding.UTF8.GetBytes(pSrc);
            int ttypelen = sizeof(ushort);
            int retlen = strbyte.Length + ttypelen;
            ChoseDataLen(retlen);
            mIndex += BufferBase.WriteToBuffer((ushort)strbyte.Length,mData,mIndex);
            mIndex += BufferBase.WriteToBuffer(strbyte,mData,mIndex);
        }


        #endregion
        #region 扩展的添加数据
        public void AddVector2(Vector2 _src)
        {
            AddFloat(_src.x);
            AddFloat(_src.y);
        }

        public void AddVector3(Vector3 _src)
        {
            AddFloat(_src.x);
            AddFloat(_src.y);
            AddFloat(_src.z);
        }

        public void AddVector4(Vector4 _src)
        {
            AddFloat(_src.x);
            AddFloat(_src.y);
            AddFloat(_src.z);
            AddFloat(_src.w);
        }

        public void AddQuaternion(Quaternion _src)
        {
            AddFloat(_src.x);
            AddFloat(_src.y);
            AddFloat(_src.z);
            AddFloat(_src.w);
        }
        #endregion


    }
}


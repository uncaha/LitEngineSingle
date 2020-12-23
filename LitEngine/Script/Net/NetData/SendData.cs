using System;
using UnityEngine;
using System.Text;
using LitEngine.CodeTool;
namespace LitEngine.Net
{
    public class SendData
    {
        byte[] mData;
        int mIndex;
        bool mIsEnd;
        #region 属性
        public byte[] Data { get { return GetData(); } }
        public int Len{get;private set;}
        public int SendLen { get { return mIndex; } }
        public int Cmd {get;private set;}
        #endregion
        public SendData(int _cmd)
        {
            mData = new byte[128];
            Cmd = _cmd;
            Len = 0;
            mIndex = 0;
            mIsEnd = false;

            mIndex += BufferBase.headInfo.WriteHead(Len,mData, mIndex);
            mIndex += BufferBase.headInfo.WriteCmd(Cmd,mData, mIndex);
        }
        public void Rest()
        {
            Len = 0;
            mIndex = BufferBase.headInfo.packageHeadLen;
            mIsEnd = false;
        }
        private byte[] GetData()
        {
            lock (this)
            {
                if (mIsEnd) return mData;
                Len = mIndex;
                BufferBase.headInfo.WriteHead(Len, mData, 0);
                mIsEnd = true;
                return mData;
            }
        }

        #region　添加数据

        private void ChoseDataLen(int _len)
        {
            if ((_len + mIndex) < mData.Length) return;
            int tlen = mData.Length;
            byte[] tdata = new byte[_len + tlen * 2];
            Array.Copy(mData, tdata, tlen);
            mData = tdata;
        }
        public void AddByte(byte _src)
        {
            ChoseDataLen(1);
            mData[mIndex] = _src;
            mIndex++;
        }

        public void AddBytes(byte[] _src)
        {
            if (_src == null) return;
            int tlen = _src.Length;
            ChoseDataLen(tlen);
            Array.Copy(_src, 0, mData, mIndex, tlen);
            mIndex += tlen;
        }

        public void AddShort(short _src)
        {
            AddBytes(BufferBase.GetBuffer(_src));
        }

        public void AddInt(int _src)
        {
            AddBytes(BufferBase.GetBuffer(_src));
        }

        public void AddLong(long _src)
        {
            AddBytes(BufferBase.GetBuffer(_src));
        }

        public void AddFloat(float _src)
        {
            AddBytes(BufferBase.GetBuffer(_src));
        }

        public void AddBool(bool _src)
        {
            AddByte(BufferBase.GetBuffer(_src));
        }

        public void AddString(string _src)
        {
            AddBytes(BufferBase.GetBuffer(_src));
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


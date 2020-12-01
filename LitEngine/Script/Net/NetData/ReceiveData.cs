using System;
using UnityEngine;
using System.Text;
using LitEngine.CodeTool;
namespace LitEngine.NetTool
{
    public class ReceiveData
    {
        public int ArrayIndex = 0;
        int mCmd;
        int mLen;

        byte[] mData;

        public bool DataUsed;
        #region 属性
        public byte[] Data
        {
            get
            {
                return mData;
            }
        }
        public int Len
        {
            get
            {
                return mLen;
            }
        }
        public int Cmd
        {
            get
            {
                return mCmd;
            }
        }
        #endregion
        int mIndex;
        object mCSLEObject;
        public ReceiveData(byte[] _buffer, int _offset)
        {
            SetBuffer(_buffer, _offset);
        }
        public ReceiveData(int _len)
        {
            mData = new byte[_len];
        }
        public ReceiveData()
        {
        }

        public void SetBuffer(byte[] _buffer, int _offset)
        {
            DataUsed = false;
            mCSLEObject = null;
            mIndex = 0;
            int tindex = _offset;
            int tlen = BufferBase.SReadInt(_buffer, tindex);
            mLen = tlen - SocketDataBase.mPackageTopLen;
            tindex += sizeof(int);
            mCmd = BufferBase.SReadInt(_buffer, tindex);
            tindex += sizeof(int);
            if (mData == null || mLen > mData.Length)
                mData = new byte[mLen];
            else
                mData.Initialize();

            mIndex = 0;
            Array.Copy(_buffer, tindex, mData, 0, mLen);
        }
        public byte[] GetData()
        {
            return mData;
        }

        #region 读取

        public byte ReadByte()
        {
            return mData[mIndex++];
        }
        public byte[] ReadBytes(int count)
        {
            byte[] ret = BufferBase.SReadBytes(mData, mIndex, count);
            mIndex += count;
            return ret;
        }

        unsafe public short ReadShort()
        {
            short u = 0;
            BufferBase.GetNetValue((byte*)&u, mData, mIndex, sizeof(short));
            mIndex += sizeof(short);
            return u;
        }

        unsafe public int ReadInt()
        {
            int u = 0;
            BufferBase.GetNetValue((byte*)&u, mData, mIndex, sizeof(int));
            mIndex += sizeof(int);
            return u;
        }

        unsafe public long ReadLong()
        {
            long u = 0;
            BufferBase.GetNetValue((byte*)&u, mData, mIndex, sizeof(long));
            mIndex += sizeof(long);
            return u;
        }

        unsafe public float ReadFloat()
        {
            float u = 0;
            BufferBase.GetNetValue((byte*)&u, mData, mIndex, sizeof(float));
            mIndex += sizeof(float);
            return u;
        }

        unsafe public bool ReadBool()
        {
            bool u = false;
            byte* pdata = (byte*)&u;
            *pdata = mData[mIndex++];
            return u;
        }

        public string ReadString()
        {
            short len = ReadShort();
            byte[] tarry = ReadBytes(len);
            return Encoding.UTF8.GetString(tarry);
        }

        #endregion
        #region 扩展的读取数据

        public Vector2 ReadVector2()
        {
            Vector2 ret = new Vector2
            {
                x = ReadFloat(),
                y = ReadFloat()
            };
            return ret;
        }
        public Vector3 ReadVector3()
        {
            Vector3 ret = new Vector3
            {
                x = ReadFloat(),
                y = ReadFloat(),
                z = ReadFloat()
            };
            return ret;
        }

        public Vector4 ReadVector4()
        {
            Vector4 ret = new Vector4
            {
                x = ReadFloat(),
                y = ReadFloat(),
                z = ReadFloat(),
                w = ReadFloat()
            };
            return ret;
        }

        public Quaternion ReadQuaternion()
        {
            Quaternion ret = new Quaternion
            {
                x = ReadFloat(),
                y = ReadFloat(),
                z = ReadFloat(),
                w = ReadFloat()
            };
            return ret;
        }

        #endregion

    }
}


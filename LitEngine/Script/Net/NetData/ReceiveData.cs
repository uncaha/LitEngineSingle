using System;
using UnityEngine;
using System.Text;
using LitEngine.CodeTool;
namespace LitEngine.Net
{
    public class ReceiveData
    {
        #region 属性
        public byte[] Data { get; private set; }
        public int Len { get; private set; }
        public int Cmd { get; private set; }

        #endregion
        int mIndex;
        public ReceiveData(byte[] _buffer, int _offset)
        {
            CopyBuffer(_buffer, _offset);
        }
        public ReceiveData(int _len)
        {
            Data = new byte[_len];
        }
        public ReceiveData()
        {
        }

        public void SetBuffer(byte[] _buffer, int _offset)
        {
            mIndex = 0;
            int tindex = _offset;
            int tlen = BufferBase.SReadInt(_buffer, tindex);

            if (tlen > BufferBase.maxLen || tlen < 0) throw new System.ArgumentOutOfRangeException("数据长度超出了限制 len = " + tlen);

            Len = tlen - SocketDataBase.mPackageTopLen;
            tindex += sizeof(int);
            Cmd = BufferBase.SReadInt(_buffer, tindex);
            tindex += sizeof(int);
            if (Data == null || Len > Data.Length)
                Data = new byte[Len];
            else
                Data.Initialize();

            mIndex = 0;
            Buffer.BlockCopy(_buffer, tindex, Data, 0, Len);
        }

        public void CopyBuffer(byte[] _buffer, int _offset)
        {
            mIndex = 0;
            int tindex = _offset;
            int tlen = BufferBase.SReadInt(_buffer, tindex);

            if (tlen > BufferBase.maxLen || tlen < 0) throw new System.ArgumentOutOfRangeException("数据长度超出了限制 len = " + tlen);

            Len = tlen - SocketDataBase.mPackageTopLen;
            tindex += sizeof(int);
            Cmd = BufferBase.SReadInt(_buffer, tindex);
            tindex += sizeof(int);
            if (Data == null || Len > Data.Length)
                Data = new byte[Len];
            else
                Data.Initialize();

            mIndex = 0;
            Array.Copy(_buffer, tindex, Data, 0, Len);
        }

        #region 读取

        public byte ReadByte()
        {
            return Data[mIndex++];
        }
        public byte[] ReadBytes(int count)
        {
            byte[] ret = BufferBase.SReadBytes(Data, mIndex, count);
            mIndex += count;
            return ret;
        }

        unsafe public short ReadShort()
        {
            short u = 0;
            BufferBase.GetNetValue((byte*)&u, Data, mIndex, sizeof(short));
            mIndex += sizeof(short);
            return u;
        }

        unsafe public int ReadInt()
        {
            int u = 0;
            BufferBase.GetNetValue((byte*)&u, Data, mIndex, sizeof(int));
            mIndex += sizeof(int);
            return u;
        }

        unsafe public long ReadLong()
        {
            long u = 0;
            BufferBase.GetNetValue((byte*)&u, Data, mIndex, sizeof(long));
            mIndex += sizeof(long);
            return u;
        }

        unsafe public float ReadFloat()
        {
            float u = 0;
            BufferBase.GetNetValue((byte*)&u, Data, mIndex, sizeof(float));
            mIndex += sizeof(float);
            return u;
        }

        unsafe public bool ReadBool()
        {
            bool u = false;
            byte* pdata = (byte*)&u;
            *pdata = Data[mIndex++];
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


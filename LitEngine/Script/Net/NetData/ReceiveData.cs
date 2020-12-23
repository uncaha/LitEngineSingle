using System;
using UnityEngine;
using System.Text;
using LitEngine.CodeTool;
namespace LitEngine.Net
{
    public class ReceiveData
    {
        public bool useCache = false;
        #region 属性
        public byte[] Data { get; private set; }
        public int RecLen { get; private set; }
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

        public void CopyBuffer(byte[] _buffer, int _offset)
        {
            mIndex = 0;
            int tindex = _offset;
            RecLen = BufferBase.headInfo.ReadHeadLen(_buffer, tindex);

            if (RecLen > BufferBase.maxLen || RecLen < 0) throw new System.ArgumentOutOfRangeException("数据长度超出了限制 len = " + RecLen);

            Len = RecLen - BufferBase.headInfo.packageHeadLen;
            Cmd = BufferBase.headInfo.ReadCmd(_buffer, tindex);

            tindex += BufferBase.headInfo.packageHeadLen;

            if (Data == null || Len > Data.Length)
                Data = new byte[Len];
            else
                Data.Initialize();

            mIndex = 0;
            Array.Copy(_buffer, tindex, Data, 0, Len);
        }

        override public string ToString()
        {
            System.Text.StringBuilder bufferstr = new System.Text.StringBuilder();
            bufferstr.AppendFormat("length = {0},bytes = ", Len);
            bufferstr.Append("{");
            for (int i = 0; i < Len; i++)
            {
                if (i != 0)
                    bufferstr.Append(",");
                bufferstr.Append(Data[i]);
            }
            bufferstr.Append("}");

            return bufferstr.ToString();
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


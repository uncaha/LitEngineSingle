using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;

namespace LitEngine.Net
{
    public enum SocketDataHeadType
    {
        none = 0,
        type_byte,
        type_int,
        type_short,
    }

    public abstract class DataFormat
    {
        public enum CmdPosType
        {
            none = 0,
            cmdFirst,
            lenFirst,
        }

        public enum ByteLenType
        {
            none = 0,
            allbytes,
            onlyContent,
        }

        public bool IsBigEndian { get; protected set; } = false;

        public SocketDataHeadType lenType { get; protected set; } = SocketDataHeadType.type_int;
        public int lenSize { get; protected set; }

        public SocketDataHeadType cmdType { get; protected set; } = SocketDataHeadType.type_int;
        public int cmdSize { get; protected set; }

        public int packageHeadLen { get; protected set; }

        public CmdPosType cmdPos { get; protected set; } = CmdPosType.lenFirst;
        public ByteLenType byteLenType { get; protected set; } = ByteLenType.allbytes;

        public bool IsCmdFirst
        {
            get { return cmdPos == CmdPosType.cmdFirst; }
        }

        public DataFormat(CmdPosType pCmdPos, ByteLenType pLenType)
        {
            cmdPos = pCmdPos;
            byteLenType = pLenType;
        }

        private int ReadByType(SocketDataHeadType pType, byte[] pBuffer, int pOffset)
        {
            int ret = 0;
            switch (pType)
            {
                case SocketDataHeadType.type_byte:
                    ret = SReadByte(pBuffer, pOffset);
                    break;
                case SocketDataHeadType.type_short:
                    ret = SReadShort(pBuffer, pOffset);
                    break;
                case SocketDataHeadType.type_int:
                    ret = SReadInt(pBuffer, pOffset);
                    break;
                default:

                    break;
            }

            return ret;
        }

        public int ReadHeadLen(byte[] pBuffer, int pOffset)
        {
            int ret = ReadByType(lenType, pBuffer, IsCmdFirst ? pOffset + cmdSize : pOffset);
            return ret;
        }

        public int ReadCmd(byte[] pBuffer, int pOffset)
        {
            int ret = ReadByType(cmdType, pBuffer, IsCmdFirst ? pOffset : pOffset + lenSize);
            return ret;
        }

        public int WriteByType(SocketDataHeadType pType, int pSrc, byte[] pBuffer, int pOffset)
        {
            int ret = 0;
            switch (pType)
            {
                case SocketDataHeadType.type_byte:
                    ret = WriteToBuffer((byte) pSrc, pBuffer, pOffset);
                    break;
                case SocketDataHeadType.type_short:
                    ret = WriteToBuffer((short) pSrc, pBuffer, pOffset);
                    break;
                case SocketDataHeadType.type_int:
                    ret = WriteToBuffer((int) pSrc, pBuffer, pOffset);
                    break;
                default:

                    break;
            }

            return ret;
        }

        public int WriteHead(int pLen, byte[] pBuffer, int pOffset)
        {
            return WriteByType(lenType, pLen, pBuffer, IsCmdFirst ? pOffset + cmdSize : pOffset);
        }

        public int WriteCmd(int pCmd, byte[] pBuffer, int pOffset)
        {
            return WriteByType(cmdType, pCmd, pBuffer, IsCmdFirst ? pOffset : pOffset + lenSize);
        }

        //pSize写入数据的长度
        public int GetContectLenByIndex(int pIndex)
        {
            switch (byteLenType)
            {
                case ByteLenType.allbytes:
                    return pIndex;
                case ByteLenType.onlyContent:
                    return pIndex - packageHeadLen;
                default:
                    return pIndex;
            }
        }

        //pSize接收到的长度
        public int GetContectLenByRecLen(int pSize)
        {
            switch (byteLenType)
            {
                case ByteLenType.allbytes:
                    return pSize - packageHeadLen;
                case ByteLenType.onlyContent:
                    return pSize;
                default:
                    return pSize;
            }
        }

        //pLen接收到的长度
        public int GetFullDataLen(int pLen)
        {
            switch (byteLenType)
            {
                case ByteLenType.allbytes:
                    return pLen;
                case ByteLenType.onlyContent:
                    return pLen + packageHeadLen;
                default:
                    return pLen;
            }
        }


        #region 读取工具

        unsafe public void GetNetValue(byte* pdata, byte[] _buffer, int _startindex, int _length)
        {
            if (_startindex + _length - 1 >= _buffer.Length)
            {
                DLog.LogError("GetNetValue数组越界");
                return;
            }

            if (IsBigEndian)
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

        public byte SReadByte(byte[] _buffer, int _startindex)
        {
            return _buffer[_startindex];
        }

        public byte[] SReadBytes(byte[] _buffer, int _startindex, int _count)
        {
            byte[] ret = new byte[_count];
            Buffer.BlockCopy(_buffer, _startindex, ret, 0, _count);
            return ret;
        }

        unsafe public short SReadShort(byte[] _buffer, int _startindex)
        {
            short u = 0;
            GetNetValue((byte*) &u, _buffer, _startindex, sizeof(short));
            return u;
        }

        unsafe public ushort SReadUShort(byte[] _buffer, int _startindex)
        {
            ushort u = 0;
            GetNetValue((byte*) &u, _buffer, _startindex, sizeof(ushort));
            return u;
        }

        unsafe public int SReadInt(byte[] _buffer, int _startindex)
        {
            int u = 0;
            GetNetValue((byte*) &u, _buffer, _startindex, sizeof(int));
            return u;
        }

        unsafe public uint SReadUInt(byte[] _buffer, int _startindex)
        {
            uint u = 0;
            GetNetValue((byte*) &u, _buffer, _startindex, sizeof(uint));
            return u;
        }

        unsafe public long SReadLong(byte[] _buffer, int _startindex)
        {
            long u = 0;
            GetNetValue((byte*) &u, _buffer, _startindex, sizeof(long));
            return u;
        }

        unsafe public float SReadFloat(byte[] _buffer, int _startindex)
        {
            float u = 0;
            GetNetValue((byte*) &u, _buffer, _startindex, sizeof(float));
            return u;
        }

        unsafe public bool SReadBool(byte[] _buffer, int _startindex)
        {
            bool u = false;
            byte* pdata = (byte*) &u;
            *pdata = _buffer[_startindex];
            return u;
        }

        public string SReadString(byte[] _buffer, int _startindex)
        {
            ushort len = SReadUShort(_buffer, _startindex);
            _startindex += sizeof(ushort);
            byte[] tarry = SReadBytes(_buffer, _startindex, len);
            return Encoding.UTF8.GetString(tarry);
        }

        #endregion

        #region 写入工具

        #region 写入buffer

        unsafe public void WriteValue(byte* pdata, int plength, byte[] pDst, int pOffset)
        {
            if (IsBigEndian)
            {
                int i = pOffset + plength - 1;
                for (; i >= pOffset; i--)
                    pDst[i] = *pdata++;
            }
            else
            {
                for (int i = pOffset, max = pOffset + plength; i < max; i++)
                    pDst[i] = *pdata++;
            }
        }

        unsafe public int WriteToBuffer(byte pSrc, byte[] pDst, int pOffset)
        {
            pDst[pOffset] = pSrc;
            return sizeof(byte);
        }

        unsafe public int WriteToBuffer(byte[] pSrc, byte[] pDst, int pOffset)
        {
            Buffer.BlockCopy(pSrc, 0, pDst, pOffset, pSrc.Length);
            return pSrc.Length;
        }

        unsafe public int WriteToBuffer(bool pSrc, byte[] pDst, int pOffset)
        {
            return WriteToBuffer((byte*) &pSrc, sizeof(bool), pDst, pOffset);
        }

        unsafe public int WriteToBuffer(float pSrc, byte[] pDst, int pOffset)
        {
            return WriteToBuffer((byte*) &pSrc, sizeof(float), pDst, pOffset);
        }

        unsafe public int WriteToBuffer(int pSrc, byte[] pDst, int pOffset)
        {
            return WriteToBuffer((byte*) &pSrc, sizeof(int), pDst, pOffset);
        }

        unsafe public int WriteToBuffer(uint pSrc, byte[] pDst, int pOffset)
        {
            return WriteToBuffer((byte*) &pSrc, sizeof(uint), pDst, pOffset);
        }

        unsafe public int WriteToBuffer(short pSrc, byte[] pDst, int pOffset)
        {
            return WriteToBuffer((byte*) &pSrc, sizeof(short), pDst, pOffset);
        }

        unsafe public int WriteToBuffer(ushort pSrc, byte[] pDst, int pOffset)
        {
            return WriteToBuffer((byte*) &pSrc, sizeof(ushort), pDst, pOffset);
        }

        unsafe public int WriteToBuffer(long pSrc, byte[] pDst, int pOffset)
        {
            return WriteToBuffer((byte*) &pSrc, sizeof(long), pDst, pOffset);
        }

        unsafe public int WriteToBuffer(ulong pSrc, byte[] pDst, int pOffset)
        {
            return WriteToBuffer((byte*) &pSrc, sizeof(ulong), pDst, pOffset);
        }

        unsafe public int WriteToBuffer(byte* pSrc, int pLen, byte[] pDst, int pOffset)
        {
            WriteValue(pSrc, pLen, pDst, pOffset);
            return pLen;
        }

        unsafe public int WriteToBuffer(string pSrc, byte[] pDst, int pOffset)
        {
            byte[] strbyte = Encoding.UTF8.GetBytes(pSrc);
            int ttypelen = sizeof(ushort);
            int retlen = strbyte.Length + ttypelen;

            int twritePos = pOffset;
            twritePos += WriteToBuffer((ushort) strbyte.Length, pDst, twritePos);
            twritePos += WriteToBuffer(strbyte, pDst, twritePos);

            return retlen;
        }

        //写入buffer

        #endregion

        #region getbuffer

        unsafe public byte[] SetNetValue(byte* pdata, int _length)
        {
            byte[] retbuffer = new byte[_length];

            if (IsBigEndian)
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

        unsafe public byte[] GetBuffer(int _src)
        {
            return SetNetValue((byte*) &_src, sizeof(int));
        }

        unsafe public byte[] GetBuffer(short _src)
        {
            return SetNetValue((byte*) &_src, sizeof(short));
        }

        unsafe public byte[] GetBuffer(ushort _src)
        {
            return SetNetValue((byte*) &_src, sizeof(ushort));
        }

        unsafe public byte[] GetBuffer(long _src)
        {
            return SetNetValue((byte*) &_src, sizeof(long));
        }

        unsafe public byte[] GetBuffer(float _src)
        {
            return SetNetValue((byte*) &_src, sizeof(float));
        }

        unsafe public byte GetBuffer(bool _src)
        {
            byte* pdata = (byte*) &_src;
            byte tBuffer = *pdata;
            return tBuffer;
        }

        public byte[] GetBuffer(string _src)
        {
            byte[] strbyte = Encoding.UTF8.GetBytes(_src);
            byte[] lenbyte = GetBuffer((ushort) strbyte.Length);
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

    public class SocketDataFormat : DataFormat
    {
        public SocketDataFormat(int pLen, int pCmdLen, CmdPosType pCmdPos, ByteLenType pLenType,
            bool pIsBigEndian = false) : base(pCmdPos, pLenType)
        {
            lenType = GetTypeAndSize(pLen);
            cmdType = GetTypeAndSize(pCmdLen);

            lenSize = pLen;
            cmdSize = pCmdLen;

            IsBigEndian = pIsBigEndian;

            packageHeadLen = lenSize + cmdSize;
        }

        public SocketDataHeadType GetTypeAndSize(int pLen)
        {
            SocketDataHeadType ret = SocketDataHeadType.none;

            switch (pLen)
            {
                case 1:
                    ret = SocketDataHeadType.type_byte;
                    break;
                case 2:
                    ret = SocketDataHeadType.type_short;
                    break;
                case 4:
                    ret = SocketDataHeadType.type_int;
                    break;
                default:
                    break;
            }

            return ret;
        }
    }
}
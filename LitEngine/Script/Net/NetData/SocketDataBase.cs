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

    public abstract class DataHead
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



        public SocketDataHeadType lenType { get; protected set; } = SocketDataHeadType.type_int;
        public int lenSize { get; protected set; }

        public SocketDataHeadType cmdType { get; protected set; } = SocketDataHeadType.type_int;
        public int cmdSize { get; protected set; }

        public int packageHeadLen { get; protected set; }

        public CmdPosType cmdPos { get; protected set; } = CmdPosType.lenFirst;
        public ByteLenType byteLenType { get; protected set; } = ByteLenType.allbytes;

        public bool IsCmdFirst { get { return cmdPos == CmdPosType.cmdFirst; } }

        public DataHead(CmdPosType pCmdPos, ByteLenType pLenType)
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
                    ret = BufferBase.SReadByte(pBuffer, pOffset);
                    break;
                case SocketDataHeadType.type_short:
                    ret = BufferBase.SReadShort(pBuffer, pOffset);
                    break;
                case SocketDataHeadType.type_int:
                    ret = BufferBase.SReadInt(pBuffer, pOffset);
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
        
        public int WriteByType(SocketDataHeadType pType,int pSrc,byte[] pBuffer, int pOffset)
        {
            int ret = 0;
            switch (pType)
            {
                case SocketDataHeadType.type_byte:
                    ret = BufferBase.WriteToBuffer((byte)pSrc, pBuffer, pOffset);
                    break;
                case SocketDataHeadType.type_short:
                    ret = BufferBase.WriteToBuffer((short)pSrc,pBuffer,pOffset);
                    break;
                case SocketDataHeadType.type_int:
                    ret = BufferBase.WriteToBuffer((int)pSrc,pBuffer,pOffset);
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
                case DataHead.ByteLenType.allbytes:
                    return pIndex;
                case DataHead.ByteLenType.onlyContent:
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
                case DataHead.ByteLenType.allbytes:
                    return pSize - packageHeadLen;
                case DataHead.ByteLenType.onlyContent:
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
                case DataHead.ByteLenType.allbytes:
                    return pLen;
                case DataHead.ByteLenType.onlyContent:
                    return pLen + packageHeadLen;
                default:
                    return pLen;
            }
        }
    }
    public class SocketDataHead : DataHead
    {
        public SocketDataHead(int pLen,int pCmdLen, CmdPosType pCmdPos, ByteLenType pLenType) : base(pCmdPos, pLenType)
        {
            int tlensize, tcmdSize;

            lenType = GetTypeAndSize(pLen);
            cmdType = GetTypeAndSize(pCmdLen);

            lenSize = pLen;
            cmdSize = pCmdLen;

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


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
        type_int,
        type_short,
        type_ushort,
    }

    public class DataHead
    {
        public SocketDataHeadType lenType = SocketDataHeadType.type_int;
        public int lenSize { get; protected set; }

        public SocketDataHeadType cmdType = SocketDataHeadType.type_int;
        public int cmdSize { get; protected set; }

        public int packageHeadLen { get; protected set; }

        private int ReadByType(SocketDataHeadType pType, byte[] pBuffer, int pOffset)
        {
            int ret = -1;
            switch (pType)
            {
                case SocketDataHeadType.type_short:
                    ret = BufferBase.SReadShort(pBuffer, pOffset);
                    break;
                case SocketDataHeadType.type_ushort:
                    ret = BufferBase.SReadUShort(pBuffer, pOffset);
                    break;
                case SocketDataHeadType.type_int:
                    ret = BufferBase.SReadInt(pBuffer, pOffset);
                    break;
                default:
                    DLog.LogErrorFormat("ReadByType -> Type error : {0}", pType);
                    break;
            }

            return ret;
        }

        public int ReadHeadLen(byte[] pBuffer, int pOffset)
        {
            int ret = ReadByType(lenType, pBuffer, pOffset);
            return ret;
        }

        public int ReadCmd(byte[] pBuffer, int pOffset)
        {
            int ret = ReadByType(cmdType, pBuffer, pOffset + lenSize);
            return ret;
        }

        public byte[] GetByte(SocketDataHeadType pType,int pSrc)
        {
            byte[] ret = null;
            switch (pType)
            {
                case SocketDataHeadType.type_short:
                    ret = BufferBase.GetBuffer((short)pSrc);
                    break;
                case SocketDataHeadType.type_ushort:
                    ret = BufferBase.GetBuffer((ushort)pSrc);
                    break;
                case SocketDataHeadType.type_int:
                    ret = BufferBase.GetBuffer((int)pSrc);
                    break;
                default:
                    DLog.LogErrorFormat("SocketData GetByte -> Type error : {0}", pType);
                    break;
            }

            return ret;
        }

        public int WriteByType(SocketDataHeadType pType,int pSrc,byte[] pBuffer, int pOffset)
        {
            int ret = 0;
            switch (pType)
            {
                case SocketDataHeadType.type_short:
                    ret = BufferBase.WriteToBuffer((short)pSrc,pBuffer,pOffset);
                    break;
                case SocketDataHeadType.type_ushort:
                    ret = BufferBase.WriteToBuffer((ushort)pSrc,pBuffer,pOffset);
                    break;
                case SocketDataHeadType.type_int:
                    ret = BufferBase.WriteToBuffer((int)pSrc,pBuffer,pOffset);
                    break;
                default:
                    DLog.LogErrorFormat("SocketData GetByte -> Type error : {0}", pType);
                    break;
            }
            return ret;
        }

        public int WriteHead(int pLen, byte[] pBuffer, int pOffset)
        {
            return WriteByType(lenType,pLen,pBuffer,pOffset);
        }
        public int WriteCmd(int pCmd,byte[] pBuffer, int pOffset)
        {
            return WriteByType(cmdType, pCmd, pBuffer, pOffset + lenSize);
        }
    }
    public class SocketDataHead<T,K> : DataHead
    {
        //T:Len Type    K:cmd Type
        public SocketDataHead()
        {
            int tlensize, tcmdSize;

            lenType = GetTypeAndSize(typeof(T),out tlensize);
            cmdType = GetTypeAndSize(typeof(K),out tcmdSize);

            lenSize = tlensize;
            cmdSize = tcmdSize;

            packageHeadLen = lenSize + cmdSize;
        }

        public SocketDataHeadType GetTypeAndSize(System.Type pType,out int pSize)
        {
            SocketDataHeadType ret = SocketDataHeadType.none;
 
            switch (pType.Name)
            {
                case "Int16":
                    ret = SocketDataHeadType.type_short;
                    pSize = 2;
                    break;
                case "UInt16":
                    ret = SocketDataHeadType.type_ushort;
                    pSize = 2;
                    break;
                case "Int32":
                    ret = SocketDataHeadType.type_int;
                    pSize = 4;
                    break;
                default:
                    pSize = -1;
                    DLog.LogErrorFormat("GetTypeAndSize -> Type error : {0}", pType.Name);
                    break;
            }

            return ret;
        }

       
    }
}


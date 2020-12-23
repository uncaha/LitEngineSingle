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
        type_uint,
        type_int,
        type_short,
        type_ushort,
    }

    public class DataHeadBase
    {
        public SocketDataHeadType type = SocketDataHeadType.type_int;
        public int dataLen = -1;
    }
    public class SocketDataHead<T> : DataHeadBase
    {
        public SocketDataHead()
        {
            System.Type ttype = typeof(T);
            switch (ttype.Name)
            {
                case "Int16":
                    type = SocketDataHeadType.type_short;
                    dataLen = 2;
                    break;
                case "UInt16":
                    type = SocketDataHeadType.type_ushort;
                    dataLen = 2;
                    break;
                case "Int32":
                    type = SocketDataHeadType.type_int;
                    dataLen = 4;
                    break;
                case "UInt32":
                    type = SocketDataHeadType.type_uint;
                    dataLen = 4;
                    break;
                default:
                    break;
            }
        }
    }
    public class SocketDataBase
    {
        
        static public int mFirstLen = 4;//包头长度占位
        static public int mCmdLen = 4;//cmd占位
        static public int mPackageTopLen = mFirstLen + mCmdLen;
    }
}


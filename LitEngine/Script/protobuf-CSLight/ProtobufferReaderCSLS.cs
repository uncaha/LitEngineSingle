using System;
using System.Text;
namespace LitEngine
{
    namespace ProtoCSLS
    {
        public class ProtobufferReaderCSLS
        {
            private int mIndex = 0;
            private byte[] mData = null;
            private int mLen = 0;
            public WireType WType
            {
                get;
                private set;
            }
            public int SelfFieldnumber
            {
                get;
                private set;
            }

            static readonly UTF8Encoding encoding = new UTF8Encoding();

            public ProtobufferReaderCSLS(byte[] _buffer, int _len)
            {
                mData = _buffer;
                mLen = _len;
                SelfFieldnumber = 0;
            }
            public void Seek(int _pos)
            {
                if (_pos > mLen || _pos < 0) return;
                mIndex = _pos;
            }

            #region 类型读取
            public object ReadByDataForward(Type _type, WireType _wt)
            {
                SelfFieldnumber = ReadFieldHeader();
                if (SelfFieldnumber <= 0) return null;
                return ReadByData(_type, _wt);
            }
            public object ReadByData(Type _type, WireType _wt)
            {
                ProtoTypeCode code = PBHelperCSLE.GetTypeCode(_type);
                object ret = null;
                switch (code)
                {
                    case ProtoTypeCode.Int32:
                        ret = ReadInt32(_wt);
                        break;
                    case ProtoTypeCode.UInt32:
                        ret = ReadUInt32(_wt);
                        break;
                    case ProtoTypeCode.Int64:
                        ret = ReadInt64(_wt);
                        break;
                    case ProtoTypeCode.UInt64:
                        ret = ReadUInt64(_wt);
                        break;
                    case ProtoTypeCode.Single:
                        ret = ReadSingle(_wt);
                        break;
                    case ProtoTypeCode.Double:
                        ret = ReadDouble(_wt);
                        break;
                    case ProtoTypeCode.Boolean:
                        ret = ReadBoolean(_wt);
                        break;

                    case ProtoTypeCode.Char:
                    case ProtoTypeCode.Byte:
                        ret = ReadByte(_wt);
                        break;
                    case ProtoTypeCode.SByte:
                        ret = ReadSByte(_wt);
                        break;
                    case ProtoTypeCode.Int16:
                        ret = ReadInt16(_wt);
                        break;
                    case ProtoTypeCode.UInt16:
                        ret = ReadUInt16(_wt);
                        break;
                    case ProtoTypeCode.Guid:
                    case ProtoTypeCode.Uri:
                    case ProtoTypeCode.ByteArray:
                    case ProtoTypeCode.Type:
                    case ProtoTypeCode.TimeSpan:
                    case ProtoTypeCode.Decimal:
                    case ProtoTypeCode.DateTime:
                    case ProtoTypeCode.String:
                        ret = ReadString(_wt);
                        break;

                }
                return ret;
            }
            public string ReadString(WireType _wt)
            {
                if (_wt == WireType.String)
                {
                    int bytes = (int)ReadUInt32Variant(false);
                    if (bytes == 0) return "";
                    if (!CanRead(bytes)) return "";

                    string s = encoding.GetString(mData, mIndex, bytes);
                    mIndex += bytes;
                    return s;
                }
                throw new ArgumentNullException("字符串读取错误");
            }
            public bool ReadBoolean(WireType _wt)
            {
                switch (ReadUInt32(_wt))
                {
                    case 0: return false;
                    case 1: return true;
                    default:
                        throw new InvalidOperationException("错误的读取类型 ReadBoolean:" + _wt);
                }
            }
            public byte ReadByte(WireType _wt)
            {
                checked { return (byte)ReadUInt32(_wt); }
            }
            public sbyte ReadSByte(WireType _wt)
            {
                checked { return (sbyte)ReadInt32(_wt); }
            }
            public float ReadSingle(WireType _wt)
            {
                switch (_wt)
                {
                    case WireType.Fixed32:
                        {
                            int value = ReadInt32(_wt);
                            return BitConverter.ToSingle(BitConverter.GetBytes(value), 0);

                        }
                    case WireType.Fixed64:
                        {
                            double value = ReadDouble(_wt);
                            float f = (float)value;
                            if (PBHelperCSLE.IsInfinity(f)
                                && !PBHelperCSLE.IsInfinity(value))
                            {
                                throw new InvalidOperationException("ReadSingle 读取错误 value:" + value);
                            }
                            return f;
                        }
                    default:
                        throw new InvalidOperationException("错误的读取类型 ReadSingle:" + _wt);
                }
            }
            public double ReadDouble(WireType _wt)
            {
                switch (_wt)
                {
                    case WireType.Fixed32:
                        return ReadSingle(_wt);
                    case WireType.Fixed64:
                        long value = ReadInt64(_wt);
                        return BitConverter.ToDouble(BitConverter.GetBytes(value), 0);
                    default:
                        throw new InvalidOperationException("错误的读取类型 ReadDouble:" + _wt);
                }
            }
            public short ReadInt16(WireType _wt)
            {
                checked { return (short)ReadInt32(_wt); }
            }
            public ushort ReadUInt16(WireType _wt)
            {
                checked { return (ushort)ReadUInt32(_wt); }
            }
            public int ReadInt32(WireType _wt)
            {
                switch (_wt)
                {
                    case WireType.Variant:
                        return (int)ReadUInt32Variant(true);
                    case WireType.Fixed32:
                        if (!CanRead(4)) return 0;
                        return ((int)mData[mIndex++])
                            | (((int)mData[mIndex++]) << 8)
                            | (((int)mData[mIndex++]) << 16)
                            | (((int)mData[mIndex++]) << 24);
                    case WireType.Fixed64:
                        long l = ReadInt64(_wt);
                        checked { return (int)l; }
                    case WireType.SignedVariant:
                        return Zag(ReadUInt32Variant(true));
                    default:
                        throw new InvalidOperationException("错误的读取类型 ReadInt32:" + _wt);
                }
            }
            public uint ReadUInt32(WireType _wt)
            {
                switch (_wt)
                {
                    case WireType.Variant:
                        return ReadUInt32Variant(false);
                    case WireType.Fixed32:
                        if (!CanRead(4)) return 0;
                        return ((uint)mData[mIndex++])
                            | (((uint)mData[mIndex++]) << 8)
                            | (((uint)mData[mIndex++]) << 16)
                            | (((uint)mData[mIndex++]) << 24);
                    case WireType.Fixed64:
                        ulong val = ReadUInt64(_wt);
                        checked { return (uint)val; }
                    default:
                        throw new InvalidOperationException("错误的读取类型 ReadUInt32:" + _wt);
                }
            }
            public long ReadInt64(WireType _wt)
            {
                switch (_wt)
                {
                    case WireType.Variant:
                        return (long)ReadUInt64Variant();
                    case WireType.Fixed32:
                        return ReadInt32(_wt);
                    case WireType.Fixed64:
                        if (!CanRead(8)) return 0;
                        return ((long)mData[mIndex++])
                            | (((long)mData[mIndex++]) << 8)
                            | (((long)mData[mIndex++]) << 16)
                            | (((long)mData[mIndex++]) << 24)
                            | (((long)mData[mIndex++]) << 32)
                            | (((long)mData[mIndex++]) << 40)
                            | (((long)mData[mIndex++]) << 48)
                            | (((long)mData[mIndex++]) << 56);

                    case WireType.SignedVariant:
                        return Zag(ReadUInt64Variant());
                    default:
                        throw new InvalidOperationException("错误的读取类型 ReadInt64:" + _wt);
                }
            }
            public ulong ReadUInt64(WireType _wt)
            {
                switch (_wt)
                {
                    case WireType.Variant:
                        return ReadUInt64Variant();
                    case WireType.Fixed32:
                        return ReadUInt32(_wt);
                    case WireType.Fixed64:
                        if (!CanRead(8)) return 0;

                        return ((ulong)mData[mIndex++])
                            | (((ulong)mData[mIndex++]) << 8)
                            | (((ulong)mData[mIndex++]) << 16)
                            | (((ulong)mData[mIndex++]) << 24)
                            | (((ulong)mData[mIndex++]) << 32)
                            | (((ulong)mData[mIndex++]) << 40)
                            | (((ulong)mData[mIndex++]) << 48)
                            | (((ulong)mData[mIndex++]) << 56);
                    default:
                        throw new InvalidOperationException("错误的读取类型 ReadUInt64:" + _wt);
                }
            }

            private const long Int64Msb = ((long)1) << 63;
            private const int Int32Msb = ((int)1) << 31;
            private static int Zag(uint ziggedValue)
            {
                int value = (int)ziggedValue;
                return (-(value & 0x01)) ^ ((value >> 1) & ~Int32Msb);
            }
            private static long Zag(ulong ziggedValue)
            {
                long value = (long)ziggedValue;
                return (-(value & 0x01L)) ^ ((value >> 1) & ~Int64Msb);
            }

            #endregion
            #region 工具
            public bool CanRead(int _readlen)
            {
                if (mIndex + _readlen - mLen > 0)
                {
                    return false;
                }
                return true;
            }

            #endregion
            #region 读取
            public byte[] ReadBytes(int _len)
            {
                if (!CanRead(_len)) return null;
                byte[] ret = new byte[_len];
                Array.Copy(mData, mIndex, ret, 0, _len);
                mIndex += _len;
                return ret;
            }

            public int ReadFieldHeader()
            {
                uint tag;
                int tFieldnumber = 0;
                if (TryReadUInt32Variant(out tag) && tag != 0)
                {
                    WType = (WireType)(tag & 7);
                    tFieldnumber = (int)(tag >> 3);
                    if (tFieldnumber < 1)
                        throw new InvalidOperationException("PB取得了错误的数据标识 FieldNumber: " + tFieldnumber.ToString());
                }
                else
                {
                    WType = WireType.None;
                    tFieldnumber = 0;
                }
                if (WType == WireType.EndGroup)
                {
                    throw new InvalidOperationException("暂不支持 WireType.EndGroup 类型");
                }
                return tFieldnumber;
            }


            internal uint ReadUInt32Variant(bool trimNegative)
            {
                uint value;
                int read = TryReadUInt32VariantWithoutMoving(trimNegative, out value);
                if (read > 0)
                {
                    mIndex += read;
                    return value;
                }
                throw new InvalidOperationException("ReadUInt32Variant 读取错误" + mData.Length);
            }
            internal ulong ReadUInt64Variant()
            {
                ulong value;
                int read = TryReadUInt64VariantWithoutMoving(out value);
                if (read > 0)
                {
                    mIndex += read;
                    return value;
                }
                throw new InvalidOperationException("ReadUint64Variant 读取错误");
            }

            internal bool TryReadUInt32Variant(out uint value)
            {
                int read = TryReadUInt32VariantWithoutMoving(false, out value);
                if (read > 0)
                {
                    mIndex += read;
                    return true;
                }
                return false;
            }
            internal int TryReadUInt32VariantWithoutMoving(bool trimNegative, out uint value)
            {
                value = 0;
                int readPos = mIndex;
                if (readPos - mLen >= 0) return -1;
                value = mData[readPos++];
                if ((value & 0x80) == 0) return 1;
                value &= 0x7F;

                if (readPos - mLen >= 0) return -1;
                uint chunk = mData[readPos++];
                value |= (chunk & 0x7F) << 7;
                if ((chunk & 0x80) == 0) return 2;

                if (readPos - mLen >= 0) return -1;
                chunk = mData[readPos++];
                value |= (chunk & 0x7F) << 14;
                if ((chunk & 0x80) == 0) return 3;

                if (readPos - mLen >= 0) return -1;
                chunk = mData[readPos++];
                value |= (chunk & 0x7F) << 21;
                if ((chunk & 0x80) == 0) return 4;

                if (readPos - mLen >= 0) return -1;
                chunk = mData[readPos];
                value |= chunk << 28; // can only use 4 bits from this chunk
                if ((chunk & 0xF0) == 0) return 5;

                if (readPos + 5 - mLen >= 0) return -1;
                if (trimNegative // allow for -ve values
                    && (chunk & 0xF0) == 0xF0
                        && mData[++readPos] == 0xFF
                        && mData[++readPos] == 0xFF
                        && mData[++readPos] == 0xFF
                        && mData[++readPos] == 0xFF
                        && mData[++readPos] == 0x01)
                {
                    return 10;
                }

                return -1;
            }

            internal int TryReadUInt64VariantWithoutMoving(out ulong value)
            {
                value = 0;
                int readPos = mIndex;
                if (readPos - mLen >= 0) return -1;
                value = mData[readPos++];
                if ((value & 0x80) == 0) return 1;
                value &= 0x7F;

                if (readPos - mLen >= 0) return -1;
                ulong chunk = mData[readPos++];
                value |= (chunk & 0x7F) << 7;
                if ((chunk & 0x80) == 0) return 2;

                if (readPos - mLen >= 0) return -1;
                chunk = mData[readPos++];
                value |= (chunk & 0x7F) << 14;
                if ((chunk & 0x80) == 0) return 3;

                if (readPos - mLen >= 0) return -1;
                chunk = mData[readPos++];
                value |= (chunk & 0x7F) << 21;
                if ((chunk & 0x80) == 0) return 4;

                if (readPos - mLen >= 0) return -1;
                chunk = mData[readPos++];
                value |= (chunk & 0x7F) << 28;
                if ((chunk & 0x80) == 0) return 5;

                if (readPos - mLen >= 0) return -1;
                chunk = mData[readPos++];
                value |= (chunk & 0x7F) << 35;
                if ((chunk & 0x80) == 0) return 6;

                if (readPos - mLen >= 0) return -1;
                chunk = mData[readPos++];
                value |= (chunk & 0x7F) << 42;
                if ((chunk & 0x80) == 0) return 7;


                if (readPos - mLen >= 0) return -1;
                chunk = mData[readPos++];
                value |= (chunk & 0x7F) << 49;
                if ((chunk & 0x80) == 0) return 8;

                if (readPos - mLen >= 0) return -1;
                chunk = mData[readPos++];
                value |= (chunk & 0x7F) << 56;
                if ((chunk & 0x80) == 0) return 9;

                if (readPos - mLen >= 0) return -1;
                chunk = mData[readPos];
                value |= chunk << 63; // can only use 1 bit from this chunk

                if ((chunk & ~(ulong)0x01) != 0)
                {
                    throw new InvalidOperationException("读取u64时出现错误");
                }


                return 10;
            }

            #endregion
        }
    }
}




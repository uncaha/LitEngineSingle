using System;
using System.Text;
namespace LitEngine
{
    namespace ProtoCSLS
    {
        public class ProtoBufferWriterCSLS
        {
            public static DataFormat mDataFormat = DataFormat.Default;
            public static bool IsWriteDefaultValue = true;
            private byte[] mDate = new byte[1024];
            private int mIndex = 0;
            private int mFieldNumber = 1;
            private WireType mWireType = WireType.None;
            public int Length
            {
                get
                {
                    return mIndex;
                }
            }
            public byte[] Data
            {
                get
                {
                    return mDate;
                }
            }
            public ProtoBufferWriterCSLS()
            {

            }

            public void WriteFieldHeader(int _fieldnumber, WireType _wireType)
            {
                if (mWireType != WireType.None)
                    throw new InvalidOperationException("Cannot write a " + _wireType.ToString() + " header until the " + mWireType.ToString() + " data has been written");

                mWireType = _wireType;
                WriteHeaderCore(_fieldnumber, _wireType);
            }
            public void WriteFieldHeader(WireType _wireType)
            {
                if (mWireType != WireType.None)
                    throw new InvalidOperationException("Cannot write a " + _wireType.ToString() + " header until the " + mWireType.ToString() + " data has been written");

                mWireType = _wireType;
                WriteHeaderCore(mFieldNumber, _wireType);
            }
            public void WriteFieldHeaderAddFieldNumber(WireType _wireType)
            {
                WriteFieldHeader(_wireType);
                FieldNumberForward();
            }
            public void FieldNumberBack()
            {
                mFieldNumber--;
            }
            public void FieldNumberForward()
            {
                mFieldNumber++;
            }
            internal void WriteHeaderCore(int _fieldNumber, WireType _wireType)
            {
                uint header = (((uint)_fieldNumber) << 3)
                    | (((uint)_wireType) & 7);
                WriteUInt32Variant(header);
            }

            #region 写入by类型

            public void WriteBoolean(bool value)
            {
                WriteUInt32(value ? (uint)1 : (uint)0);
            }
            public void WriteByte(byte value)
            {
                WriteUInt32(value);
            }
            public void WriteBytes(byte[] data, int offset, int length)
            {
                if (data == null)
                    throw new ArgumentNullException("传入的数组不可为空");
                switch (mWireType)
                {
                    case WireType.Fixed32:
                        if (length != 4) throw new InvalidOperationException("length");
                        goto CopyFixedLength;
                    case WireType.Fixed64:
                        if (length != 8)
                            throw new InvalidOperationException("长度与类型不对应 length:" + length + "wiretype:" + mWireType);
                        goto CopyFixedLength;
                    case WireType.String:
                        WriteUInt32Variant((uint)length);
                        goto CopyFixedLength;
                }
                throw new InvalidOperationException("WriteBytes WireType 类型错误");
                CopyFixedLength:
                ChoseLength(length);
                PBHelperCSLE.BlockCopy(data, offset, mDate, mIndex, length);
                IncrementedAndReset(length);
            }
            public void WriteBytes(byte[] data)
            {
                if (data == null)
                    throw new ArgumentNullException("WriteBytes 不可写入null");

                WriteBytes(data, 0, data.Length);
            }
            public void WriteString(string value)
            {
                if (mWireType != WireType.String) DebugLogTypeError();
                if (value == null) throw new ArgumentNullException("WriteString 写入数据不可为空");
                int len = value.Length;
                if (len == 0)
                {
                    WriteUInt32Variant(0);
                    mWireType = WireType.None;
                    return;
                }

                int predicted = Encoding.UTF8.GetByteCount(value);
                WriteUInt32Variant((uint)predicted);
                ChoseLength(predicted);
                int actual = Encoding.UTF8.GetBytes(value, 0, value.Length, mDate, mIndex);
                PBHelperCSLE.DebugAssert(predicted == actual);
                IncrementedAndReset(actual);
            }
            public void WriteSingle(float value)
            {
                switch (mWireType)
                {
                    case WireType.Fixed32:
                        WriteInt32(BitConverter.ToInt32(BitConverter.GetBytes(value), 0));
                        return;
                    case WireType.Fixed64:
                        WriteDouble((double)value);
                        return;
                    default:
                        DebugLogTypeError();
                        break;
                }
            }
            public void WriteDouble(double value)
            {
                switch (mWireType)
                {
                    case WireType.Fixed32:
                        float f = (float)value;
                        if (PBHelperCSLE.IsInfinity(f)
                            && !PBHelperCSLE.IsInfinity(value))
                        {
                            throw new InvalidOperationException("WriteDouble Fixed32 " + value);
                        }
                        WriteSingle(f);
                        return;
                    case WireType.Fixed64:
                        WriteInt64(BitConverter.ToInt64(BitConverter.GetBytes(value), 0));
                        return;
                    default:
                        DebugLogTypeError();
                        break;
                }
            }
            public void WriteSByte(sbyte value)
            {
                WriteInt32(value);
            }
            public void WriteInt16(short value)
            {
                WriteInt32(value);
            }
            public void WriteUInt16(ushort value)
            {
                WriteUInt32(value);
            }
            public void WriteUInt64(ulong value)
            {
                switch (mWireType)
                {
                    case WireType.Fixed64:
                        WriteInt64((long)value);
                        return;
                    case WireType.Variant:
                        WriteUInt64Variant(value);
                        mWireType = WireType.None;
                        return;
                    case WireType.Fixed32:
                        checked { WriteUInt32((uint)value); }
                        return;
                    default:
                        DebugLogTypeError();
                        break;
                }
            }
            public void WriteInt64(long value)
            {
                byte[] buffer;
                int index;
                switch (mWireType)
                {
                    case WireType.Fixed64:
                        ChoseLength(8);
                        buffer = mDate;
                        index = mIndex;
                        buffer[index] = (byte)value;
                        buffer[index + 1] = (byte)(value >> 8);
                        buffer[index + 2] = (byte)(value >> 16);
                        buffer[index + 3] = (byte)(value >> 24);
                        buffer[index + 4] = (byte)(value >> 32);
                        buffer[index + 5] = (byte)(value >> 40);
                        buffer[index + 6] = (byte)(value >> 48);
                        buffer[index + 7] = (byte)(value >> 56);
                        IncrementedAndReset(8);
                        return;
                    case WireType.SignedVariant:
                        WriteUInt64Variant(Zig(value));
                        mWireType = WireType.None;
                        return;
                    case WireType.Variant:
                        if (value >= 0)
                        {
                            WriteUInt64Variant((ulong)value);
                            mWireType = WireType.None;
                        }
                        else
                        {
                            ChoseLength(10);
                            buffer = mDate;
                            index = mIndex;
                            buffer[index] = (byte)(value | 0x80);
                            buffer[index + 1] = (byte)((int)(value >> 7) | 0x80);
                            buffer[index + 2] = (byte)((int)(value >> 14) | 0x80);
                            buffer[index + 3] = (byte)((int)(value >> 21) | 0x80);
                            buffer[index + 4] = (byte)((int)(value >> 28) | 0x80);
                            buffer[index + 5] = (byte)((int)(value >> 35) | 0x80);
                            buffer[index + 6] = (byte)((int)(value >> 42) | 0x80);
                            buffer[index + 7] = (byte)((int)(value >> 49) | 0x80);
                            buffer[index + 8] = (byte)((int)(value >> 56) | 0x80);
                            buffer[index + 9] = 0x01; // sign bit
                            IncrementedAndReset(10);
                        }
                        return;
                    case WireType.Fixed32:
                        checked { WriteInt32((int)value); }
                        return;
                    default:
                        DebugLogTypeError();
                        break;
                }
            }
            private void WriteUInt64Variant(ulong value)
            {
                ChoseLength(10);
                int count = 0;
                do
                {
                    mDate[mIndex++] = (byte)((value & 0x7F) | 0x80);
                    count++;
                } while ((value >>= 7) != 0);
                mDate[mIndex - 1] &= 0x7F;
            }

            public void WriteUInt32(uint value)
            {
                switch (mWireType)
                {
                    case WireType.Fixed32:
                        WriteInt32((int)value);
                        return;
                    case WireType.Fixed64:
                        WriteInt64((int)value);
                        return;
                    case WireType.Variant:
                        WriteUInt32Variant(value);
                        mWireType = WireType.None;
                        return;
                    default:
                        DebugLogTypeError();
                        break;
                }
            }
            public void WriteInt32(int value)
            {
                byte[] buffer;
                int index;
                switch (mWireType)
                {
                    case WireType.Fixed32:
                        ChoseLength(4);
                        WriteInt32ToBuffer(value, mDate, mIndex);
                        IncrementedAndReset(4);
                        return;
                    case WireType.Fixed64:
                        ChoseLength(8);
                        buffer = mDate;
                        index = mIndex;
                        buffer[index] = (byte)value;
                        buffer[index + 1] = (byte)(value >> 8);
                        buffer[index + 2] = (byte)(value >> 16);
                        buffer[index + 3] = (byte)(value >> 24);
                        buffer[index + 4] = buffer[index + 5] =
                            buffer[index + 6] = buffer[index + 7] = 0;
                        IncrementedAndReset(8);
                        return;
                    case WireType.SignedVariant:
                        WriteUInt32Variant(Zig(value));
                        mWireType = WireType.None;
                        return;
                    case WireType.Variant:
                        if (value >= 0)
                        {
                            WriteUInt32Variant((uint)value);
                            mWireType = WireType.None;
                        }
                        else
                        {
                            ChoseLength(10);
                            buffer = mDate;
                            index = mIndex;
                            buffer[index] = (byte)(value | 0x80);
                            buffer[index + 1] = (byte)((value >> 7) | 0x80);
                            buffer[index + 2] = (byte)((value >> 14) | 0x80);
                            buffer[index + 3] = (byte)((value >> 21) | 0x80);
                            buffer[index + 4] = (byte)((value >> 28) | 0x80);
                            buffer[index + 5] = buffer[index + 6] =
                                buffer[index + 7] = buffer[index + 8] = (byte)0xFF;
                            buffer[index + 9] = (byte)0x01;
                            IncrementedAndReset(10);
                        }
                        return;
                    default:
                        DebugLogTypeError();
                        break;
                }

            }

            private void WriteInt32ToBuffer(int value, byte[] buffer, int index)
            {
                buffer[index] = (byte)value;
                buffer[index + 1] = (byte)(value >> 8);
                buffer[index + 2] = (byte)(value >> 16);
                buffer[index + 3] = (byte)(value >> 24);
            }

            private void WriteUInt32Variant(uint value)
            {
                ChoseLength(5);
                int count = 0;
                do
                {
                    mDate[mIndex++] = (byte)((value & 0x7F) | 0x80);
                    count++;
                } while ((value >>= 7) != 0);
                mDate[mIndex - 1] &= 0x7F;
            }

            #endregion
            #region 工具
            internal static uint Zig(int value)
            {
                return (uint)((value << 1) ^ (value >> 31));
            }
            internal static ulong Zig(long value)
            {
                return (ulong)((value << 1) ^ (value >> 63));
            }
            private void ChoseLength(int _length)
            {
                if (mIndex + _length - mDate.Length > 0)
                {
                    byte[] tbuffer = new byte[mDate.Length * 2];
                    Array.Copy(mDate, 0, tbuffer, 0, mDate.Length);
                    mDate = tbuffer;
                }
            }
            private void IncrementedAndReset(int length)
            {
                mIndex += length;
                mWireType = WireType.None;
            }
            private void DebugLogTypeError()
            {
                throw new ArgumentNullException("WireType 类型错误:" + mWireType.ToString());
            }
            public bool IsDefaultValue(object _value)
            {
                Type ttype = _value.GetType();
                object tdefaultvalue = null;
                TypeCode code = System.Type.GetTypeCode(ttype);
                switch (code)
                {
                    case TypeCode.Empty:
                    case TypeCode.Boolean:
                        tdefaultvalue = GetDefaultValue<bool>();
                        break;
                    case TypeCode.Char:
                        tdefaultvalue = GetDefaultValue<char>();
                        break;
                    case TypeCode.SByte:
                        tdefaultvalue = GetDefaultValue<sbyte>();
                        break;
                    case TypeCode.Byte:
                        tdefaultvalue = GetDefaultValue<byte>();
                        break;
                    case TypeCode.Int16:
                        tdefaultvalue = GetDefaultValue<short>();
                        break;
                    case TypeCode.UInt16:
                        tdefaultvalue = GetDefaultValue<ushort>();
                        break;
                    case TypeCode.Int32:
                        tdefaultvalue = GetDefaultValue<int>();
                        break;
                    case TypeCode.UInt32:
                        tdefaultvalue = GetDefaultValue<uint>();
                        break;
                    case TypeCode.Int64:
                        tdefaultvalue = GetDefaultValue<long>();
                        break;
                    case TypeCode.UInt64:
                        tdefaultvalue = GetDefaultValue<ulong>();
                        break;
                    case TypeCode.Single:
                        tdefaultvalue = GetDefaultValue<float>();
                        break;
                    case TypeCode.Double:
                        tdefaultvalue = GetDefaultValue<double>();
                        break;
                    case TypeCode.Decimal:
                        tdefaultvalue = GetDefaultValue<Decimal>();
                        break;
                    case TypeCode.DateTime:
                        tdefaultvalue = GetDefaultValue<DateTime>();
                        break;
                    case TypeCode.String:
                        tdefaultvalue = GetDefaultValue<string>();
                        break;
                }
                return _value.Equals(tdefaultvalue);
            }
            public T GetDefaultValue<T>()
            {
                return default(T);
            }
            #endregion

            public void WriteByType(object _value, Type _type)
            {
                ProtoTypeCode code = PBHelperCSLE.GetTypeCode(_type);
                switch (code)
                {
                    case ProtoTypeCode.Int32:
                        WriteInt32((int)_value);
                        break;
                    case ProtoTypeCode.UInt32:
                        WriteUInt32((uint)_value);
                        break;
                    case ProtoTypeCode.Int64:
                        WriteInt64((long)_value);
                        break;
                    case ProtoTypeCode.UInt64:
                        WriteUInt64((ulong)_value);
                        break;
                    case ProtoTypeCode.String:
                        WriteString((string)_value);
                        break;
                    case ProtoTypeCode.Single:
                        WriteSingle((float)_value);
                        break;
                    case ProtoTypeCode.Double:
                        WriteDouble((double)_value);
                        break;
                    case ProtoTypeCode.Boolean:
                        WriteBoolean((bool)_value);
                        break;

                    case ProtoTypeCode.Char:
                    case ProtoTypeCode.Byte:
                        WriteByte((byte)_value);
                        break;
                    case ProtoTypeCode.SByte:
                        WriteSByte((sbyte)_value);
                        break;
                    case ProtoTypeCode.Int16:
                        WriteInt16((short)_value);
                        break;
                    case ProtoTypeCode.UInt16:
                        WriteUInt16((ushort)_value);
                        break;
                    case ProtoTypeCode.Guid:
                    case ProtoTypeCode.Uri:
                    case ProtoTypeCode.ByteArray:
                    case ProtoTypeCode.Type:
                    case ProtoTypeCode.TimeSpan:
                    case ProtoTypeCode.Decimal:
                    case ProtoTypeCode.DateTime:
                        WriteString((string)_value.ToString());
                        break;

                }
            }
            public void Write(object _value)
            {
                if (_value == null || !IsWriteDefaultValue && IsDefaultValue(_value)) return;
                Write(_value, mFieldNumber);
                FieldNumberForward();
            }
            public void Write(object _value, int _fieldnumber)
            {
                if (_value == null || !IsWriteDefaultValue && IsDefaultValue(_value)) return;
                WireType twiretype = PBHelperCSLE.GetWireType(_value.GetType());
                WriteFieldHeader(_fieldnumber, twiretype);
                WriteByType(_value, _value.GetType());
            }

        }
    }
}


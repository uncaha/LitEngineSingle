using System;
namespace LitEngine
{
    namespace ProtoCSLS
    {
        public enum WireType
        {
            /// <summary>
            /// Represents an error condition
            /// </summary>
            None = -1,

            /// <summary>
            /// Base-128 variant-length encoding
            /// </summary>
            Variant = 0,

            /// <summary>
            /// Fixed-length 8-byte encoding
            /// </summary>
            Fixed64 = 1,

            /// <summary>
            /// Length-variant-prefixed encoding
            /// </summary>
            String = 2,

            /// <summary>
            /// Indicates the start of a group
            /// </summary>
            StartGroup = 3,

            /// <summary>
            /// Indicates the end of a group
            /// </summary>
            EndGroup = 4,

            /// <summary>
            /// Fixed-length 4-byte encoding
            /// </summary>10
            Fixed32 = 5,

            /// <summary>
            /// This is not a formal wire-type in the "protocol buffers" spec, but
            /// denotes a variant integer that should be interpreted using
            /// zig-zag semantics (so -ve numbers aren't a significant overhead)
            /// </summary>
            SignedVariant = WireType.Variant | (1 << 3),
        }
        public enum ProtoTypeCode
        {
            Empty = 0,
            Unknown = 1, // maps to TypeCode.Object
            Boolean = 3,
            Char = 4,
            SByte = 5,
            Byte = 6,
            Int16 = 7,
            UInt16 = 8,
            Int32 = 9,
            UInt32 = 10,
            Int64 = 11,
            UInt64 = 12,
            Single = 13,
            Double = 14,
            Decimal = 15,
            DateTime = 16,
            String = 18,

            // additions
            TimeSpan = 100,
            ByteArray = 101,
            Guid = 102,
            Uri = 103,
            Type = 104
        }

        public enum DataFormat
        {
            /// <summary>
            /// Uses the default encoding for the data-type.
            /// </summary>
            Default,

            /// <summary>
            /// When applied to signed integer-based data (including Decimal), this
            /// indicates that zigzag variant encoding will be used. This means that values
            /// with small magnitude (regardless of sign) take a small amount
            /// of space to encode.
            /// </summary>
            ZigZag,

            /// <summary>
            /// When applied to signed integer-based data (including Decimal), this
            /// indicates that two's-complement variant encoding will be used.
            /// This means that any -ve number will take 10 bytes (even for 32-bit),
            /// so should only be used for compatibility.
            /// </summary>
            TwosComplement,

            /// <summary>
            /// When applied to signed integer-based data (including Decimal), this
            /// indicates that a fixed amount of space will be used.
            /// </summary>
            FixedSize,

            /// <summary>
            /// When applied to a sub-message, indicates that the value should be treated
            /// as group-delimited.
            /// </summary>
            Group
        }

        public class PBHelperCSLE
        {
            public static DataFormat mDataFormat = DataFormat.Default;
            static public ProtoTypeCode GetTypeCode(System.Type type)
            {
                TypeCode code = System.Type.GetTypeCode(type);
                switch (code)
                {
                    case TypeCode.Empty:
                    case TypeCode.Boolean:
                    case TypeCode.Char:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                    case TypeCode.DateTime:
                    case TypeCode.String:
                        return (ProtoTypeCode)code;
                }
                if (type == typeof(TimeSpan)) return ProtoTypeCode.TimeSpan;
                if (type == typeof(Guid)) return ProtoTypeCode.Guid;
                if (type == typeof(Uri)) return ProtoTypeCode.Uri;

                if (type == typeof(byte[])) return ProtoTypeCode.ByteArray;
                if (type == typeof(System.Type)) return ProtoTypeCode.Type;

                return ProtoTypeCode.Unknown;

            }
            public static bool IsInfinity(double value)
            {
                return double.IsInfinity(value);
            }
            public static void BlockCopy(byte[] from, int fromIndex, byte[] to, int toIndex, int count)
            {
                Buffer.BlockCopy(from, fromIndex, to, toIndex, count);
            }
            public static void DebugAssert(bool condition)
            {
#if DEBUG
            if (!condition && System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
            System.Diagnostics.Debug.Assert(condition);
#endif
            }

            private static WireType GetDateTimeWireType(DataFormat format)
            {
                switch (format)
                {
                    case DataFormat.Group: return WireType.StartGroup;
                    case DataFormat.FixedSize: return WireType.Fixed64;
                    case DataFormat.Default: return WireType.String;
                    default:
                        return WireType.None;
                }
            }
            private static WireType GetIntWireType(DataFormat format, int width)
            {
                switch (format)
                {
                    case DataFormat.ZigZag: return WireType.SignedVariant;
                    case DataFormat.FixedSize: return width == 32 ? WireType.Fixed32 : WireType.Fixed64;
                    case DataFormat.TwosComplement:
                    case DataFormat.Default: return WireType.Variant;
                    default:
                        return WireType.None;
                }
            }
            public static WireType GetWireType(Type _type)
            {
                WireType ret = WireType.None;
                ProtoTypeCode code = GetTypeCode(_type);
                switch (code)
                {
                    case ProtoTypeCode.Int32:
                        ret = GetIntWireType(mDataFormat, 32);
                        break;
                    case ProtoTypeCode.UInt32:
                        ret = GetIntWireType(mDataFormat, 32);
                        break;
                    case ProtoTypeCode.Int64:
                        ret = GetIntWireType(mDataFormat, 64);
                        break;
                    case ProtoTypeCode.UInt64:
                        ret = GetIntWireType(mDataFormat, 64);
                        break;
                    case ProtoTypeCode.String:
                        ret = WireType.String;
                        break;
                    case ProtoTypeCode.Single:
                        ret = WireType.Fixed32;
                        break;
                    case ProtoTypeCode.Double:
                        ret = WireType.Fixed64;
                        break;
                    case ProtoTypeCode.Boolean:
                        ret = WireType.Variant;
                        break;
                    case ProtoTypeCode.DateTime:
                        ret = GetDateTimeWireType(mDataFormat);
                        break;
                    case ProtoTypeCode.Decimal:
                        ret = WireType.String;
                        break;
                    case ProtoTypeCode.Byte:
                        ret = GetIntWireType(mDataFormat, 32);
                        break;
                    case ProtoTypeCode.SByte:
                        ret = GetIntWireType(mDataFormat, 32);
                        break;
                    case ProtoTypeCode.Char:
                        ret = WireType.Variant;
                        break;
                    case ProtoTypeCode.Int16:
                        ret = GetIntWireType(mDataFormat, 32);
                        break;
                    case ProtoTypeCode.UInt16:
                        ret = GetIntWireType(mDataFormat, 32);
                        break;
                    case ProtoTypeCode.TimeSpan:
                        ret = GetDateTimeWireType(mDataFormat);
                        break;
                    case ProtoTypeCode.Guid:
                        ret = WireType.String;
                        break;
                    case ProtoTypeCode.Uri:
                        ret = WireType.String;
                        break;
                    case ProtoTypeCode.ByteArray:
                        ret = WireType.String;
                        break;
                    case ProtoTypeCode.Type:
                        ret = WireType.String;
                        break;
                }
                return ret;
            }
        }
    }
}


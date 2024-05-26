using LitEngine.LType;
namespace LitEngine
{
    namespace Data
    {
        public sealed class DataField : DataBaseElement
        {
            public string Key { get; private set; }
            public FieldType ValueType { get; private set; }
            public object Value
            {
                get
                {
                    object ret = dvalue;
                    return ret;
                }
                set
                {
                    ValueType = GetValueType(value);
                    dvalue = null;
                    if (ValueType != FieldType.Null)
                        dvalue = value;

                }
            }
            private object dvalue;
            public DataField(string _key, object _value)
            {
                Key = _key;
                Value = _value;
            }

            public T GetValue<T>(object _defaultValue = null)
            {
                try
                {
                    checked
                    {
                        return Value != null ? (T)Value : _defaultValue == null ? default(T) : (T)_defaultValue;
                    }
                }
                catch (System.Exception erro)
                {
                    DLog.LogError(erro.ToString());
                }
                return _defaultValue == null ? default(T) : (T)_defaultValue;
            }

            #region fieldtype
            private FieldType GetValueType(object _obj)
            {
                if (_obj == null)
                    return FieldType.Null;
                else
                    return GetValueTypeByString(_obj.GetType().Name);
            }
            private FieldType GetValueTypeByString(string _str)
            {
                if (string.IsNullOrEmpty(_str) || _str.Equals("null"))
                    return FieldType.Null;
                try
                {
                    if(_str.Equals("Byte[]"))
                    {
                        return FieldType.Bytes;
                    }
                    else
                    {
                        return !string.IsNullOrEmpty(_str) ? (FieldType)System.Enum.Parse(typeof(FieldType), _str) : FieldType.Null;
                    }  
                }
                catch (System.Exception)
                {
                    return FieldType.Null;
                }

            }
            #endregion

            #region load
            override public void Load(LitEngine.IO.AESReader _loader)
            {
                Key = _loader.ReadString();
                Attribut.Load(_loader);
                ValueType = GetValueTypeByString(_loader.ReadString());
                LoadByType(_loader);
            }
            private void LoadByType(LitEngine.IO.AESReader _loader)
            {
                switch (ValueType)
                {
                    case FieldType.Boolean:
                        dvalue = _loader.ReadBoolean();
                        break;
                    case FieldType.Byte:
                        dvalue = _loader.ReadByte();
                        break;
                    case FieldType.SByte:
                        dvalue = _loader.ReadSByte();
                        break;
                    case FieldType.Int16:
                        dvalue = _loader.ReadInt16();
                        break;
                    case FieldType.UInt16:
                        dvalue = _loader.ReadUInt16();
                        break;
                    case FieldType.Int32:
                        dvalue = _loader.ReadInt32();
                        break;
                    case FieldType.UInt32:
                        dvalue = _loader.ReadUInt32();
                        break;
                    case FieldType.Int64:
                        dvalue = _loader.ReadInt64();
                        break;
                    case FieldType.UInt64:
                        dvalue = _loader.ReadUInt64();
                        break;
                    case FieldType.Single:
                        dvalue = _loader.ReadSingle();
                        break;
                    case FieldType.Double:
                        dvalue = _loader.ReadDouble();
                        break;
                    case FieldType.Decimal:
                        dvalue = _loader.ReadDecimal();
                        break;
                    case FieldType.String:
                        dvalue = _loader.ReadString();
                        break;
                    case FieldType.Char:
                        dvalue = _loader.ReadChar();
                        break;
                    case FieldType.BigInteger:
                        {
                            int tlen =  _loader.ReadInt32();
                            byte[] tbytes = _loader.ReadBytes(tlen);
                            dvalue = new System.Numerics.BigInteger(tbytes);
                        }
                        break;
                    case FieldType.Bytes:
                        {
                            int tlen = _loader.ReadInt32();
                            dvalue = _loader.ReadBytes(tlen);
                        }
                        break;
                    default:
                        break;
                }
            }
            #endregion

            #region save
            override public void Save(LitEngine.IO.AESWriter _writer)
            {
                _writer.WriteString(Key);
                Attribut.Save(_writer);
                _writer.WriteString(ValueType.ToString());
                if (ValueType != FieldType.Null)
                    SaveByType(_writer);
            }

            private void SaveByType(LitEngine.IO.AESWriter _writer)
            {
                switch (ValueType)
                {
                    case FieldType.Boolean:
                        _writer.WriteBool((bool)dvalue);
                        break;
                    case FieldType.Byte:
                        _writer.WriteByte((byte)dvalue);
                        break;
                    case FieldType.SByte:
                        _writer.WriteSByte((sbyte)dvalue);
                        break;
                    case FieldType.Int16:
                        _writer.WriteShort((short)dvalue);
                        break;
                    case FieldType.UInt16:
                        _writer.WriteUShort((ushort)dvalue);
                        break;
                    case FieldType.Int32:
                        _writer.WriteInt((int)dvalue);
                        break;
                    case FieldType.UInt32:
                        _writer.WriteUInt((uint)dvalue);
                        break;
                    case FieldType.Int64:
                        _writer.WriteLong((long)dvalue);
                        break;
                    case FieldType.UInt64:
                        _writer.WriteULong((ulong)dvalue);
                        break;
                    case FieldType.Single:
                        _writer.WriteFloat((float)dvalue);
                        break;
                    case FieldType.Double:
                        _writer.WriteDouble((double)dvalue);
                        break;
                    case FieldType.Decimal:
                        _writer.WriteDecimal((decimal)dvalue);
                        break;
                    case FieldType.String:
                        _writer.WriteString((string)dvalue);
                        break;
                    case FieldType.Char:
                        _writer.WriteChar((char)dvalue);
                        break;
                    case FieldType.BigInteger:
                        {
                            byte[] tbytes = ((System.Numerics.BigInteger)dvalue).ToByteArray();
                            _writer.WriteInt(tbytes.Length);
                            _writer.WriteBytes(tbytes);
                        }
                        break;
                    case FieldType.Bytes:
                        {
                            byte[] tbytes = (byte[])dvalue;
                            _writer.WriteInt(tbytes.Length);
                            _writer.WriteBytes(tbytes);
                        }
                        break;
                    default:
                        DLog.LogWarningFormat("暂不支持的类型,无法存储对应的数据.Key = {0} , Type ={1}", Key, ValueType);
                        break;
                }
            }
            #endregion

        }
    }
}
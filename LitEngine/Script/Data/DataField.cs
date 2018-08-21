namespace LitEngine
{
    namespace Data
    {
        public sealed class DataField : DataBaseElement
        {
            public string Key { get; private set; }
            public string ValueType { get; private set; }
            public object Value
            {
                get
                {
                    object ret = dvalue;
                    return ret;
                }
                set
                {
                    dvalue = null;
                    dvalue = value;
                    ValueType = dvalue != null ? dvalue.GetType().Name : null;
                }
            }
            private object dvalue;
            public DataField(string _key, object _value)
            {
                Key = _key;
                Value = _value;
            }

            public T TryGetValue<T>(object _defaultValue = null)
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

            #region load
            override public void Load(LitEngine.IO.AESReader _loader)
            {
                Key = _loader.ReadString();
                Attribut.Load(_loader);
                ValueType = _loader.ReadString();
                LoadByType(_loader);
            }
            private void LoadByType(LitEngine.IO.AESReader _loader)
            {
                switch (ValueType)
                {
                    case "Boolean":
                        Value = _loader.ReadBoolean();
                        break;
                    case "Byte":
                        Value = _loader.ReadByte();
                        break;
                    case "SByte":
                        Value = _loader.ReadSByte();
                        break;
                    case "Int16":
                        Value = _loader.ReadInt16();
                        break;
                    case "UInt16":
                        Value = _loader.ReadUInt16();
                        break;
                    case "Int32":
                        Value = _loader.ReadInt32();
                        break;
                    case "UInt32":
                        Value = _loader.ReadUInt32();
                        break;
                    case "Int64":
                        Value = _loader.ReadInt64();
                        break;
                    case "UInt64":
                        Value = _loader.ReadUInt64();
                        break;
                    case "Single":
                        Value = _loader.ReadSingle();
                        break;
                    case "Double":
                        Value = _loader.ReadDouble();
                        break;
                    case "Decimal":
                        Value = _loader.ReadDecimal();
                        break;
                    case "String":
                        Value = _loader.ReadString();
                        break;
                    case "Char":
                        Value = _loader.ReadChar();
                        break;
                    case "Byte[]":
                        {
                            int tlen = _loader.ReadInt32();
                            Value = _loader.ReadBytes(tlen);
                        }
                        break;
                    case "null":
                        ValueType = null;
                        break;
                    default:
                        break;
                }
            }
            #endregion

            #region save
            override  public void Save(LitEngine.IO.AESWriter _writer)
            {
                _writer.WriteString(Key);
                Attribut.Save(_writer);
                _writer.WriteString(ValueType == null ? "null" : ValueType);
                if (ValueType != null)
                    SaveByType(_writer);
            }

            private void SaveByType(LitEngine.IO.AESWriter _writer)
            {
                switch (ValueType)
                {
                    case "Boolean":
                        _writer.WriteBool((bool)Value);
                        break;
                    case "Byte":
                        _writer.WriteByte((byte)Value);
                        break;
                    case "SByte":
                        _writer.WriteSByte((sbyte)Value);
                        break;
                    case "Int16":
                        _writer.WriteShort((short)Value);
                        break;
                    case "UInt16":
                        _writer.WriteUShort((ushort)Value);
                        break;
                    case "Int32":
                        _writer.WriteInt((int)Value);
                        break;
                    case "UInt32":
                        _writer.WriteUInt((uint)Value);
                        break;
                    case "Int64":
                        _writer.WriteLong((long)Value);
                        break;
                    case "UInt64":
                        _writer.WriteULong((ulong)Value);
                        break;
                    case "Single":
                        _writer.WriteFloat((float)Value);
                        break;
                    case "Double":
                        _writer.WriteDouble((double)Value);
                        break;
                    case "Decimal":
                        _writer.WriteDecimal((decimal)Value);
                        break;
                    case "String":
                        _writer.WriteString((string)Value);
                        break;
                    case "Char":
                        _writer.WriteChar((char)Value);
                        break;
                    case "Byte[]":
                        {
                            byte[] tbytes = (byte[])Value;
                            _writer.WriteInt(tbytes.Length);
                            _writer.WriteBytes(tbytes);
                        }
                        break;
                    default:
                        DLog.LogWarning($"暂不支持的类型,无法存储对应的数据.Key = {Key} , Type ={ValueType}");
                        break;
                }
            }
            #endregion

        }
    }
}
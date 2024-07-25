using System.Collections;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using LitEngine.LType;
using LitEngine;
namespace LitEngine.Data
{
    public enum DataType
    {
        baseType = 0,
        DataObject,
        List,
        Dictionary,
    }
    public class NeedToSave : System.Attribute
    {

        public string FieldName { get; set; }
        public DataType dataType { get; private set; }
        public DataType childType { get; private set; }
        public DataType keyType { get; private set; }
        public NeedToSave(string pName, DataType pDataType, DataType pChildType = DataType.baseType, DataType pKeyType = DataType.baseType)
        {
            FieldName = pName;
            dataType = pDataType;
            childType = pChildType;
            keyType = pKeyType;
        }
        public NeedToSave(DataType pDataType, DataType pChildType = DataType.baseType, DataType pKeyType = DataType.baseType) : this("", pDataType, pChildType, pKeyType)
        {
        }

        public NeedToSave() : this("", DataType.baseType, DataType.baseType)
        {

        }
    }
    public class DataUtil
    {
        public class SaveValue
        {
            public FieldInfo info;
            public object value;
        }

        public DataUtil()
        {
        }
        #region save
        static public void Save(string pFIle, object pTarget)
        {
            LitEngine.IO.AESWriter twriter = null;
            string tfullname = GameCore.AppPersistentAssetsPath + pFIle;
            string tempfile = tfullname + ".temp";
            try
            {
                twriter = new LitEngine.IO.AESWriter(tempfile);

                SaveByField(twriter, new NeedToSave(pTarget.GetType().FullName, DataType.DataObject), pTarget);

                twriter.Flush();
                twriter.Close();
                twriter = null;
                if (File.Exists(tfullname))
                {
                    File.Delete(tfullname);
                }
                File.Copy(tempfile, tfullname);
            }
            catch (System.Exception pErro)
            {
                DLog.LogError(pErro.ToString());
            }

            if (twriter != null)
                twriter.Close();

        }

        static private void SaveByField(LitEngine.IO.AESWriter pWriter, NeedToSave pAtt, object pTarget)
        {
            pWriter.WriteInt((int)pAtt.dataType);
            pWriter.WriteString(pAtt.FieldName);

            switch (pAtt.dataType)
            {
                case DataType.baseType:
                    SaveBaseObject(pWriter, pAtt, pTarget);
                    break;
                case DataType.DataObject:
                    SaveDataObject(pWriter, pAtt, pTarget);
                    break;
                case DataType.List:
                    SaveList(pWriter, pAtt, pTarget);
                    break;
                case DataType.Dictionary:
                    SaveDictionary(pWriter, pAtt, pTarget);
                    break;
            }
        }

        static private void SaveDataObject(LitEngine.IO.AESWriter pWriter, NeedToSave pAtt, object pTarget)
        {
            pWriter.WriteBool(pTarget != null);
            if (pTarget == null) return;
            System.Type ttype = pTarget.GetType();
            var tfields = ttype.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            List<SaveValue> tfieldlist = new List<SaveValue>();

            for (int i = 0; i < tfields.Length; i++)
            {
                FieldInfo tinfo = tfields[i];
                NeedToSave tetst = tinfo.GetCustomAttribute(typeof(NeedToSave)) as NeedToSave;
                object tvalue = tinfo.GetValue(pTarget);

                if (tetst != null && tvalue != null)
                {
                    SaveValue item = new SaveValue() { info = tinfo,value = tvalue };
                    tfieldlist.Add(item);
                }
            }

            pWriter.WriteString(ttype.FullName);
            pWriter.WriteInt(tfieldlist.Count);

            for (int i = 0; i < tfieldlist.Count; i++)
            {
                SaveValue tinfo = tfieldlist[i];
                NeedToSave tatt = DataUtil.GetSaveAttribute(tinfo.value, tinfo.info.Name);
                SaveByField(pWriter, tatt, tinfo.value);
            }

        }

        static private void SaveBaseObject(LitEngine.IO.AESWriter pWriter, NeedToSave pAtt, object pTarget)
        {
            FieldType ttype = GetValueType(pTarget);
            if (ttype == FieldType.Null)
            {
                throw new System.ArgumentNullException("未知类型." + pAtt.FieldName);
            }

            SaveByType(pWriter, ttype, pTarget);
        }
        static private void SaveList(LitEngine.IO.AESWriter pWriter, NeedToSave pAtt, object pTarget)
        {
            pWriter.WriteBool(pTarget != null);
            if (pTarget == null) return;
            ArrayList tlist = new ArrayList((ICollection)pTarget);
            pWriter.WriteString(pTarget.GetType().FullName);
            pWriter.WriteInt(tlist.Count);
            for (int i = 0; i < tlist.Count; i++)
            {
                var item = tlist[i];
                SaveByField(pWriter, new NeedToSave(i.ToString(), pAtt.childType), item);
            }
        }
        static private void SaveDictionary(LitEngine.IO.AESWriter pWriter, NeedToSave pAtt, object pTarget)
        {
            pWriter.WriteBool(pTarget != null);
            if (pTarget == null) return;
            System.Type tarType = pTarget.GetType();
            MethodInfo tm = tarType.GetMethod("get_Item", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            PropertyInfo tcinfo = tarType.GetProperty("Keys");
            ArrayList tlist = new ArrayList((ICollection)tcinfo.GetValue(pTarget));

            pWriter.WriteString(pTarget.GetType().FullName);
            pWriter.WriteInt(tlist.Count);
            for (int i = 0; i < tlist.Count; i++)
            {
                var tkey = tlist[i];
                object tvalue = tm.Invoke(pTarget, new object[] { tkey });
                string tname = i.ToString();
                SaveByField(pWriter, new NeedToSave(tname, pAtt.keyType), tkey);
                SaveByField(pWriter, new NeedToSave(tname, pAtt.childType), tvalue);
            }

        }
        #endregion

        #region load
        static public object Load(string pFile, object pValue)
        {
            LitEngine.IO.AESReader tloader = null;
            object ret = pValue;
            try
            {
                string tfullname = GameCore.AppPersistentAssetsPath + pFile;
                if (!File.Exists(tfullname)) return ret;
                tloader = new LitEngine.IO.AESReader(tfullname);
                ret = ReadField(tloader, pValue, null);

                tloader.Close();
                tloader = null;
            }
            catch (System.Exception pErro)
            {
                DLog.LogError(pErro.ToString());
            }

            if (tloader != null)
            {
                tloader.Close();
            }
            return ret;
        }

        static private object ReadField(LitEngine.IO.AESReader pReader, object pValue, object pParent)
        {
            DataType ttype = (DataType)pReader.ReadInt32();
            string tFieldName = pReader.ReadString();

            FieldInfo tinfo = pParent == null ? null : pParent.GetType().GetField(tFieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (pValue == null && tinfo != null)
            {
                pValue = tinfo.GetValue(pParent);
                if (pValue == null)
                {
                    pValue = GetObjectByType(tinfo.FieldType, true);
                }
            }
            object ret = pValue;
            switch (ttype)
            {
                case DataType.baseType:
                    ret = ReadBaseType(pReader);
                    break;
                case DataType.DataObject:
                    ret = ReadDataObject(pReader, pValue);
                    break;
                case DataType.List:
                    ret = ReadList(pReader, pValue);
                    break;
                case DataType.Dictionary:
                    ret = ReadDictionary(pReader, pValue);
                    break;
            }

            if (tinfo != null)
            {
                tinfo.SetValue(pParent, ret);
            }

            return ret;
        }
        static private object ReadDataObject(LitEngine.IO.AESReader pReader, object pTarget)
        {
            bool thave = pReader.ReadBoolean();
            if (!thave) return null;
            string tfullname = pReader.ReadString();
            int tFieldCount = pReader.ReadInt32();

            for (int i = 0; i < tFieldCount; i++)
            {
                ReadField(pReader, null, pTarget);
            }
            return pTarget;
        }
        static private object ReadBaseType(LitEngine.IO.AESReader pReader)
        {
            return LoadByType(pReader);
        }

        static private object ReadList(LitEngine.IO.AESReader pReader, object pTarget)
        {
            bool thave = pReader.ReadBoolean();
            if (!thave) return null;

            string tfullname = pReader.ReadString();
            int tcount = pReader.ReadInt32();

            System.Type tselftype = pTarget.GetType();

            MethodInfo tclear = tselftype.GetMethod("Clear");
            tclear.Invoke(pTarget, null);

            MethodInfo tmethod = tselftype.GetMethod("Add");

            for (int i = 0; i < tcount; i++)
            {
                object titem = null;
                System.Type[] genericArgTypes = tselftype.GetGenericArguments();
                if (genericArgTypes != null && genericArgTypes.Length > 0)
                {
                    titem = GetObjectByType(genericArgTypes[0], true);
                }

                object tobj = ReadField(pReader, titem, null);
                tmethod.Invoke(pTarget, new object[] { tobj });
            }

            return pTarget;
        }

        static private object ReadDictionary(LitEngine.IO.AESReader pReader, object pTarget)
        {
            bool thave = pReader.ReadBoolean();
            if (!thave) return null;
            string tfullname = pReader.ReadString();
            int tcount = pReader.ReadInt32();
            System.Type tselftype = pTarget.GetType();

            MethodInfo tclear = tselftype.GetMethod("Clear");
            tclear.Invoke(pTarget, null);

            MethodInfo tmethod = tselftype.GetMethod("Add");

            for (int i = 0; i < tcount; i++)
            {
                object tkey = null;
                object titem = null;
                System.Type[] genericArgTypes = tselftype.GetGenericArguments();
                if (genericArgTypes != null && genericArgTypes.Length > 0)
                {
                    tkey = GetObjectByType(genericArgTypes[0], true);
                    titem = GetObjectByType(genericArgTypes[1], true);
                }
                tkey = ReadField(pReader, tkey, null);
                titem = ReadField(pReader, titem, null);
                tmethod.Invoke(pTarget, new object[] { tkey, titem });
            }

            return pTarget;
        }
        #endregion

        static public object GetObjectByType(System.Type pType, bool pNonPublic)
        {
            if (typeof(System.String) == pType) return "";
            object ret = System.Activator.CreateInstance(pType, pNonPublic);
            return ret;
        }
        static public NeedToSave GetSaveAttribute(object pTarget, string pFieldName)
        {
            NeedToSave ret = null;
            System.Type ttype = pTarget.GetType();
            DataType tselftype = GetDataTypeSysType(ttype);
            DataType tchildType = DataType.baseType;
            DataType tkeyType = DataType.baseType;
            if (ttype.FullName.Contains(".List"))
            {
                System.Type[] genericArgTypes = ttype.GetGenericArguments();
                if (genericArgTypes != null && genericArgTypes.Length > 0)
                {
                    tchildType = GetDataTypeSysType(genericArgTypes[0]);
                }
            }
            else if (ttype.FullName.Contains(".Dictionary"))
            {
                System.Type[] genericArgTypes = ttype.GetGenericArguments();
                if (genericArgTypes != null && genericArgTypes.Length > 0)
                {
                    tkeyType = GetDataTypeSysType(genericArgTypes[0]);
                    tchildType = GetDataTypeSysType(genericArgTypes[1]);
                }
            }

            ret = new NeedToSave(pFieldName, tselftype, tchildType, tkeyType);
            return ret;
        }

        static public DataType GetDataTypeSysType(System.Type pType)
        {
            DataType ret = DataType.baseType;
            if (pType.FullName.Contains(".List"))
            {
                ret = DataType.List;
            }
            else if (pType.FullName.Contains(".Dictionary"))
            {
                ret = DataType.Dictionary;
            }
            else
            {
                FieldType tftype = GetValueTypeByString(pType.Name);
                if (tftype == FieldType.Null)
                {
                    ret = DataType.DataObject;
                }
                else
                {
                    ret = DataType.baseType;
                }
            }
            return ret;
        }
        static public FieldType GetValueType(object _obj)
        {
            if (_obj == null)
                return FieldType.Null;
            else
                return GetValueTypeByString(_obj.GetType().Name);
        }
        static public FieldType GetValueTypeByString(string _str)
        {
            if (string.IsNullOrEmpty(_str) || _str.Equals("null"))
                return FieldType.Null;
            try
            {
                if (_str.Equals("Byte[]"))
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
        static public object LoadByType(LitEngine.IO.AESReader pLoader)
        {
            FieldType ttype = GetValueTypeByString(pLoader.ReadString());
            object ret = null;
            switch (ttype)
            {
                case FieldType.Boolean:
                    ret = pLoader.ReadBoolean();
                    break;
                case FieldType.Byte:
                    ret = pLoader.ReadByte();
                    break;
                case FieldType.SByte:
                    ret = pLoader.ReadSByte();
                    break;
                case FieldType.Int16:
                    ret = pLoader.ReadInt16();
                    break;
                case FieldType.UInt16:
                    ret = pLoader.ReadUInt16();
                    break;
                case FieldType.Int32:
                    ret = pLoader.ReadInt32();
                    break;
                case FieldType.UInt32:
                    ret = pLoader.ReadUInt32();
                    break;
                case FieldType.Int64:
                    ret = pLoader.ReadInt64();
                    break;
                case FieldType.UInt64:
                    ret = pLoader.ReadUInt64();
                    break;
                case FieldType.Single:
                    ret = pLoader.ReadSingle();
                    break;
                case FieldType.Double:
                    ret = pLoader.ReadDouble();
                    break;
                case FieldType.Decimal:
                    ret = pLoader.ReadDecimal();
                    break;
                case FieldType.String:
                    ret = pLoader.ReadString();
                    break;
                case FieldType.Char:
                    ret = pLoader.ReadChar();
                    break;
                case FieldType.BigInteger:
                    {
                        int tlen = pLoader.ReadInt32();
                        byte[] tbytes = pLoader.ReadBytes(tlen);
                        ret = new System.Numerics.BigInteger(tbytes);
                    }
                    break;
                case FieldType.Bytes:
                    {
                        int tlen = pLoader.ReadInt32();
                        ret = pLoader.ReadBytes(tlen);
                    }
                    break;
                default:
                    break;
            }
            return ret;
        }
        static public void SaveByType(LitEngine.IO.AESWriter _writer, FieldType pType, object pObject)
        {
            _writer.WriteString(pType.ToString());
            switch (pType)
            {
                case FieldType.Boolean:
                    _writer.WriteBool((bool)pObject);
                    break;
                case FieldType.Byte:
                    _writer.WriteByte((byte)pObject);
                    break;
                case FieldType.SByte:
                    _writer.WriteSByte((sbyte)pObject);
                    break;
                case FieldType.Int16:
                    _writer.WriteShort((short)pObject);
                    break;
                case FieldType.UInt16:
                    _writer.WriteUShort((ushort)pObject);
                    break;
                case FieldType.Int32:
                    _writer.WriteInt((int)pObject);
                    break;
                case FieldType.UInt32:
                    _writer.WriteUInt((uint)pObject);
                    break;
                case FieldType.Int64:
                    _writer.WriteLong((long)pObject);
                    break;
                case FieldType.UInt64:
                    _writer.WriteULong((ulong)pObject);
                    break;
                case FieldType.Single:
                    _writer.WriteFloat((float)pObject);
                    break;
                case FieldType.Double:
                    _writer.WriteDouble((double)pObject);
                    break;
                case FieldType.Decimal:
                    _writer.WriteDecimal((decimal)pObject);
                    break;
                case FieldType.String:
                    _writer.WriteString((string)pObject);
                    break;
                case FieldType.Char:
                    _writer.WriteChar((char)pObject);
                    break;
                case FieldType.BigInteger:
                    {
                        byte[] tbytes = ((System.Numerics.BigInteger)pObject).ToByteArray();
                        _writer.WriteInt(tbytes.Length);
                        _writer.WriteBytes(tbytes);
                    }
                    break;
                case FieldType.Bytes:
                    {
                        byte[] tbytes = (byte[])pObject;
                        _writer.WriteInt(tbytes.Length);
                        _writer.WriteBytes(tbytes);
                    }
                    break;
                default:
                    DLog.LogWarningFormat("暂不支持的类型,无法存储对应的数据.object = {0} , Type ={1}", pObject, pType);
                    break;
            }
        }
    }
}

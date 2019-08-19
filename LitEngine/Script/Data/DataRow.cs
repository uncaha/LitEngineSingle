using System.Collections.Generic;
namespace LitEngine
{
    namespace Data
    {
        public sealed class DataRow : DataBaseElement
        {
            private Dictionary<string, DataField> fieldMap = new Dictionary<string, DataField>();
            private List<DataField> fieldList = new List<DataField>();
            public int Count { get { return fieldList.Count; } }
            public Dictionary<string, DataField>.KeyCollection Keys { get { return fieldMap.Keys; } }
            public string Key { get; private set; }
            
            public DataRow(string _key = null)
            {
                Key = _key;
            }

            public DataField AddField(string pFieldName,object pValue = null)
            {
                if (!fieldMap.ContainsKey(pFieldName))
                {
                    AddFromField(new DataField(pFieldName, pValue));
                }
                DataField ret = fieldMap[pFieldName];
                ret.Value = pValue;
                return ret;
            }

            public void AddFromField(DataField pField)
            {
                if (!fieldMap.ContainsKey(pField.Key))
                {
                    fieldMap.Add(pField.Key, pField);
                    fieldList.Add(pField);
                }       
            }

            public void RemoveField(string pFieldName)
            {
                if (fieldMap.ContainsKey(pFieldName))
                {
                    DataField tfield = fieldMap[pFieldName];
                    fieldMap.Remove(pFieldName);
                    fieldList.Remove(tfield);
                }
                    
            }

            public object this[int pIndex]
            {
                get
                {
                    return fieldList[pIndex].Value;
                }
            }


            public object this[string _fieldKey]
            {
                get
                {
                    if (!fieldMap.ContainsKey(_fieldKey)) return null;
                    return fieldMap[_fieldKey].Value;
                }

                set
                {
                    if (value != null)
                        AddField(_fieldKey).Value = value;
                    else
                        RemoveField(_fieldKey);
                }
            }

            public T GetValue<T>(string _fieldkey, object _defaultValue = null)
            {
                if (!fieldMap.ContainsKey(_fieldkey)) return _defaultValue == null ? default(T) : (T)_defaultValue;
                return fieldMap[_fieldkey].GetValue<T>(_defaultValue);
            }

            public DataField SearchField(string _fieldkey)
            {
                if (!fieldMap.ContainsKey(_fieldkey)) return null;
                return fieldMap[_fieldkey];
            }

            override public void Load(LitEngine.IO.AESReader _loader)
            {
                Key = _loader.ReadString();
                Attribut.Load(_loader);
                int tfieldCount = _loader.ReadInt32();
                for (int i = 0; i < tfieldCount; i++)
                {
                    DataField tfield = new DataField(null, null);
                    tfield.Load(_loader);
                    AddFromField(tfield);
                }
            }
            override public void Save(LitEngine.IO.AESWriter _writer)
            {
                _writer.WriteString(Key);
                Attribut.Save(_writer);
                int tfieldCount = fieldList.Count;
                _writer.WriteInt(tfieldCount);
                for (int i = 0; i < tfieldCount; i++)
                {
                    DataField tfield = fieldList[i];
                    tfield.Save(_writer);
                }
            }
        }
    }
}

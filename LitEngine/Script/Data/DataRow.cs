using System.Collections.Generic;
namespace LitEngine
{
    namespace Data
    {
        public sealed class DataRow : DataBaseElement
        {
            private Dictionary<string, DataField> fieldMap = new Dictionary<string, DataField>();
            public Dictionary<string, DataField>.ValueCollection Fields { get { return fieldMap.Values; } }
            public Dictionary<string, DataField>.KeyCollection Keys { get { return fieldMap.Keys; } }
            public string Key { get; private set; }
            public int Count { get { return fieldMap.Count; } }
            public DataRow(string _key = null)
            {
                Key = _key;
            }

            public DataField AddField(string _fieldName)
            {
                if (!fieldMap.ContainsKey(_fieldName))
                    fieldMap.Add(_fieldName, new DataField(_fieldName, null));
                return fieldMap[_fieldName];
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
                    bool isHave = fieldMap.ContainsKey(_fieldKey);
                    if (!isHave && value != null)
                        fieldMap.Add(_fieldKey, new DataField(_fieldKey, value));
                    else if(isHave && value != null)
                        fieldMap[_fieldKey].Value = value;
                    else if (isHave && value == null)
                        fieldMap.Remove(_fieldKey);
                }
            }

            public T TryGetValue<T>(string _fieldkey, object _defaultValue = null)
            {
                if (!fieldMap.ContainsKey(_fieldkey)) return _defaultValue == null ? default(T) : (T)_defaultValue;
                return fieldMap[_fieldkey].TryGetValue<T>(_defaultValue);
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
                    fieldMap.Add(tfield.Key, tfield);
                }
            }
            override public void Save(LitEngine.IO.AESWriter _writer)
            {
                _writer.WriteString(Key);
                Attribut.Save(_writer);
                List<DataField> tfields = new List<DataField>(fieldMap.Values);
                int tfieldCount = tfields.Count;
                _writer.WriteInt(tfieldCount);
                for (int i = 0; i < tfieldCount; i++)
                {
                    DataField tfield = tfields[i];
                    tfield.Save(_writer);
                }
            }
        }
    }
}

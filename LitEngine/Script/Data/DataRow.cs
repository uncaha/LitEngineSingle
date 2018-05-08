using System.Collections.Generic;
namespace LitEngine
{
    namespace Data
    {
        public sealed class DataRow : DataBaseElement
        {
            private Dictionary<string, DataField> fieldMap = new Dictionary<string, DataField>();

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

            public DataField this[string _fieldKey]
            {
                get
                {
                    if (!fieldMap.ContainsKey(_fieldKey)) return null;
                    return fieldMap[_fieldKey];
                }

                set
                {
                    if (!fieldMap.ContainsKey(_fieldKey))
                    {
                        fieldMap.Add(_fieldKey, value);
                    }
                    else
                    {
                        if (value != null)
                            fieldMap[_fieldKey] = value;
                        else
                            fieldMap.Remove(_fieldKey);
                    }
                }
            }

            public T TryGetValue<T>(string _fieldkey)
            {
                DataField tfield = this[_fieldkey];
                return tfield != null ? (T)tfield.Value : default(T);
            }

            public void Load(LitEngine.IO.AESReader _loader)
            {
                Key = _loader.ReadString();
                int tfieldCount = _loader.ReadInt32();
                for (int i = 0; i < tfieldCount; i++)
                {
                    DataField tfield = new DataField(null, null);
                    tfield.Load(_loader);
                    fieldMap.Add(tfield.Key, tfield);
                }
            }
            public void Save(LitEngine.IO.AESWriter _writer)
            {
                _writer.WriteString(Key);
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

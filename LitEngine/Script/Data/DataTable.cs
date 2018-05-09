using System.Collections.Generic;
namespace LitEngine
{
    namespace Data
    {
        public sealed class DataTable : DataBaseElement
        {
            private Dictionary<string, DataRow> rowMap = new Dictionary<string, DataRow>();

            public string TableName { get; private set; }
            public int RowCount { get { return rowMap.Count; } }
            public Dictionary<string, DataRow>.ValueCollection Rows { get { return rowMap.Values; } }
            public Dictionary<string, DataRow>.KeyCollection Keys { get { return rowMap.Keys; } }
            public DataTable(string _tableName = null)
            {
                TableName = _tableName;
            }

            public DataRow AddRow(string _rowName)
            {
                if (!rowMap.ContainsKey(_rowName))
                    rowMap.Add(_rowName, new DataRow(_rowName));
                return rowMap[_rowName];
            }

            public DataRow this[string _rowKey]
            {
                get
                {
                    if (!rowMap.ContainsKey(_rowKey)) return null;
                    return rowMap[_rowKey];
                }

                set
                {
                    if (!rowMap.ContainsKey(_rowKey))
                    {
                        rowMap.Add(_rowKey, value);
                    }
                    else
                    {
                        if (value != null)
                            rowMap[_rowKey] = value;
                        else
                            rowMap.Remove(_rowKey);
                    }
                }
            }

            public DataField SearchField(string _rowkey, string _fieldkey)
            {
                return this[_rowkey] != null ? this[_rowkey][_fieldkey] : null;
            }

            public T TryGetValue<T>(string _rowkey, string _fieldkey)
            {
                DataField tfield = SearchField(_rowkey, _fieldkey);
                return tfield != null ? tfield.TryGetValue<T>() : default(T);
            }

            override public void Load(LitEngine.IO.AESReader _loader)
            {
                TableName = _loader.ReadString();
                int trowCount = _loader.ReadInt32();
                for (int i = 0; i < trowCount; i++)
                {
                    DataRow trow = new DataRow();
                    trow.Load(_loader);
                    rowMap.Add(trow.Key, trow);
                }
            }
            override public void Save(LitEngine.IO.AESWriter _writer)
            {
                _writer.WriteString(TableName);
                List<DataRow> trowValues = new List<DataRow>(rowMap.Values);
                int trowCount = trowValues.Count;
                _writer.WriteInt(trowCount);
                for (int i = 0; i < trowCount; i++)
                {
                    DataRow trow = trowValues[i];
                    trow.Save(_writer);
                }
            }
        }
    }
}

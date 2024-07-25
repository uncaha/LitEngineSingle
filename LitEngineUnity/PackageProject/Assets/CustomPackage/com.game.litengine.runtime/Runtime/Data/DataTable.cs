using System.Collections.Generic;
namespace LitEngine
{
    namespace Data
    {
        public sealed class DataTable : DataBaseElement
        {
            private Dictionary<string, DataRow> rowMap = new Dictionary<string, DataRow>();

            public string TableName { get; private set; }
            public int RowCount { get { return rowList.Count; } }
            public Dictionary<string, DataRow>.KeyCollection Keys { get { return rowMap.Keys; } }
            private List<DataRow> rowList = new List<DataRow>();
            public DataTable(string _tableName = null)
            {
                TableName = _tableName;
            }

            public DataRow AddRow(string _rowName)
            {
                if (!rowMap.ContainsKey(_rowName))
                {
                    AddFromRow(new DataRow(_rowName));
                }
                
                return rowMap[_rowName];
            }
            public void AddFromRow(DataRow pRow)
            {
                if (!rowMap.ContainsKey(pRow.Key))
                {
                    rowMap.Add(pRow.Key, pRow);
                    rowList.Add(pRow);
                }
            }
            public void RemoveRow(string _rowName)
            {
                if (rowMap.ContainsKey(_rowName))
                {
                    DataRow trow = rowMap[_rowName];
                    rowMap.Remove(_rowName);
                    rowList.Remove(trow);
                }
                    
            }
            public DataRow this[int pIndex]
            {
                get
                {
                    return rowList[pIndex];
                }
            }

            public DataRow this[string _rowKey]
            {
                get
                {
                    if (!rowMap.ContainsKey(_rowKey)) return null;
                    return rowMap[_rowKey];
                }
            }

            override public void Load(LitEngine.IO.AESReader _loader)
            {
                TableName = _loader.ReadString();
                Attribut.Load(_loader);
                int trowCount = _loader.ReadInt32();
                for (int i = 0; i < trowCount; i++)
                {
                    DataRow trow = new DataRow();
                    trow.Load(_loader);
                    AddFromRow(trow);
                }
            }
            override public void Save(LitEngine.IO.AESWriter _writer)
            {
                _writer.WriteString(TableName);
                Attribut.Save(_writer);
                
                int trowCount = rowList.Count;
                _writer.WriteInt(trowCount);
                for (int i = 0; i < trowCount; i++)
                {
                    DataRow trow = rowList[i];
                    trow.Save(_writer);
                }
            }
        }
    }
}

using System.Collections.Generic;
using System.IO;
namespace LitEngine
{
    namespace Data
    {
        public interface DataBaseElement
        {
            void Load(LitEngine.IO.AESReader _loader);
            void Save(LitEngine.IO.AESWriter _writer);
        }

        public class DataBase
        {
            private const string cDatafile = "GameData.bytes";
            public static DataBase Data { get { if (dataInstance == null) dataInstance = new DataBase(); return dataInstance; } }
            private static DataBase dataInstance = null;

            private Dictionary<string, DataTable> tableMap = new Dictionary<string, DataTable>();
            private DataBase()
            {
                Load();
            }

            public DataTable AddTable(string _tableName)
            {
                if (!tableMap.ContainsKey(_tableName))
                    tableMap.Add(_tableName, new DataTable(_tableName));
                return tableMap[_tableName];
            }

            public DataTable this[string _tableName]
            {
                get
                {
                    if (!tableMap.ContainsKey(_tableName)) return null;
                    return tableMap[_tableName];
                }
                set
                {
                    if (!tableMap.ContainsKey(_tableName))
                    {
                        tableMap.Add(_tableName, value);
                    }
                    else
                    {
                        if (value != null)
                            tableMap[_tableName] = value;
                        else
                            tableMap.Remove(_tableName);
                    }
                }
            }
            #region getdata
            public DataRow SearchDataRow(string _table, string _rowkey)
            {
                DataTable ttable = this[_table];
                return ttable != null ? ttable[_rowkey] : null;
            }

            public DataField SearchField(string _table, string _rowkey, string _fieldkey)
            {
                DataRow trow = SearchDataRow(_table, _rowkey);
                return trow != null ? trow[_fieldkey] : null;
            }

            public T TryGetValue<T>(string _table, string _rowkey, string _fieldkey)
            {
                DataField tfield = SearchField(_table, _rowkey, _fieldkey);
                return tfield != null ? (T)tfield.Value : default(T);
            }
            #endregion

            #region load,save
            public void Load()
            {
                string tfullname = GameCore.AppPersistentAssetsPath + cDatafile;
                if (!File.Exists(tfullname)) return;

                LitEngine.IO.AESReader tloader = new LitEngine.IO.AESReader(tfullname);

                int ttableCount = tloader.ReadInt32();
                for (int i = 0; i < ttableCount; i++)
                {
                    DataTable ttable = new DataTable();
                    ttable.Load(tloader);
                    tableMap.Add(ttable.TableName, ttable);
                }
                tloader.Close();
            }
            public void Save()
            {
                string tfullname = GameCore.AppPersistentAssetsPath + cDatafile;
                LitEngine.IO.AESWriter twriter = new LitEngine.IO.AESWriter(tfullname);

                List<DataTable> ttableValues = new List<DataTable>(tableMap.Values);
                int ttableCount = ttableValues.Count;
                twriter.WriteInt(ttableCount);
                for (int i = 0; i < ttableCount; i++)
                {
                    DataTable ttable = ttableValues[i];
                    ttable.Save(twriter);
                }
                twriter.Flush();
                twriter.Close();
            }
            #endregion
        }
    }
}

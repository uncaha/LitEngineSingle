using System.Collections.Generic;
using System.IO;
namespace LitEngine
{
    namespace Data
    {
        public abstract class DataBaseElement
        {
            public class DataAttribute
            {
                private Dictionary<string, DataField> attributes = null;
                public object this[string keyParameter]
                {
                    get
                    {
                        if (attributes == null || !attributes.ContainsKey(keyParameter)) return null;
                        return attributes[keyParameter].Value;
                    }
                    set
                    {
                        if(attributes == null) attributes = new Dictionary<string, DataField>();
                        if (attributes.ContainsKey(keyParameter))
                        {
                            if (value == null)
                                attributes.Remove(keyParameter);
                            else
                                attributes[keyParameter].Value = value;
                        }
                        else 
                        {
                            if(value != null)
                                attributes.Add(keyParameter, new DataField(keyParameter,value));
                        }
                    }
                }

                public void Load(IO.AESReader _loader)
                {
                    int tattcount = _loader.ReadInt32();
                    if (tattcount > 0)
                    {
                        attributes = new Dictionary<string, DataField>();
                        for (int i = 0; i < tattcount; i++)
                        {
                            DataField tfield = new DataField(null, null);
                            tfield.Load(_loader);
                            attributes.Add(tfield.Key, tfield);
                        }
                    }
                }

                public void Save(IO.AESWriter _writer)
                {
                    int tattcount = attributes == null ? 0 : attributes.Count;
                    _writer.WriteInt(tattcount);
                    if (tattcount > 0)
                    {
                        foreach(KeyValuePair<string, DataField>  pair in attributes)
                        {
                            pair.Value.Save(_writer);
                        }
                    }
                }
            }
            public abstract void Load(LitEngine.IO.AESReader _loader);
            public abstract void Save(LitEngine.IO.AESWriter _writer);
            public DataAttribute Attribut { get; private set; }

            public DataBaseElement()
            {
                Attribut = new DataAttribute();
            }

            public T TryGetAttribute<T>(string keyParameter)
            {
                T ret;
                try
                {
                    object obj = Attribut[keyParameter];
                    checked
                    {
                        ret = obj == null ? default(T) : (T)obj;
                    } 
                }
                catch (System.Exception erro)
                {
                    DLog.LogError(erro.ToString());
                }
                return default(T);
            }
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
                return tfield != null ? tfield.TryGetValue<T>() : default(T);
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

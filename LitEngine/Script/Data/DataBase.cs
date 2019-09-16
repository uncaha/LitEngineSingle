using System.Collections.Generic;
using System.IO;
namespace LitEngine.Data
{
    public class DataBase
    {
        public string Error { get; private set; }
        private const string cDatafile = "GameData.bytes";
        public static DataBase Data { get { if (dataInstance == null) dataInstance = new DataBase(); return dataInstance; } }
        private static DataBase dataInstance = null;

        public Dictionary<string, DataTable>.KeyCollection Keys { get { return tableMap.Keys; } }
        public int TableCount { get { return tableList.Count; } }

        private Dictionary<string, DataTable> tableMap = new Dictionary<string, DataTable>();
        private List<DataTable> tableList = new List<DataTable>();
        private DataBase()
        {
            Load();
        }

        public DataTable AddTable(string _tableName)
        {
            if (!tableMap.ContainsKey(_tableName))
            {
                DataTable newtable = new DataTable(_tableName);
                AddFromTable(newtable);
            }
                
            return tableMap[_tableName];
        }

        public void AddFromTable(DataTable _table)
        {
            if (!tableMap.ContainsKey(_table.TableName))
            {
                tableMap.Add(_table.TableName, _table);
                tableList.Add(_table);
            }
                
        }

        public void RemoveTable(string _tableName)
        {
            if (tableMap.ContainsKey(_tableName))
            {
                DataTable ttable = tableMap[_tableName];
                tableMap.Remove(_tableName);
                tableList.Remove(ttable);
            }
        }

        public DataTable this[int pIndex]
        {
            get
            {
                return tableList[pIndex];
            }
        }

        public DataTable this[string _tableName]
        {
            get
            {
                if (!tableMap.ContainsKey(_tableName)) return null;
                return tableMap[_tableName];
            }
        }

        #region load,save
        public void Clear()
        {
            tableMap.Clear();
            tableList.Clear();
        }
        public void Load()
        {
            try
            {
                string tfullname = GameCore.AppPersistentAssetsPath + cDatafile;
                if (!File.Exists(tfullname)) return;
                Clear();
                LitEngine.IO.AESReader tloader = new LitEngine.IO.AESReader(tfullname);

                int ttableCount = tloader.ReadInt32();
                for (int i = 0; i < ttableCount; i++)
                {
                    DataTable ttable = new DataTable();
                    ttable.Load(tloader);
                    AddFromTable(ttable);
                }
                tloader.Close();
                Error = null;
            }
            catch (System.Exception _e)
            {
                Error = _e.ToString();
                Clear();
                DLog.LogError(_e.ToString());
            }

        }
        public void Save()
        {
            try
            {
                string tfullname = GameCore.AppPersistentAssetsPath + cDatafile;
                string tempfile = tfullname + ".temp";
                LitEngine.IO.AESWriter twriter = new LitEngine.IO.AESWriter(tfullname);
                int ttableCount = tableList.Count;
                twriter.WriteInt(ttableCount);
                for (int i = 0; i < ttableCount; i++)
                {
                    DataTable ttable = tableList[i];
                    ttable.Save(twriter);
                }
                twriter.Flush();
                twriter.Close();

                if(File.Exists(tfullname))
                {
                    File.Delete(tfullname);
                }
                File.Copy(tempfile,tfullname);
            }
            catch (System.Exception _e )
            {
                DLog.LogError(_e.ToString());
            }
           
        }
        #endregion
    }
}

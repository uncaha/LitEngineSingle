using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using Habby.SQL.Attribute;
using UnityEngine;
using Mono.Data.Sqlite;

namespace Habby.SQL
{
    public sealed class SQLDBObject
    {
        public const string PRIMARY = "PRIMARY KEY";
        public const string UNIQUE = "UNIQUE";
        public const string NOTNULL = "NOT NULL";
        public const string DEFAULT = "DEFAULT";
        

        #region staticMTD

        public static string GetDataPath(string databasePath)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                return $"URI=file:{databasePath}";
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return $"data source={databasePath}";
            }

            return $"data source={databasePath}";
        }

        #endregion

        private SqliteConnection SqlConnection;
        private SqliteCommand SqlCommand;

        public string DBName { get; private set; }
        public bool Inited { get; private set; }

        private readonly string DBFilePath;
        
        //name HabbySQLData.db
        public SQLDBObject(string pdbName)
        {
            DBName = pdbName;
            
            try
            {
                var dbFolder = $"{Application.persistentDataPath}/HabbyDB";
                DBFilePath = $"{dbFolder}/{DBName}";

                if (!Directory.Exists(dbFolder))
                {
                    Directory.CreateDirectory(dbFolder);
                }
            
                if (!File.Exists(DBFilePath))
                {
                    SqliteConnection.CreateFile(DBFilePath);
                }
                
                if (!OpenDB())
                {
                    if (File.Exists(DBFilePath))
                    {
                        File.Move(DBFilePath,DBFilePath + ".brokenFile");
                    }
                    SqliteConnection.CreateFile(DBFilePath);
                    OpenDB();
                }

                Inited = true;
            }
            catch (System.Exception e)
            {
                SQLLog.LogError(e.ToString());
            }
        }

        bool OpenDB()
        {
            try
            {
                SqlConnection = new SqliteConnection(GetDataPath(DBFilePath));
                SqlConnection.Open();
                SqlCommand = SqlConnection.CreateCommand();

                return true;
            }
            catch (Exception e)
            {
                SQLLog.LogError($"Open DB failed. err = {e}");
            }

            return false;
        }

        public void Close()
        {
            if (SqlCommand != null)
            {
                SqlCommand.Dispose();
                SqlCommand = null;
            }

            if (SqlConnection != null)
            {
                SqlConnection.Close();
                SqlConnection = null;
            }
        }

        public SqliteDataReader ExecuteReader(string command)
        {
            if (SqlCommand == null) return null;
            SqliteDataReader ret = null;
            try
            {
                SQLLog.Log("SQL:ExecuteReader " + command);
                SqlCommand.CommandText = command;
                ret = SqlCommand.ExecuteReader();
            }
            catch (Exception e)
            {
                SQLLog.LogError(e);
            }

            return ret;
        }

        public bool ExecuteNonQuery(string command)
        {
            if (SqlCommand == null) return false;
            try
            {
                SQLLog.Log("SQL:ExecuteNonQuery " + command);
                SqlCommand.CommandText = command;
                SqlCommand.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                SQLLog.LogError(e);
            }

            return false;
        }

        public bool ExecuteScalar(string command)
        {
            if (SqlCommand == null) return false;
            try
            {
                SQLLog.Log("SQL:ExecuteScalar " + command);
                SqlCommand.CommandText = command;
                var tfirst = SqlCommand.ExecuteScalar();
                if (tfirst == null) return false;
                int result = System.Convert.ToInt32(tfirst);
                SQLLog.Log($"result = " + result);
                return result > 0;
            }
            catch (Exception e)
            {
                SQLLog.LogError(e);
            }

            return false;
        }

        #region sql

        /// <summary>
        /// creat table
        /// </summary>
        /// <param name="pTableName">table name</param>
        /// <param name="pKeyTypeList"></param>
        public bool CreateTable(string pTableName, Dictionary<string,SQLTypeObject> pKeyMap)
        {
            // CREATE TABLE table_name(column1 type1, column2 type2, column3 type3,...);

            if (pKeyMap == null || pKeyMap.Count == 0) return false;
            try
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.Append("CREATE TABLE ");
                stringBuilder.Append(pTableName);
                stringBuilder.Append(" (");

                int i = 0;
                foreach (var cur in pKeyMap)
                {
                    if (i > 0)
                    {
                        stringBuilder.Append(",");
                    }
                    
                    var item = cur.Value;
                    stringBuilder.Append(item.key);
                    stringBuilder.Append(" ");
                    stringBuilder.Append(GetTypeString(item.type));

                    if (!string.IsNullOrEmpty(item.typeAdd))
                    {
                        stringBuilder.Append(" ");
                        stringBuilder.Append(item.typeAdd);
                    }

                    i++;
                }

                stringBuilder.Append(")");
                return ExecuteNonQuery(stringBuilder.ToString());
            }
            catch (Exception e)
            {
                SQLLog.LogError(e);
            }

            return false;
        }

        /// <summary>
        /// table map
        /// </summary>
        /// <param name="pTableName">table</param>
        /// <param name="pTypes">types</param>
        /// <returns></returns>
        public SQLTableData GetTableData(string pTableName, Dictionary<string,SQLTypeObject> pTypes)
        {
            SQLTableData ret = new SQLTableData();
            var tcmd = $"SELECT * FROM {pTableName}";
            var treader = ExecuteReader(tcmd);
            
            try
            {
                if (treader != null && treader.HasRows)
                {
                    while (treader.Read())
                    {
                        var tdata = new SQLRowData();
                        object tdatakey = null;
                        for (int i = 0, max = treader.FieldCount; i < max; i++)
                        {
                            var tfieldName = treader.GetName(i);
                            if(!pTypes.ContainsKey(tfieldName)) continue;
                            
                            var tkeyobj = pTypes[tfieldName];
                            object item = null;
                            bool tisDbNull = treader.IsDBNull(i);
                            try
                            {
                                switch (tkeyobj.type)
                                {
                                    case SQLDataType.int32:
                                        item = tisDbNull ? 0 : treader.GetInt32(i);
                                        break;
                                    case SQLDataType.int64:
                                        item = tisDbNull ? 0 : treader.GetInt64(i);
                                        break;
                                    case SQLDataType.single:
                                        item = tisDbNull ? 0f : treader.GetFloat(i);
                                        break;
                                    case SQLDataType.String:
                                        item = tisDbNull ? null : treader.GetString(i);
                                        break;
                                }
                            }
                            catch (Exception e)
                            {
                                SQLLog.LogError($"cast value failed. type = {tkeyobj.key}, error = {e.Message}");
                            }


                            tdata.Add(tkeyobj.key, item);
                            if (tkeyobj.IsMapKey)
                            {
                                tdatakey = item;
                            }
                        }

                        if (tdatakey != null && !ret.ContainsKey(tdatakey))
                        {
                            ret.Add(tdatakey, tdata);
                        }
                        else
                        {
                            SQLLog.LogError($"The same key = {tdatakey}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                SQLLog.LogError(e);
            }

            treader?.Close();
            return ret;
        }

        /// <summary>
        /// insert line
        /// </summary>
        /// <param name="pTableName">table name</param>
        /// <param name="pField">file dictionary</param>
        /// <returns></returns>
        public bool InsertRow(string pTableName, Dictionary<string, object> pField)
        {
            // INSERT INTO table_name(column1, column2, column3,...) VALUES(value1, value2, value3,...);
            if (pField == null || pField.Count == 0) return false;
            try
            {
                StringBuilder tcolumns = new StringBuilder();
                tcolumns.Append(" (");

                StringBuilder tvalues = new StringBuilder();
                tvalues.Append(" VALUES(");

                int i = 0;
                foreach (var col in pField)
                {
                    if (i > 0)
                    {
                        tcolumns.Append(",");
                        tvalues.Append(",");
                    }

                    tcolumns.Append(col.Key);
                    tvalues.Append(GetValueString(col.Value));
                    i++;
                }

                tcolumns.Append(")");
                tvalues.Append(")");
                string tcmd = $"INSERT INTO {pTableName}{tcolumns}{tvalues}";

                return ExecuteNonQuery(tcmd);
            }
            catch (Exception e)
            {
                SQLLog.LogError(e);
            }

            return false;
        }

        /// <summary>
        /// delete
        /// </summary>
        /// <param name="pTableName">table name</param>
        /// <param name="key">comparison key</param>
        /// <param name="pValue">comparison value</param>
        /// <returns></returns>
        public bool DeleteRow(string pTableName, string key, object pValue)
        {
            // DELETE FROM table_name WHERE some_column = some_value;

            string tcmd = $"DELETE FROM {pTableName} WHERE {key} = {GetValueString(pValue)}";
            return ExecuteNonQuery(tcmd);
        }
        
        /// <summary>
        /// delete list
        /// </summary>
        /// <param name="pTableName"></param>
        /// <param name="key"></param>
        /// <param name="pValues"></param>
        /// <returns></returns>
        public bool DeleteList(string pTableName, string key, List<object> pValues)
        {
            StringBuilder tcmdBuild = new StringBuilder();
            for (int i = 0,max = pValues.Count; i < max; i++)
            {
                if (i > 0)
                {
                    tcmdBuild.Append(",");
                }
                var item = pValues[i];
                tcmdBuild.Append(GetValueString(item));
            }
            
            string tcmd = $"DELETE FROM {pTableName} WHERE {key} in({tcmdBuild})";
            return ExecuteNonQuery(tcmd);
        }
        
        /// <summary>
        /// clear table
        /// </summary>
        /// <param name="pTableName">table name</param>
        /// <returns></returns>
        public bool DeleteAll(string pTableName)
        {
            string tcmd = $"DELETE FROM {pTableName}";
            return ExecuteNonQuery(tcmd);
        }

        /// <summary>
        /// update
        /// </summary>
        /// <param name="pTableName">table name</param>
        /// <param name="pKey">comparison key</param>
        /// <param name="pValue">comparison Value</param>
        /// <param name="pFields">update Fields</param>
        /// <returns></returns>
        public bool UpdateValues(string pTableName, string pKey, object pValue, Dictionary<string, object> pFields)
        {
            // UPDATE table_name SET column1 = value1, column2 = value2,... WHERE some_column = some_value;
            if (string.IsNullOrEmpty(pKey) || pValue == null) return false;
            try
            {
                StringBuilder tcolumns = new StringBuilder();
                tcolumns.Append(" ");
                int i = 0;
                foreach (var col in pFields)
                {
                    if (i > 0)
                    {
                        tcolumns.Append(",");
                    }

                    tcolumns.Append(col.Key);
                    tcolumns.Append("=");
                    tcolumns.Append(GetValueString(col.Value));

                    i++;
                }

                string tcmd = $"UPDATE {pTableName} SET {tcolumns} WHERE {pKey} = {GetValueString(pValue)}";

                return ExecuteNonQuery(tcmd);
            }
            catch (Exception e)
            {
                SQLLog.LogError(e);
            }

            return false;
        }

        public bool UpdateValue(string pTableName, string wherekey, object whereValue, string upKey, object upValue)
        {
            if (string.IsNullOrEmpty(wherekey) || whereValue == null) return false;
            if (string.IsNullOrEmpty(upKey) || upValue == null) return false;
            try
            {
                string tcmd =
                    $"UPDATE {pTableName} SET {upKey}={GetValueString(upValue)} WHERE {wherekey} = {GetValueString(whereValue)}";

                return ExecuteNonQuery(tcmd);
            }
            catch (Exception e)
            {
                SQLLog.LogError(e);
            }

            return false;
        }

        public object GetMaxField(string pTableName,SQLTypeObject pType)
        {
            if (pType == null) return null;
            object ret = null;
            var tcmd = $"SELECT * FROM {pTableName} order by {pType.key} desc limit 0,1";
            
            var treader = ExecuteReader(tcmd);
            try
            {
                if (treader != null && treader.HasRows)
                {
                    if (treader.Read())
                    {
                        var tdata = new SQLRowData();
                        for (int i = 0, max = treader.FieldCount; i < max; i++)
                        {
                            var tfieldName = treader.GetName(i);
                            if(!pType.key.Equals(tfieldName)) continue;
                            
                            bool tisDbNull = treader.IsDBNull(i);
                            try
                            {
                                switch (pType.type)
                                {
                                    case SQLDataType.int32:
                                        ret = tisDbNull ? 0 : treader.GetInt32(i);
                                        break;
                                    case SQLDataType.int64:
                                        ret = tisDbNull ? 0 : treader.GetInt64(i);
                                        break;
                                    case SQLDataType.single:
                                        ret = tisDbNull ? 0f : treader.GetFloat(i);
                                        break;
                                    case SQLDataType.String:
                                        ret = tisDbNull ? null : treader.GetString(i);
                                        break;
                                }
                            }
                            catch (Exception e)
                            {
                                SQLLog.LogError($"cast value failed. type = {pType.key}, error = {e.Message}");
                            }
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                SQLLog.LogError(e);
            }

            treader?.Close();

            return ret;
        }

        /// <summary>
        /// find
        /// </summary>
        /// <param name="pTableName">table name</param>
        /// <param name="key">comparison key</param>
        /// <param name="pValue">comparison Value</param>
        /// <returns></returns>
        public bool SelectRow(string pTableName,string key, object pValue)
        {
            var tcmd = $"SELECT * FROM {pTableName} WHERE {key}={GetValueString(pValue)}";
            var treader = ExecuteReader(tcmd);
            bool ret = treader != null ? treader.HasRows : false;
            treader?.Close();
            return ret;
        }

        /// <summary>
        /// find
        /// </summary>
        /// <param name="pTableName">table name</param>
        /// <param name="keys">comparison key</param>
        /// <returns></returns>
        public bool SelectCol(string pTableName, string[] keys)
        {
            StringBuilder keysBuilder = new StringBuilder();
            int i = 0;
            foreach (var curKey in keys)
            {
                if (i > 0)
                {
                    keysBuilder.Append(",");
                }

                keysBuilder.Append(curKey);
                i++;
            }

            var tcmd = $"SELECT {keysBuilder} FROM {pTableName}";
            var treader = ExecuteReader(tcmd);
            bool ret = treader != null ? treader.HasRows : false;
            treader?.Close();
            return ret;
        }

        /// <summary>
        /// Add col
        /// </summary>
        /// <param name="pTableName">table name</param>
        /// <param name="key">comparison key</param>
        /// <param name="pValueType">type</param>
        /// <returns></returns>
        public bool AddCol(string pTableName, string key, string pValueType)
        {
            //Alter table tableName add column type
            var tcmd = $"ALTER TABLE {pTableName} ADD {key} {pValueType}";
            return ExecuteNonQuery(tcmd);
        }

        /// <summary>
        /// table exits
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <returns></returns>
        public bool ExistTable(string pTableName)
        {
            // SELECT COUNT(*) FROM sqlite_master;
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("SELECT COUNT(*) FROM sqlite_master WHERE type='table' and name='");
                stringBuilder.Append(pTableName);
                stringBuilder.Append("';");
                return ExecuteScalar(stringBuilder.ToString());
            }
            catch (Exception e)
            {
                SQLLog.LogError(e);
            }

            return false;
        }

        #endregion

        public bool RenameTable(string pSorTableName,string pDestTableName)
        {
            if (string.IsNullOrEmpty(pSorTableName) || string.IsNullOrEmpty(pDestTableName)) return false;
            
            if (!ExistTable(pSorTableName)) return false;
            string tcmd = $"ALTER TABLE {pSorTableName} RENAME TO {pDestTableName}";
            return ExecuteNonQuery(tcmd);
        }
        
        public static string GetValueString(object pValue)
        {
            if (pValue == null) return "NULL";
            if (pValue is string) return $"'{pValue}'";

            return pValue.ToString();
        }

        public static string GetTypeString(SQLDataType pType)
        {
            //"CREATE TABLE book(_id INTEGER PRIMARY KEY AUTOINCREMENT, book_id TEXT NOT NULL DEFAULT \'0\', book_name TEXT NOT NULL);"
            //PRIMARY KEY NOT NULL
            switch (pType)
            {
                case SQLDataType.int32:
                case SQLDataType.int64:
                    return "integer";
                case SQLDataType.single:
                    return "real";
                case SQLDataType.String:
                    return "text ";
            }

            return "text";
        }
        
        public static SQLDataType GetSqlDataType(System.Type pType)
        {
            if (pType == typeof(Int32))
            {
                return SQLDataType.int32;
            }

            if (pType == typeof(Int64))
            {
                return SQLDataType.int64;
            }

            if (pType == typeof(float))
            {
                return SQLDataType.single;
            }

            return SQLDataType.String;
        }
        
        public static Dictionary<string,SQLTypeObject> GetTypeList(System.Type pType)
        {
            if (pType == null) return null;

            try
            {
                var ret = new Dictionary<string,SQLTypeObject>();
                var tfields = pType.GetFields();
                int tprimaryCount = 0;
                int tmapKeyCount = 0;
                SQLTypeObject tprimaryK = null;
                foreach (var cur in tfields)
                {
                    var ttype = new SQLTypeObject();
                    ttype.fieldInfo = cur;
                    ttype.key = cur.Name;
                    ttype.type = GetSqlDataType(cur.FieldType);

                    ttype.IsAutomatic = cur.GetCustomAttribute(typeof(Automatic)) != null;
                    ttype.IsMapKey = cur.GetCustomAttribute(typeof(MapKey)) != null;
                    ttype.IsPrimary = cur.GetCustomAttribute(typeof(Primary)) != null;
                    ttype.IsUnique = ttype.IsMapKey || cur.GetCustomAttribute(typeof(Unique)) != null;
                    ttype.IsNotNull = cur.GetCustomAttribute(typeof(NotNull))  != null;
                    ttype.defaultValue = (cur.GetCustomAttribute(typeof(DefaultValue)) as DefaultValue)?.fieldValue;
                    
                    if (ttype.IsPrimary)
                    {
                        tprimaryK = ttype;
                        tprimaryCount++;
                        if (tprimaryCount > 1)
                        {
                            SQLLog.LogError($"There are too many primary keys. type = {pType.Name}");
                            return null;
                        }
                    }

                    if (ttype.IsMapKey)
                    {
                        tmapKeyCount++;
                        if (tmapKeyCount > 1)
                        {
                            SQLLog.LogError($"There are too many mapkey keys. type = {pType.Name}");
                            return null;
                        }
                    }
                    
                    ret.Add(ttype.key,ttype);
                }
            
                if (tprimaryCount == 0)
                {
                    SQLLog.LogError($"There must be a primary key. type = {pType.Name}");
                    return null;
                }

                if (tmapKeyCount == 0)
                {
                    tprimaryK.IsMapKey = true;
                }

                return ret;
            }
            catch (Exception e)
            {
                SQLLog.LogError(e);
            }

            return null;
        }
    }
}
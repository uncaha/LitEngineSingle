using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using LitEngine.SQL.Attribute;
using UnityEngine;
namespace LitEngine.SQL
{
    public class DataDictionary<TValue>  : SQLTable where TValue : class, new()
    {
        public int Count => dataMap?.Count ?? 0;
        public string MainKey { get; private set; }
        protected TableData<TValue> dataMap = new TableData<TValue>();

        protected Dictionary<string, SQLTypeObject> keyMap = new Dictionary<string, SQLTypeObject>();
        protected DataDictionary(string pTableName, string pDBName) : base(pTableName, pDBName)
        {
            InitType();
            Init();
        }

        private void InitType()
        {
            keyMap = SQLDBObject.GetTypeList(typeof(TValue));

            if (keyMap == null) return;

            foreach (var item in keyMap)
            {
                if (item.Value.IsMapKey)
                {
                    MainKey = item.Value.key;
                    break;
                }
            }
        }
        private void Init()
        {
            if (keyMap == null) return;
            if (!DB.ExistTable(tableName))
            {
                bool created = DB.CreateTable(tableName, keyMap);
                SQLLog.Log($"created = {created}");
            }
            else
            {
                LoadData();
            }
        }
        

        protected void LoadData()
        {
            if (keyMap == null || keyMap.Count == 0) return;
            try
            {
                dataMap.Clear();
                var tdatamap = DB.GetTableData(tableName, keyMap);
                tdatamap.ConverToTable(dataMap);
            }
            catch (Exception e)
            {
                SQLLog.LogError(e);
            }
        }

        public List<object> GetKeys()
        {
            var ret = new List<object>();
            ret.AddRange(dataMap.Keys);
            return ret;
        }
        
        public List<TValue> GetRows()
        {
            var ret = new List<TValue>();
            ret.AddRange(dataMap.Values);
            return ret;
        }
        
        public bool ExitsRow(object pRow)
        {
            return dataMap.ContainsKey(pRow);
        }

        public TValue GetRow(object pRow)
        {
            if (!dataMap.ContainsKey(pRow)) return null;
            return dataMap[pRow];
        }

        public bool GetDataKey(TValue pData,out object pKey)
        {
            pKey = null;

            if (!keyMap.ContainsKey(MainKey))
            {
                SQLLog.LogError("cant found mainkey in keyMap.");
                return false;
            }
            var ttype = keyMap[MainKey];
            var tinfo = ttype.fieldInfo;
            
            pKey = tinfo.GetValue(pData);

            if (pKey == null)
            {
                SQLLog.LogError($"{ttype.key} must not null.");
                return false;
            }

            return true;
        }
        public void Add(TValue pData)
        {
            try
            {
                if (!GetDataKey(pData, out object tkey)) return;
                if (tkey == null) return;
                if (DB.InsertRow(tableName, ConverToSQLFields(pData)))
                {
                    dataMap.Add(tkey, pData);
                }
            }
            catch (Exception e)
            {
                SQLLog.LogError(e);
            }
        }

        public void AddOrUpdate(TValue pData)
        {
            try
            {
                if (!GetDataKey(pData, out object tkey)) return;
                if (tkey == null) return;
                if (dataMap.ContainsKey(tkey))
                {
                    if (DB.UpdateValues(tableName, MainKey, tkey, ConverToSQLFields(pData)))
                    {
                        dataMap[tkey] = pData;
                    }
                }
                else
                {
                    if (DB.InsertRow(tableName, dataMap.ConverToRowData(pData)))
                    {
                        dataMap.Add(tkey, pData);
                    }
                }
            }
            catch (Exception e)
            {
                SQLLog.LogError(e);
            }
        }

        public object GetMaxKey()
        {
            var tkey = keyMap[MainKey];
            return DB.GetMaxField(tableName, tkey);
        }

        public void UpdateRowValues(object pRow, string[] pKeyList)
        {
            if (!dataMap.ContainsKey(pRow)) return;
            var tobj = dataMap[pRow];
            var tudpateFields = new Dictionary<string, object>();

            for (int i = 0, max = pKeyList.Length; i < max; i++)
            {
                var tkey = pKeyList[i];
                if(!keyMap.ContainsKey(tkey)) continue;
                var ttype = keyMap[tkey];
                if(ttype.IsAutomatic || ttype.IsMapKey) continue;
                
                var tfield = ttype.fieldInfo;
                if (tfield == null) continue;
                var tvalue = tfield.GetValue(tobj);
                
                tudpateFields.Add(tkey, tvalue);
            }

            if (tudpateFields.Count == 0)
            {
                SQLLog.Log("nothing can update.");
                return;
            }

            if (DB.UpdateValues(tableName, MainKey, pRow, tudpateFields))
            {
            }
        }
        
        public void UpdateRowValue(object pRow, string pKey)
        {
            try
            {
                if (!dataMap.ContainsKey(pRow) || !keyMap.ContainsKey(pKey)) return;
                
                var ttype = keyMap[pKey];
                if (ttype.IsAutomatic || ttype.IsMapKey)
                {
                    SQLLog.LogError("cant change mainkey or automatic.");
                    return;
                }
                
                var tobj = dataMap[pRow];
                var tfield = ttype.fieldInfo;
                if (tfield == null)
                {
                    SQLLog.LogError($"cant found field. key = {pKey}");
                    return;
                }

                var tvalue = tfield.GetValue(tobj);

                if (DB.UpdateValue(tableName, MainKey, pRow, pKey, tvalue))
                {
                    
                }
            }
            catch (Exception e)
            {
                SQLLog.LogError(e);
            }
        }


        public void Remove(object pRow)
        {
            if (!dataMap.ContainsKey(pRow)) return;
            if (DB.DeleteRow(tableName, MainKey, pRow))
            {
                dataMap.Remove(pRow);
            }
        }

        public void RemoveList(List<object> pKeys)
        {
            var tvalueList = new List<object>();
            for (int i = 0, max = pKeys.Count; i < max; i++)
            {
                var tkey = pKeys[i];
                if (!dataMap.ContainsKey(tkey)) continue;
                tvalueList.Add(tkey);
            }

            if (DB.DeleteList(tableName, MainKey, tvalueList))
            {
                for (int i = 0, max = tvalueList.Count; i < max; i++)
                {
                    var tkey = tvalueList[i];
                    dataMap.Remove(tkey);
                }
            }
        }

        public void Clear()
        {
            dataMap?.Clear();
            DB.DeleteAll(tableName);
        }
        
        public Dictionary<string,object> ConverToSQLFields(TValue pData)
        {
            if (pData == null) return null;
            var ret = new Dictionary<string,object>();
            
            foreach (var cur in keyMap)
            {
                var ttype = cur.Value;
                var tinfo = ttype.fieldInfo;
                var tvalue = tinfo.GetValue(pData);
                if (ttype.IsAutomatic) continue;
                ret.Add(cur.Key, tvalue);
            }

            return ret;
        }
    }
}
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using LitEngine.SQL.Attribute;

namespace LitEngine.SQL
{
    public class TableData<TValue>
    {
        public int Count => mapList.Count;
        private ConcurrentDictionary<object, TValue> mapList = new ConcurrentDictionary<object, TValue>();

        public TValue this[object pKey]
        {
            get
            {
                return mapList[pKey];
            }
            set
            {
                mapList[pKey] = value;
            }
        }

        public ICollection<object> Keys
        {
            get
            {
                return mapList.Keys;
            }
        }
        
        public ICollection<TValue> Values
        {
            get
            {
                return mapList.Values;
            }
        }

        public void Add(object pKey,TValue pValue)
        {
            mapList.TryAdd(pKey,pValue);
        }

        public void Clear()
        {
            mapList.Clear();
        }

        public TValue Remove(object pKey)
        {
            if (!mapList.ContainsKey(pKey)) return default(TValue);
            if (mapList.TryRemove(pKey, out TValue rm))
                return rm;
            return default(TValue);
        }

        public bool ContainsKey(object pKey)
        {
            return mapList.ContainsKey(pKey);
        }
        
        public SQLTableData ConverToSQLTable(string pTableName)
        {
            var ret = new SQLTableData();
            var ttypesList = SQLDBObject.GetTypeList(typeof(TValue));

            foreach (var cur in mapList)
            {
                var titem = ConverToRowData(cur.Value);
                ret.Add(cur.Key,titem);
            }

            return ret;
        }

        public SQLRowData ConverToRowData(TValue pData)
        {
            if (pData == null) return null;
            var ret = new SQLRowData();

            var ttypes = typeof(TValue).GetFields();

            foreach (var cur in ttypes)
            {
                var tvalue = cur.GetValue(pData);
                ret.Add(cur.Name, tvalue);
            }

            return ret;
        }
    }
}
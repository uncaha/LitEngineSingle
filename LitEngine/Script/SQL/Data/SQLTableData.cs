using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Habby.SQL
{
    public class SQLTableData
    {
        public int Count => mapList.Count;
        public ConcurrentDictionary<object, SQLRowData> mapList { get; private set; } = new ConcurrentDictionary<object, SQLRowData>();

        public SQLRowData this[object pKey]
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
        
        public ICollection<SQLRowData> Values
        {
            get
            {
                return mapList.Values;
            }
        }

        public void Add(object pKey,SQLRowData pValue)
        {
            mapList.TryAdd(pKey,pValue);
        }

        public void Clear()
        {
            mapList.Clear();
        }

        public void Remove(object pKey)
        {
            mapList.TryRemove(pKey,out SQLRowData rm);
        }

        public bool ContainsKey(object pKey)
        {
            return mapList.ContainsKey(pKey);
        }
        

        public TableData<T> ConverToTable<T>(TableData<T> pTable = null) where T : class, new()
        {
            var ret = pTable ?? new TableData<T>();
            ret.Clear();
            foreach (var cur in mapList)
            {
                var titem = ConverToObject<T>(cur.Value);
                ret.Add(cur.Key,titem);
            }

            return ret;
        }
        
        public T ConverToObject<T>(SQLRowData pRowData) where T : class, new()
        {
            T ret = new T();

            var ttypes = typeof(T).GetFields();
            
            foreach (var cur in ttypes)
            {
                if(!pRowData.ContainsKey(cur.Name)) continue;
                cur.SetValue(ret,pRowData[cur.Name]);
            }

            return ret;
        }
    }
}
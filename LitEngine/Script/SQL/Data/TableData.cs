using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Habby.SQL.Attribute;

namespace Habby.SQL
{
    public class TableData<TValue>
    {
        public int Count => mapList.Count;
        private StableDictionary<object, TValue> mapList = new StableDictionary<object, TValue>();

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
        
        public List<TValue> Values
        {
            get
            {
                return mapList.Values;
            }
        }

        public void Add(object pKey,TValue pValue)
        {
            mapList.Add(pKey,pValue);
        }

        public void Clear()
        {
            mapList.Clear();
        }

        public TValue Remove(object pKey)
        {
            if (!mapList.ContainsKey(pKey)) return default(TValue);
            var rm = mapList[pKey];

            return rm;
        }

        public bool ContainsKey(object pKey)
        {
            return mapList.ContainsKey(pKey);
        }
        
        public SQLTableData ConverToSQLTable(string pTableName)
        {
            var ret = new SQLTableData();
            var ttypesList = SQLDBObject.GetTypeList(typeof(TValue));
            var tkeylist = new List<object>(mapList.Keys);
            foreach (var curKey in tkeylist)
            {
                var tvalue = mapList[curKey];
                var titem = ConverToRowData(tvalue);
                ret.Add(curKey,titem);
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
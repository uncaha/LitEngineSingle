
using System;
using System.Collections.Generic;

namespace Habby.SQL
{
    public class SQLRowData : Dictionary<string, object>
    {
        public T GetValue<T>(string pKey)
        {
            if (!ContainsKey(pKey)) return default;
            try
            {
                T ret = (T) this[pKey];
                return ret;
            }
            catch (Exception e)
            {
                SQLLog.Log($" key = {pKey}, error = {e}");
            }

            return default;
        }
    }
}
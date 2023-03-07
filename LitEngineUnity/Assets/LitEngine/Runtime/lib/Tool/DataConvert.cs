using System.Collections.Generic;
using Newtonsoft.Json;

namespace LitEngine.Tool
{
    public class DataConvert
    {
        public static T FromJson<T>(string pJson)
        {
            if (string.IsNullOrEmpty(pJson)) return default(T);
            try
            {
                if (typeof(T) == typeof(string)) return default(T);
                T ret = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(pJson);
                return ret;
            }
            catch (System.Exception error)
            {
                DLog.LogError("DataConvert", $"FromJson:error={error.Message}, json={pJson}");
            }
            return default(T);
        }
        
        public static T FromDictionary<T>(Dictionary<string, object> pDic)
        {
            if (pDic == null || pDic.Count == 0) return default(T);
            try
            {
                var tjson = ToJson(pDic);
                if (tjson == null) return default(T);;
                
                return FromJson<T>(tjson);
            }
            catch (System.Exception error)
            {
                DLog.LogError("DataConvert",$"FromDictionary:error={error.Message}, Dic={pDic}");
            }
            return default(T);
        }
        
        public static Dictionary<string, object> ToDictionary(object pData)
        {
            if (pData == null) return null;
            try
            {
                
                var tjson = ToJson(pData);
                if (tjson == null) return null;
                
                return FromJson<Dictionary<string, object>>(tjson);
            }
            catch (System.Exception error)
            {
                DLog.LogError("DataConvert",$"ToDictionary:error={error.Message}, pData = {pData}");
            }
            return null;
        }
        
        public static string ToJson(object pData)
        {
            if (pData == null) return null;
            try
            {
                if (pData is string) return (string)pData;

                var setting = new JsonSerializerSettings();
                setting.NullValueHandling = NullValueHandling.Ignore;

                return Newtonsoft.Json.JsonConvert.SerializeObject(pData, null, setting);
            }
            catch (System.Exception error)
            {
                DLog.LogError("DataConvert",$"ToJson: error={error.Message}, Json={pData}");
            }
            return null;
        }

        public static void MergeFromJson(object tarObj, string pJson)
        {
            if (string.IsNullOrEmpty(pJson)) return;
            try
            {
                Newtonsoft.Json.JsonConvert.PopulateObject(pJson, tarObj);
            }
            catch (System.Exception error)
            {
                DLog.LogError("DataConvert",$"MergeFromJson:error={error.Message}, Json = {pJson}");
            }

        }
        
        public static T FromJsonNoLog<T>(string pJson)
        {
            if (string.IsNullOrEmpty(pJson)) return default(T);
            try
            {
                if (typeof(T) == typeof(string)) return default(T);
                T ret = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(pJson);
                return ret;
            }
            catch
            {
                // ignored
            }

            return default(T);
        }
        
        public static T[] ConvertToArray<T>(object pObject)
        {
            if (pObject == null) return null;
            try
            {
                var tjarray = (Newtonsoft.Json.Linq.JArray) pObject;
                return tjarray.ToObject<T[]>();
            }
            catch (System.Exception error)
            {
                DLog.LogError("DataConvert",error);
            }

            return null;
        }
    }
}
using System;
using LitEngine.LType;
using System.Reflection;
namespace LitEngine.Script.Data
{
    public static class DataObject
    {
        public static void SaveToData(this object pTar,string pTableName)
        {
            Type ttype = pTar.GetType();
            FieldInfo[] tpis = ttype.GetFields(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < tpis.Length; i++)
            {
                FieldInfo tinfo = tpis[i];
                object tvalue = tinfo.GetValue(pTar);
                bool ishave = System.Enum.IsDefined(typeof(FieldType), tinfo.FieldType.Name);
            }
        }
    }
}

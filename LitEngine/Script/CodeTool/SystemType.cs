using System;
using System.Collections.Generic;
using System.Reflection;
namespace LitEngine.CodeTool
{
    public interface IBaseType
    {
        Type TypeForCLR { get; }
        string Name { get; }
        string Tag { get; }
    }

    public class SystemType : IBaseType
    {
        public SystemType(Type _clrtype)
        {
            clrType = _clrtype;
        }
        Type clrType;
        IBaseType[] mFieldTypes = null;
        public IBaseType[] FieldTypes
        {
            get
            {
                if (mFieldTypes == null)
                {
                    FieldInfo[] tpis = clrType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                    mFieldTypes = new IBaseType[tpis.Length];
                    for (int i = 0; i < tpis.Length; i++)
                    {
                        mFieldTypes[i] = new SystemType(tpis[i].FieldType);
                    }
                }
                return mFieldTypes;
            }
        }
        public Type TypeForCLR
        {
            get
            {
                return clrType;
            }
        }
        public string Name { get; set; }
        public string Tag { get; set; }
    }

}

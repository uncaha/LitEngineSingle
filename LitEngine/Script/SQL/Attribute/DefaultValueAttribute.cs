using System;

namespace LitEngine.SQL.Attribute
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class DefaultValue : System.Attribute
    {
        public object fieldValue;
        public DefaultValue(object pValue)
        {
            fieldValue = pValue;
        }
    }
}
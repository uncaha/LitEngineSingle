using System;

namespace Habby.SQL.Attribute
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
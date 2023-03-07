using System;

namespace Habby.SQL.Attribute
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class Unique : System.Attribute
    {
        
    }
}
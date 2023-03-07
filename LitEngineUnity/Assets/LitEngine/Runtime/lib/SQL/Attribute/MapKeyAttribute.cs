using System;
namespace LitEngine.SQL.Attribute
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class MapKey : System.Attribute
    {
        
    }
}
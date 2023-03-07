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
}
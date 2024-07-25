#if ILRuntime
using System;
using ILRuntime.CLR.TypeSystem;
using LitEngine.CodeTool;

namespace LitEngine.CodeTool
{
    public class ILRType : IBaseType
    {
        public ILRType(string pName,ILType pType)
        {
            Name = pName;
            Type = pType;
            TypeForCLR = Type.TypeForCLR;
        }
    
        public ILType Type { get; private set; }
        public Type TypeForCLR { get; private set; }
        public string Name { get; private set; }
        public string Tag { get; private set; }
    }
}


#endif
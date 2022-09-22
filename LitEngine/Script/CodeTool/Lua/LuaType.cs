using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LitEngine.CodeTool
{
    public class LuaType : IBaseType
    {
        Type clrType;
        public LuaType(string pLuaName)
        {
            Name = pLuaName;
            clrType = null;
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

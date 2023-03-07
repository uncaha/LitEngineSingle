using System.Reflection;
using System.Text;

namespace LitEngine.SQL
{
    public enum SQLDataType
    {
        int32 = 0,
        int64,
        single,
        String,
    }
    public class SQLTypeObject
    {
        public string key;
        public SQLDataType type = SQLDataType.String;
        public object defaultValue;

        internal bool IsAutomatic;
        internal bool IsMapKey;
        internal bool IsPrimary;
        internal bool IsUnique;
        internal bool IsNotNull;

        internal FieldInfo fieldInfo;
        private string _typeAdd;
        public string typeAdd {
            get
            {
                if (_typeAdd != null) return _typeAdd;
                
                StringBuilder tbuild = new StringBuilder();

                bool tisstr = type == SQLDataType.String;
                
                
                if (IsPrimary)
                {
                    tbuild.Append(SQLDBObject.PRIMARY);
                    tbuild.Append(" ");
                }
                else if (IsUnique)
                {
                    tbuild.Append(SQLDBObject.UNIQUE);
                    tbuild.Append(" ");
                }

                if (IsNotNull || IsPrimary || IsUnique || !tisstr)
                {
                    tbuild.Append(SQLDBObject.NOTNULL);
                    tbuild.Append(" ");
                }

                if (!IsUnique && defaultValue != null)
                {
                    tbuild.Append(SQLDBObject.DEFAULT);
                    tbuild.Append(" ");
                    tbuild.Append(SQLDBObject.GetValueString(defaultValue));
                }
                

                _typeAdd = tbuild.ToString();
                return _typeAdd;
            }
        }
        

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LitEngine.ValueType
{
    public struct LInt
    {
        public int baseValue { get; private set; }
        public string StringValue { get; private set; }
        public static LInt Parse(string s)
        {
            LInt ret = new LInt();
            ret.StringValue = s;
            ret.baseValue = int.Parse(s);
            return ret;
        }

        public override string ToString()
        {
            return StringValue;
        }

        public bool Equals(LInt obj)
        {
            return baseValue == obj.baseValue;
        }

        public int CompareTo(LInt _value)
        {
            if (baseValue == _value.baseValue)
                return 0;
            else if (baseValue > _value.baseValue)
                return 1;
            else
                return -1;
        }

        #region 操作符
        public static LInt operator +(LInt lhs, LInt rhs)
        {
            LInt ret = new LInt();
            ret.baseValue = lhs.baseValue + rhs.baseValue;
            ret.StringValue = ret.baseValue.ToString();
            return ret;
        }

        public static LInt operator +(LInt lhs, int rhs)
        {
            LInt ret = new LInt();
            ret.baseValue = lhs.baseValue + rhs;
            ret.StringValue = ret.baseValue.ToString();
            return ret;
        }

        public static LInt operator +(int lhs, LInt rhs)
        {
            LInt ret = new LInt();
            ret.baseValue = lhs + rhs.baseValue;
            ret.StringValue = ret.baseValue.ToString();
            return ret;
        }

        public static LInt operator -(LInt lhs, LInt rhs)
        {
            LInt ret = new LInt();
            ret.baseValue = lhs.baseValue - rhs.baseValue;
            ret.StringValue = ret.baseValue.ToString();
            return ret;
        }

        public static LInt operator -(LInt lhs, int rhs)
        {
            LInt ret = new LInt();
            ret.baseValue = lhs.baseValue - rhs;
            ret.StringValue = ret.baseValue.ToString();
            return ret;
        }

        public static LInt operator -(int lhs, LInt rhs)
        {
            LInt ret = new LInt();
            ret.baseValue = lhs - rhs.baseValue;
            ret.StringValue = ret.baseValue.ToString();
            return ret;
        }
        #endregion

    }
}

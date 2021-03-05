using System;
namespace LitEngine.Value
{
    public static class FixedExtend
    {
        public static Fixed ToFixed(this Int32 i)
        {
            return new Fixed(i);
        }
        public static Fixed ToFixed(this float f)
        {
            return new Fixed(f);
        }
        public static VectorFixed2 ToVectorFixed2(this UnityEngine.Vector2 v2)
        {
            return new VectorFixed2(v2.x, v2.y);
        }

        public static VectorFixed3 ToVectorFixed3(this UnityEngine.Vector3 v3)
        {
            return new VectorFixed3(v3.x, v3.y, v3.z);
        }

        public static Fixed ToFixedRotation(this UnityEngine.Quaternion rotation)
        {
            return -rotation.eulerAngles.y.ToFixed();
        }

    }

    [Serializable]
    public struct Fixed
    {
        /// <summary>
        /// 小数占用位数
        /// </summary>
        public static int Fix_Fracbits = 16;
        public static Fixed Zero = new Fixed(0);
        public static Fixed One = new Fixed(1);
        public Int64 bits;

        public Fixed(int x)
        {
            bits = (x << Fix_Fracbits);
        }
        public Fixed(float x)
        {
            bits = (Int64)((x) * (1 << Fix_Fracbits));
        }
        public Fixed(Int64 x)
        {
            bits = ((x) * (1 << Fix_Fracbits));
        }
        public Int64 GetValue()
        {
            return bits;
        }
        public Fixed SetValue(Int64 i)
        {
            bits = i;
            return this;
        }
        public static Fixed Lerp(Fixed a, Fixed b, float t)
        {
            return a + (b - a) * t;
        }
        public static Fixed Lerp(Fixed a, Fixed b, Fixed t)
        {
            return a + (b - a) * t;
        }

        public static Fixed RotationLerp(Fixed a, Fixed b, Fixed t)
        {
            while (a < 0)
            {
                a += 360;
            }
            while (b < 0)
            {
                b += 360;
            }
            var offset1 = b - a;
            var offset2 = b - (a + 360);
            return a + t * (offset1.Abs() < offset2.Abs() ? offset1 : offset2);
        }
        public Fixed Abs()
        {

            return Fixed.Abs(this);
        }
        public Fixed Sqrt()
        {
            return Fixed.Sqrt(this);
        }

        public static Fixed Range(Fixed n, int min, int max)
        {
            if (n < min) n = new Fixed(min);
            if (n > max) n = new Fixed(max);
            return n;
        }


        #region 重载+
        public static Fixed operator +(Fixed p1, Fixed p2)
        {
            Fixed tmp;
            tmp.bits = p1.bits + p2.bits;
            return tmp;
        }
        public static Fixed operator +(Fixed p1, int p2)
        {
            Fixed tmp;
            tmp.bits = p1.bits + (Int64)(p2 << Fix_Fracbits);
            return tmp;
        }
        public static Fixed operator +(int p1, Fixed p2)
        {
            return p2 + p1;
        }
        public static Fixed operator +(Fixed p1, Int64 p2)
        {
            Fixed tmp;
            tmp.bits = p1.bits + p2 << Fix_Fracbits;
            return tmp;
        }
        public static Fixed operator +(Int64 p1, Fixed p2)
        {
            return p2 + p1;
        }

        public static Fixed operator +(Fixed p1, float p2)
        {
            Fixed tmp;
            tmp.bits = p1.bits + (Int64)(p2 * (1 << Fix_Fracbits));
            return tmp;
        }
        public static Fixed operator +(float p1, Fixed p2)
        {
            Fixed tmp = p2 + p1;
            return tmp;
        }
        #endregion
        #region 重载-
        public static Fixed operator -(Fixed p1, Fixed p2)
        {
            Fixed tmp;
            tmp.bits = p1.bits - p2.bits;
            return tmp;
        }

        public static Fixed operator -(Fixed p1, int p2)
        {
            Fixed tmp;
            tmp.bits = p1.bits - (Int64)(p2 << Fix_Fracbits);
            return tmp;
        }

        public static Fixed operator -(int p1, Fixed p2)
        {
            Fixed tmp;
            tmp.bits = (p1 << Fix_Fracbits) - p2.bits;
            return tmp;
        }
        public static Fixed operator -(Fixed p1, Int64 p2)
        {
            Fixed tmp;
            tmp.bits = p1.bits - (p2 << Fix_Fracbits);
            return tmp;
        }
        public static Fixed operator -(Int64 p1, Fixed p2)
        {
            Fixed tmp;
            tmp.bits = (p1 << Fix_Fracbits) - p2.bits;
            return tmp;
        }

        public static Fixed operator -(float p1, Fixed p2)
        {
            Fixed tmp;
            tmp.bits = (Int64)(p1 * (1 << Fix_Fracbits)) - p2.bits;
            return tmp;
        }
        public static Fixed operator -(Fixed p1, float p2)
        {
            Fixed tmp;
            tmp.bits = p1.bits - (Int64)(p2 * (1 << Fix_Fracbits));
            return tmp;
        }
        #endregion
        #region 重载*
        public static Fixed operator *(Fixed p1, Fixed p2)
        {
            Fixed tmp;
            tmp.bits = ((p1.bits) * (p2.bits)) >> (Fix_Fracbits);
            return tmp;
        }

        public static Fixed operator *(int p1, Fixed p2)
        {
            Fixed tmp;
            tmp.bits = p1 * p2.bits;
            return tmp;
        }
        public static Fixed operator *(Fixed p1, int p2)
        {
            return p2 * p1;
        }
        public static Fixed operator *(Fixed p1, float p2)
        {
            Fixed tmp;
            tmp.bits = (Int64)(p1.bits * p2);
            return tmp;
        }
        public static Fixed operator *(float p1, Fixed p2)
        {
            Fixed tmp;
            tmp.bits = (Int64)(p1 * p2.bits);
            return tmp;
        }
        #endregion
        #region 重载\
        public static Fixed operator /(Fixed p1, Fixed p2)
        {
            Fixed tmp;
            if (p2 == Fixed.Zero)
            {
                UnityEngine.Debug.LogWarning("/0");
                tmp.bits = Zero.bits;
            }
            else
            {
                tmp.bits = (p1.bits) * (1 << Fix_Fracbits) / (p2.bits);
            }
            return tmp;
        }
        public static Fixed operator /(Fixed p1, int p2)
        {
            Fixed tmp;
            if (p2 == 0)
            {
                UnityEngine.Debug.LogError("/0");
                tmp.bits = Zero.bits;
            }
            else
            {
                tmp.bits = p1.bits / (p2);
            }
            return tmp;
        }
        public static Fixed operator %(Fixed p1, int p2)
        {
            Fixed tmp;
            if (p2 == 0)
            {
                UnityEngine.Debug.LogError("/0");
                tmp.bits = Zero.bits;
            }
            else
            {
                tmp.bits = (p1.bits % (p2 << Fix_Fracbits));
            }
            return tmp;
        }
        public static Fixed operator /(int p1, Fixed p2)
        {
            Fixed tmp;
            if (p2 == Zero)
            {
                UnityEngine.Debug.LogError("/0");
                tmp.bits = Zero.bits;
            }
            else
            {
                Int64 tmp2 = ((Int64)p1 << Fix_Fracbits << Fix_Fracbits);
                tmp.bits = tmp2 / (p2.bits);
            }
            return tmp;
        }
        public static Fixed operator /(Fixed p1, Int64 p2)
        {
            Fixed tmp;
            if (p2 == 0)
            {
                UnityEngine.Debug.LogError("/0");
                tmp.bits = Zero.bits;
            }
            else
            {
                tmp.bits = p1.bits / (p2);
            }
            return tmp;
        }
        public static Fixed operator /(Int64 p1, Fixed p2)
        {
            Fixed tmp;
            if (p2 == Zero)
            {
                UnityEngine.Debug.LogError("/0");
                tmp.bits = Zero.bits;
            }
            else
            {
                if (p1 > Int32.MaxValue || p1 < Int32.MinValue)
                {
                    tmp.bits = 0;
                    return tmp;
                }
                tmp.bits = (p1 << Fix_Fracbits) / (p2.bits);
            }
            return tmp;
        }
        public static Fixed operator /(float p1, Fixed p2)
        {
            Fixed tmp;
            if (p2 == Zero)
            {
                UnityEngine.Debug.LogError("/0");
                tmp.bits = Zero.bits;
            }
            else
            {
                Int64 tmp1 = (Int64)p1 * ((Int64)1 << Fix_Fracbits << Fix_Fracbits);
                tmp.bits = (tmp1) / (p2.bits);
            }
            return tmp;
        }
        public static Fixed operator /(Fixed p1, float p2)
        {
            Fixed tmp;
            if (p2 > -0.000001f && p2 < 0.000001f)
            {
                UnityEngine.Debug.LogError("/0");
                tmp.bits = Zero.bits;
            }
            else
            {
                tmp.bits = (p1.bits << Fix_Fracbits) / ((Int64)(p2 * (1 << Fix_Fracbits)));
            }
            return tmp;
        }
        #endregion

        #region 重载><
        public static bool operator >(Fixed p1, Fixed p2)
        {
            return (p1.bits > p2.bits) ? true : false;
        }
        public static bool operator <(Fixed p1, Fixed p2)
        {
            return (p1.bits < p2.bits) ? true : false;
        }
        public static bool operator <=(Fixed p1, Fixed p2)
        {
            return (p1.bits <= p2.bits) ? true : false;
        }
        public static bool operator >=(Fixed p1, Fixed p2)
        {
            return (p1.bits >= p2.bits) ? true : false;
        }
        public static bool operator !=(Fixed p1, Fixed p2)
        {
            return (p1.bits != p2.bits) ? true : false;
        }
        public static bool operator ==(Fixed p1, Fixed p2)
        {
            return (p1.bits == p2.bits) ? true : false;
        }

        public static bool operator >(Fixed p1, float p2)
        {
            return (p1.bits > (p2 * (1 << Fix_Fracbits))) ? true : false;
        }
        public static bool operator <(Fixed p1, float p2)
        {
            return (p1.bits < (p2 * (1 << Fix_Fracbits))) ? true : false;
        }
        public static bool operator <=(Fixed p1, float p2)
        {
            return (p1.bits <= p2 * (1 << Fix_Fracbits)) ? true : false;
        }
        public static bool operator >=(Fixed p1, float p2)
        {
            return (p1.bits >= p2 * (1 << Fix_Fracbits)) ? true : false;
        }
        public static bool operator !=(Fixed p1, float p2)
        {
            return (p1.bits != p2 * (1 << Fix_Fracbits)) ? true : false;
        }
        public static bool operator ==(Fixed p1, float p2)
        {
            return (p1.bits == p2 * (1 << Fix_Fracbits)) ? true : false;
        }
        #endregion

        public static Fixed Sqrt(Fixed p1)
        {
            Fixed tmp;
            Int64 ltmp = p1.bits * (1 << Fix_Fracbits);
            tmp.bits = (Int64)Math.Sqrt(ltmp);
            return tmp;
        }
       

        public static bool Equals(Fixed p1, Fixed p2)
        {
            return (p1.bits == p2.bits) ? true : false;
        }

        public bool Equals(Fixed right)
        {
            if (bits == right.bits)
            {
                return true;
            }
            return false;
        }

        override public bool Equals(object right)
        {
            if(right is Fixed)
            {
                return this.Equals((Fixed)right);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static Fixed Max()
        {
            Fixed tmp;
            tmp.bits = Int64.MaxValue;
            return tmp;
        }

        public static Fixed Max(Fixed p1, Fixed p2)
        {
            return p1.bits > p2.bits ? p1 : p2;
        }
        public static Fixed Min(Fixed p1, Fixed p2)
        {
            return p1.bits < p2.bits ? p1 : p2;
        }

        public static Fixed Clamp(Fixed min, Fixed max, Fixed v)
        {
            if (v.bits < min.bits) return new Fixed() { bits = min.bits };
            if (v.bits > max.bits) return new Fixed() { bits = max.bits };
            return v;
        }

        public static Fixed Clamp(float min, float max, Fixed v)
        {
            return Clamp(new Fixed(min), new Fixed(max),v);
        }

        public static Fixed Clamp01(Fixed v)
        {
            return Clamp(Fixed.Zero, Fixed.One, v);
        }

        public static Fixed Precision()
        {
            Fixed tmp;
            tmp.bits = 1;
            return tmp;
        }

        public static Fixed MaxValue()
        {
            Fixed tmp;
            tmp.bits = Int64.MaxValue;
            return tmp;
        }
        public static Fixed Abs(Fixed P1)
        {
            Fixed tmp;
            tmp.bits = Math.Abs(P1.bits);
            return tmp;
        }
        public static Fixed operator -(Fixed p1)
        {
            Fixed tmp;
            tmp.bits = -p1.bits;
            return tmp;
        }

        public float ToFloat()
        {
            return bits / (float)(1 << Fix_Fracbits);
        }
        public UnityEngine.Quaternion ToUnityRotation()
        {
            return UnityEngine.Quaternion.Euler(0, -this.ToFloat(), 0);
        }
        public int ToInt()
        {
            return (int)(bits >> (Fix_Fracbits));
        }
        public override string ToString()
        {
            double tmp = (double)bits / (double)(1 << Fix_Fracbits);
            return tmp.ToString("f4");
        }
    }
}

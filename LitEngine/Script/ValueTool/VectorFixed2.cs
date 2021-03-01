using UnityEngine;
namespace LitEngine.Value
{
    public struct VectorFixed2
    {

        public Fixed x;
        public Fixed y;

        #region 属性
        public VectorFixed2 normalized
        {

            get
            {
                if (x == 0 && y == 0)
                {
                    return VectorFixed2.zero;
                }
                Fixed n = ((x * x) + (y * y)).Sqrt();

                var result = new VectorFixed2(x / n, y / n);
                result.x = Fixed.Range(result.x, -1, 1);
                result.y = Fixed.Range(result.y, -1, 1);
                return result;
            }
        }
        public Fixed magnitude
        {
            get
            {
                if (x == 0 && y == 0)
                {
                    return Fixed.Zero;
                }
                Fixed n = ((x * x) + (y * y)).Sqrt();
                return n;
            }
        }

        public static VectorFixed2 left = new VectorFixed2(-1, 0);
        public static VectorFixed2 right = new VectorFixed2(1, 0);
        public static VectorFixed2 up = new VectorFixed2(0, 1);
        public static VectorFixed2 down = new VectorFixed2(0, -1);
        public static VectorFixed2 zero = new VectorFixed2(0, 0);
        public static VectorFixed2 one = new VectorFixed2(1, 1);

        #endregion

        public VectorFixed2(float x, float y)
        {
            this.x = new Fixed(x);
            this.y = new Fixed(y);
        }

        public VectorFixed2(int x, int y)
        {
            this.x = new Fixed(x);
            this.y = new Fixed(y);
        }

        public VectorFixed2(Fixed x, Fixed y)
        {
            this.x = x;
            this.y = y;
        }

        public VectorFixed2(Vector2 v2)
        {
            this.x = new Fixed(v2.x);
            this.y = new Fixed(v2.y);
        }

        #region 方法

        public static VectorFixed2 FromVector2(Vector2 v2)
        {
            return new VectorFixed2(v2);
        }

        public Vector2 ToVector2()
        {
            return new Vector2(x.ToFloat(), y.ToFloat());
        }

        public static VectorFixed2 FromRatio(Fixed ratio)
        {
            return new VectorFixed2(MathFixed.CosAngle(ratio), MathFixed.SinAngle(ratio));
        }

        public static bool DistanceLess(VectorFixed2 a, VectorFixed2 b, Fixed len)
        {
            var xLen = a.x - b.x;
            var yLen = a.y - b.y;
            return (xLen * xLen + yLen * yLen) < len * len;
        }

        public VectorFixed2 Rotate(Fixed value)
        {
            Fixed tx, ty;
            tx = MathFixed.CosAngle(value) * x - y * MathFixed.SinAngle(value);
            ty = MathFixed.CosAngle(value) * y + x * MathFixed.SinAngle(value);
            return new VectorFixed2(tx, ty);
        }

        public Fixed ToRotation()
        {
            if (x == 0 && y == 0)
            {
                return new Fixed();
            }
            Fixed sin = this.normalized.y;
            Fixed result = Fixed.Zero;
            if (this.x >= 0)
            {
                result = MathFixed.Asin(sin) / MathFixed.PI * 180;
            }
            else
            {
                result = MathFixed.Asin(-sin) / MathFixed.PI * 180 + 180;
            }

            return result;
        }

        public Fixed Dot(VectorFixed2 b)
        {
            return Dot(this, b);
        }
        public static Fixed Dot(VectorFixed2 a, VectorFixed2 b)
        {
            return a.x * b.x + b.y * a.y;
        }

        public override string ToString()
        {
            return "{" + x.ToString() + "," + y.ToString() + "}";// + ":" + ToVector3().ToString();
        }
        #endregion


        #region 重载
        public static VectorFixed2 operator +(VectorFixed2 a, VectorFixed2 b)
        {
            return new VectorFixed2(a.x + b.x, a.y + b.y);
        }
        public static VectorFixed2 operator -(VectorFixed2 a, VectorFixed2 b)
        {
            return new VectorFixed2(a.x - b.x, a.y - b.y);
        }
        public static VectorFixed2 operator -(VectorFixed2 a)
        {
            return new VectorFixed2(-a.x, -a.y);
        }
        public static VectorFixed2 operator *(VectorFixed2 a, Fixed b)
        {
            return new VectorFixed2(a.x * b, a.y * b);
        }
        public static VectorFixed2 operator *(VectorFixed2 a, int b)
        {
            return new VectorFixed2(a.x * b, a.y * b);
        }
        public static VectorFixed2 operator *(VectorFixed2 a, float b)
        {
            return new VectorFixed2(a.x * b, a.y * b);
        }

        #endregion

    }
}

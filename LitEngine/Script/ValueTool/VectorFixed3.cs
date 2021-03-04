
using UnityEngine;
namespace LitEngine.Value
{
    [System.Serializable]
    public struct VectorFixed3
    {
        public Fixed x;
        public Fixed y;
        public Fixed z;

        #region 属性
        public static VectorFixed3 left = new VectorFixed3(-1, 0);
        public static VectorFixed3 right = new VectorFixed3(1, 0);
        public static VectorFixed3 up = new VectorFixed3(0, 1);
        public static VectorFixed3 down = new VectorFixed3(0, -1);
        public static VectorFixed3 zero = new VectorFixed3(0, 0, 0);
        public static VectorFixed3 one = new VectorFixed3(1, 1, 1);
        public static VectorFixed3 forward = new VectorFixed3(0, 0, 1);
        #endregion

        public VectorFixed3(int x = 0, int y = 0, int z = 0)
        {
            this.x = new Fixed(x);
            this.y = new Fixed(y);
            this.z = new Fixed(z);
        }
        public VectorFixed3(float x, float y, float z)
        {
            this.x = new Fixed(x);
            this.y = new Fixed(y);
            this.z = new Fixed(z);
        }
        public VectorFixed3(Fixed x, Fixed y, Fixed z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public VectorFixed3(Vector3 v3)
        {
            this.x = new Fixed(v3.x);
            this.y = new Fixed(v3.y);
            this.z = new Fixed(v3.z);
        }
        public VectorFixed3(Vector2 v2)
        {
            this.x = new Fixed(v2.x);
            this.y = new Fixed(v2.y);
            this.z = new Fixed(0f);
        }
        public Vector3 ToVector3()
        {
            return new Vector3(x.ToFloat(), y.ToFloat(), z.ToFloat());
        }

        public static VectorFixed3 FromVector3(Vector3 v3)
        {
            return new VectorFixed3(v3);
        }

        #region 重载
        public static VectorFixed3 operator +(VectorFixed3 a, VectorFixed3 b)
        {
            return new VectorFixed3(a.x + b.x, a.y + b.y, a.z + b.z);
        }
        public static VectorFixed3 operator +(VectorFixed3 a, Vector3 b)
        {
            return new VectorFixed3(a.x + b.x, a.y + b.y, a.z + b.z);
        }
        public static VectorFixed3 operator -(VectorFixed3 a, VectorFixed3 b)
        {
            return new VectorFixed3(a.x - b.x, a.y - b.y, a.z - b.z);
        }
        public static VectorFixed3 operator -(VectorFixed3 a, Vector3 b)
        {
            return new VectorFixed3(a.x - b.x, a.y - b.y, a.z - b.z);
        }
        public static VectorFixed3 operator -(VectorFixed3 a)
        {
            return new VectorFixed3(-a.x, -a.y, -a.z);
        }
        public static VectorFixed3 operator *(VectorFixed3 a, Fixed b)
        {
            return new VectorFixed3(a.x * b, a.y * b, a.z * b);
        }
        public static VectorFixed3 operator *(VectorFixed3 a, int b)
        {
            return new VectorFixed3(a.x * b, a.y * b, a.z * b);
        }
        public static VectorFixed3 operator *(VectorFixed3 a, float b)
        {
            return new VectorFixed3(a.x * b, a.y * b, a.z * b);
        }

        public static VectorFixed3 operator *(VectorFixed3 a, VectorFixed3 b)
        {
            return new VectorFixed3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
        }
        public static VectorFixed3 operator *(VectorFixed3 a, Vector3 b)
        {
            return new VectorFixed3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
        }
        #endregion

        #region 属性
        public VectorFixed3 normalized
        {
            get
            {
                var result = new VectorFixed3(x, y, z);
                result.Normalized();
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
                Fixed n = ((x * x) + (y * y) + (z * z)).Sqrt();
                return n;
            }
        }
        #endregion
        #region 方法
        public void Normalized()
        {
            if (x == 0 && y == 0 && z == 0) return;

            Fixed n = ((x * x) + (y * y) + (z * z)).Sqrt();

            x = Fixed.Range(x / n, -1, 1);
            y = Fixed.Range(y / n, -1, 1);
            z = Fixed.Range(z / n, -1, 1);
        }

        public Fixed Distance(VectorFixed3 b)
        {
            return (this - b).magnitude;
        }
        public override string ToString()
        {
            return string.Format("({0},{1},{2})", x, y, z);
        }
        #endregion

        #region static
        public static VectorFixed3 Lerp(VectorFixed3 a, VectorFixed3 b, Fixed p)
        {
            Fixed tp = Fixed.Clamp01(p);
            return a + (b - a) * tp;
        }
        public static Fixed Dot(VectorFixed3 a, VectorFixed3 b)
        {
            return a.x * b.x + b.y * a.y + a.z * b.z;
        }
        public static VectorFixed3 Cross(VectorFixed3 a, VectorFixed3 b)
        {
            return a * b;
        }
        #endregion
    }
}

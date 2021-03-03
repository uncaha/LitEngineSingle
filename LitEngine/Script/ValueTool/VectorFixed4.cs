
using UnityEngine;

namespace LitEngine.Value
{
    [System.Serializable]
    public struct VectorFixed4
    {
        public Fixed x;
        public Fixed y;
        public Fixed z;
        public Fixed w;

        #region 属性
        public static VectorFixed4 identity { get; private set; } = new VectorFixed4(Quaternion.identity);
        public Fixed moldLength
        {
            get
            {
                Fixed n = ((x * x) + (y * y) + (z * z) + (w * w)).Sqrt();
                return n;
            }
        }
        public VectorFixed4 normalized
        {
            get
            {
                Fixed n = ((x * x) + (y * y) + (z * z) + (w * w)).Sqrt();
                if (n == 0) return new VectorFixed4(Quaternion.identity);
                return new VectorFixed4(x / n, y / n, z / n, w / n);
            }
        }
        #endregion

        public VectorFixed4(int x = 0, int y = 0, int z = 0, int w = 0)
        {
            this.x = new Fixed(x);
            this.y = new Fixed(y);
            this.z = new Fixed(z);
            this.w = new Fixed(w);
        }
        public VectorFixed4(float x, float y, float z, float w)
        {
            this.x = new Fixed(x);
            this.y = new Fixed(y);
            this.z = new Fixed(z);
            this.w = new Fixed(w);
        }
        public VectorFixed4(Fixed x, Fixed y, Fixed z, Fixed w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
        public VectorFixed4(Quaternion v4)
        {
            this.x = new Fixed(v4.x);
            this.y = new Fixed(v4.y);
            this.z = new Fixed(v4.z);
            this.w = new Fixed(v4.w);
        }

      

        #region 重载
        public static VectorFixed4 operator +(VectorFixed4 a, VectorFixed4 b)
        {
            return new VectorFixed4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
        }
        public static VectorFixed4 operator -(VectorFixed4 a, VectorFixed4 b)
        {
            return new VectorFixed4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
        }
        public static VectorFixed4 operator -(VectorFixed4 a)
        {
            return new VectorFixed4(-a.x, -a.y, -a.z, -a.w);
        }
        public static VectorFixed4 operator /(VectorFixed4 a, Fixed b)
        {
            return new VectorFixed4(a.x / b, a.y / b, a.z / b, a.w / b);
        }
        public static VectorFixed4 operator *(VectorFixed4 a, Fixed b)
        {
            return new VectorFixed4(a.x * b, a.y * b, a.z * b, a.w * b);
        }
        public static VectorFixed4 operator *(VectorFixed4 a, int b)
        {
            return new VectorFixed4(a.x * b, a.y * b, a.z * b, a.w * b);
        }
        public static VectorFixed4 operator *(VectorFixed4 a, float b)
        {
            return new VectorFixed4(a.x * b, a.y * b, a.z * b, a.w * b);
        }
        public static VectorFixed4 operator *(VectorFixed4 a, VectorFixed4 rq)
        {
            return new  VectorFixed4(a.w * rq.x + a.x * rq.w + a.y * rq.z - a.z * rq.y,
                                     a.w * rq.y + a.y * rq.w + a.z * rq.x - a.x * rq.z,
                                     a.w * rq.z + a.z * rq.w + a.x * rq.y - a.y * rq.x,
                                     a.w * rq.w - a.x * rq.x - a.y * rq.y - a.z * rq.z);
        }


        #endregion

    
        #region static
        public static VectorFixed4 FromQuaternion(Quaternion v4)
        {
            return new VectorFixed4(v4);
        }

        public static Fixed Dot(VectorFixed4 a, VectorFixed4 b)
        {
            return a.x * b.x + b.y * a.y + a.z * b.z + a.w * b.w;
        }
        public static VectorFixed4 Euler(VectorFixed3 euler)
        {
            VectorFixed4 ret = new VectorFixed4();
            ret.FromEuler(euler);
            return ret;
        }

        public static VectorFixed4 Inverse(VectorFixed4 rotation)
        {
            return rotation.GetConjugate();
        }

        public static VectorFixed4 LookRotation(Vector3 forward)
        {
            var tv = new VectorFixed4(Quaternion.identity);
            tv.RotationTo(forward);
            return tv;
        }

        public static VectorFixed4 GetV4FromAxis(VectorFixed3 v,Fixed angle)
        {
            VectorFixed4 ret = new VectorFixed4();
            ret.FromAxis(v,angle);
            return ret;
        }

        public static VectorFixed4 Lerp(VectorFixed4 from,VectorFixed4 to,Fixed t)
        {
            VectorFixed4 ret = from * (1 - t) + to * t;
            Fixed tm = ret.moldLength;
            ret = ret / tm;
            return ret;
        }

        #endregion

        #region 方法     

        public void FromEuler(VectorFixed3 euler)
        {
            Fixed p = euler.x * MathFixed.PIover90;
            Fixed y = euler.y * MathFixed.PIover90;
            Fixed r = euler.z * MathFixed.PIover90;

            Fixed sinp = MathFixed.Sin(p);
            Fixed siny = MathFixed.Sin(y);
            Fixed sinr = MathFixed.Sin(r);
            Fixed cosp = MathFixed.Cos(p);
            Fixed cosy = MathFixed.Cos(y);
            Fixed cosr = MathFixed.Cos(r);

            x = sinr * cosp * cosy - cosr * sinp * siny;
            y = cosr * sinp * cosy + sinr * cosp * siny;
            z = cosr * cosp * siny - sinr * sinp * cosy;
            w = cosr * cosp * cosy + sinr * sinp * siny;

            Normalized();
        }

        public void FromAxis(VectorFixed3 v,Fixed angle)
        {
            Fixed sinAngle;
            angle /= 2;
            VectorFixed3 vn = v.normalized;

            sinAngle = MathFixed.SinAngle(angle);

            x = (vn.x * sinAngle);
            y = (vn.y * sinAngle);
            z = (vn.z * sinAngle);
            w = MathFixed.CosAngle(angle);
        }
        public void Normalized()
        {
            if (x == 0 && y == 0 && z == 0 && w == 0) return;
            Fixed n = ((x * x) + (y * y) + (z * z) + (w * w)).Sqrt();
            x /= n;
            y /= n;
            z /= n;
            w /= n;
        }
        public Quaternion ToQuaternion()
        {
            return new Quaternion(x.ToFloat(), y.ToFloat(), z.ToFloat(), w.ToFloat());
        }

        VectorFixed4 GetConjugate()
        {
            return new VectorFixed4(-x, -y, -z, w);
        }

        public void RotationTo(Vector3 forward)
        {
            var tn = forward.normalized;

            VectorFixed4 vecQuat = new VectorFixed4(tn.x,tn.y,tn.z,0f);
            VectorFixed4 resQuat = vecQuat * GetConjugate();
            resQuat = this * resQuat;
            this.x = resQuat.x;
            this.y = resQuat.y;
            this.z = resQuat.z;
            this.w = resQuat.w;
        }

        public override string ToString()
        {
            return string.Format("{{0},{1},{2},{3}}",x,y,z,w);
        }
        #endregion
    }
}


using UnityEngine;

namespace LitEngine.Value
{
    public class VectorFixed4
    {
        public Fixed x;
        public Fixed y;
        public Fixed z;
        public Fixed w;

        #region 属性
        #endregion

        public VectorFixed4(int x = 0, int y = 0, int z = 0, int w = 0)
        {
            this.x = new Fixed(x);
            this.y = new Fixed(y);
            this.z = new Fixed(z);
        }
        public VectorFixed4(float x, float y, float z, float w)
        {
            this.x = new Fixed(x);
            this.y = new Fixed(y);
            this.z = new Fixed(z);
        }
        public VectorFixed4(Fixed x, Fixed y, Fixed z, Fixed w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public VectorFixed4(Quaternion v4)
        {
            this.x = new Fixed(v4.x);
            this.y = new Fixed(v4.y);
            this.z = new Fixed(v4.z);
            this.w = new Fixed(v4.w);
        }

        public Quaternion ToQuaternion()
        {
            return new Quaternion(x.ToFloat(), y.ToFloat(), z.ToFloat(), w.ToFloat());
        }

        public static VectorFixed4 FromQuaternion(Quaternion v4)
        {
            return new VectorFixed4(v4);
        }
    }
}

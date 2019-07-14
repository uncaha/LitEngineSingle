using UnityEngine;
namespace LitEngine.Tool
{
    public class Helper
    {
        static public void SetLayer(Transform _trans, int _layer)
        {
            _trans.gameObject.layer = _layer;
            if (_trans.childCount > 0)
            {
                foreach (Transform child in _trans)
                    SetLayer(child, _layer);
            }
        }
        static public bool IsTrueFloat(float ft)
        {
            return !float.IsNaN(ft) && !float.IsInfinity(ft);
        }

        static public Vector2 Vector2Format(string _str)
        {
            if (string.IsNullOrEmpty(_str)) return Vector2.zero;
            _str = _str.Substring(1, _str.Length - 2);
            string[] tvecarr = _str.Split(',');
            Vector2 ret;
            ret.x = float.Parse(tvecarr[0]);
            ret.y = float.Parse(tvecarr[1]);
            return ret;
        }

        static public Vector3 Vector3Format(string _str)
        {
            if (string.IsNullOrEmpty(_str)) return Vector3.zero;
            _str = _str.Substring(1, _str.Length - 2);
            string[] tvecarr = _str.Split(',');
            Vector3 ret;
            ret.x = float.Parse(tvecarr[0]);
            ret.y = float.Parse(tvecarr[1]);
            ret.z = float.Parse(tvecarr[2]);

            return ret;
        }

        static public Quaternion QuaternionFormat(string _str)
        {
            if (string.IsNullOrEmpty(_str)) return Quaternion.identity;
            _str = _str.Substring(1, _str.Length - 2);
            string[] tvecarr = _str.Split(',');
            Quaternion ret;
            ret.x = float.Parse(tvecarr[0]);
            ret.y = float.Parse(tvecarr[1]);
            ret.z = float.Parse(tvecarr[2]);
            ret.w = float.Parse(tvecarr[3]);
            return ret;
        }

        static public string GetNumberKM(float _number, string _format = "f0", bool _useStr = true)
        {
            int num = 1000; //byte

            if (_number < num)
                return _number.ToString();
            else if (_number < System.Math.Pow(num, 2))
                return (_number / num).ToString(_format) + (_useStr ? "K" : ""); //kb
            else if (_number < System.Math.Pow(num, 3))
                return (_number / System.Math.Pow(num, 2)).ToString(_format) + (_useStr ? "M" : ""); //M
            else if (_number < System.Math.Pow(num, 4))
                return (_number / System.Math.Pow(num, 3)).ToString(_format) + (_useStr ? "G" : ""); //G
            return (_number / System.Math.Pow(num, 4)).ToString(_format) + (_useStr ? "T" : ""); //T
        }

        static public long GetUnixTime()
        {
            return (System.DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }

        static public int GetBitCount(long _res)
        {
            int ret = 0;
            while (_res != 0)
            {
                _res &= (_res - 1);
                ret++;
            }
            return ret;
        }

        #region bezier
        public static void MakeBezierPointsToArray(Vector3[] refArray, int offsetIndex, int length, Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent)
        {
            int tcount = length + offsetIndex;
            for (int i = offsetIndex + 1; i <= tcount; i++)
            {
                float t = i / (float)tcount;
                Vector3 pixel = CalculateCubicBezierPoint(t, startPosition, startTangent, endTangent, endPosition);
                refArray[i - 1] = pixel;
            }

        }

        public static Vector3[] MakeBezierPoints(Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent, int division)
        {
            Vector3[] ret = new Vector3[division];
            for (int i = 1; i <= division; i++)
            {
                float t = i / (float)division;
                Vector3 pixel = CalculateCubicBezierPoint(t, startPosition, startTangent, endTangent, endPosition);
                ret[i - 1] = pixel;
            }

            return ret;
        }

        public static void CalculateCubicBezierPointToVec3(ref Vector3 tovec, float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * p0;
            p += 3 * uu * t * p1;
            p += 3 * u * tt * p2;
            p += ttt * p3;
        }

        public static Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * p0;
            p += 3 * uu * t * p1;
            p += 3 * u * tt * p2;
            p += ttt * p3;
            return p;
        }
        #endregion

        #region material
        public enum ShaderPropertyType
        {
            kfloat,
            kcolor,
            ktexture,
        }

        static public void CopyProperty(Material _sor, Material _des, string _property, ShaderPropertyType _type)
        {
            if (!_des.HasProperty(_property) || !_sor.HasProperty(_property)) return;
            switch (_type)
            {
                case ShaderPropertyType.kfloat:
                    _des.SetFloat(_property, _sor.GetFloat(_property));
                    break;
                case ShaderPropertyType.kcolor:
                    _des.SetColor(_property, _sor.GetColor(_property));
                    break;
                case ShaderPropertyType.ktexture:
                    _des.SetTexture(_property, _sor.GetTexture(_property));
                    break;
                default:
                    break;
            }
        }
        #endregion

    }

}

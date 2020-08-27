using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using System.Reflection;

namespace LitEngine
{

    public class OutObject<T, M>
    {
        public T mObject;
        public M mReturnValue;
        public OutObject()
        {
        }
    }

    public class ScriptTool
    {
        #region mathfun
        static public Vector3 QAndV3(Quaternion rotation, Vector3 point)
        {
            return rotation * point;
        }

        static public Quaternion QAndQ(Quaternion lhs, Quaternion rhs)
        {
            return lhs * rhs;
        }
        static public Vector3 FAndV3(float fl, Vector3 v3)
        {
            return fl * v3;
        }

        static public float V3Dot(Vector3 v1, Vector3 v2)
        {
            return Vector3.Dot(v1, v2);
        }

        static public float Acos(float angle)
        {
            return (float)System.Math.Acos(angle);
        }
        static public int MoveLeft(int _obj, int _pos)
        {
            return _obj << _pos;
        }
        static public int MoveRight(int _obj, int _pos)
        {
            return _obj >> _pos;
        }
        static public int AAndB(int _a, int _b)
        {
            return _a & _b;
        }
        static public int AOrB(int _a, int _b)
        {
            return _a | _b;
        }
        static public bool IsBiger(float a, float b)
        {
            return a > b;
        }

        static public void GCCollect()
        {
            GC.Collect();
        }
        #endregion


        #region compiler
        static public void CompilerDll(string _filename, string _args)
        {
            try
            {
                System.Diagnostics.Process tprocess = new System.Diagnostics.Process();
                tprocess.StartInfo.FileName = _filename;
                tprocess.StartInfo.Arguments = _args;

                tprocess.StartInfo.RedirectStandardOutput = true;
                tprocess.BeginOutputReadLine();
                tprocess.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(SortOutputHandler);
                tprocess.Start();

                tprocess.WaitForExit();
                tprocess.CancelOutputRead();
                tprocess.Close();
            }
            catch (System.Exception ex)
            {
                DLog.LogError(ex.Message);
            }
        }

        public static void SortOutputHandler(object sendingProcess, System.Diagnostics.DataReceivedEventArgs outLine)
        {
           DLog.Log( outLine.Data);
            // Collect the sort command output.
            if (!string.IsNullOrEmpty(outLine.Data))
            {
            }
        }
        #endregion

        #region outfun
        static public OutObject<T, M> GetOutTypeStruct<T, M>(object _object, string _methodname, int _pos, params object[] _parmas)
        {
            OutObject<T, M> ret = new OutObject<T, M>();
            Type ttype = _object.GetType();
            if (_parmas != null)
            {
                Type[] ttypes = new Type[_parmas.Length];

                for (int i = 0; i < _parmas.Length; i++)
                {
                    if (i != _pos)
                        ttypes[i] = _parmas[i].GetType();
                    else
                        ttypes[i] = _parmas[i].GetType().MakeByRefType();
                }
                MethodInfo methodinfo = ttype.GetMethod(_methodname, ttypes);
                ret.mReturnValue = (M)methodinfo.Invoke(_object, _parmas);
                ret.mObject = (T)_parmas[_pos];
            }
            return ret;
        }
        static public OutObject<RaycastHit, bool> RayCast(Vector3 origin, Vector3 direction)
        {
            OutObject<RaycastHit, bool> ret = new OutObject<RaycastHit, bool>();

            RaycastHit thit;
            ret.mReturnValue = Physics.Raycast(origin, direction, out thit);
            ret.mObject = thit;

            return ret;

        }

        static public OutObject<Color, bool> StringToColorRGBA(string _str)
        {
            OutObject<Color, bool> ret = new OutObject<Color, bool>();
            ret.mReturnValue = ColorUtility.TryParseHtmlString(_str, out ret.mObject);
            return ret;
        }

        #endregion

        static public void IsWheelTouchGrount(OutObject<RaycastHit, bool> _obj, Vector3 origin, Vector3 direction)
        {
            if (_obj == null) return;
            RaycastHit thit;
            _obj.mReturnValue = Physics.Raycast(origin, direction, out thit);
            _obj.mObject = thit;
        }

        static public ArrayList GetCompents<T>(Transform _trans)
        {
            if (_trans == null) return null;
            ArrayList ret = new ArrayList();
            ChoseCompents<T>(ref ret, _trans);
            return ret;
        }

        static private void ChoseCompents<T>(ref ArrayList _list, Transform _trans)
        {
            T comp = _trans.GetComponent<T>();
            if (comp != null)
                _list.Add(comp);
            foreach (Transform tchild in _trans)
            {
                ChoseCompents<T>(ref _list, tchild);
            }
        }

        static public T GetTypeObject<T>(object _obj)
        {
            return (T)_obj;
        }
        static public Type GetType<T>()
        {
            return typeof(T);
        }
        static public Type GetType(object _obj)
        {
            return _obj.GetType();
        }
        static public bool IsNull(UnityEngine.Object _obj)
        {
            if (_obj == null)
                return true;
            return false;
        }

        #region shaderfun
        static public void RestShadersOnEditer(params Material[] _mats)
        {
            if (_mats == null || _mats.Length == 0) return;
            int tlen = _mats.Length;
            for (int i = 0; i < tlen; i++)
            {
                RestShaderOnEditer(_mats[i]);
            }
        }
        static public void ChangeShader(Transform _target, string _shadername)
        {
            if (_target == null)
            {
                DLog.LogError("切换shader失败，target为空 ");
                return;
            }
            Shader tshader = Shader.Find(_shadername);
            if (tshader == null)
            {
                DLog.LogError("切换shader失败，没找到 " + _shadername);
                return;
            }

            Renderer trander = _target.GetComponent<Renderer>();
            if (trander == null)
            {
                DLog.LogError("切换shader失败，Rander为空 ");
                return;
            }
            trander.sharedMaterial.shader = tshader;
        }
        static public void RestShaderOnEditer(Material _mat)
        {
            if (Application.platform != RuntimePlatform.WindowsEditor) return;
            if (_mat == null) return;
            Shader tshader = Shader.Find(_mat.shader.name);

            //  if (_mat.shader.name.Equals("Particles/Additive"))
            //   tshader = Shader.Find("MADFINGER/Particles/Additive TwoSide");
            // else if (_mat.shader.name.Equals("Particles/Alpha Blended"))
            //  tshader = Shader.Find("MADFINGER/Particles/Alpha Blended");
            if (tshader != null)
                _mat.shader = tshader;
        }

        #endregion

        public static void CloseShadowCaster(Transform _trans, bool _show, UnityEngine.Rendering.ShadowCastingMode _mode)
        {
            Renderer[] tarry = _trans.GetComponentsInChildren<Renderer>(_trans);
            foreach (Renderer trander in tarry)
            {
                trander.shadowCastingMode = _mode;
                trander.receiveShadows = _show;
            }
        }
    }
}


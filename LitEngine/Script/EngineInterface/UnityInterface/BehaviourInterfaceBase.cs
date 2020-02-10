using UnityEngine;
using System;
using System.Collections.Generic;
using LitEngine.Method;
using LitEngine.CodeTool;
namespace LitEngine
{
    using UpdateSpace;
    namespace ScriptInterface
    {
        public class BehaviourInterfaceBase : MonoBehaviour
        {
            #region 成员
            protected bool mIsDestory = false;
            protected bool mInitScript = false;

            protected CodeToolBase mCodeTool = null;

            public string mScriptClass = "";
            protected IBaseType mScriptType;
            protected object mObject = null;

            private Dictionary<string, MethodBase> mMethodCache = new Dictionary<string, MethodBase>();

            public object ScriptObject
            {
                get { return mObject; }
            }

            #region update
            protected UpdateBase mUpdateDelegate = null;
            protected UpdateBase mFixedUpdateDelegate = null;
            protected UpdateBase mLateUpdateDelegate = null;
            protected UpdateBase mOnGUIDelegate = null;

            #endregion
            #endregion

            #region 设置update间隔

            virtual public void SetUpdateInterval(float _time)
            {
                if (mUpdateDelegate != null)
                    mUpdateDelegate.MaxTime = _time;
            }
            virtual public void SetFixedUpdateInterval(float _time)
            {
                if (mFixedUpdateDelegate != null)
                    mFixedUpdateDelegate.MaxTime = _time;
            }
            virtual public void SetLateUpdateInterval(float _time)
            {
                if (mLateUpdateDelegate != null)
                    mLateUpdateDelegate.MaxTime = _time;
            }
            virtual public void SetOnGUIInterval(float _time)
            {
                if (mOnGUIDelegate != null)
                    mOnGUIDelegate.MaxTime = _time;
            }
            #endregion

            #region 脚本初始化以及析构

            public BehaviourInterfaceBase()
            {

            }
            ~BehaviourInterfaceBase()
            {
            }

            virtual public void ClearScriptObject()
            {
                if(mIsDestory)
                {
                    return;
                }
                mIsDestory = true;

                CallFunctionVoid("OnDestroy");

                if (mOnGUIDelegate != null)
                {
                    mOnGUIDelegate.Dispose();
                    mOnGUIDelegate = null;
                }
                if (mLateUpdateDelegate != null)
                {
                    mLateUpdateDelegate.Dispose();
                    mLateUpdateDelegate = null;
                }
                if (mFixedUpdateDelegate != null)
                {
                    mFixedUpdateDelegate.Dispose();
                    mFixedUpdateDelegate = null;
                }
                if (mUpdateDelegate != null)
                {
                    mUpdateDelegate.Dispose();
                    mUpdateDelegate = null;
                }

                mObject = null;
                mScriptType = null;
                mCodeTool = null;
            }

            virtual protected void InitParamList()
            {
                mUpdateDelegate = mCodeTool.GetUpdateObjectAction("Update", mScriptClass, ScriptObject);
                if(mUpdateDelegate != null)
                    mUpdateDelegate.Owner = GameUpdateManager.Instance.UpdateList;

                mFixedUpdateDelegate = mCodeTool.GetUpdateObjectAction("FixedUpdate", mScriptClass, ScriptObject);
                if (mFixedUpdateDelegate != null)
                    mFixedUpdateDelegate.Owner = GameUpdateManager.Instance.FixedUpdateList;

                mLateUpdateDelegate = mCodeTool.GetUpdateObjectAction("LateUpdate", mScriptClass, ScriptObject);
                if (mLateUpdateDelegate != null)
                    mLateUpdateDelegate.Owner = GameUpdateManager.Instance.LateUpdateList;

                mOnGUIDelegate = mCodeTool.GetUpdateObjectAction("OnGUI", mScriptClass, ScriptObject);
                if (mOnGUIDelegate != null)
                {
                    mOnGUIDelegate.MaxTime = 0;
                    mOnGUIDelegate.Owner = GameUpdateManager.Instance.OnGUIList;
                }
                    
            }
            virtual public void InitScript(string _class)
            {
                if (string.IsNullOrEmpty(_class) || mInitScript) return;
                mScriptClass = _class;
                InitScriptOnAwake();
                if(gameObject.activeInHierarchy)
                {
                    CallFunctionVoid("Awake");
                    OnEnable();
                }
            }

            virtual protected void InitScriptOnAwake()
            {
                if (string.IsNullOrEmpty(mScriptClass) || mInitScript) return;
                try
                {
                    mCodeTool = GameCore.CodeTool;
                    mScriptType = mCodeTool.GetLType(mScriptClass);
                    mObject = mCodeTool.GetObject(mScriptClass, this);
                    InitParamList();
                    InitInterface();
                    mInitScript = true;
                }
                catch (Exception _erro)
                {
                    DLog.LogError(string.Format("脚本初始化出错:Class = {0},GameObject = {1},InitScript ->{2}", mScriptClass, gameObject.name, _erro.ToString()));
                }
            }

            virtual protected void InitInterface()
            {

            }
            #endregion
            #region 调用脚本函数
            virtual public void CallFunVoidStringPams(string _FunNameAndStrPams)
            {
                string[] tstrs = _FunNameAndStrPams.Split('|');
                CallScriptFunctionByNameParams(tstrs[0], tstrs[1]);
            }

            virtual public void CallFunctionVoid(string _FunctionName)
            {
                try
                {
                    if (mObject == null || mScriptType == null || mCodeTool == null) return;
                    MethodBase tmethod = GetMethod(_FunctionName);
                    if (tmethod == null) return;
                    tmethod.Call();
                }
                catch (Exception _erro)
                {
                    DLog.LogError(string.Format("[{0}->{1}] [GameObject:{2}] Error:{3}", mScriptClass, _FunctionName, gameObject.name, _erro.ToString()));
                }
            }

            virtual public object CallScriptFunctionByNameParams(string _FunctionName, params object[] _prams)
            {
                try {
                    if (mObject == null || mScriptType == null || mCodeTool == null) return null;
                    int tpramcount = _prams != null ? _prams.Length : 0;
                    MethodBase tmethod = GetMethod(_FunctionName, tpramcount);
                    if (tmethod == null) return null;
                    return tmethod.Invoke(_prams);
                }
                catch (Exception _erro)
                {
                    DLog.LogError( string.Format("[{0}->{1}] [GameObject:{2}] Error:{3}",mScriptClass, _FunctionName, gameObject.name, _erro.ToString()));
                }
                return null;
            }

            public MethodBase GetMethod(string pFunctionName,int pPramCount = 0)
            {
                string tkey = pFunctionName + pPramCount;
                MethodBase ret = null;
                if (!mMethodCache.ContainsKey(tkey))
                {
                    ret = mCodeTool.GetLMethod(mScriptType, mObject, pFunctionName, pPramCount);
                    mMethodCache.Add(tkey, ret);
                }

                ret = mMethodCache[tkey];
                return ret;
            }

            #endregion
            #region 调用委托
            protected void CallAction(Action _action)
            {
                try
                {
                    if (_action != null)
                        _action();
                }
                catch (Exception _error)
                {
                    DLog.LogError( string.Format("[{0}] Error:{1}", mScriptClass, _error.ToString()));
                }
            }
            protected void CallAction<T>(Action<T> _action,T _param)
            {
                try
                {
                    if (_action != null)
                        _action(_param);
                }
                catch (Exception _error)
                {
                    DLog.LogError( string.Format("[{0}] Error:{1}", mScriptClass, _error.ToString()));
                }
            }
            #endregion
            #region ObjectFun
            virtual public void SetActive(bool _active)
            {
                if (gameObject.activeInHierarchy == _active) return;
                gameObject.SetActive(_active);
            }
            virtual public void PlaySound(AudioClip _audio)
            {
                PlayAudioManager.PlaySound(_audio);
            }
            virtual public void PlayAnimation(string _state)
            {

            }
            #endregion
            #region Unity 
            virtual protected void Awake()
            {
                InitScriptOnAwake();
                CallFunctionVoid("Awake");
            }
            virtual protected void Start()
            {
                CallFunctionVoid("Start");
                RegAll();
            }
            virtual protected void OnDestroy()
            {
                ClearScriptObject();
            }

            virtual protected void OnDisable()
            {
                CallFunctionVoid("OnDisable");
                UnRegAll();
            }
            virtual protected void OnEnable()
            {
                CallFunctionVoid("OnEnable");
                RegAll();
            }
            #endregion
            #region 注册与卸载
            virtual protected void UnRegAll()
            {
                if (mUpdateDelegate != null)
                    mUpdateDelegate.UnRegToOwner();

                if (mFixedUpdateDelegate != null)
                    mFixedUpdateDelegate.UnRegToOwner();

                if (mLateUpdateDelegate != null)
                    mLateUpdateDelegate.UnRegToOwner();

                if (mOnGUIDelegate != null)
                    mOnGUIDelegate.UnRegToOwner();

            }
            virtual protected void RegAll()
            {
                if (mUpdateDelegate != null)
                    mUpdateDelegate.RegToOwner();

                if (mFixedUpdateDelegate != null)
                    mFixedUpdateDelegate.RegToOwner();

                if (mLateUpdateDelegate != null)
                    mLateUpdateDelegate.RegToOwner();

                if (mOnGUIDelegate != null)
                    mOnGUIDelegate.RegToOwner();
            }
            #endregion
        }
    }
    
}

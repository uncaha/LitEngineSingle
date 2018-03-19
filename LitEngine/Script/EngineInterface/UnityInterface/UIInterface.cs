using UnityEngine;
using System.Collections.Generic;
namespace LitEngine
{
    namespace ScriptInterface
    {
        
        public class UIInterface : BehaviourInterfaceBase
        {
            public enum UISate
            {
                Normal = 0,
                Showing,
                Hidden,
            }
            protected UISate mState = UISate.Normal;
            protected Dictionary<UIAniType, UIAnimator> mAniMap;
            protected UIAnimator mCurAni;
            #region 脚本初始化以及析构
            public UIInterface()
            {
                
            }

            override protected void InitParamList()
            {
                base.InitParamList();
            }

            override public void ClearScriptObject()
            {
                base.ClearScriptObject();
            }
            #endregion
            #region Unity 
            protected override void Awake()
            {
                base.Awake();
                Animator tanitor = GetComponent<Animator>();
                if (tanitor != null)
                {
                    tanitor.Stop();
                    tanitor.Rebind();
                    tanitor.enabled = false;

                }

                UIAnimator[] tanimators = GetComponents<UIAnimator>();
                if (tanimators.Length > 0)
                {
                    mAniMap = new Dictionary<UIAniType, UIAnimator>();
                    for (int i = 0; i < tanimators.Length; i++)
                    {
                        switch (tanimators[i].Type)
                        {
                            case UIAniType.Hide:
                                tanimators[i].Init(OnHideAnimationEnd);
                                break;
                            case UIAniType.Show:
                                tanimators[i].Init(OnShowAnimationEnd);
                                break;
                            case UIAniType.Normal:
                                tanimators[i].Init(OnNormalAnimationEnd);
                                break;
                            case UIAniType.Custom:
                                tanimators[i].Init(OnCustomAnimationEnd);
                                break;
                        }
                        tanimators[i].enabled = false;
                        mAniMap.Add(tanimators[i].Type, tanimators[i]);
                    }
                }
            }
            override protected void OnDisable()
            {
                base.OnDisable();
            }

            override protected void OnEnable()
            {
                base.OnEnable();
            }

            override protected void OnDestroy()
            {
                mCurAni = null;
                if(mAniMap != null)
                    mAniMap.Clear();
                base.OnDestroy();
            }
            #endregion
            #region 调用脚本函数
            override public object CallScriptFunctionByNameParams(string _FunctionName, params object[] _prams)
            {
                if (mState != UISate.Normal) return null;
                return base.CallScriptFunctionByNameParams(_FunctionName, _prams);
            }
            #endregion
            #region Call

            #region ani
            override public void PlayAnimation(string _state)
            {
                if (mAniMap == null) return;
                if (!mAniMap.ContainsKey(UIAniType.Custom)) return;
                if (mCurAni != null)
                    mCurAni.Stop();
                mCurAni = mAniMap[UIAniType.Custom];
                mCurAni.Play(_state);
            }

            protected bool PlayUIAni(UIAniType _type)
            {
                if (mAniMap == null) return false;
                if (!mAniMap.ContainsKey(_type)) return false;
                if (!mAniMap[_type].CanPlay) return false;
                if (mCurAni != null && mCurAni.Type == _type && mCurAni.IsPlaying) return true;
                if(mCurAni != null)
                    mCurAni.Stop();
                mCurAni = mAniMap[_type];
                return mCurAni.Play();
            }
            override public void SetActive(bool _active)
            {
                if (_active)
                {
                    base.SetActive(true);
                    mState = UISate.Showing;
                    if (!PlayUIAni(UIAniType.Show))
                    {
                        OnShowAnimationEnd();
                    }
                }  
                else
                {
                    if (!gameObject.activeInHierarchy) return;

                    mState = UISate.Hidden;
                    if (!PlayUIAni(UIAniType.Hide))
                    {
                        base.SetActive(false);
                        OnHideAnimationEnd();
                    }
                         
                }              
            }

            protected void OnShowAnimationEnd()
            {
                PlayUIAni(UIAniType.Normal);
                mState = UISate.Normal;
                CallScriptFunctionByNameParams("OnShowAnimationEnd");
            }

            protected void OnHideAnimationEnd()
            {
                mState = UISate.Normal;
                base.SetActive(false);
                CallScriptFunctionByNameParams("OnHideAnimationEnd");
            }

            protected void OnNormalAnimationEnd()
            {
                CallScriptFunctionByNameParams("OnNormalAnimationEnd");
            }

            protected void OnCustomAnimationEnd()
            {
                CallScriptFunctionByNameParams("OnCustomAnimationEnd");
            }

            #endregion
            #endregion
        }
    }

     
}

using UnityEngine;
using System.Collections.Generic;
namespace LitEngine.ScriptInterface
{
    public class UIInterface : BehaviourInterfaceBase
    {
        [System.Serializable]
        public class UIObject
        {
            public string key;
            public GameObject gameobject;
        }
        public enum UISate
        {
            Normal = 0,
            Showing,
            Hidden,
        }
        public int Deep = 0;
        public UIObject[] objects;
        protected Dictionary<string, GameObject> objectDic = new Dictionary<string, GameObject>();
        protected UISate mState = UISate.Normal;
        protected Dictionary<UIAniType, UIAnimator> mAniMap;
        protected UIAnimator mCurAni;
        #region 脚本初始化以及析构
        public UIInterface()
        {

        }

        override protected void InitInterface()
        {
            UnityEngine.Animator tanitor = GetComponent<UnityEngine.Animator>();
            if (tanitor != null)
            {
                tanitor.enabled = false;
                tanitor.Rebind();

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

            if (objects != null && objects.Length > 0)
            {
                for (int i = 0; i < objects.Length; i++)
                {
                    objectDic.Add(objects[i].key, objects[i].gameobject);
                    if (mObject != null)
                    {
                        mCodeTool.SetTargetMember(mObject, objects[i].key, objects[i].gameobject);
                    }
                }

            }
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
            if (mAniMap != null)
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
        virtual public void BtnCall(string _key)
        {
            CallScriptFunctionByNameParams("BtnCall", _key);
        }

        virtual public void BtnPressDown(string _key)
        {
            CallScriptFunctionByNameParams("BtnPressDown", _key);
        }

        virtual public void BtnPressUP(string _key)
        {
            CallScriptFunctionByNameParams("BtnPressUP", _key);
        }
        #endregion
        #region Call

        public Object this[string key]
        {
            get
            {
                if (!objectDic.ContainsKey(key)) return null;
                return objectDic[key];
            }
        }

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

        virtual protected bool PlayUIAni(UIAniType _type)
        {
            if (mAniMap == null) return false;
            if (!mAniMap.ContainsKey(_type)) return false;
            if (!mAniMap[_type].CanPlay) return false;
            if (mCurAni != null && mCurAni.Type == _type && mCurAni.IsPlaying) return true;
            if (mCurAni != null)
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
                    OnShowAnimationEnd(null);
                }
            }
            else
            {
                if (!gameObject.activeInHierarchy) return;

                mState = UISate.Hidden;
                if (!PlayUIAni(UIAniType.Hide))
                {
                    base.SetActive(false);
                    OnHideAnimationEnd(null);
                }

            }
        }

        virtual protected void OnShowAnimationEnd(string senderKey)
        {
            PlayUIAni(UIAniType.Normal);
            mState = UISate.Normal;
            CallScriptFunctionByNameParams("OnShowAnimationEnd");
        }

        virtual protected void OnHideAnimationEnd(string senderKey)
        {
            mState = UISate.Normal;
            base.SetActive(false);
            CallScriptFunctionByNameParams("OnHideAnimationEnd");
        }

        virtual protected void OnNormalAnimationEnd(string senderKey)
        {
            CallScriptFunctionByNameParams("OnNormalAnimationEnd");
        }

        virtual protected void OnCustomAnimationEnd(string senderKey)
        {
            CallScriptFunctionByNameParams("OnCustomAnimationEnd");
        }

        #endregion
        #endregion
    }
}

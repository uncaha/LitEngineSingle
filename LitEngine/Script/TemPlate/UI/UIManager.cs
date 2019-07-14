using UnityEngine;
using System.Collections.Generic;
using LitEngine.UpdateSpace;
namespace LitEngine.TemPlate.UI
{
    public class UIManager : MonoManagerBase
    {
        public static string sRootPfbName = "UIRoot";
        public static string sUIFolder = "UI/";

        private static object lockobj = new object();
        static private UIManager _Instance;
        static public UIManager Instance
        {
            get
            {
                if (_Instance == null)
                {
                    lock (lockobj)
                    {
                        if (_Instance == null)
                        {
                            Object tres = Resources.Load(sUIFolder + sRootPfbName);
                            if (tres != null)
                            {
                                GameObject tobj = ((GameObject)Instantiate(tres));
                                GameObject.DontDestroyOnLoad(tobj);
                                tobj.name = "UIRoot";
                                _Instance = tobj.AddComponent<UIManager>();
                            }
                        }
                    }

                }

                return _Instance;
            }
        }

        override public void DestroyManager()
        {
            UIManager.HideAll();
            UIManager.ReleaseAllHide();
            _Instance = null;
            GameObject.DestroyImmediate(gameObject);
            base.DestroyManager();
        }

        private Dictionary<string, UIBase> mUiCache = new Dictionary<string, UIBase>();
        private Dictionary<string, UIBase> mShowList = new Dictionary<string, UIBase>();
        private Dictionary<string, UIBase> mHideList = new Dictionary<string, UIBase>();

        private List<UIBase> updateList = new List<UIBase>();
        private List<string> UIStack = new List<string>();
        private UIManager()
        {

        }

        private void Awake()
        {
            UpdateObject tobj = new UpdateObject("UIManager", new Method.Method_Action(UIUpdate),null);
            tobj.MaxTime = 0;
            GameUpdateManager.RegUpdate(tobj);
        }

        private float dataTimer = 0;
        private void UIUpdate()
        {
            float dt = Time.deltaTime;
            dataTimer += dt;
            for (int i = updateList.Count - 1; i >= 0; i--)
            {
                UIBase tui = updateList[i];
                if (tui != null && tui.IsCanUpdate)
                {
                    tui.UpdateUI(dt);
                    if (dataTimer > 0.2f)
                        tui.UpdateUIDate();
                }

            }
            if (dataTimer > 0.2f)
                dataTimer = 0;
        }

        private UIBase LoadUI(string _uiname)
        {
            Object tres = Resources.Load(sUIFolder + _uiname);
            if (tres == null) return null;
            ((GameObject)tres).SetActive(false);
            GameObject tui = ((GameObject)GameObject.Instantiate(tres));
            tui.name = tui.name.Replace("(Clone)", "");

            tui.transform.SetParent(transform, false);

            UIBase tuiScript = tui.GetComponent<UIBase>();
            if (tuiScript == null)
            {
                DLog.LogError("UI未能找到 UIBase 接口.");
                GameObject.DestroyImmediate(tui);
                return null;
            }
            tuiScript.Init(_uiname);
            mUiCache.Add(_uiname, tuiScript);

            return tuiScript;
        }

        protected void ShortUI(UIBase _nowUI)
        {
            List<UIBase> tlist = new List<UIBase>(mUiCache.Values);
            tlist.Sort(UIShortFun);

            int tcount = tlist.Count;

            for (int i = 0; i < tcount; i++)
            {
                tlist[i].transform.SetSiblingIndex(i);
            }
        }

        protected int UIShortFun(UIBase a, UIBase b)
        {
            int r = a.deep.CompareTo(b.deep);
            return r;
        }

        public UIBase ShowStackUI()
        {
            if (UIStack.Count == 0) return null;
            int tindex = UIStack.Count - 1;
            string tuiName = UIStack[tindex];
            UIStack.RemoveAt(tindex);
            return Show(tuiName, false);
        }

        public void ClearStack()
        {
            UIStack.Clear();
        }

        static public UIBase Show(string _uiname, bool addStack = true)
        {
            return Instance._Show(_uiname, addStack);
        }
        protected UIBase _Show(string _uiname, bool addStack = false)
        {
            if (string.IsNullOrEmpty(_uiname)) return null;
            UIBase tshowUI = null;
            if (!mUiCache.ContainsKey(_uiname))
                tshowUI = LoadUI(_uiname);
            else
                tshowUI = mUiCache[_uiname];

            if (tshowUI == null) return null;

            if (tshowUI.Actived) return tshowUI;
            tshowUI.SetActive(true);
            if (addStack)
                UIStack.Add(_uiname);
            ShortUI(tshowUI);
            return tshowUI;
        }

        static public T Get<T>(string _uiname) where T : UIBase
        {
            if (Instance.mShowList.ContainsKey(_uiname))
                return (T)Instance.mShowList[_uiname];
            return null;
        }

        static public void Hide(string _uiname)
        {
            Instance._Hide(_uiname);
        }
        protected void _Hide(string _uiname)
        {
            if (string.IsNullOrEmpty(_uiname)) return;
            if (!mUiCache.ContainsKey(_uiname)) return;
            if (!mUiCache[_uiname].Actived) return;
            mUiCache[_uiname].SetActive(false);
        }


        static public bool IsShow(string _uiname)
        {
            return Instance._IsShow(_uiname);
        }
        protected bool _IsShow(string _uiname)
        {
            if (!mShowList.ContainsKey(_uiname)) return false;
            return true;
        }


        static public void AddShowCache(string _uiname)
        {
            Instance._AddShowCache(_uiname);
        }
        protected void _AddShowCache(string _uiname)
        {
            if (!mShowList.ContainsKey(_uiname))
                mShowList.Add(_uiname, mUiCache[_uiname]);
            if (mHideList.ContainsKey(_uiname))
                mHideList.Remove(_uiname);
            if (!updateList.Contains(mUiCache[_uiname]))
                updateList.Add(mUiCache[_uiname]);
        }


        static public void AddHideCache(string _uiname)
        {
            Instance._AddHideCache(_uiname);
        }
        protected void _AddHideCache(string _uiname)
        {
            if (mShowList.ContainsKey(_uiname))
                mShowList.Remove(_uiname);
            if (!mHideList.ContainsKey(_uiname))
                mHideList.Add(_uiname, mUiCache[_uiname]);
            if (updateList.Contains(mUiCache[_uiname]))
                updateList.Remove(mUiCache[_uiname]);
        }


        static public void HideAll(string exceptional = null)
        {
            Instance._HideAll(exceptional);
        }
        protected void _HideAll(string exceptional = null)
        {
            List<string> tkeys = new List<string>(mShowList.Keys);
            for (int i = tkeys.Count - 1; i >= 0; i--)
            {
                if (!string.IsNullOrEmpty(exceptional) && tkeys[i].Equals(exceptional)) continue;
                Hide(tkeys[i]);
            }
        }


        static public void RemoveFromAllCache(string _uiname)
        {
            Instance._RemoveFromAllCache(_uiname);
        }
        protected void _RemoveFromAllCache(string _uiname)
        {
            if (string.IsNullOrEmpty(_uiname)) return;

            if (mShowList.ContainsKey(_uiname))
                mShowList.Remove(_uiname);
            if (mHideList.ContainsKey(_uiname))
                mHideList.Remove(_uiname);
            if (mUiCache.ContainsKey(_uiname))
                mUiCache.Remove(_uiname);
        }

        static public void ReleaseUI(string _uiname)
        {
            Instance._ReleaseUI(_uiname);
        }
        protected void _ReleaseUI(string _uiname)
        {
            if (string.IsNullOrEmpty(_uiname)) return;
            if (!mUiCache.ContainsKey(_uiname)) return;
            UIBase tui = mUiCache[_uiname];
            tui.Dispose();
        }


        static public void ReleaseAllHide()
        {
            Instance._ReleaseAllHide();
        }
        protected void _ReleaseAllHide()
        {
            List<string> tkeys = new List<string>(mHideList.Keys);
            for (int i = tkeys.Count - 1; i >= 0; i--)
            {
                ReleaseUI(tkeys[i]);
            }
        }
    }
}

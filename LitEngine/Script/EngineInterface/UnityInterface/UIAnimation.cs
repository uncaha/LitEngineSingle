using UnityEngine;
namespace LitEngine.ScriptInterface
{
    public enum UIAniType
    {
        None = 0,
        Show,
        Hide,
        Normal,
        Custom,
    }
    public class UIAnimator : CustomAnimator
    {
        public UIAniType Type = UIAniType.None;

        override protected void Awake()
        {
            base.Awake();
        }

        override public bool Play(string _state)
        {
            if (Type != UIAniType.Custom) return false;
            return base.Play(_state);
        }
    }
}
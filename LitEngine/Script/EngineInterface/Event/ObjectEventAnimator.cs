using UnityEngine;
namespace LitEngine.ScriptInterface.Event
{
    public class ObjectEventAnimator : ObjectEventBase
    {
        public Animator target;
        public string Key;

        private CustomAnimator aniControl;
        protected override void Awake()
        {
            base.Awake();
            target.enabled = false;
            aniControl = target.gameObject.AddComponent<CustomAnimator>();
        }
        override public void Play()
        {
            aniControl.Play(Key);
        }
        override public void Stop()
        {
            aniControl.Stop();
        }

        override public bool IsPlaying
        {
            get {
                return aniControl.IsPlaying;
            }
        }
    }

}

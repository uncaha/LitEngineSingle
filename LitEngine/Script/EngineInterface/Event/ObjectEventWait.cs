using UnityEngine;
namespace LitEngine.ScriptInterface.Event
{
    public class ObjectEventWait : ObjectEventBase
    {
        public float waitSecond = 1;
        protected override void Awake()
        {
            base.Awake();
        }
        override public void Play()
        {
            if (_IsPlaying) return;   
            StopAllCoroutines();
            _IsPlaying = true;
            StartCoroutine(StartWait());
        }
        override public void Stop()
        {
            StopAllCoroutines();
            _IsPlaying = false;
        }

        System.Collections.IEnumerator StartWait()
        {
            yield return new WaitForSeconds(waitSecond);
            _IsPlaying = false;
        }

        private bool _IsPlaying = false;
        override public bool IsPlaying { get { return _IsPlaying; } }
    }
}

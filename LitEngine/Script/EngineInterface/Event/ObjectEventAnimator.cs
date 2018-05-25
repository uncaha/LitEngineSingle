using UnityEngine;
namespace LitEngine.ScriptInterface.Event
{
    public class ObjectEventAnimator : ObjectEventBase
    {
        override public void Awake()
        {

        }

        override public void Play()
        {
            ((Animator)Target).enabled = true;
            ((Animator)Target).Play(Key);
        }
        override public void Stop()
        {
            ((Animator)Target).enabled = false;
        }

        override public bool IsPlaying
        {
            get {
                AnimatorStateInfo tstate = ((Animator)Target).GetCurrentAnimatorStateInfo(0);
                float ttime = Mathf.Clamp01(tstate.normalizedTime);
                return !tstate.loop && ttime == 1f;
            }
        }
    }

}

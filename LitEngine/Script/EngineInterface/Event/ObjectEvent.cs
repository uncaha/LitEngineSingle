using UnityEngine;
namespace LitEngine.ScriptInterface.Event
{
    [System.Serializable]
    public class ObjectEvent : ObjectEventBase
    {
        protected ObjectEventBase targetEvent;
        override public void Init()
        {
            System.Type ttype = Target.GetType();
            if (ttype.Equals(typeof(Animator)))
                targetEvent = new ObjectEventAnimator();
            else if (ttype.Equals(typeof(AudioSource)))
                targetEvent = new ObjectEventAudioSource();
            else if (ttype.Equals(typeof(ParticleSystem)))
                targetEvent = new ObjectEventParticle();
            else
                targetEvent = new ObjectEventCustom();
            targetEvent.Target = Target;
            targetEvent.Parent = Parent;
            targetEvent.Key = Key;
            targetEvent.Init();
        }

        override public void Play()
        {
            if (targetEvent == null) return;
            targetEvent.Play();
        }
        override public void Stop()
        {
            if (targetEvent == null) return;
            targetEvent.Stop();
        }
        override public bool IsPlaying
        {
            get { return targetEvent != null ? targetEvent.IsPlaying : false; }
        }
    }

}

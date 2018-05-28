using UnityEngine;
namespace LitEngine.ScriptInterface.Event
{
    public class ObjectEventParticle : ObjectEventBase
    {
        public ParticleSystem target;
        override public void Play()
        {
            target.Play(true);
        }
        override public void Stop()
        {
            target.Stop(true);
        }

        override public bool IsPlaying
        {
            get { return target.isPlaying;}
        }
    }
}

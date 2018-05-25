using UnityEngine;
namespace LitEngine.ScriptInterface.Event
{
    public class ObjectEventParticle : ObjectEventBase
    {
        override public void Init()
        {
            Stop();
        }

        override public void Play()
        {
            ((ParticleSystem)Target).Play(true);
        }
        override public void Stop()
        {
            ((ParticleSystem)Target).Stop(true);
        }

        override public bool IsPlaying
        {
            get { return ((ParticleSystem)Target).isPlaying;}
        }
    }
}

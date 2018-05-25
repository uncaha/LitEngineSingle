using UnityEngine;
namespace LitEngine.ScriptInterface.Event
{
    public class ObjectEventAudioSource : ObjectEventBase
    {
        override public void Init()
        {
            Stop();
        }

        override public void Play()
        {
            ((AudioSource)Target).Play();
        }
        override public void Stop()
        {
            ((AudioSource)Target).Stop();
        }

        override public bool IsPlaying
        {
            get { return ((AudioSource)Target).isPlaying; }
        }
    }
}

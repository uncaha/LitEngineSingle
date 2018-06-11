using UnityEngine;
namespace LitEngine.ScriptInterface.Event
{
    public class ObjectEventAudioClip : ObjectEventBase
    {
        public enum SoundPlayType
        {
            normal = 0,
            useSoundMix,
        }
        public AudioClip target;
        public SoundPlayType PlayType = SoundPlayType.normal;

        override public void Play()
        {
            switch (PlayType)
            {
                case SoundPlayType.normal:
                    PlayAudioManager.PlaySound(target);
                    break;
                case SoundPlayType.useSoundMix:
                    PlayAudioManager.PlayMixerSound(target);
                    break;
                default:
                    break;
            }
           
        }
        override public void Stop()
        {
        }

        override public bool IsPlaying
        {
            get
            {
                return false;
            }
        }
    }
}

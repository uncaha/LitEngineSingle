using UnityEngine;
namespace LitEngine.ScriptInterface.Event
{
    public class ObjectEventAudioSource : ObjectEventBase
    {
        public enum SoundPlayType
        {
            normal = 0,
            useSoundMix,
        }
        public AudioSource target;
        public SoundPlayType PlayType = SoundPlayType.normal;
        protected override void Awake()
        {
            base.Awake();
            if (target == null) return;
            switch (PlayType)
            {
                case SoundPlayType.normal:
                    target.outputAudioMixerGroup = null;
                    break;
                case SoundPlayType.useSoundMix:
                    target.outputAudioMixerGroup = PlayAudioManager.SoundMixer;
                    break;
                default:
                    break;
            }
        }
        override public void Play()
        {
            target.Play();
        }
        override public void Stop()
        {
            target.Stop();
        }

        override public bool IsPlaying
        {
            get
            {
                return target.isPlaying;
            }
        }
    }
}

using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
namespace LitEngine
{
    public class PlayAudioManager : MonoManagerBase
    {
        private static PlayAudioManager sInstance = null;
        private static PlayAudioManager Instance
        {
            get
            {
                if (sInstance == null)
                {
                    GameObject tobj = new GameObject("PlayAudioManager");
                    GameObject.DontDestroyOnLoad(tobj);
                    sInstance = tobj.AddComponent<PlayAudioManager>();
                }
                return sInstance;
            }
        }

        private AudioMixerGroup _BackMusicMixer = null;
        public static AudioMixerGroup BackMusicMixer
        {
            get
            {
                return Instance._BackMusicMixer;
            }

            set
            {
                Instance._BackMusicMixer = value;
                Instance.mBackMusic.outputAudioMixerGroup = value;
            }
        }

        private AudioMixerGroup _SoundMixer = null;
        public static AudioMixerGroup SoundMixer
        {
            get
            {
                return Instance._SoundMixer;
            }

            set
            {
                Instance._SoundMixer = value;
                for (int i = 0; i < Instance.mMaxSoundCount; i++)
                {
                    Instance.mAudioMixerSounds[i].outputAudioMixerGroup = value;
                }
            }
        }

        static private float mVolume = 1;
        static public float Volume
        {
            get { return mVolume; }
            set
            {
                mVolume = value;
                MusicVolume = mMusicVolume;
                SoundVolume = mSoundVolume;
            }
        }

        static private float mMusicRealVolume = 1;
        static private float mMusicVolume = 1;
        static public float MusicVolume
        {
            get { return mMusicVolume; }
            set
            {
                mMusicVolume = Mathf.Clamp01(value);
                mMusicRealVolume = mMusicVolume * Volume;
                Instance.mBackMusic.volume = mMusicRealVolume;
            }
        }
        static private float mSoundRealVolume = 1;
        static private float mSoundVolume = 1;
        static public float SoundVolume
        {
            get { return mSoundVolume; }
            set
            {
                mSoundVolume = Mathf.Clamp01(value);
                mSoundRealVolume = Volume * mSoundVolume;
                for (int i = 0; i < Instance.mMaxSoundCount; i++)
                {
                    Instance.mAudioSounds[i].volume = mSoundRealVolume;
                }

                for (int i = 0; i < Instance.mMaxSoundCount; i++)
                {
                    Instance.mAudioMixerSounds[i].volume = mSoundRealVolume;
                }

                for (int i = 0; i < Instance.outSideAudioSounds.Count; i++)
                {
                    Instance.outSideAudioSounds[i].volume = mSoundRealVolume;
                }
            }
        }

        private int mMaxSoundCount = 3;

        private AudioSource mBackMusic;
        private AudioSource[] mAudioMixerSounds;
        private AudioSource[] mAudioSounds;

        private List<AudioSource> outSideAudioSounds = new List<AudioSource>();

        private int mIndex = 0;
        private int mMixerIndex = 0;

        private List<AudioSource> audioSources = new List<AudioSource>();
        private void Awake()
        {
            mBackMusic = gameObject.AddComponent<AudioSource>();

            mAudioMixerSounds = new AudioSource[mMaxSoundCount];
            for (int i = 0; i < mMaxSoundCount; i++)
            {
                mAudioMixerSounds[i] = gameObject.AddComponent<AudioSource>();
            }

            mAudioSounds = new AudioSource[mMaxSoundCount];
            for (int i = 0; i < mMaxSoundCount; i++)
            {
                mAudioSounds[i] = gameObject.AddComponent<AudioSource>();
            }

            audioSources.Add(mBackMusic);
            audioSources.AddRange(mAudioMixerSounds);
            audioSources.AddRange(mAudioSounds);
        }
        override protected void OnDestroy()
        {
            audioSources.Clear();
            base.OnDestroy();
        }

        static public bool IsChild(AudioSource targetAudio)
        {
            return Instance.audioSources.Contains(targetAudio);
        }

        static public bool IsInOutSideList(AudioSource targetAudio)
        {
            return Instance.outSideAudioSounds.Contains(targetAudio);
        }

        static public void AddOutSideAudioSource(AudioSource _audioSource)
        {
            if (!Instance.outSideAudioSounds.Contains(_audioSource))
                Instance.outSideAudioSounds.Add(_audioSource);
        }

        static public void RemoveOutSideAudioSource(AudioSource _audioSource)
        {
            if (Instance.outSideAudioSounds.Contains(_audioSource))
                Instance.outSideAudioSounds.Remove(_audioSource);
        }

        static public AudioSource PlayMixerSound(AudioClip _clip)
        {
            if (Instance.mMixerIndex == Instance.mMaxSoundCount) Instance.mMixerIndex = 0;
            AudioSource ret = Instance.mAudioMixerSounds[Instance.mMixerIndex];
            ret.Stop();
            ret.clip = _clip;
            ret.loop = false;
            ret.Play();
            Instance.mMixerIndex++;
            return ret;
        }

        static public AudioSource PlaySound(AudioClip _clip)
        {
            if (Instance.mIndex == Instance.mMaxSoundCount) Instance.mIndex = 0;
            AudioSource ret = Instance.mAudioSounds[Instance.mIndex];
            ret.Stop();
            ret.clip = _clip;
            ret.loop = false;
            ret.Play();
            Instance.mIndex++;
            return ret;
        }

        static public void PlayMusic(AudioClip _clip)
        {
            if (Instance.mBackMusic.clip != null && Instance.mBackMusic.clip.Equals(_clip)) return;
            Instance.mBackMusic.Stop();
            Instance.mBackMusic.clip = _clip;
            Instance.mBackMusic.loop = true;
            Instance.mBackMusic.Play();
        }

        #region stop,pause
        static public void StopMusic()
        {
            Instance.mBackMusic.Stop();
        }

        static public void PauseMusic()
        {
            Instance.mBackMusic.Pause();
        }

        static public void UnPauseMusic()
        {
            Instance.mBackMusic.UnPause();
        }

        static public void StopSound()
        {
            for (int i = 0; i < Instance.mMaxSoundCount; i++)
            {
                Instance.mAudioSounds[i].Stop();
            }

            for (int i = 0; i < Instance.mMaxSoundCount; i++)
            {
                Instance.mAudioMixerSounds[i].Stop();
            }

            for (int i = 0; i < Instance.outSideAudioSounds.Count; i++)
            {
                Instance.outSideAudioSounds[i].Stop();
            }
        }

        static public void PauseSound()
        {
            for (int i = 0; i < Instance.mMaxSoundCount; i++)
            {
                Instance.mAudioSounds[i].Pause();
            }

            for (int i = 0; i < Instance.mMaxSoundCount; i++)
            {
                Instance.mAudioMixerSounds[i].Pause();
            }

            for (int i = 0; i < Instance.outSideAudioSounds.Count; i++)
            {
                Instance.outSideAudioSounds[i].Pause();
            }
        }

        static public void UnPauseSound()
        {
            for (int i = 0; i < Instance.mMaxSoundCount; i++)
            {
                Instance.mAudioSounds[i].UnPause();
            }

            for (int i = 0; i < Instance.mMaxSoundCount; i++)
            {
                Instance.mAudioMixerSounds[i].UnPause();
            }

            for (int i = 0; i < Instance.outSideAudioSounds.Count; i++)
            {
                Instance.outSideAudioSounds[i].UnPause();
            }
        }

        static public void PauseAllAudioExceptSound()
        {
            AudioSource[] tevents = FindObjectsOfType<AudioSource>();
            int tlen = tevents.Length;
            for (int i = 0; i < tlen; i++)
            {
                if (!IsChild(tevents[i]) && !IsInOutSideList(tevents[i]))
                    tevents[i].Pause();
            }
            PauseMusic();
        }
        static public void UnPauseAllAudioExceptSound()
        {
            AudioSource[] tevents = FindObjectsOfType<AudioSource>();
            int tlen = tevents.Length;
            for (int i = 0; i < tlen; i++)
            {
                if (!IsChild(tevents[i]) && !IsInOutSideList(tevents[i]))
                    tevents[i].UnPause();
            }
            UnPauseMusic();
        }
        #endregion
        static public void ClearUnUsedOutSideAudioSource()
        {
            for (int i = Instance.outSideAudioSounds.Count - 1; i >= 0; i--)
            {
                if (Instance.outSideAudioSounds[i] == null)
                    Instance.outSideAudioSounds.RemoveAt(i);
            }
        }

        static public void Clear()
        {
            Instance.mBackMusic.Stop();
            Instance.mBackMusic.clip = null;
            for (int i = 0; i < Instance.mMaxSoundCount; i++)
            {
                Instance.mAudioSounds[i].Stop();
                Instance.mAudioSounds[i].clip = null;
            }

            for (int i = 0; i < Instance.mMaxSoundCount; i++)
            {
                Instance.mAudioMixerSounds[i].Stop();
                Instance.mAudioMixerSounds[i].clip = null;
            }
        }
    }
}

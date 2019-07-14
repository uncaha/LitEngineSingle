using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
namespace LitEngine
{
    public class AudioGroup
    {
        public List<AudioSource> AudioList { get; private set; }
        public GameObject gameObject { get; private set; }
        public int Index { get; private set; }
        private bool useMixer = false;
        private float _Volume = 1;
        public float Volume
        {
            get { return _Volume; }
            set
            {
                _Volume = Mathf.Clamp01(value);
                for (int i = 0; i < AudioList.Count; i++)
                {
                    AudioList[i].volume = _Volume;
                }
            }
        }
        public AudioGroup(GameObject _object, int _count, bool _useMixer = false)
        {
            Index = 0;
            gameObject = _object;
            useMixer = _useMixer;
            AudioList = new List<AudioSource>();
            for (int i = 0; i < _count; i++)
            {
                AudioSource tsour = gameObject.AddComponent<AudioSource>();
                AudioList.Add(tsour);
            }
        }

        public AudioSource Play(AudioClip _clip)
        {
            if (_clip == null) return null;
            if (Index >= AudioList.Count) Index = 0;
            AudioSource ret = AudioList[Index];
            ret.Stop();
            if (useMixer && ret.outputAudioMixerGroup != PlayAudioManager.SoundMixer)
                ret.outputAudioMixerGroup = PlayAudioManager.SoundMixer;
            ret.clip = _clip;
            ret.loop = false;
            ret.Play();
            Index++;
            return ret;
        }

        public void Pause()
        {
            for (int i = 0; i < AudioList.Count; i++)
            {
                AudioList[i].Pause();
            }
        }

        public void UnPause()
        {
            for (int i = 0; i < AudioList.Count; i++)
            {
                AudioList[i].UnPause();
            }
        }

        public void Stop()
        {
            for (int i = 0; i < AudioList.Count; i++)
            {
                AudioList[i].Stop();
            }
        }

        public void Clear()
        {
            for (int i = 0; i < AudioList.Count; i++)
            {
                AudioList[i].Stop();
                AudioList[i].clip = null;
            }
        }

        public bool Contains(AudioSource _audio)
        {
            return AudioList.Contains(_audio);
        }
    }
    public class PlayAudioManager : MonoManagerBase
    {
        private static object lockobj = new object();
        private static PlayAudioManager sInstance = null;
        private static PlayAudioManager Instance
        {
            get
            {
                if (sInstance == null)
                {
                    lock (lockobj)
                    {
                        if (sInstance == null)
                        {
                            GameObject tobj = new GameObject("PlayAudioManager");
                            GameObject.DontDestroyOnLoad(tobj);
                            sInstance = tobj.AddComponent<PlayAudioManager>();
                            sInstance.Init();
                        }
                    }
                }

                return sInstance;
            }
        }
        private PlayAudioManager() { }

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

                for (int i = 0; i < Instance.audioSources.Count; i++)
                {
                    Instance.audioSources[i].volume = mSoundRealVolume;
                }
            }
        }

        private int mMaxSoundCount = 3;

        private AudioSource mBackMusic;
        private AudioGroup mAudioMixerSounds;
        private AudioGroup mAudioSounds;

        private Dictionary<string, AudioGroup> audioGroup = new Dictionary<string, AudioGroup>();

        private GameObject mixerSoundsObject;
        private GameObject soundsObject;
        private GameObject groupAudioSoundObject;
        private List<AudioSource> audioSources = new List<AudioSource>();
        private bool isInit = false;
        private void Awake()
        {

        }
        private void Init()
        {
            if (isInit) return;
            isInit = true;

            mBackMusic = gameObject.AddComponent<AudioSource>();

            mixerSoundsObject = new GameObject("mixerSoundsObject");
            mixerSoundsObject.transform.SetParent(transform);
            soundsObject = new GameObject("soundsObject");
            soundsObject.transform.SetParent(transform);
            groupAudioSoundObject = new GameObject("groupAudioSoundObject");
            groupAudioSoundObject.transform.SetParent(transform);

            mAudioMixerSounds = new AudioGroup(mixerSoundsObject, mMaxSoundCount, true);
            mAudioSounds = new AudioGroup(soundsObject, mMaxSoundCount, false);

            audioSources.AddRange(mAudioMixerSounds.AudioList);
            audioSources.AddRange(mAudioSounds.AudioList);
        }
        override protected void OnDestroy()
        {
            audioSources.Clear();
            base.OnDestroy();
        }

        static public bool IsChild(AudioSource targetAudio)
        {
            return Instance.mBackMusic.Equals(targetAudio) && Instance.audioSources.Contains(targetAudio);
        }

        static public void AddOutSideAudioSource(AudioSource _audioSource)
        {
            if (_audioSource == null) return;
            if (Instance.audioSources.Contains(_audioSource)) return;
            Instance.audioSources.Add(_audioSource);
            _audioSource.volume = SoundVolume;
        }

        static public void RemoveOutSideAudioSource(AudioSource _audioSource)
        {
            if (_audioSource == null) return;
            if (!Instance.audioSources.Contains(_audioSource)) return;
            Instance.audioSources.Remove(_audioSource);
        }

        static public void AddAudioSoundGroup(string _key, bool _useMixer = false)
        {
            if (Instance.audioGroup.ContainsKey(_key)) return;
            GameObject tkeyobj = new GameObject(_key + "-Object");
            tkeyobj.transform.SetParent(Instance.groupAudioSoundObject.transform);
            AudioGroup tlist = new AudioGroup(tkeyobj, Instance.mMaxSoundCount, _useMixer);
            tlist.Volume = SoundVolume;
            Instance.audioGroup.Add(_key, tlist);
            Instance.audioSources.AddRange(tlist.AudioList);
        }

        static public void RemoveSoundGroup(string _key)
        {
            if (!Instance.audioGroup.ContainsKey(_key)) return;
            AudioGroup tg = Instance.audioGroup[_key];
            GameObject.Destroy(tg.gameObject);
            ClearDestoryedSound();
        }

        static public AudioSource PlayMixerSound(AudioClip _clip)
        {
            if (_clip == null) return null;
            return Instance.mAudioMixerSounds.Play(_clip);
        }

        static public AudioSource PlaySound(AudioClip _clip)
        {
            if (_clip == null) return null;
            return Instance.mAudioSounds.Play(_clip);
        }

        static public AudioSource PlaySoundByGroup(string _key, AudioClip _clip)
        {
            if (_clip == null || !Instance.audioGroup.ContainsKey(_key)) return null;
            return Instance.audioGroup[_key].Play(_clip);
        }

        static public void PlayMusic(AudioClip _clip)
        {
            if (_clip == null || (Instance.mBackMusic.clip != null && Instance.mBackMusic.clip.Equals(_clip))) return;
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
            for (int i = 0; i < Instance.audioSources.Count; i++)
            {
                Instance.audioSources[i].Stop();
            }
        }

        static public void PauseSound()
        {
            for (int i = 0; i < Instance.audioSources.Count; i++)
            {
                Instance.audioSources[i].Pause();
            }
        }

        static public void UnPauseSound()
        {
            for (int i = 0; i < Instance.audioSources.Count; i++)
            {
                Instance.audioSources[i].UnPause();
            }
        }

        static public void PauseAllAudioExceptSound()
        {
            AudioSource[] tevents = FindObjectsOfType<AudioSource>();
            int tlen = tevents.Length;
            for (int i = 0; i < tlen; i++)
            {
                if (!IsChild(tevents[i]))
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
                if (!IsChild(tevents[i]))
                    tevents[i].UnPause();
            }
            UnPauseMusic();
        }
        #endregion
        static public void ClearDestoryedSound()
        {
            for (int i = Instance.audioSources.Count - 1; i >= 0; i--)
            {
                if (Instance.audioSources[i] == null)
                    Instance.audioSources.RemoveAt(i);
            }
        }

        static public void Clear()
        {
            Instance.mBackMusic.Stop();
            Instance.mBackMusic.clip = null;
            for (int i = 0; i < Instance.audioSources.Count; i++)
            {
                Instance.audioSources[i].Stop();
                Instance.audioSources[i].clip = null;
            }
        }
    }
}

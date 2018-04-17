using UnityEngine;
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

        static private float mVolume = 1;
        static public float Volume
        {
            get { return mVolume; }
            set {
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
            }
        }

        private int mMaxSoundCount = 3;

        private AudioSource mBackMusic;
        private AudioSource[] mAudioSounds;
        
        private int mIndex = 0;

        private void Awake()
        {
            mBackMusic = gameObject.AddComponent<AudioSource>();
            mAudioSounds = new AudioSource[mMaxSoundCount];
            for(int i = 0;i< mMaxSoundCount; i++)
            {
                mAudioSounds[i] = gameObject.AddComponent<AudioSource>();
            }
        }
        override protected void OnDestroy()
        {
            base.OnDestroy();
        }

        static public void PlaySound(AudioClip _clip)
        {
            if (Instance.mIndex == Instance.mMaxSoundCount) Instance.mIndex = 0;
            Instance.mAudioSounds[Instance.mIndex].Stop();
            Instance.mAudioSounds[Instance.mIndex].clip = _clip;
            Instance.mAudioSounds[Instance.mIndex].loop = false;
            Instance.mAudioSounds[Instance.mIndex].Play();
            Instance.mIndex++;
        }

        static public void PlayMusic(AudioClip _clip)
        {
            Instance.mBackMusic.Stop();
            Instance.mBackMusic.clip = _clip;
            Instance.mBackMusic.loop = true;
            Instance.mBackMusic.Play();
        }

        static public void StopMusic()
        {
            Instance.mBackMusic.Stop();
        }

        static public void StopSound()
        {
            for (int i = 0; i < Instance.mMaxSoundCount; i++)
            {
                Instance.mAudioSounds[i].Stop();
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
        }
    }
}

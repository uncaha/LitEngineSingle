using UnityEngine;
namespace LitEngine
{
    public class PlayAudioManager : MonoManagerBase
    {
        private float mMusicValue = 1;
        public float MusicValue {
            get { return mMusicValue; }
            set
            {
                mMusicValue = Mathf.Clamp01(value);
                mBackMusic.volume = mMusicValue;
            }
        }
        private float mSoundValue = 1;
        public float SoundValue
        {
            get { return mSoundValue; }
            set
            {
                mSoundValue = Mathf.Clamp01(value);
                for (int i = 0; i < mMaxSoundCount; i++)
                {
                    mAudioSounds[i].volume = mSoundValue;
                }
            }
        }

        private AudioSource mBackMusic;
        private AudioSource[] mAudioSounds;
        private int mMaxSoundCount = 3;
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

        public void PlaySound(AudioClip _clip)
        {
            if (mIndex == mMaxSoundCount) mIndex = 0;
            mAudioSounds[mIndex].Stop();
            mAudioSounds[mIndex].clip = _clip;
            mAudioSounds[mIndex].loop = false;
            mAudioSounds[mIndex].Play();
            mIndex++;
        }

        public void PlayMusic(AudioClip _clip)
        {
            mBackMusic.Stop();
            mBackMusic.clip = _clip;
            mBackMusic.loop = true;
            mBackMusic.Play();
        }

        public void StopMusic()
        {
            mBackMusic.Stop();
        }

        public void StopSound()
        {
            for (int i = 0; i < mMaxSoundCount; i++)
            {
                mAudioSounds[i].Stop();
            }
        }

        public void Clear()
        {
            mBackMusic.Stop();
            mBackMusic.clip = null;
            for (int i = 0; i < mMaxSoundCount; i++)
            {
                mAudioSounds[i].Stop();
                mAudioSounds[i].clip = null;
            }
        }

        #region static
      
        #endregion


    }
}

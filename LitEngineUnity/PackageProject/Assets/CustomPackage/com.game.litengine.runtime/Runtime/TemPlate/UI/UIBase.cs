using UnityEngine;
namespace LitEngine.TemPlate.UI
{
    public abstract class UIBase : MonoBehaviour
    {
        public int deep = 0;
        public string Key { get; protected set; }
        public bool Actived { get; private set; }
        protected bool isDisposed = false;
        public bool IsCanUpdate { get; protected set; }
        #region unity
        virtual protected void Awake()
        {

        }

        virtual protected void OnEnable()
        {
            Rest();
            IsCanUpdate = true;
        }

        virtual protected void OnDisable()
        {
            IsCanUpdate = false;
        }

        virtual protected void OnDestroy()
        {

        }
        #endregion

        #region ui
        virtual public void Init(string newKey)
        {
            Key = newKey;
        }

        virtual public void SetActive(bool _active, bool _autorelease = false)
        {
            Actived = _active;
            if (_active)
            {
                UIManager.AddShowCache(Key);
            }
            else
            {
                UIManager.AddHideCache(Key);
            }
            gameObject.SetActive(_active);
        }

        virtual public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;
            UIManager.RemoveFromAllCache(Key);
            GameObject.DestroyImmediate(this.gameObject);
        }
        virtual public void Rest()
        {
        }
        #endregion
        virtual public void PlayAudio(AudioClip _clip)
        {
            PlayAudioManager.PlaySound(_clip);
        }

        virtual public void UpdateUI(float dt)
        {

        }

        virtual public void UpdateUIDate()
        {

        }
    }

}

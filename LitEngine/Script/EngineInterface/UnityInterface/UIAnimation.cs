using UnityEngine;
namespace LitEngine
{
    namespace ScriptInterface
    {
        public enum UIAniType
        {
            None = 0,
            Show,
            Hide,
            Normal,
            Custom,
        }
        public class UIAnimator : MonoBehaviour
        {
            public UIAniType Type = UIAniType.None;
            public string State;
            public bool IsPlaying { get; protected set; }
            protected Animator mAnimator;
            protected System.Action mEndCallback;
            public bool CanPlay { get; protected set; }

            protected bool IsDone
            {
                get
                {
                    AnimatorStateInfo tstate = mAnimator.GetCurrentAnimatorStateInfo(0);
                    float ttime = Mathf.Clamp01(tstate.normalizedTime);
                    return !tstate.loop && ttime == 1f;
                }
            }
            virtual protected void Awake()
            {
                mAnimator = GetComponent<Animator>();
                if (mAnimator != null && mAnimator.enabled)
                    mAnimator.enabled = false;
                GetCanPlay();
            }

            virtual protected void GetCanPlay()
            {
                if (mAnimator == null)
                {
                    CanPlay = false;
                    return;
                }

                int hashid = Animator.StringToHash(State);
                CanPlay = mAnimator.HasState(0, hashid);
            }

            virtual public void Init(System.Action _action)
            {
                mEndCallback = _action;
            }

            virtual public bool Play(string _state)
            {
                if (Type != UIAniType.Custom) return false; 
                State = _state;
                GetCanPlay();
                return Play();
            }

            virtual public bool Play()
            {
                if (!CanPlay) return false;
                if (IsPlaying) return true;
                mAnimator.enabled = false;
                mAnimator.Stop();
                mAnimator.Rebind();
                mAnimator.Play(State, 0);
                SetEnable(true);
                return true;
            }

            virtual public void Stop()
            {
                if (!CanPlay) return;
                mAnimator.Stop();
                SetEnable(false);
            }

            virtual public void GoToEnd()
            {
                Stop();
                if (mEndCallback != null)
                    mEndCallback();
            }

            virtual protected void SetEnable(bool _active)
            {
                if (enabled == _active) return;
                IsPlaying = _active;
                enabled = _active;
            }

            virtual protected bool LateUpdate()
            {
                if (!CanPlay)
                {
                    SetEnable(false);
                    return false;
                }
                if (!IsPlaying) return false;
                mAnimator.Update(Time.deltaTime);
                if (IsDone)
                    GoToEnd();
                return true;
            }
        }
    }
}
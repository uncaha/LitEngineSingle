using UnityEngine;
namespace LitEngine
{
    namespace ScriptInterface
    {
        public class CustomAnimator : MonoBehaviour
        {
            public string State = "";
            public bool IsPlaying { get { return enabled; } }
            public bool CanPlay { get; protected set; }
            public bool UnScaledTime { get; set; }
            protected UnityEngine.Animator mAnimator;
            protected System.Action<string> mEndCallback;

            virtual protected bool IsDone
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
                UnScaledTime = false;
                mAnimator = GetComponent<UnityEngine.Animator>();
                if (mAnimator != null && mAnimator.enabled)
                    mAnimator.enabled = false;
                GetCanPlay();
                enabled = false;
            }

            virtual protected void GetCanPlay()
            {
                if (mAnimator == null)
                {
                    CanPlay = false;
                    return;
                }

                int hashid = UnityEngine.Animator.StringToHash(State);
                CanPlay = mAnimator.HasState(0, hashid);
            }

            virtual public void Init(System.Action<string> actionCall,string normalState = null)
            {
                mEndCallback = actionCall;
                if (normalState != null)
                    State = normalState;
            }

            virtual public bool Play(string _state)
            {
                State = _state;
                GetCanPlay();
                return Play();
            }

            virtual public bool Play()
            {
                if (!CanPlay) return false;
                if (IsPlaying) return true;
                mAnimator.enabled = false;
                mAnimator.Rebind();
                mAnimator.Play(State, 0);
                SetEnable(true);
                return true;
            }

            virtual public void Stop()
            {
                if (!CanPlay) return;
                mAnimator.enabled = false;
                SetEnable(false);
            }

            virtual public void GoToEnd()
            {
                Stop();
                if (mEndCallback != null)
                    mEndCallback(State);
            }

            virtual protected void SetEnable(bool _active)
            {
                if (enabled == _active) return;
                enabled = _active;
            }

            virtual protected bool LateUpdate()
            {
                if (!CanPlay)
                {
                    SetEnable(false);
                    return false;
                }
                mAnimator.Update(UnScaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
                if (IsDone)
                    GoToEnd();
                return true;
            }
        }
    }
}

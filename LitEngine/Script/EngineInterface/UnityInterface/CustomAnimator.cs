using UnityEngine;
using LitEngine.UpdateSpace;
using LitEngine.Method;
namespace LitEngine.ScriptInterface
{
    public class CustomAnimator : MonoBehaviour
    {
        public string State = "";
        public bool IsPlaying { get { return enabled; } }
        public bool CanPlay { get; protected set; }
        public bool UnScaledTime { get; set; }
        protected UnityEngine.Animator mAnimator;
        protected System.Action<string> mEndCallback;
        protected UpdateObject lateUpdateobject;
        virtual protected bool IsDone
        {
            get
            {
                if (mAnimator.runtimeAnimatorController != null)
                {
                    AnimatorStateInfo tstate = mAnimator.GetCurrentAnimatorStateInfo(0);
                    float ttime = Mathf.Clamp01(tstate.normalizedTime);
                    return !tstate.loop && ttime == 1f;
                }
                else
                {
                    return true;
                }
                
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

            lateUpdateobject = new UpdateObject("CustomAnimator", new Method_Action(AniUpdate),this);
            lateUpdateobject.MaxTime = 0;
            lateUpdateobject.Owner = GameUpdateManager.Instance.LateUpdateList;
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

        virtual public void Init(System.Action<string> actionCall, string normalState = null)
        {
            mEndCallback = actionCall;
            if (normalState != null)
                State = normalState;
        }

        virtual public bool Play(string _state)
        {
            if (IsPlaying && State != null && State.Equals(_state)) return true;
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

        virtual protected void RegLateUpdate()
        {
            if (lateUpdateobject != null)
                lateUpdateobject.RegToOwner();
        }

        virtual protected void UnRegLateUpdate()
        {
            if (lateUpdateobject != null)
                lateUpdateobject.UnRegToOwner();
        }

        virtual protected void OnDestroy()
        {
            UnRegLateUpdate();
        }

        virtual protected void OnDisable()
        {
            UnRegLateUpdate();
        }
        virtual protected void OnEnable()
        {
            RegLateUpdate();
        }

        virtual protected void AniUpdate()
        {
            if (!CanPlay)
            {
                SetEnable(false);
                return;
            }
            mAnimator.Update(UnScaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
            if (IsDone)
                GoToEnd();
        }
    }
}

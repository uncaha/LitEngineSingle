using UnityEngine;
namespace LitEngine.ScriptInterface
{
    using Event;
    public class ScriptInterfaceTriggerEvent : ScriptInterfaceTrigger
    {
        public enum TriggerTargetType
        {
            selfTrigger = 1,
            allTrigger,
            manualCall,
            selfSceneTrigger,
        }
        public TriggerTargetType targetType = TriggerTargetType.manualCall;
        public ObjectEvents Events;
        override protected void Awake()
        {
            base.Awake();
            if (Events != null)
                Events.Init(this);
        }
        override protected void OnTriggerEnter(Collider _other)
        {
            if (!CanEnter) return;
            if (mTriggerTarget != null && !mTriggerTarget.Equals(_other.transform)) return;
            if (!string.IsNullOrEmpty(TriggerTargetName) && !_other.name.Equals(TriggerTargetName)) return;
            if (mTriggerEnterTimer > Time.realtimeSinceStartup) return;

            mTriggerEnterTimer = Time.realtimeSinceStartup + mTriggerEnterInterval;
            if(mOnTriggerEnter != null)
                CallAction(mOnTriggerEnter, _other);

            OnEventEnter();
        }

        private void OnEventEnter()
        {
            if (targetType == TriggerTargetType.manualCall) return;
            if (Events != null)
                Events.OnEventEnter();
        }

        public void ManualEventEnter()
        {
            if (targetType != TriggerTargetType.manualCall) return;
            if (Events != null)
                Events.OnEventEnter();
        }

    }
}

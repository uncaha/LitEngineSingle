﻿using UnityEngine;
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
            if (targetType == TriggerTargetType.manualCall) return;
            if (mTriggerTarget != null && mTriggerTarget != _other.transform) return;
            if (!string.IsNullOrEmpty(TriggerTargetName) && !_other.name.Equals(TriggerTargetName)) return;
            if (mTriggerEnterTimer > Time.realtimeSinceStartup) return;
            mTriggerEnterTimer = Time.realtimeSinceStartup + mTriggerEnterInterval;
            CallAction(mOnTriggerEnter, _other);
            OnEventEnter();
        }

        public void OnEventEnter()
        {
            if (Events != null)
                Events.OnEventEnter();
        }
    }
}
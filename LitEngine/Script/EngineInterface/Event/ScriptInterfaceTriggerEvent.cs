using UnityEngine;
namespace LitEngine.ScriptInterface
{
    using Event;
    public class ScriptInterfaceTriggerEvent : ScriptInterfaceTrigger
    {
        public ObjectEvent EventSingle = null;
        public ObjectEventsQueue EventQueue = null;
        public ObjectEventsSynchronize EventsSynchronize = null;

        override protected void Awake()
        {
            base.Awake();
        }
        override protected void OnTriggerEnter(Collider _other)
        {
            base.mOnTriggerEnter(_other);
            if (EventSingle != null)
                EventSingle.OnEventEnter();
            if (EventQueue != null)
                EventQueue.Play();
            if (EventsSynchronize != null)
                EventsSynchronize.Play();
        }
    }
}

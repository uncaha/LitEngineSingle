using UnityEngine;
namespace LitEngine.ScriptInterface
{
    using Event;
    public class ScriptInterfaceTriggerEvent : ScriptInterfaceTrigger
    {
        public ObjectEvent Events = null;

        //synchronize
        //  public ObjectEventQueue Events = null;
        override protected void OnTriggerEnter(Collider _other)
        {
            base.mOnTriggerEnter(_other);
        }
    }
}

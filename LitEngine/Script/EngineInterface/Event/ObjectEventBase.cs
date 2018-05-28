using UnityEngine;
namespace LitEngine.ScriptInterface.Event
{
    public enum EventType
    {
        Play = 0,
        Stop,
    }
    public abstract class ObjectEventBase : MonoBehaviour
    {
        public EventType Type = EventType.Play;    
        public int Index = 0;    
        public BehaviourInterfaceBase Parent { get; private set; }
        virtual protected void Awake()
        {
            Parent = GetComponent<ScriptInterfaceTriggerEvent>();
        }

        abstract public void Play();
        abstract public void Stop();
        abstract public bool IsPlaying { get;}

        virtual public void OnEventEnter()
        {
            
            switch (Type)
            {
                case EventType.Play:
                    Play();
                    break;
                case EventType.Stop:
                    Stop();
                    break;
                default:
                    break;
            }
            if (Parent != null)
                Parent.CallScriptFunctionByNameParams("OnEventEnter", this);
        }
    }
}

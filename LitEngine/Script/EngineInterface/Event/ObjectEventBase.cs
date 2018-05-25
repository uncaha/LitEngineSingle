using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LitEngine.ScriptInterface.Event
{
    public enum EventType
    {
        Play = 0,
        Stop,
    }
    public abstract class ObjectEventBase
    {
        public EventType Type = EventType.Play;
        public Object Target;
        public string Key;
        public BehaviourInterfaceBase Parent { get; set; }

        abstract public void Init();
        abstract public void Play();
        abstract public void Stop();
        abstract public bool IsPlaying { get; }

        virtual public void OnEventEnter()
        {
            if (Parent != null)
                Parent.CallScriptFunctionByNameParams("OnEventEnter", Target, Key, Type);
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
        }
    }
}

using UnityEngine;
namespace LitEngine.ScriptInterface.Event
{
    public class ObjectEventCustom : ObjectEventBase
    {
        public UnityEngine.Object target;
        override public void Play()
        {
            if (Parent != null)
                Parent.CallScriptFunctionByNameParams("OnEventCustomEnter", this,"Play");
        }
        override public void Stop()
        {
            if (Parent != null)
                Parent.CallScriptFunctionByNameParams("OnEventCustomEnter", this, "Stop");
        }

        override public bool IsPlaying
        {
            get { return false; }
        }
    }
}

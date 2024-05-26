using UnityEngine;
namespace LitEngine.ScriptInterface.Event
{
    public class ObjectEventCallScript : ObjectEventBase
    {
        public string paramStrs;
        protected override void Awake()
        {
            base.Awake();
        }
        override public void Play()
        {
            if (Parent != null)
                Parent.CallScriptFunctionByNameParams("ObjectEventCallScriptPlay", paramStrs);
        }
        override public void Stop()
        {
            if (Parent != null)
                Parent.CallScriptFunctionByNameParams("ObjectEventCallScriptStop", paramStrs);
        }

        private bool _IsPlaying = false;
        override public bool IsPlaying { get { return _IsPlaying; } }
    }
}

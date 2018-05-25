using UnityEngine;
namespace LitEngine.ScriptInterface.Event
{
    public class ObjectEventCustom : ObjectEventBase
    {
        override public void Awake()
        {

        }

        override public void Play()
        {
        }
        override public void Stop()
        {
        }

        override public bool IsPlaying
        {
            get { return false; }
        }
    }
}

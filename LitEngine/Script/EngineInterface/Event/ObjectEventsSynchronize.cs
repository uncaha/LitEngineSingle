using UnityEngine;
namespace LitEngine.ScriptInterface.Event
{
    [System.Serializable]
    public class ObjectEventsSynchronize
    {
        public BehaviourInterfaceBase Parent = null;
        public ObjectEvent[] Events = null;
        private int maxCount = 0;
        public void Init()
        {
            if (Events == null) return;
            maxCount = Events.Length;
            for (int i = 0; i < maxCount; i++)
            {
                Events[i].Parent = Parent;
                Events[i].Init();
            }
        }

        public void Play()
        {
            if (Events == null) return;
            for (int i = 0; i < maxCount; i++)
            {
                Events[i].Play();
            }

        }
        public void Stop()
        {
            if (Events == null) return;
            for (int i = 0; i < maxCount; i++)
            {
                Events[i].Stop();
            }
        }

        public bool IsPlaying
        {
            get
            {
                bool tisplayering = false;
                for (int i = 0; i < maxCount; i++)
                {
                    if (!Events[i].IsPlaying) continue;
                    tisplayering = true;
                    break;
                }
                return tisplayering;
            }
        }
    }
}

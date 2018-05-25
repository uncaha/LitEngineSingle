using UnityEngine;
namespace LitEngine.ScriptInterface.Event
{
    [System.Serializable]
    public class ObjectEventsQueue
    {
        public BehaviourInterfaceBase Parent = null;
        public ObjectEvent[] Events = null;
        private int playIndex = 0;
        private int maxCount = 0;
        private System.Collections.IEnumerator playIEnumerator;
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
            if (Events == null || IsPlaying) return;
            IsPlaying = true;
            playIndex = 0;
            playIEnumerator = PlayQueue();
            Parent.StartCoroutine(playIEnumerator);
        }
        public void Stop()
        {
            if (Events == null) return;
            IsPlaying = false;
            if(playIEnumerator != null)
                Parent.StopCoroutine(playIEnumerator);
            playIEnumerator = null;
        }

        public bool IsPlaying
        {
            get;
            private set;
        }

        System.Collections.IEnumerator PlayQueue()
        {
            Events[playIndex].Play();
            while (true)
            {
                if (Events[playIndex].IsPlaying) yield return null;
                playIndex++;
                if (playIndex >= maxCount) break;
                Events[playIndex].Play();
            }
            IsPlaying = false;
            playIEnumerator = null;
        }
    }
}

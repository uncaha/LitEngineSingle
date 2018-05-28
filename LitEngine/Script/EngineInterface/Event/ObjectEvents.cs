using UnityEngine;
namespace LitEngine.ScriptInterface.Event
{
    [System.Serializable]
    public class ObjectEvents
    {
        public enum PlayType
        {
            Synchronize = 1,
            queue,
        }
        public EventEnterType EnterType = EventEnterType.repeatability;
        public PlayType playType = PlayType.Synchronize;
        private ObjectEventBase[] Events = null;
        private BehaviourInterfaceBase Parent = null;
        private int playCount = 0;
        private int playIndex = 0;
        private int maxCount = 0;
        private System.Collections.IEnumerator playIEnumerator;
        public void Init(ScriptInterfaceTriggerEvent newParent)
        {
            Parent = newParent;
            Events = Parent.GetComponents<ObjectEventBase>();
            if (Events == null) return;
            System.Array.Sort(Events, (a, b) => { return a.Index.CompareTo(b.Index); });
            maxCount = Events.Length;
        }
        #region queue
        public void PlayQueue()
        {
            if (Events == null || IsPlaying) return;
            playIndex = 0;
            playIEnumerator = StartQueue();
            Parent.StartCoroutine(playIEnumerator);
        }
        public void StopQueue()
        {
            if (Events == null) return;
            if (playIEnumerator != null)
                Parent.StopCoroutine(playIEnumerator);
            playIEnumerator = null;
            for (int i = 0; i < Events.Length; i++)
            {
                Events[i].Stop();
            }
        }

        System.Collections.IEnumerator StartQueue()
        {
           
            while (true)
            {
                Events[playIndex].OnEventEnter();
                while (Events[playIndex].IsPlaying)
                    yield return null;
                playIndex++;
                if (playIndex >= maxCount) break;
            }
            playIEnumerator = null;
        }

        #endregion
        #region Synchronize
        public void PlaySynchronize()
        {
            if (Events == null) return;
            for (int i = 0; i < maxCount; i++)
            {
                Events[i].OnEventEnter();
            }
        }

        public void StopSynchronize()
        {
            if (Events == null) return;
            for (int i = 0; i < maxCount; i++)
            {
                Events[i].Stop();
            }
        }
        #endregion
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

        public void OnEventEnter()
        {
            playCount++;
            if (EnterType == EventEnterType.oneshoot && playCount > 1) return;
            switch (playType)
            {
                case PlayType.Synchronize:
                    PlaySynchronize();
                    break;
                case PlayType.queue:
                    PlayQueue();
                    break;
                default:
                    break;
            }
        }
    }
}

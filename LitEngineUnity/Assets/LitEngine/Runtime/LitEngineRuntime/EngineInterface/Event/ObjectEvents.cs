using UnityEngine;
using System.Collections.Generic;
namespace LitEngine.ScriptInterface.Event
{
    [System.Serializable]
    public class ObjectEvents
    {
        public class EventsGroup
        {
            public int GroupId;
            public List<ObjectEventBase> Events { get; private set; }
            public EventsGroup(int newId)
            {
                Events = new List<ObjectEventBase>();
                GroupId = newId;
            }

            public void Play()
            {
                if (Events.Count == 0) return;
                for (int i = 0; i < Events.Count; i++)
                {
                    Events[i].OnEventEnter();
                }
            }
            public void Stop()
            {
                if (Events.Count == 0) return;
                for (int i = 0; i < Events.Count; i++)
                {
                    Events[i].Stop();
                }
            }

            public bool IsPlaying
            {
                get
                {
                    bool tisplayering = false;
                    for (int i = 0; i < Events.Count; i++)
                    {
                        if (!Events[i].IsPlaying) continue;
                        tisplayering = true;
                        break;
                    }
                    return tisplayering;
                }
            }
        }
        public enum PlayType
        {
            Synchronize = 1,
            queue,
            groupQueue,
        }
        public enum EventEnterType
        {
            oneshot = 1,
            repeatability,
        }
        public EventEnterType EnterType = EventEnterType.repeatability;
        public PlayType playType = PlayType.Synchronize;
        private ObjectEventBase[] Events = null;
        private ScriptInterfaceTriggerEvent Parent = null;
        private int playCount = 0;
        private int playIndex = 0;
        private int maxCount = 0;
        private System.Collections.IEnumerator playIEnumerator;
        private System.Collections.IEnumerator WaitEndEnumerator;
        private List<EventsGroup> Groups = null;
        public void Init(ScriptInterfaceTriggerEvent newParent)
        {
            Parent = newParent;
            Events = Parent.GetComponents<ObjectEventBase>();
            if (Events == null) return;
            maxCount = Events.Length;
            System.Array.Sort(Events, (a, b) => { return a.Index.CompareTo(b.Index); });

            #region initgroup
            Dictionary<int, EventsGroup> tgroups = new Dictionary<int, EventsGroup>();
            for (int i = 0; i < Events.Length; i++)
            {
                if (!tgroups.ContainsKey(Events[i].Index))
                    tgroups.Add(Events[i].Index, new EventsGroup(Events[i].Index));
                tgroups[Events[i].Index].Events.Add(Events[i]);
            }

            Groups = new List<EventsGroup>(tgroups.Values);
            Groups.Sort((a, b) => { return a.GroupId.CompareTo(b.GroupId); });

            #endregion
        }
        #region queue
        public void PlayQueue()
        {
            if (Events == null || Events.Length == 0 || IsPlaying) return;
            
            if (Parent.gameObject.activeInHierarchy)
            {
                playIEnumerator = StartQueue();
                Parent.StartCoroutine(playIEnumerator);
            }
                
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
            playIndex = 0;
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
        #region groupQueue
        public void PlayGroupQueue()
        {
            if (Groups == null || Groups.Count == 0 || IsPlaying) return;
            
            if (Parent.gameObject.activeInHierarchy)
            {
                playIEnumerator = StartGroupQueue();
                Parent.StartCoroutine(playIEnumerator);
            }     
        }
        public void StopGroupQueue()
        {
            if (Groups == null) return;
            if (playIEnumerator != null)
                Parent.StopCoroutine(playIEnumerator);
            playIEnumerator = null;
            for (int i = 0; i < Groups.Count; i++)
            {
                Groups[i].Stop();
            }
        }

        System.Collections.IEnumerator StartGroupQueue()
        {
            playIndex = 0;
            while (true)
            {
                Groups[playIndex].Play();
                while (Groups[playIndex].IsPlaying)
                    yield return null;
                playIndex++;
                if (playIndex >= Groups.Count) break;
            }
            playIEnumerator = null;
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
            if (EnterType == EventEnterType.oneshot)
            {
                if (playCount > 1) return;
                Parent.CanEnter = false; 
            }
            switch (playType)
            {
                case PlayType.Synchronize:
                    PlaySynchronize();
                    break;
                case PlayType.queue:
                    PlayQueue();
                    break;
                case PlayType.groupQueue:
                    PlayGroupQueue();
                    break;
                default:
                    break;
            }
            if(WaitEndEnumerator != null)
                Parent.StopCoroutine(WaitEndEnumerator);

            if (Parent.gameObject.activeInHierarchy && IsPlaying)
            {
                WaitEndEnumerator = WaitEnd();
                Parent.StartCoroutine(WaitEndEnumerator);
            }
            else
            {
                if (Parent != null)
                    Parent.CallFunctionVoid("OnGroupEventsEnd");
            }
           
        }

        private System.Collections.IEnumerator WaitEnd()
        {
            while (IsPlaying)
                yield return new WaitForSeconds(0.2f);
            if (Parent != null)
                Parent.CallFunctionVoid("OnGroupEventsEnd");
            WaitEndEnumerator = null;
        }
    }
}

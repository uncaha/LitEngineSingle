using System.Collections.Generic;
using System;
namespace LitEngine.Event
{
    public class EventDispatch
    {
        static private object lockobj = new object();
        static private EventDispatch sEventdp = null;
        static private EventDispatch Eventdp
        {
            get
            {
                if (sEventdp == null)
                {
                    lock (lockobj)
                    {
                        if (sEventdp == null)
                            sEventdp = new EventDispatch();
                    }
                }

                return sEventdp;
            }
        }

        private Dictionary<System.Type, EventGroup> mReceiver = new Dictionary<System.Type, EventGroup>();
        private Dictionary<int, ObjectGroupList> objParentlists = new Dictionary<int, ObjectGroupList>();
        private EventDispatch() { }

        static public void Reg(object pTarget, System.Type pType, Action<object> pReceiver)
        {
            if (pReceiver == null) return;
            EventGroup tgroup;
            if (!Eventdp.mReceiver.ContainsKey(pType))
            {
                tgroup = new EventGroup(pType);
                Eventdp.mReceiver.Add(pType, tgroup);
            }
            else
            {
                tgroup = Eventdp.mReceiver[pType];
            }

            tgroup.Add(pTarget, pReceiver);

            int thash = pTarget.GetHashCode();
            if (!Eventdp.objParentlists.ContainsKey(thash))
            {
                Eventdp.objParentlists.Add(thash, new ObjectGroupList(pTarget));
            }
            Eventdp.objParentlists[thash].Add(tgroup);
        }

        static public void UnReg(object pTarget, System.Type pType)
        {
            if (pTarget == null) return;
            int thash = pTarget.GetHashCode();
            if (!Eventdp.objParentlists.ContainsKey(thash)) return;
            if (Eventdp.objParentlists[thash].Remove(pType) == 0)
            {
                Eventdp.objParentlists.Remove(thash);
            }
        }

        static public void UnRegAllEvent(object pTarget)
        {
            if (pTarget == null) return;
            int thash = pTarget.GetHashCode();
            if (!Eventdp.objParentlists.ContainsKey(thash)) return;
            Eventdp.objParentlists[thash].Clear();
            Eventdp.objParentlists.Remove(thash);
        }

        static public void Refresh()
        {
            var tbackup = Eventdp.objParentlists;
            Eventdp.objParentlists = new Dictionary<int, ObjectGroupList>();
            foreach (var item in tbackup)
            {
                if (item.Value.target == null) continue;
                Eventdp.objParentlists.Add(item.Key, item.Value);
            }
        }

        static public void Send(Type keyType, object pData)
        {
            try
            {
                if (!Eventdp.mReceiver.ContainsKey(keyType)) return;
                Eventdp.mReceiver[keyType].Call(pData);
            }
            catch (Exception erro)
            {
                DLog.LogError("Event", "EventDispatch: " + erro.Message);
            }

        }
    }
}


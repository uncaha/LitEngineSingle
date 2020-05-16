using System.Collections.Generic;
using System;
namespace LitEngine.TemPlate.Event
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
        private Dictionary<object, ObjectGroupList> objParentlists = new Dictionary<object, ObjectGroupList>();
        private EventDispatch() { }

        static public void Reg(object pTarget, System.Type pType, Action<object> pReceiver)
        {
            if (pReceiver == null) return;
            EventGroup tgroup = null;
            if (!Eventdp.mReceiver.ContainsKey(pType))
            {
                Eventdp.mReceiver.Add(pType, new EventGroup(pType));
            }
            tgroup = Eventdp.mReceiver[pType];
            tgroup.Add(pTarget, pReceiver);
            

            if(!Eventdp.objParentlists.ContainsKey(pTarget))
            {
                Eventdp.objParentlists.Add(pTarget, new ObjectGroupList(pTarget));
            }
            Eventdp.objParentlists[pTarget].Add(tgroup);
        }

        static public void UnReg(object pTarget, System.Type pType)
        {
            if (pTarget == null) return;
            if (!Eventdp.objParentlists.ContainsKey(pTarget)) return;
            if (Eventdp.objParentlists[pTarget].Remove(pType) == 0)
            {
                Eventdp.objParentlists.Remove(pTarget);
            }
        }

        static public void UnRegAllEvent(object pTarget)
        {
            if (pTarget == null) return;
            if (!Eventdp.objParentlists.ContainsKey(pTarget)) return;
            Eventdp.objParentlists[pTarget].Clear();
            Eventdp.objParentlists.Remove(pTarget);
        }

        static public void Refresh()
        {
            Dictionary<object, ObjectGroupList> tbackup = Eventdp.objParentlists;
            Eventdp.objParentlists = new Dictionary<object, ObjectGroupList>();
            foreach (var item in tbackup)
            {
                if (item.Key == null) continue;
                Eventdp.objParentlists.Add(item.Key, item.Value);
            }
        }

        static public void Send(object pObject)
        {
            if (pObject == null) return;
            try
            {
                Type tkey = pObject.GetType();
                if (!Eventdp.mReceiver.ContainsKey(tkey)) return;
                Eventdp.mReceiver[tkey].Call(pObject);
            }
            catch (Exception erro)
            {
                DLog.LogError("EventDispatch: " + erro.Message);
            }
            
        }
    }
}


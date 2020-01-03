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

        private Dictionary<Enum, EventGroup> mReceiver = new Dictionary<Enum, EventGroup>();
        private Dictionary<object, ObjectGroupList> objParentlists = new Dictionary<object, ObjectGroupList>();
        private EventDispatch() { }

        static public void Reg(object pTarget, Enum _def, Action<object> pReceiver)
        {
            if (pReceiver == null) return;
            EventGroup tgroup = null;
            if (!Eventdp.mReceiver.ContainsKey(_def))
            {
                Eventdp.mReceiver.Add(_def, new EventGroup(_def));
            }
            tgroup = Eventdp.mReceiver[_def];
            tgroup.Add(pTarget, pReceiver);
            

            if(!Eventdp.objParentlists.ContainsKey(pTarget))
            {
                Eventdp.objParentlists.Add(pTarget, new ObjectGroupList(pTarget));
            }
            Eventdp.objParentlists[pTarget].Add(tgroup);
        }

        static public void UnReg(object pTarget, Enum _def)
        {
            if (pTarget == null) return;
            if (!Eventdp.objParentlists.ContainsKey(pTarget)) return;
            if (Eventdp.objParentlists[pTarget].Remove(_def) == 0)
            {
                Eventdp.objParentlists.Remove(pTarget);
            }
        }

        static public void UnRegByTarget(object pTarget)
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

        static public void Send(Enum _def, object _sendObject = null)
        {
            if (!Eventdp.mReceiver.ContainsKey(_def)) return;
            Eventdp.mReceiver[_def].Call(_sendObject);
        }
    }
}


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
        private EventDispatch() { }

        static public void Reg(object pTarget, Enum _def, Action<object> pReceiver)
        {
            if (pReceiver == null) return;
            if (!Eventdp.mReceiver.ContainsKey(_def))
                Eventdp.mReceiver.Add(_def, new EventGroup(_def));

            Eventdp.mReceiver[_def].Add(pTarget, pReceiver);
        }

        static public void UnReg(object pTarget, Enum _def)
        {
            if (pTarget == null) return;
            if (!Eventdp.mReceiver.ContainsKey(_def)) return;
            Eventdp.mReceiver[_def].Remove(pTarget);
        }

        static public void UnRegByTarget(object pTarget)
        {
            if (pTarget == null) return;
            var tlist = Eventdp.mReceiver;
            foreach (var item in tlist)
            {
                item.Value.Remove(pTarget);
            }
        }

        static public void Send(Enum _def, object _sendObject = null)
        {
            if (!Eventdp.mReceiver.ContainsKey(_def)) return;
            Eventdp.mReceiver[_def].Call(_sendObject);
        }
    }
}


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

        static public void Reg(Enum _def, Action<object> _receiver)
        {
            if (_receiver == null) return;
            if (!Eventdp.mReceiver.ContainsKey(_def))
                Eventdp.mReceiver.Add(_def, new EventGroup(_def));

            Eventdp.mReceiver[_def].Add(_receiver);
        }

        static public void UnReg(Enum _def, Action<object> _receiver)
        {
            if (_receiver == null) return;
            if (!Eventdp.mReceiver.ContainsKey(_def)) return;
            Eventdp.mReceiver[_def].Remove(_receiver);
        }

        static public void Send(Enum _def, object _sendObject = null)
        {
            if (!Eventdp.mReceiver.ContainsKey(_def)) return;
            Eventdp.mReceiver[_def].Call(_sendObject);
        }
    }
}


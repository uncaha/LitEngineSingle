
using System.Collections.Generic;
namespace LitEngine.TemPlate.Event
{
    public class EventGroup
    {
        public System.Enum Key { get; private set; }
        public List<System.Action<object>> Delgates { get; private set; }
        public EventGroup(System.Enum _key)
        {
            Key = _key;
            Delgates = new List<System.Action<object>>();
        }

        public void Add(System.Action<object> _delgate)
        {
            if(!Delgates.Contains(_delgate))
                Delgates.Add(_delgate);
        }

        public void Remove(System.Action<object> _delgate)
        {
            if (Delgates.Contains(_delgate))
                Delgates.Remove(_delgate);
        }

        public void Call(object _obj)
        {
            for (int i = Delgates.Count - 1; i >= 0 ; i--)
            {
                CallDelgate(Delgates[i], _obj);
            }
        }
        private void CallDelgate(System.Action<object> _action,object _obj)
        {
#if LITDEBUG
            try
            {
                _action(_obj);
            }
            catch (System.Exception _e)
            {
                DLog.LogError(_e.ToString());
            }
#else
            _action(_obj);
#endif
        }
    }

}

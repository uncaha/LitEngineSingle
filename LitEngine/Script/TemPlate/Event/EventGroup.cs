
using System.Collections.Generic;
namespace LitEngine.TemPlate.Event
{
    public class EventGroup
    {
        public System.Enum Key { get; private set; }
        public Dictionary<object,System.Action<object>> Delgates { get; private set; }

        protected List<object> Keys;
        public EventGroup(System.Enum _key)
        {
            Key = _key;
            Delgates = new Dictionary<object, System.Action<object>>();
        }

        public void Add(object target, System.Action<object> _delgate)
        {
            if(!Delgates.ContainsKey(_delgate))
            {
                Delgates.Add(target, _delgate);
                Keys = new List<object>(Delgates.Keys);
            }
        }

        public void Remove(object target)
        {
            if (Delgates.ContainsKey(target))
                Delgates.Remove(target);
            Keys = new List<object>(Delgates.Keys);
        }

        public void Call(object _obj)
        {
            for (int i = Keys.Count - 1; i >= 0; i--)
            {
                object tkey = Keys[i];
                var tact = Delgates[tkey];

                if (tact == null || (tact.Target == null && !tact.Method.IsStatic))
                {
                    Delgates.Remove(tkey);
                    continue;
                }
                CallDelgate(tact, _obj);
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

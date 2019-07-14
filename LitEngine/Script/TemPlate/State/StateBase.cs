using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LitEngine.TemPlate.State
{
    public class StateData
    {
        private List<object> objects;

        public StateData(params object[] _objects)
        {
            if (_objects != null)
            {
                objects = new List<object>();
                objects.AddRange(_objects);
            }
        }

        public void Add(object _data)
        {
            objects.Add(_data);
        }

        public T Get<T>(int _index)
        {
            if (objects == null || _index < 0 || _index >= objects.Count) return default(T);
            return (T)objects[_index];
        }
    }
    public abstract class StateBase
    {
        protected StateData stateData;
        abstract public void OnEnter();
        abstract public void OnQuit();
        abstract public void Dispose();
        abstract public void Update();
        virtual protected T GetData<T>(int _index)
        {
            if (stateData == null) return default(T);
            return stateData.Get<T>(_index);
        }
        virtual public void SetStateData(object[] _objects)
        {
            if (_objects == null || _objects.Length == 0) return;

            stateData = new StateData(_objects);
        }
    }
}

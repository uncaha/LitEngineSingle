﻿using UnityEngine;
using System.Collections.Generic;
namespace LitEngine.TemPlate.Event
{
    internal class EventObject
    {
        public object Target { get; private set; }
        public System.Action<object> EventDelgate { get; private set;}
        public EventObject(object pTar, System.Action<object> pDel)
        {
            Target = pTar;
            EventDelgate = pDel;
        }

        public bool IsLife
        {
            get
            {
                return (Target == null && EventDelgate.Method.IsStatic) || (Target != null && !EventDelgate.Method.IsStatic);
            }
        }

        public void Call(object pObject)
        {
            try
            {
                EventDelgate(pObject);
            }
            catch (System.Exception _e)
            {
                DLog.LogError(_e.ToString());
            }
        }

    }
}

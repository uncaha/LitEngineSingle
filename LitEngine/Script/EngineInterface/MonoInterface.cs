using System;
using UnityEngine;
namespace LitEngine
{
    namespace ScriptInterface
    {
        public interface MonoInterface
        {
            Transform transform { get; }
            GameObject gameobject { get; }
            BehaviourInterfaceBase Parent { get; }
            void Awake();
        }
    } 
}

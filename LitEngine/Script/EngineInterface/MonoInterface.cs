using System;
using UnityEngine;
namespace LitEngine.ScriptInterface
{
    public interface MonoInterface
    {
        Transform transform { get; }
        GameObject gameobject { get; }
        BehaviourInterfaceBase Parent { get; }
        void Awake();
    }
}

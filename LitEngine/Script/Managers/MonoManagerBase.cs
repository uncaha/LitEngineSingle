using UnityEngine;
using System;
namespace LitEngine
{
    public interface ManagerInterface : IDisposable
    {
    }
    public class MonoManagerBase : MonoBehaviour
    {
        virtual protected void OnDestroy()
        {
        }

        virtual public void DestroyManager()
        {
            GameObject.DestroyImmediate(gameObject);
        }
    }
}

using UnityEngine;
using System;
namespace LitEngine
{
    public interface ManagerInterface : IDisposable
    {

    }

    public abstract class MonoManagerBase : MonoBehaviour
    {

        internal void InitMgr()
        {
            Init();
        }

        virtual protected void Init()
        {
        }
    }
    
    
    public class MonoManagerGeneric<T> : MonoManagerBase where T : MonoManagerBase
    {
        protected static T sInstance = null;
        public static T Instance
        {
            get
            {
                if (sInstance == null)
                {
                    GameObject tobj = new GameObject(typeof(T).Name);
                    GameObject.DontDestroyOnLoad(tobj);
                    tobj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

                    sInstance = tobj.AddComponent<T>();
                    sInstance.InitMgr();
                }

                return sInstance;
            }
        }
        
        static public bool IsHaveInstance
        {
            get { return sInstance != null; }
        }

    }

}

using System.Collections.Generic;
using UnityEngine;

namespace LitEngine.LoadAsset
{
    public class GameObjectCacheQueue
    {
        public string AssetName { get; private set; }
        private Queue<GameObject> que = new Queue<GameObject>();

        private GameObject resObject = null;
        public GameObjectCacheQueue(string pAssetName)
        {
            AssetName = pAssetName;
            resObject = ResourcesManager.Load<GameObject>(AssetName);
        }

        public GameObject Dequeue()
        {
            if (que.Count == 0)
            {
                var ret = GameObject.Instantiate(resObject);
                return ret;
            }
            return que.Dequeue();
        }

        public void Enqueue(GameObject pObj)
        {
            if(pObj == null) return;
            pObj.SetActive(false);
            que.Enqueue(pObj);
        }

        public void Clear()
        {
            while (que.Count > 0)
            {
                var tobj = que.Dequeue();
                GameObject.Destroy(tobj);
            }

            if (resObject != null)
            {
                ResourcesManager.ReleaseAsset(AssetName);
            }
        }
    }
}
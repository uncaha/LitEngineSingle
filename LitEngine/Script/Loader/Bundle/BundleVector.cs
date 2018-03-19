using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace LitEngine
{
    namespace Loader
    {
        public class BundleVector 
        {
            private Dictionary<string, BaseBundle> mList = new Dictionary<string, BaseBundle>();
            public BundleVector()
            {
            }

            public BaseBundle this[string key]
            {
                get
                {
                    return mList[BaseBundle.DeleteSuffixName(key).ToLower()];
                }
                set
                {
                    mList[BaseBundle.DeleteSuffixName(key).ToLower()] = value;
                }
            }

            public Dictionary<string, BaseBundle>.ValueCollection values
            {
                get
                {
                    return mList.Values;
                }
            }

            public Dictionary<string, BaseBundle>.KeyCollection Keys
            {
                get
                {
                    return mList.Keys;
                }
            }

            public void Clear()
            {
                ArrayList tlist = new ArrayList(mList.Keys);
                foreach (string tkey in tlist)
                {
                    Remove(tkey);
                }
            }

            public bool Contains(string _key)
            {
                return mList.ContainsKey(BaseBundle.DeleteSuffixName(_key).ToLower());
            }

            public void Add(BaseBundle _bundle)
            {
                _bundle.Parent = this;
                mList.Add(_bundle.AssetName, _bundle);
            }

            public void Remove(BaseBundle _bundle, bool _destory = true)
            {
                if (_bundle == null)
                {
                    Debug.LogWarning("can not remove null");
                    return;
                }

                Remove(_bundle.AssetName);
            }

            public void Remove(string _key, bool _destory = true)
            {
                _key = BaseBundle.DeleteSuffixName(_key).ToLower();
                if (!Contains(_key))
                    return;
                BaseBundle tbundle = this[_key];
                tbundle.Parent = null;
                mList.Remove(BaseBundle.DeleteSuffixName(_key));              
                if (_destory)
                    tbundle.Destory();
            }

            public void ReleaseBundle(BaseBundle _bundle)
            {
                if (_bundle == null)
                {
                    Debug.LogWarning("can not release null");
                    return;
                }
                ReleaseBundle(_bundle.AssetName);
            }

            public void ReleaseBundle(string _key)
            {
                _key = BaseBundle.DeleteSuffixName(_key).ToLower();
                if (Contains(_key))
                    this[_key].Release();
                else
                    DLog.LogError( "BundleVector 尝试释放一个不存在的资源._key = " + _key);
            }
        }
    }
}

   

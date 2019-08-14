using UnityEngine;
using System.Collections.Generic;
public class SpriteArray : MonoBehaviour
{
    private bool isinit = false;
    private Dictionary<string, Sprite> spdic;
    public Sprite[] sprites;

    private void Init()
    {
        if (isinit) return;
        if (sprites != null && sprites.Length > 0)
        {
            spdic = new Dictionary<string, Sprite>();
            for (int i = 0; i < sprites.Length; i++)
            {
                if(spdic.ContainsKey(sprites[i].name))
                {
                    DLog.LogError("SpriteArray init error.重复的key : " + sprites[i].name);
                    continue;
                }
                spdic.Add(sprites[i].name, sprites[i]);
            }
        }
        isinit = true;
    }

    public Sprite this[string _name]
    {
        get
        {
            if (!isinit) Init();
            if (spdic == null || !spdic.ContainsKey(_name)) return null;
            return spdic[_name];
        }
    }

    private void OnDestroy()
    {
        if(spdic != null)
            spdic.Clear();
    }
}
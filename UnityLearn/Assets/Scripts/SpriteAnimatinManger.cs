using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteAnimatinManger
{


    private static SpriteAnimatinManger _instance = null;

    private Dictionary<string, Object[]> _cacheMap = null;
	// Use this for initialization

    public SpriteAnimatinManger()
    {
        _cacheMap = new Dictionary<string, Object[]>();
    }
    static public SpriteAnimatinManger getInstance()
    {
        if (_instance == null)
        {
            _instance = new SpriteAnimatinManger();
        }
        return _instance;
    }

    public Object[] LoadPlistResource(string file)
    {
        if (!_cacheMap.ContainsKey(file))
        {
            //加载整一张图集，此方法会返回一个Object[]，里面包含了图集的纹理 Texture2D和图集下的全部Sprite
            Object[] _atlas = Resources.LoadAll("Plist/" + file);
            _cacheMap[file] = _atlas;
        }
        return _cacheMap[file];
    }

    public void removePlistResource(string file)
    {
        Object[] _atlas = _cacheMap[file];
        if (_atlas != null)
        {
            for (int i = 1; i < _atlas.Length; i++)
            {
                unloadAsset(_atlas[i]);
            }
            _cacheMap[file] = null;
        }
    }

    public void unloadAsset(Object obj)
    {
        Resources.UnloadAsset(obj);
    }
       
}

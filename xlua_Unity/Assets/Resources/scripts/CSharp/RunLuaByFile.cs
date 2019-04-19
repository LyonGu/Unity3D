using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XLua;

/*
 
  通过文件执行lua脚本
 * 
 * lua脚本放到resources目录下，只能以txt为后缀
 */
public class RunLuaByFile : MonoBehaviour {


    LuaEnv env = null;
	// Use this for initialization
	void Start () {
        env = new LuaEnv();

        TextAsset txtAsset = Resources.Load<TextAsset>("SimpleLua.lua");
        env.DoString(txtAsset.ToString());
	}
	
	// Update is called once per frame
     void OnDestroy()
    {
        //释放luaenv
        env.Dispose();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XLua;

/*
 
  通过文件执行lua脚本
 * 
 * lua脚本放到resources目录下，只能以txt为后缀
 */
public class RunLuaByRequire : MonoBehaviour
{


    LuaEnv env = null;
	// Use this for initialization
	void Start () {
        env = new LuaEnv();

        env.DoString("require 'SimpleLua'");  //不用加lua后缀 双引号中用单引号
	}
	
	// Update is called once per frame
     void OnDestroy()
    {
        //释放luaenv
        env.Dispose();
    }
}

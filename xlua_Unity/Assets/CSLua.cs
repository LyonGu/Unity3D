using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class CSLua : MonoBehaviour {

    private LuaEnv luaEnv;



	// Use this for initialization
	void Start () {
        luaEnv = new LuaEnv();
        luaEnv.DoString("print('lua形式打印输出')");
        luaEnv.DoString("CS.UnityEngine.Debug.Log('cs 形式打印输出')");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void onDestory()
    {
        luaEnv.Dispose();
    }
}

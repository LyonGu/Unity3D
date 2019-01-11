using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuaInterface;

public class TestLua : MonoBehaviour {

	// Use this for initialization
	void Start () {

        LuaState lua = new LuaState();
        lua.Start();
        string hello =
            @"                
                print('hello tolua')                                  
            ";

        lua.DoString(hello, "HelloWorld.cs");
        lua.CheckTop();
        lua.Dispose();
        lua = null;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

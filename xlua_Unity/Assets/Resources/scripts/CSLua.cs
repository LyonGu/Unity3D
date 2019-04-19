using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using XLua;

public class CSLua : MonoBehaviour {

    private LuaEnv luaEnv;



	// Use this for initialization
	void Start () {
        luaEnv = new LuaEnv();
     

        luaEnv.DoString("print('lua形式打印输出')");
        luaEnv.DoString("CS.UnityEngine.Debug.Log('cs 形式打印输出')");

        //只能包含Resources下的 xxx.lua.txt
        luaEnv.DoString(@"require 'test1'");
        //luaEnv.DoString(@"require 'main'");


        /*
         //添加自定义的load
            luaEnv.AddLoader(LuaLoader);
            luaEnv.DoString("require 'Lua/test2.lua'"); //文件名为test2.lua.txt 也要是txt后缀
            
         * 
         * 
        //luaEnv.AddLoader((ref string filename) =>
        //{
        //    if (filename == "InMemory")
        //    {
        //        string script = "return {ccc = 9999}";
        //        return System.Text.Encoding.UTF8.GetBytes(script);
        //    }
        //    return null;
        //});
        //luaEnv.DoString("print('InMemory.ccc=', require('InMemory').ccc)");
 
         */

        luaEnv.AddLoader(MyCustomLoader);
        luaEnv.DoString("require 'main'");//直接引用lua文件，推荐使用这种方式

        //GameObject  game = GameObject.Find("Game");
        //GameObject hello = new GameObject("HelloWorld");
        //hello.transform.parent = game.transform;

    }

    //引用非Resources目录下的lua文件，不用后缀是txt
    private byte[] LuaLoader(ref string filepath)
    {
        TextAsset script = Resources.Load(filepath) as TextAsset; 
       return script.bytes;
    }

    private byte[] MyCustomLoader(ref string filepath)
    {
        // 通过自定义filepath的解析方式来实现特殊加载功能

        // 1. 从指定的路径加载Lua文件
        filepath = Application.dataPath + "/Resources/" + filepath + ".lua";
        if (File.Exists(filepath))
        {
            return File.ReadAllBytes(filepath);
            //string script = File.ReadAllText(filepath);
            //return System.Text.Encoding.UTF8.GetBytes(script);
        }
        // 2. 从自定义的默认位置加载Lua文件
        else
        {
            string defaultFolder = Application.dataPath + "/myluafiles/";
            string file = defaultFolder + filepath + ".lua";
            if (File.Exists(file))
            {
                return File.ReadAllBytes(file);
            }
        }

        // 其他加载方式：
        // 3. 加载网络文件
        // 4. 加载压缩文件并解压
        // 5. 加载加密文件并解密

        return null;
    }





	// Update is called once per frame
	void Update () {
		
	}

    void onDestory()
    {
        luaEnv.Dispose();
    }
}

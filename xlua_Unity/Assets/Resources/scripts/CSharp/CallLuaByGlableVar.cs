using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XLua;
using System.IO;


public class CallLuaByGlableVar : MonoBehaviour {

    LuaEnv env = null;

    // Use this for initialization
    void Start()
    {
        env = new LuaEnv();

        env.AddLoader(CustomMyLoader);

        //只加载一个主文件，然后其他lua文件在main文件里引用
        //main.lua里用require包含的lua文件也会调用自定义的lua加载器CustomMyLoader
        env.DoString("require 'GameMain'");

        testCSharpCallLua();
    }

    public void testCSharpCallLua()
    {
        string str1 = env.Global.Get<string>("str");
        int age = env.Global.Get<int>("age");
        float floNumber = env.Global.Get<float>("floNumber");
        bool IsFisrtTime = env.Global.Get<bool>("IsFisrtTime");
        Debug.Log("str1=========" + str1);
        Debug.Log("age=========" + age);
        Debug.Log("floNumber=========" + floNumber);
        Debug.Log("IsFisrtTime=========" + IsFisrtTime);
    }


    private byte[] CustomMyLoader(ref string fileName)
    {

        fileName = fileName.Replace(".", "/");
        byte[] byArrayReturn = null; //返回数据
        //定义lua路径
        string luaPath = Application.dataPath + "/Resources/scripts/LuaScripts/" + fileName + ".lua";
        //读取lua路径中指定lua文件内容
        string strLuaContent = File.ReadAllText(luaPath);
        //数据类型转换
        byArrayReturn = System.Text.Encoding.UTF8.GetBytes(strLuaContent);

        return byArrayReturn;
    }

    private void OnDestroy()
    {
        //释放luaenv
        env.Dispose();
    }
}

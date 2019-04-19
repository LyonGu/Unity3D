using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XLua;
using System.IO;


/*
    C#--> lua
 *   方式1： 使用class(struct)来映射得到lua中的table内容。值拷贝
 
 */
public class CallLuaTableByClass : MonoBehaviour {

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
        //得到lua中的表信息
        GameLanguage gameLan = env.Global.Get<GameLanguage>("gameLanguage");

        //输出
        Debug.Log("gameLan.str1="+ gameLan.str1);
        Debug.Log("gameLan.str1=" + gameLan.str2);
        Debug.Log("gameLan.str1=" + gameLan.str3);
        Debug.Log("gameLan.str1=" + gameLan.str4);

        //演示class 映射的值拷贝原理
        gameLan.str1 = "我是修改过的编程语言";
        //lua语言中是否改过来，测试下。
        env.DoString("print('修改后的gameLanguage.str1='..gameLanguage.str1)");//结果是： “C#语言”

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

    //定义内部类
    public class GameLanguage
    {

        public string str1;
        public string str2;
        public string str3;
        public string str4;
    }
}

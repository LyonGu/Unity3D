using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XLua;
using System.IO;


/*
    C#--> lua
 *     使用luaTable 方式进行映射。

 Description:
            优点： 功能强大， 且使用方便。
 *          劣势:  效率低。（不推荐使用）
 
 */
public class CallLuaTableByLuaTable : MonoBehaviour {

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
        

       LuaTable tabGameUser = env.Global.Get<LuaTable>("gameUser");
       //输出显示
       Debug.Log("name=" + tabGameUser.Get<string>("name"));
       Debug.Log("Age=" + tabGameUser.Get<int>("age"));
       Debug.Log("ID=" + tabGameUser.Get<string>("ID"));

       //输出表中函数
       LuaFunction funSpeak = tabGameUser.Get<LuaFunction>("Speak");
       funSpeak.Call();
       LuaFunction funWalking = tabGameUser.Get<LuaFunction>("Walking");
       funWalking.Call();

       LuaFunction funCalulation = tabGameUser.Get<LuaFunction>("Calulation");
       object[] objArray = funCalulation.Call(tabGameUser, 10, 20); //返回值是用object[]来获取的
       Debug.Log("输出结果=" + objArray[0]);//输出结果： 70

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

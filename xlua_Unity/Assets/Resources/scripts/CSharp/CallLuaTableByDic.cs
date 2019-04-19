using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XLua;
using System.IO;


/*
    C#--> lua
 *    如果lua中的表比较简单，可以使用Dictionary<> ，或者list<> 之间映射。 引用拷贝

 Description:
 *          优点： 编写简单，效率不错。
 *          缺点： 无法映射lua中的复杂Table
 
 */
public class CallLuaTableByDic : MonoBehaviour {

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
        //得到lua中的简单表信息 object 可以映射多种类型
        Dictionary<string, object> gameLan = env.Global.Get<Dictionary<string, object>>("gameLanguage");
       
        // //输出
        Debug.Log("gameLan.str1=" + gameLan["str1"]);
        Debug.Log("gameLan.str2=" + gameLan["str2"]);
        Debug.Log("gameLan.str3=" + gameLan["str3"]);
        Debug.Log("gameLan.str4=" + gameLan["str4"]);

        //  //演示class 映射的值拷贝原理
        gameLan["str1"] = "我是修改过的编程语言";

        Debug.Log("修改后的gameLan.str1=" + gameLan["str1"]);


        //得到一个更加简单lua表，使用List<> 来映射。
        List<string> liProLan=env.Global.Get<List<string>>("programLanguage");
        
        //输出List<> 映射的结果
        Debug.Log("常用编程语言： ");
        foreach (string item in liProLan)
        {
            Debug.Log(item);
        }

       

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

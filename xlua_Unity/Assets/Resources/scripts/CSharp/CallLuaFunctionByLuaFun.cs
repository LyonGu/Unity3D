using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XLua;
using System.IO;


/*
    通过LuaFunction来调用lua文件中的方法，优点使用简单，缺点效率低，不推荐使用
 
 */
public class CallLuaFunctionByLuaFun : MonoBehaviour {

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
        //得到lua中的函数信息（通过LuaFunction来进行映射）
        LuaFunction luaFun = env.Global.Get<LuaFunction>("ProcMyFunc1");
        LuaFunction luaFun2 = env.Global.Get<LuaFunction>("ProcMyFunc2");
        LuaFunction luaFun3 = env.Global.Get<LuaFunction>("ProcMyFunc3");
        //调用具有多返回数值。
        LuaFunction luaFun4 = env.Global.Get<LuaFunction>("ProcMyFunc5");

        luaFun.Call();
        luaFun2.Call(1, 2);

        //有返回值的方法用 object[]来获取
        object[] objArray = luaFun3.Call(1, 2);
        Debug.Log(string.Format("luaFun3测试多返回数值 res1={0}", objArray[0]));

        object[] objArray2 = luaFun4.Call(22, 80);
        Debug.Log(string.Format("luaFun4测试多返回数值 res1={0},res2={1},res3={2}", objArray2[0], objArray2[1], objArray2[2]));
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

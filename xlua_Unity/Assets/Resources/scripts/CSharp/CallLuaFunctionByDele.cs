using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using System.IO;
using System;


/*
    C#--> lua
方式一： 采用delegate 方式。（官方推荐方式，效率高） 必须让xlua生成代码
 */
public class CallLuaFunctionByDele : MonoBehaviour
{

    LuaEnv env = null;

    //自定义委托
    public delegate void delAdding(int num1, int num2);
    Action act = null;
    delAdding act2 = null;

   
  
    //注意： 以下两种委托定义，需要配置文件支持。 xlua/Examples/ExampleGenConfig.cs, 真实项目中需要拷贝出这个文件
    Action<int, int, int> act3 = null;
    Func<int, int, int> act4 = null;



    //自定义委托(使用out关键字) 使用ref/out关键字需要打标签  
    //ref 既代表输入也代表输出 out代表输出
    [CSharpCallLua]
    public delegate void delAddingMutilReturn(int num1, int num2, out int res1, out int res2, out int res3);
    [CSharpCallLua]
    public delegate void delAddingMutilRetRef(ref int num1, ref int num2, out int result);


    delAddingMutilReturn act5 = null;
    delAddingMutilRetRef act6 = null;

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

        //得到lua中的函数信息（通过委托来进行映射）
        act = env.Global.Get<Action>("ProcMyFunc1");
        //使用自定义委托调用具备两个输入参数的lua中的函数
        act2 = env.Global.Get<delAdding>("ProcMyFunc2");

        act();
        act2(1,33);

        //定义三个输入参数的委托。
        act3 = env.Global.Get<Action<int, int, int>>("ProcMyFunc4");
        //定义具备返回数值，两个输入数值的委托
        act4 = env.Global.Get<Func<int, int, int>>("ProcMyFunc3");

        act3(20, 30, 40);
        int intResult = act4(60, 40);
        Debug.Log("Func 委托，输出结果=" + intResult);

        //多返回值函数 
        //得到lua中的具有多个返回数值的函数（通过委托out关键字来进行映射）
        act5 = env.Global.Get<delAddingMutilReturn>("ProcMyFunc5");

        //输出返回结果
        int intOutRes1 = 0;
        int intOutRes2 = 0;
        int intOutRes3 = 0;
        act5(100, 880, out intOutRes1, out intOutRes2, out intOutRes3);
        Debug.Log(string.Format("使用out关键字，测试多输出 res1={0},res2={1},res3={2}", intOutRes1, intOutRes2, intOutRes3));


        //得到lua中的具有多个返回数值的函数（通过委托ref关键字来进行映射）
        act6 = env.Global.Get<delAddingMutilRetRef>("ProcMyFunc5");

        //输出返回结果
        int intNum1 = 20;
        int intNum2 = 30;
        intResult = 0;

        act6(ref intNum1, ref intNum2, out intResult);
        Debug.Log(string.Format("使用ref关键字，测试多输出 res1={0},res2={1},res3={2}", intNum1, intNum2, intResult));

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
        act = null;
        act2 = null;
        act3 = null;
        act4 = null;
        act5 = null;
        act6 = null;
        env.Dispose();
    }


}

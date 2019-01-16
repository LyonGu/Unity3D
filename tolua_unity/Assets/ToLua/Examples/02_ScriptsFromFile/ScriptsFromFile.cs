using UnityEngine;
using System.Collections;
using LuaInterface;
using System;
using System.IO;

//展示searchpath 使用，require 与 dofile 区别
public class ScriptsFromFile : MonoBehaviour 
{
    LuaState lua = null;
    private string strLog = "";  
    private LuaLooper looper = null;  

	void Start () 
    {
#if UNITY_5 || UNITY_2017 || UNITY_2018		
        Application.logMessageReceived += Log;
#else
        Application.RegisterLogCallback(Log);
#endif         
        new LuaResLoader();
        lua = new LuaState();                
        lua.Start();    

        LuaBinder.Bind(lua);
        DelegateFactory.Init();         
        looper = gameObject.AddComponent<LuaLooper>();
        looper.luaState = lua;

        //如果移动了ToLua目录，自己手动修复吧，只是例子就不做配置了
        string fullPath = Application.dataPath + "\\ToLua/Examples/02_ScriptsFromFile";
        lua.AddSearchPath(fullPath);        
    }

    void Log(string msg, string stackTrace, LogType type)
    {
        strLog += msg;
        strLog += "\r\n";
    }

    void OnGUI()
    {
        GUI.Label(new Rect(100, Screen.height / 2 - 100, 600, 400), strLog);

        if (GUI.Button(new Rect(50, 50, 120, 45), "DoFile"))
        {
            strLog = "";
            lua.DoFile("ScriptsFromFile.lua");                        
        }
        else if (GUI.Button(new Rect(50, 150, 120, 45), "Require"))
        {
            strLog = "";   

            lua.Require("ScriptsFromFile"); 

            //Debugger.Log("Read var from lua: {0}", lua["var2read"]);

            //函数调用 
            LuaFunction func = lua.GetFunction("testCall");
            func.BeginPCall(); 
            func.Push(123456);
            func.Push(gameObject);
            func.PCall();
            func.EndPCall();
            func.Dispose();  

            func = lua.GetFunction("testCall1");  
            func.BeginPCall(); 
            func.Push(1);
            func.PCall();
            double arg1 = func.CheckNumber();
            Debugger.Log("return is {0}", arg1);
        // string arg2 = func.CheckString();
            func.EndPCall();
            func.Dispose();  


            func = lua.GetFunction("testCall2");  
            func.BeginPCall(); 
            func.Push(1);
            func.PCall();
             arg1 = func.CheckNumber();
            string arg2 = func.CheckString();
            Debugger.Log("return is {0} {1}", arg1,arg2);
        // string arg2 = func.CheckString();
            func.EndPCall();
            func.Dispose();              
        }

        lua.Collect();
        lua.CheckTop();
    }

    void OnApplicationQuit()
    {
        lua.Dispose();
        lua = null;
#if UNITY_5 || UNITY_2017 || UNITY_2018	
        Application.logMessageReceived -= Log;
#else
        Application.RegisterLogCallback(null);
#endif 
    }
}

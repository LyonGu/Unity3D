using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

public class XluaCallCSharp : MonoBehaviour {

    LuaMemoryLeakChecker.Data data = null;
    int tick = 0;
    LuaEnv env = null;

    // Use this for initialization
    void Start () {
        env = new LuaEnv ();

        env.AddLoader (CustomMyLoader);

        //只加载一个主文件，然后其他lua文件在main文件里引用
        //main.lua里用require包含的lua文件也会调用自定义的lua加载器CustomMyLoader
        env.DoString ("require 'GameMain'");

        data = env.StartMemoryLeakCheck ();
        Debug.Log ("Start, PotentialLeakCount:" + data.PotentialLeakCount);

    }

    private byte[] CustomMyLoader (ref string fileName) {

        fileName = fileName.Replace (".", "/");
        byte[] byArrayReturn = null; //返回数据
        //定义lua路径
        string luaPath = Application.dataPath + "/Resources/scripts/LuaScripts/" + fileName + ".lua";
        //读取lua路径中指定lua文件内容
        string strLuaContent = File.ReadAllText (luaPath);
        //数据类型转换
        byArrayReturn = System.Text.Encoding.UTF8.GetBytes (strLuaContent);

        return byArrayReturn;
    }

    private void OnDestroy () {
        //释放luaenv
        env.Dispose ();
    }

    private void Update () {
        tick++;
        env.Tick ();
        if (tick % 30 == 0) {
            data = env.MemoryLeakCheck (data);
            Debug.Log ("Update, PotentialLeakCount:" + data.PotentialLeakCount);
        }

        if (tick % 180 == 0) {
            Debug.Log (env.MemoryLeakReport (data));
            data = env.StartMemoryLeakCheck ();
        }

    }
}
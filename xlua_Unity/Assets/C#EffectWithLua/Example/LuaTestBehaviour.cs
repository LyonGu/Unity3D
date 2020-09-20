using UnityEngine;
using XLua;
using System;
using System.IO;


// 简单演示如何在C#访问lua数组
[LuaCallCSharp]
public class LuaTestBasicExample
{
    private LuaArrAccess Table;

    public void CSharpExample(LuaArrAccess arrAccess)
    {
        Debug.Assert(arrAccess != null);

        Table = arrAccess;

        arrAccess.SetDouble(123, 123.5);
        arrAccess.GetInt(1);
    }
}


// 测试C#中能否正确处理int/double
[LuaCallCSharp]
public class LuaTestIntDoubleUsage
{
    private LuaArrAccess Table;

    private static bool DoubleEqual(double A, double B)
    {
        return System.Math.Abs(A - B) < 0.00001;
    }

    public void PinTable(LuaArrAccess TablePin)
    {
        Debug.Assert(TablePin != null);

        Table = TablePin;
        Debug.Assert(Table.IsValid() == true);
        TestCapacity();
        Get1();
        Get123();
        TestSet();
    }

    private void TestCapacity()
    {
        Debug.Log("Table.GetArrayCapacity()=" + Table.GetArrayCapacity());
        Debug.Assert(Table.GetArrayCapacity() == 128);
    }

    private void Get1()
    {
        Debug.Log("Table.GetInt(1)=" + Table.GetInt(1).ToString());
        Debug.Assert(Table.GetInt(1) == 111);
        Debug.Log("Table.GetDouble(1)=" + Table.GetDouble(1).ToString());
        Debug.Assert(DoubleEqual(Table.GetDouble(1), 111));
    }

    private void Get123()
    {
        Debug.Log("Table.GetDouble(123)=" + Table.GetDouble(123).ToString());
        Debug.Assert(DoubleEqual(Table.GetDouble(123), 321.5));
        Debug.Log("Table.GetInt(123)=" + Table.GetInt(123).ToString());
        Debug.Assert(Table.GetInt(123) == 321);
    }

    private void TestSet()
    {
        Table.SetDouble(1, 99999.5);
        Table.SetInt(123, 123);
        Debug.Assert(Table.GetInt(1) == 99999);
        Debug.Assert(Table.GetInt(123) == 123);
    }

    public void Step1()
    {
        Debug.Assert(Table.GetInt(123) == 123);
        Debug.Assert(Table.GetDouble(123) == 123);
        //Debug.Assert(Table.GetDouble(124) == 0);
        Table.SetDouble(124, 31222);
        Debug.Assert(Table.GetDouble(124) == 31222);
    }

    public void Step2()
    {
        Debug.Assert(Table.IsValid() == false);
    }
}


// 测试数组动态扩充
[LuaCallCSharp]
public class LuaTestArrayExpand
{
    private LuaArrAccess Table;

    public void PinTable(LuaArrAccess TablePin)
    {
        Table = TablePin;
        Debug.Assert(Table.IsValid() == true);
        uint ArrSize = Table.GetArrayCapacity();
        Debug.Assert(ArrSize == 128);// array size is always power of 2
        Table.SetInt(11, 321);
    }

    public void Check()
    {
        Debug.Assert(Table.GetArrayCapacity() == 512);
        int Tmp = Table.GetInt(123);
        Debug.Assert(Tmp == 0);
        Debug.Assert(Table.GetInt(11) == 321);
        Tmp = Table.GetInt(512);
        Debug.Assert(Tmp == 512);

        Debug.Log("LuaTestArrayExpand is done");
    }
}


// 测试lua数组被gc后C#是否会保持内存安全
[LuaCallCSharp]
public class LuaTestGCSafety
{
    private LuaArrAccess Table;

    public void PinTable(LuaArrAccess TablePin)
    {
        Table = TablePin;
        Debug.Log("PinTable" + Table.ToString());
        Debug.Assert(Table.IsValid() == true);
    }

    public void Check()
    {
        Debug.Assert(Table.IsValid() == false, Table.ToString());

        Debug.Log("LuaTestGCSafety is done");
    }
}

[LuaCallCSharp]
public class LuaTestPerf
{
    private LuaArrAccess Table;

    public void PinTable(LuaArrAccess TablePin)
    {
        Table = TablePin;
    }

    public void Step1(int count)
    {
        LuaJitArrAccess TableJit = Table as LuaJitArrAccess;
        if(TableJit != null)
        {
            for (int i = 1; i < count; i++)
            {
                TableJit.SetInt(i, i);
                TableJit.GetIntFast(i);
            }
        }
        else
        {
            for (int i = 1; i < count; i++)
            {
                Table.SetInt(i, i);
                Table.GetInt(i);
            }
        }
        
    }
}




[LuaCallCSharp]
public class LuaTestBehaviour : MonoBehaviour
{
    public TextAsset luaScript;

    internal static LuaEnv luaEnv = new LuaEnv(); //all lua behaviour shared one luaenv only!
    internal static float lastGCTime = 0;
    internal const float GCInterval = 1;//1 second 

    private Action luaStart;
    private Action luaUpdate;
    private Action luaOnDestroy;

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

    public void SetCS(int idx, int v)
    {

    }

    public int GetCS(int idx)
    {
        return 1;
    }

    private LuaTable scriptEnv;

    void Awake()
    {
        luaEnv.AddLoader(CustomMyLoader);
        scriptEnv = luaEnv.NewTable();
        LuaTable meta = luaEnv.NewTable();
        meta.Set("__index", luaEnv.Global);
        scriptEnv.SetMetaTable(meta);
        meta.Dispose();

        scriptEnv.Set("self", this);

        // 重要：初始化LuaCSharpArr
        LuaArrAccessAPI.RegisterPinFunc(luaEnv.L);

        //luaEnv.DoString(luaScript.text, "LuaBehaviour", scriptEnv);

        luaEnv.DoString ("require 'LuaTestScript'", "LuaBehaviour", scriptEnv);

        Action luaAwake = scriptEnv.Get<Action>("awake");
        scriptEnv.Get("start", out luaStart);
        scriptEnv.Get("update", out luaUpdate);
        scriptEnv.Get("ondestroy", out luaOnDestroy);

        if (luaAwake != null)
        {
            luaAwake();
        }
    }

    // Use this for initialization
    void Start()
    {
        if (luaStart != null)
        {
            luaStart();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (luaUpdate != null)
        {
            luaUpdate();
        }
        if (Time.time - LuaTestBehaviour.lastGCTime > GCInterval)
        {
            luaEnv.Tick();
            LuaTestBehaviour.lastGCTime = Time.time;
        }
    }

    void OnDestroy()
    {
        if (luaOnDestroy != null)
        {
            luaOnDestroy();
        }
        luaOnDestroy = null;
        luaUpdate = null;
        luaStart = null;
        scriptEnv.Dispose();
    }
}
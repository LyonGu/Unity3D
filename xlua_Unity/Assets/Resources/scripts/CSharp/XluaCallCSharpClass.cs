using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class XluaCallCSharpClass : XluaCallCSharpParentClass
{

    public int childAge1 = 10;
    private string name = "hxp";
    private static XluaCallCSharpClass _instance = null;

    private delegate void xluaCallBack();
    xluaCallBack _callBack;

    //定义结构体(建议结构体成员为小写)
    public struct MyStruct
    {
        public string x;
        public string y;
      
    }

    //定义接口
    [CSharpCallLua]
    public interface MyInterface
    {
        //接口要使用属性的形式
        int x { get; set; }
        int y { get; set; }
        void Speak();
        void SpeakSelf();
      
    }

    //定义委托
    [CSharpCallLua]
    public delegate void MyDelegate(int num);


    static public  XluaCallCSharpClass getInstance()
    {
        if (_instance == null)
        {
            _instance = new XluaCallCSharpClass();
        }
        return _instance;
    }


    public void xluaCallCSharp1()
    {
        Debug.Log(GetType() + "/xluaCallCSharp1 :从xlua调用过来");
    }

    void xluaCallCSharp2()
    {
        Debug.Log(GetType() + "/xluaCallCSharp2 :从xlua调用过来");
    }

    public int xluaCallCSharp3(int num)
    {
        return num + 1;
    }

    public string xluaCallCSharp3(string name)
    {
        return name;
    }


    public void xluaCallCSharp4(params object[] values)
    {

        //string value0 = (string)values[0];
        //string value1 = (string)values[1];
        //string value2 = (string)values[2];
        //Debug.Log(GetType() + "/value======" + value0);
        //Debug.Log(GetType() + "/value======" + value1);
        //Debug.Log(GetType() + "/value======" + value2);

        //_callBack = (xluaCallBack)values[3];
        //_callBack();

        //string value0 = values[0] as string;

        //Debug.Log(GetType() + "/value======" + value0);
        _callBack = (xluaCallBack)values[3];
        _callBack();
    }


    //带有结构体参数的方法
    public void xluaCallCSharp5(MyStruct p)
    {
        Debug.Log("测试lua调用结构体方法");
        Debug.Log("p.x=" + p.x);
        Debug.Log("p.y=" + p.y);
    }

    //方法具有接口为参数的
    public void xluaCallCSharp6(MyInterface p)
    {
        Debug.Log("测试lua调用具有接口为参数的方法");
        Debug.Log("p.x=" + p.x);
        Debug.Log("p.y=" + p.y);
        p.Speak();
        p.SpeakSelf();
    }


    //方法具有委托为参数
    public void xluaCallCSharp7(MyDelegate p)
    {
        Debug.Log(GetType() + "/Method6()/委托参数:");
        //调用
        p.Invoke(88);
    }

    ////方法具有委托为参数
    public void xluaCallCSharp8(MyInterface p1, MyDelegate p2)
    {
        Debug.Log("xluaCallCSharp8 p.x=" + p1.x);
        Debug.Log("xluaCallCSharp8 p.y=" + p1.y);
        p1.Speak();
        p1.SpeakSelf();
        //调用
        p2.Invoke(88);
    }


    //定义一个具有多返回数值的方法
    public int xluaCallCSharp9(int num1, out int num2, ref int num3)
    {
        Debug.Log(GetType() + "/xluaCallCSharp9()/测试lua接收C#的多返回数值");
        num2 = 3000;
        num3 = 999;
        return num1 + 100;
    }


    


}

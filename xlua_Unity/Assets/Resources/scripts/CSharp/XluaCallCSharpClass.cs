using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XluaCallCSharpClass
{

    private static XluaCallCSharpClass _instance = null;

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
   

    


}

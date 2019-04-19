using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XluaCallCSharpParentClass
{

    public int parentAge = 100;


    public void xluaCallCSharp1Parent()
    {
        Debug.Log(GetType() + "/xluaCallCSharp1Parent :从xlua调用过来");
    }

    void xluaCallCSharp2Parent()
    {
        Debug.Log(GetType() + "/xluaCallCSharp2Parent :从xlua调用过来");
    }

    public int xluaCallCSharp3Parent(int num)
    {
        return num + 1;
    }
   
}

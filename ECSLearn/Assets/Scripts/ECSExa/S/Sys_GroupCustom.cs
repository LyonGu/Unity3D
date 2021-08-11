using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Scripting;

public class Sys_GroupCustom: ComponentSystemGroup
{
    // Start is called before the first frame update
    public Sys_GroupCustom()
    {
    }
}


/*
 * 自定义的group只能保证update的顺序，不能保证OnCreate的顺序, 
 * 怎样定义system的OnCreate调用顺序 TODO
 * 
 */



[UpdateInGroup(typeof(InitializationSystemGroup))]
public class Sys_customeOne : SystemBase
{

    protected override void OnCreate()
    {
        Debug.Log($"Sys_customeOne====={UnityEngine.Time.frameCount}");
    }

    protected override void OnUpdate()
    {
//        Debug.Log($"Sys_customeOne Update====={UnityEngine.Time.frameCount}");
    }
}

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateBefore(typeof(Sys_customeOne))]
public class Sys_customeTwo : SystemBase
{

    protected override void OnCreate()
    {
        Debug.Log($"Sys_customeTwo====={UnityEngine.Time.frameCount}");
    }

    protected override void OnUpdate()
    {
//        Debug.Log($"Sys_customeTwo Update====={UnityEngine.Time.frameCount}");
    }
}
//
//[UpdateInGroup(typeof(Sys_GroupCustom))]
//[UpdateBefore(typeof(Sys_customeOne))]
//public class Sys_customeThree : SystemBase
//{
//
//    protected override void OnCreate()
//    {
//        Debug.Log($"Sys_customeThree@@====={UnityEngine.Time.frameCount}");
//    }
//
//    protected override void OnUpdate()
//    {
//        
//    }
//}



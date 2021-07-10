using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class MenuCommand_EX : Editor
{
    //用于获取当前操作的组件
    [MenuItem("CONTEXT/Transform/Init")]
    static void Init(MenuCommand cmd)
    {
        Transform tran = cmd.context as Transform;
        Debug.Log($"当前操作对象名称是 {tran.name}");
    }
}

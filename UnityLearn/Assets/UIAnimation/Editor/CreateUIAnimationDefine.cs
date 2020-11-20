using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class CreateUIAnimationDefine : MonoBehaviour {

    [MenuItem("UIAnimation/构建ADE", false, 3)]
    public static void DelAssetBundle()
    {
        Assembly assem = Assembly.Load("Assembly-CSharp");
        //通过反射拿到要生成asset的类//
        Type Serialize = assem.GetType("UIAnimationDefine");
        //根据类名创建一个ScriptableObject 实例//
        object serializeObj = ScriptableObject.CreateInstance(Serialize);

        AssetDatabase.CreateAsset(serializeObj as UnityEngine.Object, FileUtil.GetProjectRelativePath(Application.dataPath + "/UIAnimationDefine.asset"));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}

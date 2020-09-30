using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



//必须继承ShaderGUI，然后重载OnGUI方法
public class CustomShaderGUI : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        // render the default gui 先默认渲染一遍
        base.OnGUI(materialEditor, properties);

        Material targetMat = materialEditor.target as Material;

        // see if redify is set, and show a checkbox
        bool CS_BOOL = Array.IndexOf(targetMat.shaderKeywords, "CS_BOOL") != -1;

        EditorGUI.BeginChangeCheck();
        CS_BOOL = EditorGUILayout.Toggle("CS_BOOL", CS_BOOL);

        if (EditorGUI.EndChangeCheck())
        {
            // enable or disable the keyword based on checkbox
            if (CS_BOOL)
                //启用变体CS_BOOL
                targetMat.EnableKeyword("CS_BOOL");
            else
                //禁用变体CS_BOOL
                targetMat.DisableKeyword("CS_BOOL");
        }
    }
}

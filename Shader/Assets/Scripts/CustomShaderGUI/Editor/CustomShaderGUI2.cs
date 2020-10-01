using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CustomShaderGUI2 : ShaderGUI
{
    public enum ShadingMode
    { 
        Simple,
        Reflective,
    }

    ShadingMode ShadingModeVal = ShadingMode.Simple;
    bool CheckBoxValue = false;

    private void SetKeyword(Material material, string keyword, bool state)
    {
        if (state)
        {
            material.EnableKeyword(keyword);     
        }
        else
        {
            material.DisableKeyword(keyword);
        }
    }

    private ShadingMode GetMaterialShadingMode(Material material)
    {
        ShadingMode Ret = ShadingMode.Simple;
        if (Array.IndexOf(material.shaderKeywords, "_SHADEMODE_SIMPLE") != -1)
        {
            Ret = ShadingMode.Simple;
            return Ret;
        }
        else if (Array.IndexOf(material.shaderKeywords, "_SHADEMODE_REFLECTIVE") != -1)
        {
            Ret = ShadingMode.Reflective;
            return Ret;
        }
        return Ret;
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        base.OnGUI(materialEditor, properties);

        Material targetMat = materialEditor.target as Material;
    }


}

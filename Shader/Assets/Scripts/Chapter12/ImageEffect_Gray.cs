using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageEffect_Gray : PostEffectsBase{

    public Shader grayShader;
    private Material grayMaterial;


    public Material material
    {
        get {
            grayMaterial = CheckShaderAndCreateMaterial(grayShader, grayMaterial);
            return grayMaterial;
        }
    }

    [Range(0.0f, 1.0f)]
    public float _LuminosityAmount = 1.0f;

    

    //OnRenderImage绘制绘制完所有透明和不透明的物体后每一帧调用
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (material != null)
        {
            //设置shader属性值
            material.SetFloat("_LuminosityAmount", _LuminosityAmount);

            //把屏幕纹理src传进shader里_MainTex属性
            Graphics.Blit(src, dest, material);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }

}

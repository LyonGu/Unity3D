using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageEffect_BlendMode : PostEffectsBase{

    public Shader blendModeShader;
    private Material blendModeMaterial;


    public Material material
    {
        get {
            blendModeMaterial = CheckShaderAndCreateMaterial(blendModeShader, blendModeMaterial);
            return blendModeMaterial;
        }
    }

    [Range(0.0f, 1.0f)]
    public float blendOpacity = 1.0f;

    

    //OnRenderImage绘制绘制完所有透明和不透明的物体后每一帧调用
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (material != null)
        {
            //设置shader属性值
            material.SetFloat("_Opacity", blendOpacity);

            //把屏幕纹理src传进shader里_MainTex属性
            Graphics.Blit(src, dest, material);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }

}
